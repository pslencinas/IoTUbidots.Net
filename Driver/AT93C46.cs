using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace SERIAL_EEPROM
{
    public class AT93CXX
    {
        private const byte START_BIT = 0x01;

        #region "OPCODE's Definition"

            private const byte OPCODE_EWEN = 0x0;       //0b00
            private const byte OPCODE_EWDS = 0x0;       //0b00
            private const byte OPCODE_WRITE = 0x1;      //0b01
            private const byte OPCODE_READ = 0x2;       //0b10
            private const byte OPCODE_ERASE = 0x3;      //0b11
        
        #endregion

        /// <summary>
        /// Memory Organization BYTE/WORD
        /// </summary>
        public enum enumORG
        {
            BYTE = 8,
            WORD = 16
        }

        /// <summary>
        /// Memory Model --> Size capacity
        /// </summary>
        /// 
        public enum enumMODEL
        {
            AT93C06 = 64,       //A06-7/A05-6 0x0040/0x0020     10/9
            AT93C46 = 128,
            AT93C56 = 256,      //A08-9/A07-8 0x0100/0x0080     12/11
            AT93C66 = 512,
            AT93C76 = 1024,     //A10-11/A09-10 0x0400/0x0200   14/13
            AT93C86 = 2048
        }
        
        /// <summary>
        /// MEMORY SIZE in [BYTES]
        /// </summary>
        static private int MEMORY_SIZE = 0;
        
        /// <summary>
        /// DATA LENTH in [BITS] depending on the "ORG PIN"
        /// </summary>
        static private byte DATA_LENGHT = 0x08;

        /// <summary>
        /// OPCODE LENGHT depending on MEMORY SIZE & ORGANIZATION
        /// </summary>
        static private byte OPCODE_LENGHT = 0x00;

        #region "Default Hardware Pinout Mapping Definition"
        
            static OutputPort CS = null;
            static OutputPort SCK = null;
            static OutputPort DI = null;
            static InputPort DO =null; 
        
        #endregion

        /// <summary>
        /// Class Constructor
        /// </summary>
        /// <param name="memModel">Device Model</param>
        /// <param name="memOrganization">16-Bit / 8-Bit Memory Organization</param>
        /// <param name="pCHIPSELECT">MCU Pin Connected to the memeory CS (output)</param>
        /// <param name="pSERIALCLOCK">MCU Pin Connected to the memory SCK pin (output)</param>
        /// <param name="pDATAINPUT">MCU Pin Connected to the memory DI pin (output)</param>
        /// <param name="pDATAOUTPUT">MCU Pin Connected to the memory DO pin (input)</param>
        public AT93CXX(enumMODEL memModel, enumORG memOrganization, Cpu.Pin pCHIPSELECT, Cpu.Pin pSERIALCLOCK, Cpu.Pin pDATAINPUT, Cpu.Pin pDATAOUTPUT ) 
        {
            CS = new OutputPort(pCHIPSELECT, false);                             //CHIP SELECT -->
            SCK = new OutputPort(pSERIALCLOCK, false);                           //SERIAL CLOCK -->
            DI = new OutputPort(pDATAINPUT, false);                              //MASTER OUTPUT -->> SLAVE INPUT
            DO = new InputPort(pDATAOUTPUT, false, Port.ResistorMode.PullUp);     //MASTER INPUT <<-- SLAVE OUTPUT

            //Based on the device model and the "organization" selected, we can calculate the memory size in bytes and the data lenght
            MEMORY_SIZE = (int)memModel; if (memOrganization == enumORG.WORD) { MEMORY_SIZE /= 2; DATA_LENGHT = 16; }
            
            //With the memory size we can calculate how many address bits are requiered plus the start bit and the opcodes bits
            OPCODE_LENGHT = (byte)((System.Math.Log10(MEMORY_SIZE) / System.Math.Log10(2)) + 3);
        }

        #region "Private Functions and Procedures"

            /// <summary>
            /// Write the Opcode+Address to the serial device, after this procedure ends, CS remains still active
            /// </summary>
            /// <param name="OperationCode">Instruction OPCODE</param>
            /// <param name="Address">Address value</param>
            private void Write_OpCode(byte OperationCode, int Address)
            {
                int OpCode = (START_BIT << (OPCODE_LENGHT - 1)) + (OperationCode << (OPCODE_LENGHT - 3)) + (Address & 0x7FF);

                SCK.Write(false); DI.Write(false);      //CLOCK LOW & DI LOW

                CS.Write(true);                         //CS HIGH

                //WRITE OPCODE + ADDRESS

                for (int index = 0; index < OPCODE_LENGHT; index++)
                {
                    if (((OpCode << index) & (1 << (OPCODE_LENGHT - 1))) == (1 << (OPCODE_LENGHT - 1)))
                        DI.Write(true);
                    else
                        DI.Write(false);

                    SCK.Write(true); SCK.Write(false);      //CLOCK PULSE _|-|_    "OPCODE BITn"
                }
            }

            /// <summary>
            /// Reads Data (BYTE/WORD) from device after OPCODE+ADDRESS has been sent to the device
            /// </summary>
            /// <returns>Value read from device</returns>
            private int Read_DataByte()
            {
                int data = 0x0000;

                for (int index = 0; index < DATA_LENGHT; index++)
                {
                    SCK.Write(true); SCK.Write(false);      //CLOCK PULSE _|-|_    "DATA BITn"

                    if (DO.Read() == true)
                        data = ((data << 1)) + 1;
                    else
                        data = ((data << 1)) + 0;
                }

                return data;
            }

            /// <summary>
            /// Writes data (BYTE/WORD) after OPCODE+ADDRESS has been sent to the device.
            /// </summary>
            /// <param name="Value">Value to be written</param>
            /// <param name="timeout">Optional Timeout value in [mS]</param>
            /// <returns>TRUE if value was correclty written / FALSE on failure</returns>
            private bool Write_DataByte(int Value, int timeout=100)
            {
                if (DATA_LENGHT == 0x08) { Value = (Value & 0xFF); }

                for (int index = 0; index < DATA_LENGHT; index++)
                {
                    if (((Value << index) & (1 << (DATA_LENGHT - 1))) == (1 << (DATA_LENGHT - 1)))
                        DI.Write(true);
                    else
                        DI.Write(false);

                    SCK.Write(true); SCK.Write(false);  //CLOCK PULSE _|-|_    "DATA BITn"
                }

                CS.Write(false); CS.Write(true);        //CS PULSE -|_|-    

                while (DO.Read() == false)
                {
                    //TODO: Implementar TIMEOUT x si falla --> return false;
                }

                return true;
            }
        
        #endregion

        #region "Exposed Functions and Procedures"

            /// <summary>
            /// Reads a single memory location and return its value
            /// </summary>
            /// <param name="Address">Address location</param>
            /// <returns>The Value of that memory address</returns>
            public int ReadAddress(int Address)
            {
                int data = 0x0000;

                Write_OpCode(OPCODE_READ, Address); //WRITE OPCODE+ADDRESS

                data = Read_DataByte();             //READ DATA

                CS.Write(false);                    //CS LOW

                return data;
            }

            public int ReadAddress(int[] Buffer, int Address, int Size)
            {
                for (int index = 0; index < Size; index++)
                {
                
                }
                
                return 0;
            }
            
            /// <summary>
            /// Enables Memory WRITE/ERASE
            /// </summary>
            public void EWEN()
            {
                Write_OpCode(OPCODE_EWEN, 0x0060);  //WRITE OPCODE+ADDRESS

                CS.Write(false);                    //CS LOW
            }
            
            /// <summary>
            /// Disables Memory WRITE/ERASE
            /// </summary>
            public void EWDS()
            {
                Write_OpCode(OPCODE_EWDS, 0x0000);  //WRITE OPCODE+ADDRESS

                CS.Write(false);                    //CS LOW
            }

            /// <summary>
            /// Erase a single memory address, the default value after erase is all bits to "1"
            /// </summary>
            /// <param name="Address">Address Location</param>
            /// <param name="timeout">Optional Timeout value in [mS]</param>
            /// <returns>TRUE if it was erased / FALSE on failure</returns>
            public bool EraseAddress(int Address, int timeout=100)
            {
                Write_OpCode(OPCODE_ERASE, Address);    //WRITE OPCODE+ADDRESS

                while (DO.Read() == false)
                {
                    //TODO: Implementar TIMEOUT x si falla --> return false;
                }

                Thread.Sleep(1);

                CS.Write(false);                        //CS LOW

                return true;
            }

            /// <summary>
            /// Writes data in a specific memory location with previous erase and post verification
            /// </summary>
            /// <param name="Address">Address Location</param>
            /// <param name="Data">Value to write to device</param>
            /// <returns>TRUE if data was succesfully written to device / FALSE on failure</returns>
            public bool WriteAddress(int Address, int Data)
            {
                if (EraseAddress(Address) == false) return false;   //ERASE ADDRESS

                Write_OpCode(OPCODE_WRITE, Address);                //WRITE OPCODE+ADDRESS

                Write_DataByte(Data);

                CS.Write(false);    //CS LOW
                
                Thread.Sleep(1);

                //POST VERIFICATION

                int retData = ReadAddress(Address);     //READ DATA

                CS.Write(false);                        //CS LOW

                if (retData != Data) return false;

                return true;
            }

            public bool EraseALL()
            {

                return true;
            }
        #endregion
    }
}

