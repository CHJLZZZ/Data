using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using CommonBase.Logger;

using LightMeasure;
using BaseTool;

namespace OpticalMeasuringSystem
{
    class TaskBaseCalibration
    {
        public InfoManager infoManager;
        public bool IsDone = false;
        public string ErrorCode = "";

        public EnState State = EnState.Idel;
        public MIL_ID FlowImage;

        private BaseCalibrationId calibrationData;
        public TaskBaseCalibration(cls29MSetting param, BaseCalibrationId calibrationData, ref MIL_ID image, int ipStep, InfoManager infoManager)
        {
            this.param = param;
            this.ipStep = ipStep;
            this.calibrationData = calibrationData;
            this.infoManager = infoManager;

            if (this.FlowImage != MIL.M_NULL)
            {
                MIL.MbufFree(this.FlowImage);
                this.FlowImage = MIL.M_NULL;
            }

            MIL.MbufClone(image, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, ref this.FlowImage);
            MIL.MbufCopy(image, this.FlowImage);

            //MIL.MbufSave(@"D:\SrcImage.tif", image);
            //MIL.MbufSave(@"D:\FlowImage.tif", this.FlowImage);
        }

        public enum EnState
        {
            Idel,
            Init,
            IpRemoveHotPixel,
            IpFFC,
            IpCalibration,
            IpWarpingCalibration,
            IpLaserCalibration,
            SaveImage,
            Finish,
            Alarm,
        }

        private cls29MSetting param;
        private Thread myThread;

        private string recipePath = "";
        private string directoryPath = "";
        private string pattern = "";
        private string xyz = "";
        private int ipStep = -1;

        private myTimer timer1 = new myTimer(); // 流程時間
        private myTimer timer2 = new myTimer(); // 存圖時間
        private myTimer timer3 = new myTimer(); // 自動曝光總時間       

        public void Run()
        {
            this.IsDone = false;
            this.myThread = new Thread(FSM);
            this.myThread.Start();
        }

        public void ShotDown()
        {
            this.myThread.Abort();
        }

