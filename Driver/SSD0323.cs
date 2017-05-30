using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace DISPLAY_OLED
{
    class SSD0323
    {

        #region "Pin Definition"
            
            static private OutputPort DataCommand;
        
            static private OutputPort OLED_Power;
        
            static private SPI spi;

        #endregion

            public enum enumDisplayMode
            {
                NORMAL = 0xA4,
                ALL_ON = 0xA5,
                ALL_OFF = 0xA6,
                INVERSE = 0xA7
            }
        
        #region "Variables"

            private byte[] data_TX;
        
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pOLED_DC">pin that controls the Data/Command line</param>
        /// <param name="pOLED_VCC">pin that controls the VCC power </param>
        /// <param name="spi_module">SPI module used for communication</param>
        public SSD0323(Cpu.Pin pOLED_DC, Cpu.Pin pOLED_VCC, SPI spi_module) 
        {
            DataCommand = new OutputPort(pOLED_DC, false);    // Data/Command PIN   

            OLED_Power = new OutputPort(pOLED_VCC, false);    // 12V Oled power ON/OFF

            spi = spi_module;
        }

        private void WriteOLED(byte[] value, bool IsData)
        {
            byte[] data_rx = new byte[value.Length];

            if (IsData == true) { DataCommand.Write(true); }    //Select DATA/COMMAND line

            spi.WriteRead(value, data_rx);

            DataCommand.Write(false);
        }

        public void EnableDisplay()
        {
            //Power UP !!!

            data_TX = new byte[] {0xAE}; WriteOLED(data_TX, false);

            OLED_Power.Write(true);

            System.Threading.Thread.Sleep(100);

            data_TX = new byte[] {0xAF}; WriteOLED(data_TX, false);
        }

        public void DisableDisplay()
        {
            //Power Down !!!

            data_TX = new byte[] { 0xAE }; WriteOLED(data_TX, false);

            OLED_Power.Write(false);

            System.Threading.Thread.Sleep(100);
        }
        
        public void InitializeDisplay()
        {
            data_TX = new byte[] { 0x15, 0, 63 }; WriteOLED(data_TX, false);  //Set Column Address 0-63 (2px/BYTE)
            
            data_TX = new byte[] { 0x75, 0, 63 }; WriteOLED(data_TX, false);  //Set Row Address 0-63
            
            data_TX = new byte[] { 0x81, 0x33 }; WriteOLED(data_TX, false);  //Contrast Control

            data_TX = new byte[] { 0x86 }; WriteOLED(data_TX, false);  //Full Current
            
            data_TX = new byte[] { 0xA0, 0x41 }; WriteOLED(data_TX, false); //remap

            data_TX = new byte[] { 0xA2, 0x44 }; WriteOLED(data_TX, false); //Row Offset

            data_TX = new byte[] { 0xA1, 0x00 }; WriteOLED(data_TX, false); //startLine

            data_TX = new byte[] { 0xA4 }; WriteOLED(data_TX, false); //Normal

            data_TX = new byte[] { 0xAF }; WriteOLED(data_TX, false); //Display ON
        }

        public void SetDisplayMode(enumDisplayMode Mode)
        {
            switch (Mode)
            {
                case enumDisplayMode.NORMAL: data_TX = new byte[] { 0xA4 }; break;
                case enumDisplayMode.ALL_ON: data_TX = new byte[] { 0xA5 }; break;
                case enumDisplayMode.ALL_OFF: data_TX = new byte[] { 0xA6 }; break;
                case enumDisplayMode.INVERSE: data_TX = new byte[] { 0xA7 }; break;
            }
            
            WriteOLED(data_TX, false);
        }

        public void AllPixelsON()
        {
            data_TX = new byte[] { 0xA5 }; WriteOLED(data_TX, false);  //Set Column Address 0-63 (2px/BYTE)
        }

        public void FillDisplay(byte x, byte y, byte width, byte height, byte deep)
        {
            data_TX = new byte[] { 0x15, 0, 63 }; WriteOLED(data_TX, false);  //Set Column Address 0-63 (2px/BYTE)
            data_TX = new byte[] { 0x75, 0, 63 }; WriteOLED(data_TX, false);  //Set Row Address 0-63

            data_TX = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            byte[] data_RX = new byte[data_TX.Length];

            DataCommand.Write(true); //Select DATA/COMMAND line

            for (int pos = 0; pos < (4096 / 16); pos++)
            {
                spi.WriteRead(data_TX, data_RX);
            }

            DataCommand.Write(false);
        }

        public void ClearDisplay()
        {
            data_TX = new byte[] {0x15,0,63}; WriteOLED(data_TX, false);  //Set Column Address 0-63 (2px/BYTE)
            data_TX = new byte[] {0x75,0,63}; WriteOLED(data_TX, false);  //Set Row Address 0-63

            data_TX = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            byte[] data_RX = new byte[data_TX.Length];

            DataCommand.Write(true); //Select DATA/COMMAND line
            
            for (int pos = 0; pos < (4096 / 16); pos++)
            {
                spi.WriteRead(data_TX, data_RX);
            }

            DataCommand.Write(false);
        }

        public void ClearArea(byte col, byte row, byte width, byte height)
        {
            FillArea(col, row, width, height, 0x00);
        }

        public void FillArea(byte col, byte row, byte width, byte height, byte color = 0x0F)
        {
            int iLastByteIndex; byte iBufferByteIndex; bool bSendDataPending; byte[] command_TX;

            if ((width % 2) == 0) { data_TX = new byte[(width / 2)]; } else { data_TX = new byte[(width / 2) + 1]; }

            byte[] data_rx = new byte[data_TX.Length];

            command_TX = new byte[] { 0x75, (byte)(row), (byte)(row + height) }; WriteOLED(command_TX, false);  //Set Row Address 0-63

            command_TX = new byte[] { 0x15, (byte)(col / 2), (byte)((col + width) / 2) }; WriteOLED(command_TX, false);  //Set Column Address 0-63 (2px/BYTE)

            for(byte bRowAux=0; bRowAux<height; bRowAux++)
            {
                iLastByteIndex = -1; iBufferByteIndex =0; bSendDataPending = false;

                //command_TX = new byte[] { 0x75, (byte)(row + bRowAux), (byte)(row + height) }; WriteOLED(command_TX, false);  //Set Row Address 0-63

                //command_TX = new byte[] { 0x15, (byte)(col / 2), (byte)((col + width) / 2) }; WriteOLED(command_TX, false);  //Set Column Address 0-63 (2px/BYTE)

                for (byte bColAux = 0; bColAux < width; bColAux++)
                {
                    byte bByteIndex = (byte)((bColAux + col) / 2);

                    if (bByteIndex != iLastByteIndex) 
                    {
                        if (bSendDataPending == true) { iBufferByteIndex++; bSendDataPending = false; }
                        
                        if (bSendDataPending == false) { iLastByteIndex = bByteIndex; bSendDataPending = true; }
                    }

                    if ((byte)(bColAux + col) % 2 == 0) { data_TX[iBufferByteIndex] += (byte)(color << 4); } else { data_TX[iBufferByteIndex] += (byte)(color << 0); } 
                }
                
                DataCommand.Write(true); spi.WriteRead(data_TX, data_rx); DataCommand.Write(false);
            }
        }

        /// <summary>
        /// Print a string message into OLED
        /// </summary>
        /// <param name="text">message to write</param>
        /// <param name="column">start column</param>
        /// <param name="row">start row (top of the letters)</param>
        /// <param name="grayLevel">(Optional) Gary Level of the text</param>
        public void PrintString(string text, byte column, byte row, byte grayLevel = 0x0F)
        {
            byte[] bText=System.Text.UTF8Encoding.UTF8.GetBytes(text);

            for (byte bRow = 0; bRow < 8; bRow++)
            {
                data_TX = new byte[] { 0x15, (byte)(column/2), 63 }; WriteOLED(data_TX, false);               //Set Column Address 0-63 (2px/BYTE)
                data_TX = new byte[] { 0x75, (byte)(row + bRow), 63 }; WriteOLED(data_TX, false);   //Set Row Address 0-63

                for (byte bChar = 0; bChar < bText.Length; bChar++)
                {
                    byte[] character = Font6x7[bText[bChar] - 32];  //Obtenemos los 5 bytes del CHAR

                    data_TX = new byte[] { 0x00, 0x00, 0x00 };
 
                    for (byte col = 0; col < 5; col++)
                    {
                        byte charByte = character[col];

                        bool BitValue = false ; if(((int)(charByte) & ((int)System.Math.Pow(2, bRow)))!=0) BitValue=true;

                        if (BitValue == true) data_TX[(int)(col / 2)] += (byte)((col%2==0) ? (grayLevel << 4) : (grayLevel << 0));
                    }

                    WriteOLED(data_TX, true);   //Send data to OLED display
                }
            }
        }

        #region "Fonts"

            private byte[][] Font6x7 = new byte[][]
            {
                new byte[] {0x00, 0x00, 0x00, 0x00, 0x00},            // Code for char  
                new byte[] {0x00, 0x00, 0x5F, 0x00, 0x00},            // Code for char !
                new byte[] {0x00, 0x07, 0x00, 0x07, 0x00},            // Code for char "
                new byte[] {0x14, 0x7F, 0x14, 0x7F, 0x14},            // Code for char #
                new byte[] {0x24, 0x2A, 0x7F, 0x2A, 0x12},            // Code for char $
                new byte[] {0x23, 0x13, 0x08, 0x64, 0x62},            // Code for char %
                new byte[] {0x36, 0x49, 0x55, 0x22, 0x50},            // Code for char &
                new byte[] {0x00, 0x00, 0x05, 0x03, 0x00},            // Code for char '
                new byte[] {0x00, 0x1C, 0x22, 0x41, 0x00},            // Code for char (
                new byte[] {0x00, 0x00, 0x41, 0x22, 0x1C},            // Code for char )
                new byte[] {0x14, 0x08, 0x3E, 0x08, 0x14},            // Code for char *
                new byte[] {0x08, 0x08, 0x3E, 0x08, 0x08},            // Code for char +
                new byte[] {0x00, 0x00, 0x50, 0x30, 0x00},            // Code for char ,
                new byte[] {0x08, 0x08, 0x08, 0x08, 0x08},            // Code for char -
                new byte[] {0x00, 0x00, 0x60, 0x60, 0x00},            // Code for char .
                new byte[] {0x20, 0x10, 0x08, 0x04, 0x02},            // Code for char /
                new byte[] {0x3E, 0x51, 0x49, 0x45, 0x3E},            // Code for char 0
                new byte[] {0x00, 0x42, 0x7F, 0x40, 0x00},            // Code for char 1
                new byte[] {0x42, 0x61, 0x51, 0x49, 0x46},            // Code for char 2
                new byte[] {0x21, 0x41, 0x45, 0x4B, 0x31},            // Code for char 3
                new byte[] {0x18, 0x14, 0x12, 0x7F, 0x10},            // Code for char 4
                new byte[] {0x27, 0x45, 0x45, 0x45, 0x39},            // Code for char 5
                new byte[] {0x3C, 0x4A, 0x49, 0x49, 0x30},            // Code for char 6
                new byte[] {0x01, 0x71, 0x09, 0x05, 0x03},            // Code for char 7
                new byte[] {0x36, 0x49, 0x49, 0x49, 0x36},            // Code for char 8
                new byte[] {0x06, 0x49, 0x49, 0x29, 0x1E},            // Code for char 9
                new byte[] {0x00, 0x00, 0x36, 0x36, 0x00},            // Code for char :
                new byte[] {0x00, 0x00, 0x56, 0x36, 0x00},            // Code for char ;
                new byte[] {0x00, 0x08, 0x14, 0x22, 0x41},            // Code for char <
                new byte[] {0x14, 0x14, 0x14, 0x14, 0x14},            // Code for char =
                new byte[] {0x00, 0x41, 0x22, 0x14, 0x08},            // Code for char >
                new byte[] {0x02, 0x01, 0x51, 0x09, 0x06},            // Code for char ?
                new byte[] {0x32, 0x49, 0x79, 0x41, 0x3E},            // Code for char @
                new byte[] {0x7E, 0x11, 0x11, 0x11, 0x7E},            // Code for char A
                new byte[] {0x7F, 0x49, 0x49, 0x49, 0x36},            // Code for char B
                new byte[] {0x3E, 0x41, 0x41, 0x41, 0x22},            // Code for char C
                new byte[] {0x7F, 0x41, 0x41, 0x22, 0x1C},            // Code for char D
                new byte[] {0x7F, 0x49, 0x49, 0x49, 0x41},            // Code for char E
                new byte[] {0x7F, 0x09, 0x09, 0x09, 0x01},            // Code for char F
                new byte[] {0x3E, 0x41, 0x49, 0x49, 0x7A},            // Code for char G
                new byte[] {0x7F, 0x08, 0x08, 0x08, 0x7F},            // Code for char H
                new byte[] {0x00, 0x41, 0x7F, 0x41, 0x00},            // Code for char I
                new byte[] {0x20, 0x40, 0x41, 0x3F, 0x01},            // Code for char J
                new byte[] {0x7F, 0x08, 0x14, 0x22, 0x41},            // Code for char K
                new byte[] {0x7F, 0x40, 0x40, 0x40, 0x40},            // Code for char L
                new byte[] {0x7F, 0x02, 0x0C, 0x02, 0x7F},            // Code for char M
                new byte[] {0x7F, 0x04, 0x08, 0x10, 0x7F},            // Code for char N
                new byte[] {0x3E, 0x41, 0x41, 0x41, 0x3E},            // Code for char O
                new byte[] {0x7F, 0x09, 0x09, 0x09, 0x06},            // Code for char P
                new byte[] {0x3E, 0x41, 0x51, 0x21, 0x5E},            // Code for char Q
                new byte[] {0x7F, 0x09, 0x19, 0x29, 0x46},            // Code for char R
                new byte[] {0x46, 0x49, 0x49, 0x49, 0x31},            // Code for char S
                new byte[] {0x01, 0x01, 0x7F, 0x01, 0x01},            // Code for char T
                new byte[] {0x3F, 0x40, 0x40, 0x40, 0x3F},            // Code for char U
                new byte[] {0x1F, 0x20, 0x40, 0x20, 0x1F},            // Code for char V
                new byte[] {0x3F, 0x40, 0x38, 0x40, 0x3F},            // Code for char W
                new byte[] {0x63, 0x14, 0x08, 0x14, 0x63},            // Code for char X
                new byte[] {0x07, 0x08, 0x70, 0x08, 0x07},            // Code for char Y
                new byte[] {0x61, 0x51, 0x49, 0x45, 0x43},            // Code for char Z
                new byte[] {0x00, 0x7F, 0x41, 0x41, 0x00},            // Code for char [
                new byte[] {0x02, 0x04, 0x08, 0x10, 0x20},            // Code for char BackSlash
                new byte[] {0x00, 0x41, 0x41, 0x7F, 0x00},            // Code for char ]
                new byte[] {0x04, 0x02, 0x01, 0x02, 0x04},            // Code for char ^
                new byte[] {0x40, 0x40, 0x40, 0x40, 0x40},            // Code for char _
                new byte[] {0x00, 0x01, 0x02, 0x04, 0x00},            // Code for char `
                new byte[] {0x20, 0x54, 0x54, 0x54, 0x78},            // Code for char a
                new byte[] {0x7F, 0x50, 0x48, 0x48, 0x30},            // Code for char b
                new byte[] {0x7F, 0x50, 0x48, 0x48, 0x30},            // Code for char b
                new byte[] {0x38, 0x44, 0x44, 0x44, 0x20},            // Code for char c
                new byte[] {0x7F, 0x50, 0x48, 0x48, 0x30},            // Code for char b
                new byte[] {0x30, 0x48, 0x48, 0x50, 0x7F},            // Code for char d
                new byte[] {0x38, 0x54, 0x54, 0x54, 0x18},            // Code for char e
                new byte[] {0x08, 0x7E, 0x09, 0x01, 0x06},            // Code for char f
                new byte[] {0x0C, 0x52, 0x52, 0x52, 0x3E},            // Code for char g
                new byte[] {0x7F, 0x08, 0x04, 0x04, 0x78},            // Code for char h
                new byte[] {0x00, 0x00, 0x7A, 0x00, 0x00},            // Code for char i
                new byte[] {0x00, 0x20, 0x40, 0x44, 0x3D},            // Code for char j
                new byte[] {0x7F, 0x10, 0x28, 0x44, 0x00},            // Code for char k
                new byte[] {0x00, 0x41, 0x7F, 0x40, 0x00},            // Code for char l
                new byte[] {0x7C, 0x04, 0x18, 0x04, 0x78},            // Code for char m
                new byte[] {0x7C, 0x08, 0x04, 0x04, 0x78},            // Code for char n
                new byte[] {0x38, 0x44, 0x44, 0x44, 0x38},            // Code for char o
                new byte[] {0x7C, 0x14, 0x14, 0x14, 0x08},            // Code for char p
                new byte[] {0x04, 0x0A, 0x0A, 0x0C, 0x3E},            // Code for char q
                new byte[] {0x7C, 0x08, 0x04, 0x04, 0x08},            // Code for char r
                new byte[] {0x48, 0x54, 0x54, 0x54, 0x20},            // Code for char s
                new byte[] {0x02, 0x3F, 0x42, 0x40, 0x20},            // Code for char t
                new byte[] {0x3C, 0x40, 0x40, 0x20, 0x7C},            // Code for char u
                new byte[] {0x1C, 0x20, 0x40, 0x20, 0x1C},            // Code for char v
                new byte[] {0x3C, 0x40, 0x30, 0x40, 0x3C},            // Code for char w
                new byte[] {0x44, 0x28, 0x10, 0x28, 0x44},            // Code for char x
                new byte[] {0x0C, 0x50, 0x50, 0x50, 0x3C},            // Code for char y
                new byte[] {0x44, 0x64, 0x54, 0x4C, 0x44},            // Code for char z
                new byte[] {0x00, 0x08, 0x36, 0x41, 0x00},            // Code for char {
                new byte[] {0x00, 0x00, 0x7F, 0x00, 0x00},            // Code for char |
                new byte[] {0x00, 0x41, 0x36, 0x08, 0x00},            // Code for char }
                new byte[] {0x10, 0x08, 0x08, 0x10, 0x08},            // Code for char ~
                new byte[] {0x00, 0x3E, 0x22, 0x3E, 0x00}             // Code for char 
            };
        
        #endregion

    }
}

