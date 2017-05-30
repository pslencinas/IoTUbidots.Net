using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace DIGITAL_INPUTS
{
    class MC33972
    {
        #region "Commands"

        private const byte MSDI_SWITCHSTAT_CMD      = 0x00;
        private const byte MSDI_SETTINGS_CMD        = 0x01;
        private const byte MSDI_WAKEUPINTSP_CMD     = 0x02;
        private const byte MSDI_WAKEUPINTSG_CMD     = 0x03;
        private const byte MSDI_WETCURRENTSP_CMD    = 0x04;
        private const byte MSDI_WETCURRENTSG_CMD    = 0x05;
        private const byte MSDI_ANALOG_CMD          = 0x06;
        private const byte MSDI_WETTINGTIMERSP_CMD  = 0x07;
        private const byte MSDI_WETTINGTIMERSG_CMD  = 0x08;
        private const byte MSDI_TRISTATESP_CMD      = 0x09;
        private const byte MSDI_TRISTATESG_CMD      = 0x0A;
        private const byte MSDI_CALIBRATION_CMD     = 0x0B;
        private const byte MSDI_SLEEP_CMD           = 0x0C;
        private const byte MSDI_RESET_CMD           = 0x7F;

        #endregion

        #region "SP/SG configuration"

        public const byte MSDI_NONE     = 0x0000;
        public const byte MSDI_SP0      = 0x0001;
        public const byte MSDI_SP1      = 0x0002;
        public const byte MSDI_SP2      = 0x0004;
        public const byte MSDI_SP3      = 0x0008;
        public const byte MSDI_SP4      = 0x0010;
        public const byte MSDI_SP5      = 0x0020;
        public const byte MSDI_SP6      = 0x0040;
        public const byte MSDI_SP7      = 0x0080;
        public const byte MSDI_SP_ALL   = 0x00FF;

        public const ushort MSDI_SG0    = 0x0001;
        public const ushort MSDI_SG1    = 0x0002;
        public const ushort MSDI_SG2    = 0x0004;
        public const ushort MSDI_SG3    = 0x0008;
        public const ushort MSDI_SG4    = 0x0010;
        public const ushort MSDI_SG5    = 0x0020;
        public const ushort MSDI_SG6    = 0x0040;
        public const ushort MSDI_SG7    = 0x0080;
        public const ushort MSDI_SG8    = 0x0100;
        public const ushort MSDI_SG9    = 0x0200;
        public const ushort MSDI_SG10   = 0x0400;
        public const ushort MSDI_SG11   = 0x0800;
        public const ushort MSDI_SG12   = 0x1000;
        public const ushort MSDI_SG13   = 0x2000;
        public const ushort MSDI_SG_ALL = 0x3FFF;

        private const byte MSDI_SG_INPUTS = 0x00;
        private const byte MSDI_SP_INPUTS = 0x01;

        public enum MSDI_INPUTS
        {
            GROUND_SWITCHS = MSDI_SG_INPUTS,
            PROGRAMMABLE_SWITCHS = MSDI_SP_INPUTS
        }

        #endregion

        #region "Configuration constants"

        private const ushort MSDI_SWtoGND = 0;
        private const ushort MSDI_SWtoBAT = 1;
        
        private const ushort MSDI_2mA = 0;
        private const ushort MSDI_16mA = 1;

        public enum BatteryGroundSelect
        {
            SWITCH_TO_GROUND = MSDI_SWtoGND,
            SWITCH_TO_BATTERY = MSDI_SWtoBAT
        }

        #endregion

        #region "Analog Command"

        /* Current Mode */
        private const ushort MSDI_AN_HiZ = 0x0000;
        private const ushort MSDI_AN_2mA = 0x0020;
        private const ushort MSDI_AN_16mA = 0x0040;

        /* Channel */
        private const ushort MSDI_AN_NONE = 0x0000;
        private const ushort MSDI_AN_SG0 = 0x0001;
        private const ushort MSDI_AN_SG1 = 0x0002;
        private const ushort MSDI_AN_SG2 = 0x0003;
        private const ushort MSDI_AN_SG3 = 0x0004;
        private const ushort MSDI_AN_SG4 = 0x0005;
        private const ushort MSDI_AN_SG5 = 0x0006;
        private const ushort MSDI_AN_SG6 = 0x0007;
        private const ushort MSDI_AN_SG7 = 0x0008;
        private const ushort MSDI_AN_SG8 = 0x0009;
        private const ushort MSDI_AN_SG9 = 0x000A;
        private const ushort MSDI_AN_SG10 = 0x000B;
        private const ushort MSDI_AN_SG11 = 0x000C;
        private const ushort MSDI_AN_SG12 = 0x000D;
        private const ushort MSDI_AN_SG13 = 0x000E;
        private const ushort MSDI_AN_SP0 = 0x000F;
        private const ushort MSDI_AN_SP1 = 0x0010;
        private const ushort MSDI_AN_SP2 = 0x0011;
        private const ushort MSDI_AN_SP3 = 0x0012;
        private const ushort MSDI_AN_SP4 = 0x0013;
        private const ushort MSDI_AN_SP5 = 0x0014;
        private const ushort MSDI_AN_SP6 = 0x0015;
        private const ushort MSDI_AN_SP7 = 0x0016;

        /// <summary>
        /// set the desired current
        /// </summary>
        public enum AnalogCurrentSettings
        {
            HIGH_IMPEDANCE = MSDI_AN_HiZ,
            CURRENT_2mA = MSDI_AN_2mA,
            CURRENT_16mA = MSDI_AN_16mA
        }

        /// <summary>
        /// Analog Channel Select
        /// </summary>
        public enum AnalogChannel
        {
            ANALOG_NONE = MSDI_AN_NONE,
            ANALOG_SG0 = MSDI_AN_SG0,
            ANALOG_SG1 = MSDI_AN_SG1,
            ANALOG_SG2 = MSDI_AN_SG2,
            ANALOG_SG3 = MSDI_AN_SG3,
            ANALOG_SG4 = MSDI_AN_SG4,
            ANALOG_SG5 = MSDI_AN_SG5,
            ANALOG_SG6 = MSDI_AN_SG6,
            ANALOG_SG7 = MSDI_AN_SG7,
            ANALOG_SG8 = MSDI_AN_SG8,
            ANALOG_SG9 = MSDI_AN_SG9,
            ANALOG_SG10 = MSDI_AN_SG10,
            ANALOG_SG11 = MSDI_AN_SG11,
            ANALOG_SG12 = MSDI_AN_SG12,
            ANALOG_SG13 = MSDI_AN_SG13,
            ANALOG_SP0 = MSDI_AN_SP0,
            ANALOG_SP1 = MSDI_AN_SP1,
            ANALOG_SP2 = MSDI_AN_SP2,
            ANALOG_SP3 = MSDI_AN_SP3,
            ANALOG_SP4 = MSDI_AN_SP4,
            ANALOG_SP5 = MSDI_AN_SP5,
            ANALOG_SP6 = MSDI_AN_SP6,
            ANALOG_SP7 = MSDI_AN_SP7
        }

        #endregion

        #region "Timer values for Sleep Command"

        private const ushort MSDI_NO_SCAN = 0x0000;
        private const ushort MSDI_SCAN_1ms = 0x0001;
        private const ushort MSDI_SCAN_2ms = 0x0002;
        private const ushort MSDI_SCAN_4ms = 0x0003;
        private const ushort MSDI_SCAN_8ms = 0x0004;
        private const ushort MSDI_SCAN_16ms = 0x0005;
        private const ushort MSDI_SCAN_32ms = 0x0006;
        private const ushort MSDI_SCAN_64ms = 0x0007;

        private const ushort MSDI_INT_32ms = 0x0000;
        private const ushort MSDI_INT_64ms = 0x0008;
        private const ushort MSDI_INT_128ms = 0x0010;
        private const ushort MSDI_INT_256ms = 0x0018;
        private const ushort MSDI_INT_512ms = 0x0020;
        private const ushort MSDI_INT_1024ms = 0x0028;
        private const ushort MSDI_INT_2048ms = 0x0030;
        private const ushort MSDI_NO_INTms = 0x0038;

        /// <summary>
        /// The scan timer sets the polling period between input switch reads in Sleep mode
        /// </summary>
        public enum scanPeriod
        {
            NO_SCAN = MSDI_NO_SCAN,
            SCAN_1mS = MSDI_SCAN_1ms,
            SCAN_2mS = MSDI_SCAN_2ms,
            SCAN_4mS = MSDI_SCAN_4ms,
            SCAN_8mS = MSDI_SCAN_8ms,
            SCAN_16mS = MSDI_SCAN_16ms,
            SCAN_32mS = MSDI_SCAN_32ms,
            SCAN_64mS = MSDI_SCAN_64ms
        }

        /// <summary>
        /// The interrupt timer is used as a periodic wake-up timer.
        /// </summary>
        public enum interruptPeriod
        {
            INTERRUPT_32mS = MSDI_INT_32ms,
            INTERRUPT_64mS = MSDI_INT_64ms,
            INTERRUPT_128mS = MSDI_INT_128ms,
            INTERRUPT_256mS = MSDI_INT_256ms,
            INTERRUPT_512mS = MSDI_INT_512ms,
            INTERRUPT_1024mS = MSDI_INT_1024ms,
            INTERRUPT_2048mS = MSDI_INT_2048ms,
            NO_INTERRUPT_WAKEUP = MSDI_NO_INTms
        }

        #endregion

        #region "Status FLAGS"
        
        private const ushort MSDI_INTFLG = 0x40;
        private const ushort MSDI_THEMFLG = 0x80;
        
        #endregion

        #region "VARIABLES"

        private SPI.Configuration config = null; private SPI spi = null; 

        private InterruptPort spiInt = null;    //IRQ Pin settings

        public delegate void InputChangeEventHandler(object sender, SwitchsStatus inputs);

        public event InputChangeEventHandler InputsChange;

        private static Object SPI_Lock = new Object();

        public struct SwitchsStatus
        {
            public bool interrupt;
            public bool thermal;
            public ushort MSDI_SG;
            public byte MSDI_SP;
        }

        private SwitchsStatus _switchStatus;

        private ushort[] RegisterValue = new ushort[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pCS">CS pin definition</param>
        /// <param name="pINT">INT pin definition</param>
        /// <param name="spiModule">SPI module</param>
        public MC33972(Cpu.Pin pCS, Cpu.Pin pINT, SPI.SPI_module spiModule)
        {
            config = new SPI.Configuration(pCS, false, 5, 5, false, false, 10, spiModule);
            spi = new SPI(config);

            spiInt = new InterruptPort(pINT, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            spiInt.OnInterrupt += new NativeEventHandler(spi_OnInterrupt);
            
            initialize();
        }

        protected virtual void spi_OnInterrupt(UInt32 port, UInt32 state, DateTime when)
        {
            if (state == 0)
            {
                InputChangeEventHandler handler = InputsChange;

                _switchStatus = GetInputsStatus();

                if (handler != null)
                {
                    handler(this, _switchStatus);
                }
            }

            return;
        }

        private void initialize()
        {
            SendCommand(MSDI_RESET_CMD, MSDI_NONE);
            SendCommand(MSDI_SETTINGS_CMD, MSDI_NONE);
            SendCommand(MSDI_WAKEUPINTSP_CMD, MSDI_NONE);
            SendCommand(MSDI_WAKEUPINTSG_CMD, MSDI_NONE);
            SendCommand(MSDI_WETCURRENTSP_CMD, MSDI_SP_ALL);
            SendCommand(MSDI_WETCURRENTSG_CMD, MSDI_SG_ALL);
            SendCommand(MSDI_ANALOG_CMD, (MSDI_AN_NONE | (MSDI_AN_HiZ<<5)));
            SendCommand(MSDI_WETTINGTIMERSP_CMD, MSDI_SP_ALL);
            SendCommand(MSDI_WETTINGTIMERSG_CMD, MSDI_SG_ALL);
            SendCommand(MSDI_TRISTATESP_CMD, MSDI_NONE);
            SendCommand(MSDI_TRISTATESG_CMD, MSDI_NONE);
        }

        private void SendCommand(byte command, ushort data)
        {
            byte[] dataFrame = new byte[3];

            dataFrame[0] = command;
            dataFrame[1] = (byte)(data >> 8);
            dataFrame[2] = (byte)(data & 0xFF);

            if(command!=MSDI_RESET_CMD) RegisterValue[command] = data;
            
            spi.WriteRead(dataFrame, dataFrame);
        }

        /// <summary>
        /// Read input status from chip
        /// </summary>
        /// <returns>switch status bits</returns>
        public SwitchsStatus GetInputsStatus()
        {
            lock (SPI_Lock)
            {
                byte[] data = new byte[3]; UInt32 lRet = 0x00; SwitchsStatus sRet;

                data[0] = 0x00; //1st byte
                data[1] = 0x00; //2nd byte
                data[2] = 0x00; //3er byte

                spi.WriteRead(data, data);

                lRet = (UInt32)((data[0] << 16) + (data[1] << 8) + (data[2] << 0));
                
                sRet.MSDI_SG = (ushort)(lRet & 0x3FFF);
                sRet.MSDI_SP = (byte)((lRet>>14) & 0xFF);
               
                if ((lRet & MSDI_INTFLG) == MSDI_INTFLG) sRet.interrupt = true; else sRet.interrupt = false;
                if ((lRet & MSDI_INTFLG) == MSDI_INTFLG) sRet.thermal = true; else sRet.thermal = false;

                return sRet;
            }
            
        }

        /// <summary>
        /// Set MSDI in sleep mode and configures scan and interrupt timers
        /// </summary>
        /// <param name="TimersConfig">Scan and interrupt timer values</param>
        public void SleepMode_IntTimers(interruptPeriod interrupt_period, scanPeriod scan_period)
        {
            byte TimersConfig = (byte)((int)interrupt_period + (int)scan_period);
            SendCommand(MSDI_SLEEP_CMD, TimersConfig);
        }

        /// <summary>
        /// Configure MSDI SPx pin(s) as switch to battery or switch to ground
        /// </summary>
        /// <param name="spInput">SPx pin(s) (MSDI_SP0, MSDI_SP1, ...MSDI_SP7) OR combinations are valid</param>
        /// <param name="Connect">MSDI_SWtoBAT, MSDI_SWtoGND</param>
        public void Set_SPinputs(byte spInputs, BatteryGroundSelect Connect)
        {
            byte auxiliar = (byte)(RegisterValue[MSDI_SETTINGS_CMD]&0xFF);

            switch (Connect)
            {
                case BatteryGroundSelect.SWITCH_TO_BATTERY: 
                    { auxiliar |= spInputs; } break;
                
                case BatteryGroundSelect.SWITCH_TO_GROUND:
                    { auxiliar &= (byte)(~spInputs); } break;
            }

            SendCommand(MSDI_SETTINGS_CMD, auxiliar);
        }

        /// <summary>
        /// Select SPx or SGx inputs wetting current
        /// </summary>
        /// <param name="inputs_type"> sets SPx or SGx inputs</param>
        /// <param name="inputs">SPx pin(s) (MSDI_SP0, MSDI_SP1, ... MSDI_SP7) OR combinations are valid, 
        ///                      SGx pin(s) (MSDI_SG0, MSDI_SG1, ... MSDI_SG13) </param>
        /// <param name="isMetallic"> false = MSDI_2mA,  true = MSDI_16mA</param>
        public void Set_WettingCurrent(MSDI_INPUTS inputs_type, ushort inputs, bool isMetallic)
        {
            switch (inputs_type)
            {
                case MSDI_INPUTS.GROUND_SWITCHS: 
                {
                    if (isMetallic)
                        SendCommand(MSDI_WETCURRENTSG_CMD, (ushort)((RegisterValue[MSDI_WETCURRENTSG_CMD] | inputs) & 0x3FF));
                    else
                        SendCommand(MSDI_WETCURRENTSG_CMD, (ushort)((RegisterValue[MSDI_WETCURRENTSG_CMD] & (ushort)(~inputs)) & 0x3FF));
                } 
                break;
                
                case MSDI_INPUTS.PROGRAMMABLE_SWITCHS:
                {
                    if (isMetallic)
                        SendCommand(MSDI_WETCURRENTSP_CMD, (ushort)((RegisterValue[MSDI_WETCURRENTSP_CMD] | inputs) & 0xFF));
                    else
                        SendCommand(MSDI_WETCURRENTSP_CMD, (ushort)((RegisterValue[MSDI_WETCURRENTSP_CMD] & (ushort)(~inputs)) & 0xFF));
                }
                break;
            }
        }

        /// <summary>
        /// Enable/ disable the wetting timer for SPx or SGx inputs
        /// </summary>
        /// <param name="inputs_type"> sets SPx or SGx inputs</param>
        /// <param name="inputs">SPx pin(s) (MSDI_SP0, MSDI_SP1, ... MSDI_SP7) OR combinations are valid, 
        ///                      SGx pin(s) (MSDI_SG0, MSDI_SG1, ... MSDI_SG13) </param>
        /// <param name="enable"> true = ENABLE, false = DISABLE</param>
        public void Set_WettingTimer(MSDI_INPUTS inputs_type, byte inputs, bool enable)
        {
            switch (inputs_type)
            {
                case MSDI_INPUTS.GROUND_SWITCHS:
                    {
                        if (enable)
                            SendCommand(MSDI_WETTINGTIMERSG_CMD, (ushort)((RegisterValue[MSDI_WETTINGTIMERSG_CMD] | inputs) & 0x3FF));
                        else
                            SendCommand(MSDI_WETTINGTIMERSG_CMD, (ushort)((RegisterValue[MSDI_WETTINGTIMERSG_CMD] & (ushort)(~inputs)) & 0x3FF));
                    }
                    break;

                case MSDI_INPUTS.PROGRAMMABLE_SWITCHS:
                    {
                        if (enable)
                            SendCommand(MSDI_WETTINGTIMERSP_CMD, (ushort)((RegisterValue[MSDI_WETCURRENTSP_CMD] | inputs) & 0xFF));
                        else
                            SendCommand(MSDI_WETTINGTIMERSP_CMD, (ushort)((RegisterValue[MSDI_WETCURRENTSP_CMD] & (ushort)(~inputs)) & 0xFF));
                    }
                    break;
            }
        }

        /// <summary>
        /// Set SPx or SGx inputs in high impedance state
        /// </summary>
        /// <param name="inputs_type"> sets SPx or SGx inputs</param>
        /// <param name="inputs">SPx pin(s) (MSDI_SP0, MSDI_SP1, ... MSDI_SP7) OR combinations are valid, 
        ///                      SGx pin(s) (MSDI_SG0, MSDI_SG1, ... MSDI_SG13) </param>
        /// <param name="enable"> true = hiZ, false = normal</param>
        public void Set_HighImpedance(MSDI_INPUTS inputs_type, ushort inputs, bool enable)
        {
            switch (inputs_type)
            {
                case MSDI_INPUTS.GROUND_SWITCHS:
                    {
                        if (enable)
                            SendCommand(MSDI_TRISTATESG_CMD, (ushort)((RegisterValue[MSDI_TRISTATESG_CMD] | inputs) & 0x3FF));
                        else
                            SendCommand(MSDI_TRISTATESG_CMD, (ushort)((RegisterValue[MSDI_TRISTATESG_CMD] & (ushort)(~inputs)) & 0x3FF));
                    }
                    break;

                case MSDI_INPUTS.PROGRAMMABLE_SWITCHS:
                    {
                        if (enable)
                            SendCommand(MSDI_TRISTATESP_CMD, (ushort)((RegisterValue[MSDI_TRISTATESP_CMD] | inputs) & 0xFF));
                        else
                            SendCommand(MSDI_TRISTATESP_CMD, (ushort)((RegisterValue[MSDI_TRISTATESP_CMD] & (ushort)(~inputs)) & 0xFF));
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Select SPx or SGx inputs for wake-up interrupt 
        /// </summary>
        /// <param name="inputs_type"> sets SPx or SGx inputs</param>
        /// <param name="inputs">SPx pin(s) (MSDI_SP0, MSDI_SP1, ... MSDI_SP7) OR combinations are valid, 
        ///                      SGx pin(s) (MSDI_SG0, MSDI_SG1, ... MSDI_SG13) </param>
        /// <param name="enable"> true = wakeup/interrupt, false = disable interrupt</param>
        public void Select_WakeUp_Interrupt(MSDI_INPUTS inputs_type, ushort inputs, bool enable)
        {
            switch (inputs_type)
            {
                case MSDI_INPUTS.GROUND_SWITCHS:
                    {
                        if (enable)
                            SendCommand(MSDI_WAKEUPINTSP_CMD, (ushort)((RegisterValue[MSDI_WAKEUPINTSP_CMD] | inputs) & 0x3FF));
                        else
                            SendCommand(MSDI_WAKEUPINTSP_CMD, (ushort)((RegisterValue[MSDI_WAKEUPINTSP_CMD] & (ushort)(~inputs)) & 0x3FF));
                    }
                    break;

                case MSDI_INPUTS.PROGRAMMABLE_SWITCHS:
                    {
                        if (enable)
                            SendCommand(MSDI_WAKEUPINTSP_CMD, (ushort)((RegisterValue[MSDI_WAKEUPINTSP_CMD] | inputs) & 0xFF));
                        else
                            SendCommand(MSDI_WAKEUPINTSP_CMD, (ushort)((RegisterValue[MSDI_WAKEUPINTSP_CMD] & (ushort)(~inputs)) & 0xFF));
                    }
                    break;
            }
        }

        /// <summary>
        /// Select analog input channel and desired current or high impedance
        /// </summary>
        /// <param name="channel">Analog channel</param>
        /// <param name="currentSettings">MSDI_AN_HiImpedance, MSDI_AN_2mA, MSDI_AN_16mA</param>
        public void Set_Analog_Channel(AnalogChannel channel, AnalogCurrentSettings currentSettings)
        {
            SendCommand(MSDI_ANALOG_CMD, (ushort)((int)currentSettings | (int)channel));
        }
    
    }

}
