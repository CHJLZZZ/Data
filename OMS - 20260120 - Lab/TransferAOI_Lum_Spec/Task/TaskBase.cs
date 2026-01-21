using CommonBase.Logger;
using BaseTool;
using HardwareManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FrameGrabber;

namespace OpticalMeasuringSystem
{
    public class TaskBase
    {
        public Thread MThread;

        public int FlowStep = 0;
        public bool FlowRun = false;

        public EnumMotoControlPackage motoControlPackage = GlobalVar.DeviceType;
        public InfoManager Info;
        public MilDigitizer Grabber;
        public IMotorControl Motor;

        public TaskBase(MilDigitizer grabber, IMotorControl motorControl, InfoManager info)
        {
            this.Info = info;
            this.Grabber = grabber;
            this.Motor = motorControl;
        }

        public virtual void Start(object Para)
        {
            if (!FlowRun)
            {
                MThread = new Thread(() => MainFlow(Para));
                MThread.Start();
            }
            else
            {
                FlowRun = false;
            }
        }

        public virtual void MainFlow(object InputPara)
        {
        }

   
    }
}
