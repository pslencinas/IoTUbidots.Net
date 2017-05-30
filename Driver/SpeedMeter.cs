using System;
using System.IO.Ports;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.STM32F4;

using GHI.Processor;

namespace SPEEDMETER
{
    class SpeedMeter
    {
        private const UInt32 PERIPH_BASE = 0x40000000;
        private const UInt32 APB1PERIPH_BASE = PERIPH_BASE;
        private const UInt32 AHB1PERIPH_BASE = PERIPH_BASE + 0x00020000;

        private const UInt32 RCC_BASE = AHB1PERIPH_BASE + 0x3800;
        
        private const UInt32 TIM3_BASE = APB1PERIPH_BASE + 0x0400;
        private const UInt32 GPIOC_BASE = AHB1PERIPH_BASE + 0x0800;

        private const UInt32 TIM12_BASE = APB1PERIPH_BASE + 0x1800;
        private const UInt32 GPIOB_BASE = AHB1PERIPH_BASE + 0x0400;

        private static bool bTaskControl = false;

        private struct speedMeterValues
        {
            public uint Calibration;
            public uint InterruptPulseCounter;
            public float InterruptDistance;
            public float TotalDistance;
            public float PartialDistance;
            public float SpeedKMH;
        }

        static private speedMeterValues speedMeter;

        public SpeedMeter(uint calibration = 50)
        {
            speedMeter.Calibration = calibration;
            speedMeter.TotalDistance = 0;
            speedMeter.PartialDistance = 0;
            speedMeter.SpeedKMH = 0;

            //<<<----- Entrada de Interrupcion donde se conecta el velocimetro ----->>>
            //InterruptPort speed = new InterruptPort((Cpu.Pin)Microsoft.SPOT.Hardware.STM32F4.Pins.GPIO_PIN_B_14, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            //speed.OnInterrupt += new NativeEventHandler(speed_OnInterrupt);
        }
        
        /// <summary>
        /// Handler de Interrupcion del velocimetro (NO FUNCIONA !!!)
        /// </summary>
        static protected void speed_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (state == 1)
            {
                speedMeter.InterruptPulseCounter++;
            }

            return;
        }
        
        /// <summary>
        /// Start the meassuring thread
        /// </summary>
        public void StartMeter()
        {
            bTaskControl = true;

            System.Threading.Thread tPCNTR = new System.Threading.Thread(thread_T3_PULSECOUNTER);

            tPCNTR.Start();
        }

        /// <summary>
        /// Stop the meassuring thread
        /// </summary>
        public void StopMeter()
        {
            bTaskControl = false;
        }

        static private void thread_T3_PULSECOUNTER()
        {
            uint CCR2; uint last_count = 0;
            
            Register RCC_APB1ENR = new Register(RCC_BASE + 0x40);
            Register RCC_AHB1ENR = new Register(RCC_BASE + 0x30);

            Register GPIOC_MODER = new Register(GPIOC_BASE + 0x00);
            Register GPIOC_AFRL = new Register(GPIOC_BASE + 0x20);

            Register TIM3_CR1 = new Register(TIM3_BASE + 0x00);     //TIM control register 1
            Register TIM3_SMCR = new Register(TIM3_BASE + 0x08);    //TIM slave mode control register
            Register TIM3_CCMR1 = new Register(TIM3_BASE + 0x18);   //TIM capture/compare mode register 1
            Register TIM3_CCER = new Register(TIM3_BASE + 0x20);    //TIM capture/compare enable register
            Register TIM3_CCR2 = new Register(TIM3_BASE + 0x38);    //TIM capture/compare register 2

            RCC_APB1ENR.SetBits(0x01 << 1);                         //TIM3 clock enabled
            RCC_AHB1ENR.SetBits(0x01 << 2);                         //GPIOC enabled

            GPIOC_MODER.ClearBits(0x03 << 14);
            GPIOC_MODER.SetBits(0x02 << 14);                        //PC7 as Alternate function mode

            GPIOC_AFRL.ClearBits(0xFF000000);
            GPIOC_AFRL.SetBits(0x20000000);                         //PC7 Alternative function 2

            TIM3_CCMR1.SetBits(1 << 8);                             //CC2 channel is configured as input, IC2 is mapped on TI2

            TIM3_CCER.ClearBits(0xF0);
            TIM3_CCER.SetBits((1 << 4));                            //CC2E(4)=1,CC2NP(5)=0,CC2P(7)=0 CAPTURE RISING' EDGE 0x01xxxx

            TIM3_SMCR.SetBits(0x07 << 0);                           //SMS=111 External Clock Mode 1 - Rising edges of the selected trigger (TRGI) clock the counter
            TIM3_SMCR.SetBits(0x06 << 4);                           //TS=110 Filtered Timer Input 2 (TI2FP2)

            TIM3_CCR2.Value = 0;                                    //Clear CCR
            
            TIM3_CR1.SetBits(0x01);                                 //Counter enabled

            last_count = TIM3_CCR2.Value;

            while (bTaskControl)
            {
                System.Threading.Thread.Sleep(1000);

                CCR2 = TIM3_CCR2.Value;

                speedMeter.InterruptPulseCounter = CCR2 - last_count;

                last_count = CCR2;

                speedMeter.InterruptDistance = ((float)(speedMeter.InterruptPulseCounter) / speedMeter.Calibration);
                speedMeter.InterruptDistance *= 100;

                speedMeter.SpeedKMH = speedMeter.InterruptDistance * 3.6f;

                speedMeter.PartialDistance += speedMeter.InterruptDistance;
                speedMeter.TotalDistance += speedMeter.InterruptDistance;

                //Debug.Print(speedMeter.PartialDistance.ToString());
            }

            TIM3_CR1.ClearBits(0x01);   //Counter disabled
        }

