using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Matrox.MatroxImagingLibrary;

namespace LightMeasure
{
    public class MilNetHelper
    {
        /// <summary>
        ///  Sets whether the printing of error messages to screen is enabled in MIL.NET.
        /// </summary>
        /// <param name="type">MilErrorControlType</param>
        /// <param name="milApp">MIL application's ID</param>
        /// 

        #region --- MilAppException ---
        public static void MilAppException(MilErrorControlType type, ref MIL_ID milApp)
        {
            MIL_INT controlValue = MIL.M_PRINT_ENABLE;
            switch (type)
            {
                case MilErrorControlType.ThrowExecption:
                    controlValue = MIL.M_THROW_EXCEPTION;
                    break;

                case MilErrorControlType.PrintDisable:
                    controlValue = MIL.M_PRINT_DISABLE;
                    break;

                case MilErrorControlType.PrintEnable:
                default:
                    controlValue = MIL.M_PRINT_ENABLE;
                    break;
            }

            MIL.MappControl(
                milApp,
                MIL.M_ERROR,
                controlValue);
        }
        #endregion

        #region --- MilBufferFree ---
        public static void MilBufferFree(ref MIL_ID img)
        {
            if (img != MIL.M_NULL)
            {
                MIL.MbufFree(img);
                img = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilPatternFree ---
        public static void MilPatternFree(ref MIL_ID img)
        {
            if (img != MIL.M_NULL)
            {
                MIL.MpatFree(img);
                img = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilMimFree ---
        public static void MilMimFree(ref MIL_ID mimContext)
        {
            if (mimContext != MIL.M_NULL)
            {
                MIL.MimFree(mimContext);
                mimContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilBlobFree ---
        public static void MilBlobFree(ref MIL_ID milBlobContext)
        {
            if (milBlobContext != MIL.M_NULL)
            {
                MIL.MblobFree(milBlobContext);
                milBlobContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilCalFree ---
        public static void MilCalFree(ref MIL_ID milCalContext)
        {
            if (milCalContext != MIL.M_NULL)
            {
                MIL.McalFree(milCalContext);
                milCalContext = MIL.M_NULL;
            }
        }
        #endregion

        #region --- MilMeasFree ---
        public static void MilMeasFree(ref MIL_ID milMeasContext)
        {
            if (milMeasContext != MIL.M_NULL)
            {
                MIL.MmeasFree(milMeasContext);
                milMeasContext = MIL.M_NULL;
            }
        }
        #endregion


    }
}
