using System;
using System.IO.Ports;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

namespace FLOWMETER
{
    class FlowMeter
    {
        private static uint InterruptPulseCounter = 0;
        
        private struct flowMeterValues
        {
            public uint Calibration;
            public float Total;
            public float Parcial;
            public float Liters;
        }

        private static DateTime hTime; private static float frequency = 0;

        static private flowMeterValues fwMeter;

        private static bool bTaskControl = false;

        private const byte BUFFER_SIZE = 10; private static float[] buffer = new float[BUFFER_SIZE];

        private const uint DELAY_THREAD = 50;

        /// <summary>
        /// FlowMeter constructor
        /// </summary>
        /// <param name="calibration">calibration constant pulse/litre</param>
        public FlowMeter(uint calibration = 10)
        {
            fwMeter.Calibration = calibration;
        }
        
        /// <summary>
        /// Handler de Interrupcion del caudalimetro
        /// </summary>
        static void flow_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (state == 1)
            {
                frequency = 1000 * (float)TimeSpan.TicksPerMillisecond / time.Subtract(hTime).Ticks; hTime = time; 
                
                //InterruptPulseCounter++;
            }

            return;
        }

        /// <summary>
        /// Start the meassuring thread
        /// </summary>
        public void StartMeter()
        {
            bTaskControl = true;

            System.Threading.Thread tFLOWMETER = new System.Threading.Thread(thread_FLOWMETER);

            tFLOWMETER.Start();
        }

        /// <summary>
        /// Stop the meassuring thread
        /// </summary>
        public void StopMeter()
        {
            bTaskControl = false;
        }

        static private void thread_FLOWMETER()
        {
            byte index = 0; float Average=0;

            //<<<----- Entrada de Interrupcion donde se conecta el caudalimetro ----->>>
            InterruptPort flow = new InterruptPort((Cpu.Pin)Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_B_15, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            flow.OnInterrupt += new NativeEventHandler(flow_OnInterrupt);

            while (bTaskControl)
            {

                System.Threading.Thread.Sleep(50);

                InterruptPulseCounter = 0;

                buffer[index] = frequency; index++;

                if (index >= BUFFER_SIZE)
                {
                    for (index = 0; index < BUFFER_SIZE; index++)
                        Average += buffer[index];

                    Average /= index;

                    index = 0;

                    fwMeter.Liters= ((float)Average / fwMeter.Calibration);
                    fwMeter.Parcial += fwMeter.Liters;
                    fwMeter.Total += fwMeter.Liters;
                }
                
            }
        }

        /// <summary>
        /// returns the meassured flow in [LITERS/MIN]
        /// </summary>
        public float Liters
        {
            get { return fwMeter.Liters; }
        }

        public float Partial
        {
            get { return fwMeter.Parcial; }
        }

        public float Total
        {
            get { return fwMeter.Total; }
        }

        public uint Calibration
        {
            get { return fwMeter.Calibration; }
            set { fwMeter.Calibration = value; }
        }

        public void ResetPartial()
        {
            fwMeter.Parcial = 0;
        }
    }
}
