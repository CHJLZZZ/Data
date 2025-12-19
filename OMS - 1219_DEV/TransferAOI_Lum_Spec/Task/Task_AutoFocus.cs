using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using HardwareManager;
using BaseTool;

using Matrox.MatroxImagingLibrary;
using ClosedXML.Excel;
using FrameGrabber;
using System.ComponentModel;

namespace OpticalMeasuringSystem
{

    public class Task_AutoFocus
    {
        public event Action<int, double,  bool> UpdateFocusValue; //Pos1, Value1, Pos2, Value2, Finish

        private EnumMotoControlPackage MotorType = GlobalVar.DeviceType;

        private InfoManager info;
        private MilDigitizer grabber;
        private IMotorControl motorControl;

        public bool FlowRun = false;
        public int AutoFocusPos = 0;
        public double AutoFocusValue = 0;
        public int AutoFocusPos_2 = 0;
        public double AutoFocusValue_2 = 0;
        public string VCurve_List = "";

        private FlowStep NowStep = FlowStep.GetFocusValue;
        public FlowStep Step
        {
            get => NowStep;
        }

        private FlowResult Flow_Result = new FlowResult();
        public FlowResult Result
        {
            get => Flow_Result;
        }

        public Task_AutoFocus(MilDigitizer grabber, IMotorControl motorControl, InfoManager info)
        {
            this.info = info;
            this.grabber = grabber;
            this.motorControl = motorControl;
        }

        public void Start(AutoFocus_FlowPara Para)
        {
            if (!FlowRun)
            {
                FlowRun = true;
                Thread mThread = new Thread(() => Flow(Para));
                mThread.Start();
            }
            else
            {
                FlowRun = false;
                Start(Para);
            }
        }

        public void Stop()
        {
            FlowRun = false;
        }

