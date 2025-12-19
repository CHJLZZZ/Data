using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommonBase.Logger;
using HardwareManager;
using static MyMilIp;
using BaseTool;
using FrameGrabber;
using System.ComponentModel;

namespace OpticalMeasuringSystem
{
    public class Task_AutoExpTime
    {
        public InfoManager infoManager = null;
        public MilDigitizer grabber = null;

        public bool IsDone = false;
        public string ErrorCode = "";

        public EnState State = EnState.Idle;

        private FlowResult Flow_Result;

        public FlowResult Result
        {
            get => this.Flow_Result;
        }

        public Task_AutoExpTime(cls29MSetting RD, int ipStep, InfoManager infoManager, MilDigitizer grabber)
        {
            this.Para = new AutoExpTime_FlowPara();
            this.Para.AutoExposureEnable = RD.AutoExposureEnable;
            this.Para.AutoExposureChangeStep = RD.AutoExposureChangeStep;
            this.Para.AutoExposureMinTime = RD.AutoExposureMinTime;
            this.Para.MaxExposureTime = RD.MaxExposureTime;
            this.Para.ExposureTimedDefault = RD.ExposureTimedDefault[ipStep];
            this.Para.AutoExposureTargetGray = RD.AutoExposureTargetGray[ipStep];
            this.Para.AutoExposureGrayTolerance = RD.AutoExposureGrayTolerance;
            this.Para.AutoExposureIsUseRoi = RD.AutoExposureIsUseRoi;
            this.Para.AutoExposureRoiX = RD.AutoExposureRoiX;
            this.Para.AutoExposureRoiY = RD.AutoExposureRoiY;

            this.Para.AutoExposureRoiWidth = RD.AutoExposureRoiWidth;
            this.Para.AutoExposureRoiHeight = RD.AutoExposureRoiHeight;
            this.Para.FixedExposureTime = RD.FixedExposureTime[ipStep];
            this.Para.IsSaveSourceImage = RD.AutoExposure_SaveImage;
            this.Para.IsSaveDebugImage = RD.IsSaveDebugImage;
            this.Para.HotPixelIsDoStep = RD.HotPixelIsDoStep;
            this.Para.HotPixel_Pitch = RD.HotPixel_Pitch[ipStep];
            this.Para.HotPixel_BTH = RD.HotPixel_BTH[ipStep];
            this.Para.HotPixel_DTH = RD.HotPixel_DTH[ipStep];

            switch (ipStep)
            {
                case 0:
                    this.Para.Gain = RD.CameraGainX;
                    break;

                case 1:
                    this.Para.Gain = RD.CameraGainY;
                    break;

                case 2:
                    this.Para.Gain = RD.CameraGainZ;
                    break;
            }

            this.ipStep = ipStep;
            this.infoManager = infoManager;
            this.grabber = grabber;
        }

        public Task_AutoExpTime(AutoExpTime_FlowPara Para, int ipStep, InfoManager infoManager, MilDigitizer grabber)
        {
            this.Para = new AutoExpTime_FlowPara();
            this.Para.AutoExposureEnable = Para.AutoExposureEnable;
            this.Para.AutoExposureChangeStep = Para.AutoExposureChangeStep;
            this.Para.AutoExposureMinTime = Para.AutoExposureMinTime;
            this.Para.MaxExposureTime = Para.MaxExposureTime;
            this.Para.ExposureTimedDefault = Para.ExposureTimedDefault;
            this.Para.AutoExposureTargetGray = Para.AutoExposureTargetGray;
            this.Para.AutoExposureGrayTolerance = Para.AutoExposureGrayTolerance;
            this.Para.AutoExposureIsUseRoi = Para.AutoExposureIsUseRoi;
            this.Para.AutoExposureRoiX = Para.AutoExposureRoiX;
            this.Para.AutoExposureRoiY = Para.AutoExposureRoiY;

            this.Para.AutoExposureRoiWidth = Para.AutoExposureRoiWidth;
            this.Para.AutoExposureRoiHeight = Para.AutoExposureRoiHeight;
            this.Para.FixedExposureTime = Para.FixedExposureTime;
            this.Para.IsSaveSourceImage = Para.IsSaveSourceImage;
            this.Para.IsSaveDebugImage = Para.IsSaveDebugImage;
            this.Para.HotPixelIsDoStep = Para.HotPixelIsDoStep;
            this.Para.HotPixel_Pitch = Para.HotPixel_Pitch;
            this.Para.HotPixel_BTH = Para.HotPixel_BTH;
            this.Para.HotPixel_DTH = Para.HotPixel_DTH;
            this.Para.Gain = Para.Gain;

            this.ipStep = ipStep;
            this.infoManager = infoManager;
            this.grabber = grabber;
        }

