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

namespace OpticalMeasuringSystem
{
    public class LumCorrectionPara
    {

        #region --- 01. HardWare Setting ---

        [Category("01. Lum Correction Para"), DisplayName("01. List of Parameters")]
        public List<CorrectionPara> Para_List { get; set; } = new List<CorrectionPara>();

        #endregion


        #region --- 方法函式 ---

        #region --- Create ---
        public void Create(LumCorrectionPara clsRecipe, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LumCorrectionPara));
            TextWriter writer = new StreamWriter(filename);

            serializer.Serialize(writer, clsRecipe);
            writer.Close();
        }
        #endregion

        #region --- Read ---
        public LumCorrectionPara Read(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LumCorrectionPara));
            FileStream fp = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            LumCorrectionPara Sfp = (LumCorrectionPara)serializer.Deserialize(fp);
            fp.Close();

            return Sfp;
        }
        #endregion

        #endregion               

    }

}
