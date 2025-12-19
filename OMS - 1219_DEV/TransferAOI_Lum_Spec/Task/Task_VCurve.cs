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
using System.Xml.Serialization;

namespace OpticalMeasuringSystem
{
    public class Task_VCurve
    {
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

        public Task_VCurve(MilDigitizer grabber, IMotorControl motorControl, InfoManager info)
        {
            this.info = info;
            this.grabber = grabber;
            this.motorControl = motorControl;
        }


        public void Start(VCurve_FlowPara Para)
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
            this.FlowRun = false;
        }

        public void Flow(VCurve_FlowPara Para)
        {
            //AiryMember device = AiryMember.Focuser;
            MIL_ID[] milZone = new MIL_ID[5];
            MIL_ID[] milROI = new MIL_ID[5];
            MIL_INT SizeX = 0;
            MIL_INT SizeY = 0;

            int PartIndex = 0;

            int Upper_Limit = Para.OptimalPos + Para.PosOffset;
            int Lower_Limit = 0;

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

            int ROI_StX = 0;
            int ROI_StY = 0;
            int ROI_SizeX = 0;
            int ROI_SizeY = 0;

            double Inspect_Percent = 0.8;
            double Inspect_OffsetRatio = (1 - Inspect_Percent) * 0.5;

            bool isVCurve_Proc = true;

            string[] location = new string[5];
            string[] dirPath = new string[5];
            string filePath = "";

            Dictionary<int, double>[] FocusInfo = new Dictionary<int, double>[5];
            TimeManager TM_TimeOut = new TimeManager();
            TM_TimeOut.SetDelay(Para.PartCount * 90000);

            // Initial
            this.NowStep = FlowStep.GetOriFocusValue;

            if (Para.OptimalPos == -1)
            {
                Upper_Limit = -1;
                Lower_Limit = -1;
                this.VCurve_List = "";
                isVCurve_Proc = false;
            }
            else
            {
                Upper_Limit = (int)(Para.OptimalPos + 0.5 * Para.PosOffset);
                Lower_Limit = (int)(Para.OptimalPos - 0.5 * Para.PosOffset);
                this.VCurve_List = "";
                isVCurve_Proc = true;

                if (Upper_Limit >= motorControl.Focus_LimitF())
                    Upper_Limit = motorControl.Focus_LimitF();
                else if (Upper_Limit <= motorControl.Focus_LimitR())
                    Upper_Limit = motorControl.Focus_LimitR();

                if (Lower_Limit >= motorControl.Focus_LimitF())
                    Lower_Limit = motorControl.Focus_LimitF();
                else if (Lower_Limit <= motorControl.Focus_LimitR())
                    Lower_Limit = motorControl.Focus_LimitR();
            }

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    milZone[i] = MIL.M_NULL;
                    milROI[i] = MIL.M_NULL;

                    MIL.MbufChild2d(this.grabber.grabImage, 0, 0, 1, 1, ref milZone[i]);

                    SizeX = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_X, MIL.M_NULL);
                    SizeY = MIL.MbufInquire(this.grabber.grabImage, MIL.M_SIZE_Y, MIL.M_NULL);

                    MIL.MbufChild2d(this.grabber.grabImage,
                        Convert.ToInt32(Inspect_OffsetRatio * SizeX), Convert.ToInt32(Inspect_OffsetRatio * SizeY),
                        Convert.ToInt32(Inspect_Percent * SizeX), Convert.ToInt32(Inspect_Percent * SizeY),
                        ref milZone[i]);

                    ROI_SizeX = Convert.ToInt32(SizeX * Para.ROI_SizeX_P / 100);
                    ROI_SizeY = ROI_SizeX;  //正方形

