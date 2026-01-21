using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightMeasure;
using HardwareManager;
using BaseTool;
using CommonSettings;

namespace OpticalMeasuringSystem
{
    public static class GlobalVar
    {
        public static bool IsSecretHidden = GlobalConfig.IsSecretHidden;
        public static bool RoiIsRect = false;

        public static EnumMotoControlPackage DeviceType = EnumMotoControlPackage.Airy_211_USB_Wheel_Motor;
        public static clsSystemSetting SD = new clsSystemSetting();
        public static LumCorrectionPara CorrData = new LumCorrectionPara();


        public static cls29MSetting OmsParam = new cls29MSetting();


        public static class Device
        {
            public static ASCOM_FilterCtrl XyzFilter = new ASCOM_FilterCtrl();
            public static ASCOM_FilterCtrl NdFilter = new ASCOM_FilterCtrl();
            public static int XyzPosition = 0;
            public static int NdPosition = 0;

            public static SerialPort LaserPort;
        }

        public static class Config
        {
            public static string ConfigPath = @"D:\OpticalMeasurementData\LUM\Config";
            public static string SystemLogPath = @"D:\OpticalMeasurementData\LUM\Log\systemlog";
            public static string NetworkLogPath = @"D:\OpticalMeasurementData\LUM\Log\networklog";
            public static string WarningLogPath = @"D:\OpticalMeasurementData\LUM\Log\warninglog";
            public static string ErrorLogPath = @"D:\OpticalMeasurementData\LUM\Log\errorlog";
            public static string DebugLogPath = @"D:\OpticalMeasurementData\LUM\Log\debuglog";
            public static string OutputPath = @"D:\OpticalMeasurementData\LUM\Output";

            public static string DirectoryPath = "";
            public static string RecipeName = "";
            public static string RecipePath = "";
        }

        public static class ProcessInfo
        {
            // 開始流程的小視窗
            public static string UserName = string.Empty;
            public static string PanelID = string.Empty;
            public static string Pattern = string.Empty;
            public static string Action = string.Empty;
            public static string CalibrationFile = string.Empty;
            public static string FFCFile = string.Empty;

            // 紀錄選項
            public static int ActionIndex = -1;
            public static int NdIndex = -1;
            public static int CalibrationFileIndex = -1;
            public static int FFCFileIndex = -1;

            public static int[] PatternNdPos = new int[3];  // 紀錄取像的ND位置
            public static int[] XyzExposureTime = new int[3] { -1, -1, -1 }; // 紀錄取像的曝光時間
            public static Point3D[] AllPatternNdPos = new Point3D[4];   //記錄所有Pattern取像的ND位置

            public static string[] XyzStr = { "X", "Y", "Z" };
            public static int XyzIpStep = 0;
        }

        public static class LightMeasure
        {
            public static MilColorImage TristimulusImage = new MilColorImage();
            public static MilChromaImage ChromaImage = new MilChromaImage();
        }

        public static ClsAutoLuminance AutoLuminance = new ClsAutoLuminance();
    }

   

  

}
