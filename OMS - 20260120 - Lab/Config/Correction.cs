using CommonSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseTool
{

    public class CorrectionPara
    {
        const bool Browsable = !GlobalConfig.IsSecretHidden;

        [Category("Correction Para"), DisplayName("01. ND")]
        [TypeConverter(typeof(EnumTypeConverter))]
        public FW_ND_Remark ND { get; set; } = FW_ND_Remark.ND_12_5;

        [Category("Correction Para"), DisplayName("02. Target Gray")]
        public int TargetGray { get; set; } = 1000;

        [Category("Correction Para"), DisplayName("03. Light List")]
        public List<double> Light_List { get; set; } = new List<double>();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("Correction Para"), DisplayName("11. X Coefficient"), Browsable(Browsable)]
        public CorrectionCoefficient X_Coefficient { get; set; } = new CorrectionCoefficient();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("Correction Para"), DisplayName("12. Y Coefficient"), Browsable(Browsable)]
        public CorrectionCoefficient Y_Coefficient { get; set; } = new CorrectionCoefficient();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Category("Correction Para"), DisplayName("13. Z Coefficient"), Browsable(Browsable)]
        public CorrectionCoefficient Z_Coefficient { get; set; } = new CorrectionCoefficient();

        public override string ToString()
        {
            // 獲取枚舉成員的 FieldInfo
            FieldInfo field = ND.GetType().GetField(ND.ToString());
            // 獲取 DescriptionAttribute
            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            string ND_Msg = attribute.Description;

            return $"{ND_Msg}_{TargetGray}";
        }
    }

    public class CorrectionCoefficient
    {
        [Category("Coefficient"), DisplayName("01. Slope")]
        public double Slope { get; set; } = 1000;

        [Category("Coefficient"), DisplayName("02. Intercept")]
        public double Intercept { get; set; } = 1000;

        [Category("Coefficient"), DisplayName("03. R²")]
        public double RSQ { get; set; } = 1000;

        [Category("Coefficient"), DisplayName("11. Alpha")]
        public double Alpha { get; set; } = 1;


        public override string ToString()
        {
            return $"{Slope}, {Intercept}, {RSQ}, {Alpha}";
        }
    }
}
