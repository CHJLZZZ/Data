using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Globalization;
using CommonSettings;
//using System.Windows.Media.Media3D;

namespace BaseTool
{
    public partial class Point3D
    {
        public int X;
        public int Y;
        public int Z;

        public Point3D(int[] arr)
        {
            if (arr.Length > 3)
            {
                X = -1;
                Y = -1;
                Z = -1;
            }
            else
            {
                X = arr[0];
                Y = arr[1];
                Z = arr[2];
            }
        }

    }

    public class cls29MSetting
    {
        const bool Browsable = !GlobalConfig.IsSecretHidden;

        public void Create(cls29MSetting clsRecipe, string filename)
        {

            string directory = Path.GetDirectoryName(filename);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(cls29MSetting));
            TextWriter writer = new StreamWriter(filename);

            serializer.Serialize(writer, clsRecipe);
            writer.Close();
        }

        public cls29MSetting Read(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(cls29MSetting));
            FileStream fp = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            cls29MSetting Sfp = (cls29MSetting)serializer.Deserialize(fp);
            fp.Close();

            return Sfp;
        }

        #region "--- 01. 一般 ---"            

        [Category("01. 一般"), DisplayName("01. 偵錯存圖"), Description("存出偵錯處理影像"), Browsable(Browsable)]
        public bool IsSaveDebugImage { get; set; } = false;

        #endregion

        #region "--- 02. 相機 ---"

        [Category("02. 相機"), DisplayName("01. X-Filter Gain (1~32)"), Description("流程中X轉盤使用的Gain值 (1~32)"), Browsable(Browsable)]
        public int CameraGainX { get; set; } = 0;

        [Category("02. 相機"), DisplayName("02. Y-Filter Gain (1~32)"), Description("流程中Y轉盤使用的Gain值 (1~32)"), Browsable(Browsable)]
        public int CameraGainY { get; set; } = 0;

        [Category("02. 相機"), DisplayName("03. Z-Filter Gain (1~32)"), Description("流程中Z轉盤使用的Gain值 (1~32)"), Browsable(Browsable)]
        public int CameraGainZ { get; set; } = 0;

        #endregion

        #region "--- 03. 自動曝光 & ROI使用&設定 ---"

        [Category("03. 自動曝光"), DisplayName("01. 啟動自動曝光"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool AutoExposureEnable { get; set; } = true;

        [Category("03. 自動曝光"), DisplayName("02. 自動曝光存圖"), Description("存出自動曝光結果影像"), Browsable(Browsable)]
        public bool AutoExposure_SaveImage { get; set; } = true;

        [Category("03. 自動曝光"), DisplayName("03. 首次調整曝光時間"), Description("單步調整曝光時間量(ns)"), Browsable(Browsable)]
        public int AutoExposureChangeStep { get; set; } = 100;

        [Category("03. 自動曝光"), DisplayName("04. 最小曝光時間"), Description("最小曝光時間(ns)"), Browsable(Browsable)]
        public int AutoExposureMinTime { get; set; } = 300000;

        [Category("03. 自動曝光"), DisplayName("05. 最大曝光時間"), Description("最大曝光時間(ns)"), Browsable(Browsable)]
        public int MaxExposureTime { get; set; } = 20000;

        [Category("03. 自動曝光"), DisplayName("06. 預設曝光時間"), Description("初始曝光時間(ns)"), Browsable(Browsable)]
        public int[] ExposureTimedDefault { get; set; } = { 100000, 100000, 100000 };

        [Category("03. 自動曝光"), DisplayName("07. 目標灰階(R)"), Description("最亮點目標灰階值"), Browsable(Browsable)]
        public int[] AutoExposureTargetGray { get; set; } = { 3480, 3480, 3480 };

        [Category("03. 自動曝光"), DisplayName("08. 灰階誤差容許範圍"), Description("最亮點與目標灰階值容許誤差 (0.00 ~ 1.00), X. Y. Z filter 通用"), Browsable(Browsable)]
        public double AutoExposureGrayTolerance { get; set; } = 0.05;

        [Category("03. 自動曝光"), DisplayName("11. 是否使用ROI)"), Description("使用: true;\t不使用: false"), Browsable(Browsable)]
        public bool AutoExposureIsUseRoi { get; set; } = false;

        [Category("03. 自動曝光"), DisplayName("12. ROI Start X"), Description("ROI左上頂點X座標"), Browsable(Browsable)]
        public int AutoExposureRoiX { get; set; } = 0;

        [Category("03. 自動曝光"), DisplayName("13. ROI Start Y"), Description("ROI左上頂點Y座標"), Browsable(Browsable)]
        public int AutoExposureRoiY { get; set; } = 0;

        [Category("03. 自動曝光"), DisplayName("14. ROI Width"), Description("ROI寬"), Browsable(Browsable)]
        public int AutoExposureRoiWidth { get; set; } = 0;

        [Category("03. 自動曝光"), DisplayName("15. ROI Height"), Description("ROI高"), Browsable(Browsable)]
        public int AutoExposureRoiHeight { get; set; } = 0;

        [Category("03. 自動曝光"), DisplayName("16. ROI GrayBypassPercent"), Description("Bypass前0~99%值"), Browsable(Browsable)]
        public double AutoExposureRoiGrayBypassPercent { get; set; } = 0;

        [Category("03. 自動曝光"), DisplayName("21. 固定曝光時間"), Description("固定曝光時間 (us)"), Browsable(Browsable)]
        public int[] FixedExposureTime { get; set; } = { 100000, 100000, 100000 };

        #endregion

        #region "--- 04.OneColor Function ---"

        [Category("04. OneColor Function"), DisplayName("01. 啟動設定校正的 XYZ"), Description("X Enable, Y Enable, Z Enable"), Browsable(Browsable)]
        public bool[] OneColorCalibration_XYZ_Enable { get; set; } = { false, true, false };

        [Category("04. OneColor Function"), DisplayName("02. XYZ Filter"), Description("X,Y,Z"), Browsable(Browsable)]
        public int[] FilterWheel_XYZ_OneColor { get; set; } = { 0, 1, 2 };

        [Category("04. OneColor Function"), DisplayName("03. ND Filter"), Description("X,Y,Z"), Browsable(Browsable)]
        public int[] FilterWheel_ND_OneColor { get; set; } = { 0, 0, 0 };

        #endregion

        #region "--- 05.第三方驗證"

        [Category("05. 第三方驗證"), DisplayName("01. Type"), Description(""), Browsable(Browsable)]
        public string SpectralType { get; set; } = "";

        [Category("05. 第三方驗證"), DisplayName("02. ROI Start X"), Description("ROI左上頂點X座標"), ReadOnly(true)]
        public int ThirdPartyRoiX { get; set; } = 0;

        [Category("05. 第三方驗證"), DisplayName("03. ROI Start Y"), Description("ROI左上頂點Y座標"), ReadOnly(true)]
        public int ThirdPartyRoiY { get; set; } = 0;

        [Category("05. 第三方驗證"), DisplayName("04. ROI Width"), Description("ROI寬"), ReadOnly(true)]
        public int ThirdPartyRoiWidth { get; set; } = 0;

        [Category("05. 第三方驗證"), DisplayName("05. ROI Height"), Description("ROI高"), ReadOnly(true)]
        public int ThirdPartyRoiHeight { get; set; } = 0;

        #endregion

        #region "--- 06.Hot Pixel ---"

        [Category("06. Hot Pixel"), DisplayName("01. 噪點移除流程"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool HotPixelIsDoStep { get; set; } = true;

        [Category("06. Hot Pixel"), DisplayName("02. Pitch"), Description("X,Y,Z,W"), Browsable(Browsable)]
        public int[] HotPixel_Pitch { get; set; } = { 1, 1, 1, 1 };

        [Category("06. Hot Pixel"), DisplayName("03. B_TH"), Description("X,Y,Z,W"), Browsable(Browsable)]
        public double[] HotPixel_BTH { get; set; } = { 1, 1, 1, 1 };

        [Category("06. Hot Pixel"), DisplayName("04. D_TH"), Description("X,Y,Z,W"), Browsable(Browsable)]
        public double[] HotPixel_DTH { get; set; } = { 1, 1, 1, 1 };

        #endregion

        #region "--- 07.FFC ---"

        [Category("07. 一般"), DisplayName("01. 平場校正流程"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool FfcIsDoStep { get; set; } = true;

        #endregion

        #region "--- 08.Distortion Calibration ---"

        [Category("08. 畸變校正"), DisplayName("01. 開啟流程"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool CalibrationIsDoStep { get; set; } = true;

        #endregion

        #region "--- 09.Laser Calibration ---"

        [Category("09. 雷射結構光校正"), DisplayName("01. 開啟流程"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool LaserCalibrationIsDoStep { get; set; } = false;

        [Category("09. 雷射結構光校正"), DisplayName("02. ROI(x,y,w,h)"), Description("X,Y,Z,W"), Browsable(Browsable)]
        public string[] LaserCalibration_ROI { get; set; } = { "0,0,0,0", "0,0,0,0", "0,0,0,0" };

        #endregion             

        #region "--- 10.Local Search ---"

        [Category("10. Local Search"), DisplayName("01. 是否執行 Pixel Align"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool PixelAlignIsDoStep { get; set; } = false;

        [Category("10. Local Search"), DisplayName("02. X方向顆數"), Description("500,1000"), Browsable(Browsable)]
        public int[] PixelAlign_panelInfo_ResolutionX { get; set; } = { 1000 };

        [Category("10. Local Search"), DisplayName("03. Y方向顆數"), Description("500,1000"), Browsable(Browsable)]
        public int[] PixelAlign_panelInfo_ResolutionY { get; set; } = { 700 };

        [Category("10. Local Search"), DisplayName("04. Coarse_Threshold"), Description("500,1000"), Browsable(Browsable)]
        public int[] PixelAlign_Coarse_Threshold { get; set; } = { 800 };

        [Category("10. Local Search"), DisplayName("05. DilateNums"), Description("10,20"), Browsable(Browsable)]
        public int[] PixelAlign_Coarse_CloseNums { get; set; } = { 10 };

        [Category("10. Local Search"), DisplayName("06. FindPanelPixel_AreaMin"), Description("0,10"), Browsable(Browsable)]
        public int[] PixelAlign_FindPanelPixel_AreaMin { get; set; } = { 0 };

        [Category("10. Local Search"), DisplayName("07. FindPanelPixel_FirstPixelX"), Description("100,500,1000"), Browsable(Browsable)]
        public double[] PixelAlign_FindPanelPixel_FirstPixelX { get; set; } = { 600.0 };

        [Category("10. Local Search"), DisplayName("08. FindPanelPixel_FirstPixelY"), Description("100,500,1000"), Browsable(Browsable)]
        public double[] PixelAlign_FindPanelPixel_FirstPixelY { get; set; } = { 500 };

        [Category("10. Local Search"), DisplayName("09. FindPanelPixel_RowFindPitchX"), Description("0.0,1.0,5.0"), Browsable(Browsable)]
        public double[] PixelAlign_FindPanelPixel_RowFindPitchX { get; set; } = { 4.0 };

        [Category("10. Local Search"), DisplayName("10. FindPanelPixel_RowFindPitchY"), Description("0.0,1.0,5.0"), Browsable(Browsable)]
        public double[] PixelAlign_FindPanelPixel_RowFindPitchY { get; set; } = { 0.0 };

        [Category("10. Local Search"), DisplayName("11. FindPanelPixel_ColFindPitchX"), Description("0.0,1.0,5.0"), Browsable(Browsable)]
        public double[] PixelAlign_FindPanelPixel_ColFindPitchX { get; set; } = { 0.0 };

        [Category("10. Local Search"), DisplayName("12. FindPanelPixel_ColFindPitchY"), Description("0.0,1.0,5.0"), Browsable(Browsable)]
        public double[] PixelAlign_FindPanelPixel_ColFindPitchY { get; set; } = { 4.0 };

        [Category("10. Local Search"), DisplayName("13. 是否執行 Pixel Statistic"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool PixelStatIsDoStep { get; set; } = false;

        [Category("10. Local Search"), DisplayName("14. SearchRegion"), Description("1,5,10"), Browsable(Browsable)]
        public int[] PixelAlign_Stat_Radius { get; set; } = { 2 };

        #endregion

        #region "--- 11.FindExtreme ---"

        [Category("11. FindExtreme"), DisplayName("01. 是否執行"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool FindExtremeIsDoStep { get; set; } = false;

        [Category("11. FindExtreme"), DisplayName("02. Kernel Size"), Description("Odd Number >= 3"), Browsable(Browsable)]
        public int[] FindExtremeKernelSize { get; set; } = { 17, 17 };

        [Category("11. FindExtreme"), DisplayName("03. ROI Padding"), Description("ROI Padding >= 0"), Browsable(Browsable)]
        public int FindExtremeROIPadding { get; set; } = 10;

        #endregion

        #region "--- 12.WarpingCalibration ---"

        [Category("12. WarpingCalibration"), DisplayName("01. 是否執行"), Description("執行: true;\t不執行: false"), Browsable(Browsable)]
        public bool WarpingCalibrationIsDoStep { get; set; } = false;

        #endregion


    }
}
