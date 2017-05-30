using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using CRC;

namespace IR_METER
{
    class MLX90614 : CRC8
    {
        private const double ZERO_KELVING = 273.15;

        private const int ERROR_TEMP = 273;

        //eeprom address...

        private const byte EEPROM_ADDRESS_TO_MAX = 0x00;
        private const byte EEPROM_ADDRESS_TO_MIN = 0x01;
        private const byte EEPROM_ADDRESS_PWMCTRL = 0x02;
        private const byte EEPROM_ADDRESS_TA_RANGE = 0x03;
        private const byte EEPROM_ADDRESS_EMISSIVITY = 0x04;
        private const byte EEPROM_ADDRESS_CONFIG_REG = 0x05;
        private const byte EEPROM_ADDRESS_SMBUS_DEVICE = 0x0E;
        
        //ram address...

        private const byte RAM_ADDRESS_TA = 0x06;
        private const byte RAM_ADDRESS_TOBJ1 = 0x07;
        private const byte RAM_ADDRESS_TOBJ2 = 0x08;

        private const byte FLAGS_ADDRESS = 0xF0;

        #region I2C Variables

            private const int MLX90614_ClockRate = 20;              // Freq [kHz]
        
            private const byte MLX90614_Base_Address = 0x61;        // From the documentation (Default 0x5A)

            private const int defaultTimeout = 1000;                // Not sure what the units are

            /// <summary>
            /// By design, .NETMF only allows one object per I2C bus device. This one must be share with all the 
            /// devices on the physical i2c bus. Multiple devices can use the one bus, by supplying Configurations 
            /// for each device and then changing the configuration used when talking to a device.
            /// </summary>
            
            static private I2CDevice _i2cBus;
            
            //Configuraciones para cada dispositivo...

            I2CDevice.Configuration I2CConfigA = new I2CDevice.Configuration(MLX90614_Base_Address + 0, MLX90614_ClockRate);
            I2CDevice.Configuration I2CConfigB = new I2CDevice.Configuration(MLX90614_Base_Address + 1, MLX90614_ClockRate);
            I2CDevice.Configuration I2CConfigC = new I2CDevice.Configuration(MLX90614_Base_Address + 2, MLX90614_ClockRate);
            I2CDevice.Configuration I2CConfigD = new I2CDevice.Configuration(MLX90614_Base_Address + 3, MLX90614_ClockRate);
            I2CDevice.Configuration I2CConfigE = new I2CDevice.Configuration(MLX90614_Base_Address + 4, MLX90614_ClockRate);

            private static byte DEVICE_COUNT = 3;

            
        #endregion

        private static CRC8 PEC = new CRC.CRC8();

        private int objectTemperature = 0;

        private int ambientTemperature = 0;

        private int[] objectTemperatureDev; //= new int[DEVICE_COUNT];

        private int[] ambientTemperatureDev; //= new int[DEVICE_COUNT];

        private readonly object _i2cLock = new Object();

        //-----------------------------------------------------------------
        // THERMOSTAT PARAM's
        //-----------------------------------------------------------------

        const byte DEFAULT_MIN_TEMP_CRITICAL = 65;
        const byte DEFAULT_MIN_TEMP_WARNING = 75;

        const byte DEFAULT_MAX_TEMP_WARNING = 85;
        const byte DEFAULT_MAX_TEMP_CRITICAL = 95;

        private static int minTempCritical;
        private static int minTempWarning;
        private static int maxTempCritical;
        private static int maxTempWarning;
        
        //-----------------------------------------------------------------
        // THERMOSTAT SYSTEM STATUS
        //-----------------------------------------------------------------

        public enum enumIR_STATUS
        {
            UNKNOWN = 0,
            SENSORS_ERROR,
            DISABLED,
            CRITICAL_LOW,
            WARNING_LOW,
            NORMAL,
            WARNING_HIGH,
            CRITICAL_HIGH
        };

        private static enumIR_STATUS systemTempStatus = enumIR_STATUS.UNKNOWN;
        
        private static enumIR_STATUS systemTempStatusBkp = enumIR_STATUS.UNKNOWN;

