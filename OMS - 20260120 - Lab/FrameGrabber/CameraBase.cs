using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCvSharp;
using System.IO.Ports;
using BaseTool;

using ASI_ERROR_CODE = FrameGrabber.ASICameraCtrl.ASI_ERROR_CODE;
using ASI_CONTROL_TYPE = FrameGrabber.ASICameraCtrl.ASI_CONTROL_TYPE;
using ASI_EXPOSURE_STATUS = FrameGrabber.ASICameraCtrl.ASI_EXPOSURE_STATUS;
using ASI_CAMERA_INFO = FrameGrabber.ASICameraCtrl.ASI_CAMERA_INFO;

using Size = OpenCvSharp.Size;
using System.Threading;

namespace FrameGrabber
{
    /// Error code 規則
    /// CN: 第幾種相機; C1 = ZWO; C2 = Matrox;
    /// MN: 第幾個Function, 從00開始
    /// PQ: Function裡的第幾個例外, 從00開始

    #region --- CameraBase ---

    public class CameraBase
    {
        public MIL_ID GrabImage = MIL.M_NULL;
        public MIL_ID milDark = MIL.M_NULL;

        public int MaxWidth;
        public int MaxHeight;
        public int ImageBits;
        public int MaxGrayScale;
        public int Bin;
        public string CurrentDarkPath = "";

        public CameraBase()
        {
        }

        #region --- Initial ---

        public virtual void Initial(MIL_ID milSys, string cameraId, string bin = "1")
        {
        }

        public virtual void Initial(MIL_ID milSys)
        {
        }

        #endregion --- Initial ---

        public virtual void Open(string CameraID)
        {
        }

        public virtual void OpenCooler()
        {
        }

        public virtual void OpenFan()
        {
        }

        public virtual void Capture(string DarkPath)
        {

        }

        public virtual void Capture()
        {

        }

        public virtual void Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark)
        {
        }

        public virtual void Capture(ref MIL_ID milSource, string outputFolder, string PatternName)
        {
        }
        

        public virtual void SetGrabExposureTime(double exposureTime)
        {
        }

        public virtual double GetGrabExposureTime()
        {
            return 0;
        }

        public virtual void SetGain(double gain)
        {
        }

        public virtual double GetGain()
        {
            return 0;
        }


        public virtual double GetTemperature(int iCameraID)
        {
            return 0.0;
        }

        public virtual double GetTemperature()
        {
            return 0.0;
        }

        public virtual void Dispose()
        {
        }

        #region --- Com Port Control ---

        #region --- OpenComPort ---

        public virtual void OpenComPort(string PortName, int BaudRate)
        {
        }

        #endregion --- OpenComPort ---

        #region --- CloseComPort ---

        public virtual void CloseComPort()
        {
        }

        #endregion --- CloseComPort ---