        public enum EnState
        {
            Idle,
            Init,
            Capture,
            IpRemoveHotPixel,
            AutoRoi,
            CheckGrayValue,
            PredictExposureTime,
            AutoExposureCapture,
            Finish,
            Alarm,
            Warning,
        }

        private AutoExpTime_FlowPara Para;

        private myTimer timer1 = new myTimer(); // 流程時間
        private myTimer timer2 = new myTimer(); // 存圖時間
        private myTimer timer3 = new myTimer(); // 自動曝光總時間

        private int ipStep = -1;
        private string xyz = "";
        private string nd = "";

        private int exposureTime = 0;
        private int cameraGain = 0;
        private int maxExposureTime = 0;
        private int currentExposureTime = 0;
        private int exposureStep = 0;   // 第一次自動曝光要增加多少
        private int oldGrayMean = 0;
        private int tryTimes = 0;
        private double targetGray = 0.0;
        private double errorTolerance = 0.0;

        private PixelInfo maxPixel;
        private int roiGrayMean = -1;

        private bool FlowRun = false;
        public bool IsRun
        {
            get => this.FlowRun;
        }

        public void Start(int SerialNo = 0)
        {
            if (!FlowRun)
            {
                Thread mThread = new Thread(() => FSM(SerialNo));
                mThread.Start();
            }
            else
            {
                FlowRun = false;
                Start(SerialNo);
            }
        }

        public void ShotDown()
        {
            FlowRun = false;
        }

