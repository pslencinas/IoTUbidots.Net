using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

using GPIO_CONTROL;
using DIGITAL_INPUTS;
using BLUETOOTH;

namespace MFConsoleApplication1
{
    public class Program
    {
        static IOPIN gpioControl = null;
        static IOPIN.INPUTS inputs = null;
        private static UInt32 digitalInputs;
        static SerialPort GPS_UART3;
        static HC06 bluetooth;
        public static double sensorReading = 0;
        static string innerBufferU3 = "";
        private static string ResponseData;
        private static string PrevResponseData;
        static OutputPort LED_GREEN; 
        static OutputPort LED_RED; 
        static OutputPort LED_1;
        static OutputPort LED_3;
        static OutputPort LED_4;
        static Boolean LED1 = false;

        public static void Main()
        {
            
            LED_GREEN = new OutputPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_C_4, false);
            LED_RED = new OutputPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_C_5, false);
            LED_1 = new OutputPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_D_12, false);
            LED_3 = new OutputPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_D_14, false);
            LED_4 = new OutputPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_D_15, false);

            //<<<----- Entrada de Interrupcion donde se conecta el boton USER de la placa ----->>>
            InterruptPort btn = new InterruptPort((Cpu.Pin)Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_A_7, 
                true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

            btn.OnInterrupt += new NativeEventHandler(btn_OnInterrupt);

            Debug.Print("Dentro de Main()");

            gpioControl = new IOPIN();
            inputs = new IOPIN.INPUTS();    //configurados en modo switch to ground pines SG y S-to-Batt los pines SP
            
            // Thread para leer los valores digitales de los pines SG
            Thread MyThread = new Thread(ThreadLoop);
            MyThread.Start();

            // Thread para leer valores del FONA y envialos a Ubidots
            System.Threading.Thread tGPS = new System.Threading.Thread(thread_GPS);
            tGPS.Start();

            // Thread para leer valor de la bateria
            System.Threading.Thread tANALOG = new System.Threading.Thread(thread_ANALOG);
            tANALOG.Start();


            //BLUETOOTH MANAGER
            bluetooth = new HC06("COM2", BaudRate.Baudrate9600, (Cpu.Pin)Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_B_13);
            bluetooth.CommandReceived += new HC06.CommandReceivedEventHandler(udpCommand_CmdReceived);
            bluetooth.StartServer();
            // Los comandos tienen que ser del tipo <CCC+AAA>
            // CCC = comandos de 3 digitos
            // AAA = Datos

            
            while (true)
            {
                LED_GREEN.Write(true);
                
                Thread.Sleep(500);

                LED_GREEN.Write(false);
                
                Thread.Sleep(500);

               
            }
            
        }

        #region "ANALOG"

        static void thread_ANALOG()
        {
           
            AnalogInput ADC_CH6 = new AnalogInput(Cpu.AnalogChannel.ANALOG_6);  //ANALOG6 == PC2 (VOLT)
            
           
            while (true)
            {
                sensorReading = (ADC_CH6.Read() + 0.7975) * 11;
                //Debug.Print("Nivel de Tensión: " + sensorReading.ToString("F2"));
                
                Thread.Sleep(500);
            }

        }

        #endregion

        #region "FONa"

        public static void thread_GPS()
        {
            //OutputPort GPS_POWER = new OutputPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_B_12, true);

            //<<<----- UART/GPS SF2820 Example ----->>>

            GPS_UART3 = new SerialPort("COM3", 9600);
            GPS_UART3.Handshake = Handshake.None;
            GPS_UART3.Parity = Parity.None;
            GPS_UART3.StopBits = StopBits.One;
            GPS_UART3.DataBits = 8;
            GPS_UART3.ReadTimeout = 0;
            GPS_UART3.Open(); // <--- Open COM
            Debug.Print("COM3 abierto");

            byte[] tx_data;
            byte[] rx_data = new byte[64];

            Thread.Sleep(5000);

           

            while (SendCommFona("AT+SAPBR=1,1", 'O', 'K')) ;

            while (true)
            {
                
                while (SendCommFona("AT+CIPSHUT", 'O', 'K')) ;
                while (SendCommFona("AT+CGATT=1", 'O', 'K')) ;
                while (SendCommFona("AT+CIPSTART=\"TCP\",\"things.ubidots.com\",\"80\"", 'C', 'T')) ;
                while (SendCommFona("AT+CIPSEND", '\n', '>')) ;
                                
                int num;
                String len;
                String var;
                var = "{\"value\":" + sensorReading.ToString("F2") + "}";          //value is the sensor value
                num = var.Length;
                len = num.ToString();

                Debug.Print("Enviando POST");
                GPS_UART3.Flush();
                tx_data = Encoding.UTF8.GetBytes("POST /api/v1.6/devices/FONA/variable_fona/values HTTP/1.1\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("Host: things.ubidots.com\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("X-Auth-Token: EPPokxfymQUNuowFkaLPKSdeNtFci8\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("Content-Type: application/json\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("Content-Length: " + len + "\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("{\"value\":" + sensorReading.ToString("F2") + "}\r\n");
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                tx_data = Encoding.UTF8.GetBytes("\r\n" + (char)26);
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
                Thread.Sleep(3000);

                while (SendCommFona("", 'O', 'K')) ;
                while (SendCommFona("AT+CIPCLOSE", 'O', 'K')) ;

                Thread.Sleep(5000);
            }

            GPS_UART3.Close();
        
        }

        #endregion

        #region Funciones FONA

        public static Boolean SendCommFona(String comando, char Ini, char Fin)
        {
            int read_count = 0;
            byte[] tx_data;
            byte[] rx_data;
            Boolean waiting = true;
            Boolean error = false;

            GPS_UART3.DiscardOutBuffer();
            GPS_UART3.DiscardInBuffer();
            //Debug.Print("Analizando comando: "+comando);
            GPS_UART3.Flush();
            tx_data = Encoding.UTF8.GetBytes(comando+"\r\n");
            if (comando.Equals(""))
            {
                rx_data = new byte[256];    
            }
            else
            {
                rx_data = new byte[64];
                GPS_UART3.Write(tx_data, 0, tx_data.Length);
            }
            
            // read the data
            do
            {
                Thread.Sleep(200);
                read_count = GPS_UART3.Read(rx_data, 0, rx_data.Length);
                if (read_count > 1)
                {
                    //Debug.Print("read_count: " + read_count);
                    string datoRec = new string(System.Text.UTF8Encoding.UTF8.GetChars(rx_data));
                    Debug.Print(datoRec);
                    for (int i = 0; i < read_count; i++)
                    {
                        if (rx_data[i] == Ini && rx_data[i + 1] == Fin)
                        {
                            waiting = false;
                            break;
                        }else if (rx_data[i] == 'E' && rx_data[i + 1] == 'R' && rx_data[i + 2] == 'R')
                        {
                            waiting = false;
                            error = true;
                            break;

                        } 
                    }
                }
            } while (waiting);

            GPS_UART3.DiscardOutBuffer();
            GPS_UART3.DiscardInBuffer();
            GPS_UART3.Flush();
            Thread.Sleep(300);

            if (error) { 
                return true; 
            }else { 
                return false; 
            }


        }
        #endregion 

        #region "BLUETOOTH"

        // Recivo comandos via BT siguiendo la lógica LED+ON / LED+OFF
        private static void udpCommand_CmdReceived(object sender, CmdReceivedEventArgs e)
        {
            Debug.Print("Comando leido en Program.cs: " + e.Command);

            switch (e.Command)
            {
               
                case "DIN":
                    {
                        Debug.Print("Dentro de DIN BT");

                        digitalInputs = inputs.Read();

                        switch ((string)e.Data.ToString())
                        {
                            case "ALL":   //<DIN+ALL>
                                {
                                    e.ResponseData = (string)digitalInputs.ToString("X");
                                }
                                break;

                            case "SP":   //<DIN+SP>
                                {
                                    e.ResponseData = (string)(digitalInputs & 0xFF).ToString("X");
                                }
                                break;

                            case "SG":   //<DIN+SG>
                                {
                                    e.ResponseData = (string)((digitalInputs & 0xFF) >> 8).ToString("X");
                                }
                                break;
                        }
                    }
                    break;

                case "LED":
                    {
                        Debug.Print("e.Data: " + e.Data.ToString());
                        if ((string)e.Data.ToString() == "ON")//<LED+ON> <LED+OFF>
                        {
                            LED_3.Write(true);
                        }
                        if ((string)e.Data.ToString() == "OFF")
                        {
                            LED_3.Write(false);
                        }
                        if ((string)e.Data.ToString() == "AT")
                        {
                           
                        }
                        e.ResponseData = (string)e.Data;
                    }
                    break;

             

               
              
               
              

                default: break;
            }
        }

        #endregion

        #region ThreadLoop

        static void ThreadLoop()
        {
            while (true)
            {
                digitalInputs = inputs.Read();  //leo los 14bits SG + 8bits SP
                
                ResponseData = (string)(digitalInputs >> 8).ToString("X");  //me quedo con los 14bits SG

                if (ResponseData.Equals("00"))
                {
                    LED_4.Write(false);
                    
                }
                else if (ResponseData.Equals("01"))
                {
                    LED_4.Write(true);
                    
                }

                if (!ResponseData.Equals(PrevResponseData))
                {
                    Debug.Print("ResponseData: " + ResponseData);
                    PrevResponseData = ResponseData;
                }
                Thread.Sleep(200);

            }

        }

        #endregion  

        #region "Hardware pin's IRQ"

        /// <summary>
        /// Handler de Interrupcion del pulsador "AUX-S2" que esta en la placa
        /// </summary>
        static void btn_OnInterrupt(uint port, uint state, DateTime time)
        {
            Debug.Print("Dentro de btn_OnInterrupt");
            Debug.Print("state: " + state);

            if (state == 1)
            {
                if (LED1 == false)
                {
                    LED_1.Write(true);
                    LED1 = true;
                }

                                              
            }
            if (state == 0)
            {
                if (LED1 == true)
                {
                    LED_1.Write(false);
                    LED1 = false;
                }

            }

            return;
        }

        #endregion

    }
}