        private void FSM()
        {
            while (!this.IsDone)
            {
                switch (State)
                {
                    case EnState.Idel:
                        {
                            this.ErrorCode = "";
                        }
                        break;

                    case EnState.Init:
                        {
                            this.recipePath = GlobalVar.Config.RecipePath;
                            this.directoryPath = GlobalVar.Config.DirectoryPath;
                            this.pattern = GlobalVar.ProcessInfo.Pattern;
                            this.xyz = GlobalVar.ProcessInfo.XyzStr[this.ipStep];

                            State = EnState.IpFFC;
                        }
                        break;

                    case EnState.IpFFC:
                        {
                            if (this.param.FfcIsDoStep)
                            {
                                timer1.Start();

                                try
                                {
                                    //DebugSaveImage($"{this.pattern}_{this.xyz}_FFC_Before", FlowImage);
                                    MyMilIp.FFC(FlowImage, this.calibrationData.Offset, this.calibrationData.Flat, this.calibrationData.Dark, ref FlowImage);
                                    DebugSaveImage($"{this.pattern}_{this.xyz}_FFC", FlowImage);
                                }
                                catch (Exception ex)
                                {
                                    this.ErrorCode = $"[TaskBaseCalibration] - [IpFFC] - {ex.Message}";
                                    this.State = EnState.Alarm;
                                }

                                timer1.Stop();
                                this.infoManager.Debug($"[{this.xyz}][IpFFC] {timer1.timeSpend} ms");
                            }

                            State = EnState.IpCalibration;
                        }
                        break;

                    case EnState.IpCalibration:
                        {
                            if (this.param.CalibrationIsDoStep)
                            {
                                timer1.Start();

                                try
                                {
                                    //DebugSaveImage($"{this.pattern}_{this.xyz}_Calibration_Before", FlowImage);
                                    MyMilIp.Calibration(this.calibrationData.Calibration, ref FlowImage);
                                    DebugSaveImage($"{this.pattern}_{this.xyz}_Calibration", FlowImage);
                                }
                                catch (Exception ex)
                                {
                                    this.ErrorCode = $"[TaskBaseCalibration] - [IpCalibration] - {ex.Message}";
                                    this.State = EnState.Alarm;
                                }

                                timer1.Stop();
                                this.infoManager.Debug($"[{this.xyz}][IpCalibration] {timer1.timeSpend} ms");
                            }

                            State = EnState.IpWarpingCalibration;
                        }
                        break;

                    case EnState.IpWarpingCalibration:
                        {
                            if (param.WarpingCalibrationIsDoStep)
                            {
                                timer1.Start();
                                try
                                {
                                    //findedge
                                    Point[] CornerPoint = new Point[4];
                                    MyMilIp.FindCornerbyEdgeFinder(FlowImage, ref CornerPoint);
                                    this.infoManager.General($"[{this.xyz}][FindCornerbyEdgeFinder] (P0,P1,P2,P3)= ({CornerPoint[0].X},{CornerPoint[0].Y}), " +
                                                                                                                    $"({CornerPoint[1].X},{CornerPoint[1].Y}), " +
                                                                                                                    $"({CornerPoint[2].X},{CornerPoint[2].Y}), " +
                                                                                                                    $"({CornerPoint[3].X},{CornerPoint[3].Y})");
                                    bool cornerpointerror = false;
                                    for (int cornerindex = 0; cornerindex < CornerPoint.Length; cornerindex++)
                                    {
                                        if (CornerPoint[cornerindex].X == 0 & CornerPoint[cornerindex].Y == 0)
                                        {
                                            this.infoManager.Error($"[{this.xyz}][FindCornerbyEdgeFinder] CornerPoint({cornerindex})=(0,0)");
                                            cornerpointerror = true;
                                            break;
                                        }
                                    }
                                    if (cornerpointerror)
                                    {
                                        State = EnState.IpLaserCalibration;
                                        break;
                                    }
                                    //warping
                                    MyMilIp.Warp(FlowImage, CornerPoint[0], CornerPoint[2], CornerPoint[3], CornerPoint[1], CornerPoint[0], CornerPoint[3], FlowImage);

                                    DebugSaveImage($"{this.pattern}_{this.xyz}_Warping", FlowImage);
                                }
                                catch (Exception ex)
                                {
                                    this.ErrorCode = $"[TaskBaseCalibration] - [IpWarpingCalibration] - {ex.Message}";
                                    this.State = EnState.Alarm;
                                }
                                timer1.Stop();
                                this.infoManager.Debug($"[{this.xyz}][IpWarpingCalibration] {timer1.timeSpend} ms");
                            }
                            State = EnState.IpLaserCalibration;
                        }
                        break;

                    case EnState.IpLaserCalibration:
                        {
                            if (param.LaserCalibrationIsDoStep)
                            {
                                timer1.Start();

                                string[] roi = this.param.LaserCalibration_ROI[this.ipStep].Split(',');

                                LaserCalibrationParam param = new LaserCalibrationParam();
                                param.Template_Path = $"{this.recipePath}\\LaserCalibration\\{this.pattern}_Template.tif";
                                param.Target_Path = $"{this.recipePath}\\LaserCalibration\\{this.pattern}_Target.tif";
                                param.ROI = new System.Drawing.Rectangle(Convert.ToInt32(roi[0]), Convert.ToInt32(roi[1]), Convert.ToInt32(roi[2]), Convert.ToInt32(roi[3]));
                                param.WaitCorrectId = FlowImage;

                                string imageName = "";
                                imageName = $"{this.pattern}_{this.xyz}_LaserCalibration.tif";
                                param.Output_Path = $@"{this.directoryPath}\{imageName}";

                                MyMilIp.Calibration_SecondCorrect(param);
                                MIL.MbufRestore(param.Output_Path, MyMil.MilSystem, ref FlowImage);

                                timer1.Stop();
                                this.infoManager.Debug($"[{this.xyz}][IpLaserCalibration] {timer1.timeSpend} ms");
                            }

                            State = EnState.SaveImage;
                        }
                        break;

                    case EnState.SaveImage:
                        {
                            timer1.Start();

                            string ImgName = $"{this.pattern}_{this.xyz}.tif";
                            string SavePath = $@"{this.directoryPath}\{this.pattern}\{ImgName}";

                            try
                            {
                                MIL.MbufSave(SavePath, this.FlowImage);
                            }
                            catch (Exception ex)
                            {
                                this.ErrorCode = $"[TaskBaseCalibration] - [SaveImage] - {ex.Message}";
                                this.State = EnState.Alarm;
                            }

                            timer1.Stop();
                            this.infoManager.Debug($"[{this.xyz}][SaveImage] {timer1.timeSpend} ms");

                            State = EnState.Finish;
                        }
                        break;

                    case EnState.Finish:
                        {
                            this.IsDone = true;

                            State = EnState.Idel;
                        }
                        break;

                    case EnState.Alarm:
                        {
                            this.IsDone = true;
                            this.infoManager.Error(this.ErrorCode);

                            State = EnState.Idel;
                        }
                        break;
                }

                Thread.Sleep(1);
            }
        }

