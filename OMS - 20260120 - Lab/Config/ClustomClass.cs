using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BaseTool
{
    #region --- AiryUnit ---



    #endregion --- AiryUnit ---

    public class PointD
    {
        public double X;
        public double Y;
        public PointD(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }
    }

    public enum FW_XYZ_Remark
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    public enum FW_ND_Remark
    {
        [Description("None")]
        None = -1,
        [Description("12.5%")]
        ND_12_5 = 0,
        [Description("1.56%")]
        ND_1_56,
        [Description("0.25%")]
        ND_0_25,
        [Description("100%")]
        ND_100,          
    }

    public enum RGB_Remark
    {
        R = 0,
        G,
        B,
    }

    public class EnumTypeConverter : EnumConverter
    {
        private Type enumType;

        public EnumTypeConverter(Type type) : base(type)
        {
            enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destType)
        {
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, value));
            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi,
                                        typeof(DescriptionAttribute));
            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
                                           object value)
        {
            foreach (FieldInfo fi in enumType.GetFields())
            {
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi,
                                            typeof(DescriptionAttribute));
                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(enumType, fi.Name);
            }
            return Enum.Parse(enumType, (string)value);
        }

        public class CustomCollectionEditor : CollectionEditor
        {
            public CustomCollectionEditor(Type type) : base(type) { }

            protected override CollectionForm CreateCollectionForm()
            {
                CollectionForm form = base.CreateCollectionForm();
                form.StartPosition = FormStartPosition.CenterScreen;

                // 調整對話框的大小
                Form editorForm = form as Form;
                if (editorForm != null)
                {
                    editorForm.Width = 800; // 設定寬度
                    editorForm.Height = 600; // 設定高度
                }
                return form;
            }
        }

        public class FilePathArrayConverter : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var array = value as string[];
                if (array == null) return base.GetProperties(context, value, attributes);

                PropertyDescriptor[] props = new PropertyDescriptor[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    props[i] = new ArrayElementPropertyDescriptor(i);
                }
                return new PropertyDescriptorCollection(props);
            }
        }

        public class ArrayElementPropertyDescriptor : PropertyDescriptor
        {
            private int index;

            public ArrayElementPropertyDescriptor(int index) : base("Item " + index, null)
            {
                this.index = index;
            }

            public override Type ComponentType => typeof(string[]);
            public override bool IsReadOnly => false;
            public override Type PropertyType => typeof(string);

            public override object GetValue(object component)
            {
                var array = component as string[];
                return array?[index];
            }

            public override void SetValue(object component, object value)
            {
                var array = component as string[];
                if (array != null) array[index] = value as string;
            }

            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;
        }
    }
}