        public void FSM(int SerialNo)
        {
            FlowRun = true;
            this.State = EnState.Init;

            while (FlowRun)
            {
                switch (this.State)
                {
                    case EnState.Idle:
                        {
                            this.ErrorCode = "";
                        }
                        break;

                    case EnState.Init:
                        {
                            if (this.Para.ExposureTimedDefault < this.Para.AutoExposureMinTime)
                            {
                                this.infoManager.Error($"Default ExposureTime < Min ExposureTime ");

                                this.State = EnState.Alarm;
                                break;
                            }

                            if (this.Para.ExposureTimedDefault > this.Para.MaxExposureTime)
                            {
                                this.infoManager.Error($"Default ExposureTime > Min ExposureTime ");

                                this.State = EnState.Alarm;
                                break;
                            }



                            this.Flow_Result = new FlowResult();

                            this.IsDone = false;
                            this.xyz = GlobalVar.ProcessInfo.XyzStr[this.ipStep];
                            this.nd = $"{GlobalVar.Device.NdPosition }";

                            this.State = EnState.Capture;
                        }
                        break;

                    case EnState.Capture:
                        {
                            timer1.Start();
                            timer3.Start();

                            if (this.Para.AutoExposureEnable)
                            {
                                oldGrayMean = 0;
                                maxExposureTime = this.Para.MaxExposureTime;
                                exposureTime = this.Para.ExposureTimedDefault;
                                targetGray = this.Para.AutoExposureTargetGray;
                                errorTolerance = this.Para.AutoExposureGrayTolerance;

                                if (this.Para.AutoExposureChangeStep > maxExposureTime)  //2023/07/05 Rick add
                                    this.Para.AutoExposureChangeStep = (int)(0.5 * maxExposureTime);
                            }
                            else
                            {
                                exposureTime = this.Para.FixedExposureTime;
                            }

                            cameraGain = this.Para.Gain;

                            currentExposureTime = exposureTime;
                            this.infoManager.Debug($"[{this.xyz}][{this.nd}][Capture] ExposureTime {exposureTime} us");

                            try
                            {
                                this.grabber.SetExposureTime(currentExposureTime);
                               

                                this.grabber.SetCameraGain(cameraGain);
                                this.grabber.Grab();
                            }
                            catch (Exception ex)
                            {
                                this.ErrorCode = $"[TaskAutoExposure] - [Capture] - {ex.Message}";
                                this.State = EnState.Alarm;
                            }

                            //// 設定要顯示的影像
                            //MyMilDisplay.SetDisplayImage(ref this.grabber.grabImage, 1);
                            //frmMain.IsNeedUpdate = true;

                            timer1.Stop();
                            this.infoManager.Debug($"[{this.xyz}][{this.nd}][Capture] {timer1.timeSpend} ms");

                            this.State = EnState.IpRemoveHotPixel;
                        }
                        break;

                    case EnState.IpRemoveHotPixel:
                        {
                            if (this.Para.HotPixelIsDoStep)
                            {
                                timer1.Start();

                                int pixelPitch = this.Para.HotPixel_Pitch;
                                double brightThreshold = this.Para.HotPixel_BTH;
                                double darkThreshold = this.Para.HotPixel_DTH;
                                HotPixel(ref this.grabber.grabImage, pixelPitch, brightThreshold, darkThreshold);

                                DebugSaveImage($"[{this.xyz}] IpRemoveHotPixel", this.grabber.grabImage);

                                timer1.Stop();
                                this.infoManager.Debug($"[{this.xyz}][IpRemoveHotPixel] {timer1.timeSpend} ms");
                            }

                            State = EnState.AutoRoi;
                        }
                        break;

                    case EnState.AutoRoi:
                        {
                            timer1.Start();

                            int[] temp = new int[1] { 0 };

                            if (this.Para.AutoExposureIsUseRoi)
                            {
                                System.Drawing.Rectangle roi = new System.Drawing.Rectangle();
                                roi.X = this.Para.AutoExposureRoiX;
                                roi.Y = this.Para.AutoExposureRoiY;
                                roi.Width = this.Para.AutoExposureRoiWidth;
                                roi.Height = this.Para.AutoExposureRoiHeight;

                                if (roi.X + roi.Width > this.grabber.MaxWidth ||
                                    roi.Y + roi.Height > this.grabber.MaxHeight)
                                {
                                    this.ErrorCode = "Roi out of Image";
                                    this.State = EnState.Alarm;

                                    break;
                                }

                                maxPixel = AutoRoiBrightestPoint(this.grabber.grabImage, roi);
                            }
                            else
                            {
                                maxPixel = AutoRoiBrightestPoint(this.grabber.grabImage);
                            }
                            this.infoManager.Network($"[{this.xyz}][AutoRoi] Use Point: {maxPixel.X}, {maxPixel.Y}");

                            roiGrayMean = maxPixel.MaxPixel;

                            timer1.Stop();
                            this.infoManager.Debug($"[{this.xyz}][AutoRoi] {timer1.timeSpend} ms");
                            this.infoManager.Network($"[{this.xyz}][AutoRoi] roiGrayMean = {roiGrayMean}");

                            if (this.Para.AutoExposureEnable)
                                this.State = EnState.CheckGrayValue;
                            else
                                this.State = EnState.Finish;
                        }
                        break;

                    case EnState.CheckGrayValue:
                        {
                            timer1.Start();

                            int minExposureTime = this.Para.AutoExposureMinTime;
                            int grayUpperBound = Convert.ToInt32(targetGray * (1 + errorTolerance));
                            double ErrorRatio = Math.Abs(roiGrayMean - targetGray) / targetGray;

                            this.Flow_Result.ExpTime = currentExposureTime;
                            this.Flow_Result.GrayMean = roiGrayMean;

                            if (roiGrayMean == 4095)
                                System.Threading.Thread.Sleep(10);

                            if (roiGrayMean > grayUpperBound && currentExposureTime == minExposureTime)
                            {
                                // 目前曝光時間 == 最小曝光時間 還是過曝
                                this.infoManager.Network($"[{this.xyz}][CheckGrayValue] ExposureTime = {currentExposureTime}, GrayValue = {roiGrayMean}, auto exposure failed");
                                this.ErrorCode = $"[TaskAutoExposure] - [Capture] - {"auto exposure failed : At the minimum exposure time, it is still overexposed."}";
                                this.State = EnState.Warning;
                            }
                            else if (currentExposureTime == this.Para.MaxExposureTime)
                            {
                                if (roiGrayMean > targetGray)
                                {
                                    this.infoManager.Network($"[{this.xyz}][CheckGrayValue] Go to predict exposure time");
                                    this.State = EnState.PredictExposureTime;
                                }
                                else
                                {
                                    this.infoManager.Network($"[{this.xyz}][CheckGrayValue] ExposureTime = {currentExposureTime}, GrayValue = {roiGrayMean}, use maximum exposure time");
                                    this.State = EnState.Finish;

                                    this.Flow_Result.ExpTime = currentExposureTime;
                                    this.Flow_Result.GrayMean = roiGrayMean;

                                }
                            }
                            else if (ErrorRatio <= errorTolerance)   // 誤差 < 容忍值
                            {
                                // 誤差 < 容忍 -> 自動曝光完成
                                this.infoManager.Network($"[{this.xyz}][CheckGrayValue] Final Exposure Time = {currentExposureTime}");
                                this.State = EnState.Finish;


                                this.Flow_Result.ExpTime = currentExposureTime;
                                this.Flow_Result.GrayMean = roiGrayMean;
                            }
                            else
                            {
                                this.infoManager.Network($"[{this.xyz}][CheckGrayValue] Go to predict exposure time");
                                this.State = EnState.PredictExposureTime;
                            }

                            timer1.Stop();
                            this.infoManager.Debug($"[{this.xyz}][CheckGrayValue] {timer1.timeSpend} ms");
                        }
                        break;

                    case EnState.PredictExposureTime:
                        {
                            if (tryTimes > 30)
                            {
                                this.infoManager.Error($"[{this.xyz}][PredictExposureTime] Predict exposure time falied");
                                this.State = EnState.Alarm;
                                break;
                            }

                            timer1.Start();

                            int minExposureTime = this.Para.AutoExposureMinTime;
                            double factor = 0.0;
                            double diff = 0.0;

                            if (oldGrayMean != 0 && exposureStep != 0 && roiGrayMean != oldGrayMean)
                            {
                                int GrayOffset = Math.Abs(roiGrayMean - oldGrayMean);
                                //1單位時間可影響多少灰階
                                factor = (double)GrayOffset / (double)exposureStep;
                                exposureStep = (int)(Math.Abs(targetGray - roiGrayMean) / factor);

                                this.infoManager.Network($"[{this.xyz}][PredictExposureTime] extrapolation exposureStep = {exposureStep}");
                            }
                            else if (exposureStep == 0)
                            {
                                if (currentExposureTime <= 0) currentExposureTime = this.Para.AutoExposureMinTime;
                                factor = (double)roiGrayMean / (double)currentExposureTime; // 1單位時間影響多少灰階
                                diff = Math.Abs((double)targetGray - (double)roiGrayMean);  // 距離目標差多少灰階
                                if (factor <= 1) factor = 1;

                                exposureStep = Convert.ToInt32(diff / factor);

                                this.infoManager.Network($"[{this.xyz}][PredictExposureTime] scale exposureStep = {exposureStep}");
                            }

                            if (targetGray > roiGrayMean) //目標>當前
                            {
                                currentExposureTime += exposureStep;
                                this.infoManager.Network($"[{this.xyz}][PredictExposureTime] newExposureTime = {currentExposureTime}");

                                if (currentExposureTime >= maxExposureTime)
                                {
                                    currentExposureTime = maxExposureTime;
                                    this.infoManager.Network($"[{this.xyz}][PredictExposureTime] currentExposureTime > maxExposureTime, use maxExposureTime({maxExposureTime} ms).");
                                    //this.State = EnState.AutoExposure;
                                }
                            }
                            else //當前>目標
                            {
                                currentExposureTime -= exposureStep;
                                this.infoManager.Network($"[{this.xyz}][PredictExposureTime] extrapolation exposureStep = {exposureStep}");

                                if (currentExposureTime <= minExposureTime)
                                {
                                    currentExposureTime = minExposureTime;
                                }
                            }

                            if (oldGrayMean <= this.grabber.MaxGrayScale)
                            {
                                oldGrayMean = roiGrayMean;
                            }

                            //if (roiGrayMean < this.grabber.MaxGrayScale)
                            //{
                            //    oldGrayMean = roiGrayMean;
                            //}


                            timer1.Stop();
                            this.infoManager.Debug($"[{this.xyz}][PredictExposureTime] {timer1.timeSpend} ms");

                            this.State = EnState.AutoExposureCapture;
                        }
                        break;

                    case EnState.AutoExposureCapture:
                        {
                            timer1.Start();

                            tryTimes++;
                            this.grabber.SetExposureTime(currentExposureTime);
                            this.infoManager.Debug($"[{this.xyz}][AutoExposureCapture] ExposureTime {currentExposureTime} us");

                            try
                            {
                                this.grabber.Grab();
                            }
                            catch (Exception ex)
                            {
                                this.ErrorCode = $"[TaskAutoExposure] - [Capture] - {ex.Message}";
                                this.State = EnState.Alarm;
                            }

                            // 設定要顯示的影像
                            MyMilDisplay.SetDisplayImage(ref this.grabber.grabImage, ipStep);

                            timer1.Stop();
                            this.infoManager.Debug($"[{this.xyz}][AutoExposureCapture] {timer1.timeSpend} ms");

                            this.State = EnState.IpRemoveHotPixel;
                        }
                        break;

                    case EnState.Finish:
                        {
                            timer1.Start();

                            tryTimes = 0;
                            oldGrayMean = 0;
                            exposureStep = 0;
                            GlobalVar.ProcessInfo.PatternNdPos[this.ipStep] = GlobalVar.Device.NdPosition;

                            if (this.Para.IsSaveSourceImage)
                            {
                                timer2.Start();
                                string ImgName = $"{GlobalVar.ProcessInfo.Pattern}_{GlobalVar.ProcessInfo.XyzStr[this.ipStep]}_Grab.tif";
                                MIL.MbufSave(GlobalVar.Config.DirectoryPath + @"\" + ImgName, this.grabber.grabImage);
                                timer2.Stop();
                                this.infoManager.Debug($"[{this.xyz}][CaptureFinish] Save Source Image {timer2.timeSpend} ms");
                            }

                            GlobalVar.ProcessInfo.XyzExposureTime[this.ipStep] = currentExposureTime;

                            timer1.Stop();
                            timer3.Stop();
                            this.infoManager.Debug($"[{this.xyz}][Finish] {timer1.timeSpend} ms");
                            this.infoManager.Debug($"[{this.xyz}][AutoExposureTotalTime] {timer3.timeSpend} ms");

                            this.IsDone = true;

                            FlowRun = false;

                        }
                        break;

                    case EnState.Alarm:
                        {
                            this.IsDone = true;
                            this.infoManager.Error(ErrorCode);
                            FlowRun = false;
                        }
                        break;

                    case EnState.Warning:
                        {
                            this.IsDone = true;
                            FlowRun = false;

                            //this.State = EnState.Idle;
                        }
                        break;
                }

                Thread.Sleep(1);
            }
        }

        private void DebugSaveImage(string filename, MIL_ID image)
        {
            if (this.Para.IsSaveDebugImage)
            {
                MIL.MbufSave($@"{GlobalVar.Config.DirectoryPath}\{filename}.tif", image);
            }
        }

        private void CheckState(bool isRun)
        {
            if (!isRun)
            {
                this.State = EnState.Finish;
                this.infoManager.General("TaskOneColorCalibration failed. User stop the process.");
            }
        }

        public class FlowResult
        {
            public double GrayMean = 0.0;
            public double ExpTime = 0.0;

            public FlowResult()
            {

            }
        }
    }

