using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace DISPLAY_LCD
{
    class LCD_HD44780
    {
        #region "CONST's"

            private const bool COMMAND_TYPE = false;
            private const bool DATA_TYPE = true;

            private const byte LCD_DD_RAM_ADDR1 = 0x80;
            private const byte LCD_DD_RAM_ADDR2 = 0xC0;

            private const byte LCD_CLEARDISPLAY = 0x01;
            private const byte LCD_RETURNHOME = 0x02;

            private const byte LCD_MODE_8BITS = 0x08;
            private const byte LCD_MODE_4BITS = 0x04;
            
        #endregion

        #region "HARDWARE PIN DEFINITIONS"
        
            static OutputPort LCD_DATA = null;         //Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_C_2
            static OutputPort LCD_CLK = null;          //Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_C_3
            static OutputPort LCD_STROBE = null;       //Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_B_11

        #endregion

        private bool _visible = true;
        private bool _showCursor = false;
        private bool _blinkCursor=false;

        private byte _numLines = 2;
        private byte _numColumns = 16;

        private byte _modeBits = LCD_MODE_4BITS;

        private byte _busData = 0x00;
        
        /// <summary>
        /// Enum with LCD line identificator
        /// </summary>
        public enum eRow
        {
            BOTH = 0,
            UPPER = 1,
            LOWER = 2
        }

        /// <summary>
        /// Enum with text aligment posibilities
        /// </summary>
        public enum eAligment
        {
            CENTER = 0,
            LEFT= 1,
            RIGHT = 2,
            MANUAL = 3
        }

        private byte _currColumn = 0;
        private eRow _currRow = eRow.UPPER;

        private long _TicksPerMicroSecond = TimeSpan.TicksPerMillisecond / 1000;

        private void Wait(long microseconds)
        {
            var then = Utility.GetMachineTime().Ticks;
            var ticksToWait = microseconds * _TicksPerMicroSecond;
            while (true)
            {
                var now = Utility.GetMachineTime().Ticks;
                if ((now - then) > ticksToWait) break;
            }
        }

        private const int PULSE_EN_DELAY = 1000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pLCD_DATA">Cpu PIN connected to DATA input in Serial Shift Register</param>
        /// <param name="pLCD_CLK">Cpu PIN connected to CLOCK input in Serial Shift Register</param>
        /// <param name="pLCD_STROBE">Cpu PIN connected to STROBE input in Serial Shift Register</param>
        public LCD_HD44780(Cpu.Pin pLCD_DATA, Cpu.Pin pLCD_CLK, Cpu.Pin pLCD_STROBE) 
        {
            LCD_DATA = new OutputPort(pLCD_DATA, false);
            LCD_CLK = new OutputPort(pLCD_CLK, false);
            LCD_STROBE = new OutputPort(pLCD_STROBE, false);
            
            _modeBits = LCD_MODE_4BITS;
        }

        /// <summary>
        /// Writes raw data to shift register
        /// </summary>
        /// <param name="data">data to write</param>
        private void WriteDataBus(byte data)
        {
            LCD_STROBE.Write(false);

            for (int index = 0; index < 8; index++)
            {
                if (((data << index) & (1 << 7)) == (1 << 7))
                    LCD_DATA.Write(true);
                else
                    LCD_DATA.Write(false);

                LCD_CLK.Write(true); LCD_CLK.Write(false);      //CLOCK PULSE _|-|_ 
            }

            LCD_STROBE.Write(true); LCD_STROBE.Write(false);
        }

        /// <summary>
        /// Writes DATA/COMMAND to the LCD controller
        /// </summary>
        /// <param name="value">value to write</param>
        /// <param name="IsData">specifies if value is data or command</param>
        private void WriteLCD(byte value, bool IsData)
        {
            byte dataAux = 0;

            //----------------------------------------
            //Send HI NIBBLE
            //----------------------------------------

            dataAux = (byte)((value & 0xF0) >> 4);

            dataAux += (1 << 4); if (IsData) dataAux += (1 << 5); //EN + WRITE + RS

            WriteDataBus(dataAux); //Send data with EN=1

            Thread.Sleep(1);    //Wait(PULSE_EN_DELAY);

            //EN = 0 _|-|_

            dataAux = (byte)((value & 0xF0) >> 4);

            if (IsData) dataAux += (1 << 5); //EN + WRITE + RS

            WriteDataBus(dataAux); //Send data with EN=0

            Thread.Sleep(1);    //Wait(PULSE_EN_DELAY);

            //----------------------------------------
            //Send LO NIBBLE
            //----------------------------------------

            dataAux = (byte)(value & 0x0F);

            dataAux += (1 << 4); if (IsData) dataAux += (1 << 5); //EN + WRITE + RS

            WriteDataBus(dataAux); //Send data with EN=1

            Thread.Sleep(1);    //Wait(PULSE_EN_DELAY);

            //EN = 0 _|-|_

            dataAux = (byte)(value & 0x0F);

            if (IsData) dataAux += (1 << 5); //EN + WRITE + RS

            WriteDataBus(dataAux); //Send data with EN=0

            Thread.Sleep(1);    //Wait(PULSE_EN_DELAY);

        }

        /// <summary>
        /// Set display optiones in Control Register
        /// </summary>
        /// <param name="DisplayON">Enable/Disable Display</param>
        /// <param name="CursorON">Show/Hide Cursor/param>
        /// <param name="BlinkCursor">Enable/Disable Blink</param>
        private void DisplayControl(bool DisplayON, bool CursorON, bool BlinkCursor)
        {
            byte value = (1 << 3);

            if (DisplayON == true) value += (1 << 2);
            if (CursorON == true) value += (1 << 1);
            if (BlinkCursor == true) value += (1 << 0);

            WriteLCD(value, COMMAND_TYPE);
        }
        
        /// <summary>
        /// Initialize HD44780 LCD Controller
        /// <remarks>after initialization, DISPLAY=ON/CURSOR=OFF/BLINK=OFF</remarks>
        /// </summary>
        public void Init(byte columns, byte lines)
        {
            _numColumns = columns; _numLines = lines;

            WriteDataBus(0x00);     //LCD_RS.Write(false); LCD_EN.Write(false);
            
            //Step 1.   Power on, then delay > 100 ms 
            //There are two different values (with two different references) specified on the datasheet flowchart for this initial delay 
            //but neither reference is from when the power was first applied. The delay required from power-up must obviously be more than 
            //40 mS and I have arbitrarily chosen to use 100 mS. Since this delay only occurs once it doesen't make sense to try to speed up 
            //program execution time by skimping on this delay. 

            Thread.Sleep(100);   //Wait for more than 15 ms after VCC rises to 4.5 V

            //Step 2.   Instruction 00110000b (30h), then delay > 4.1 ms 
            //This is a special case of the Function Set instruction where the lower four bits are irrelevant.
            //These four bits are shown as asterisks on flowcharts because the controller will ignore them.
            //They are shown as '0's in these notes because that is how most programmers deal with irrelevant bits.
            //This first instruction, for some unexplained reason, takes significantly longer to complete than the ones that come later. 
            WriteDataBus(0x13); Thread.Sleep(1); WriteDataBus(0x03);
            
            Thread.Sleep(5);

            //Step 3.   Instruction 00110000b (30h), then delay > 100 us 
            //This is a second instance of the special case of the Function Set instruction.
            //The controller does not normally expect to receive more than one 'Function Set' instruction so this may
            //account for the longer than normal execution time.
            WriteDataBus(0x13); Thread.Sleep(1); WriteDataBus(0x03);

            Thread.Sleep(1);

            //Step 4.   Instruction 00110000b (30h), then delay > 100 us 
            //This is a third instance of the special case of the Function Set instruction. By now the LCD controller realizes 
            //that what is really intended is a 'reset', and it is now ready for the real Function Set instruction followed by 
            //the rest of the initialization instructions. The flowcharts do not specify what time delay belongs here.   
            //I have chosen 100 us to agree with the previous instruction. It may be possible to check the busy flag here. 

            WriteDataBus(0x12); Thread.Sleep(1); WriteDataBus(0x02);
            
            //Step 5.   Instruction 00111000b (38h), then delay > 53 us or check BF 
            //This is the real Function Set instruction. This is where the interface, the number of lines, and the font are specified.
            //Since we are implementing the 8-bit interface we make D = 1. The number of lines being specified here is the number of 'logical' lines 
            //as perceived by the LCD controller, it is NOT the number of 'physical' lines (or rows) that appear on the actual display.
            //This should almost always be two lines so we set N=1 (go figure). There are very few displays capable of displaying a 5x10 font so the 5x7 choice 
            //is almost always correct and we set F=0. 

            WriteLCD(0x28, false);

            //Step 6.   Instruction 00001000b (08h), then delay > 53 us or check BF 
            //This is the Display on/off Control instruction. This instruction is used to control several aspects of the display but now is NOT the time to set 
            //the display up the way we want it. The flow chart shows the instruction as 00001000, not 00001DCB which indicates that the Display (D), 
            //the Cursor (C), and the Blinking (B) should all be turned off by making the corresponding bits = 0. 

            WriteLCD(0x08, false);

            //Step 7.   Instruction 00000001b (01h), then delay > 3 ms or check BF 
            //This is the Clear Display instruction which, since it has to write information to all 80 DDRAM addresses, takes more time to execute than 
            //most of the other instructions. On some flow charts the comment is incorrectly labeled as 'Display on' but the instruction itself is correct. 

            WriteLCD(0x01, false);
            
            //Step 8.   Instruction 00000110b (06h), then delay > 53 us or check BF 
            //This is the Entry Mode Set instruction. This instruction determines which way the cursor and/or the display moves when we enter a string of characters.
            //We normally want the cursor to increment (move from left to right) and the display to not shift so we set I/D=1 and S=0.
            //If your application requires a different configuration you could change this instruction, but my recommendation is to leave this instruction 
            //alone and just add another Entry Mode Set instruction where appropriate in your program. 

            WriteLCD(0x06, false);

            //Step 9.   Initialization ends 
            //This is the end of the actual intitalization sequence, but note that step 6 has left the display off. 

            //Step 10.   Instruction 00001100b (0Ch), then delay > 53 us or check BF 
            //This is another Display on/off Control instruction where the display is turned on and where the cursor can be made visible and/or 
            //the cursor location can be made to blink.   This example shows the the display on and the other two options off, D=1, C=0, and B=0. 

            DisplayControl(_visible, _showCursor, _blinkCursor);
        }

        /// <summary>
        /// Set cursor position
        /// </summary>
        /// <param name="row">Specifies the row</param>
        /// <param name="column">Specifies the column</param>
        public void CursorPosition(eRow row, byte column)
        {
            _currRow = row; _currColumn = column;

            if (row == eRow.UPPER)
                WriteLCD((byte)(LCD_DD_RAM_ADDR1 + column), COMMAND_TYPE);      //Posicionamos la linea 1

            if (row == eRow.LOWER)
                WriteLCD((byte)(LCD_DD_RAM_ADDR2 + column), COMMAND_TYPE);      //Posicionamos la linea 2
            
            System.Threading.Thread.Sleep(2);
        }

        /// <summary>
        /// Clear the screen
        /// </summary>
        /// <param name="row">Specifies the row to clear</param>
        /// <remarks>After clear cursor is set to the firts column of the row</remarks>
        public void Clear(eRow row)
        {
            if (row == eRow.BOTH)
            {
                WriteLCD(LCD_CLEARDISPLAY, COMMAND_TYPE); System.Threading.Thread.Sleep(2);    //Return HOME + Clear LCD
            }
            else
            {
                CursorPosition(row, 0);

                for (byte index = 0; index < _numColumns; index++)
                {
                    WriteLCD(0x20, DATA_TYPE);  //Print &H20 Char & Return HOME
                }

                CursorPosition(row, 0);
            }

        }

        public void Write(string text, eAligment aligment, eRow row, byte offsetColumn)
        {
            byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(text);

            switch (aligment)
            {
                case eAligment.MANUAL: { CursorPosition(row, (byte)(offsetColumn-1)); break; }

                case eAligment.LEFT: { CursorPosition(row, 0x00); break; }

                case eAligment.RIGHT: { CursorPosition(row, (byte)(_numColumns - text.Length)); break; }

                case eAligment.CENTER: { CursorPosition(row, (byte)((_numColumns - text.Length) / 2)); break; }
            }
             
            for (int i = 0; i < buffer.Length; i++)
            {
                WriteLCD(buffer[i], DATA_TYPE);
            }
        }
        
        #region "PROPIEDADES"
        
            public bool Visible
            {
                get { return _visible; }
                set
                {
                    _visible = value; DisplayControl( _visible, _showCursor , _blinkCursor);
                }
            }

            public bool ShowCursor
            {
                get { return _showCursor; }
                set
                {
                    _showCursor = value; DisplayControl(_visible, _showCursor, _blinkCursor);
                }
            }

            public bool BlinkCursor
            {
                get { return _blinkCursor; }
                set
                {
                    _blinkCursor = value; DisplayControl(_visible, _showCursor, _blinkCursor);
                }
            }    
        #endregion
    }

}
