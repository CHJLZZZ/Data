using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Reflection;

using BaseTool;
using CommonSettings;

namespace BaseTool
{
    public class clsSystemSetting
    {
        const bool Browsable = !GlobalConfig.IsSecretHidden;

        #region --- 01. HardWare Setting ---

        [TypeConverter(typeof(MototrControlPackageConverter))]
        [Category("01. HardWare Setting"), DisplayName("01. Motor Control Package"), Description("硬體選擇 : 轉盤與馬達控制組合")]
        public EnumMotoControlPackage MotorControl_Package { get; set; } = EnumMotoControlPackage.Ascom_Wheel_Airy_Motor;

        [TypeConverter(typeof(XYZFilter_PositionModeConverter))]
        [Category("01. HardWare Setting"), DisplayName("02. XYZ Filter Position Mode"), Description("硬體選擇 : XYZ Filter 可移動位置模式")]
        public EnumXYZFilter_PositionMode XYZFilter_PositionMode { get; set; } = EnumXYZFilter_PositionMode.AIC_4_Positions;

        #endregion

        #region --- 02. Mode Setting ---

        [TypeConverter(typeof(MototrControlPackageConverter))]
        [Category("02. Mode Setting"), DisplayName("01. Measure Mode"), Description("模式選擇")]
        public EnumMeasureMode MeasureMode { get; set; } = EnumMeasureMode.Luminance;
            
        #endregion

        #region --- 03. Camera ---

        [TypeConverter(typeof(CamTypeConverter))]
        [Category("03. Camera"), DisplayName("01. Cam Type"), Description("")]
        public string Camera_Type { get; set; } = "M_SYSTEM_RADIENTPRO";

        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("03. Camera"), DisplayName("02. Cam File"), Description("")]
        public string Camera_CamFile { get; set; } = @"D:\OpticalMeasurementData\LUM\Config\Vieworks_VA-61MC_2Tap_SW_Infinite.dcf";

        [Category("03. Camera"), DisplayName("03. Cam No"), Description("")]
        public int Camera_CamNo { get; set; } = 0;

        [Category("03. Camera"), DisplayName("04. Normal Angle(0D)"), Description("ex. true/false"),Browsable(Browsable)]
        public bool NormalAngle_0D { get; set; } = false;

        [Category("03. Camera"), DisplayName("05. Focal Length (mm)"), Description("Focal Length"), Browsable(Browsable)]
        public double Camera_FocalLength { get; set; } = 0.0;

        [Category("03. Camera"), DisplayName("06. Pixel Size (um)"), Description("Pixel Size"), Browsable(Browsable)]
        public double Camera_PixelSize { get; set; } = 0.0;

        #endregion

        #region --- 04. ASCOM ---

        [TypeConverter(typeof(WheelNameConverter))]
        [Category("04. ASCOM"), DisplayName("01. XYZ Filter Wheel Name"), Description(""), Browsable(Browsable)]
        public string XYZFilterName { get; set; } = "ASCOM.EFW2.FilterWheel";

        [TypeConverter(typeof(WheelNameConverter))]
        [Category("04. ASCOM"), DisplayName("02. ND Filter Wheel Name"), Description(""), Browsable(Browsable)]
        public string NDFilterName { get; set; } = "ASCOM.EFW2_2.FilterWheel";

        #endregion

        #region --- 04. Auo Motor ---

        [TypeConverter(typeof(FocuserNameConverter))]
        [Category("04. Auo Motor"), DisplayName("01. Motor Com Port"), Description(""), Browsable(Browsable)]
        public string AuoMotor_Comport { get; set; } = "COM1";
               
        [Category("04. Auo Motor"), DisplayName("02. Motor Move Speed"), Description(""), Browsable(Browsable)]
        public int AuoMotor_MoveSpeed { get; set; } = 500;

        [Category("04. Auo Motor"), DisplayName("03. Motor Move Timeout"), Description(""), Browsable(Browsable)]
        public int AuoMotor_MoveTimeout { get; set; } = 10000;

        #endregion

        #region --- 05. CircleTac ---

        [Category("05. CircleTac"), DisplayName("01. Read Mcu"), Description("")]
        public bool CircleTac_ReadMcu { get; set; } =false;

        [Category("05. CircleTac"), DisplayName("02. Init Pos : Focuser"), Description("")]
        public int CircleTac_InitPos_Focuser { get; set; } = 0;

        [Category("05. CircleTac"), DisplayName("03. Init Pos : Aperture"), Description("")]
        public int CircleTac_InitPos_Aperture { get; set; } = 0;

