/// This is a driver class for the Sparkfun MPR121 Capacitive Touch Keypad
/// SKU: SEN-10250 http://www.sparkfun.com/products/10250
/// SparkFun provided an Arduino example which I have translated to .NetMF
/// The original version may be downloaded from the following link if it is 
/// still active. 
/// http://www.sparkfun.com/datasheets/Sensors/Capacitive/MPR121_Keypad_Example.zip

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace TOUCHKEYPAD
{
    public class MPR121
    {
        #region MPR121_Defines

            // MPR121 Register Defines
            private const Byte ELE0_ELE7_TOUCH_STATUS = 0x00;
            private const Byte ELE8_ELE11_ELEPROX_TOUCH_STATUS = 0x01;
            private const Byte ELE0_7_OOR_STATUS = 0x02;
            private const Byte ELE8_11_ELEPROX_OOR_STATUS = 0x03;
            private const Byte ELE0_FILTERED_DATA_LSB = 0x04;
            private const Byte ELE0_FILTERED_DATA_MSB = 0x05;
            private const Byte ELE1_FILTERED_DATA_LSB = 0x06;
            private const Byte ELE1_FILTERED_DATA_MSB = 0x07;
            private const Byte ELE2_FILTERED_DATA_LSB = 0x08;
            private const Byte ELE2_FILTERED_DATA_MSB = 0x09;
            private const Byte ELE3_FILTERED_DATA_LSB = 0x0A;
            private const Byte ELE3_FILTERED_DATA_MSB = 0x0B;
            private const Byte ELE4_FILTERED_DATA_LSB = 0x0C;
            private const Byte ELE4_FILTERED_DATA_MSB = 0x0D;
            private const Byte ELE5_FILTERED_DATA_LSB = 0x0E;
            private const Byte ELE5_FILTERED_DATA_MSB = 0x0F;
            private const Byte ELE6_FILTERED_DATA_LSB = 0x10;
            private const Byte ELE6_FILTERED_DATA_MSB = 0x11;
            private const Byte ELE7_FILTERED_DATA_LSB = 0x12;
            private const Byte ELE7_FILTERED_DATA_MSB = 0x13;
            private const Byte ELE8_FILTERED_DATA_LSB = 0x14;
            private const Byte ELE8_FILTERED_DATA_MSB = 0x15;
            private const Byte ELE9_FILTERED_DATA_LSB = 0x16;
            private const Byte ELE9_FILTERED_DATA_MSB = 0x17;
            private const Byte ELE10_FILTERED_DATA_LSB = 0x18;
            private const Byte ELE10_FILTERED_DATA_MSB = 0x19;
            private const Byte ELE11_FILTERED_DATA_LSB = 0x1A;
            private const Byte ELE11_FILTERED_DATA_MSB = 0x1B;
            private const Byte ELEPROX_FILTERED_DATA_LSB = 0x1C;
            private const Byte ELEPROX_FILTERED_DATA_MSB = 0x1D;
            
            private const Byte ELE0_BASELINE_VALUE = 0x1E;
            private const Byte ELE1_BASELINE_VALUE = 0x1F;
            private const Byte ELE2_BASELINE_VALUE = 0x20;
            private const Byte ELE3_BASELINE_VALUE = 0x21;
            private const Byte ELE4_BASELINE_VALUE = 0x22;
            private const Byte ELE5_BASELINE_VALUE = 0x23;
            private const Byte ELE6_BASELINE_VALUE = 0x24;
            private const Byte ELE7_BASELINE_VALUE = 0x25;
            private const Byte ELE8_BASELINE_VALUE = 0x26;
            private const Byte ELE9_BASELINE_VALUE = 0x27;
            private const Byte ELE10_BASELINE_VALUE = 0x28;
            private const Byte ELE11_BASELINE_VALUE = 0x29;
            private const Byte ELEPROX_BASELINE_VALUE = 0x2A;

            private const Byte MHD_RISING = 0x2B;
            private const Byte NHD_AMOUNT_RISING = 0x2C;
            private const Byte NCL_RISING = 0x2D;
            private const Byte FDL_RISING = 0x2E;
            private const Byte MHD_FALLING = 0x2F;
            private const Byte NHD_AMOUNT_FALLING = 0x30;
            private const Byte NCL_FALLING = 0x31;
            private const Byte FDL_FALLING = 0x32;
            private const Byte NHD_AMOUNT_TOUCHED = 0x33;
            private const Byte NCL_TOUCHED = 0x34;
            private const Byte FDL_TOUCHED = 0x35;
            private const Byte ELEPROX_MHD_RISING = 0x36;
            private const Byte ELEPROX_NHD_AMOUNT_RISING = 0x37;
            private const Byte ELEPROX_NCL_RISING = 0x38;
            private const Byte ELEPROX_FDL_RISING  = 0x39;
            private const Byte ELEPROX_MHD_FALLING  = 0x3A;
            private const Byte ELEPROX_NHD_AMOUNT_FALLING = 0x3B;
            private const Byte ELEPROX_FDL_FALLING = 0x3C;
            private const Byte ELEPROX_NHD_AMOUNT_TOUCHED = 0x3E;
            private const Byte ELEPROX_NCL_TOUCHED = 0x3F;
            private const Byte ELEPROX_FDL_TOUCHED = 0x40;

            private const Byte ELE0_TOUCH_THRESHOLD = 0x41;
            private const Byte ELE0_RELEASE_THRESHOLD = 0x42;
            private const Byte ELE1_TOUCH_THRESHOLD = 0x43;
            private const Byte ELE1_RELEASE_THRESHOLD = 0x44;
            private const Byte ELE2_TOUCH_THRESHOLD = 0x45;
            private const Byte ELE2_RELEASE_THRESHOLD = 0x46;
            private const Byte ELE3_TOUCH_THRESHOLD = 0x47;
            private const Byte ELE3_RELEASE_THRESHOLD = 0x48;
            private const Byte ELE4_TOUCH_THRESHOLD = 0x49;
            private const Byte ELE4_RELEASE_THRESHOLD = 0x4A;
            private const Byte ELE5_TOUCH_THRESHOLD = 0x4B;
            private const Byte ELE5_RELEASE_THRESHOLD = 0x4C;
            private const Byte ELE6_TOUCH_THRESHOLD = 0x4D;
            private const Byte ELE6_RELEASE_THRESHOLD = 0x4E;
            private const Byte ELE7_TOUCH_THRESHOLD = 0x4F;
            private const Byte ELE7_RELEASE_THRESHOLD = 0x50;
            private const Byte ELE8_TOUCH_THRESHOLD = 0x51;
            private const Byte ELE8_RELEASE_THRESHOLD = 0x52;
            private const Byte ELE9_TOUCH_THRESHOLD = 0x53;
            private const Byte ELE9_RELEASE_THRESHOLD = 0x54;
            private const Byte ELE10_TOUCH_THRESHOLD = 0x55;
            private const Byte ELE10_RELEASE_THRESHOLD = 0x56;
            private const Byte ELE11_TOUCH_THRESHOLD = 0x57;
            private const Byte ELE11_RELEASE_THRESHOLD = 0x58;
            private const Byte ELEPROX_TOUCH_THRESHOLD = 0x59;
            private const Byte ELEPROX_RELEASE_THRESHOLD = 0x5A;
            private const Byte DEBOUNCE_TOUCH_AND_RELEASE = 0x5B;

            private const Byte AFE_CONFIGURATION = 0x5C;
            private const Byte AFE_CONFIGURATION2 = 0x5D;

            private const Byte ELECTRODE_CONFIG = 0x5E;
            private const Byte ELE0_CURRENT = 0x5F;
            private const Byte ELE1_CURRENT = 0x60;
            private const Byte ELE2_CURRENT = 0x61;
            private const Byte ELE3_CURRENT = 0x62;
            private const Byte ELE4_CURRENT = 0x63;
            private const Byte ELE5_CURRENT = 0x64;
            private const Byte ELE6_CURRENT = 0x65;
            private const Byte ELE7_CURRENT = 0x66;
            private const Byte ELE8_CURRENT = 0x67;
            private const Byte ELE9_CURRENT = 0x68;
            private const Byte ELE10_CURRENT = 0x69;
            private const Byte ELE11_CURRENT = 0x6A;
            private const Byte ELEPROX_CURRENT = 0x6B;

            private const Byte ELE0_ELE1_CHARGE_TIME = 0x6C;
            private const Byte ELE2_ELE3_CHARGE_TIME = 0x6D;
            private const Byte ELE4_ELE5_CHARGE_TIME = 0x6E;
            private const Byte ELE6_ELE7_CHARGE_TIME = 0x6F;
            private const Byte ELE8_ELE9_CHARGE_TIME = 0x70;
            private const Byte ELE10_ELE11_CHARGE_TIME = 0x71;
            private const Byte ELEPROX_CHARGE_TIME = 0x72;

            private const Byte GPIO_CONTROL_0 = 0x73;
            private const Byte GPIO_CONTROL_1 = 0x74;
            private const Byte GPIO_DATA = 0x75;
            private const Byte GPIO_DIRECTION = 0x76;
            private const Byte GPIO_ENABLE = 0x77;
            private const Byte GPIO_SET = 0x78;
            private const Byte GPIO_CLEAR = 0x79;
            private const Byte GPIO_TOGGLE = 0x7A;
            private const Byte AUTO_CONFIG_CONTROL_0 = 0x7B;
            private const Byte AUTO_CONFIG_CONTROL_1 = 0x7C;
            private const Byte AUTO_CONFIG_USL = 0x7D;
            private const Byte AUTO_CONFIG_LSL = 0x7E;
            private const Byte AUTO_CONFIG_TARGET_LEVEL = 0x7F;
            
            // Global Constants

            private const Byte TOUCH_THRESHOLD = 35;
            private const Byte RELEASE_THRESHOLD = 30;

        #endregion

        public const int NO_EVENT = -1;

        private const ushort MPR121Address = 0x5A;          // From the documentation (ADDR to GND = 0x5A)

        private const int MPR121ClockRate = 100;            // Freq [kHz]

        private const int defaultTimeout = 1000;            // Not sure what the units are

        #region I2C Variables
    
            /// <summary>
            /// By design, .NETMF only allows one object per I2C bus device. This one must be share with all the 
            /// devices on the physical i2c bus. Multiple devices can use the one bus, by supplying Configurations 
            /// for each device and then changing the configuration used when talking to a device.
            /// </summary>
            
            static private I2CDevice i2cBus;
            
        #endregion

        public delegate void KeyPressEventHandler(object sender, int key);
        
        public event KeyPressEventHandler KeyPressed;

        private static System.Threading.AutoResetEvent autoEventI2C;

        /// <summary>
        /// The constructor
        /// </summary>
        public MPR121()
        {
            System.Threading.Thread tMPR121 = new System.Threading.Thread(thread_MPR121);
            
            tMPR121.Start();
        }

        void thread_MPR121()
        {
            autoEventI2C = new System.Threading.AutoResetEvent(false);

            i2cBus = new I2CDevice(new I2CDevice.Configuration(MPR121Address, MPR121ClockRate));

            init_device();  //Init MPR121 Device

            System.Threading.Thread.Sleep(25);

            //IRQ Pin settings

            InterruptPort keyInt = new InterruptPort(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_A_1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);

            keyInt.OnInterrupt += new NativeEventHandler(keyboard_OnInterrupt);

            Debug.Print("MPR121 initialized...");

            while (true)
            {
                Debug.Print("MPR121 wating for IRQ...");

                autoEventI2C.WaitOne();

                int key_pressed = getKey();

                KeyPressEventHandler handler = KeyPressed;

                if (handler != null)
                {
                    handler(this, key_pressed);
                }
            }
        }

        /// <summary>
        /// Write data to the MPR-121, 
        /// </summary>
        /// <param name="data">the first byte of the data must be the MPR-121 register to be written to.</param>
        /// <returns>number of bytes sent</returns>

        private int writeTo(byte[] data)
        {
            int bytesSent;

            I2CDevice.I2CTransaction writeXAction = I2CDevice.CreateWriteTransaction(data);

            I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { writeXAction };

            bytesSent = i2cBus.Execute(actions, defaultTimeout);

            return bytesSent;
        }

        /// <summary>
        /// Return the byte at the requested address. 
        /// </summary>
        /// <param name="address">The address of the MPR-121 register to read</param>
        /// <returns>The value read. Note that 0 could mean either 0 bytes or failure</returns>

        public byte readByte(byte address)
        {
            byte[] mybuff = new byte[1];

            I2CDevice.I2CTransaction sendAddress = I2CDevice.CreateWriteTransaction(new byte[] { address });

            I2CDevice.I2CTransaction readData = I2CDevice.CreateReadTransaction(mybuff);

            I2CDevice.I2CTransaction[] actions = new I2CDevice.I2CTransaction[] { sendAddress, readData };

            i2cBus.Execute(actions, defaultTimeout);

            return mybuff[0];
        }

        /// <summary>
        /// Return the currently pressed key if there is one. Else return -1.
        /// </summary>
        /// <returns> -1  there is no discernable key touched
        ///            0-11 the key that has been touched.
        ///            The number corresponds to the number printed on the board.
        /// </returns>

        private int getKey()
        {
            int key = NO_EVENT;

            int touchstatus = (readByte(ELE8_ELE11_ELEPROX_TOUCH_STATUS) << 8) + (readByte(ELE0_ELE7_TOUCH_STATUS) << 0);

            switch(touchstatus)
			{
                case 0x0001: key = 0; break;
                case 0x0002: key = 1; break;
                case 0x0004: key = 2; break;
                case 0x0008: key = 3; break;
                case 0x0010: key = 4; break;
                case 0x0020: key = 5; break;
                case 0x0040: key = 6; break;
                case 0x0080: key = 7; break;
                case 0x0100: key = 8; break;
                case 0x0200: key = 9; break;
                case 0x0400: key = 10; break;
                case 0x0800: key = 11; break;
                default: break;
			}
            
            return key;
        }

        /// <summary>
        /// Configure the IC on the keyboard translated from the code provided by Sparkfun.
        /// </summary>
        
        private void init_device()
        {
            int bytesSent = 0;

            bytesSent = writeTo(new Byte[] { 0x80, 0x63 });     // Software Reset

            bytesSent = writeTo(new Byte[] { ELECTRODE_CONFIG, 0x00 });  // Standby Mode

            // Section A --> This group controls filtering when data is > baseline.

            bytesSent = writeTo(new Byte[] { MHD_RISING, 0x01 });
            bytesSent = writeTo(new Byte[] { NHD_AMOUNT_RISING, 0x01 });
            bytesSent = writeTo(new Byte[] { NCL_RISING, 0x00 });
            bytesSent = writeTo(new Byte[] { FDL_RISING, 0x00 });

            // Section B --> This group controls filtering when data is < baseline.

            bytesSent = writeTo(new Byte[] { MHD_FALLING, 0x01 });
            bytesSent = writeTo(new Byte[] { NHD_AMOUNT_FALLING, 0x01 });
            bytesSent = writeTo(new Byte[] { NCL_FALLING, 0xFF });
            bytesSent = writeTo(new Byte[] { FDL_FALLING, 0x02 });

            // Section C --> This group sets touch and release thresholds for each electrode

            bytesSent = writeTo(new Byte[] { ELE0_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE0_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE1_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE1_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE2_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE2_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE3_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE3_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE4_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE4_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE5_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE5_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE6_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE6_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE7_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE7_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE8_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE8_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE9_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE9_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE10_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE10_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE11_TOUCH_THRESHOLD, TOUCH_THRESHOLD });
            bytesSent = writeTo(new Byte[] { ELE11_RELEASE_THRESHOLD, RELEASE_THRESHOLD });
            
            // Section D --> Set the Filter Configuration (Set ESI2)

            bytesSent = writeTo(new Byte[] { AFE_CONFIGURATION, 0x10 });
            bytesSent = writeTo(new Byte[] { AFE_CONFIGURATION2, 0x24 });

            // Section E --> Electrode Configuration (Enable ALL (12) Electrodes and set to run mode

            bytesSent = writeTo(new Byte[] { ELECTRODE_CONFIG, 0x0C });    // Enables all 12 Electrodes

        }

        /// <summary>
        /// mpr121 IQR handler
        /// </summary>
        /// <param name="port"></param>
        /// <param name="state"></param>
        /// <param name="when"></param>

        protected virtual void keyboard_OnInterrupt(uint port, uint state, DateTime when)
        {
            if (state == 0)
            {
                autoEventI2C.Set();
            }
            
            return;
        }

    }
}