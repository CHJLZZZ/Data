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
    public class Task_AutoPattern
    {
        public event Action<string> MonitorStep;
        public event Action<string> LogMsg;

        //Device
        private InfoManager Info = null;
        private HardwareUnit Hardware = null;
        private AutoPattern_FlowPara FlowPara = new AutoPattern_FlowPara();

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

        public Task_AutoPattern(InfoManager Info, HardwareUnit Hardware)
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

        public void Start(AutoPattern_FlowPara SourcePara)
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

        public void Flow(AutoPattern_FlowPara Para)
        {
            TimeManager TM = new TimeManager();
            TimeManager TM_FW_Move = new TimeManager();

            NowStep = FlowStep.Check_Device;
            FlowRun = true;

            Task_AutoExpTime AutoExpTime = null;
            AutoExpTime_FlowPara AutoExpTime_Para = new AutoExpTime_FlowPara();

            string OutputFolder = $@"D:\OMS\AutoPattern\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            string[] XYZ_Ary = { "X", "Y", "Z" };

            int XYZ_Idx = 0;
            int PAT_Idx = 0;

            List<string> PAT_List = new List<string>();

            if (Para.PatternSourceType == AutoPattern_FlowPara.SourceType.Image)
            {
                if (Para.ImageSize.X <= 0 || Para.ImageSize.Y <= 0)
                {
                    SaveLog($"ImageSize Error ({Para.ImageSize.X} x {Para.ImageSize.Y})", true);
                    return;
                }
            }

            double[] ExposureTime = new double[3];

            switch (Para.PatternSourceType)
            {
                case AutoPattern_FlowPara.SourceType.Image:
                    {
                        foreach (string Folder in Para.SourceFolder)
                        {
                            if (Folder == null) continue;
                            if (Folder.Trim() == "") continue;

                            if (Directory.Exists(Folder))
                            {
                                // 搜尋所有子資料夾，並過濾副檔名
                                string[] allFiles = Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories);

                                foreach (string file in allFiles)
                                {
                                    string ext = Path.GetExtension(file).ToLower();
                                    if (ext == ".jpg" || ext == ".bmp" || ext == ".png")
                                    {
                                        PAT_List.Add(file);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case AutoPattern_FlowPara.SourceType.csv:
                    {
                        foreach (string Folder in Para.SourceFolder)
                        {
                            if (Folder == null) continue;
                            if (Folder.Trim() == "") continue;

                            if (Directory.Exists(Folder))
                            {
                                // 搜尋所有子資料夾，並過濾副檔名
                                string[] allFiles = Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories);

                                foreach (string file in allFiles)
                                {
                                    string ext = Path.GetExtension(file).ToLower();
                                    if (ext == ".csv")
                                    {
                                        string[] CsvLines = File.ReadAllLines(file);

                                        foreach (string CsvLine in CsvLines)
                                        {
                                            string[] SplitMsg = CsvLine.Split(',');

                                            if (SplitMsg.Length < 3) continue;

                                            int R, G, B;
                                            int W = Para.ImageSize.X;
                                            int H = Para.ImageSize.Y;
                                            bool ConvertR = int.TryParse(SplitMsg[0], out R);
                                            bool ConvertG = int.TryParse(SplitMsg[1], out G);
                                            bool ConvertB = int.TryParse(SplitMsg[2], out B);

                                            if (ConvertR && ConvertG && ConvertB)
                                            {
                                                string folderPath = Path.GetDirectoryName(file);
                                                string PatternPath = $@"{folderPath}\R[{R}]_G[{G}]_B[{B}].bmp";
                                                bool Rtn = GenerateColorImages(PatternPath, R, G, B, W, H);

                                                if (Rtn) PAT_List.Add(PatternPath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }


            PAT_List = PAT_List.Distinct().ToList();

            while (FlowRun)
            {
                switch (NowStep)
                {
                    case FlowStep.Check_Device:
                        {
                            bool Check = true;

                            //Check FW
                            bool Check_FW = true;
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        if (GlobalVar.Device.XyzFilter == null)
                                        {
                                            SaveLog($"Check Device Fail : FW is null", true);
                                            Check_FW = false;
                                            break;
                                        }

                                        if (!GlobalVar.Device.XyzFilter.is_FW_Work)
                                        {
                                            SaveLog($"Check Device Fail : FW is not Work", true);
                                            Check_FW = false;
                                            break;
                                        }
                                    }
                                    break;
                            }

                            Check &= Check_FW;

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

                            //Check Filter List
                            bool Check_PAT_List = (PAT_List.Count > 0);
                            if (!Check_PAT_List)
                            {
                                SaveLog($"Check Parameter Fail : Pattern List Count = 0", true);
                            }
                            Check &= Check_PAT_List;

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

                            NowStep = FlowStep.Set_Pattern;
                        }
                        break;

                    case FlowStep.Set_Pattern:
                        {
                            ExposureTime = new double[3];

                            string ImgPath = PAT_List[PAT_Idx];
                            string PatternName = Path.GetFileNameWithoutExtension(PAT_List[PAT_Idx]);

                            bool Rtn = DesktopManager.SetBackgroud(ImgPath);

                            if (Rtn)
                            {
                                SaveLog($"Set Pattern : {PatternName}");

                                if (Para.BypassAIC)
                                {
                                    SaveLog($"Bypass AIC , Delay : {Para.BypassAIC_DelayTime} ms");
                                    Thread.Sleep(Para.BypassAIC_DelayTime);

                                    NowStep = FlowStep.Check_PAT_Idx;
                                }
                                else
                                {
                                    Thread.Sleep(1000);
                                    XYZ_Idx = 0;

                                    NowStep = FlowStep.Filter_Change;
                                }
                            }
                            else
                            {
                                SaveLog($"Set Pattern Fail : {PatternName}",true);
                                WriteRawData_ErrorReport($@"{OutputFolder}\ErrorReport.csv", PAT_List[PAT_Idx], -1);
                                NowStep = FlowStep.Check_PAT_Idx;
                            }
                        }
                        break;

                    case FlowStep.Filter_Change:
                        {
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        GlobalVar.Device.XyzFilter.ChangeWheel(XYZ_Idx);
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        this.Hardware.Motor.Move(-1, -1, XYZ_Idx, -1);
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                    }
                                    break;
                            }

                            TM_FW_Move.SetDelay(60 * 1000);

                            SaveLog($"Filter Change : XYZ = {XYZ_Idx}");

                            NowStep = FlowStep.Check_Filter_Change;
                        }
                        break;

                    case FlowStep.Check_Filter_Change:
                        {
                            bool XyzReady = false;

                            // Change Wheel Ready
                            switch (GlobalVar.DeviceType)
                            {
                                case EnumMotoControlPackage.Ascom_Wheel_Airy_Motor:
                                case EnumMotoControlPackage.Ascom_Wheel_Auo_Motor:
                                    {
                                        XyzReady = GlobalVar.Device.XyzFilter.IsReady;
                                    }
                                    break;

                                case EnumMotoControlPackage.Airy_4In1_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_4In1_TCPIP_Wheel_Motor:
                                case EnumMotoControlPackage.Airy_211_USB_Wheel_Motor:
                                case EnumMotoControlPackage.CircleTac:
                                    {
                                        if (!this.Hardware.Motor.IsMoveProc())
                                        {
                                            XyzReady = (this.Hardware.Motor.MoveFlowStatus() == UnitStatus.Finish && this.Hardware.Motor.MoveStatus_FW1() == UnitStatus.Finish);
                                        }
                                    }
                                    break;

                                case EnumMotoControlPackage.MS5515M:
                                    {
                                        XyzReady = true;
                                    }
                                    break;
                            }

                            if (XyzReady)
                            {
                                SaveLog($"Filter Change Finish");
                                NowStep = FlowStep.AutoExposureTime;
                            }
                            else
                            {
                                if (TM_FW_Move.IsTimeOut())
                                {
                                    SaveLog($"Filter Change Timeout", true);
                                    NowStep = FlowStep.Alarm;
                                }
                            }
                        }
                        break;

                    case FlowStep.AutoExposureTime:
                        {
                            AutoExpTime = new Task_AutoExpTime(Para.AutoExpTime_Para, XYZ_Idx, this.Info, this.Hardware.Grabber);
                            AutoExpTime.Start(XYZ_Idx);

                            string XYZ = XYZ_Ary[XYZ_Idx];
                            string PAT = Path.GetFileNameWithoutExtension(PAT_List[PAT_Idx]);
                            string Remark = $"{PAT}_{XYZ}";

                            SaveLog($"[{Remark}] Auto ExposureTime Start");

                            Thread.Sleep(100);

                            NowStep = FlowStep.Check_AutoExposureTime;
                        }
                        break;

                    case FlowStep.Check_AutoExposureTime:
                        {
                            if (!AutoExpTime.IsRun)
                            {
                                string XYZ = XYZ_Ary[XYZ_Idx];
                                string PAT = Path.GetFileNameWithoutExtension(PAT_List[PAT_Idx]);
                                string Remark = $"{PAT}_{XYZ}";

                                switch (AutoExpTime.State)
                                {
                                    case Task_AutoExpTime.EnState.Finish:
                                        {
                                            ExposureTime[XYZ_Idx] = AutoExpTime.Result.ExpTime;

                                            SaveLog($"[{Remark}] Auto ExposureTime Finish");
                                            NowStep = FlowStep.Capture;
                                        }
                                        break;

                                    case Task_AutoExpTime.EnState.Warning:
                                    case Task_AutoExpTime.EnState.Alarm:
                                        {
                                            ExposureTime[XYZ_Idx] = -1;

                                            WriteRawData_ErrorReport($@"{OutputFolder}\ErrorReport.csv", PAT_List[PAT_Idx], XYZ_Idx);
                                            SaveLog($"[{Remark}] Auto ExposureTime Fail", true);
                                            NowStep = FlowStep.Check_PAT_Idx;
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case FlowStep.Capture:
                        {
                            string XYZ = XYZ_Ary[XYZ_Idx];
                            string PAT = Path.GetFileNameWithoutExtension(PAT_List[PAT_Idx]);
                            string Remark = $"{PAT}_{XYZ}";

                            // 取得最後一層資料夾名稱
                            string folderName = new DirectoryInfo(Path.GetDirectoryName(PAT_List[PAT_Idx])).Name;

                            // 合併：資料夾名稱 
                            string SavePath = $@"{OutputFolder}\{folderName}\{PAT}\";
                            Directory.CreateDirectory(SavePath);

                            double ExpTime = AutoExpTime.Result.ExpTime;
                            this.Hardware.Grabber.SetExposureTime(ExpTime);
                            this.Hardware.Grabber.Grab();

                            SaveLog($"[{Remark}] Capture,  ExpTime = {ExpTime}");

                            string ImgPath = $@"{SavePath}\{XYZ}.tif";

                            MIL.MbufSave(ImgPath, this.Hardware.Grabber.grabImage);

                            NowStep = FlowStep.Check_XYZ_Idx;
                        }
                        break;

                    case FlowStep.Check_XYZ_Idx:
                        {
                            XYZ_Idx++;

                            if (XYZ_Idx < 3)
                            {
                                NowStep = FlowStep.Filter_Change;
                            }
                            else
                            {
                                NowStep = FlowStep.Check_PAT_Idx;
                            }
                        }
                        break;

                    case FlowStep.Check_PAT_Idx:
                        {
                            WriteRawData($@"{OutputFolder}\RawData.csv", PAT_List[PAT_Idx], ExposureTime);

                            PAT_Idx++;

                            if (PAT_Idx < PAT_List.Count)
                            {
                                NowStep = FlowStep.Set_Pattern;
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
        }

        public enum FlowStep
        {
            Check_Device = 0,
            Check_Parameter,
            Init,

            Set_Pattern,
            Filter_Change,
            Check_Filter_Change,
            AutoExposureTime,
            Check_AutoExposureTime,

            Capture,

            Check_XYZ_Idx,
            Check_PAT_Idx,

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

        private bool WriteRawData(string SavePath, string Pattern, double[] ExposureTime)
        {
            try
            {
                StringBuilder SB = new StringBuilder();

                //Title
                if (!File.Exists(SavePath))
                {
                    List<string> TitleList = new List<string>();

                    TitleList.Add("Pattern Path");
                    TitleList.Add("ExposureTime - X");
                    TitleList.Add("ExposureTime - Y");
                    TitleList.Add("ExposureTime - Z");

                    string Title = string.Join(",", TitleList);

                    SB.Append(Title + "\r\n");
                }

                //Info
                List<string> InfoList = new List<string>();

                InfoList.Add($"{Pattern}");
                InfoList.Add($"{ExposureTime[0]}");
                InfoList.Add($"{ExposureTime[1]}");
                InfoList.Add($"{ExposureTime[2]}");

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

        private bool GenerateColorImages(string SavePath, int R, int G, int B, int width, int height)
        {
            try
            {
                Color color;

                color = Color.FromArgb(R, G, B);

                using (Bitmap bmp = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(color);
                    //bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Bmp);
                    bmp.Save(SavePath, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                return true;
            }
            catch (Exception ex)
            {
                SaveLog($"Create Image Fail : {ex.Message}", true);
                return false;
            }
        }


        #endregion

    }

    public class AutoPattern_FlowPara
    {
        [Category("A. Pattern 來源"), DisplayName("01. 來源類型")]
        public SourceType PatternSourceType { get; set; } = SourceType.Image;

        [Category("A. Pattern 來源"), DisplayName("02. Pattern 資料夾")]
        public string[] SourceFolder { get; set; } = new string[10];

        [Category("A. Pattern 來源"), DisplayName("03. Pattern大小 (Image Type)")]
        public Point ImageSize { get; set; } = new Point(480, 270);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("B. 拍照"), DisplayName("01. 自動曝光參數")]
        public AutoExpTime_FlowPara AutoExpTime_Para { get; set; } = new AutoExpTime_FlowPara();

        [Category("C. Bypass AIC"), DisplayName("01. Bypass")]
        public bool BypassAIC { get; set; } = false;

        [Category("C. Bypass AIC"), DisplayName("02. Delay Time (ms)")]
        public int BypassAIC_DelayTime { get; set; } = 5000;

        public void Clone(AutoPattern_FlowPara Source)
        {
            //A
            this.PatternSourceType = Source.PatternSourceType;
            this.SourceFolder = Source.SourceFolder;
            this.ImageSize = Source.ImageSize;

            //B
            this.AutoExpTime_Para.Clone(Source.AutoExpTime_Para);

            //C
            this.BypassAIC = Source.BypassAIC;
            this.BypassAIC_DelayTime = Source.BypassAIC_DelayTime;
        }

        public static void WriteXML(AutoPattern_FlowPara m, string fileName)
        {
            try
            {
                XmlSerializer serializer;
                StreamWriter sw;

                serializer = new XmlSerializer(typeof(AutoPattern_FlowPara));
                sw = new StreamWriter(fileName);
                serializer.Serialize(sw, m);

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Write Xml Fail : {ex.Message}");
            }
        }

        public static AutoPattern_FlowPara ReadXML(string fileName)
        {
            try
            {
                XmlSerializer serializer;
                FileStream fs;
                AutoPattern_FlowPara m;

                serializer = new XmlSerializer(typeof(AutoPattern_FlowPara));
                fs = new FileStream(fileName, FileMode.Open);
                m = (AutoPattern_FlowPara)serializer.Deserialize(fs);
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
