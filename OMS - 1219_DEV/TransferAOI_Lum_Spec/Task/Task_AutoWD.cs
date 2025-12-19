using BaseTool;
using CommonBase.Logger;
using FrameGrabber;
using HardwareManager;
using LightMeasure;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OpticalMeasuringSystem
{
    public class Task_AutoWD
    {
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;
        private AutoWD_FlowPara FlowPara = new AutoWD_FlowPara();

        //Task
        private Task_BaseAlign BaseAlign = null;

        //Flow
        private bool FlowRun = false;

        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowStep NowStep = FlowStep.Check_Device;
        public FlowStep Step
        {
            get => NowStep;
        }

        public Task_AutoWD(InfoManager Info, HardwareUnit Hardware)
        {
            this.Info = Info;
            this.Hardware = Hardware;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            if (!isAlm)
            {
                this.Info.General($"{Msg}");
            }
            else
            {
                this.Info.Error($"{Msg}");
            }

            LogMsg?.Invoke(Msg);
        }

        public void Start(AutoWD_FlowPara SourcePara)
        {
            this.FlowPara.Clone(SourcePara);

            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            Thread mThread = new Thread(() => Flow(this.FlowPara));
            mThread.Start();
        }

        public void Stop()
        {
            this.FlowRun = false;

            if (this.BaseAlign != null)
            {
                this.BaseAlign.Stop();
            }
        }

        public void Flow(AutoWD_FlowPara Para)
        {
            TimeManager TM = new TimeManager();
            TimeManager TM_FW_Move = new TimeManager();

            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            Task_AutoFocus AutoFocus = new Task_AutoFocus(Hardware.Grabber, Hardware.Motor, Info);
            AutoFocus_FlowPara AutoFoucs_Para = new AutoFocus_FlowPara();
            AutoFoucs_Para.Clone(Para.AutoFocus_Para);

            string OutputFolder = $@"D:\OMS\AutoWD\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            List<double> WD_List = new List<double>();
            WD_List = Para.WD_List.ToList();
            int WD_Idx = 0;
            int Capture_Idx = 0;
            double PLC_Offset = 0;
            double Current_PLC_X = 0;

            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check Grabber
                            bool Check_Grabber = (this.Hardware.Grabber != null);
                            if (!Check_Grabber)
                            {
                                SaveLog($"Check Device Fail :  Grabber is null", true);
                            }

                            Check &= Check_Grabber;

                            if (Check)
                            {
                                SaveLog($"Check Device OK");
                                NowStep = FlowStep.Check_Parameter;
                            }
                            else
                            {
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    case FlowStep.Check_Parameter:
                        {
                            bool Check = true;

                            //Check System
                            bool Check_System = (GlobalVar.SD.Camera_Type != "M_SYSTEM_HOST");
                            if (!Check_System)
                            {
                                SaveLog($"Check Parameter Fail : MIL System = M_SYSTEM_HOST", true);
                            }
                            Check &= Check_System;

                            //Check WD List
                            bool Check_WD_List = (WD_List.Count > 0);
                            if (!Check_WD_List)
                            {
                                SaveLog($"Check Parameter Fail : Pattern List Count = 0", true);
                            }

                            Check &= Check_WD_List;

                            if (Check)
                            {
                                SaveLog($"Check Parameter OK");
                                NowStep = FlowStep.Init;
                            }
                            else
                            {
                                NowStep = FlowStep.Alarm;
                            }
                        }
                        break;

                    case FlowStep.Init:
                        {
                            SaveLog($"Flow Start");

                            Directory.CreateDirectory(OutputFolder);

                            NowStep = FlowStep.CalWD;
                        }
                        break;

                    case FlowStep.AutoFocus:
                        {
                            AutoFocus.Start(AutoFoucs_Para);
                            SaveLog($"AutoFocus Start");

                            Thread.Sleep(1000);
                            NowStep = FlowStep.Check_AutoFocus;
                        }
                        break;

                    case FlowStep.Check_AutoFocus:
                        {
                            if (!AutoFocus.FlowRun)
                            {
                                switch (AutoFocus.Step)
                                {
                                    case Task_AutoFocus.FlowStep.Finish:
                                        {
                                            SaveLog($"AutoFocus Finish");
                                            NowStep = FlowStep.CalWD;
                                        }
                                        break;

                                    case Task_AutoFocus.FlowStep.Error:
                                        {
                                            SaveLog($"AutoFocus Error", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;

                                    case Task_AutoFocus.FlowStep.TimeOut:
                                        {
                                            SaveLog($"AutoFocus Timeout", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.CalWD:
                        {
                            Hardware.Grabber.Grab();

                            //Target
                            double TargetWD = WD_List[WD_Idx];
                            TargetWD = Math.Round(TargetWD, 3);
                            double Tolerance = Para.WD_Tolerance;
                            double Range_Max = TargetWD + Tolerance;
                            double Range_Min = TargetWD - Tolerance;

                            string SaveFolder = $@"{OutputFolder}\{TargetWD} mm\";
                            string Remark = $"{Capture_Idx}";
                            ScreenRatioResult CalResult = CalScreenRatio(Hardware.Grabber.grabImage, -1, SaveFolder, Remark, Para.SaveImage);

                            //Current
                            double ImageX = CalResult.Width;
                            double RealX = Para.Real_X_Size;
                            double f = GlobalVar.SD.Camera_FocalLength;
                            double PixelSize = GlobalVar.SD.Camera_PixelSize;
                            double CurrentWD = RealX / ImageX * f / PixelSize * 1000.0;
                            CurrentWD = Math.Round(CurrentWD, 3);

                            bool Check = (CurrentWD >= Range_Min && CurrentWD <= Range_Max);

                            SaveLog($"Cal WD , TargetWD = {TargetWD}±{Tolerance} mm , CurrentWD = {CurrentWD} mm");


                            if (Check)
                            {
                                SaveLog($"WD Check OK , Next WD");

                                NowStep = FlowStep.Check_WD_Idx;
                            }
                            else
                            {
                                PLC_Offset = TargetWD - CurrentWD;
                                SaveLog($"WD Check Fail , Diff = {PLC_Offset}" ,true);

                                NowStep = FlowStep.PLC_Move;
                            }

                            //Record RowData
                            string FilePath = $@"{OutputFolder}\RawData.csv";
                            double PLC_X = Current_PLC_X;
                            Task_AutoFocus.FlowResult AF_Result = AutoFocus.Result;

                            WriteRawData(FilePath, TargetWD, CurrentWD, PLC_X, AF_Result, Check);
                        }
                        break;

                    case FlowStep.PLC_Move:
                        {
                            double Get_PLC_X = Hardware.PLC.Plc2Pc.AxisPos;

                            if (Get_PLC_X != 0)
                            {
                                Capture_Idx++;

                                double X = Get_PLC_X + (PLC_Offset*1000);

                                X = Math.Round(X, 1);


                                this.Hardware.PLC.AbsMove(X);
                                Current_PLC_X = X;

                                SaveLog($"PLC Move , X = {X}");

                                NowStep = FlowStep.Check_PLC_InPos;
                            }
                        }
                        break;

                    case FlowStep.Check_PLC_InPos:
                        {
                            if (!Hardware.PLC.IsRun)
                            {
                                switch (Hardware.PLC.Status)
                                {
                                    case X_PLC_Ctrl.MoveStatus.Finish:
                                        {
                                            SaveLog($"Check PLC Move OK");
                                            NowStep = FlowStep.AutoFocus;
                                        }
                                        break;

                                    case X_PLC_Ctrl.MoveStatus.Alarm:
                                        {
                                            SaveLog($"Check PLC Move Fail", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Check_WD_Idx:
                        {
                            WD_Idx++;
                            Capture_Idx = 0;

                            if (WD_Idx < WD_List.Count)
                            {
                                double CurrentWD = WD_List[WD_Idx - 1];
                                double TargetWD = WD_List[WD_Idx];

                                PLC_Offset = TargetWD - CurrentWD;

                                NowStep = FlowStep.PLC_Move;
                            }
                            else
                            {
                                NowStep = FlowStep.Finish;
                            }
                        }
                        break;

                    case FlowStep.Finish:
                        {
                            SaveLog($"Flow Finish");
                            FlowRun = false;
                        }
                        break;

                    case FlowStep.Alarm:
                        {
                            SaveLog($"Flow Alarm", true);
                            FlowRun = false;
                        }
                        break;
                }

                MonitorStep?.Invoke($"[{(int)NowStep}] {NowStep}");

                Thread.Sleep(10);
            }

            AutoFocus.Stop();
        }

        public enum FlowStep
        {
            Check_Device = 0,
            Check_Parameter,
            Init,

            AutoFocus,
            Check_AutoFocus,
            CalWD,
            PLC_Move,
            Check_PLC_InPos,
            Check_WD_Idx,

            Finish,
            Alarm,
        }

        #region Method

        private bool WriteRawData_ErrorReport(string SavePath, string Pattern, int XYZ_Index)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("Pattern Path");
                    TitleList.Add("Fail Index (XYZ)");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{Pattern}");
                InfoList.Add($"{XYZ_Index}");

                string Info = string.Join(",", InfoList);

                SB.Append(Info + "\r\n");

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Write RawData Fail : {ex.Message}");
                return false;
            }
        }

        private bool WriteRawData(string SavePath, double TargetWD, double CurrentWD, double PLC_X_Pos, Task_AutoFocus.FlowResult Focuser_Pos, bool Status)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("Target WD");
                    TitleList.Add("Current WD");
                    TitleList.Add("Aperture Pos"); 
                    TitleList.Add("PLC X Pos"); 
                    TitleList.Add("Focuser Pos");
                    TitleList.Add("Status");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{TargetWD}");
                InfoList.Add($"{CurrentWD}");
                InfoList.Add($"{this.Hardware.Motor.NowPos_Aperture()}");
                InfoList.Add($"{this.Hardware.PLC.Plc2Pc.AxisPos}");
                InfoList.Add($"{this.Hardware.Motor.NowPos_Focuser()}");
                InfoList.Add($"{Status}");

                string Info = string.Join(",", InfoList);

                SB.Append(Info + "\r\n");

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Write RowData Fail : {ex.Message}");
                return false;
            }
        }

        public ScreenRatioResult CalScreenRatio(MIL_ID Img, int Threshold, string SaveFolder, string Backup, bool SaveImage = false) //W,H
        {
            MIL_ID blobContext = MIL.M_NULL;
            MIL_ID blobResult = MIL.M_NULL;
            MIL_ID mil_16U_SrcImag = MIL.M_NULL;

            int blobCount = 0;
            double Blob_H = 0.0;
            double Blob_W = 0.0;

            MIL_INT SizeX = 0;
            MIL_INT SizeY = 0;

            bool Debug = false;

            ScreenRatioResult Result = new ScreenRatioResult();

            if (Debug || SaveImage) Directory.CreateDirectory(SaveFolder);

            try
            {
                SizeX = MIL.MbufInquire(Img, MIL.M_SIZE_X, MIL.M_NULL);
                SizeY = MIL.MbufInquire(Img, MIL.M_SIZE_Y, MIL.M_NULL);
                // Blob
                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobContext);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref blobResult);
                //搜尋最大面積的 Blob
                MIL.MblobControl(blobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_GRAYSCALE, MIL.M_ENABLE);
                MIL.MblobControl(blobContext, MIL.M_SORT1, MIL.M_AREA);
                MIL.MblobControl(blobContext, MIL.M_SORT1_DIRECTION, MIL.M_SORT_DOWN);
                MIL.MblobControl(blobContext, MIL.M_SAVE_RUNS, MIL.M_ENABLE);

                MIL.MbufAlloc2d(MyMil.MilSystem, SizeX, SizeY, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_16U_SrcImag);

                MIL.MbufCopy(Img, mil_16U_SrcImag);

                MIL.MimDilate(mil_16U_SrcImag, mil_16U_SrcImag, 5, MIL.M_GRAYSCALE);
                MIL.MimErode(mil_16U_SrcImag, mil_16U_SrcImag, 5, MIL.M_GRAYSCALE);

                if (SaveImage)
                {
                    MIL.MbufSave($@"{SaveFolder}/{Backup}.tif", mil_16U_SrcImag);
                }

                if (Debug)
                {
                    MIL.MbufSave($@"{SaveFolder}/{Backup}_Source.tif", mil_16U_SrcImag);
                }

                if (Threshold == -1)
                {
                    MIL_INT BinValue = MIL.MimBinarize(mil_16U_SrcImag, MIL.M_NULL, MIL.M_BIMODAL, MIL.M_NULL, MIL.M_NULL);
                    MIL.MimBinarize(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_GREATER_OR_EQUAL, BinValue, MIL.M_NULL);
                }
                else
                {
                    MIL.MimBinarize(mil_16U_SrcImag, mil_16U_SrcImag, MIL.M_FIXED + MIL.M_LESS, Threshold, MIL.M_NULL);
                }

                if (Debug)
                {
                    MIL.MbufSave($@"{SaveFolder}/{Backup}_Binary.tif", mil_16U_SrcImag);
                }

                MIL.MblobCalculate(blobContext, mil_16U_SrcImag, MIL.M_NULL, blobResult);
                MIL.MblobGetResult(blobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref blobCount);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);
                MIL.MblobDraw(MIL.M_DEFAULT, blobResult, mil_16U_SrcImag, MIL.M_DRAW_HOLES, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                if (Debug)
                {
                    MIL.MbufSave($@"{SaveFolder}/{Backup}_Blob.tif", mil_16U_SrcImag);
                }

                if (blobCount > 0)
                {
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_FERET_X, ref Blob_W);
                    MIL.MblobGetResult(blobResult, MIL.M_BLOB_INDEX(0), MIL.M_FERET_Y, ref Blob_H);

                    Result.ScreenRatio_W = (double)Blob_W / (double)SizeX;
                    Result.ScreenRatio_H = (double)Blob_H / (double)SizeY;
                    Result.Width = Blob_W;
                    Result.Height = Blob_H;
                }
                else
                {
                    Result = new ScreenRatioResult();
                }
            }
            catch
            {
                Result = new ScreenRatioResult();
            }
            finally
            {
                if (mil_16U_SrcImag != MIL.M_NULL)
                {
                    MIL.MbufFree(mil_16U_SrcImag);
                    mil_16U_SrcImag = MIL.M_NULL;
                }

                if (blobContext != MIL.M_NULL)
                {
                    MIL.MblobFree(blobContext);
                    blobContext = MIL.M_NULL;
                }

                if (blobResult != MIL.M_NULL)
                {
                    MIL.MblobFree(blobResult);
                    blobResult = MIL.M_NULL;
                }
            }

            return Result;
        }

        public double GetWD(double RealX)
        {
            ScreenRatioResult CalResult = CalScreenRatio(Hardware.Grabber.grabImage, -1, "", "", false);

            //Current
            double ImageX = CalResult.Width;      
            double f = GlobalVar.SD.Camera_FocalLength;
            double PixelSize = GlobalVar.SD.Camera_PixelSize;
            double CurrentWD = RealX / ImageX * f / PixelSize * 1000.0;
            CurrentWD = Math.Round(CurrentWD, 3);

            return CurrentWD;
        }

        public class ScreenRatioResult
        {
            public double ScreenRatio_H { get; set; } = 0.0;
            public double ScreenRatio_W { get; set; } = 0.0;
            public double Width { get; set; } = 0.0;
            public double Height { get; set; } = 0.0;
        }

        #endregion

    }

    public class AutoWD_FlowPara
    {
        [Category("A. 工作距離"), DisplayName("01. 待測物長度 (mm)")]
        public double Real_X_Size { get; set; } = 0.0;

        [Category("A. 工作距離"), DisplayName("02. 工作距離清單 (mm)")]
        public double[] WD_List { get; set; } = new double[] { };

        [Category("A. 工作距離"), DisplayName("03. 誤差容許值 (mm)")]
        public double WD_Tolerance { get; set; } = 10;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("B. 自動對焦"), DisplayName("01. 自動對焦參數")]
        public AutoFocus_FlowPara AutoFocus_Para { get; set; } = new AutoFocus_FlowPara();

        [Category("C. 其他"), DisplayName("01. 照片留存")]
        public bool SaveImage { get; set; } = true;


        public void Clone(AutoWD_FlowPara Source)
        {
            //A
            this.Real_X_Size = Source.Real_X_Size;
            this.WD_List = Source.WD_List;
            this.WD_Tolerance = Source.WD_Tolerance;

            //B
            this.AutoFocus_Para.Clone(Source.AutoFocus_Para);
        }

        public static void WriteXML(AutoWD_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(AutoWD_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static AutoWD_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                AutoWD_FlowPara m;

                serializer = new XmlSerializer(typeof(AutoWD_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (AutoWD_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }

        public enum SourceType
        {
            Image,
            csv,
        }
    }
}