        private static bool bEnableThermostat = false;

        //-----------------------------------------------------------------
        // THERMOSTAT EVENT's
        //-----------------------------------------------------------------

        public delegate void TempChangeEventHandler(object sender, enumIR_STATUS status, int temperature);

        public event TempChangeEventHandler TempChanged;

        /// <summary>
        /// The constructor
        /// </summary>
        public MLX90614(I2CDevice i2cBus, object i2cLock, byte DevCount)
        {
            _i2cBus = i2cBus;

            _i2cLock = i2cLock;

            DEVICE_COUNT = DevCount;

            objectTemperatureDev = new int[DEVICE_COUNT];

            ambientTemperatureDev = new int[DEVICE_COUNT];

            System.Threading.Thread tMLX90614 = new System.Threading.Thread(thread_MLX90614);
            
            tMLX90614.Start();
        }

        void thread_MLX90614()
        {
            byte[] data = null; double tempK; byte device = 0;

            System.Threading.Thread.Sleep(300);

            //byte prgAddress = MLX90614_Base_Address;
            
            //ProgramDeviceAddress(prgAddress);
            
            //Search for connected devices...

            for (device = 0; device < DEVICE_COUNT; device++)
            {
                switch (device)
                {
                    case 0: { data = readByte(I2CConfigA, 0x20 + EEPROM_ADDRESS_SMBUS_DEVICE); } break; //DEVICE "A" -> 0x61
                    case 1: { data = readByte(I2CConfigB, 0x20 + EEPROM_ADDRESS_SMBUS_DEVICE); } break; //DEVICE "B" -> 0x62
                    case 2: { data = readByte(I2CConfigC, 0x20 + EEPROM_ADDRESS_SMBUS_DEVICE); } break; //DEVICE "C" -> 0x63
                    case 3: { data = readByte(I2CConfigD, 0x20 + EEPROM_ADDRESS_SMBUS_DEVICE); } break; //DEVICE "D" -> 0x64
                    case 4: { data = readByte(I2CConfigE, 0x20 + EEPROM_ADDRESS_SMBUS_DEVICE); } break; //DEVICE "E" -> 0x65
                }

                Debug.Print("MLX90614 found at address 0x" + data[0].ToString("X")); System.Threading.Thread.Sleep(300);
            }
            
            device = 0; //point to first device before start

            while (true)
            {
                System.Threading.Thread.Sleep(1000);

                //-----------------------------------------
                // Leemos la temperatura del ambiente...
                //-----------------------------------------

                switch (device)
                {
                    case 0: { data = readByte(I2CConfigA, RAM_ADDRESS_TA); } break;
                    case 1: { data = readByte(I2CConfigB, RAM_ADDRESS_TA); } break;
                    case 2: { data = readByte(I2CConfigC, RAM_ADDRESS_TA); } break;
                    case 3: { data = readByte(I2CConfigD, RAM_ADDRESS_TA); } break;
                    case 4: { data = readByte(I2CConfigE, RAM_ADDRESS_TA); } break;
                }

                if ((data[1] & 0x80) == 0x80)
                {
                    tempK = -1;
                }
                else
                {
                    tempK = ((data[1] << 8) + data[0]);
                    tempK = tempK / 50;
                    tempK = tempK - ZERO_KELVING;
                }

                ambientTemperatureDev[device] = (int)tempK;

                //-----------------------------------------
                // Leemos la temperatura del objeto...
                //-----------------------------------------

                System.Threading.Thread.Sleep(1000);

                switch (device)
                {
                    case 0: { data = readByte(I2CConfigA, RAM_ADDRESS_TOBJ1); } break;
                    case 1: { data = readByte(I2CConfigB, RAM_ADDRESS_TOBJ1); } break;
                    case 2: { data = readByte(I2CConfigC, RAM_ADDRESS_TOBJ1); } break;
                    case 3: { data = readByte(I2CConfigD, RAM_ADDRESS_TOBJ1); } break;
                    case 4: { data = readByte(I2CConfigE, RAM_ADDRESS_TOBJ1); } break;
                }

                if ((data[1] & 0x80) == 0x80)
                {
                    tempK = -1;
                }
                else
                {
                    tempK = ((data[1] << 8) + data[0]);
                    tempK = tempK / 50;
                    tempK = tempK - ZERO_KELVING;
                }

                objectTemperatureDev[device] = (int)tempK;

                device = (byte)(device + 1);        // Point next device

                if (device > (DEVICE_COUNT - 1))    // Check if last device has been reached
                {
                    //ambientTemperature = CalculateAmbientAverageTemp();

                    objectTemperature = CalculateObjectAverageTemp();

                    thermostatControl();    //Thermostat Control
                    
                    device = 0;             //Back to first device
                }
            }

        }