        [Category("05. CircleTac"), DisplayName("04. Init Pos : XYZ Filter"), Description("")]
        public int CircleTac_InitPos_FW1 { get; set; } = 0;

        [Category("05. CircleTac"), DisplayName("05. Init Pos : ND Filter"), Description("")]
        public int CircleTac_InitPos_FW2 { get; set; } = 0;

        #endregion

        #region --- 06. Focuse Motor ---

        [TypeConverter(typeof(FocuserNameConverter))]
        [Category("06. Focuser"), DisplayName("01. Focuser Motor Com Port"), Description("Focuser Motor's COM Port"), Browsable(Browsable)]
        public string FocuserMotor_ComPort { get; set; } = "COM1";

        [TypeConverter(typeof(FocuserNameConverter))]
        [Category("06. Focuser"), DisplayName("02. FW1 Com Port (for 211 USB)"), Description("FW1's COM Port"), Browsable(Browsable)]
        public string FocuserMotor_ComPort_2 { get; set; } = "COM1";

        [TypeConverter(typeof(FocuserNameConverter))]
        [Category("06. Focuser"), DisplayName("03. FW2 Com Port (for 211 USB)"), Description("FW2's COM Port"), Browsable(Browsable)]
        public string FocuserMotor_ComPort_3 { get; set; } = "COM1";

        #endregion

        #region --- 08. UrRobot ---

        [Category("08. urRobot"), DisplayName("01. UrRobot IPAddress"), Description("UrRobot IPAddress"), Browsable(Browsable)]
        public string UrRobot_IPAddress { get; set; } = "192.168.255.255";

        [Category("08. urRobot"), DisplayName("11. UrRobot Acc"), Description("UrRobot Acc"), Browsable(Browsable)]
        public double UrRobot_Acc { get; set; } = 0.15;

        [Category("08. urRobot"), DisplayName("12. UrRobot Speed"), Description("UrRobot Speed"), Browsable(Browsable)]
        public double UrRobot_Speed { get; set; } = 0.15;

        [Category("08. urRobot"), DisplayName("13. UrRobot Time "), Description("UrRobot Time"), Browsable(Browsable)]
        public double UrRobot_Time { get; set; } = 0.0;

        [Category("08. urRobot"), DisplayName("14. UrRobot Blendradius"), Description("UrRobot Blendradius"), Browsable(Browsable)]
        public double UrRobot_Blendradius { get; set; } = 0.0;

        #endregion

        #region --- 09. X PLC ---

        [Category("09. X PLC"), DisplayName("01. X PLC IPAddress"), Description("X PLC IPAddress"), Browsable(Browsable)]
        public string X_PLC_IPAddress { get; set; } = "192.168.1.1";

        [Category("09. X PLC"), DisplayName("02. X PLC Timeout"), Description("X PLC Timeout"), Browsable(Browsable)]
        public int X_PLC_Timeout { get; set; } = 3000;

        #endregion

        #region --- 10. D65 Light ---

        [Category("10. D65 Light"), DisplayName("01. D65 Light Comport"), Description("D65 Light Comport"), Browsable(Browsable)]
        public int D65_Light_Comtport { get; set; } = 1;

        [Category("10. D65 Light"), DisplayName("02. D65 Light BaudRate"), Description("D65 Light BaudRate"), Browsable(Browsable)]
        public int D65_Light_Baudrate { get; set; } = 9600;

        #endregion

        #region --- 11. SR-3A ---

        [Category("11. SR-3A"), DisplayName("01. SR-3A Comport"), Description("SR-3A Comport"), Browsable(Browsable)]
        public int SR3_Comtport { get; set; } = 1;

        [Category("11. SR-3A"), DisplayName("02. SR-3A BaudRate"), Description("SR-3A BaudRate"), Browsable(Browsable)]
        public int SR3_Baudrate { get; set; } = 9600;

        #endregion

        #region --- 12. LightSource A ---

        [Category("12. LightSource A"), DisplayName("01. LightSourceA IP"), Description("LightSourceA IP"), Browsable(Browsable)]
        public string LightSourceA_IP { get; set; } = "";

        [Category("12. LightSource A"), DisplayName("02. LightSourceA Port"), Description("LightSourceA Port"), Browsable(Browsable)]
        public int LightSourceA_Port { get; set; } = 80;

        #endregion


        #region --- 13. CornerstoneB ---

