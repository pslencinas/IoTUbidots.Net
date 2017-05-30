using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace BLUETOOTH
{
    class HC06
    {
        private static SerialPort BT_UART;

        private static string innerBufferU2 = "";

        private static Queue bluetoothInQueue = new Queue();

        private static OutputPort BT_POWER = null;

        private static bool bServerControl = false;

        public HC06(string com_name, BaudRate baudrate, Cpu.Pin pPOWER, bool startSever = true)
        {
            //OutputPort BT_POWER = new OutputPort(Cpu.Pin pPOWER, true);

            BT_UART = new SerialPort(com_name, (int)baudrate);
            BT_UART.Handshake = Handshake.None;
            BT_UART.Parity = Parity.None;
            BT_UART.StopBits = StopBits.One;
            BT_UART.DataBits = 8;
            BT_UART.DiscardInBuffer();
            BT_UART.DiscardOutBuffer();
            BT_UART.DataReceived += new SerialDataReceivedEventHandler(BlueTooth_DataReceived);

            BT_UART.Open(); // <--- Open COM

            bServerControl = startSever;
        }

        static void BlueTooth_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (BT_UART.BytesToRead > 20) 
            { 
                BT_UART.DiscardInBuffer(); 
                innerBufferU2 = string.Empty; 
                return; 
            }

            byte[] buf = new byte[BT_UART.BytesToRead];

            int rxCnt = BT_UART.Read(buf, 0, buf.Length);

            innerBufferU2 += new string(System.Text.UTF8Encoding.UTF8.GetChars(buf));
            Debug.Print("Comando recibido: " + innerBufferU2);

            //int backspace = innerBufferU2.IndexOf('\b');

            if (innerBufferU2.IndexOf('\n') != -1)
            {
                Debug.Print("innerBufferU2: " + innerBufferU2);
                string[] lines = innerBufferU2.Split('\n');
                
                innerBufferU2 = lines[lines.Length - 1];  //always keep the portion after the last newline in the buffer

                foreach (string command in lines)
                {
                    Debug.Print("command: " + command);
                    if (command != string.Empty)
                        bluetoothInQueue.Enqueue(command);
                    
                }
            }
        }

        public delegate void CommandReceivedEventHandler(object sender, CmdReceivedEventArgs e);

        public event CommandReceivedEventHandler CommandReceived;

        private void OnCommandReceived(CmdReceivedEventArgs e)
        {
            CommandReceived(this, e);
        }

        public void StartServer()
        {
            Debug.Print("StartServer BT");
            new Thread(thread_BLUETOOTH).Start();
        }

        public void StopServer()
        {
            bServerControl = false;
        }

        public void thread_BLUETOOTH()
        {
            //<<<----- UART/Bluetooth HC-05/06 Example ----->>>

            string btMessageReceived;

            string btMessageSent = string.Empty;

            byte[] txBuffer = null;

            bServerControl = true;

            while (bServerControl)
            {
                
                if ((BT_UART.BytesToWrite == 0) && (bluetoothInQueue.Count != 0))
                {
                    txBuffer = null;

                    btMessageReceived = (string)bluetoothInQueue.Dequeue();
                    Debug.Print("InQueue: " + btMessageReceived);

                    btMessageSent = AnalizeCommand(btMessageReceived);    //analizamos el comando recibido

                    if (btMessageSent.Length != 0)
                    {
                        //Debug.Print("enviando=" + btMessageSent);

                        txBuffer = System.Text.UTF8Encoding.UTF8.GetBytes(btMessageSent + "\r\n");

                        BT_UART.Write(txBuffer, 0, txBuffer.Length);

                        btMessageSent = string.Empty;
                    }
                }

                System.Threading.Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="btMessage"></param>

        private string AnalizeCommand(string btMessage)
        {
            string sCommand = btMessage;    // btMessage.Substring(0, btMessage.Length - 1);

            if (sCommand[sCommand.Length - 1] == '\r') 
                sCommand = sCommand.Substring(0, sCommand.Length - 1);   //remove \r char if any
            
            //verificamos si la trama comienza y termina con "<...>"
            if ((sCommand[0] != '<') || (sCommand[sCommand.Length - 1] != '>'))
                return "<NACK>";

            string sCMD = sCommand.Substring(1, 3).ToUpper();

            string sPAYLOAD = string.Empty;

            if (sCommand.Length > 6) 
            { 
                sPAYLOAD = sCommand.Substring(5, sCommand.Length - 6); 
            }

            string response = "<NACK>"; 
            CmdReceivedEventArgs args = null;

            switch (sCMD)
            {
                case "DEV":
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                        if (args.ResponseData != string.Empty)
                            response = "<ACK+DEV+" + args.ResponseData + ">";
                        else
                            response = "<NACK+DEV>";
                    }
                    break;

                case "DIN":
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                        if (args.ResponseData != string.Empty)
                            response = "<ACK+DIN+" + args.ResponseData + ">";
                        else
                            response = "<NACK+DIN>";
                    }
                    break;

                case "LED":
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); 
                        OnCommandReceived(args);

                        Debug.Print("args.ResponseDate: " + args.ResponseData);
                        if (args.ResponseData != string.Empty)
                            response = "<ACK+LED+" + args.ResponseData + ">";
                        else
                            response = "<NACK+LED>";
                    }
                    break;

                case "JSK":   //<JSK+READ>
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                        if (args.ResponseData != string.Empty)
                            response = "<ACK+JSK+" + args.ResponseData + ">";
                        else
                            response = "<NACK+JSK>";
                    }
                    break;

                case "AIN":
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                        if (args.ResponseData != string.Empty)
                            response = "<ACK+AIN+" + args.ResponseData + ">";
                        else
                            response = "<NACK+AIN>";
                    }
                    break;

                case "OUT":   //<OUT+SET+XX> o <OUT+CLR+XX> <OUT+GET+XX> (BIT#01~32 / 00=ALL)
                    {
                        if (sPAYLOAD.Length == 6)
                        {
                            args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                            if (args.ResponseData != string.Empty)
                            {
                                response = "<ACK+OUT+" + args.ResponseData + ">";
                            }
                            else
                                response = "<NACK+OUT>";
                        }
                    }
                    break;

               
                case "GPS":   //<GPS+GPRMC>
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                        if (args.ResponseData != string.Empty)
                            response = "<ACK+GPS+" + args.ResponseData + ">";
                        else
                            response = "<NACK+GPS>";
                    }
                    break;

               
                case "SPD":
                    {
                        args = new CmdReceivedEventArgs(sCMD, sPAYLOAD); OnCommandReceived(args);

                        if (args.ResponseData != string.Empty)
                            response = "<ACK+SPD+" + args.ResponseData + ">";
                        else
                            response = "<NACK+SPD>";
                    }
                    break;

                case "CLK":
                    {
                        string sDateAux=string.Empty;

                        if ((sPAYLOAD.Substring(0, 3) == "SET") && (sPAYLOAD.Length == 18))    //<CLK+SET>
                        {
                            sDateAux = sPAYLOAD.Substring(4, sPAYLOAD.Length - 4);

                            DateTime RTC = SetClock(sDateAux);

                            if (RTC != DateTime.MinValue)
                            {
                                args = new CmdReceivedEventArgs(sCMD, RTC); OnCommandReceived(args);

                                if (args.ResponseData != string.Empty)
                                    response = "<ACK+CLK+" + args.ResponseData + ">";
                                else
                                    response = "<NACK+CLK>";
                            }
                            else
                                response = "<NACK+CLK>\n";
                        }

                        if (sPAYLOAD.Substring(0, 3) == "GET")   //<CLK+GET>
                        {
                            string sGetTimeInfo = GetClock();

                            if (sGetTimeInfo != string.Empty)
                                response = "<ACK+CLK+" + sGetTimeInfo + ">\n";
                            else
                                response = "<NACK+CLK>\n";
                        }
                    }
                    break;

            }

            if (args != null)
            {

            }

            return response;
        }

        private DateTime SetClock(string sDataCMD)  //YYYYMMDDHHMMSS
        {
            if ((sDataCMD == string.Empty) || (sDataCMD.Length != 14)) return DateTime.MinValue;

            DateTime RTC_Time;

            try
            {
                RTC_Time = new DateTime(Convert.ToInt16(sDataCMD.Substring(0, 4)), Convert.ToInt16(sDataCMD.Substring(4, 2)), Convert.ToInt16(sDataCMD.Substring(6, 2)),
                                        Convert.ToInt16(sDataCMD.Substring(8, 2)), Convert.ToInt16(sDataCMD.Substring(10, 2)), Convert.ToInt16(sDataCMD.Substring(12, 2)), 00);
            }
            catch (Exception e)
            {
                return RTC_Time = DateTime.MinValue;
            }

            return RTC_Time;
        }

        private string GetClock()
        {
            return DateTime.Now.ToString();
        }

    }

    public class CmdReceivedEventArgs : EventArgs
    {
        public string Command {
            get; 
            set; }
        public object Data { get; private set; }
        public string ResponseData { get; set; }

        public CmdReceivedEventArgs(string command, object data)
        {
            Command = command;
            Data = data;
        }
    }
}
