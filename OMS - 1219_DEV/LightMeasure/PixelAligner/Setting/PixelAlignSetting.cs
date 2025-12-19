using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class PixelAlignSetting
    {
        public CoarseAlignSetting CoarseAlignSetting;
        public FindPanelPixelSetting FindPanelPixelSetting;
        public string outPath;

        public PixelAlignSetting()
        {
            this.CoarseAlignSetting = new CoarseAlignSetting();
            this.FindPanelPixelSetting = new FindPanelPixelSetting();
            this.outPath = ".\\";
        }

        public PixelAlignSetting(PixelAlignSetting obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(PixelAlignSetting obj)
        {
            this.CoarseAlignSetting.Copy(obj.CoarseAlignSetting);
            this.FindPanelPixelSetting.Copy(obj.FindPanelPixelSetting);
            this.outPath = obj.outPath;

        }

    }
}