        [Category("13. CornerstoneB"), DisplayName("01. CornerstoneB Comport"), Description("CornerstoneB Comport"), Browsable(Browsable)]
        public int CornerstoneB_Comtport { get; set; } = 1;

        [Category("13. CornerstoneB"), DisplayName("02. CornerstoneB BaudRate"), Description("CornerstoneB BaudRate"), Browsable(Browsable)]
        public int CornerstoneB_Baudrate { get; set; } = 9600;

        #endregion

        #region --- 13. CornerstoneB ---

        [Category("14. PowerSupply"), DisplayName("01. PowerSupply Comport"), Description("PowerSupply Comport"), Browsable(Browsable)]
        public int PowerSupply_Comtport { get; set; } = 1;

        [Category("14. PowerSupply"), DisplayName("02. PowerSupply BaudRate"), Description("PowerSupply BaudRate"), Browsable(Browsable)]
        public int PowerSupply_Baudrate { get; set; } = 9600;

        #endregion

        #region --- 方法函式 ---

        #region --- Create ---
        public void Create(clsSystemSetting clsRecipe, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(clsSystemSetting));
            TextWriter writer = new StreamWriter(filename);

            serializer.Serialize(writer, clsRecipe);
            writer.Close();
        }
        #endregion

        #region --- Read ---
        public clsSystemSetting Read(string filename)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(clsSystemSetting));
            FileStream fp = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            clsSystemSetting Sfp = (clsSystemSetting)serializer.Deserialize(fp);
            fp.Close();

            return Sfp;
        }
        #endregion

        #endregion

        #region --- [01] CamTypeConverter ---
        public class CamTypeConverter : StringConverter
        {
            public override Boolean GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }
            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                // 0=純色，1=漸層
                List<string> list = new List<string>();
                list.Add("M_SYSTEM_HOST");
                list.Add("M_SYSTEM_DEFAULT");
                list.Add("M_SYSTEM_SOLIOS");
                list.Add("M_SYSTEM_RADIENTPRO");
                list.Add("M_SYSTEM_RADIENTEVCL");
                list.Add("M_SYSTEM_RADIENTCXP");
                list.Add("M_SYSTEM_RAPIXOCXP");

                list.Add("Z_SYSTEM");

                return new StandardValuesCollection(list);
            }
        }

        #endregion

        #region --- [02] WheelNameConverter ---
        public class WheelNameConverter : StringConverter
        {
            public override Boolean GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }
            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> list = new List<string>();
                list.Add("ASCOM.EFW2.FilterWheel");
                list.Add("ASCOM.EFW2_2.FilterWheel");
                list.Add("");

                return new StandardValuesCollection(list);
            }
        }

        #endregion

        #region --- [03] FocuserNameConverter ---
        public class FocuserNameConverter : StringConverter
        {
            public override Boolean GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }
            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> list = new List<string>();
                string[] myPort;
                myPort = System.IO.Ports.SerialPort.GetPortNames();
                list.AddRange(myPort);
                list.Add("");

                return new StandardValuesCollection(list);
            }
        }

        #endregion

        #region --- [04] MototrControlPackageConverter ---
        class MototrControlPackageConverter : EnumConverter
        {
            private Type _enumType;

            public MototrControlPackageConverter(Type type) : base(type)
            {
                _enumType = type;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
            {
                return destType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
            {
                FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                if (dna != null)
                    return dna.Description;
                else
                    return value.ToString();
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
            {
                return srcType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                foreach (FieldInfo fi in _enumType.GetFields())
                {
                    DescriptionAttribute dna =
                    (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                    if ((dna != null) && ((string)value == dna.Description))
                        return Enum.Parse(_enumType, fi.Name);
                }

                return Enum.Parse(_enumType, (string)value);
            }

        }

        #endregion

        #region --- [05] XYZFilter_PositionModeConverter ---
        class XYZFilter_PositionModeConverter : EnumConverter
        {
            private Type _enumType;

            public XYZFilter_PositionModeConverter(Type type) : base(type)
            {
                _enumType = type;
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
            {
                return destType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
            {
                FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                if (dna != null)
                    return dna.Description;
                else
                    return value.ToString();
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
            {
                return srcType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                foreach (FieldInfo fi in _enumType.GetFields())
                {
                    DescriptionAttribute dna =
                    (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                    if ((dna != null) && ((string)value == dna.Description))
                        return Enum.Parse(_enumType, fi.Name);
                }

                return Enum.Parse(_enumType, (string)value);
            }

        }

        #endregion

    }
}