                    // Get ROI
                    switch (i)
                    {
                        case (int)ROI_Location.Left_Top:
                            ROI_StX = Convert.ToInt32(Inspect_OffsetRatio * SizeX);
                            ROI_StY = Convert.ToInt32(Inspect_OffsetRatio * SizeY);
                            location[i] = "Left_Top";
                            break;
                        case (int)ROI_Location.Left_Bottom:
                            ROI_StX = Convert.ToInt32(Inspect_OffsetRatio * SizeX);
                            ROI_StY = Convert.ToInt32((1 - Inspect_OffsetRatio) * SizeY) - ROI_SizeY - 1;
                            location[i] = "Left_Bottom";
                            break;
                        case (int)ROI_Location.Center:
                            ROI_StX = Convert.ToInt32(0.5 * (SizeX - ROI_SizeX));
                            ROI_StY = Convert.ToInt32(0.5 * (SizeY - ROI_SizeY));
                            location[i] = "Center";
                            break;
                        case (int)ROI_Location.Right_Top:
                            ROI_StX = Convert.ToInt32((1 - Inspect_OffsetRatio) * SizeX) - ROI_SizeX - 1;
                            ROI_StY = Convert.ToInt32(Inspect_OffsetRatio * SizeY);
                            location[i] = "Right_Top";
                            break;
                        case (int)ROI_Location.Right_Bottom:
                            ROI_StX = Convert.ToInt32((1 - Inspect_OffsetRatio) * SizeX) - ROI_SizeX - 1;
                            ROI_StY = Convert.ToInt32((1 - Inspect_OffsetRatio) * SizeY) - ROI_SizeY - 1;
                            location[i] = "Right_Bottom";
                            break;
                    }
                    MIL.MbufChildMove(milZone[i], ROI_StX, ROI_StY, ROI_SizeX, ROI_SizeY, MIL.M_DEFAULT);

                    if (milROI[i] == MIL.M_NULL)
                        MIL.MbufClone(milZone[i], MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref milROI[i]);