        private void thermostatControl()
        {
            //----------------------------------------------------------------------------------
            // TERMOSTATO --> Verificamos si la temperatura promedio esta dentro de los limites
            //----------------------------------------------------------------------------------

            if (objectTemperature == ERROR_TEMP)
            {
                systemTempStatus = enumIR_STATUS.SENSORS_ERROR;
            }
            else
            {
                if (bEnableThermostat == true)
                {
                    if (objectTemperature < minTempCritical)
                    {
                        systemTempStatus = enumIR_STATUS.CRITICAL_LOW; //Debug.Print("CRITICAL LOW");  //CRITICAL LOW
                    }
                    else
                    {
                        if ((objectTemperature >= minTempCritical) && (objectTemperature < minTempWarning))
                        {
                            systemTempStatus = enumIR_STATUS.WARNING_LOW; //Debug.Print("WARNING LOW");   //WARNING LOW
                        }
                        else
                        {
                            if ((objectTemperature >= minTempWarning) && (objectTemperature <= maxTempWarning))
                            {
                                systemTempStatus = enumIR_STATUS.NORMAL; //Debug.Print("NORMAL");       //NORMAL
                            }
                            else
                            {
                                if ((objectTemperature <= maxTempCritical) && (objectTemperature > maxTempWarning))
                                {
                                    systemTempStatus = enumIR_STATUS.WARNING_HIGH; //Debug.Print("WARNING HIGH"); //WARNING HIGH
                                }
                                else
                                {
                                    if (objectTemperature > maxTempCritical)
                                    {
                                        systemTempStatus = enumIR_STATUS.CRITICAL_HIGH; //Debug.Print("CRITICAL HIGH");   //CRITICAL HIGH
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    systemTempStatus = enumIR_STATUS.DISABLED;
            }

            if (systemTempStatusBkp != systemTempStatus)
            {
                TempChangeEventHandler handler = TempChanged;

                if (handler != null)
                {
                    handler(this, systemTempStatus, objectTemperature);
                }

                systemTempStatusBkp = systemTempStatus;
            }
        }
        
        /// <summary>
        /// Write data to the MLX90614 
        /// </summary>
        /// <param name="data">the first byte of the data must be the MLX90614 register to be written to.</param>
        /// <returns>number of bytes sent</returns>

        private int writeTo(I2CDevice.Configuration config, byte[] data)
        {
            int bytesSent;

            lock (_i2cLock)
            {
                _i2cBus.Config = config;

                I2CDevice.I2CTransaction writeXAction = I2CDevice.CreateWriteTransaction(data);

                I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { writeXAction };

                bytesSent = _i2cBus.Execute(actions, defaultTimeout);
            }
            
            return bytesSent;
        }

        /// <summary>
        /// Return the byte at the requested address from MLX90614. 
        /// </summary>
        /// <param name="address">The address of the MLX90614 register to read</param>
        /// <returns>The value read. Note that 0 could mean either 0 bytes or failure</returns>

        public byte[] readByte(I2CDevice.Configuration config, byte address)
        {
            byte[] mybuff = new byte[3];

            lock (_i2cLock)
            {
                _i2cBus.Config = config;

                I2CDevice.I2CTransaction sendAddress = I2CDevice.CreateWriteTransaction(new byte[] { address });

                I2CDevice.I2CTransaction readData = I2CDevice.CreateReadTransaction(mybuff);

                I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { sendAddress, readData };

                _i2cBus.Execute(actions, defaultTimeout);
            }

            return mybuff;
        }

        public bool ProgramDeviceAddress(byte NewAddress)
        {
            byte[] data;

            WriteEEPROM(new I2CDevice.Configuration(0x00, MLX90614_ClockRate), EEPROM_ADDRESS_SMBUS_DEVICE, (ushort)(0xBE00+(NewAddress & 0xFF)));

            System.Threading.Thread.Sleep(1000);

            data = readByte(new I2CDevice.Configuration(NewAddress, MLX90614_ClockRate), 0x20 + EEPROM_ADDRESS_SMBUS_DEVICE);

            Debug.Print("MLX90614 I2C device address = 0x" + data[0].ToString("X"));

            if (data[0] != NewAddress) return false;

            return true;
        }
        
        private int WriteEEPROM(I2CDevice.Configuration config, byte address, ushort value)
        {
            int bytesSent = 0; byte crc8;

            byte eeprom_address = (byte)(0x20 + address);
            byte lsb_data = (byte)(value & 0xFF);
            byte msb_data = (byte)((value >> 8) & 0xFF);

            //Hacemos un borrado escribiendo 0x00

            crc8 = PEC.ComputeChecksum(new byte[] { (byte)((config.Address << 1) & 0xFE), eeprom_address, 0x00, 0x00 });

            bytesSent = writeTo(config, new Byte[] { eeprom_address, 0x00, 0x00, crc8 });

            System.Threading.Thread.Sleep(200);

            //Enviamos el nuevo valor a la EEPROM

            crc8 = PEC.ComputeChecksum(new byte[] { (byte)((config.Address << 1) & 0xFE), eeprom_address, lsb_data, msb_data });

            bytesSent = writeTo(config, new Byte[] { eeprom_address, lsb_data, msb_data, crc8 });
            
            return bytesSent;
        }

        private ushort ReadFlags(I2CDevice.Configuration config)
        {
            byte[] data; ushort flags = 0;

            data = readByte(config, FLAGS_ADDRESS);

            flags = (ushort)((data[1] << 8) + data[0]);

            return flags;
        }

    #region "Properties"

        public int DevicesCount
        {
            get { return DEVICE_COUNT; }
        }

        //Ambient Temperature AVG & Devices

        public int AmbientTemp
        {
            get { return ambientTemperature; }
        }

        public int[] AmbientTempDevices
        {
            get { return ambientTemperatureDev; }
        }

        //Object Temperature AVG & Devices
        
        public int ObjectTemp
        {
            get { return objectTemperature; }
        }

        public int[] ObjectTempDevices
        {
            get { return objectTemperatureDev; }
        }

    #endregion

    #region "AVG Calculation"

        private int CalculateObjectAverageTemp()
        {
            //Calculamos el promedio...

            byte device = 0; objectTemperature = 0;

            for(device=0;device<DEVICE_COUNT;device++)
            {
                objectTemperature += objectTemperatureDev[device]; 
            }

            objectTemperature /= DEVICE_COUNT;
           
            return objectTemperature;
        }

        private int CalculateAmbientAverageTemp()
        {
            //Calculamos el promedio...
         
            byte device = 0; ambientTemperature = 0;

            for(device=0;device<DEVICE_COUNT;device++)
            {
                ambientTemperature += ambientTemperatureDev[device]; 
            }

            ambientTemperature /= DEVICE_COUNT;

            return ambientTemperature;
        }
    
    #endregion

    #region "THERMOSTAT"

        public bool EnableThermostat
        {
            get { return bEnableThermostat; }
            
            set { bEnableThermostat = value; }
        }

        //Thermostat Configuration

        public int WarningMaxTemp
        {
            get { return maxTempWarning; }
            set { maxTempWarning = value; }
        }

        public int WarningMinTemp
        {
            get { return minTempWarning; }
            set { minTempWarning = value; }
        }

        public int CriticalMaxTemp
        {
            get { return maxTempCritical; }
            set { maxTempCritical = value; }
        }

        public int CriticalMinTemp
        {
            get { return minTempCritical; }
            set { minTempCritical = value; }
        }

    #endregion

    #region "PEC Calculator CRC"

    #endregion

    }
  
}