        #endregion --- Com Port Control ---
    }

    #endregion --- CameraBase ---

    #region --- ASICamera ---

    public class ASICamera : CameraBase
    {
        private MIL_ID milSystem;

        public ASICamera()
        {
            this.MaxGrayScale = 65535;
        }

        #region "--- Open ---"

        public override void Open(string CameraID)
        {
            ASI_ERROR_CODE errorCode;
            int iCameraID = Convert.ToInt32(CameraID);

            // 打開相機
            errorCode = ASICameraCtrl.ASIOpenCamera(iCameraID);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Open camera failed.");
            }
        }

        #endregion "--- Open ---"

        #region "--- initCamera ---"

        private void initCamera(string CameraID)
        {
            ASI_ERROR_CODE errorCode;
            int iCameraID = Convert.ToInt32(CameraID);

            errorCode = ASICameraCtrl.ASIInitCamera(iCameraID);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Initial camera failed.");
            }
        }

        #endregion "--- initCamera ---"

        #region "--- AllocateGrabImage ---"

        private void AllocateGrabImage(string CameraID, int width, int height, int bin)
        {
            ASI_ERROR_CODE errorCode;
            int iCameraID = Convert.ToInt32(CameraID);

            this.MaxWidth = width;
            this.MaxHeight = height;
            this.Bin = bin;

            try
            {
                if (this.GrabImage != MIL.M_NULL)
                {
                    MIL.MbufFree(this.GrabImage);
                    this.GrabImage = MIL.M_NULL;
                }

                MIL.MbufAlloc2d(this.milSystem,
                              width,
                              height,
                              16 + MIL.M_UNSIGNED,
                              MIL.M_IMAGE + MIL.M_PROC + MIL.M_GRAB + MIL.M_DISP,
                              ref this.GrabImage);

                MIL.MbufClear(this.GrabImage, 0);

                errorCode = ASICameraCtrl.ASISetROIFormat(iCameraID,
                    width,
                    height,
                    bin,
                    ASICameraCtrl.ASI_IMG_TYPE.ASI_IMG_RAW16);

                if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
                {
                    throw new Exception("Set ROI format failed.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion "--- AllocateGrabImage ---"

        #region "--- SetStartPos ---"

        private void SetStartPos(string CameraID, int iStartX, int iStartY)
        {
            ASI_ERROR_CODE errorCode;
            int iCameraID = Convert.ToInt32(CameraID);

            errorCode = ASICameraCtrl.ASISetStartPos(iCameraID, iStartX, iStartY);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Set ROI start position failed.");
            }
        }

        #endregion "--- SetStartPos ---"

        #region "--- Initial ---"

        public override void Initial(MIL_ID milSys, string cameraId, string bin = "1")
        {
            int binNum;
            this.milSystem = milSys;
            ASI_CAMERA_INFO cameraInfo = new ASI_CAMERA_INFO();

            try
            {
                ASICameraCtrl.ASIGetCameraProperty(out cameraInfo, 0);

                if (int.TryParse(bin, out binNum))
                {
                    if (binNum != 1 && binNum != 2 && binNum != 4)
                    {
                        throw new Exception("Unsupport bin number.");
                    }
                }
                else
                {
                    throw new Exception("Bin number error.");
                }

                this.Open(cameraId);

                this.initCamera(cameraId);

                this.AllocateGrabImage(cameraId,
                    cameraInfo.MaxWidth / binNum,
                    cameraInfo.MaxHeight / binNum,
                    binNum);

                this.SetStartPos(cameraId, 0, 0);

                this.OpenCooler();

                this.OpenFan();

                this.SetGain(0);

                this.SetGamma(50);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion "--- Initial ---"

        #region "--- OpenCooler ---"

        public override void OpenCooler()
        {
            ASI_ERROR_CODE errorCode;

            // 開製冷
            errorCode = ASICameraCtrl.ASISetControlValue(0, ASI_CONTROL_TYPE.ASI_COOLER_ON, 1);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Initial camera failed. (C0-101-00)");
            }
        }

        #endregion "--- OpenCooler ---"

        #region "--- OpenFan ---"

        public override void OpenFan()
        {
            ASI_ERROR_CODE errorCode;

            // 開風扇
            errorCode = ASICameraCtrl.ASISetControlValue(0, ASI_CONTROL_TYPE.ASI_FAN_ON, 1);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Initial camera failed. (C0-102-00)");
            }
        }

        #endregion "--- OpenFan ---"

        #region "--- Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark) ---"

        public override void Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark)
        {
            ASI_ERROR_CODE errorCode;
            ASI_EXPOSURE_STATUS status;

            Size size = new Size(this.MaxWidth, this.MaxHeight);
            Mat mat = new Mat(size, MatType.CV_16U);
            IntPtr ptr = mat.DataStart;

            int waitCount = 0;

            // 開始曝光
            errorCode = ASICameraCtrl.ASIStartExposure(0, ASICameraCtrl.ASI_BOOL.ASI_FALSE);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Start exposure failed.");
            }

            // 等待曝光完成
            while (true)
            {
                ASICameraCtrl.ASIGetExpStatus(0, out status);

                if (status != ASI_EXPOSURE_STATUS.ASI_EXP_WORKING)
                {
                    break;
                }
                waitCount++;
                Thread.Sleep(1);
            }

            // 結束曝光
            errorCode = ASICameraCtrl.ASIStopExposure(0);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Stop exposure failed.");
            }

            // 確認曝光狀態
            if (status != ASI_EXPOSURE_STATUS.ASI_EXP_SUCCESS)
            {
                throw new Exception($"ASI Camera Exposure status error. ({status.ToString()})");
            }

            // 取得曝光資料
            errorCode = ASICameraCtrl.ASIGetDataAfterExp(0, ptr, this.MaxWidth * this.MaxHeight * 2);
            if (status != ASI_EXPOSURE_STATUS.ASI_EXP_SUCCESS)
            {
                throw new Exception("Get exposure data failed.");
            }

            //// 調用OpenCV方法存圖(確認取像是否正確)
            //Cv2.ImWrite("d:\\1.tif", mat);

            // 把影像塞進MIL_ID
            var bytes = new byte[mat.Total() * 2];
            Marshal.Copy(mat.Data, bytes, 0, bytes.Length);
            MIL.MbufPut2d(this.GrabImage, 0, 0, this.MaxWidth, this.MaxHeight, bytes);

            if (GrabSubDark)
                MIL.MimArith(this.GrabImage, milDark, milSource, MIL.M_SUB_ABS + MIL.M_SATURATION);
            else
                MIL.MbufCopy(this.GrabImage, milSource);
        }

        #endregion "--- Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark) ---"

        #region "--- Capture ---"

        public override void Capture()
        {
            ASI_ERROR_CODE errorCode;
            ASI_EXPOSURE_STATUS status;

            Size size = new Size(this.MaxWidth, this.MaxHeight);
            Mat mat = new Mat(size, MatType.CV_16U);
            IntPtr ptr = mat.DataStart;

            int waitCount = 0;

            // 開始曝光
            errorCode = ASICameraCtrl.ASIStartExposure(0, ASICameraCtrl.ASI_BOOL.ASI_FALSE);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Start exposure failed.");
            }

            // 等待曝光完成
            while (true)
            {
                ASICameraCtrl.ASIGetExpStatus(0, out status);

                if (status != ASI_EXPOSURE_STATUS.ASI_EXP_WORKING)
                {
                    break;
                }
                waitCount++;
                Thread.Sleep(1);
            }

            // 結束曝光
            errorCode = ASICameraCtrl.ASIStopExposure(0);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Stop exposure failed.");
            }

            // 確認曝光狀態
            if (status != ASI_EXPOSURE_STATUS.ASI_EXP_SUCCESS)
            {
                throw new Exception($"ASI Camera Exposure status error. ({status.ToString()})");
            }

            // 取得曝光資料
            errorCode = ASICameraCtrl.ASIGetDataAfterExp(0, ptr, this.MaxWidth * this.MaxHeight * 2);
            if (status != ASI_EXPOSURE_STATUS.ASI_EXP_SUCCESS)
            {
                throw new Exception("Get exposure data failed.");
            }

            //// 調用OpenCV方法存圖(確認取像是否正確)
            //Cv2.ImWrite("d:\\1.tif", mat);

            // 把影像塞進MIL_ID
            var bytes = new byte[mat.Total() * 2];
            Marshal.Copy(mat.Data, bytes, 0, bytes.Length);
            MIL.MbufPut2d(this.GrabImage, 0, 0, this.MaxWidth, this.MaxHeight, bytes);
        }

        #endregion "--- Capture ---"

        #region "--- SetGrabExposureTime ---"

        public override void SetGrabExposureTime(double exposureTime)
        {
            ASI_ERROR_CODE errorCode = ASI_ERROR_CODE.ASI_SUCCESS;

            if (exposureTime < 1000)
            {
                throw new Exception("Set exposure time failed. (C0-201-00)");
            }

            errorCode = ASICameraCtrl.ASISetControlValue(0,
                                                         ASICameraCtrl.ASI_CONTROL_TYPE.ASI_EXPOSURE,
                                                         Convert.ToInt32(exposureTime));

            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Set exposure time failed. (C0-201-01)");
            }

            int t = ASICameraCtrl.ASIGetControlValue(0, ASI_CONTROL_TYPE.ASI_EXPOSURE);
        }

        #endregion "--- SetGrabExposureTime ---"

        #region "--- SetGain ---"

        public override void SetGain(double gain)
        {
            ASI_ERROR_CODE errorCode;

            //TODO: 相機設定上下限?
            if (gain > 570 || gain < 0)
            {
                throw new Exception("Gain Value is out of domain.");
            }

            // Gain
            errorCode = ASICameraCtrl.ASISetControlValue(0, ASI_CONTROL_TYPE.ASI_GAIN, (int)gain);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Set camera gain failed.");
            }
        }

        #endregion "--- SetGain ---"

        #region "--- SetGamma ---"

        public void SetGamma(double gamma)
        {
            ASI_ERROR_CODE errorCode;

            //TODO: 相機設定上下限?
            if (gamma > 100 || gamma < 0)
            {
                throw new Exception("Gamma Value is out of domain.");
            }

            // Gamma
            errorCode = ASICameraCtrl.ASISetControlValue(0, ASI_CONTROL_TYPE.ASI_GAMMA, (int)gamma);
            if (errorCode != ASI_ERROR_CODE.ASI_SUCCESS)
            {
                throw new Exception("Set camera gamma failed.");
            }
        }

        #endregion "--- SetGamma ---"

        #region "--- GetTemperature ---"

        public override double GetTemperature(int iCameraID)
        {
            double t = ASICameraCtrl.ASIGetControlValue(0, ASI_CONTROL_TYPE.ASI_TEMPERATURE);
            return t / 10.0;
        }

        #endregion "--- GetTemperature ---"

        #region "--- Dispose ---"

        public override void Dispose()
        {
            ASICameraCtrl.ASICloseCamera(0);

            if (this.GrabImage != MIL.M_NULL)
            {
                MIL.MbufFree(this.GrabImage);
                this.GrabImage = MIL.M_NULL;
            }
        }

        #endregion "--- Dispose ---"

        #region --- Com Port Control ---

        #region --- OpenComPort ---

        public override void OpenComPort(string PortName, int BaudRate)
        {
        }

        #endregion --- OpenComPort ---

        #region --- CloseComPort ---

        public override void CloseComPort()
        {
        }

        #endregion --- CloseComPort ---

        #endregion --- Com Port Control ---
    }

    #endregion --- ASICamera ---

    #region --- MatroxCamera ---

    public class MatroxCamera : CameraBase
    {
        private MIL_ID milSystem;
        private MIL_ID milDigitizer = MIL.M_NULL;

        private MilDigParam DigPara = new MilDigParam();
        // Control Camera Gain
        public SerialPort ComPort = null;

        public bool isRecive = false;
        public bool IsComportConnect = false;


        // Com Pots
        private String[] ComPorts;

        private CLProtocolDataStruct CLProtocolData = new CLProtocolDataStruct();

        public MatroxCamera()
        {
            this.MaxGrayScale = 4095;
        }

        #region "--- Open ---"

        public override void Open(string CameraID)
        {
        }

        #endregion "--- Open ---"

        #region "--- Initial ---"

        public void Initial(MIL_ID milSys, MilDigParam DigPara)
        {
            this.milSystem = milSys;

            this.DigPara = DigPara;
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                        {
                            this.CreateGrab(1);
                            this.AllocGrabImage();
                        }
                        break;

                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            this.CreateGrab(this.DigPara.Camera_No, DigPara.Camera_CamFile);
                            this.AllocGrabImage();

                            this.GenICam_Init();
                        }
                        break;


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region GenICam_Init

        private int GenICam_Init()
        {
            MIL_INT BoardType = 0;
            MIL_INT SystemNum = 0;
            MIL_INT DigitizerNum = 0;

            MIL.MsysInquire(this.milSystem, MIL.M_BOARD_TYPE, ref BoardType);

            if ((BoardType & MIL.M_CL) == 0)
            {
                Console.WriteLine("This example program can only be used with a Camera Link system type");
                Console.WriteLine("such as Matrox Solios, Matrox Radient or Matrox Rapixo Camera Link boards.");
                Console.WriteLine("Please ensure that the default system type is set accordingly in MIL Config");
                return 1;
            }

            Console.WriteLine("This example shows how to use GenICam with Camera Link devices.\n");
            Console.WriteLine("GenICam is supported with Camera Link devices as long as your camera");
            Console.WriteLine("vendor supplies a standard compliant CLProtocol dll or your device supports GenCP");
            Console.WriteLine("Press any key to enumerate the device identifiers supported by");
            Console.WriteLine("installed CLProtocol libraries.\n");

            CLProtocolEnumDeviceIDs(this.milDigitizer, ref CLProtocolData);

            if (CLProtocolData.NbDevIDs == 0)
            {
                Console.WriteLine("\nNo CLProtocol libraries have been found.");
                Console.WriteLine("Make sure the CLProtocol library supplied by your camera vendor is");
                Console.WriteLine("properly installed.\n");

                return 1;
            }

            // 查詢系統和數字化器編號
            SystemNum = MIL.MsysInquire(milSystem, MIL.M_NUMBER);
            DigitizerNum = MIL.MdigInquire(milDigitizer, MIL.M_NUMBER);

            // 查詢系統描述符用於下面的輸出
            StringBuilder systemDescriptor = new StringBuilder();
            MIL.MsysInquire(milSystem, MIL.M_SYSTEM_DESCRIPTOR, systemDescriptor);

            // 要求用戶選擇與其相機匹配的正確設備標識符
            Console.WriteLine("\nPlease select the CLProtocol device identifier of the camera connected to:");
            Console.WriteLine("{0} M_DEV{1} digitizer M_DEV{2} (0-{3})", systemDescriptor, SystemNum, DigitizerNum, CLProtocolData.NbDevIDs);

            CLProtocolSelectDeviceID(milDigitizer, ref CLProtocolData, 0);

            // 打印消息
            Console.WriteLine("\nNow showing the camera's features through the feature browser window.");
            Console.WriteLine("You can use the feature browser to change camera parameters.\n");

            // 此時 CLProtocol (和 GenICam®) 應該已經正確初始化

            return 0;
        }

        /* Enumerates device IDs supported by installed CLProtocol libraries. */
        private void CLProtocolEnumDeviceIDs(MIL_ID MilDigitizer, ref CLProtocolDataStruct CLProtocolData)
        {
            CLProtocolData.NbDevIDs = MIL.MdigInquire(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID_NUM);
            CLProtocolData.DevIdStrLen = MIL.MdigInquire(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID_SIZE_MAX);

            Console.WriteLine("Installed CLProtocol devices found:");

            if (CLProtocolData.NbDevIDs > 0)
            {
                //CLProtocolData.DevIDs = new List<StringBuilder>((int)CLProtocolData.NbDevIDs);

                Console.WriteLine("{0,2}|{1,22}|{2,10}|{3,20}|{4,20}", "Nb", "CLProtocol File Name", "Vendor", "Family", "Model");
                Console.WriteLine("--+----------------------+----------+--------------------+--------------------\n");

                for (int i = 0; i < CLProtocolData.NbDevIDs; i++)
                {
                    CLProtocolData.DevIDs.Add(new StringBuilder());
                    // 查詢設備 ID
                    MIL.MdigInquire(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID + i, CLProtocolData.DevIDs[i]);

                    // 分解字符串以使打印更易讀
                    string[] Field = new string[7];
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.DriverDirectory,ref Field[0], 256);
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.DriverFileName, ref Field[1], 256);
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.Manufacturer, ref Field[2], 256);
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.Family, ref Field[3], 256);
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.Model, ref Field[4], 256);
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.Version, ref Field[5], 256);
                    ExtractField(CLProtocolData.DevIDs[i].ToString(), FieldType.SerialNumber, ref Field[6], 256);

                    Console.WriteLine("{0,2} {1,22} {2,10} {3,20} {4,20}", i, Field[1], Field[2], Field[3], Field[4]);
                }

                Console.WriteLine("\n{0,2} Use Default from MilConfig.", CLProtocolData.NbDevIDs);

            }

        }

        /* Prompts user to select a CLProtocol device identifier matching his camera. */
        public void CLProtocolSelectDeviceID(MIL_ID MilDigitizer, ref CLProtocolDataStruct CLProtocolData, int Idx)
        {
            CLProtocolData.UserSelection = Idx;
            
            MIL.MdigControl(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID, "M_DEFAULT");

            //if (CLProtocolData.UserSelection == CLProtocolData.NbDevIDs)
            //{
            //    MIL.MdigControl(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID, "M_DEFAULT");
            //}
            //else
            //{
            //    MIL.MdigControl(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID, CLProtocolData.DevIDs[CLProtocolData.UserSelection].ToString());
            //}

            // 初始化 CLProtocol 驅動程序和 GenICam®
            // 如果發生錯誤，很可能是選擇了錯誤的 CLProtocol 設備標識符
            MIL.MdigControl(MilDigitizer, MIL.M_GC_CLPROTOCOL, MIL.M_ENABLE);
        }

        public void ExtractField(string deviceId, FieldType field, ref string extractedData, int dataSize)
        {
            int cnt = 0;
            string tempString = deviceId;

            // 根據字段類型設置計數器
            switch (field)
            {
                case FieldType.DriverDirectory:
                    cnt = 0;
                    break;
                case FieldType.DriverFileName:
                    cnt = 1;
                    break;
                case FieldType.Manufacturer:
                    cnt = 2;
                    break;
                case FieldType.Family:
                    cnt = 3;
                    break;
                case FieldType.Model:
                    cnt = 4;
                    break;
                case FieldType.Version:
                    cnt = 5;
                    break;
                case FieldType.SerialNumber:
                    cnt = 6;
                    break;
            }

            // 使用 C# 的字符串分割功能
            string[] tokens = tempString.Split('#');

            // 檢查是否有足夠的令牌並提取所需字段
            if (cnt < tokens.Length)
            {
                // 確保不超過目標緩衝區大小
                string result = tokens[cnt];
                if (result.Length >= dataSize)
                {
                    result = result.Substring(0, dataSize - 1);
                }
                extractedData = result;
            }
            else
            {
                extractedData = string.Empty;
            }
        }


        #endregion

        #endregion "--- Initial ---"

        #region "--- SetGrabExposureTime ---"

        public override void SetGrabExposureTime(double exposureTime)
        {
            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        // Do Nothing
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                    {
                        double ExTime = 0;
                        MIL.MdigControl(milDigitizer,
                                        MIL.M_TIMER_DURATION + MIL.M_TIMER1,
                                        exposureTime * 1000.0);
                        MIL.MdigInquire(milDigitizer,
                                        MIL.M_TIMER_DURATION + MIL.M_TIMER1,
                                        ref ExTime);
                    }
                    break;

                case "M_SYSTEM_RAPIXOCXP":
                    {
                        MIL.MdigControlFeature(milDigitizer, MIL.M_DEFAULT, "ExposureTime", MIL.M_TYPE_DOUBLE, ref exposureTime);
                    }
                    break;
            }
        }

        #endregion "--- SetGrabExposureTime ---"

        #region "--- GetGrabExposureTime ---"

        public override double GetGrabExposureTime()
        {
            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        return 0;
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                    {
                        double ExTime = 0;
                        MIL.MdigInquire(milDigitizer,
                                        MIL.M_TIMER_DURATION + MIL.M_TIMER1,
                                        ref ExTime);

                        return ExTime / 1000.0;
                    }
                    break;

                case "M_SYSTEM_RAPIXOCXP":
                    {
                        double ExTime = 0;

                        MIL.MdigInquireFeature(milDigitizer, MIL.M_DEFAULT, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ExTime);
                        return ExTime;
                    }
                    break;
            }

            return 0;

        }

        #endregion "--- GetGrabExposureTime ---"


        #region "--- SetGain ---"

        public override void SetGain(double gain)
        {
            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        // Do Nothing
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                case "M_SYSTEM_RAPIXOCXP":
                    {
                        //double Gain = 1;
                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "GainSelector", MIL.M_TYPE_STRING, "DigitalAll");
                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref gain);


                    }
                    break;
            }
        }

        #endregion "--- SetGain ---"

        #region "--- GetGrabGain ---"

        public override double GetGain()
        {
            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        // Do Nothing
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                case "M_SYSTEM_RAPIXOCXP":
                    {
                        double Gain = 0;
                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "GainSelector", MIL.M_TYPE_STRING, "DigitalAll");
                        MIL.MdigInquireFeature(milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref Gain);

                        return Gain;
                    }
                    break;
            }

            return 0;
        }

        #endregion "--- SetGain ---"

        #region "--- Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark) ---"

        public override void Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark)
        {
            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        MIL.MdigGrab(milDigitizer, this.GrabImage);
                        MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_END);
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                    {
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_ABORT, MIL.M_DEFAULT);
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);
                        MIL.MdigControl(milDigitizer, MIL.M_TIMER_TRIGGER_SOFTWARE + MIL.M_TIMER1, 1);
                        MIL.MdigGrab(milDigitizer, this.GrabImage);
                        MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_END);
                    }
                    break;

                case "M_SYSTEM_RAPIXOCXP":
                    {
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_ABORT, MIL.M_DEFAULT);

                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "TriggerSelector", MIL.M_TYPE_STRING, "ExposureStart");  ////TriggerSelector各家尚未統一ExposureStart為VieWorks
                                                                                                                                           //MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "TriggerMode", MIL.M_TYPE_STRING, "On");
                                                                                                                                           //MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "TriggerSource", MIL.M_TYPE_STRING, "Software");

                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);

                        MIL.MdigGrab(milDigitizer, this.GrabImage);
                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_EXECUTE, "TriggerSoftware", MIL.M_TYPE_COMMAND, MIL.M_NULL);
                        MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_END);
                    }
                    break;

                default:
                    throw new Exception("Unsupport system.");
            }

           

            if (GrabSubDark)
                MIL.MimArith(this.GrabImage, milDark, milSource, MIL.M_SUB_ABS + MIL.M_SATURATION);
            else
                MIL.MbufCopy(this.GrabImage, milSource);
        }

        #endregion "--- Capture(ref MIL_ID milSource, ref MIL_ID milDark, bool GrabSubDark) ---"

        #region "--- Capture(ref MIL_ID milSource, string outputFolder, string PatternName) ---"

        public override void Capture(ref MIL_ID milSource, string outputFolder, string PatternName)
        {
            try
            {
                string srcImagePath = "";
                string sourceImageFolder = String.Format(@"{0}\Source",
                        outputFolder);
                if (!Directory.Exists(sourceImageFolder)) Directory.CreateDirectory(sourceImageFolder);

                srcImagePath = string.Format(@"{0}\{1}.tif", sourceImageFolder, PatternName);

                if (!File.Exists(srcImagePath))
                    throw new Exception(string.Format(@"[Capture] File[{0}] was exists !", srcImagePath));

                MIL.MbufLoad(srcImagePath, milSource);
            }
            catch (Exception ex)
            {
                throw new Exception(@"[Capture] " + ex.ToString());
            }
        }

        #endregion "--- Capture(ref MIL_ID milSource, string outputFolder, string PatternName) ---"

        #region "--- Capture ---"

        public override void Capture()
        {
            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        MIL.MdigGrab(milDigitizer, this.GrabImage);
                        MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_END);
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                    {
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_ABORT, MIL.M_DEFAULT);
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);
                        MIL.MdigControl(milDigitizer, MIL.M_TIMER_TRIGGER_SOFTWARE + MIL.M_TIMER1, 1);
                        MIL.MdigGrab(milDigitizer, this.GrabImage);
                        MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_END);
                    }
                    break;

                case "M_SYSTEM_RAPIXOCXP":
                    {
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_ABORT, MIL.M_DEFAULT);

                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "TriggerSelector", MIL.M_TYPE_STRING, "ExposureStart");  ////TriggerSelector各家尚未統一ExposureStart為VieWorks
                                                                                                                                           //MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "TriggerMode", MIL.M_TYPE_STRING, "On");
                                                                                                                                           //MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "TriggerSource", MIL.M_TYPE_STRING, "Software");

                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);

                        MIL.MdigGrab(milDigitizer, this.GrabImage);
                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_EXECUTE, "TriggerSoftware", MIL.M_TYPE_COMMAND, MIL.M_NULL);
                        MIL.MdigGrabWait(milDigitizer, MIL.M_GRAB_END);
                    }
                    break;

                default:
                    throw new Exception("Unsupport system.");
            }
        }

        #endregion "--- Capture ---"

        #region "--- Dispose ---"

        public override void Dispose()
        {
            if (this.GrabImage != MIL.M_NULL)
            {
                MIL.MbufFree(this.GrabImage);
                this.GrabImage = MIL.M_NULL;
            }

            if (this.milDigitizer != MIL.M_NULL)
            {
                MIL.MdigFree(this.milDigitizer);
            }

            // Control Camera Gain
            if (this.ComPort != null)
            {
                this.CloseComPort();
            }
        }

        #endregion "--- Dispose ---"

        #region "--- CreateGrab ---"

        private void CreateGrab(int CardNum, string CamFile = "M_DEFAULT")
        {

            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    {
                        MIL.MdigAlloc(this.milSystem, 1, "M_DEFAULT", MIL.M_DEFAULT, ref this.milDigitizer);
                    }
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                case "M_SYSTEM_RAPIXOCXP":
                    {
                        MIL.MdigAlloc(this.milSystem, CardNum, CamFile, MIL.M_DEFAULT, ref this.milDigitizer);
                    }
                    break;

                case "M_SYSTEM_RADIENTCXP_":
                    {
                        MIL.MdigAlloc(this.milSystem, CardNum, CamFile, MIL.M_DEFAULT, ref this.milDigitizer);
                        //MIL.MdigControlFeature(this.milDigitizer, MIL.M_FEATURE_VALUE, "TriggerMode", MIL.M_TYPE_STRING, "On");
                        //MIL.MdigControlFeature(this.milDigitizer, MIL.M_FEATURE_VALUE, "TriggerSource", MIL.M_TYPE_STRING, "Software");


                        //  MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, 6000);
                        MIL.MdigControl(milDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);

                        MIL.MdigControl(this.milDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);
                    }
                    break;
            }

            this.ImageBits = (int)MIL.MdigInquire(milDigitizer, MIL.M_SIZE_BIT, MIL.M_NULL);
            this.MaxWidth = (int)MIL.MdigInquire(milDigitizer, MIL.M_SIZE_X, MIL.M_NULL);
            this.MaxHeight = (int)MIL.MdigInquire(milDigitizer, MIL.M_SIZE_Y, MIL.M_NULL);
        }

        #endregion "--- CreateGrab ---"

        #region --- AllocGrabImage ---

        #region --- [1] AllocGrabImage ---

        private void AllocGrabImage()
        {
            try
            {
                MIL_INT ImageSign = 0;
                MIL_INT ImageDepth = 0;
                MIL_INT ImageWidth = 0;
                MIL_INT ImageHeight = 0;

                if (this.GrabImage != MIL.M_NULL)
                {
                    MIL.MbufFree(this.GrabImage);
                    this.GrabImage = MIL.M_NULL;
                }

                MIL.MdigInquire(milDigitizer, MIL.M_SIGN, ref ImageSign);
                MIL.MdigInquire(milDigitizer, MIL.M_SIZE_BIT, ref ImageDepth);
                MIL.MdigInquire(milDigitizer, MIL.M_SIZE_X, ref ImageWidth);
                MIL.MdigInquire(milDigitizer, MIL.M_SIZE_Y, ref ImageHeight);

                this.MaxWidth = (int)ImageWidth;
                this.MaxHeight = (int)ImageHeight;

                // image format control
                if (ImageDepth > 8 && ImageDepth <= 16)
                {
                    ImageDepth = 16;
                }
                else if (ImageDepth > 0 && ImageDepth <= 8)
                {
                    ImageDepth = 8;
                }
                else
                {
                    // unsupprot format
                    return;
                }

                MIL.MbufAlloc2d(this.milSystem,
                                ImageWidth,
                                ImageHeight,
                                ImageDepth + ImageSign,
                                MIL.M_IMAGE + MIL.M_PROC + MIL.M_GRAB + MIL.M_DISP,
                                ref this.GrabImage);

                if (ImageDepth > 8)
                {
                    MIL.MbufControl(this.GrabImage, MIL.M_MIN, 0);
                    MIL.MbufControl(this.GrabImage, MIL.M_MAX, 4095);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(@"[AllocGrabImage] " + ex.ToString());
            }
        }

        #endregion --- [1] AllocGrabImage ---


        #endregion --- AllocGrabImage ---

        #region "--- GetTemperature ---"

        public override double GetTemperature()
        {
            double DeviceTemperature = 1.0;

            switch (this.DigPara.Camera_Type)
            {
                case "M_SYSTEM_HOST":
                    break;

                case "M_SYSTEM_DEFAULT":
                case "M_SYSTEM_SOLIOS":
                case "M_SYSTEM_RADIENTPRO":
                case "M_SYSTEM_RADIENTEVCL":
                case "M_SYSTEM_RADIENTCXP":
                    {
                        MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "DeviceTemperatureSelector", MIL.M_TYPE_STRING, "Sensor");
                        MIL.MdigInquireFeature(milDigitizer, MIL.M_FEATURE_VALUE, "DeviceTemperature", MIL.M_TYPE_DOUBLE, ref DeviceTemperature);

                        //double ExTime = 0;
                        //double exposureTime = 300000;

                        //MIL.MdigControlFeature(milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref exposureTime);

                        //MIL.MdigInquireFeature(milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ExTime);
                    }
                    break;

                default:
                    throw new Exception("Unsupport system.");
            }

            return DeviceTemperature;
        }

        #endregion "--- Capture ---"

        #region --- Com Port Control ---

        #region --- OpenComPort ---

        public override void OpenComPort(string PortName, int BaudRate)
        {
            this.ComPorts = SerialPort.GetPortNames();

            if (!this.ComPorts.Contains(PortName))
            {
                this.ComPort = null;
                this.IsComportConnect = false;
            }
            else
            {
                if (this.ComPort == null)
                    this.ComPort = new SerialPort();

                if (this.ComPort.IsOpen)
                    this.ComPort.Close();

                // Setting
                this.ComPort.PortName = PortName;
                this.ComPort.BaudRate = BaudRate;
                this.ComPort.Parity = Parity.None;
                this.ComPort.DataBits = 8;
                this.ComPort.StopBits = StopBits.One;

                this.ComPort.Open();
                this.IsComportConnect = true;
            }
        }

        #endregion --- OpenComPort ---

        #region --- CloseComPort ---

        public override void CloseComPort()
        {
            if (!this.IsComportConnect) return;

            this.ComPort.Close();
            this.IsComportConnect = false;
        }

        #endregion --- CloseComPort ---

        #endregion --- Com Port Control ---
    }

    #endregion --- MatroxCamera ---

    public class CLProtocolDataStruct
    {
        public MIL_INT NbDevIDs { get; set; } = 0;
        public MIL_INT DevIdStrLen { get; set; } = 0;
        public List<StringBuilder> DevIDs { get; set; } = new List<StringBuilder>();
        public int UserSelection { get; set; } = 0;
    }

    public enum FieldType
    {
        DriverDirectory = 0,
        DriverFileName,
        Manufacturer,
        Family,
        Model,
        Version,
        SerialNumber
    }

}