                    dirPath[i] = string.Format("{0}\\{1}\\{2}", Para.OutputFolder, "V_Curve", location[i]);
                    if (!Directory.Exists(dirPath[i]))
                        Directory.CreateDirectory(dirPath[i]);
                    else
                        FolderManager.Delete_All_Files(dirPath[i]);
                }

                do
                {
                    if (TM_TimeOut.IsTimeOut())
                    {
                        this.FlowRun = false;
                        this.NowStep = FlowStep.TimeOut;  // ReCheck

                        this.info.General("V Curve Proc TimeOut !");
                    }

                    switch (this.NowStep)
                    {
                        case FlowStep.GetOriFocusValue:
                            {
                                // Initial
                                for (int i = 0; i < 5; i++)
                                {
                                    FocusInfo[i] = new Dictionary<int, double>();
                                    FocusInfo[i].Clear();
                                }

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

                                // Move to min Position
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

                                Thread.Sleep(100);

                                this.NowStep = FlowStep.CheckDirection_CheckMove;  // ReCheck
                            }
                            break;

                        case FlowStep.CheckDirection_CheckMove:
                            {
                                if (!this.motorControl.IsMoveProc())
                                {
                                    switch (this.motorControl.MoveFlowStatus())
                                    {

                                        case UnitStatus.Finish:
                                            {
                                                PartIndex = 0;
                                                // 從前面掃描至極限位置
                                                //------------------------------------------------------
                                                MoveStep = (int)(Math.Abs(Pos_2 - Pos_1) / (Para.PartCount - 1));

                                                // 從較小的值出發
                                                if (Pos_1 < Pos_2)
                                                    MoveStart = Pos_1;
                                                else
                                                    MoveStart = Pos_2;

                                                this.NowStep = FlowStep.MoveNextPos;

                                            }
                                            break;

                                        default:
                                            {
                                                this.NowStep = FlowStep.Error;
                                                this.info.General("Move Flow Error");
                                            }
                                            break;
                                    }
                                }
                            }
                            break;

                        case FlowStep.MoveNextPos:
                            {
                                int MovePulse = MoveStart + (PartIndex) * MoveStep;

                                if (MovePulse >= motorControl.Focus_LimitF())
                                    MovePulse = motorControl.Focus_LimitF();

                                if (MovePulse <= motorControl.Focus_LimitR())
                                    MovePulse = motorControl.Focus_LimitR();

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

                                System.Threading.Thread.Sleep(100);

                                this.NowStep = FlowStep.GetFocusValue;

                            }
                            break;

                        case FlowStep.GetFocusValue:
                            {
                                if (!this.motorControl.IsMoveProc())
                                {
                                    switch (this.motorControl.MoveFlowStatus())
                                    {

                                        case UnitStatus.Finish:
                                            {
                                                this.grabber.Grab();
                                                Pos = this.motorControl.GetPosition(MotorMember.Focuser);
                                                //MIL.MbufCopy(milZone, milROI);
                                                //FocusValue = this.Calculate_FocusValue(this.grabber.grabImage);

                                                for (int j = 0; j < 5; j++)
                                                {

                                                    FocusValue = this.Calculate_FocusValue(milZone[j]);
                                                    this.AutoFocusPos = Pos;
                                                    this.AutoFocusValue = FocusValue;

                                                    //Save ROI
                                                    if (isVCurve_Proc)
                                                    {
                                                        filePath = string.Format(@"{0}\{1}_Pos{2}_Val{3}.tif", dirPath[j], location[j], Pos, FocusValue);
                                                        MIL.MbufSave(filePath, milZone[j]);
                                                    }

                                                    FocusInfo[j].Add(Pos, FocusValue);

                                                }

                                                PartIndex++;

                                                if (PartIndex < Para.PartCount)
                                                {
                                                    this.NowStep = FlowStep.MoveNextPos;
                                                }
                                                else
                                                {
                                                    for (int j = 0; j < 5; j++)
                                                    {
                                                        //[2] Sorting                                     
                                                        var sortedDict = FocusInfo[j].OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

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
                                                    }

                                                    if (isVCurve_Proc == false)  // 搜尋對焦位置
                                                    {
                                                        if ((Pos_1 > Pos_2 && Pos_1 < Pos_3) || (Pos_1 > Pos_3 && Pos_1 < Pos_2))
                                                        {
                                                            Pos_1 = Pos_3;
                                                            FocusValue_1 = FocusValue_3;
                                                        }

                                                        if ((int)(Math.Abs(Pos_1 - Pos_2)) / Para.PartCount <= Para.Min_Step)
                                                        {
                                                            //
                                                            this.AutoFocusValue = FocusValue_1;
                                                            this.AutoFocusPos = Pos_1;
                                                            this.AutoFocusValue_2 = FocusValue_2;
                                                            this.AutoFocusPos_2 = Pos_2;

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
                                                                    this.NowStep = FlowStep.Finish;
                                                                    break;
                                                                }
                                                                else
                                                                    System.Threading.Thread.Sleep(100);
                                                            }
                                                        }
                                                        else
                                                        {

                                                            // Move to Min Pos(Pos_1, Pos_2)
                                                            if (Pos_1 < Pos_2)
                                                            {
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
                                                    }
                                                    else
                                                    {   // Get V Curve List
                                                        this.AutoFocusValue = FocusValue_1;
                                                        this.AutoFocusPos = Pos_1;
                                                        this.AutoFocusValue_2 = FocusValue_2;
                                                        this.AutoFocusPos_2 = Pos_2;

                                                        this.VCurve_List = "";

                                                        // Output Excel File
                                                        string ExcelPath = string.Format(@"{0}\\{1}\\VCurve_{2}.xlsx", Para.OutputFolder, "V_Curve", DateTime.Now.ToString("yyyyddMM"));

                                                        for (int j = 0; j < 5; j++)
                                                        {
                                                            VCurve_WriteToCSV(ExcelPath, FocusInfo[j], j);
                                                        }


                                                        this.NowStep = FlowStep.Finish;
                                                    }
                                                }

                                            }
                                            break;

                                        default:
                                            {
                                                this.NowStep = FlowStep.Error;
                                                this.info.General("Move Flow Error");
                                            }
                                            break;
                                    }
                                }
                            }
                            break;


                        case FlowStep.Finish:
                            {   // Get V Curve List
                                this.AutoFocusValue = FocusValue_1;
                                this.AutoFocusPos = Pos_1;
                                this.AutoFocusValue_2 = FocusValue_2;
                                this.AutoFocusPos_2 = Pos_2;

                                this.VCurve_List = "";

                                // Output Excel File
                                string ExcelPath = string.Format(@"{0}\\{1}\\VCurve_{2}.xlsx", Para.OutputFolder, "V_Curve", DateTime.Now.ToString("yyyyddMM"));

                                for (int j = 0; j < 5; j++)
                                {
                                    VCurve_WriteToCSV(ExcelPath, FocusInfo[j], j);
                                }

                                this.FlowRun = false;
                                this.info.General("V Curve Proc Finish !");
                            }
                            break;

                        case FlowStep.Error:
                            {
                                this.FlowRun = false;
                                this.info.Error("V Curve Proc Error !");
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
                TM_TimeOut = null;

                for (int i = 0; i < 5; i++)
                    if (milZone[i] != MIL.M_NULL)
                    {
                        MIL.MbufFree(milZone[i]);
                        milZone[i] = MIL.M_NULL;
                    }

                for (int i = 0; i < 5; i++)
                    if (milROI[i] != MIL.M_NULL)
                    {
                        MIL.MbufFree(milROI[i]);
                        milROI[i] = MIL.M_NULL;
                    }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }



        #region Method

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

        public void VCurve_WriteToCSV(string filePath, Dictionary<int, double> FocusInfo, int ROILocation)
        {
            string strPos = "";
            string strFocusVal = "";
            int Count = FocusInfo.Count;
            int i = 2;
            XLWorkbook wbook = null;
            IXLWorksheet ws;

            if (File.Exists(filePath))
            {
                wbook = new XLWorkbook(filePath);
                ws = wbook.Worksheet(1);
            }
            else
            {
                wbook = new XLWorkbook();
                ws = wbook.Worksheets.Add("Sheet1");
            }

            switch (ROILocation)
            {
                case (int)ROI_Location.Left_Top:
                    strPos = "A";
                    strFocusVal = "B";
                    ws.Cell("B1").Value = "左上";
                    ws.Cell("A2").Value = "調焦Pulse量";
                    ws.Cell("B2").Value = "對焦分數";
                    break;
                case (int)ROI_Location.Left_Bottom:
                    strPos = "C";
                    strFocusVal = "D";
                    ws.Cell("D1").Value = "左下";
                    ws.Cell("C2").Value = "調焦Pulse量";
                    ws.Cell("D2").Value = "對焦分數";
                    break;
                case (int)ROI_Location.Center:
                    strPos = "E";
                    strFocusVal = "F";
                    ws.Cell("F1").Value = "中心";
                    ws.Cell("E2").Value = "調焦Pulse量";
                    ws.Cell("F2").Value = "對焦分數";
                    break;
                case (int)ROI_Location.Right_Top:
                    strPos = "G";
                    strFocusVal = "H";
                    ws.Cell("H1").Value = "右上";
                    ws.Cell("G2").Value = "調焦Pulse量";
                    ws.Cell("H2").Value = "對焦分數";
                    break;
                case (int)ROI_Location.Right_Bottom:
                    strPos = "I";
                    strFocusVal = "J";
                    ws.Cell("J1").Value = "右下";
                    ws.Cell("I2").Value = "調焦Pulse量";
                    ws.Cell("J2").Value = "對焦分數";
                    break;
            }

            foreach (KeyValuePair<int, double> item in FocusInfo)
            {
                i = i + 1;
                ws.Cell($"{strPos}{i}").Value = item.Key;
                ws.Cell($"{strFocusVal}{i}").Value = item.Value;
            }

            // 副檔名 : .xlsx
            wbook.SaveAs(filePath);

            wbook = null;
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

        public enum ROI_Location
        {
            Left_Top = 0,
            Left_Bottom,
            Center,
            Right_Top,
            Right_Bottom,
        }
    }

    public class VCurve_FlowPara
    {
        [Category("VCurve Para"), DisplayName("01. Output_DirPath"), Description("Output_DirPath"), Browsable(false), XmlIgnore]
        public string OutputFolder { get; set; } = "";

        [Category("VCurve Para"), DisplayName("01. Optimal Pos"), Description("Optimal Pos")]
        public int OptimalPos { get; set; } = -1;

        [Category("VCurve Para"), DisplayName("02. Pos Offset"), Description("Pos Offset")]
        public int PosOffset { get; set; } = 0;

        [Category("VCurve Para"), DisplayName("03. Min Step"), Description("Min Step")]
        public int Min_Step { get; set; } = 50;

        [Category("VCurve Para"), DisplayName("04. ROILocation"), Description("Left-Top, Left-Bottom, Center, Right-Top, Right-Bottom")]
        public int ROILocation { get; set; } = 0;

        [Category("VCurve Para"), DisplayName("05. ROI SizeX (%)"), Description(" ROI SizeX (%)")]
        public int ROI_SizeX_P { get; set; } = 10;

        [Category("VCurve Para"), DisplayName("06. PartCount"), Description("PartCount")]
        public int PartCount { get; set; } = 11;

        public void Clone(VCurve_FlowPara Source)
        {
            this.OutputFolder = Source.OutputFolder;
            this.OptimalPos = Source.OptimalPos;
            this.PosOffset = Source.PosOffset;
            this.Min_Step = Source.Min_Step;
            this.ROILocation = Source.ROILocation;
            this.ROI_SizeX_P = Source.ROI_SizeX_P;
            this.PartCount = Source.PartCount;
        }

        public override string ToString()
        {
            return "...";
        }
    }





}
