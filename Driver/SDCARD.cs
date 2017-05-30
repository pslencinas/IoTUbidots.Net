using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;
using Microsoft.SPOT.IO;

using Gadgeteer.Modules.GHIElectronics;

using GHI.IO.Storage;

using IR_METER;

namespace SDCARD
{
    //-----------------------------------------------------------------
    // SDCARD
    //-----------------------------------------------------------------

    class SDCardMemory
    {

        private static string HeaderFile = "";
        
        public delegate void ConfigurationReadEventHandler(object sender, DTX0063.Program.structConfigParms sCfgParms);

        public event ConfigurationReadEventHandler ConfigurationRead;

        public delegate void MemoryInsertedEventHandler (object sender);

        public event MemoryInsertedEventHandler MemoryInserted;

        public delegate void MemoryRemovedEventHandler(object sender);

        public event MemoryRemovedEventHandler MemoryRemoved;

        //-----------------------------------------------------------------
        // SDCARD Control Status
        //-----------------------------------------------------------------

        public enum enumStatusSDCARD
        {   THREAD_IDLE = 0,
            INITIALIZE,
            CHECK_INSERTED,
            MEMORY_INSERTED,
            MEMORY_REMOVED,
            CARD_NOT_PRESENT,
            MOUNT_CARD,
            CARD_MOUNTED, 
            UNMOUNT_CARD,
            CARD_UNMOUNTED,
            INITIALIZING,
            IS_FORMATTED,
            INIT_COMPLETE,
            INIT_FAILURE,
            INIT_FINISHED,
            READ_CONFIG_FILE,
            CONFIG_COMPLETE,
            WAIT_FOR_RECORDING,
            START_RECORDING,
            RECORDING_FILE,
            STOP_RECORDING,
            RECORDING_COMPLETE,
        }

        private static enumStatusSDCARD statusSDCARD = enumStatusSDCARD.THREAD_IDLE;

        //-----------------------------------------------------------------
        // SDCARD EVENT's
        //-----------------------------------------------------------------

        //public delegate void SDCardEventHandler(object sender, enumStatusSDCARD status);

        //public event SDCardEventHandler SDCardEvent;

        private static GHI.IO.Storage.SDCard sdCard = null;

        private static FileStream FileHandle = null;

        private static string SDCardFileName = null;
        
        private static string SDCardConfigFileName = null;

        private static bool bSDCardInitComplete = false;

        private static string strDataToWrite = "";

        private static AutoResetEvent autoEvent = new AutoResetEvent(false);

        private static bool isSDCardPresent = false;

        private static InterruptPort sdcp;

        /// <summary>
        /// The constructor
        /// </summary>
        public SDCardMemory()
        {
        }

        #region "PRIVATE FUNCTIONS"