    public class AutoExpTime_FlowPara
    {
        [Category("03. 自動曝光"), DisplayName("01. 啟動自動曝光"), Description("執行: true;\t不執行: false")]
        public bool AutoExposureEnable { get; set; } = true;

        [Category("06. Camera"), DisplayName("02. Gain"), Description("X,Y,Z,W")]
        public int Gain { get; set; } = 1;

        [Category("03. 自動曝光"), DisplayName("03. 首次調整曝光時間"), Description("單步調整曝光時間量(ns)")]
        public int AutoExposureChangeStep { get; set; } = 100;

        [Category("03. 自動曝光"), DisplayName("04. 最小曝光時間"), Description("最小曝光時間(ns)")]
        public int AutoExposureMinTime { get; set; } = 300000;

        [Category("03. 自動曝光"), DisplayName("05. 最大曝光時間"), Description("最大曝光時間(ns)")]
        public int MaxExposureTime { get; set; } = 20000;

        [Category("03. 自動曝光"), DisplayName("06. 預設曝光時間"), Description("初始曝光時間(ns)")]
        public int ExposureTimedDefault { get; set; } = 100000;

        [Category("03. 自動曝光"), DisplayName("07. 目標灰階(R)"), Description("最亮點目標灰階值")]
        public int AutoExposureTargetGray { get; set; } = 3480;