        private void Flow(AutoFocus_FlowPara Para)
        {
            //AiryMember device = AiryMember.Focuser;
            MIL_ID milZone = MIL.M_NULL;
            MIL_INT SizeX = 0;
            MIL_INT SizeY = 0;

            int Upper_Limit = Para.Upper_Limit;
            int Lower_Limit = Para.Lower_Limit;
            int Min_Step = Para.Min_Step;
            int Dir = Para.MoveDir;
            bool UseROI = Para.UseROI;
            Rectangle ROI = Para.ROI;

            //Jog
            double FocusValue = 0;
            double FocusValue_1 = 0;
            double FocusValue_2 = 0;
            double FocusValue_3 = 0;

            int MoveStart = 0;
            int MoveStep = 100;
            int Pos = 0;
            int Pos_1 = 0;
            int Pos_2 = 0;
            int Pos_3 = 0;
            int PartCount = 10;

            Dictionary<int, double> FocusInfo = new Dictionary<int, double>();
            TimeManager TM = new TimeManager();
            TimeManager TM_TimeOut = new TimeManager();
            TM_TimeOut.SetDelay(PartCount * 90000);

            // Initial
            this.NowStep = FlowStep.GetOriFocusValue;

            try
            {
                do
                {
                    if (TM_TimeOut.IsTimeOut())
                    {
                        this.NowStep = FlowStep.TimeOut;
                    }

                    switch (this.NowStep)
                    {
                        case FlowStep.GetOriFocusValue:
                            {
                                // Initial
                                FocusInfo.Clear();

                                if (Upper_Limit == -1)
                                {
                                    if (this.motorControl.Focus_LimitF() > this.motorControl.Focus_LimitR())
                                    {
                                        Pos_2 = this.motorControl.Focus_LimitF();
                                        Pos_1 = this.motorControl.Focus_LimitR();
                                    }
                                    else
                                    {
                                        Pos_2 = this.motorControl.Focus_LimitR();
                                        Pos_1 = this.motorControl.Focus_LimitF();
                                    }
                                }
                                else
                                {
                                    if (Upper_Limit > Lower_Limit)
                                    {
                                        Pos_2 = Upper_Limit;
                                        Pos_1 = Lower_Limit;
                                    }
                                    else
                                    {
                                        Pos_2 = Lower_Limit;
                                        Pos_1 = Upper_Limit;
                                    }
                                }

                                // Get ROI
                                SizeX = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_X, MIL.M_NULL);
                                SizeY = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_Y, MIL.M_NULL);

                                MIL.MbufChild2d(this.grabber.grabImage,
                                    Convert.ToInt32(0.1 * SizeX), Convert.ToInt32(0.1 * SizeY),
                                    Convert.ToInt32(0.8 * SizeX), Convert.ToInt32(0.8 * SizeY),
                                    ref milZone);

                                // Move To Postion 
                                switch (MotorType)
                                {
                                    case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                    case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                    case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                    case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                    case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    case EnumMotoControlPackage.MS5515M:
                                    case EnumMotoControlPackage.CircleTac:
                                        {
                                            int TargetPos = (Dir > 0) ? Pos_1 : Pos_2;
                                            this.motorControl.Move(TargetPos, -1, -1, -1);
                                        }
                                        break;
                                }

                                this.NowStep = FlowStep.CheckDirection_CheckMove;  // ReCheck
                                TM.SetDelay(100);
                            }
                            break;

                        case FlowStep.CheckDirection_CheckMove:
                            if (TM.IsTimeOut())
                            {
                                TM.SetDelay(100);
                                FocusInfo.Clear();

                                if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc() )
                                {
                                    // 從前面掃描至極限位置
                                    //------------------------------------------------------
                                    MoveStep = (int)(Math.Abs(Pos_2 - Pos_1) / PartCount) * Dir;

                                    // 從較小的值出發
                                    //if (Pos_1 < Pos_2)
                                    //    MoveStart = Pos_1;
                                    //else
                                    //    MoveStart = Pos_2;
                                    if (Dir > 0)
                                    {
                                        MoveStart = Math.Min(Pos_1, Pos_2);
                                    }
                                    else
                                    {
                                        MoveStart = Math.Max(Pos_1, Pos_2);
                                    }
                                    //[1] Get all PartCount Pos & Focus Value 
                                    int MovePulse = 0;
                                    for (int i = 0; i <= PartCount; i++)
                                    {
                                        MovePulse = MoveStart + (i) * MoveStep;

                                        //if (MovePulse >= 150000)
                                        //    MovePulse = 149990;

                                        //if (MovePulse <= 0)
                                        //    MovePulse = 10;

                                        // Move To Postion 
                                        switch (MotorType)
                                        {
                                            case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                            case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                            case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                            case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                            case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                            case EnumMotoControlPackage.MS5515M:
                                            case EnumMotoControlPackage.CircleTac:
                                                {
                                                    this.motorControl.Move(MovePulse, -1, -1, -1);
                                                }
                                                break;
                                        }

                                        while (this.FlowRun)
                                        {
                                            if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc())
                                            {
                                                System.Threading.Thread.Sleep(100);

                                                //Grab Image
                                                this.grabber.Grab();

                                                //取像後Delay (ms)
                                                //Thread.Sleep(500);

                                                Pos = MovePulse;
                                                FocusValue = this.Calculate_FocusValue(this.grabber.grabImage, Para.UseROI, Para.ROI);
                                                this.AutoFocusPos = Pos;
                                                this.AutoFocusValue = FocusValue;
                                                UpdateFocusValue?.Invoke(Pos, FocusValue, false);

                                                break;
                                            }
                                            else
                                                System.Threading.Thread.Sleep(100);
                                        }
                                        // Update Focus Value
                                        FocusInfo.Add(Pos, FocusValue);
                                        System.Threading.Thread.Sleep(100);

                                    }

                                    //[2] Sorting                                     
                                    var sortedDict = FocusInfo.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

                                    // 1st Max Focus Value
                                    Pos_1 = sortedDict.Keys.ToList()[0];
                                    FocusValue_1 = sortedDict.Values.ToList()[0];

                                    // 2nd Max Focus Value
                                    Pos_2 = sortedDict.Keys.ToList()[1];
                                    FocusValue_2 = sortedDict.Values.ToList()[1];

                                    // 3rd Max Focus Value
                                    Pos_3 = sortedDict.Keys.ToList()[2];
                                    FocusValue_3 = sortedDict.Values.ToList()[2];

                                    // Error handling
                                    if (FocusValue_1 == 0 && FocusValue_2 == 0 && FocusValue_3 == 0)
                                    {
                                        this.NowStep = FlowStep.Error;  // ReCheck  
                                        break;
                                    }

                                    if ((Pos_1 > Pos_2 && Pos_1 < Pos_3) || (Pos_1 > Pos_3 && Pos_1 < Pos_2))
                                    {
                                        Pos_1 = Pos_3;
                                        FocusValue_1 = FocusValue_3;
                                    }

                                    if ((int)(Math.Abs(Pos_1 - Pos_2)) / PartCount <= Min_Step)
                                    {
                                        Pos_1 = sortedDict.Keys.ToList()[0];
                                        FocusValue_1 = sortedDict.Values.ToList()[0];

                                        //
                                        this.AutoFocusValue = FocusValue_1;
                                        this.AutoFocusPos = Pos_1;
                                        this.AutoFocusValue_2 = FocusValue_2;
                                        this.AutoFocusPos_2 = Pos_2;

                                        Flow_Result = new FlowResult {
                                            FocusPos = this.AutoFocusPos,
                                            FocusValue = Math.Round(this.AutoFocusValue, 5, MidpointRounding.AwayFromZero),
                                            FocusPos_2 = this.AutoFocusPos_2,
                                            FocusValue_2 = Math.Round(this.AutoFocusValue_2, 5, MidpointRounding.AwayFromZero),
                                        };

                                        this.NowStep = FlowStep.Finish;

                                        UpdateFocusValue?.Invoke(Pos_1, FocusValue_1, true);
                                        // Move To Best Position
                                        switch (MotorType)
                                        {
                                            case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                            case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                            case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                            case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                            case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                            case EnumMotoControlPackage.MS5515M:
                                            case EnumMotoControlPackage.CircleTac:
                                                {
                                                    this.motorControl.Move(this.AutoFocusPos, -1, -1, -1);
                                                }
                                                break;
                                        }

                                        while (this.FlowRun)
                                        {
                                            if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc())
                                            {
                                                this.grabber.Grab();
                                                break;
                                            }
                                        }

                                        while (this.FlowRun)
                                        {
                                            if (this.motorControl.MoveStatus_Focuser() == UnitStatus.Finish && !this.motorControl.IsMoveProc())
                                            {
                                                this.NowStep = FlowStep.Finish;
                                                break;
                                            }
                                            else
                                                System.Threading.Thread.Sleep(100);
                                        }
                                    }
                                    else
                                    {
                                        //[3] Move to Nest PartCount Procedure     
                                        if (Pos_1 < Pos_2)
                                        {
                                            // Move To Position
                                            switch (MotorType)
                                            {
                                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                                case EnumMotoControlPackage.MS5515M:
                                                case EnumMotoControlPackage.CircleTac:
                                                    {
                                                        this.motorControl.Move(Pos_1, -1, -1, -1);
                                                    }
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            // Move To Position
                                            switch (MotorType)
                                            {
                                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                                case EnumMotoControlPackage.MS5515M:
                                                case EnumMotoControlPackage.CircleTac:
                                                    {
                                                        this.motorControl.Move(Pos_2, -1, -1, -1);
                                                    }
                                                    break;
                                            }
                                        }

                                        do
                                        {
                                            System.Threading.Thread.Sleep(10);
                                        } while (this.motorControl.MoveStatus_Focuser() != UnitStatus.Finish && !this.motorControl.IsMoveProc());

                                        this.FlowRun = true;
                                        this.NowStep = FlowStep.CheckDirection_CheckMove;  // ReCheck   

                                    }
                                    //------------------------------------------------------                                      
                                }
                            }
                            break;