        static private bool fOpen(string filename, System.IO.FileMode filemode)
        {
            if ((filename == null) || (filename == "")) return false;

            SDCardFileName = filename;

            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            FileHandle = new FileStream(rootDirectory + @"\" + SDCardFileName, filemode);

            if (FileHandle == null) return false;

            return true;
        }

        static private void fClose()
        {
            FileHandle.Close();
        }

        static private void fWrite(string strData)
        {
            byte[] data = Encoding.UTF8.GetBytes(strData);

            FileHandle.Write(data, 0, data.Length);
        }

        static private string fRead()
        {
            const int BUFFER_SIZE = 512;

            byte[] data = new byte[BUFFER_SIZE]; string readData;

            int count = FileHandle.Read(data, 0, data.Length);

            readData = new string(Encoding.UTF8.GetChars(data, 0, count));
            
            return readData;
        }

        static private DTX0063.Program.structConfigParms ReadConfigParmsFromFile(string data)
        {
            //PARM's OFFSETS

            const byte OFFSET_CFG_IDENTIFIER = 0;
            const byte OFFSET_CFG_DEVICE_NAME = 1;
            const byte OFFSET_CFG_SERIAL_NUMBER = 2;
            const byte OFFSET_CFG_SENSORS_COUNT = 3;
            const byte OFFSET_CFG_SAMPLING_TIME = 4; 
            const byte OFFSET_CFG_PROGRAMS_COUNT = 5; 
            const byte OFFSET_CFG_TEMP_CHANGES_BEEP = 6; 
            const byte OFFSET_CFG_TEMP_CRITICAL_ALARM = 7;

            const byte OFFSET_PRG_IDENTIFIER = 0;
            const byte OFFSET_PRG_NUMBER = 1;
            const byte OFFSET_PRG_MIN_CRITICAL_TEMP = 2;
            const byte OFFSET_PRG_MIN_WARNING_TEMP = 3;
            const byte OFFSET_PRG_MAX_WARNING_TEMP = 4;
            const byte OFFSET_PRG_MAX_CRITICAL_TEMP = 5;

            DTX0063.Program.structConfigParms cfgParmsAux = new DTX0063.Program.structConfigParms();

            string[] dataLines = data.Split(new char[] { '\n' });

            foreach (string line in dataLines)
            {
                if (line == "") break;

                string lineAux = line.TrimEnd(new char[] { '\n', '\r' });

                string[] parms = lineAux.Split(new char[] { ',' });

                if (parms[OFFSET_CFG_IDENTIFIER].Equals("CFG"))
                {
                    cfgParmsAux.Device = parms[OFFSET_CFG_DEVICE_NAME];
                    cfgParmsAux.SerialNumber = parms[OFFSET_CFG_SERIAL_NUMBER];

                    cfgParmsAux.SensorsCount= byte.Parse(parms[OFFSET_CFG_SENSORS_COUNT]);

                    cfgParmsAux.SamplingTime = byte.Parse(parms[OFFSET_CFG_SAMPLING_TIME]);

                    cfgParmsAux.ProgramsCount = byte.Parse(parms[OFFSET_CFG_PROGRAMS_COUNT]);

                    if (cfgParmsAux.ProgramsCount > 0)
                        cfgParmsAux.Programs = new DTX0063.Program.structProgramsParms[cfgParmsAux.ProgramsCount];

                    if (parms[OFFSET_CFG_TEMP_CHANGES_BEEP] == "1")
                        cfgParmsAux.TempChangesBeep = true;
                    else
                        cfgParmsAux.TempChangesBeep = false;

                    if (parms[OFFSET_CFG_TEMP_CRITICAL_ALARM] == "1")
                        cfgParmsAux.TempCriticalAlarm = true;
                    else
                        cfgParmsAux.TempCriticalAlarm = false;
                }

                if (parms[OFFSET_PRG_IDENTIFIER].Equals("PRG"))
                {
                    int index = int.Parse(parms[OFFSET_PRG_NUMBER]) - 1;

                    if (index >= 0)
                    {
                        cfgParmsAux.Programs[index].identifier = int.Parse(parms[OFFSET_PRG_NUMBER]);
                        cfgParmsAux.Programs[index].maxTempCritical = int.Parse(parms[OFFSET_PRG_MAX_CRITICAL_TEMP]);
                        cfgParmsAux.Programs[index].maxTempWarning = int.Parse(parms[OFFSET_PRG_MAX_WARNING_TEMP]);
                        cfgParmsAux.Programs[index].minTempCritical = int.Parse(parms[OFFSET_PRG_MIN_CRITICAL_TEMP]);
                        cfgParmsAux.Programs[index].minTempWarning = int.Parse(parms[OFFSET_PRG_MIN_WARNING_TEMP]);
                    }
                }
            }

            return cfgParmsAux;
        }

        #endregion

        #region "MAIN THREAD"

        static void sdcp_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (state == 1)
            {
                if (sdCard.Mounted == true) { sdCard.Unmount(); sdCard.Dispose(); }

                isSDCardPresent = false; statusSDCARD = enumStatusSDCARD.MEMORY_REMOVED;
            }
            else
            {
                isSDCardPresent = true; statusSDCARD = enumStatusSDCARD.MEMORY_INSERTED;
            }

            return;
        }