        private void DebugSaveImage(string filename, MIL_ID image)
        {
            if (this.param.IsSaveDebugImage)
            {
                timer2.Start();

                try
                {
                    MIL.MbufSave($@"{this.directoryPath}\{filename}.tif", image);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                timer2.Stop();
                this.infoManager.Debug($"[{this.xyz}]\tIpRemoveHotPixel-SaveImage {timer2.timeSpend} ms");
            }
        }
    }

    class BaseCalibrationId
    {
        public MIL_ID Dark;
        public MIL_ID Flat;
        public MIL_ID Offset;

        public MIL_ID Calibration = MIL.M_NULL;

        public BaseCalibrationId()
        {
            this.Dark = MIL.M_NULL;
            this.Flat = MIL.M_NULL;
            this.Offset = MIL.M_NULL;
            this.Calibration = MIL.M_NULL;
        }

        public void Load(string darkPath, string flatPath, string offsetPath, string calibrationPath)
        {
            string errMsg = "";
            try
            {
                if (darkPath == "")
                {
                    if (!System.IO.File.Exists(darkPath)) errMsg += string.Format("[darkPath] {0} was not exists ! \r\n ; ", darkPath);
                }

                if (flatPath == "")
                {
                    if (!System.IO.File.Exists(flatPath)) errMsg += string.Format("[flatPath] {0} was not exists ! \r\n ; ", flatPath);
                }

                if (offsetPath == "")
                {
                    if (!System.IO.File.Exists(offsetPath)) errMsg += string.Format("[offsetPath] {0} was not exists ! \r\n ; ", offsetPath);
                }

                if (calibrationPath == "")
                {
                    if (!System.IO.File.Exists(calibrationPath)) errMsg += string.Format("[calibrationPath] {0} was not exists ! \r\n ; ", calibrationPath);
                }

                if (errMsg != "") throw new Exception(errMsg);

                if (darkPath == "")
                {
                    MilNetHelper.MilBufferFree(ref this.Dark);
                    MIL.MbufRestore(darkPath, MyMil.MilSystem, ref this.Dark);
                }

                if (flatPath == "")
                {
                    MilNetHelper.MilBufferFree(ref this.Flat);
                    MIL.MbufRestore(flatPath, MyMil.MilSystem, ref this.Flat);
                }

                if (offsetPath == "")
                {
                    MilNetHelper.MilBufferFree(ref this.Offset);
                    MIL.MbufRestore(offsetPath, MyMil.MilSystem, ref this.Offset);
                }

                if (calibrationPath == "")
                {
                    MilNetHelper.MilCalFree(ref this.Calibration);
                    MIL.McalRestore(calibrationPath, MyMil.MilSystem, MIL.M_DEFAULT, ref this.Calibration);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "(" + ex.StackTrace + ")");
            }

        }

        public void Dispose()
        {
            if (this.Dark != MIL.M_NULL)
            {
                MIL.MbufFree(this.Dark);
                this.Dark = MIL.M_NULL;
            }

            if (this.Flat != MIL.M_NULL)
            {
                MIL.MbufFree(this.Flat);
                this.Flat = MIL.M_NULL;
            }

            if (this.Offset != MIL.M_NULL)
            {
                MIL.MbufFree(this.Offset);
                this.Offset = MIL.M_NULL;
            }

            if (this.Calibration != MIL.M_NULL)
            {
                MIL.McalFree(this.Calibration);
                this.Calibration = MIL.M_NULL;
            }
        }

        public bool IsReady()
        {
            if (GlobalVar.OmsParam.FfcIsDoStep)
            {
                if (this.Dark == MIL.M_NULL || this.Flat == MIL.M_NULL || this.Offset == MIL.M_NULL)
                {
                    return false;
                }
            }


            if (GlobalVar.OmsParam.CalibrationIsDoStep)
            {
                if (this.Calibration == MIL.M_NULL)
                {
                    return false;
                }
            }

            return true;

        }
    }
}
