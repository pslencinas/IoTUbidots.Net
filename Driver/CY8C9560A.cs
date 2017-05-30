using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace DIGITAL_OUTPUTS
{
    class CY8C9560A
    {
        #region "CONSTANT's"

            //Puerto Nº0 -> Configuracion de los registros x default

            private const byte CY8C9560A_P0DIR                 = 0xFF;
            private const byte CY8C9560A_P0IRQMASK             = 0xFF;    
            private const byte CY8C9560A_P0_INVERSION          = 0x00;
            private const byte CY8C9560A_P0_PWMSEL             = 0x00;

            private const byte CY8C9560A_P0DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P0DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P0DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P0DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P0DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P0DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P0DM_HIGHZ            = 0x00;

            //Puerto Nº1 -> Configuracion de los registros x default

            private const byte CY8C9560A_P1DIR                 = 0xFF;
            private const byte CY8C9560A_P1IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P1_INVERSION          = 0x00;
            private const byte CY8C9560A_P1_PWMSEL             = 0x00;

            private const byte CY8C9560A_P1DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P1DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P1DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P1DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P1DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P1DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P1DM_HIGHZ            = 0x00;

            //Puerto Nº2 -> Configuracion de los registros x default

            private const byte IO_A1                           = 0x01;
            private const byte IO_WD                           = 0x02;
            private const byte IO_A2                           = 0x03;
            private const byte IO_A3                           = 0x04;

            private const byte CY8C9560A_P2DIR                 = 0xFF;
            private const byte CY8C9560A_P2IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P2_INVERSION          = 0x00;
            private const byte CY8C9560A_P2_PWMSEL             = 0x00;

            private const byte CY8C9560A_P2DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P2DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P2DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P2DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P2DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P2DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P2DM_HIGHZ            = 0x00;

            //Puerto Nº3 -> Configuracion de los registros x default

            private const byte CY8C9560A_P3DIR                 = 0xFF;
            private const byte CY8C9560A_P3IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P3_INVERSION          = 0x00;
            private const byte CY8C9560A_P3_PWMSEL             = 0x00;

            private const byte CY8C9560A_P3DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P3DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P3DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P3DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P3DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P3DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P3DM_HIGHZ            = 0x00;

            //Puerto Nº4 -> Configuracion de los registros x default

            private const byte CY8C9560A_P4DIR                 = 0xFF;
            private const byte CY8C9560A_P4IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P4_INVERSION          = 0x00;
            private const byte CY8C9560A_P4_PWMSEL             = 0x00;

            private const byte CY8C9560A_P4DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P4DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P4DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P4DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P4DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P4DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P4DM_HIGHZ            = 0x00;

            //Puerto Nº5 -> Configuracion de los registros x default

            private const byte CY8C9560A_P5DIR = 0xFF;
            private const byte CY8C9560A_P5IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P5_INVERSION          = 0x00;
            private const byte CY8C9560A_P5_PWMSEL             = 0x00;

            private const byte CY8C9560A_P5DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P5DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P5DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P5DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P5DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P5DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P5DM_HIGHZ            = 0x00;

            //Puerto Nº6 -> Configuracion de los registros x default

            private const byte CY8C9560A_P6DIR                 = 0xFF;
            private const byte CY8C9560A_P6IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P6_INVERSION          = 0x00;
            private const byte CY8C9560A_P6_PWMSEL             = 0x00;

            private const byte CY8C9560A_P6DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P6DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P6DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P6DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P6DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P6DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P6DM_HIGHZ            = 0x00;

            //Puerto Nº7 -> Configuracion de los registros x default

            private const byte CY8C9560A_P7DIR                 = 0xFF;
            private const byte CY8C9560A_P7IRQMASK             = 0xFF;   
            private const byte CY8C9560A_P7_INVERSION          = 0x00;
            private const byte CY8C9560A_P7_PWMSEL             = 0x00;

            private const byte CY8C9560A_P7DM_PULLUP           = 0x00;
            private const byte CY8C9560A_P7DM_PULLDOWN         = 0xFF;
            private const byte CY8C9560A_P7DM_OPEN_DRAIN_HIGH  = 0x00;
            private const byte CY8C9560A_P7DM_OPEN_DRAIN_LOW   = 0x00;
            private const byte CY8C9560A_P7DM_STRONG_FAST      = 0x00;
            private const byte CY8C9560A_P7DM_STRONG_LOW       = 0x00;
            private const byte CY8C9560A_P7DM_HIGHZ            = 0x00;
        
            //--------------------------------------------------------
            // Device Registers Addresses
            //--------------------------------------------------------

            private const byte CY8C9560A_INPUT_P0              = 0x00;    //Input Port 0
            private const byte CY8C9560A_INPUT_P1              = 0x01;    //Input Port 1
            private const byte CY8C9560A_INPUT_P2              = 0x02;    //Input Port 2
            private const byte CY8C9560A_INPUT_P3              = 0x03;    //Input Port 3
            private const byte CY8C9560A_INPUT_P4              = 0x04;    //Input Port 4
            private const byte CY8C9560A_INPUT_P5              = 0x05;    //Input Port 5
            private const byte CY8C9560A_INPUT_P6              = 0x06;    //Input Port 6
            private const byte CY8C9560A_INPUT_P7              = 0x07;    //Input Port 7

            private const byte CY8C9560A_OUTPUT_P0             = 0x08;    //Output Port 0
            private const byte CY8C9560A_OUTPUT_P1             = 0x09;    //Output Port 1
            private const byte CY8C9560A_OUTPUT_P2             = 0x0A;    //Output Port 2
            private const byte CY8C9560A_OUTPUT_P3             = 0x0B;    //Output Port 3
            private const byte CY8C9560A_OUTPUT_P4             = 0x0C;    //Output Port 4
            private const byte CY8C9560A_OUTPUT_P5             = 0x0D;    //Output Port 5
            private const byte CY8C9560A_OUTPUT_P6             = 0x0E;    //Output Port 6
            private const byte CY8C9560A_OUTPUT_P7             = 0x0F;    //Output Port 7

            private const byte CY8C9560A_IRQ_STATUS_P0         = 0x10;    //Interrupt Status Port 0
            private const byte CY8C9560A_IRQ_STATUS_P1         = 0x11;    //Interrupt Status Port 1
            private const byte CY8C9560A_IRQ_STATUS_P2         = 0x12;    //Interrupt Status Port 2
            private const byte CY8C9560A_IRQ_STATUS_P3         = 0x13;    //Interrupt Status Port 3
            private const byte CY8C9560A_IRQ_STATUS_P4         = 0x14;    //Interrupt Status Port 4
            private const byte CY8C9560A_IRQ_STATUS_P5         = 0x15;    //Interrupt Status Port 5
            private const byte CY8C9560A_IRQ_STATUS_P6         = 0x16;    //Interrupt Status Port 6
            private const byte CY8C9560A_IRQ_STATUS_P7         = 0x17;    //Interrupt Status Port 7

            private const byte CY8C9560A_PORT_SELECT           = 0x18;    //Port Select

            private const byte CY8C9560A_IRQ_MASK              = 0x19;    //Interrupt Mask
            private const byte CY8C9560A_PWM_PORT_OUTPUT       = 0x1A;    //Select PWM for Port output
            private const byte CY8C9560A_INVERSION             = 0x1B;    //Inversion
            private const byte CY8C9560A_PIN_DIRECTION         = 0x1C;    //Pin Direction Input/Output
            private const byte CY8C9560A_MODE_PULLUP           = 0x1D;    //Drive Mode Pull-UP
            private const byte CY8C9560A_MODE_PULLDOWN         = 0x1E;    //Drive Mode Pull-DOWN
            private const byte CY8C9560A_MODE_OPEN_DRAIN_HIGH  = 0x1F;    //Drive Mode OpenDrain HIGH
            private const byte CY8C9560A_MODE_OPEN_DRAIN_LOW   = 0x20;    //Drive Mode OpenDrain LOW
            private const byte CY8C9560A_MODE_STRONG           = 0x21;    //Drive Mode Strong
            private const byte CY8C9560A_MODE_SLOW_STRONG      = 0x22;    //Drive Mode Slow Strong
            private const byte CY8C9560A_MODE_HIGHZ            = 0x23;    //Drive Mode High Z

            //Reserved 0x24, 0x25, 0x26, 0x27

            private const byte CY8C9560A_PWM_SELECT            = 0x28;    //PWM Select
            private const byte CY8C9560A_CONFIG_PWM            = 0x29;    //Config PWM
            private const byte CY8C9560A_PERIOD_PWM            = 0x2A;    //Period PWM
            private const byte CY8C9560A_PULSE_WIDTH_PWM       = 0x2B;    //Pulse Width PWM
            private const byte CY8C9560A_PROGRAMMABLE_DIVIDER  = 0x2C;    //Programmable Divider
            private const byte CY8C9560A_ENABLE_WDE_EEE_EERO   = 0x2D;    //Enable WDE, EEE, EERO
            private const byte CY8C9560A_DEVICEID_STATUS       = 0x2E;    //Device ID / Status
            private const byte CY8C9560A_WATCHDOG              = 0x2F;    //WatchDog
            private const byte CY8C9560A_COMMAND               = 0x30;    //Command

            public enum Registers : byte
            {
                PORT0_INPUT = CY8C9560A_INPUT_P0,
                PORT1_INPUT = CY8C9560A_INPUT_P1,
                PORT2_INPUT = CY8C9560A_INPUT_P2,
                PORT3_INPUT = CY8C9560A_INPUT_P3,
                PORT4_INPUT = CY8C9560A_INPUT_P4,
                PORT5_INPUT = CY8C9560A_INPUT_P5,
                PORT6_INPUT = CY8C9560A_INPUT_P6,
                PORT7_INPUT = CY8C9560A_INPUT_P7,
                PORT0_OUTPUT = CY8C9560A_OUTPUT_P0,
                PORT1_OUTPUT = CY8C9560A_OUTPUT_P1,
                PORT2_OUTPUT = CY8C9560A_OUTPUT_P2,
                PORT3_OUTPUT = CY8C9560A_OUTPUT_P3,
                PORT4_OUTPUT = CY8C9560A_OUTPUT_P4,
                PORT5_OUTPUT = CY8C9560A_OUTPUT_P5,
                PORT6_OUTPUT = CY8C9560A_OUTPUT_P6,
                PORT7_OUTPUT = CY8C9560A_OUTPUT_P7,
                PORT0_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P0,
                PORT1_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P1,
                PORT2_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P2,
                PORT3_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P3,
                PORT4_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P4,
                PORT5_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P5,
                PORT6_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P6,
                PORT7_IRQ_STATUS = CY8C9560A_IRQ_STATUS_P7,
                PORT_SELECT = CY8C9560A_PORT_SELECT,
                IRQ_MASK = CY8C9560A_IRQ_MASK,
                PWM_PORT_OUTPUT = CY8C9560A_PWM_PORT_OUTPUT,
                INVERSION = CY8C9560A_INVERSION,
                PIN_DIRECTION = CY8C9560A_PIN_DIRECTION,
                MODE_PULLUP = CY8C9560A_MODE_PULLUP,
                MODE_PULLDOWN = CY8C9560A_MODE_PULLDOWN,
                MODE_OPEN_DRAIN_HIGH = CY8C9560A_MODE_OPEN_DRAIN_HIGH,
                MODE_OPEN_DRAIN_LOW = CY8C9560A_MODE_OPEN_DRAIN_LOW,
                MODE_STRONG = CY8C9560A_MODE_STRONG,
                MODE_SLOW_STRONG = CY8C9560A_MODE_SLOW_STRONG,
                MODE_HIGHZ = CY8C9560A_MODE_HIGHZ,
                PWM_SELECT = CY8C9560A_PWM_SELECT,
                CONFIG_PWM = CY8C9560A_CONFIG_PWM,
                PERIOD_PWM = CY8C9560A_PERIOD_PWM,
                PULSE_WIDTH_PWM = CY8C9560A_PULSE_WIDTH_PWM,
                PROGRAMMABLE_DIVIDER = CY8C9560A_PROGRAMMABLE_DIVIDER,
                ENABLE_WDE_EEE_EERO = CY8C9560A_ENABLE_WDE_EEE_EERO,
                DEVICEID_STATUS = CY8C9560A_DEVICEID_STATUS,
                WATCHDOG = CY8C9560A_WATCHDOG,
                COMMAND = CY8C9560A_COMMAND
            }

        #endregion
        
        #region "MODEL AND PORTS"

            public enum Model : byte
            {
                UNKNOWN = 0x00,
                CY8C9560A = 0x60,
                CY8C9540A = 0x40,
                CY8C9520A = 0x20
            }

            private Model DeviceModel = Model.UNKNOWN;

            private const byte CY8C9560A_PORT0 = 0x00;
            private const byte CY8C9560A_PORT1 = 0x01;
            private const byte CY8C9560A_PORT2 = 0x02;
            private const byte CY8C9560A_PORT3 = 0x03;
            private const byte CY8C9560A_PORT4 = 0x04;
            private const byte CY8C9560A_PORT5 = 0x05;
            private const byte CY8C9560A_PORT6 = 0x06;
            private const byte CY8C9560A_PORT7 = 0x07;

            public enum Ports : byte
            {
                PORT0 = CY8C9560A_PORT0,
                PORT1 = CY8C9560A_PORT1,
                PORT2 = CY8C9560A_PORT2,
                PORT3 = CY8C9560A_PORT3,
                PORT4 = CY8C9560A_PORT4,
                PORT5 = CY8C9560A_PORT5,
                PORT6 = CY8C9560A_PORT6,
                PORT7 = CY8C9560A_PORT7
            }
        #endregion

        private const byte CY8C9560A_PORT_ADDRESS   = 0x20;
        private const byte CY8C9560A_MEMORY_ADDRESS = 0xA0;

        private const byte CY8C9520A_PORT_COUNT     = 0x03;                         //Cantidad de Puertos del Expander CY8C9520A=3
        private const byte CY8C9540A_PORT_COUNT     = 0x05;                         //Cantidad de Puertos del Expander CY8C9540A=8
        private const byte CY8C9560A_PORT_COUNT     = 0x08;                         //Cantidad de Puertos del Expander CY8C9560A=8

        private const ushort DeviceAddress          = CY8C9560A_PORT_ADDRESS;       // From the documentation (A0 to GND = 0x20)
        private const ushort DeviceClockRate        = 50;                           // Clock Frequency [kHz]
        private const ushort DeviceDefaultTimeout   = 1000;                         // Not sure what the units are
        private const ushort DeviceInitMaxRetry     = 5;                            // Cantidad de reintentos
        
        private ushort DevicePortCount = CY8C9560A_PORT_COUNT;         // Not sure what the units are
        
        //public struct PortConfig
        //{
        //    public byte Interrupt_Mask;
        //    public byte PWM_PortOutput;
        //    public byte Inversion;
        //    public byte Pin_Direction;
        //    public byte DriveMode_PullUp;
        //    public byte DriveMode_PullDown;
        //    public byte DriveMode_OpenDrain_High;
        //    public byte DriveMode_OpenDrain_Low;
        //    public byte DriveMode_Strong_Fast;
        //    public byte DriveMode_Strong_Slow;
        //    public byte DriveMode_HighZ;
        //}

        #region I2C Variables

            /// <summary>
            /// By design, .NETMF only allows one object per I2C bus device. This one must be share with all the 
            /// devices on the physical i2c bus. Multiple devices can use the one bus, by supplying Configurations 
            /// for each device and then changing the configuration used when talking to a device.
            /// </summary>

            static private I2CDevice i2cBus = null;

            static private I2CDevice.Configuration i2cConfig = null;

        #endregion

        private readonly Object _lock = new Object();

        //IRQ & XRST Pin settings

        private static OutputPort i2cRst = null;        //Cpu.Pin pRST = Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_D_3;
        
        private static InterruptPort i2cInt = null;     //Cpu.Pin pINT = Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_A_1;

        public delegate void InputChangeEventHandler(object sender, InterruptEventArgs interrupt);
        
        public event InputChangeEventHandler InputsChange;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRST"></param>
        /// <param name="pINT"></param>

        public CY8C9560A(Cpu.Pin pRST, Cpu.Pin pINT)
        {
            i2cInt = new InterruptPort(pINT, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLevelHigh);
            i2cInt.OnInterrupt += new NativeEventHandler(i2c_OnInterrupt);
            
            i2cRst = new OutputPort(pRST, false);

            Initialize();
        }

        public void Initialize()
        {
            int retry = DeviceInitMaxRetry;

            i2cConfig = new I2CDevice.Configuration(DeviceAddress, DeviceClockRate);
            i2cBus = new I2CDevice(i2cConfig);

            while (retry!=0)
            {
                i2cRst.Write(true); System.Threading.Thread.Sleep(10);
                i2cRst.Write(false); System.Threading.Thread.Sleep(100);

                Debug.Print("I2C device INIT complete #" + retry.ToString());

                switch (ReadRegister(Registers.DEVICEID_STATUS) & 0xF0)
                {
                    case (byte)Model.CY8C9520A:
                        {
                            DeviceModel = Model.CY8C9520A;
                            DevicePortCount = CY8C9520A_PORT_COUNT;
                            Debug.Print("CY8C9520A Found...");
                        } break;

                    case (byte)Model.CY8C9540A:
                        {
                            DeviceModel = Model.CY8C9540A;
                            DevicePortCount = CY8C9540A_PORT_COUNT;
                            Debug.Print("CY8C9540A Found...");
                        } break;

                    case (byte)Model.CY8C9560A:
                        {
                            DeviceModel = Model.CY8C9560A;
                            DevicePortCount = CY8C9560A_PORT_COUNT;
                            Debug.Print("CY8C9560A Found...");
                        } break;

                    default: { DeviceModel = Model.UNKNOWN; DevicePortCount = 0; } break;
                }

                //if device detected clear all interrupt flags

                if (DeviceModel != Model.UNKNOWN)
                {
                    for (byte devicePort = 0; devicePort < DevicePortCount; devicePort++)
                    {
                        var intMask = ReadRegister((byte)((byte)Registers.PORT0_IRQ_STATUS + devicePort));

                        retry = 0;
                    }
                }
                else
                    retry--;

            }
        }

        protected virtual void i2c_OnInterrupt(UInt32 port, UInt32 state, DateTime when)
        {
            if (state == 1)
            {
                InputChangeEventHandler handler = InputsChange;

                if (handler != null)
                {
                    // Loop through the enabled ports and find which pin(s) threw the event.
                    for (byte devicePort = 0; devicePort < DevicePortCount; devicePort ++)
                    {
                        // Get the interrupt status of all pins on the port.
                        var intMask = ReadRegister((byte)((byte)Registers.PORT0_IRQ_STATUS + devicePort));
                        if (intMask == 0) continue;         // This port didn't trigger the event.  Move on...

                        // Raise events for each of the pins that triggered the interrupt.
                        for (byte pin = 0; pin < 8; pin++)
                        {
                            if ((intMask & (1 << pin)) > 0)
                            {
                                handler(this, new InterruptEventArgs(devicePort, pin, DateTime.Now));
                            }
                        }
                    }
                }
            }

            return;
        }

        #region "REGISTER READ/WIRTE METHODS"
        
        /// <summary>
        /// Write data to the CY8C9560A chip
        /// </summary>
        /// <param name="register">the CY8C95x0A register to be written to</param>
        /// <param name="value">the data to be written into the register</param>
        /// <returns>number of bytes sent</returns>

        private int WriteRegister(Registers register, byte value)
        {
            int bytesSent = -1;

            lock (_lock)
            {
                byte[] mybuff = new byte[2] { (byte)register, value };

                I2CDevice.I2CTransaction writeXAction = I2CDevice.CreateWriteTransaction(mybuff);

                I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { writeXAction };

                bytesSent = i2cBus.Execute(actions, DeviceDefaultTimeout);
            }

            return bytesSent;
        }

        /// <summary>
        /// Write data to the CY8C9560A chip
        /// </summary>
        /// <param name="register">the CY8C95x0A register to be written to</param>
        /// <param name="value">the data to be written into the register</param>
        /// <returns>number of bytes sent</returns>

        private int WriteRegister(byte registerAddress, byte registerValue)
        {
            int bytesSent = -1;

            lock (_lock)
            {
                byte[] mybuff = new byte[2] { registerAddress, registerValue };

                I2CDevice.I2CTransaction writeXAction = I2CDevice.CreateWriteTransaction(mybuff);

                I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { writeXAction };

                bytesSent = i2cBus.Execute(actions, DeviceDefaultTimeout);
            }

            return bytesSent;
        }
        
        /// <summary>
        /// Return the value of the requested register address. 
        /// </summary>
        /// <param name="address">The address of the CY8C95x0A register to read</param>
        /// <returns>The value read. Note that 0 could mean either 0 bytes or failure</returns>

        private byte ReadRegister(Registers register)
        {
            byte[] mybuff = new byte[1];

            lock (_lock)
            {
                I2CDevice.I2CTransaction sendAddress = I2CDevice.CreateWriteTransaction(new byte[] { (byte)register });

                I2CDevice.I2CTransaction readData = I2CDevice.CreateReadTransaction(mybuff);

                I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { sendAddress, readData };

                if (i2cBus.Execute(actions, DeviceDefaultTimeout) == 0)
                {
                    Debug.Print("Failed to perform I2C transaction");
                }
            }
            
            return mybuff[0];
        }

        /// <summary>
        /// Return the value of the requested register address. 
        /// </summary>
        /// <param name="address">The address of the CY8C95x0A register to read</param>
        /// <returns>The value read. Note that 0 could mean either 0 bytes or failure</returns>

        private byte ReadRegister(byte registerAddress)
        {
            byte[] mybuff = new byte[1];

            lock (_lock)
            {
                I2CDevice.I2CTransaction sendAddress = I2CDevice.CreateWriteTransaction(new byte[] { registerAddress });

                I2CDevice.I2CTransaction readData = I2CDevice.CreateReadTransaction(mybuff);

                I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { sendAddress, readData };

                if (i2cBus.Execute(actions, DeviceDefaultTimeout) == 0)
                {
                    Debug.Print("Failed to perform I2C transaction");
                }
            }
            return mybuff[0];
        }
        
        #endregion

        /// <summary>
        /// Gets the port number from a given pin ID.
        /// </summary>
        /// <param name="pinId">An IOPin.</param>
        /// <returns>Port number of the given pin.</returns>
        internal static byte GetPortNumber(IOPin pinId)
        {
            return (byte)((byte)(pinId) >> 4);
        }

        /// <summary>
        /// Gets the pin number from a given pin ID.
        /// </summary>
        /// <param name="pin">An IOPin.</param>
        /// <returns>Pin number of the given pin.</returns>
        internal static byte GetPinNumber(IOPin pin)
        {
            return (byte)((byte)(pin) & 0x0f);
        }

        /// <summary>
        /// Reads the value of a port.
        /// </summary>
        /// <param name="port">The port to read.</param>
        /// <returns>The value of the port.</returns>
        public byte Read(Ports port)
        {
            return ReadRegister((byte)((int)Registers.PORT0_INPUT+(int)port));
        }

        private byte Read(byte port)
        {
            return ReadRegister((byte)(Registers.PORT0_INPUT + port));
        }

        /// <summary>
        /// Reads the value of a pin.
        /// </summary>
        /// <param name="pin">The pin to read.</param>
        /// <returns>High (true) or low (false) state of the pin.</returns>
        public bool Read(IOPin pin)
        {
            var portVal = Read(GetPortNumber(pin));
            var pinNumber = GetPinNumber(pin);
            return (portVal & (1 << pinNumber)) != 0;
        }

        /// <summary>
        /// Writes a value to the specified port.
        /// </summary>
        /// <param name="port">Port to write to.</param>
        /// <param name="value">Value to write to the port.</param>
        public void Write(Ports port, byte value)
        {
            WriteRegister((byte)((int)Registers.PORT0_OUTPUT + (int)port), value);
        }

        private void Write(byte port, byte value)
        {
            WriteRegister((byte)((int)Registers.PORT0_OUTPUT + port), value);
        }

        /// <summary>
        /// Writes a value to the specified pin.
        /// </summary>
        /// <param name="pin">Pin to write to.</param>
        /// <param name="state">State value to write to the pin.</param>
        public void Write(IOPin pin, bool state)
        {
            Write(GetPortNumber(pin), GetPinNumber(pin), state);
        }

        /// <summary>
        /// Write a value to a single pin of the specified port.
        /// </summary>
        /// <param name="port">Port to write to.</param>
        /// <param name="pin">Pin to write to.</param>
        /// <param name="state">Value to write to the pin.</param>
        private void Write(byte port, byte pin, bool state)
        {
            lock (_lock)
            {
                var b = Read(port); // Read port

                if (state)  // Config pin
                {
                    b |= (byte)(1 << (pin));
                }
                else
                {
                    b &= (byte)~(1 << (pin));
                }
                
                Write(port, b); // Apply
            }
        }

        /// <summary>
        /// Set the pins direction of the selected port
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="outputMask"></param>
        public void SetDirection(Ports port, byte outputMask)
        {
            lock (_lock)
            {
                WriteRegister(Registers.PORT_SELECT, (byte)port);               // Select port
                WriteRegister(Registers.PIN_DIRECTION, (byte)(~outputMask));    // select pins direction for the port.                    
            }
        }

        /// <summary>
        /// Sets the direction of a single pin.
        /// </summary>
        /// <param name="pin">the pin to set the direction</param>
        /// <param name="direction">sets input or output</param>
        public void SetDirection(IOPin pin, PinDirection direction)
        {
            lock (_lock)
            {
                var port = GetPortNumber(pin);
                var pinNum = GetPinNumber(pin);

                // Get current direction values for all pins on the port.
                WriteRegister(Registers.PORT_SELECT, port);
                var d = ReadRegister(Registers.PIN_DIRECTION);

                // Update just the direction of our pin.
                if (direction == PinDirection.Input)
                {
                    d |= (byte)(1 << (pinNum));
                }
                else
                {
                    d &= (byte)~(1 << (pinNum));
                }
                WriteRegister(Registers.PIN_DIRECTION, d);
            }
        }

         /// <summary>
        /// Gets a mask of the pins on a port that have interrupts enabled.
        /// </summary>
        /// <param name="port">The port to inspect.</param>
        /// <returns>Pin mask indicating those that have interrupts enabled.</returns>
        public byte GetInterruptsEnabled(Ports port)
        {
            lock (_lock)
            {
                WriteRegister(Registers.PORT_SELECT, (byte)port); // Select port
                return ReadRegister(Registers.IRQ_MASK);
            }
        }

        /// <summary>
        /// Enables interrupts for all pins on a port according to a mask.
        /// </summary>
        /// <param name="port">The port to update.</param>
        /// <param name="enableMask">Mask that specifies the port enable setting for each pin.  1 = enabled, 0 = disabled.</param>
        public void SetInterruptEnable(Ports port, byte enableMask)
        {
            lock (_lock)
            {
                WriteRegister(Registers.PORT_SELECT, (byte)port);    // Select port
                WriteRegister(Registers.IRQ_MASK, (byte)~enableMask);       // Enable interrupt for select pins on the port.                    
            }
        }

        /// <summary>
        /// Inverts the input logic of an input port.
        /// </summary>
        /// <param name="port">The port to update.</param>
        /// <param name="enableMask">Mask specifying the pins to enable inversion.</param>
        public void EnableInputLogicInversion(Ports port, byte enableMask)
        {
            lock (_lock)
            {
                WriteRegister(Registers.PORT_SELECT, (byte)port);   // Select port
                WriteRegister(Registers.INVERSION, enableMask);     // Set the intersion flags.
            }
        }

        /// <summary>
        /// Sets all the pins on a port to the same resistor mode.
        /// </summary>
        /// <param name="port">The port number to set.</param>
        /// <param name="resistorMode">The resistor mode assign to the port.</param>
        /// <param name="pinMask">The pins to assign to the specified resistor mode. </param>
        public void SetResistorMode(Ports port, ResistorMode resistorMode, byte pinMask = 0xff)
        {
            lock (_lock)
            {
                WriteRegister(Registers.PORT_SELECT, (byte)port);
                WriteRegister((byte)resistorMode, pinMask);
            }
        }

        /// <summary>
        /// Sets all the pins on a port to the same resistor mode.
        /// </summary>
        /// <param name="port">The port number to set.</param>
        /// <param name="pin">The pin to set.</param>
        /// <param name="resistorMode">The resistor mode assign to the port.</param>
        public void SetResistorMode(byte port, byte pin, ResistorMode resistorMode)
        {
            lock (_lock)
            {
                WriteRegister(Registers.PORT_SELECT, port);
                var b = ReadRegister((byte)resistorMode); // Read the current values for the resistor mode.
                b |= (byte)(1 << (pin)); // Config pin
                WriteRegister((byte)resistorMode, b); // Apply
            }
        }

        /// <summary>
        /// Sets the resistor mode on a pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="resistorMode">The resistor mode to assign to the pin.</param>
        public void SetResistorMode(IOPin pin, ResistorMode resistorMode)
        {
            SetResistorMode(GetPortNumber(pin), GetPinNumber(pin), resistorMode);
        }

        /// <summary>
        /// Identifies a pin's IO direction.
        /// </summary>
        public enum PinDirection : byte
        {
            Output = 0,
            Input = 1
        }

        /// <summary>
        /// IO pin drive modes.
        /// </summary>
        public enum ResistorMode : byte
        {
            /// <summary>
            /// This mode is the opposite of the Pull-down mode. In this mode, HIGH output is driven strong and LOW output 
            /// is through an internal pull-down resistor of approximately 5.6K. 
            /// This mode may be used as an input, for example with a switch connected to GND. 
            /// Once the pull-up resistor is enabled, the state of the pin is read using the input register. 
            /// This mode may also be used as output.
            /// </summary>
            ResistivePullUp = 0x1D,
            /// <summary>
            /// In this mode, HIGH output is driven strong, and LOW output is through an internal pull-down resistor of approximately 5.6K. 
            /// This mode may be used as an input, for example with a switch connected to 3.3V. This mode may also be used as output.
            /// </summary>
            ResistivePullDown = 0x1E,
            /// <summary>
            /// In this mode, the HIGH output is driven with a slow strong drive. The LOW output is high impedence.
            /// </summary>
            OpenDrainHigh = 0x1F,
            /// <summary>
            /// In this mode, the LOW output is driven with a slow strong drive and HIGH output is high impedence.
            /// This mode is suitable for I2C bus where external pull-up resistors are used.
            /// </summary>
            OpenDrainLow = 0x20,
            /// <summary>
            /// Strong High, Strong Low, FastOutput Mode.
            /// Use the Strong mode if your pin is an output driving a load. 
            /// The pin has a low impedance connection to GND and 3.3V when driven high and low. 
            /// Do not use the Strong mode if the pin is an input.
            /// </summary>
            StrongDrive = 0x21,
            /// <summary>
            /// Strong High, Strong Low, Slow Output Mode.
            /// This mode is similar to the Strong mode, but the slope of the output is slightly controlled 
            /// so that high harmonics are not present when the output switches.
            /// </summary>
            SlowStrongDrive = 0x22,
            /// <summary>
            /// High Z mode.
            /// This mode is normally used when the pin will be driven high and low externally of the module.
            /// </summary>
            HighImpedence = 0x23
        }

        public enum PwmPin : byte
        {
            Pwm0 = 0x60,
            Pwm1,
            Pwm2,
            Pwm3,
            Pwm4,
            Pwm5,
            Pwm6,
            Pwm7,
            Pwm8 = 0x70,
            Pwm9,
            Pwm10,
            Pwm11,
            Pwm12,
            Pwm13,
            Pwm14,
            Pwm15
        }

        public enum IOPin : byte
        {
            Port0_Pin0 = 0x00,
            Port0_Pin1,
            Port0_Pin2,
            Port0_Pin3,
            Port0_Pin4,
            Port0_Pin5,
            Port0_Pin6,
            Port0_Pin7,
            Port1_Pin0 = 0x10,
            Port1_Pin1,
            Port1_Pin2,
            Port1_Pin3,
            Port1_Pin4,
            Port1_Pin5,
            Port1_Pin6,
            Port1_Pin7,
            Port2_Pin0 = 0x20,
            Port2_Pin1,
            Port2_Pin2,
            Port2_Pin3,
            Port3_Pin0 = 0x30,
            Port3_Pin1,
            Port3_Pin2,
            Port3_Pin3,
            Port3_Pin4,
            Port3_Pin5,
            Port3_Pin6,
            Port3_Pin7,
            Port4_Pin0 = 0x40,
            Port4_Pin1,
            Port4_Pin2,
            Port4_Pin3,
            Port4_Pin4,
            Port4_Pin5,
            Port4_Pin6,
            Port4_Pin7,
            Port5_Pin0 = 0x50,
            Port5_Pin1,
            Port5_Pin2,
            Port5_Pin3,
            Port5_Pin4,
            Port5_Pin5,
            Port5_Pin6,
            Port5_Pin7,
            Port6_Pwm0 = 0x60,
            Port6_Pwm1,
            Port6_Pwm2,
            Port6_Pwm3,
            Port6_Pwm4,
            Port6_Pwm5,
            Port6_Pwm6,
            Port6_Pwm7,
            Port7_Pwm8 = 0x70,
            Port7_Pwm9,
            Port7_Pwm10,
            Port7_Pwm11,
            Port7_Pwm12,
            Port7_Pwm13,
            Port7_Pwm14,
            Port7_Pwm15
        }
    }

    /// <summary>
    /// Event arguments that are passed during an Interrupt event.
    /// </summary>
    public class InterruptEventArgs : EventArgs
    {
        public InterruptEventArgs(byte port, byte pin, DateTime timestamp)
        {
            Port = port;
            Pin = pin;
            Timestamp = timestamp;
        }

        /// <summary>
        /// The port where the interrupt occurred.
        /// </summary>
        public byte Port { get; private set; }

        /// <summary>
        /// The pin that triggered the event.
        /// </summary>
        public byte Pin { get; private set; }

        /// <summary>
        /// The date/time that the interrupt occurred.
        /// </summary>
        public DateTime Timestamp { get; private set; }
    }
}