        [Category("03. 自動曝光"), DisplayName("08. 灰階誤差容許範圍"), Description("最亮點與目標灰階值容許誤差 (0.00 ~ 1.00), X. Y. Z filter 通用")]
        public double AutoExposureGrayTolerance { get; set; } = 0.05;

        [Category("03. 自動曝光"), DisplayName("11. 是否使用ROI)"), Description("使用: true;\t不使用: false")]
        public bool AutoExposureIsUseRoi { get; set; } = false;

        [Category("03. 自動曝光"), DisplayName("12. ROI Start X"), Description("ROI左上頂點X座標")]
        public int AutoExposureRoiX { get; set; } = 1;

        [Category("03. 自動曝光"), DisplayName("13. ROI Start Y"), Description("ROI左上頂點Y座標")]
        public int AutoExposureRoiY { get; set; } = 1;

        [Category("03. 自動曝光"), DisplayName("14. ROI Width"), Description("ROI寬")]
        public int AutoExposureRoiWidth { get; set; } = 9000;

        [Category("03. 自動曝光"), DisplayName("15. ROI Height"), Description("ROI高")]
        public int AutoExposureRoiHeight { get; set; } = 6000;

        [Category("03. 自動曝光"), DisplayName("21. 固定曝光時間"), Description("固定曝光時間 (us)")]
        public int FixedExposureTime { get; set; } = 100000;