        //static private void thread_T12_PULSECOUNTER()
        //{
        //    uint CCR1 = 0; uint CCR2;

        //    Register RCC_APB1ENR = new Register(RCC_BASE + 0x40);
        //    Register RCC_AHB1ENR = new Register(RCC_BASE + 0x30);

        //    Register GPIOB_MODER = new Register(GPIOB_BASE + 0x00);
        //    Register GPIOB_AFRH = new Register(GPIOB_BASE + 0x24);

        //    Register TIM12_CR1 = new Register(TIM12_BASE + 0x00);   //TIM control register 1
        //    Register TIM12_SMCR = new Register(TIM12_BASE + 0x08);  //TIM slave mode control register
        //    Register TIM12_CCMR1 = new Register(TIM12_BASE + 0x18); //TIM capture/compare mode register 1
        //    Register TIM12_CCER = new Register(TIM12_BASE + 0x20);  //TIM capture/compare enable register
        //    Register TIM12_CNT = new Register(TIM12_BASE + 0x24);   //TIM Counter
        //    Register TIM12_CCR1 = new Register(TIM12_BASE + 0x34);  //TIM capture/compare register 1
        //    Register TIM12_CCR2 = new Register(TIM12_BASE + 0x38);  //TIM capture/compare register 2

        //    RCC_APB1ENR.SetBits(0x40);                              //TIM12 clock enabled (BIT 6)
        //    RCC_AHB1ENR.SetBits(0x02);                              //GPIOB enabled

        //    GPIOB_MODER.ClearBits(0xC0000000);
        //    GPIOB_MODER.SetBits(0x80000000);                        //PB15 as Alternate function mode

        //    GPIOB_AFRH.ClearBits(0xF0000000);
        //    GPIOB_AFRH.SetBits(0x90000000);                         //PB15 Alternative function 9

        //    TIM12_CCMR1.Value = 0x02;                               //CC1 channel as input, IC1 is mapped on TI2

        //    TIM12_SMCR.Value = 0x67;                                //SMS=111 External Clock Mode 1 - Rising edges of the selected trigger (TRGI) clock the counter
        //    //TS=110 Filtered Timer Input 2 (TI2FP2)

        //    TIM12_CCER.Value = 0x03;                                //CC2E=1 / CC1E=1  BOTH CAPTURE FALLIN' EDGE

        //    TIM12_CCR1.Value = 0;                                   //Clear CCR
        //    TIM12_CCR2.Value = 0;                                   //Clear CCR
        //    TIM12_CNT.Value = 0;                                    //Clear Counter

        //    TIM12_CR1.Value = 0x01;                                   //Counter enabled

        //    while (true)
        //    {
        //        Thread.Sleep(1000);

        //        CCR1 = TIM12_CCR1.Value;

        //        CCR2 = TIM12_CCR2.Value;

        //        Debug.Print(CCR2.ToString());
        //    }
        //}

        /// <summary>
        /// returns the meassured speed in [KM/H]
        /// </summary>
        public float SpeedKMH
        {
            get { return speedMeter.SpeedKMH; }
        }

        /// <summary>
        /// returns the meassured distance TOTAL in [KM/H]
        /// </summary>
        public float TotalDistance
        {
            get { return speedMeter.TotalDistance; }
        }

        /// <summary>
        /// returns the meassured distance PARCIAL in [KM/H]
        /// </summary>
        public float PartialDistance
        {
            get { return speedMeter.PartialDistance; }
        }

        public void resetPartialDistance()
        {
            speedMeter.PartialDistance = 0;
        }

        public uint Calibration
        {
            get { return speedMeter.Calibration; }
            set { speedMeter.Calibration = value; }
        }
    }
}
