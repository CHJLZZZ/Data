using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Matrox.MatroxImagingLibrary;

namespace LightMeasure
{
    public class MilChromaImage
    {
        public MIL_ID ImgCx;
        public MIL_ID ImgCy;


        public MilChromaImage()
        {
            this.ImgCx = MIL.M_NULL;
            this.ImgCy = MIL.M_NULL;
        }

        public MilChromaImage(
            MIL_ID imgCx,
            MIL_ID imgCy)
            : this()
        {
            this.ImgCx = imgCx;
            this.ImgCy = imgCy;
        }

        public void Check()
        {
            if (this.ImgCx == MIL.M_NULL)
            {
                throw new Exception("ImgCx is M_NULL");
            }

            if (this.ImgCy == MIL.M_NULL)
            {
                throw new Exception("ImgCy is M_NULL");
            }
        }

        public void Free()
        {
            MilNetHelper.MilBufferFree(ref ImgCx);
            MilNetHelper.MilBufferFree(ref ImgCy);
        }

    }
}
