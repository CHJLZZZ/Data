using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Matrox.MatroxImagingLibrary;

namespace LightMeasure
{
    #region --- 01.MilColorImage ---

    
    public class MilColorImage
    {
        public MIL_ID ImgX;
        public MIL_ID ImgY;
        public MIL_ID ImgZ;

        public MilColorImage()
        {
            this.Free();

            this.ImgX = MIL.M_NULL;
            this.ImgY = MIL.M_NULL;
            this.ImgZ = MIL.M_NULL;
        }

        #region --- (1) MilColorImage ---
        public MilColorImage(
            MIL_ID imgX,
            MIL_ID imgY,
            MIL_ID imgZ)
        {
            this.ImgX = imgX;
            this.ImgY = imgY;
            this.ImgZ = imgZ;
        }
        #endregion

        #region --- (2) MilColorImage ---
        public MilColorImage(
            string fiterName,
            string path,
            MIL_ID milSys)
        {

            if (fiterName.ToUpper() == "X")
            {
                MilNetHelper.MilBufferFree(ref this.ImgX);

                MIL.MbufRestore(
                    path,
                    milSys,
                    ref this.ImgX);
            }
            else if (fiterName.ToUpper() == "Y")
            {
                MilNetHelper.MilBufferFree(ref this.ImgY);

                MIL.MbufRestore(
                    path,
                    milSys,
                    ref this.ImgY);
            }
            else if (fiterName.ToUpper() == "Z")
            {
                MilNetHelper.MilBufferFree(ref this.ImgZ);

                MIL.MbufRestore(
                    path,
                    milSys,
                    ref this.ImgZ);
            }

        }
        #endregion

        public void Check()
        {
            if (this.ImgX == MIL.M_NULL)
            {
                throw new Exception("ImgX is M_NULL");
            }

            if (this.ImgY == MIL.M_NULL)
            {
                throw new Exception("ImgY is M_NULL");
            }

            if (this.ImgZ == MIL.M_NULL)
            {
                throw new Exception("ImgZ is M_NULL");
            }
        }

        public void CheckY()
        {
            if (this.ImgY == MIL.M_NULL)
            {
                throw new Exception("ImgY is M_NULL");
            }
        }

        public void Free()
        {
            MilNetHelper.MilBufferFree(ref this.ImgX);
            MilNetHelper.MilBufferFree(ref this.ImgY);
            MilNetHelper.MilBufferFree(ref this.ImgZ);
        }

    }
    #endregion

    #region --- 02.MilMonoImage ---
    public class MilMonoImage
    {
        public MIL_ID Img;

        public MilMonoImage()
        {
            this.Img = MIL.M_NULL;
        }

        public MilMonoImage(
            MIL_ID img)
        {
            this.Img = img;
        }

        #region --- (1) MilMonoImage ---
        public MilMonoImage(
            string fiterName,
            string path,
            MIL_ID milSys)
        {
            MilNetHelper.MilBufferFree(ref this.Img);

            MIL.MbufRestore(
                path,
                milSys,
                ref this.Img);      
        }
        #endregion

        public void Check()
        {
            if (this.Img == MIL.M_NULL)
            {
                throw new Exception("Img is M_NULL");
            }
        }

        public void Free()
        {
            MilNetHelper.MilBufferFree(ref this.Img);
        }
    }
    #endregion

    #region --- 03.MilMatrixImage ---
    public class MilMatrixImage
    {
        // m00 m01 m02
        // m10 m11 m12
        // m20 m21 m22
        public MIL_ID ImgM00;
        public MIL_ID ImgM01;
        public MIL_ID ImgM02;

        public MIL_ID ImgM10;
        public MIL_ID ImgM11;
        public MIL_ID ImgM12;

        public MIL_ID ImgM20;
        public MIL_ID ImgM21;
        public MIL_ID ImgM22;

        public MilMatrixImage()
        {
            this.ImgM00 = MIL.M_NULL;
            this.ImgM01 = MIL.M_NULL;
            this.ImgM01 = MIL.M_NULL;

            this.ImgM10 = MIL.M_NULL;
            this.ImgM11 = MIL.M_NULL;
            this.ImgM12 = MIL.M_NULL;

            this.ImgM20 = MIL.M_NULL;
            this.ImgM21 = MIL.M_NULL;
            this.ImgM22 = MIL.M_NULL;
        }

        public MilMatrixImage(
            MIL_ID ImgM00, MIL_ID ImgM01, MIL_ID ImgM02,
            MIL_ID ImgM10, MIL_ID ImgM11, MIL_ID ImgM12,
            MIL_ID ImgM20, MIL_ID ImgM21, MIL_ID ImgM22)
            : this()
        {
            this.ImgM00 = ImgM00;
            this.ImgM01 = ImgM01;
            this.ImgM02 = ImgM02;

            this.ImgM10 = ImgM10;
            this.ImgM11 = ImgM11;
            this.ImgM12 = ImgM12;

            this.ImgM20 = ImgM20;
            this.ImgM21 = ImgM21;
            this.ImgM22 = ImgM22;
        }

        public void Check()
        {
            if (this.ImgM00 == MIL.M_NULL)
            {
                throw new Exception("ImgM00 is M_NULL");
            }
            if (this.ImgM01 == MIL.M_NULL)
            {
                throw new Exception("ImgM01 is M_NULL");
            }
            if (this.ImgM02 == MIL.M_NULL)
            {
                throw new Exception("ImgM02 is M_NULL");
            }

            if (this.ImgM10 == MIL.M_NULL)
            {
                throw new Exception("ImgM10 is M_NULL");
            }
            if (this.ImgM11 == MIL.M_NULL)
            {
                throw new Exception("ImgM11 is M_NULL");
            }
            if (this.ImgM12 == MIL.M_NULL)
            {
                throw new Exception("ImgM12 is M_NULL");
            }

            if (this.ImgM20 == MIL.M_NULL)
            {
                throw new Exception("ImgM20 is M_NULL");
            }
            if (this.ImgM21 == MIL.M_NULL)
            {
                throw new Exception("ImgM21 is M_NULL");
            }
            if (this.ImgM22 == MIL.M_NULL)
            {
                throw new Exception("ImgM22 is M_NULL");
            }
        }

        public void Free()
        {
            MilNetHelper.MilBufferFree(ref this.ImgM00);
            MilNetHelper.MilBufferFree(ref this.ImgM01);
            MilNetHelper.MilBufferFree(ref this.ImgM02);

            MilNetHelper.MilBufferFree(ref this.ImgM10);
            MilNetHelper.MilBufferFree(ref this.ImgM11);
            MilNetHelper.MilBufferFree(ref this.ImgM12);

            MilNetHelper.MilBufferFree(ref this.ImgM20);
            MilNetHelper.MilBufferFree(ref this.ImgM21);
            MilNetHelper.MilBufferFree(ref this.ImgM22);
        }

    }
    #endregion

}
