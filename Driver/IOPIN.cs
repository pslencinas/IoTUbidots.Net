using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using DIGITAL_OUTPUTS;
using DIGITAL_INPUTS;

namespace GPIO_CONTROL
{
    class IOPIN
    {
        private static CY8C9560A IO60P = null;
        
        public static MC33972 inputs = null;
                   
        public IOPIN()
        {
            IO60P = new CY8C9560A(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_D_3, 
                                  Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_A_1);

            inputs = new MC33972(Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_D_4, 
                                 Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_A_0,
                                 SPI.SPI_module.SPI1);
        }

        public class OUTPUTS
        {
            private static UInt32 digitalOutput = 0x00; 
       
            public OUTPUTS()
            {
                //OUTPUT1 to OUTPUT8
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT0, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT0, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT0, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT0, 0x00);

                //OUTPUT9 to OUTPUT16
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT1, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT1, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT1, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT1, 0x00);

                //OUTPUT17 to OUTPUT24
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT3, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT3, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT3, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT3, 0x00);

                //OUTPUT25 to OUTPUT32
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT4, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT4, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT4, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT4, 0x00);

                //Valve Advanced Control
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT2, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT2, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT2, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT2, 0x00);

                //VALVE1 to VALVE8
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT6, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT6, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT6, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT6, 0x00);

                //VALVE9 to VALVE16
                IO60P.SetInterruptEnable(CY8C9560A.Ports.PORT7, 0x00);
                IO60P.SetDirection(CY8C9560A.Ports.PORT7, 0xFF);
                IO60P.SetResistorMode(CY8C9560A.Ports.PORT7, CY8C9560A.ResistorMode.SlowStrongDrive);
                IO60P.Write(CY8C9560A.Ports.PORT7, 0x00);
            }

            public void Write(UInt32 value)
            {
                digitalOutput = value;

                IO60P.Write(CY8C9560A.Ports.PORT0, (byte)(digitalOutput & 0xFF));
                IO60P.Write(CY8C9560A.Ports.PORT1, (byte)((digitalOutput >> 8) & 0xFF));
                IO60P.Write(CY8C9560A.Ports.PORT3, (byte)((digitalOutput >> 16) & 0xFF));
                IO60P.Write(CY8C9560A.Ports.PORT4, (byte)((digitalOutput >> 24) & 0xFF));
            }

            public UInt32 Read()
            {
                digitalOutput = IO60P.Read(CY8C9560A.Ports.PORT0);
                digitalOutput += (UInt32)(IO60P.Read(CY8C9560A.Ports.PORT1) << 8);
                digitalOutput += (UInt32)(IO60P.Read(CY8C9560A.Ports.PORT3) << 16);
                digitalOutput += (UInt32)(IO60P.Read(CY8C9560A.Ports.PORT4) << 24);

                return digitalOutput;
            }
        }

        public class VALVES
        {
            private static UInt32 valvesOutput = 0x00;

            public void Write(UInt32 value)
            {
                valvesOutput = value;

                IO60P.Write(CY8C9560A.Ports.PORT6, (byte)(valvesOutput & 0xFF));
                IO60P.Write(CY8C9560A.Ports.PORT7, (byte)((valvesOutput >> 8) & 0xFF));
                IO60P.Write(CY8C9560A.Ports.PORT2, (byte)((valvesOutput >> 16) & 0x0F));
            }

            public UInt32 Read()
            {
                valvesOutput = IO60P.Read(CY8C9560A.Ports.PORT6);
                valvesOutput += (UInt32)(IO60P.Read(CY8C9560A.Ports.PORT7) << 8);
                valvesOutput += (UInt32)(IO60P.Read(CY8C9560A.Ports.PORT2) << 16);
                
                return valvesOutput;
            }
        }

        public class JOYSTICK
        {
            private const CY8C9560A.Ports JOYSTICK_PORT = CY8C9560A.Ports.PORT5;

            private static byte joystickStatus = 0x00;

            public JOYSTICK(bool useIRQ = false)
            {
                if (useIRQ == true) IO60P.InputsChange += new CY8C9560A.InputChangeEventHandler(CY8C9560A_InputsChange);

                //Joystick INPUT Port
                IO60P.SetDirection(JOYSTICK_PORT, 0x00);
                IO60P.SetResistorMode(JOYSTICK_PORT, CY8C9560A.ResistorMode.ResistivePullUp);
                IO60P.SetInterruptEnable(JOYSTICK_PORT, 0x00);
            }

            static void CY8C9560A_InputsChange(object sender, InterruptEventArgs interrupt)
            {
                Debug.Print(string.Concat("CY8C9560A IRQ PORT", interrupt.Port.ToString(), " PIN ", interrupt.Pin.ToString()));
            }

            public byte Read()
            {
                joystickStatus = (byte)~(IO60P.Read(CY8C9560A.Ports.PORT5));

                return joystickStatus;
            }
        }

        public class INPUTS
        {
            private static MC33972.SwitchsStatus digitalInputs;
            
            public INPUTS(bool useIRQ = false)
            {
                Debug.Print("Dentro de IOPIN INPUTS");

                if(useIRQ == true) inputs.InputsChange += new MC33972.InputChangeEventHandler(MC33972_InputsChange);

                inputs.Set_SPinputs(MC33972.MSDI_SP_ALL, MC33972.BatteryGroundSelect.SWITCH_TO_BATTERY);
                inputs.Select_WakeUp_Interrupt(MC33972.MSDI_INPUTS.PROGRAMMABLE_SWITCHS, MC33972.MSDI_SP7, true);
            }

            static void MC33972_InputsChange(object sender, MC33972.SwitchsStatus inputs)
            {
                Debug.Print("MC33972 SG = 0x" + inputs.MSDI_SG.ToString("X"));
                Debug.Print("MC33972 SP = 0x" + inputs.MSDI_SP.ToString("X"));
            }

            public UInt32 Read()
            {
                digitalInputs = inputs.GetInputsStatus();

                return (UInt32)((digitalInputs.MSDI_SG<<8)+(digitalInputs.MSDI_SP));
            }
        }

        

    }
}
