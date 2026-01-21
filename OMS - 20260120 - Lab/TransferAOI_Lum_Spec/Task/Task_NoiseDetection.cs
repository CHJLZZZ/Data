using BaseTool;
using CommonBase.Logger;
using HardwareManager;
using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OpticalMeasuringSystem
{
    public class Task_NoiseDetection
    {
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;

        //Task
        private Task_BaseAlign BaseAlign = null;

        //Flow
        private bool FlowRun = false;
        private UnitStatus CheckForm_IsCheck = UnitStatus.Idle;


        public bool IsRun
        {
            get => FlowRun;
        }

        private FlowStep NowStep = FlowStep.Check_Device;
        public FlowStep Step
        {
            get => NowStep;
        }

        public Task_NoiseDetection(InfoManager Info, HardwareUnit Hardware)
        {
            this.Info = Info;
            this.Hardware = Hardware;
        }

        public void SaveLog(string Msg, bool isAlm = false)
        {
            string LogTitle = "Noise Detection";

            if (!isAlm)
            {
                this.Info.General($"[{LogTitle}] {Msg}");
            }
            else
            {
                this.Info.Error($"[{LogTitle}] {Msg}");
            }

            LogMsg?.Invoke(Msg);
        }

        public void Start(NoiseDetection_FlowPara SourcePara)
        {
            if (FlowRun)
            {
                FlowRun = false;
                Thread.Sleep(100);
            }

            NoiseDetection_FlowPara Para = new NoiseDetection_FlowPara();
            Para.Clone(SourcePara);

            Thread mThread = new Thread(() => Flow(Para));
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

        public void Flow(NoiseDetection_FlowPara Para)
        {
            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            int CycleIdx = 0;

            RecordPara RecordInfo = new RecordPara();
            List<Point>[] NoiseList = new List<Point>[] { };
            Para.OutputFolder = $@"{Para.OutputFolder}\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";


            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check Grabber
                            bool Check_Grabber = true;
                            Check_Grabber = (this.Hardware.Grabber != null);
                            if (!Check_Grabber)
                            {
                                SaveLog($"Check Device Fail :  Grabber is null", true);
                            }

                            Check &= Check_Grabber;


                            if (Check)
                            {
                                SaveLog($"Check Devie OK");
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

                            Directory.CreateDirectory(Para.OutputFolder);
                            NoiseDetection_FlowPara.WriteXML(Para, $@"{Para.OutputFolder}\Config.xml");

                            NowStep = FlowStep.BaseAlign;
                        }
                        break;

                    case FlowStep.BaseAlign:
                        {
                            BaseAlign = new Task_BaseAlign(this.Info, this.Hardware);
                            BaseAlign.Start(Para.BaseAlign_Para);

                            Thread.Sleep(1000);
                            SaveLog($"Base Align Start");
                            NowStep = FlowStep.Check_BaseAlign;
                        }
                        break;

                    case FlowStep.Check_BaseAlign:
                        {
                            if (!BaseAlign.IsRun)
                            {
                                switch (BaseAlign.Step)
                                {
                                    case Task_BaseAlign.FlowStep.Finish:
                                        {
                                            SaveLog($"Check Base Align Finish");
                                            NowStep = FlowStep.CheckType;
                                        }
                                        break;

                                    default:
                                        {
                                            SaveLog($"Check Base Align Fail , Step = {BaseAlign.Step}", true);
                                            NowStep = FlowStep.Alarm;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.CheckType:
                        {
                            SaveLog($"Flow Type = {Para.Type}");

                            switch (Para.Type)
                            {
                                case NoiseType.Dark:
                                case NoiseType.DarkAndBright:
                                    {
                                        NowStep = FlowStep.DarkNoise_Detection_Start;
                                    }
                                    break;

                                case NoiseType.Bright:
                                    {
                                        NowStep = FlowStep.BrightNoise_Detection_Start;
                                    }
                                    break;
                            }
                        }
                        break;

                    #region Dark Noise

                    case FlowStep.DarkNoise_Detection_Start:
                        {
                            SaveLog($"### Bright Noise Detection Start");

                            RecordInfo = new RecordPara();
                            RecordInfo.Type = NoiseType.Dark;
                            RecordInfo.ExposureTime = Para.DarkNoise_ExpTime;
                            RecordInfo.UpperThreashold = Math.Max(Para.DarkNoise_UpperThreashold, Para.DarkNoise_LowerThreashold);
                            RecordInfo.LowerThreashold = Math.Min(Para.DarkNoise_UpperThreashold, Para.DarkNoise_LowerThreashold);

                            CycleIdx = 0;
                            NoiseList = new List<Point>[Para.CycleCount];

                            NowStep = FlowStep.Capture;
                        }
                        break;

                    #endregion

                    #region Bright Noise

                    case FlowStep.BrightNoise_Detection_Start:
                        {
                            SaveLog($"### Dark Noise Detection Start");

                            RecordInfo = new RecordPara();
                            RecordInfo.Type = NoiseType.Bright;
                            RecordInfo.ExposureTime = Para.BrightNoise_ExpTime;
                            RecordInfo.UpperThreashold = Math.Max(Para.BrightNoise_UpperThreashold, Para.BrightNoise_LowerThreashold);
                            RecordInfo.LowerThreashold = Math.Min(Para.BrightNoise_UpperThreashold, Para.BrightNoise_LowerThreashold);

                            CycleIdx = 0;
                            NoiseList = new List<Point>[Para.CycleCount];

                            NowStep = FlowStep.Show_CheckForm;
                        }
                        break;

                    case FlowStep.Show_CheckForm:
                        {
                            CheckForm_IsCheck = UnitStatus.Running;

                            Form mainForm = Application.OpenForms["ManualForm"];
                            mainForm.BeginInvoke((MethodInvoker)delegate {
                                FlowCheckForm CheckForm = new FlowCheckForm();

                                CheckForm.isCheck -= CheckForm_isCheck;
                                CheckForm.isCheck += CheckForm_isCheck;

                                CheckForm.SetPare("亮噪點偵測 請蓋鏡頭蓋\r\n請確認已蓋鏡頭蓋");
                                CheckForm.ShowDialog();
                                CheckForm.BringToFront();
                            }
                            );

                            SaveLog($"Show UserCheckForm");
                            NowStep = FlowStep.Check_NextStep;
                        }
                        break;

                    case FlowStep.Check_NextStep:
                        {
                            switch (CheckForm_IsCheck)
                            {
                                case UnitStatus.Finish:
                                    {
                                        SaveLog($"UserCheckForm Select OK");
                                        NowStep = FlowStep.Capture;
                                    }
                                    break;

                                case UnitStatus.Idle:
                                    {
                                        SaveLog($"UserCheckForm Select NG", true);
                                        NowStep = FlowStep.Alarm;
                                    }
                                    break;
                            }
                        }
                        break;

                    #endregion

                    case FlowStep.Capture:
                        {
                            double ExpTime = RecordInfo.ExposureTime;
                            NoiseType Type = RecordInfo.Type;

                            SaveLog($"### Type = {Type} , Cycle = {CycleIdx + 1}/{Para.CycleCount}");

                            this.Hardware.Grabber.SetExposureTime(ExpTime);
                            this.Hardware.Grabber.Grab();
                            Thread.Sleep(100);

                            SaveLog($"Capture , ExpTime : {ExpTime}");


                            string SaveFolder = $@"{Para.OutputFolder}\{Type}\";
                            Directory.CreateDirectory(SaveFolder);

                            string ImgPath = $@"{SaveFolder}\{CycleIdx}.tif";
                            MIL.MbufSave(ImgPath, this.Hardware.Grabber.grabImage);
                            SaveLog($"Save Image : {ImgPath}");

                            NowStep = FlowStep.SearchNoise;
                        }
                        break;

                    case FlowStep.SearchNoise:
                        {
                            double LowerThreashold = RecordInfo.LowerThreashold;
                            double UpperThreashold = RecordInfo.UpperThreashold;

                            //RecordInfo.NoiseList = NoiseSearch_Blob(this.Hardware.Grabber.grabImage, LowerThreashold, UpperThreashold);
                            RecordInfo.NoiseList = NoiseSearch_GetValue(this.Hardware.Grabber.grabImage, LowerThreashold, UpperThreashold);
                            
                            NoiseList[CycleIdx] = new List<Point>();
                            foreach (NoiseInfo Noise in RecordInfo.NoiseList)
                            {
                                NoiseList[CycleIdx].Add(new Point(Noise.X, Noise.Y));
                            }

                            SaveLog($"Noise Search , Count : {RecordInfo.NoiseList.Count}");

                            NowStep = FlowStep.RecordCsv;
                        }
                        break;

                    case FlowStep.RecordCsv:
                        {
                            RecordInfo.CycleIdx = CycleIdx;
                            WriteRowData($@"{Para.OutputFolder}\RowData_{RecordInfo.Type}.csv", RecordInfo);

                            NowStep = FlowStep.CheckCycleIdx;
                        }
                        break;

                    case FlowStep.CheckCycleIdx:
                        {
                            CycleIdx++;

                            if (CycleIdx < Para.CycleCount)
                            {
                                NowStep = FlowStep.Capture;
                            }
                            else
                            {
                                WriteResult($@"{Para.OutputFolder}\Result_{RecordInfo.Type}.csv", NoiseList);

                                bool DarkAndBright = (Para.Type == NoiseType.DarkAndBright);
                                bool DarkFlow = (RecordInfo.Type == NoiseType.Dark);

                                if (DarkAndBright && DarkFlow)
                                {
                                    NowStep = FlowStep.BrightNoise_Detection_Start;
                                }
                                else
                                {
                                    NowStep = FlowStep.Finish;
                                }
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
            }
        }

        private void CheckForm_isCheck(bool Check)
        {
            CheckForm_IsCheck = (Check) ? UnitStatus.Finish : UnitStatus.Idle;
        }

        public enum FlowStep
        {
            //Check & Init 
            Check_Device = 0,
            Check_Parameter,
            Init,

            //Base Align
            BaseAlign,
            Check_BaseAlign,

            //CheckType
            CheckType,

            //Dark Noise
            DarkNoise_Detection_Start,

            //Bright Noise
            BrightNoise_Detection_Start,
            Show_CheckForm,
            Check_NextStep,

            //Capture
            Capture,
            SearchNoise,
            RecordCsv,
            CheckCycleIdx,

            Finish,
            Alarm,
        }

        public List<NoiseInfo> NoiseSearch_Blob(MIL_ID SourceImg, double LowerThreadshold, double UpperThreadshold)
        {
            List<NoiseInfo> Result = new List<NoiseInfo>();

            MIL_INT Sign = MIL.MbufInquire(SourceImg, MIL.M_SIGN);
            MIL_INT Depth = MIL.MbufInquire(SourceImg, MIL.M_SIZE_BIT);
            MIL_INT Width = MIL.MbufInquire(SourceImg, MIL.M_SIZE_X);
            MIL_INT Height = MIL.MbufInquire(SourceImg, MIL.M_SIZE_Y);
            MIL_INT Band = MIL.MbufInquire(SourceImg, MIL.M_SIZE_BAND);

            MIL_ID BinImg = MIL.M_NULL;
            MIL_ID BlobContext = MIL.M_NULL;
            MIL_ID BlobResult = MIL.M_NULL;

            try
            {
                MIL.MbufAlloc2d(MyMil.MilSystem, Width, Height, Depth + Sign, MIL.M_IMAGE + MIL.M_PROC, ref BinImg);
                MIL.MbufClear(BinImg, 0L);
                MIL.MbufCopy(SourceImg, BinImg);

                // 再篩選出小於等於 upperThreshold 的像素
                MIL.MimBinarize(SourceImg, BinImg, MIL.M_FIXED + MIL.M_IN_RANGE, LowerThreadshold, UpperThreadshold);

                MIL.MblobAlloc(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobContext);
                MIL.MblobAllocResult(MyMil.MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref BlobResult);

                // Enable feature calculation.
                MIL.MblobControl(BlobContext, MIL.M_BOX, MIL.M_ENABLE);
                MIL.MblobControl(BlobContext, MIL.M_CENTER_OF_GRAVITY + MIL.M_BINARY, MIL.M_ENABLE);

                MIL.MblobCalculate(BlobContext, BinImg, MIL.M_NULL, BlobResult);

                MIL_INT TotalBlobs = 0;
                MIL.MblobGetResult(BlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref TotalBlobs);

                NoiseInfo[] _result = new NoiseInfo[TotalBlobs];

                double[] CogXs = new double[TotalBlobs];
                double[] CogYs = new double[TotalBlobs];
                MIL.MblobGetResult(BlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_X + MIL.M_BINARY, CogXs);
                MIL.MblobGetResult(BlobResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY_Y + MIL.M_BINARY, CogYs);

                for (int i = 0; i < TotalBlobs; i++)
                {
                    _result[i] = new NoiseInfo();

                    int X = (int)CogXs[i];
                    int Y = (int)CogYs[i];

                    _result[i].X = X;
                    _result[i].Y = Y;

                    if ((int)Band == 3)
                    {
                        int[] redValue = new int[1] { 0 };
                        int[] greenValue = new int[1] { 0 };
                        int[] blueValue = new int[1] { 0 };

                        MIL.MbufGetColor2d(SourceImg, MIL.M_SINGLE_BAND, MIL.M_RED, X, Y, 1, 1, redValue);
                        MIL.MbufGetColor2d(SourceImg, MIL.M_SINGLE_BAND, MIL.M_GREEN, X, Y, 1, 1, greenValue);
                        MIL.MbufGetColor2d(SourceImg, MIL.M_SINGLE_BAND, MIL.M_BLUE, X, Y, 1, 1, blueValue);

                        _result[i].Gray = (redValue.Average() + greenValue.Average() + blueValue.Average()) / 3.0;
                    }
                    else
                    {
                        int[] Value = new int[1];
                        MIL.MbufGet2d(SourceImg, X, Y, 1, 1, Value);

                        _result[i].Gray = Value.Average();
                    }
                }

                Result = _result.ToList();
            }
            catch (Exception ex)
            {
                SaveLog($"Noise Search Error : {ex.Message}", true);
            }
            finally
            {
                if (BinImg != MIL.M_NULL)
                {
                    MIL.MbufFree(BinImg);
                }

                if (BlobContext != MIL.M_NULL)
                {
                    MIL.MblobFree(BlobContext);
                    BlobContext = MIL.M_NULL;
                }

                if (BlobResult != MIL.M_NULL)
                {
                    MIL.MblobFree(BlobResult);
                    BlobResult = MIL.M_NULL;
                }
            }

            return Result;
        }

        public List<NoiseInfo> NoiseSearch_GetValue(MIL_ID SourceImg, double LowerThreadshold, double UpperThreadshold)
        {
            List<NoiseInfo> Result = new List<NoiseInfo>();

            MIL_INT Width = MIL.MbufInquire(SourceImg, MIL.M_SIZE_X);
            MIL_INT Height = MIL.MbufInquire(SourceImg, MIL.M_SIZE_Y);
            MIL_INT Band = MIL.MbufInquire(SourceImg, MIL.M_SIZE_BAND);

            MIL_ID ResultImg = MIL.M_NULL;

            try
            {
                MIL.MbufAlloc2d(MyMil.MilSystem, Width, Height, 32 + MIL.M_FLOAT, MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC, ref ResultImg);

                MIL.MbufCopy(SourceImg, ResultImg);

                float[] ImgData = new float[Width * Height * Band];

                MIL.MbufGet(ResultImg, ImgData);

                for (int i = 0; i < ImgData.Length; i++)
                {
                    bool InRange = (ImgData[i] >= LowerThreadshold && ImgData[i] <= UpperThreadshold);

                    if (InRange)
                    {
                        int X = i % (int)Width;
                        int Y = i / (int)Width;
                        float Gray = ImgData[i];

                        Result.Add(new NoiseInfo {
                            X = X,
                            Y = Y,
                            Gray = Gray,
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                SaveLog($"Noise Search Error : {ex.Message}", true);
            }
            finally
            {
                if (ResultImg != MIL.M_NULL)
                {
                    MIL.MbufFree(ResultImg);
                }
            }

            return Result;
        }


        private bool WriteRowData(string SavePath, RecordPara Para)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                if (!File.Exists(SavePath))
                {
                    SB.Append($"### ExposureTime = {Para.ExposureTime}" + "\r\n");
                    SB.Append($"### LowerThreashold = {Para.LowerThreashold}" + "\r\n");
                    SB.Append($"### UpperThreashold = {Para.UpperThreashold}" + "\r\n");

                    List<string> TitleList = new List<string>();
                    TitleList.Add("Cycle Index");
                    TitleList.Add("X");
                    TitleList.Add("Y");
                    TitleList.Add("Gray");
                    string Title = string.Join(",", TitleList);
                    SB.Append(Title + "\r\n");
                }

                //Info

                foreach (NoiseInfo Noise in Para.NoiseList)
                {
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{Para.CycleIdx}");
                    InfoList.Add($"{Noise.X}");
                    InfoList.Add($"{Noise.Y}");
                    InfoList.Add($"{Noise.Gray}");

                    string Info = string.Join(",", InfoList);
                    SB.Append(Info + "\r\n");
                }

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

        private bool WriteResult(string SavePath, List<Point>[] NoiseList)
        {
            try
            {
                // 使用 HashSet 來找出所有 List 共同存在的點
                HashSet<Point> commonPoints = new HashSet<Point>(NoiseList[0]);

                for (int i = 1; i < NoiseList.Length; i++)
                {
                    commonPoints.IntersectWith(NoiseList[i]); // 取交集
                }

                StringBuilder SB = new StringBuilder();

                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();
                    TitleList.Add("X");
                    TitleList.Add("Y");
                    string Title = string.Join(",", TitleList);
                    SB.Append(Title + "\r\n");
                }

                //Info

                foreach (Point Noise in commonPoints)
                {
                    List<string> InfoList = new List<string>();

                    InfoList.Add($"{Noise.X}");
                    InfoList.Add($"{Noise.Y}");

                    string Info = string.Join(",", InfoList);
                    SB.Append(Info + "\r\n");
                }

                StreamWriter SW = new StreamWriter(SavePath, true, encoding: Encoding.GetEncoding("GB2312"));
                SW.Write(SB);
                SW.Close();

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Write Result Fail : {ex.Message}");
                return false;
            }
        }


        public class RecordPara
        {
            public int CycleIdx { get; set; } = 0;
            public NoiseType Type { get; set; } = NoiseType.Dark;
            public double ExposureTime { get; set; } = 0.0;
            public double UpperThreashold { get; set; } = 0.0;
            public double LowerThreashold { get; set; } = 0.0;

            public List<NoiseInfo> NoiseList { get; set; } = new List<NoiseInfo>();
        }

        public class NoiseInfo
        {
            public int X { get; set; } = 0;
            public int Y { get; set; } = 0;
            public double Gray { get; set; } = 0.0;
        }

        public enum NoiseType
        {
            Dark,
            Bright,
            DarkAndBright,
        }
    }

    public class NoiseDetection_FlowPara
    {
        [Category("A. Base Align Flow"), DisplayName("01. Base Align FlowPara"), Description("Base Align FlowPara"), Browsable(false)]
        public BaseAlign_FlowPara BaseAlign_Para { get; set; } = new BaseAlign_FlowPara();

        [Category("A. Flow Para"), DisplayName("01. Output Folder"), Description("Output Folder"), ReadOnly(true), XmlIgnore]
        public string OutputFolder { get; set; } = @"D:\OMS\Noise Detection\";

        [Category("A. Flow Para"), DisplayName("02. Noise Type"), Description("Noise Type")]
        public Task_NoiseDetection.NoiseType Type { get; set; } = Task_NoiseDetection.NoiseType.Dark;

        [Category("A. Flow Para"), DisplayName("03. Cycle Count"), Description("Cycle Count")]
        public int CycleCount { get; set; } = 10;

        //Dark Noise
               
        [Category("B. Dark Noise Para"), DisplayName("01. ExpTime"), Description("ExpTime")]
        public double DarkNoise_ExpTime { get; set; } = 300000;

        [Category("B. Dark Noise Para"), DisplayName("02. Lower Threashold"), Description("Lower Threashold")]
        public double DarkNoise_LowerThreashold { get; set; } = 10;

        [Category("B. Dark Noise Para"), DisplayName("03. Upper Threashold"), Description("Upper Threashold")]
        public double DarkNoise_UpperThreashold { get; set; } = 4000;


        //Bright Noise

        [Category("C. Bright Noise Para"), DisplayName("01. ExpTime"), Description("ExpTime")]
        public double BrightNoise_ExpTime { get; set; } = 300000;

        [Category("C. Bright Noise Para"), DisplayName("02. Lower Threashold"), Description("Lower Threashold")]
        public double BrightNoise_LowerThreashold { get; set; } = 10;

        [Category("C. Bright Noise Para"), DisplayName("03. Upper Threashold"), Description("Upper Threashold")]
        public double BrightNoise_UpperThreashold { get; set; } = 4000;

        public void Clone(NoiseDetection_FlowPara Source)
        {
            this.BaseAlign_Para.Clone(Source.BaseAlign_Para);

            //A
            this.OutputFolder = Source.OutputFolder;
            this.Type = Source.Type;
            this.CycleCount = Source.CycleCount;

            //B
            this.DarkNoise_ExpTime = Source.DarkNoise_ExpTime;
            this.DarkNoise_LowerThreashold = Source.DarkNoise_LowerThreashold;
            this.DarkNoise_UpperThreashold = Source.DarkNoise_UpperThreashold;

            //C
            this.BrightNoise_ExpTime = Source.BrightNoise_ExpTime;
            this.BrightNoise_LowerThreashold = Source.BrightNoise_LowerThreashold;
            this.BrightNoise_UpperThreashold = Source.BrightNoise_UpperThreashold;
        }

        public static void WriteXML(NoiseDetection_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(NoiseDetection_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static NoiseDetection_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                NoiseDetection_FlowPara m;

                serializer = new XmlSerializer(typeof(NoiseDetection_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (NoiseDetection_FlowPara)serializer.Deserialize(fs);
                fs.Close();

                return m;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Read Xml Fail : {ex.Message}");

                return null;
            }
        }
    }


}