        [Category("01. 一般"), DisplayName("22. 自動曝光存圖"), Description("存出自動曝光結果影像")]
        public bool IsSaveSourceImage { get; set; } = true;

        [Category("01. 一般"), DisplayName("23. 偵錯存圖"), Description("存出偵錯處理影像")]
        public bool IsSaveDebugImage { get; set; } = true;

        [Category("06. Hot Pixel"), DisplayName("50. 噪點移除流程"), Description("執行: true;\t不執行: false"), Browsable(false)]
        public bool HotPixelIsDoStep { get; set; } = false;

        [Category("06. Hot Pixel"), DisplayName("51. Pitch"), Description("X,Y,Z,W"), Browsable(false)]
        public int HotPixel_Pitch { get; set; } = 1;

        [Category("06. Hot Pixel"), DisplayName("52. B_TH"), Description("X,Y,Z,W"), Browsable(false)]
        public double HotPixel_BTH { get; set; } = 1;

        [Category("06. Hot Pixel"), DisplayName("53. D_TH"), Description("X,Y,Z,W"), Browsable(false)]
        public double HotPixel_DTH { get; set; } = 1;

        public void Clone(AutoExpTime_FlowPara Source)
        {
            this.AutoExposureEnable = Source.AutoExposureEnable;
            this.Gain = Source.Gain;
            this.AutoExposureChangeStep = Source.AutoExposureChangeStep;
            this.AutoExposureMinTime = Source.AutoExposureMinTime;
            this.MaxExposureTime = Source.MaxExposureTime;
            this.ExposureTimedDefault = Source.ExposureTimedDefault;
            this.AutoExposureTargetGray = Source.AutoExposureTargetGray;
            this.AutoExposureGrayTolerance = Source.AutoExposureGrayTolerance;
            this.AutoExposureIsUseRoi = Source.AutoExposureIsUseRoi;
            this.AutoExposureRoiX = Source.AutoExposureRoiX;
            this.AutoExposureRoiY = Source.AutoExposureRoiY;
            this.AutoExposureRoiWidth = Source.AutoExposureRoiWidth;
            this.AutoExposureRoiHeight = Source.AutoExposureRoiHeight;
            this.FixedExposureTime = Source.FixedExposureTime;
            this.IsSaveSourceImage = Source.IsSaveSourceImage;
            this.IsSaveDebugImage = Source.IsSaveDebugImage;
            this.HotPixelIsDoStep = Source.HotPixelIsDoStep;
            this.HotPixel_Pitch = Source.HotPixel_Pitch;
            this.HotPixel_BTH = Source.HotPixel_BTH;
            this.HotPixel_DTH = Source.HotPixel_DTH;
        }


    }


}