                        case FlowStep.Finish:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Focus Finish !");
                            }
                            break;
                        case FlowStep.Error:
                            {
                                this.FlowRun = false;
                                this.info.General("Caption Error !");
                            }
                            break;
                        case FlowStep.TimeOut:
                            {
                                this.FlowRun = false;
                                this.info.General("Auto Focus TimeOut !");
                            }
                            break;
                    }
                } while (this.FlowRun);

            }
            catch 
            {
                this.FlowRun = false;
                this.NowStep = FlowStep.Error;
            }
            finally
            {
                FocusInfo = null;
                TM = null;
                TM_TimeOut = null;

                if (milZone != MIL.M_NULL)
                {
                    MIL.MbufFree(milZone);
                    milZone = MIL.M_NULL;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        #region Method


        public int GetFocusValue()
        {
            this.grabber.Grab();
            return this.Calculate_FocusValue(this.grabber.grabImage);
        }

        public int Calculate_FocusValue(MIL_ID image, bool UseROI = false, Rectangle ROI = new Rectangle())
        {
            MIL_INT FocusScore = 0;

            if (UseROI)
            {
                double SizeX = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_X, MIL.M_NULL);
                double SizeY = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_Y, MIL.M_NULL);

                MIL_ID ROI_Image = MIL.M_NULL;

                MIL.MbufChild2d(this.grabber.grabImage,
                           ROI.X, ROI.Y,
                           ROI.Width, ROI.Height,
                           ref ROI_Image);

                MIL.MdigFocus(MIL.M_NULL,
                            ROI_Image,
                            MIL.M_DEFAULT,
                            null,
                            MIL.M_NULL,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_EVALUATE,
                            ref FocusScore);

                MIL.MbufFree(ROI_Image);
            }
            else
            {
                MIL.MdigFocus(MIL.M_NULL,
                            image,
                            MIL.M_DEFAULT,
                            null,
                            MIL.M_NULL,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_DEFAULT,
                            MIL.M_EVALUATE,
                            ref FocusScore);
            }

            return ((int)FocusScore);
        }

        #endregion


        public enum FlowStep
        {
            GetOriFocusValue = 0,
            CheckDirection_CheckMove,
            MoveNextPos,
            GetFocusValue,

            Stop,
            Error,
            TimeOut,
            Finish,
        }

        public class FlowResult
        {
            public int FocusPos { get; set; } = 0;
            public double FocusValue { get; set; } = 0;
            public int FocusPos_2 { get; set; } = 0;
            public double FocusValue_2 { get; set; } = 0;
        }
    }

    public class AutoFocus_FlowPara
    {
        [Category("AutoFocus Para"), DisplayName("01. Upper Limit"), Description("Upper Limit")]
        public int Upper_Limit { get; set; } = 0;

        [Category("AutoFocus Para"), DisplayName("02. Lower Limit"), Description("Lower Limit")]
        public int Lower_Limit { get; set; } = 0;

        [Category("AutoFocus Para"), DisplayName("03. Min Step"), Description("Min Step")]
        public int Min_Step { get; set; } = 0;

        [Category("AutoFocus Para"), DisplayName("04. Move Dir"), Description("Move Dir ( 1:Foward , 2:Reverse )")]
        public int MoveDir { get; set; } = 1;

        [Category("AutoFocus Para"), DisplayName("05. Use ROI"), Description("Use ROI")]
        public bool UseROI { get; set; } = false;

        [Category("AutoFocus Para"), DisplayName("06. ROI"), Description("ROI")]
        public Rectangle ROI { get; set; } = new Rectangle();

        public void Clone(AutoFocus_FlowPara Source)
        {
            this.Upper_Limit = Source.Upper_Limit;
            this.Lower_Limit = Source.Lower_Limit;
            this.Min_Step = Source.Min_Step;
            this.MoveDir = Source.MoveDir;
            this.UseROI = Source.UseROI;
            this.ROI = Source.ROI;
        }

        public override string ToString()
        {
            return "...";
        }
    }

}
