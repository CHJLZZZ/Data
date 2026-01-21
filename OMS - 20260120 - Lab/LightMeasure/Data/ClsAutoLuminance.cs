using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace LightMeasure
{
    public class ClsAutoLuminance
    {

        public List<sLuminancePair> LuminancePair;

        public ClsAutoLuminance()
        {
            LuminancePair = new List<sLuminancePair>();
        }

        public static void WriteData(ClsAutoLuminance autoluminance, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ClsAutoLuminance));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, autoluminance);
            writer.Close();
        }

        public static ClsAutoLuminance ReadData(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ClsAutoLuminance));
            FileStream fp = new FileStream(filename, FileMode.Open, FileAccess.Read);
            ClsAutoLuminance Sfp = (ClsAutoLuminance)serializer.Deserialize(fp);
            fp.Close();
            return Sfp;
        }
    }

    public struct sLuminancePair
    {
        public double Factor;
        public double Luminance;
    }
}