        private void thread_SDCARD()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                
                switch (statusSDCARD)
                {
                    case enumStatusSDCARD.THREAD_IDLE: { System.Threading.Thread.Sleep(1000); } break;

                    case enumStatusSDCARD.INITIALIZE: 
                        {
                            //<<<----- Entrada de Interrupcion donde se conecta el boton USER de la placa ----->>>
                          
                            sdcp = new InterruptPort((Cpu.Pin)Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_A_7, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                            sdcp.OnInterrupt += new NativeEventHandler(sdcp_OnInterrupt);

                            statusSDCARD = enumStatusSDCARD.CHECK_INSERTED; 
                        } 
                        break;
                    
                    case enumStatusSDCARD.CHECK_INSERTED:
                        {
                            if (sdcp.Read() == false)
                            {
                                isSDCardPresent = true; statusSDCARD = enumStatusSDCARD.MEMORY_INSERTED;
                            }
                            else
                            {
                                isSDCardPresent = false; statusSDCARD = enumStatusSDCARD.MEMORY_REMOVED;
                            }
                        }
                        break;

                    case enumStatusSDCARD.MEMORY_INSERTED:
                        {
                            MemoryInsertedEventHandler handler = MemoryInserted;

                            if (handler != null)
                            {
                                handler(this);
                            }

                            statusSDCARD = enumStatusSDCARD.INITIALIZING;
                        }
                        break;

                    case enumStatusSDCARD.MEMORY_REMOVED:
                        {
                            MemoryRemovedEventHandler handler = MemoryRemoved;

                            if (handler != null)
                            {
                                handler(this);
                            }

                            statusSDCARD = enumStatusSDCARD.CARD_NOT_PRESENT;
                        }
                        break;

                    case enumStatusSDCARD.CARD_NOT_PRESENT:
                        {
                            System.Threading.Thread.Sleep(300);
                        }
                        break;

                    case enumStatusSDCARD.INITIALIZING:
                        {
                            System.Threading.Thread.Sleep(1000);

                            try
                            {
                                sdCard = new GHI.IO.Storage.SDCard();

                                sdCard.ForceInitialization();

                                if (sdCard.Mounted == true) sdCard.Unmount();

                                statusSDCARD = enumStatusSDCARD.MOUNT_CARD;
                            }
                            catch (Exception ex)
                            {
                                statusSDCARD = enumStatusSDCARD.INIT_FAILURE;

                                Debug.Print(ex.Message);
                            }
                        }
                        break;
                    
                    case enumStatusSDCARD.MOUNT_CARD:
                        {
                            try
                            {
                                sdCard.Mount(); 
                                
                                statusSDCARD = enumStatusSDCARD.CARD_MOUNTED;
                            }
                            catch (Exception ex)
                            {
                                sdCard.Unmount(); statusSDCARD = enumStatusSDCARD.INIT_FAILURE;

                                Debug.Print(ex.Message);
                            }
                        }
                        break;

                    case enumStatusSDCARD.CARD_MOUNTED: { statusSDCARD = enumStatusSDCARD.IS_FORMATTED; } break;

                    case enumStatusSDCARD.IS_FORMATTED:
                        { 
                               if (VolumeInfo.GetVolumes()[0].IsFormatted == false)
                                {
                                    Debug.Print("Storage is not formatted. Format on PC with FAT32/FAT16 first!");

                                    statusSDCARD = enumStatusSDCARD.INIT_FAILURE;

                                    sdCard.Unmount();
                                }
                                else
                                {
                                    bSDCardInitComplete = false; statusSDCARD = enumStatusSDCARD.INIT_COMPLETE;
                                }

                        } break;

                    case enumStatusSDCARD.UNMOUNT_CARD: 
                        {
                            try
                            {
                                sdCard.Unmount();
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                            }

                            statusSDCARD = enumStatusSDCARD.CARD_UNMOUNTED;
                    
                        } break;

                    case enumStatusSDCARD.CARD_UNMOUNTED:
                        {
                            statusSDCARD = enumStatusSDCARD.CARD_UNMOUNTED;

                            System.Threading.Thread.Sleep(1000);
                        } 
                        break;

                    case enumStatusSDCARD.INIT_COMPLETE:
                        {
                            if(bSDCardInitComplete==false)
                                statusSDCARD = enumStatusSDCARD.READ_CONFIG_FILE;
                            else
                                statusSDCARD = enumStatusSDCARD.WAIT_FOR_RECORDING;
                        }
                        break;

                    case enumStatusSDCARD.INIT_FAILURE:
                        {
                            System.Threading.Thread.Sleep(1000);

                            bSDCardInitComplete = false;

                            statusSDCARD = enumStatusSDCARD.INIT_FAILURE;
                        }
                        break;

                    case enumStatusSDCARD.READ_CONFIG_FILE:
                        {
                            //READ CFG CONFIGURATION FROM SDCARD !!!!

                            fOpen(SDCardConfigFileName, FileMode.Open);
                             
                            DTX0063.Program.structConfigParms sysConfigParms = new DTX0063.Program.structConfigParms();
                            
                            sysConfigParms = ReadConfigParmsFromFile(fRead());
                            
                            fClose();

                            //Fire the event with the configuration struct...

                            ConfigurationReadEventHandler handler = ConfigurationRead;

                            if (handler != null)
                            {
                                handler(this, sysConfigParms);
                            }
             
                            statusSDCARD = enumStatusSDCARD.CONFIG_COMPLETE;
                        }
                        break;

                    case enumStatusSDCARD.CONFIG_COMPLETE: 
                        {
                            statusSDCARD = enumStatusSDCARD.INIT_FINISHED; 
                        }
                        break;

                    case enumStatusSDCARD.INIT_FINISHED:
                        {
                            bSDCardInitComplete = true;

                            statusSDCARD = enumStatusSDCARD.WAIT_FOR_RECORDING; 
                        } 
                        break;

                    case enumStatusSDCARD.WAIT_FOR_RECORDING: 
                        {
                            statusSDCARD = enumStatusSDCARD.WAIT_FOR_RECORDING; 
                            
                            System.Threading.Thread.Sleep(900);
                        } 
                        break;

                    case enumStatusSDCARD.START_RECORDING:
                        {
                            //<----- HEADER ----->

                            fOpen(SDCardFileName, FileMode.Create);

                            fWrite(HeaderFile);

                            autoEvent.Reset(); 

                            statusSDCARD = enumStatusSDCARD.RECORDING_FILE;
                        }
                        break;

                    case enumStatusSDCARD.RECORDING_FILE:
                        {
                            autoEvent.WaitOne();    //WAIT FOR SEMAPHORO TO RECORD DATA !!!!!!

                            if (statusSDCARD == enumStatusSDCARD.RECORDING_FILE)
                            {
                                fWrite(strDataToWrite);
                            }
                            else
                            {
                                statusSDCARD = enumStatusSDCARD.STOP_RECORDING;
                            }
                            
                            autoEvent.Reset(); 
                             
                        }
                        break;

                    case enumStatusSDCARD.STOP_RECORDING:
                        {
                            try
                            {
                                fClose();
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);    
                            }
                            
                            statusSDCARD = enumStatusSDCARD.RECORDING_COMPLETE;
                        }
                        break;

                    case enumStatusSDCARD.RECORDING_COMPLETE: { statusSDCARD = enumStatusSDCARD.WAIT_FOR_RECORDING; } break;
                }

            }
        }
        
        #endregion

        #region "PROPERTIES"

        public string FileName
        {
            get { return SDCardFileName; }

            set { SDCardFileName = value; }
        }

        public string ConfigFileName
        {
            get { return SDCardConfigFileName; }
            
            set { SDCardConfigFileName = value; }
        }

        public bool isInitializationComplete
        {
            get { return bSDCardInitComplete; }
        }

        public bool isRecording
        {
            get
            {
                if (statusSDCARD == enumStatusSDCARD.RECORDING_FILE)
                    return true;
                else
                    return false;
            }
        }

        public enumStatusSDCARD sdCardStatus
        {
            get { return statusSDCARD; }
        }

        #endregion

        #region "PUBLIC RECORDING FUNCTIONS"

        public void StartMachine()
        {
            statusSDCARD = enumStatusSDCARD.INITIALIZE;

            System.Threading.Thread tSDCARD = new System.Threading.Thread(thread_SDCARD);

            tSDCARD.Start();
        }

        public bool Unmount()
        {
            if (statusSDCARD == enumStatusSDCARD.WAIT_FOR_RECORDING)
            {
                statusSDCARD = enumStatusSDCARD.UNMOUNT_CARD; return true;
            }
            else
                return false;
        }

        public bool Mount()
        {
            if (statusSDCARD == enumStatusSDCARD.CARD_UNMOUNTED)
            {
                statusSDCARD = enumStatusSDCARD.MOUNT_CARD; return true;
            }
            else
                return false;
        }
        
        public bool StartRecording(string FileName, string Header)
        {
            if (bSDCardInitComplete == true)
            {
                switch (statusSDCARD)
                {
                    case enumStatusSDCARD.WAIT_FOR_RECORDING:
                    {
                        SDCardFileName = FileName;

                        HeaderFile = Header;

                        statusSDCARD = enumStatusSDCARD.START_RECORDING;
                    } 
                    break;

                    default: return false;
                }
                
                return true;
            }
            else
                return false;
        }

        public int WriteData(string data)
        {
            if (statusSDCARD == enumStatusSDCARD.RECORDING_FILE)
            {
                strDataToWrite = data;
                
                autoEvent.Set();
                
                return 0;
            }
            else
                return -1;
        }

        public bool StopRecording()
        {
            if (statusSDCARD == enumStatusSDCARD.RECORDING_FILE)
            {
                statusSDCARD = enumStatusSDCARD.STOP_RECORDING; autoEvent.Set(); return true;
            }
            else
                return false;
        }
        
        #endregion
    }
}
