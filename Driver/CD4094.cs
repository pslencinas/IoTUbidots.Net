using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace SERIAL_OUTPUT
{
    class CD4094
    {
        #region "HARDWARE PIN DEFINITIONS"
            private OutputPort DATA = null;        //DATA
            private OutputPort CLK = null;         //CLOCK
            private OutputPort STROBE = null;      //STROBE
        #endregion
        
        #region "CONST's"
            private const UInt32 DEFAULT_VALUE = 0x00;
        #endregion

        #region "VARS GLOBALES"
            
            private UInt32 actual_value;
            
            private static Object SR_Lock = new Object();

            private byte bit_lenght = 8;
        
        #endregion
    
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pDATA">DATA pin</param>
        /// <param name="pCLOCK">CLOCK pin</param>
        /// <param name="pSTROBE">STROBE pin</param>
        
        public CD4094(Cpu.Pin pDATA, Cpu.Pin pCLOCK, Cpu.Pin pSTROBE)
        {
            DATA = new OutputPort(pDATA, false);        //DATA
            CLK = new OutputPort(pCLOCK, false);         //CLOCK
            STROBE = new OutputPort(pSTROBE, false);     //STROBE

            Write(DEFAULT_VALUE);
        }

        /// <summary>
        /// Set the outputs according the value
        /// </summary>
        /// <param name="value">value to be set</param>
        public void Write(UInt32 value)
        {
            lock (SR_Lock)
            {
                STROBE.Write(false);

                for (int index = 0; index < bit_lenght ; index++)
                {
                    if ((UInt32)((value << index) & (1 << (bit_lenght - 1))) == (1 << (bit_lenght - 1)))
                        DATA.Write(true);
                    else
                        DATA.Write(false);

                    CLK.Write(true); CLK.Write(false);      //CLOCK PULSE _|-|_ 
                }

                STROBE.Write(true); STROBE.Write(false);    //STROBE PULSE _|-|_ 

                actual_value = value;
            }
        }

        /// <summary>
        /// Returns the last value written to the shift registers
        /// </summary>
        /// <returns>Actual SRs value</returns>
        public UInt32 Read() { return actual_value; }

        public void Write(byte number, bool status)
        {
            if(status==true)
                actual_value |= (UInt32)(1<<number);
            else 
                actual_value &= ~((UInt32)(1<<number));

            Write(actual_value);
        }
    }
}
