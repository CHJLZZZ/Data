using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Matrox.MatroxImagingLibrary;
using BaseTool;

namespace LightMeasure
{
    public class LightMeasurer
    {
        private MIL_ID milSystem;

        private LightMeasurer()
        {
            this.milSystem = MIL.M_NULL;
        }

        public LightMeasurer(MIL_ID milSys)
            : this()
        {
            if (milSys == MIL.M_NULL)
            {
                throw new Exception("milSys is M_NULL.");
            }

            this.milSystem = milSys;
        }

        #region ---Create---

        /// <summary>
        ///  This function is used to generate the correction data.
        /// </summary>
        /// <param name="colorImage">Filter X, Y, Z image</param>
        /// <param name="correctionData">Correction data</param>
        /// 
        #region --- [1] CreateCorrectionData ---
        public void CreateCorrectionData(
            MilColorImage colorImage,
            bool[] xyzEnable,
            ref MeasureData correctionData,
            double BypassPercent = 0.0)
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                // Calculate
                for (int i = 0; i < correctionData.DataList.Count; i++)
                {
                    FourColorData infoManager = correctionData.DataList[i];
                    this.CalculateTristimulus(ref infoManager);

                    this.CalculateRegionMean(
                        imgX,
                        imgY,
                        imgZ,
                        correctionData.RegionMethod,
                        ref infoManager,
                        BypassPercent);


                    this.CalculateCoefficient(correctionData.ExposureTime, ref infoManager);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CreateCorrectionData] {0}",
                        ex.Message));
            }
        }
        #endregion

        /// <summary>
        ///  This function is used to generate the correction data.
        /// </summary>
        /// <param name="monoImage">Filter Y image</param>
        /// <param name="correctionData">Correction data</param>
        /// 
        #region --- [2] CreateCorrectionDataY ---
        public void CreateCorrectionDataY(
            MilMonoImage monoImage,
            ref MeasureData correctionData,
            double BypassPercent = 0.0)
        {
            MIL_ID imgY = MIL.M_NULL;

            try
            {
                // Check
                if (monoImage == null)
                {
                    throw new Exception("monoImage is null.");
                }

                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                monoImage.Check();
                imgY = monoImage.Img;

                // Calculate
                for (int i = 0; i < correctionData.DataList.Count; i++)
                {
                    FourColorData infoManager = correctionData.DataList[i];
                    this.CalculateTristimulusY(ref infoManager);

                    this.CalculateRegionMean(
                        MIL.M_NULL,
                        imgY,
                        MIL.M_NULL,
                        correctionData.RegionMethod,
                        ref infoManager,
                        BypassPercent);

                    this.CalculateCoefficientY(correctionData.ExposureTime, ref infoManager);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CreateCorrectionData] {0}",
                        ex.Message));
            }
        }
        #endregion

        /// <summary>
        ///  This function is used to generate the coefficient image.
        /// </summary>
        /// <param name="correctionData">Correction data</param>
        /// <param name="coeffImage">Coefficien X, Y, Z image</param>
        /// 
        #region --- [3] CreateCoeffImage ---
        public void CreateCoeffImage(
            MeasureData correctionData,
            ref MilColorImage coeffImage)
        {
            MIL_ID coeffX = MIL.M_NULL;
            MIL_ID coeffY = MIL.M_NULL;
            MIL_ID coeffZ = MIL.M_NULL;

            try
            {
                // Check
                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                coeffX = this.CreateCoeffImage(correctionData, "X");
                coeffY = this.CreateCoeffImage(correctionData, "Y");
                coeffZ = this.CreateCoeffImage(correctionData, "Z");

                coeffImage = new MilColorImage(coeffX, coeffY, coeffZ);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref coeffX);
                MilNetHelper.MilBufferFree(ref coeffY);
                MilNetHelper.MilBufferFree(ref coeffZ);

                if (coeffImage != null)
                {
                    coeffImage.Free();
                    coeffImage = null;
                }

                throw new Exception(
                    string.Format(
                        "[CreateCoeffImage] {0}",
                        ex.Message));
            }
        }
        #endregion

        /// <summary>
        ///  This function is used to generate the coefficient Y image.
        /// </summary>
        /// <param name="correctionData">Correction data</param>
        /// <param name="coeffImageY">Coefficien Y image</param>
        /// 
        #region --- [4] CreateCoeffImageY ---
        public void CreateCoeffImageY(
            MeasureData correctionData,
            ref MilMonoImage coeffImageY)
        {
            MIL_ID coeffY = MIL.M_NULL;

            try
            {
                // Check
                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                coeffY = this.CreateCoeffImage(correctionData, "Y");

                coeffImageY = new MilMonoImage(coeffY);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref coeffY);

                if (coeffImageY != null)
                {
                    coeffImageY.Free();
                    coeffImageY = null;
                }

                throw new Exception(
                    string.Format(
                        "[CreateCoeffImage] {0}",
                        ex.Message));
            }
        }
        #endregion

        #region --- [5] CreateCorrectionDataTpat ---
        public void CreateCorrectionDataTpat(
            MilColorImage colorImage,
            MilColorImage colorImage2,
            ref MeasureData correctionData,
            ref MeasureData correctionData2,
            ref MeasureData correctionDataTarget)
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            MIL_ID imgX2 = MIL.M_NULL;
            MIL_ID imgY2 = MIL.M_NULL;
            MIL_ID imgZ2 = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }
                if (colorImage2 == null)
                {
                    throw new Exception("colorImage2 is null.");
                }

                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }
                if (correctionData2 == null)
                {
                    throw new Exception("correctionData2 is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                if (correctionData2.DataList.Count < 1)
                {
                    throw new Exception("correctionData2 DataList count is 0.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                colorImage2.Check();
                imgX2 = colorImage2.ImgX;
                imgY2 = colorImage2.ImgY;
                imgZ2 = colorImage2.ImgZ;

                for (int i = 0; i < correctionData.DataList.Count; i++)
                {
                    FourColorData infoManager = correctionData.DataList[i];
                    FourColorData info2 = correctionData2.DataList[i];
                    FourColorData infoTarget = correctionDataTarget.DataList[i];

                    this.CalculateTristimulus(ref infoManager);
                    this.CalculateTristimulus(ref info2);

                    this.CalculateRegionMean(
                        imgX, imgY, imgZ, correctionData.RegionMethod,
                        ref infoManager);

                    this.CalculateRegionMean(
                        imgX2, imgY2, imgZ2, correctionData2.RegionMethod,
                        ref info2);

                    this.CalculateCoefficient(correctionData.ExposureTime, ref infoManager);
                    this.CalculateCoefficient(correctionData2.ExposureTime, ref info2);

                    //
                    this.CalculateXdY(ref infoTarget);
                    this.CalculateZdY(ref infoTarget);

                    //
                    this.CalculateFactorAB(
                        correctionData.ExposureTime, correctionData2.ExposureTime,
                        infoManager, info2, ref infoTarget);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CreateCorrectionData] {0}",
                        ex.Message));
            }
        }
        #endregion

        #region --- [6] CreateCoeffImageTpat ---
        public void CreateCoeffImageTpat(
            MeasureData correctionDataTarget,
            ref MilColorImage ImgfactorA,
            ref MilColorImage ImgfactorB,
            ref MilMonoImage ImgXdY,
            ref MilMonoImage ImgZdY)
        {
            MIL_ID factorA_X = MIL.M_NULL;
            MIL_ID factorA_Y = MIL.M_NULL;
            MIL_ID factorA_Z = MIL.M_NULL;

            try
            {
                // Check
                if (correctionDataTarget == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionDataTarget.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                factorA_X = this._createFactorAImageTpat(correctionDataTarget, "X");
                factorA_Y = this._createFactorAImageTpat(correctionDataTarget, "Y");
                factorA_Z = this._createFactorAImageTpat(correctionDataTarget, "Z");

                ImgfactorA = new MilColorImage(factorA_X, factorA_Y, factorA_Z);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref factorA_X);
                MilNetHelper.MilBufferFree(ref factorA_Y);
                MilNetHelper.MilBufferFree(ref factorA_Z);

                if (ImgfactorA != null)
                {
                    ImgfactorA.Free();
                    ImgfactorA = null;
                }

                throw new Exception(
                    string.Format(
                        "[CreateCoeffImageTpat] factorA {0}",
                        ex.Message));
            }

            //
            MIL_ID factorB_X = MIL.M_NULL;
            MIL_ID factorB_Y = MIL.M_NULL;
            MIL_ID factorB_Z = MIL.M_NULL;

            try
            {
                factorB_X = this._createFactorBImageTpat(correctionDataTarget, "X");
                factorB_Y = this._createFactorBImageTpat(correctionDataTarget, "Y");
                factorB_Z = this._createFactorBImageTpat(correctionDataTarget, "Z");

                ImgfactorB = new MilColorImage(factorB_X, factorB_Y, factorB_Z);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref factorB_X);
                MilNetHelper.MilBufferFree(ref factorB_Y);
                MilNetHelper.MilBufferFree(ref factorB_Z);

                if (ImgfactorB != null)
                {
                    ImgfactorB.Free();
                    ImgfactorB = null;
                }

                throw new Exception(
                    string.Format(
                        "[CreateCoeffImageTpat] factorB {0}",
                        ex.Message));
            }

            //
            MIL_ID xdY = MIL.M_NULL;
            try
            {
                xdY = this._createXdYImageTpat(correctionDataTarget);
                ImgXdY = new MilMonoImage(xdY);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref xdY);

                if (ImgXdY != null)
                {
                    ImgXdY.Free();
                    ImgXdY = null;
                }

                throw new Exception(
                    string.Format(
                        "[CreateCoeffImageTpat] xdY {0}",
                        ex.Message));
            }

            MIL_ID zdY = MIL.M_NULL;
            try
            {
                zdY = this._createZdYImageTpat(correctionDataTarget);
                ImgZdY = new MilMonoImage(zdY);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref zdY);

                if (ImgZdY != null)
                {
                    ImgZdY.Free();
                    ImgZdY = null;
                }

                throw new Exception(
                    string.Format(
                        "[CreateCoeffImageTpat] zdY {0}",
                        ex.Message));
            }
        }
        #endregion

        #endregion

        #region ---Predict---

        /// <summary>
        ///  Predict result use corrected data.
        /// </summary>
        /// <param name="colorImage">Capture X, Y, Z image</param>
        /// <param name="correctionData">Correction data</param>
        /// <param name="predictData">Predict data</param>
        /// 
        #region --- [1] PredictMeasureData ---
        public void PredictMeasureData(
            MilColorImage colorImage,
            MeasureData correctionData,
            bool[] xyzEnable,
            ref MeasureData predictData)
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                predictData.ImageInfo.Copy(correctionData.ImageInfo);
                predictData.MeasureMatrixInfo.Copy(correctionData.MeasureMatrixInfo);
                predictData.DataList.Clear();
                for (int i = 0; i < correctionData.DataList.Count; i++)
                {
                    FourColorData infoManager = new FourColorData(
                        correctionData.DataList[i]);
                    predictData.DataList.Add(infoManager);

                    this.CalculateRegionMean(
                        imgX,
                        imgY,
                        imgZ,
                        correctionData.RegionMethod,
                        ref infoManager);

                    this.PredictTristimulus(predictData.ExposureTime, ref infoManager);
                    this.PredictChroma(ref infoManager);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[PredictMeasureData] {0}",
                        ex.Message));
            }
        }
        #endregion

        #region --- [2] PredictAmcMeasureData ---
        public void PredictAmcMeasureData(
            MilColorImage colorImage,
            MilColorImage coeffImage,
            MeasureData correctionData,
            ref MeasureData predictData)
        {
            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (correctionData == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                colorImage.Check();

                predictData.ImageInfo.Copy(correctionData.ImageInfo);
                predictData.MeasureMatrixInfo.Copy(correctionData.MeasureMatrixInfo);
                predictData.ExposureTime.Copy(correctionData.ExposureTime);
                predictData.DataList.Clear();
                for (int i = 0; i < correctionData.DataList.Count; i++)
                {
                    FourColorData infoManager = new FourColorData(
                        correctionData.DataList[i]);

                    this.CalculateRegionMean(
                        colorImage.ImgX,
                        colorImage.ImgY,
                        colorImage.ImgZ,
                        correctionData.RegionMethod,
                        ref infoManager);

                    this.CalculateRegionCoeff(
                        coeffImage.ImgX,
                        coeffImage.ImgY,
                        coeffImage.ImgZ,
                        correctionData.RegionMethod,
                        ref infoManager);

                    this.PredictTristimulus(predictData.ExposureTime, ref infoManager);
                    this.PredictChroma(ref infoManager);

                    predictData.DataList.Add(infoManager);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[PredictAmcMeasureData] {0}",
                        ex.Message));
            }
        }
        #endregion

        #region --- [3] PredictMeasureDataTpat ---
        public void PredictMeasureDataTpat(
            MilColorImage colorImage,
            MeasureData correctionDataTarget,
            bool[] xyzEnable,
            ref MeasureData predictData)
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (correctionDataTarget == null)
                {
                    throw new Exception("correctionData is null.");
                }

                if (correctionDataTarget.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                predictData.ExposureTime.Copy(correctionDataTarget.ExposureTime);
                predictData.ImageInfo.Copy(correctionDataTarget.ImageInfo);
                predictData.MeasureMatrixInfo.Copy(correctionDataTarget.MeasureMatrixInfo);
                predictData.DataList.Clear();
                for (int i = 0; i < correctionDataTarget.DataList.Count; i++)
                {
                    FourColorData infoManager = new FourColorData(
                        correctionDataTarget.DataList[i]);
                    predictData.DataList.Add(infoManager);

                    this.CalculateRegionMean(
                        imgX,
                        imgY,
                        imgZ,
                        correctionDataTarget.RegionMethod,
                        ref infoManager);

                    this.PredictTristimulusTpat(predictData.ExposureTime, ref infoManager);
                    this.PredictChroma(ref infoManager);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[PredictMeasureDataTpat] {0}",
                        ex.Message));
            }
        }
        #endregion

        /// <summary>
        ///  Use coefficient image and predict data, predict tristimulus and chroma image.
        /// </summary>
        /// <param name="colorImage">Capture X, Y, Z image</param>
        /// <param name="coeffImage">Coefficient X, Y, Z image</param>
        /// <param name="predictData">Predict data</param>
        /// <param name="tristimulusImage">Tristimulus X, Y, Z image</param>
        /// <param name="chromaImage">Chroma Cx, Cy image</param>
        /// 
        #region --- [4] PredictChromaImage ---
        public void PredictChromaImage(
            MilColorImage colorImage,
            MilColorImage coeffImage,
            MeasureData predictData,
            ref MilColorImage tristimulusImage,
            ref MilChromaImage chromaImage)
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            MIL_ID coeffX = MIL.M_NULL;
            MIL_ID coeffY = MIL.M_NULL;
            MIL_ID coeffZ = MIL.M_NULL;

            MIL_ID imgTristimulusX = MIL.M_NULL;
            MIL_ID imgTristimulusY = MIL.M_NULL;
            MIL_ID imgTristimulusZ = MIL.M_NULL;

            MIL_ID imgChromaX = MIL.M_NULL;
            MIL_ID imgChromaY = MIL.M_NULL;

            MIL_ID imgTemp = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (coeffImage == null)
                {
                    throw new Exception("coeffImage is null.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                coeffImage.Check();
                coeffX = coeffImage.ImgX;
                coeffY = coeffImage.ImgY;
                coeffZ = coeffImage.ImgZ;

                MIL_INT sizeX = MIL.MbufInquire(imgX, MIL.M_SIZE_X);
                MIL_INT sizeY = MIL.MbufInquire(imgX, MIL.M_SIZE_Y);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusZ);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgChromaX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgChromaY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTemp);
                MIL.MbufClear(imgTristimulusX, 0);
                MIL.MbufClear(imgTristimulusY, 0);
                MIL.MbufClear(imgTristimulusZ, 0);
                MIL.MbufClear(imgChromaX, 0);
                MIL.MbufClear(imgChromaY, 0);
                MIL.MbufClear(imgTemp, 0);

                // TristimulusX, Y, Z
                MIL.MimArith(imgX, predictData.ExposureTime.X, imgTristimulusX, MIL.M_DIV_CONST);
                MIL.MimArith(imgY, predictData.ExposureTime.Y, imgTristimulusY, MIL.M_DIV_CONST);
                MIL.MimArith(imgZ, predictData.ExposureTime.Z, imgTristimulusZ, MIL.M_DIV_CONST);
                MIL.MimArith(imgTristimulusX, coeffX, imgTristimulusX, MIL.M_MULT);
                MIL.MimArith(imgTristimulusY, coeffY, imgTristimulusY, MIL.M_MULT);
                MIL.MimArith(imgTristimulusZ, coeffZ, imgTristimulusZ, MIL.M_MULT);

                tristimulusImage = new MilColorImage(
                    imgTristimulusX, imgTristimulusY, imgTristimulusZ);

                // X + Y + Z
                MIL.MimArith(imgTristimulusX, imgTristimulusY, imgTemp, MIL.M_ADD);
                MIL.MimArith(imgTemp, imgTristimulusZ, imgTemp, MIL.M_ADD);

                // ChromaX, Y
                MIL.MimArith(imgTristimulusX, imgTemp, imgChromaX, MIL.M_DIV);
                MIL.MimArith(imgTristimulusY, imgTemp, imgChromaY, MIL.M_DIV);

                chromaImage = new MilChromaImage(imgChromaX, imgChromaY);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref imgTristimulusX);
                MilNetHelper.MilBufferFree(ref imgTristimulusY);
                MilNetHelper.MilBufferFree(ref imgTristimulusZ);
                MilNetHelper.MilBufferFree(ref imgChromaX);
                MilNetHelper.MilBufferFree(ref imgChromaY);

                if (tristimulusImage != null)
                {
                    tristimulusImage.Free();
                    tristimulusImage = null;
                }

                if (chromaImage != null)
                {
                    chromaImage.Free();
                    chromaImage = null;
                }

                throw new Exception(
                    string.Format(
                        "[PredictChromaImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref imgTemp);
            }
        }
        #endregion

        /// <summary>
        ///  Use coefficient image and predict data, predict tristimulus Y image.
        /// </summary>
        /// <param name="monoImage">Source Image Y</param>
        /// <param name="coeffImage">Coefficient Y image</param>
        /// <param name="predictData">Predict data</param>
        /// <param name="tristimulusImage">Tristimulus Y image</param>
        /// 
        #region --- [5] Predict_TristimulusY_Image ---
        public void Predict_TristimulusY_Image(
            MIL_ID sourceImage,
            MIL_ID coeffImage,
            MeasureData predictData,
            ref MIL_ID tristimulusImage)
        {
            MIL_ID imgTristimulusY = MIL.M_NULL;
            MIL_INT sizeX = 0;
            MIL_INT sizeY = 0;
            bool debug = false;

            try
            {
                // Check
                if (sourceImage == MIL.M_NULL)
                {
                    throw new Exception("sourceImage is null.");
                }

                if (coeffImage == MIL.M_NULL)
                {
                    throw new Exception("coeffImage is null.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                sizeX = MIL.MbufInquire(sourceImage, MIL.M_SIZE_X);
                sizeY = MIL.MbufInquire(sourceImage, MIL.M_SIZE_Y);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);

                MIL.MbufClear(imgTristimulusY, 0);

                // TristimulusY
                MIL.MimArith(sourceImage, predictData.ExposureTime.Y, imgTristimulusY, MIL.M_DIV_CONST);
                if(debug)
                    MIL.MbufSave(@"D:\imgTristimulusY_1.tif", imgTristimulusY);

                MIL.MimArith(imgTristimulusY, coeffImage, tristimulusImage, MIL.M_MULT);
                if (debug)
                {
                    MIL.MbufSave(@"D:\coeffImage_Y.tif", coeffImage);
                    MIL.MbufSave(@"D:\imgTristimulusY_2.tif", tristimulusImage);
                }
                    
            }
            catch (Exception ex)
            {              
                throw new Exception(
                    string.Format(
                        "[Predict_TristimulusY_Image] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref imgTristimulusY);
            }

        }
        #endregion

        #region --- [6] PredictChromaImageTpat ---
        public void PredictChromaImageTpat(
            MilColorImage colorImage,
            MilColorImage ImgFactorA,
            MilColorImage ImgFactorB,
            MilMonoImage ImgXdY,
            MilMonoImage ImgZdY,
            MeasureData predictData,
            ref MilColorImage tristimulusImage,
            ref MilChromaImage chromaImage)
        {
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            MIL_ID factorAx = MIL.M_NULL;
            MIL_ID factorAy = MIL.M_NULL;
            MIL_ID factorAz = MIL.M_NULL;

            MIL_ID factorBx = MIL.M_NULL;
            MIL_ID factorBy = MIL.M_NULL;
            MIL_ID factorBz = MIL.M_NULL;

            // CA
            MIL_ID paramCaXdY = MIL.M_NULL;
            MIL_ID paramCaZdY = MIL.M_NULL;

            MIL_ID imgTristimulusX = MIL.M_NULL;
            MIL_ID imgTristimulusY = MIL.M_NULL;
            MIL_ID imgTristimulusZ = MIL.M_NULL;

            MIL_ID imgChromaX = MIL.M_NULL;
            MIL_ID imgChromaY = MIL.M_NULL;

            MIL_ID imgTemp = MIL.M_NULL;
            MIL_ID paramImgXdY = MIL.M_NULL;
            MIL_ID paramImgZdY = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (ImgFactorA == null)
                {
                    throw new Exception("ImgFactorA is null.");
                }

                if (ImgFactorB == null)
                {
                    throw new Exception("ImgFactorB is null.");
                }

                if (ImgXdY == null)
                {
                    throw new Exception("ImgXdY is null.");
                }

                if (ImgZdY == null)
                {
                    throw new Exception("ImgZdY is null.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                ImgFactorA.Check();
                factorAx = ImgFactorA.ImgX;
                factorAz = ImgFactorA.ImgY;
                factorAy = ImgFactorA.ImgZ;

                ImgFactorB.Check();
                factorBx = ImgFactorB.ImgX;
                factorBz = ImgFactorB.ImgY;
                factorBy = ImgFactorB.ImgZ;

                ImgXdY.Check();
                paramCaXdY = ImgXdY.Img;

                ImgZdY.Check();
                paramCaZdY = ImgZdY.Img;

                MIL_INT sizeX = MIL.MbufInquire(imgX, MIL.M_SIZE_X);
                MIL_INT sizeY = MIL.MbufInquire(imgX, MIL.M_SIZE_Y);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusZ);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgChromaX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgChromaY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTemp);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref paramImgXdY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref paramImgZdY);

                MIL.MbufClear(imgTristimulusX, 0);
                MIL.MbufClear(imgTristimulusY, 0);
                MIL.MbufClear(imgTristimulusZ, 0);
                MIL.MbufClear(imgChromaX, 0);
                MIL.MbufClear(imgChromaY, 0);
                MIL.MbufClear(paramImgXdY, 0);
                MIL.MbufClear(paramImgZdY, 0);

                // y = ax + b
                // TristimulusX, Y, Z
                MIL.MimArith(imgX, predictData.ExposureTime.X, imgTristimulusX, MIL.M_DIV_CONST);
                MIL.MimArith(imgY, predictData.ExposureTime.Y, imgTristimulusY, MIL.M_DIV_CONST);
                MIL.MimArith(imgZ, predictData.ExposureTime.Z, imgTristimulusZ, MIL.M_DIV_CONST);

                MIL.MimArith(imgTristimulusX, factorAx, imgTristimulusX, MIL.M_MULT);
                MIL.MimArith(imgTristimulusY, factorAy, imgTristimulusY, MIL.M_MULT);
                MIL.MimArith(imgTristimulusZ, factorAz, imgTristimulusZ, MIL.M_MULT);

                MIL.MimArith(imgTristimulusX, factorBx, imgTristimulusX, MIL.M_ADD);
                MIL.MimArith(imgTristimulusY, factorBy, imgTristimulusY, MIL.M_ADD);
                MIL.MimArith(imgTristimulusZ, factorBz, imgTristimulusZ, MIL.M_ADD);

                // current's X/Y, Z/Y
                // X * (ca/img)

                MIL.MimArith(imgTristimulusX, imgTristimulusY, paramImgXdY, MIL.M_DIV);
                MIL.MimArith(imgTristimulusZ, imgTristimulusY, paramImgZdY, MIL.M_DIV);

                MIL.MimArith(paramCaXdY, paramImgXdY, paramCaXdY, MIL.M_DIV);
                MIL.MimArith(paramCaZdY, paramImgZdY, paramCaZdY, MIL.M_DIV);
                MIL.MimArith(imgTristimulusX, paramCaXdY, imgTristimulusX, MIL.M_MULT);
                MIL.MimArith(imgTristimulusZ, paramCaXdY, imgTristimulusZ, MIL.M_MULT);

                // X * ca / img
                //MIL.MimArith(imgTristimulusX, paramCaXdY, imgTristimulusX, MIL.M_MULT);
                //MIL.MimArith(imgTristimulusZ, paramCaZdY, imgTristimulusZ, MIL.M_MULT);
                //MIL.MimArith(imgTristimulusX, paramImgXdY, imgTristimulusX, MIL.M_DIV);
                //MIL.MimArith(imgTristimulusZ, paramImgZdY, imgTristimulusZ, MIL.M_DIV);

                tristimulusImage = new MilColorImage(
                    imgTristimulusX, imgTristimulusY, imgTristimulusZ);

                // X + Y + Z
                MIL.MimArith(imgTristimulusX, imgTristimulusY, imgTemp, MIL.M_ADD);
                MIL.MimArith(imgTemp, imgTristimulusZ, imgTemp, MIL.M_ADD);

                // ChromaX, Y
                MIL.MimArith(imgTristimulusX, imgTemp, imgChromaX, MIL.M_DIV);
                MIL.MimArith(imgTristimulusY, imgTemp, imgChromaY, MIL.M_DIV);

                chromaImage = new MilChromaImage(imgChromaX, imgChromaY);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref imgTristimulusX);
                MilNetHelper.MilBufferFree(ref imgTristimulusY);
                MilNetHelper.MilBufferFree(ref imgTristimulusZ);
                MilNetHelper.MilBufferFree(ref imgChromaX);
                MilNetHelper.MilBufferFree(ref imgChromaY);

                if (tristimulusImage != null)
                {
                    tristimulusImage.Free();
                    tristimulusImage = null;
                }

                if (chromaImage != null)
                {
                    chromaImage.Free();
                    chromaImage = null;
                }

                throw new Exception(
                    string.Format(
                        "[PredictChromaImageTpat] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref imgTemp);
                MilNetHelper.MilBufferFree(ref paramImgXdY);
                MilNetHelper.MilBufferFree(ref paramImgZdY);
            }
        }
        #endregion

        /// <summary>
        ///  Generate all statistical images using cog data, tristimulus and chroma images.
        /// </summary>
        /// <param name="cogData">Panel pixel cog position data</param>
        /// <param name="panelInfo">Panel information</param>
        /// <param name="radius">Statistic radius</param>
        /// <param name="tristimulusImage">Tristimulus X, Y, Z image</param>
        /// <param name="chromaImage">Chroma Cx, Cy image</param>
        /// <param name="tristimulusStatImage">Tristimulus X, Y, Z statistic image</param>
        /// <param name="chromaStatImage">Chroma Cx, Cy statistic image</param>
        /// 
        #region --- [7] PredictStatisticImage ---
        public void PredictStatisticImage(
            BlobData[,] cogData,
            PanelInfo panelInfo,
            int radius,
            MilColorImage tristimulusImage,
            MilChromaImage chromaImage,
            ref MilColorImage tristimulusStatImage,
            ref MilChromaImage chromaStatImage)
        {
            MIL_ID tristimulusStatX = MIL.M_NULL;
            MIL_ID tristimulusStatY = MIL.M_NULL;
            MIL_ID tristimulusStatZ = MIL.M_NULL;

            MIL_ID chromaStatCx = MIL.M_NULL;
            MIL_ID chromaStatCy = MIL.M_NULL;

            try
            {
                // Check
                if (cogData == null)
                {
                    throw new Exception("cogData is null.");
                }

                if (panelInfo == null)
                {
                    throw new Exception("panelInfo is null.");
                }

                if (tristimulusImage == null)
                {
                    throw new Exception("tristimulusImage is null.");
                }

                if (chromaImage == null)
                {
                    throw new Exception("chromaImage is null.");
                }

                if(radius <= 0)
                {
                    throw new Exception("radius <= 0.");
                }

                tristimulusImage.Check();
                chromaImage.Check();

                this.PredictStatisticSingleImage(cogData, panelInfo, radius,
                    tristimulusImage.ImgX, ref tristimulusStatX);

                this.PredictStatisticSingleImage(cogData, panelInfo, radius,
                    tristimulusImage.ImgY, ref tristimulusStatY);

                this.PredictStatisticSingleImage(cogData, panelInfo, radius,
                    tristimulusImage.ImgZ, ref tristimulusStatZ);

                this.PredictStatisticSingleImage(cogData, panelInfo, radius,
                    chromaImage.ImgCx, ref chromaStatCx);

                this.PredictStatisticSingleImage(cogData, panelInfo, radius,
                    chromaImage.ImgCy, ref chromaStatCy);

                //
                //this.PredictStatisticValue(cogData, panelInfo, chromaImage.ImgCx, ref chromaStatCx);
                //this.PredictStatisticValue(cogData, panelInfo, chromaImage.ImgCy, ref chromaStatCy);

                tristimulusStatImage = new MilColorImage(
                    tristimulusStatX, tristimulusStatY, tristimulusStatZ);

                chromaStatImage = new MilChromaImage(
                    chromaStatCx, chromaStatCy);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref tristimulusStatX);
                MilNetHelper.MilBufferFree(ref tristimulusStatY);
                MilNetHelper.MilBufferFree(ref tristimulusStatZ);
                MilNetHelper.MilBufferFree(ref chromaStatCx);
                MilNetHelper.MilBufferFree(ref chromaStatCy);

                if (tristimulusStatImage != null)
                {
                    tristimulusStatImage.Free();
                }

                if (chromaStatImage != null)
                {
                    chromaStatImage.Free();
                }

                throw new Exception(
                    string.Format(
                        "[PredictStatisticImage] {0}",
                        ex.Message));
            }
        }
        #endregion


        /// <summary>
        ///  Use coefficient image and predict data, predict tristimulus image.
        /// </summary>
        /// <param name="monoImageY">Capture Y image</param>
        /// <param name="coeffImage">Coefficient image</param>
        /// <param name="predictData">Predict data</param>
        /// <param name="tristimulusImage">Tristimulus Y image</param>
        /// 
        #region --- [8] PredictTristimulusYImage ---
        public void PredictTristimulusYImage(
            MilMonoImage monoImageY,
            MilColorImage coeffImage,
            MeasureData predictData,
            ref MilColorImage tristimulusImage)
        {
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID coeffY = MIL.M_NULL;
            MIL_ID imgTristimulusY = MIL.M_NULL;

            try
            {
                // Check
                if (coeffImage == null)
                {
                    throw new Exception("coeffImage is null.");
                }

                if (predictData == null)
                {
                    throw new Exception("predictData is null.");
                }

                monoImageY.Check();
                imgY = monoImageY.Img;

                coeffImage.CheckY();
                coeffY = coeffImage.ImgY;


                MIL_INT sizeX = MIL.MbufInquire(imgY, MIL.M_SIZE_X);
                MIL_INT sizeY = MIL.MbufInquire(imgY, MIL.M_SIZE_Y);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);

                MIL.MbufClear(imgTristimulusY, 0);

                // Tristimulus Y
                MIL.MimArith(imgY, predictData.ExposureTime.Y, imgTristimulusY, MIL.M_DIV_CONST);
                MIL.MimArith(imgTristimulusY, coeffY, imgTristimulusY, MIL.M_MULT);

                tristimulusImage = new MilColorImage(MIL.M_NULL, imgTristimulusY, MIL.M_NULL);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref imgTristimulusY);

                if (tristimulusImage != null)
                {
                    tristimulusImage.Free();
                    tristimulusImage = null;
                }

                throw new Exception(
                    string.Format(
                        "[PredictTristimulusYImage] {0}",
                        ex.Message));
            }

        }
        #endregion

        /// <summary>
        ///  Generate statistical single images using cog data and image.
        /// </summary>
        /// <param name="cogData">Panel pixel cog position data</param>
        /// <param name="panelInfo">Panel information</param>
        /// <param name="radius">Statistic radius</param>
        /// <param name="srcImage">Single image</param>
        /// <param name="statImage">Single statistic image</param>
        /// 
        #region --- [9] PredictStatisticSingleImage ---
        public void PredictStatisticSingleImage(
            BlobData[,] cogData,
            PanelInfo panelInfo,
            int radius,
            MIL_ID srcImage,
            ref MIL_ID statImage)
        {
            int panelSizeX = 0;
            int panelSizeY = 0;

            float[,] imageData = null;
            MIL_ID srcimageChild = MIL.M_NULL;
            MIL_ID maskImage = MIL.M_NULL;

            statImage = MIL.M_NULL;

            try
            {
                // Check
                if (cogData == null)
                {
                    throw new Exception("cogData is null.");
                }

                if (panelInfo == null)
                {
                    throw new Exception("panelInfo is null.");
                }

                if (srcImage == MIL.M_NULL)
                {
                    throw new Exception("srcImage is M_NULL.");
                }

                panelSizeX = panelInfo.ResolutionX;
                panelSizeY = panelInfo.ResolutionY;

                if (panelSizeX <= 0)
                {
                    throw new Exception("panelSizeX <= 0.");
                }

                if (panelSizeY <= 0)
                {
                    throw new Exception("panelSizeY <= 0.");
                }

                if (radius <= 0)
                {
                    throw new Exception("radius <= 0.");
                }

                imageData = new float[panelSizeY, panelSizeX];

                MIL.MbufAlloc2d(this.milSystem, panelSizeX, panelSizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref statImage);
                MIL.MbufClear(statImage, 0);

                // Mil's rule
                int circleSize = radius * 2 + 1;
                MIL.MbufAlloc2d(
                    this.milSystem,
                    circleSize,
                    circleSize,
                    1 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC,
                    ref maskImage);
                MIL.MbufClear(maskImage, 0);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);

                MIL.MgraArcFill(
                    MIL.M_DEFAULT,
                    maskImage,
                    (double)radius,
                    (double)radius,
                    (double)radius,
                    (double)radius,
                    0,
                    360);

                //Stopwatch sw = new Stopwatch();
                //sw.Restart();
                MIL.MbufChild2d(srcImage, 0, 0, 1, 1, ref srcimageChild);
                for (int y = 0; y < panelSizeY; y++)
                {
                    for (int x = 0; x < panelSizeX; x++)
                    {
                        BlobData b = cogData[y, x];

                        b.CogX = Math.Round(b.CogX, MidpointRounding.AwayFromZero);
                        b.CogY = Math.Round(b.CogY, MidpointRounding.AwayFromZero);

                        // Cut roi calculete
                        int offsetX = (int)b.CogX - radius;
                        int offsetY = (int)b.CogY - radius;

                        // TODO: Check region?
                        MIL.MbufChildMove(srcimageChild, offsetX, offsetY,
                            circleSize, circleSize, MIL.M_DEFAULT);

                        double mean =
                            this.StatMaskMean(srcimageChild, maskImage);

                        imageData[y, x] = (float)mean;
                    }
                }
                //sw.Stop();
                //double microseconds = sw.ElapsedTicks * (1000 * 1000) / Stopwatch.Frequency;
                //Console.WriteLine(string.Format("SP: {0} us", microseconds));

                MIL.MbufPut(statImage, imageData);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref statImage);

                throw new Exception(
                    string.Format(
                        "[PredictStatisticSingleImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref maskImage);
                MilNetHelper.MilBufferFree(ref srcimageChild);
            }
        }
        #endregion

        /// <summary>
        ///  Generate statistical images using cog data、image.
        ///  Generate csv file with pattern type.
        /// </summary>
        /// <param name="cogData">Panel pixel cog position data</param>
        /// <param name="panelInfo">Panel information</param>
        /// <param name="radius">Statistic radius</param>
        /// <param name="srcImage">Single image</param>
        /// <param name="outCSVFilePath">output csv file path</param>
        /// <param name="statisticalShape">BP statistical Shape</param
        /// <param name="statImage">Single statistic image</param>

        #region --- [10] PredictStatisticSingleImage2 ---
        public void PredictStatisticSingleImage2(
            BlobData[,] cogData,
            PanelInfo panelInfo,
            int radius,
            MIL_ID srcImage,
            string outCSVFilePath,
            EnumBPStatisticalShape statisticalShape,
            ref MIL_ID statImage)
        {
            int panelSizeX = 0;
            int panelSizeY = 0;

            float[,] imageData = null;
            MIL_ID srcimageChild = MIL.M_NULL;
            MIL_ID maskImage = MIL.M_NULL;

            statImage = MIL.M_NULL;
            StreamWriter writer = new StreamWriter(outCSVFilePath);
            StringBuilder strBuilder = new StringBuilder();

            try
            {
                // Check
                if (cogData == null)
                {
                    throw new Exception("cogData is null.");
                }

                if (panelInfo == null)
                {
                    throw new Exception("panelInfo is null.");
                }

                if (srcImage == MIL.M_NULL)
                {
                    throw new Exception("srcImage is M_NULL.");
                }

                panelSizeX = panelInfo.ResolutionX;
                panelSizeY = panelInfo.ResolutionY;

                if (panelSizeX <= 0)
                {
                    throw new Exception("panelSizeX <= 0.");
                }

                if (panelSizeY <= 0)
                {
                    throw new Exception("panelSizeY <= 0.");
                }

                if (radius <= 0)
                {
                    throw new Exception("radius <= 0.");
                }

                imageData = new float[panelSizeY, panelSizeX];

                MilNetHelper.MilBufferFree(ref statImage);                

                MIL.MbufAlloc2d(this.milSystem, panelSizeX, panelSizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref statImage);
                MIL.MbufClear(statImage, 0);

                // Mil's rule
                int circleSize = radius * 2 + 1;
                MIL.MbufAlloc2d(
                    this.milSystem,
                    circleSize,
                    circleSize,
                    1 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC,
                    ref maskImage);
                MIL.MbufClear(maskImage, 0);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);

                switch (statisticalShape)
                {
                    case EnumBPStatisticalShape.Circle:
                        MIL.MgraArcFill(MIL.M_DEFAULT, maskImage, (double)radius, (double)radius,
                            (double)radius, (double)radius, 0, 360);
                        break;

                    case EnumBPStatisticalShape.Rectangle:
                        MIL.MimArith(maskImage, 1, maskImage, MIL.M_ADD_CONST + MIL.M_SATURATION);
                        break;
                }
                //MIL.MbufSave(@"D:\maskImage.tif", maskImage);

                //Stopwatch sw = new Stopwatch();
                //sw.Restart();
                MIL.MbufChild2d(srcImage, 0, 0, 1, 1, ref srcimageChild);
                double mean = 0.0;
                
                for (int y = 0; y < panelSizeY; y++)
                {
                    for (int x = 0; x < panelSizeX; x++)
                    {
                        BlobData b = cogData[y, x];

                        b.CogX = Math.Round(b.CogX, MidpointRounding.AwayFromZero);
                        b.CogY = Math.Round(b.CogY, MidpointRounding.AwayFromZero);

                        // Cut roi calculete
                        int offsetX = (int)b.CogX - radius;
                        int offsetY = (int)b.CogY - radius;

                        // TODO: Check region?
                        MIL.MbufChildMove(srcimageChild, offsetX, offsetY,
                            circleSize, circleSize, MIL.M_DEFAULT);

                        mean = this.StatMaskMean(srcimageChild, maskImage);

                        //imageData[y, x] = (float)mean;
                        imageData[y, x] = (float)mean;

                        if (x < panelSizeX - 1)
                            strBuilder.Append(mean.ToString() + ",");                           
                        else
                            strBuilder.Append(mean.ToString());
                    }

                    strBuilder.AppendLine();                   
                }

                //sw.Stop();
                //double microseconds = sw.ElapsedTicks * (1000 * 1000) / Stopwatch.Frequency;
                //Console.WriteLine(string.Format("SP: {0} us", microseconds));

                writer.Write(strBuilder);

                MIL.MbufPut(statImage, imageData);

            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref statImage);
                
                throw new Exception(
                    string.Format(
                        "[PredictStatisticSingleImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref maskImage);
                MilNetHelper.MilBufferFree(ref srcimageChild);

                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
                    
                if(strBuilder != null)                
                    strBuilder = null;                
            }
        }
        #endregion

        #region --- [11] PredictStatisticValue ---
        public void PredictStatisticValue(
            BlobData[,] cogData,
            PanelInfo panelInfo,
            MIL_ID srcImage,
            ref MIL_ID statImage)
        {
            int panelSizeX = 0;
            int panelSizeY = 0;

            float[,] imageData = null;

            statImage = MIL.M_NULL;

            try
            {
                // Check
                if (cogData == null)
                {
                    throw new Exception("cogData is null.");
                }

                if (panelInfo == null)
                {
                    throw new Exception("panelInfo is null.");
                }

                if (srcImage == MIL.M_NULL)
                {
                    throw new Exception("srcImage is M_NULL.");
                }

                panelSizeX = panelInfo.ResolutionX;
                panelSizeY = panelInfo.ResolutionY;

                if (panelSizeX <= 0)
                {
                    throw new Exception("panelSizeX <= 0.");
                }

                if (panelSizeY <= 0)
                {
                    throw new Exception("panelSizeY <= 0.");
                }

                imageData = new float[panelSizeY, panelSizeX];

                MIL.MbufAlloc2d(this.milSystem, panelSizeX, panelSizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref statImage);
                MIL.MbufClear(statImage, 0);

                //Stopwatch sw = new Stopwatch();
                //sw.Restart();
                for (int y = 0; y < panelSizeY; y++)
                {
                    for (int x = 0; x < panelSizeX; x++)
                    {
                        BlobData b = cogData[y, x];

                        b.CogX = Math.Round(b.CogX, MidpointRounding.AwayFromZero);
                        b.CogY = Math.Round(b.CogY, MidpointRounding.AwayFromZero);

                        // Cut roi calculete
                        int offsetX = (int)b.CogX;
                        int offsetY = (int)b.CogY;

                        int[] lFetchedValue = new int[1] { 0 };
                        MIL.MbufGet2d(srcImage, offsetX, offsetY, 1, 1, lFetchedValue);

                        double mean = lFetchedValue[0];

                        imageData[y, x] = (float)mean;
                    }
                }
                //sw.Stop();
                //double microseconds = sw.ElapsedTicks * (1000 * 1000) / Stopwatch.Frequency;
                //Console.WriteLine(string.Format("SP: {0} us", microseconds));

                MIL.MbufPut(statImage, imageData);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref statImage);

                throw new Exception(
                    string.Format(
                        "[PredictStatisticSingleImage] {0}",
                        ex.Message));
            }
            finally
            {
            }

        }
        #endregion

        // §ξ封☆邪★法☆印ξ§
        public void PredictStatisticSingleImageOld(
            BlobData[,] cogData,
            PanelInfo panelInfo,
            int radius,
            MIL_ID srcImage,
            ref MIL_ID statImage)
        {
            int panelSizeX = 0;
            int panelSizeY = 0;

            float[,] imageData = null;
            statImage = MIL.M_NULL;

            try
            {
                // Check
                if (cogData == null)
                {
                    throw new Exception("cogData is null.");
                }

                if (panelInfo == null)
                {
                    throw new Exception("panelInfo is null.");
                }

                if (srcImage == MIL.M_NULL)
                {
                    throw new Exception("srcImage is M_NULL.");
                }

                panelSizeX = panelInfo.ResolutionX;
                panelSizeY = panelInfo.ResolutionY;

                if (panelSizeX <= 0)
                {
                    throw new Exception("panelSizeX <= 0.");
                }

                if (panelSizeY <= 0)
                {
                    throw new Exception("panelSizeY <= 0.");
                }

                if (radius <= 0)
                {
                    throw new Exception("radius <= 0.");
                }

                imageData = new float[panelSizeY, panelSizeX];

                MIL.MbufAlloc2d(this.milSystem, panelSizeX, panelSizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref statImage);
                MIL.MbufClear(statImage, 0);

                Stopwatch sw = new Stopwatch();
                sw.Restart();
                for (int y = 0; y < panelSizeY; y++)
                {
                    for (int x = 0; x < panelSizeX; x++)
                    {
                        BlobData b = cogData[y, x];

                        CircleRegionInfo infoManager = new CircleRegionInfo
                        {
                            CenterX = (int)b.CogX,
                            CenterY = (int)b.CogY,
                            Radius = radius
                        };

                        double mean =
                            this.StatCircleMean(srcImage, infoManager);

                        imageData[y, x] = (float)mean;
                    }
                }
                sw.Stop();
                double ticks = sw.ElapsedTicks;
                Console.WriteLine(string.Format("{0} ms", sw.ElapsedMilliseconds));

                MIL.MbufPut(statImage, imageData);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref statImage);

                throw new Exception(
                    string.Format(
                        "[PredictStatisticSingleImage] {0}",
                        ex.Message));
            }
        }

        #endregion

        #region ---Calculate---

        #region --- CalculateTristimulus ---
        public void CalculateTristimulus(ref MeasureData correctionData)
        {
            for (int i = 0; i < correctionData.DataList.Count; i++)
            {
                FourColorData infoManager = correctionData.DataList[i];
                this.CalculateTristimulus(ref infoManager);
            }
        }
        #endregion

        #region --- CalculateTristimulus ---
        public void CalculateTristimulus(ref FourColorData infoManager)
        {
            CieChromaInfo cieChroma = infoManager.CieChroma;
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;

            this.ChromaToTristimulus(cieChroma, ref tristimulus);
        }
        #endregion

        #region --- CalculateTristimulusY ---
        public void CalculateTristimulusY(ref FourColorData infoManager)
        {
            CieChromaInfo cieChroma = infoManager.CieChroma;
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;

            this.LuminanceToTristimulusY(cieChroma, ref tristimulus);
        }
        #endregion

        #region --- CalculateRegionMean ---
        public void CalculateRegionMean(
            MIL_ID imgX,
            MIL_ID imgY,
            MIL_ID imgZ,
            EnumRegionMethod regionMethod,
            ref FourColorData infoManager,
            double BypassPercent = 0.0)
        {
            double mean;

            if (regionMethod == EnumRegionMethod.Circle)
            {
                if (imgX != MIL.M_NULL)
                {
                    mean = this.StatCircleMean(imgX, infoManager.CircleRegionInfo, BypassPercent);
                    infoManager.GrayMean.X = mean;
                }

                if (imgY != MIL.M_NULL)
                {
                    mean = this.StatCircleMean(imgY, infoManager.CircleRegionInfo, BypassPercent);
                    infoManager.GrayMean.Y = mean;
                }

                if (imgZ != MIL.M_NULL)
                {
                    mean = this.StatCircleMean(imgZ, infoManager.CircleRegionInfo, BypassPercent);
                    infoManager.GrayMean.Z = mean;
                }
            }
            else
            {
                if (imgX != MIL.M_NULL)
                {
                    mean = this.StatRectMean(imgX, infoManager.RectRegionInfo);
                    infoManager.GrayMean.X = mean;
                }

                if (imgY != MIL.M_NULL)
                {
                    mean = this.StatRectMean(imgY, infoManager.RectRegionInfo);
                    infoManager.GrayMean.Y = mean;
                }

                if (imgZ != MIL.M_NULL)
                {
                    mean = this.StatRectMean(imgZ, infoManager.RectRegionInfo);
                    infoManager.GrayMean.Z = mean;
                }
            }
        }
        #endregion

        #region --- CalculateRegionMean ---
        public void CalculateRegionMean(
            MIL_ID imgX,
            MIL_ID imgY,
            MIL_ID imgZ,
            EnumRegionMethod regionMethod,
            ref FourColorData infoManager)
        {
            double mean;

            if (regionMethod == EnumRegionMethod.Circle)
            {
                if(imgX != MIL.M_NULL)
                {
                    mean = this.StatCircleMean(imgX, infoManager.CircleRegionInfo);
                    infoManager.GrayMean.X = mean;
                }                

                if (imgY != MIL.M_NULL)
                {
                    mean = this.StatCircleMean(imgY, infoManager.CircleRegionInfo);
                    infoManager.GrayMean.Y = mean;
                }                

                if (imgZ != MIL.M_NULL)
                {
                    mean = this.StatCircleMean(imgZ, infoManager.CircleRegionInfo);
                    infoManager.GrayMean.Z = mean;
                }                
            }
            else
            {
                if (imgX != MIL.M_NULL)
                {
                    mean = this.StatRectMean(imgX, infoManager.RectRegionInfo);
                    infoManager.GrayMean.X = mean;
                }

                if (imgY != MIL.M_NULL)
                {
                    mean = this.StatRectMean(imgY, infoManager.RectRegionInfo);
                    infoManager.GrayMean.Y = mean;
                }

                if (imgZ != MIL.M_NULL)
                {
                    mean = this.StatRectMean(imgZ, infoManager.RectRegionInfo);
                    infoManager.GrayMean.Z = mean;
                }
            }
        }
        #endregion

        #region --- CalculateRegionCoeff ---
        public void CalculateRegionCoeff(
            MIL_ID coeffImgX,
            MIL_ID coeffImgY,
            MIL_ID coeffImgZ,
            EnumRegionMethod regionMethod,
            ref FourColorData infoManager)
        {
            double mean;
            if (regionMethod == EnumRegionMethod.Circle)
            {
                mean = this.StatCircleMean(coeffImgX, infoManager.CircleRegionInfo);
                infoManager.Coefficient.X = mean;

                mean = this.StatCircleMean(coeffImgY, infoManager.CircleRegionInfo);
                infoManager.Coefficient.Y = mean;

                mean = this.StatCircleMean(coeffImgZ, infoManager.CircleRegionInfo);
                infoManager.Coefficient.Z = mean;
            }
            else
            {
                mean = this.StatRectMean(coeffImgX, infoManager.RectRegionInfo);
                infoManager.Coefficient.X = mean;

                mean = this.StatRectMean(coeffImgY, infoManager.RectRegionInfo);
                infoManager.Coefficient.Y = mean;

                mean = this.StatRectMean(coeffImgZ, infoManager.RectRegionInfo);
                infoManager.Coefficient.Z = mean;
            }
        }
        #endregion

        #region --- CalculateCoefficient ---
        public void CalculateCoefficient(ref MeasureData correctionData)
        {
            // 計算校正係數
            for (int i = 0; i < correctionData.DataList.Count; i++)
            {
                FourColorData infoManager = correctionData.DataList[i];
                this.CalculateCoefficient(correctionData.ExposureTime, ref infoManager);
            }
        }
        #endregion

        #region --- CalculateCoefficient ---
        public void CalculateCoefficient(ThreeDimensionalInfo time, ref FourColorData infoManager)
        {
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;
            ThreeDimensionalInfo grayMean = infoManager.GrayMean;
            ThreeDimensionalInfo coeff = infoManager.Coefficient;

            coeff.X = (grayMean.X > 0) ?
                (tristimulus.X * time.X / grayMean.X) : 0;

            coeff.Y = (grayMean.Y > 0) ?
                (tristimulus.Y * time.Y / grayMean.Y) : 0;

            coeff.Z = (grayMean.Z > 0) ?
                (tristimulus.Z * time.Z / grayMean.Z) : 0;
        }
        #endregion

        #region --- CalculateCoefficientY ---
        public void CalculateCoefficientY(ThreeDimensionalInfo time, ref FourColorData infoManager)
        {
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;
            ThreeDimensionalInfo grayMean = infoManager.GrayMean;
            ThreeDimensionalInfo coeff = infoManager.Coefficient;

            coeff.Y = (grayMean.Y > 0) ?
                (tristimulus.Y * time.Y / grayMean.Y) : 0;
        }
        #endregion

        #region --- CalculateFactorAB ---
        public void CalculateFactorAB(
            ThreeDimensionalInfo timeA, ThreeDimensionalInfo timeB,
            FourColorData infoA, FourColorData infoB, ref FourColorData infoTarget)
        {
            //
            double y1 = infoA.Tristimulus.X;
            double x1 = infoA.GrayMean.X / timeA.X;
            double y2 = infoB.Tristimulus.X;
            double x2 = infoB.GrayMean.X / timeB.X;

            var xValues = new double[] { x1, x2 };
            var yValues = new double[] { y1, y2 };

            double rSquared = 0.0;
            double intercept = 0.0;
            double slope = 0.0;

            this._linearRegression(
                xValues, yValues,
                ref rSquared,
                ref intercept,
                ref slope);

            infoTarget.FactorA.X = slope;
            infoTarget.FactorB.X = intercept;

            // Y
            y1 = infoA.Tristimulus.Y;
            x1 = infoA.GrayMean.Y / timeA.Y;
            y2 = infoB.Tristimulus.Y;
            x2 = infoB.GrayMean.Y / timeB.Y;

            xValues = new double[] { x1, x2 };
            yValues = new double[] { y1, y2 };

            this._linearRegression(xValues, yValues, ref rSquared, ref intercept, ref slope);

            infoTarget.FactorA.Y = slope;
            infoTarget.FactorB.Y = intercept;

            // Z
            y1 = infoA.Tristimulus.Z;
            x1 = infoA.GrayMean.Z / timeA.Z;
            y2 = infoB.Tristimulus.Z;
            x2 = infoB.GrayMean.Z / timeB.Z;

            xValues = new double[] { x1, x2 };
            yValues = new double[] { y1, y2 };

            this._linearRegression(xValues, yValues, ref rSquared, ref intercept, ref slope);

            infoTarget.FactorA.Z = slope;
            infoTarget.FactorB.Z = intercept;
        }
        #endregion

        #region --- CalculateXdY ---
        public void CalculateXdY(ref FourColorData infoTarget)
        {
            infoTarget.XdY = infoTarget.CieChroma.Cy > 0 ?
                 (infoTarget.CieChroma.Cx) / infoTarget.CieChroma.Cy : 0;
        }
        #endregion

        #region --- CalculateZdY ---
        public void CalculateZdY(ref FourColorData infoTarget)
        {
            infoTarget.ZdY = infoTarget.CieChroma.Cy > 0 ?
                (1 - infoTarget.CieChroma.Cx - infoTarget.CieChroma.Cy) / infoTarget.CieChroma.Cy : 0;
        }
        #endregion

        #region --- PredictCircleMean ---
        public void PredictCircleMean(
            MIL_ID imgX,
            MIL_ID imgY,
            MIL_ID imgZ,
            MeasureData correctionData,
            ref MeasureData predictData)
        {
            // 使用設定好的Circle位置資訊算GrayMean
            predictData.DataList.Clear();
            for (int i = 0; i < correctionData.DataList.Count; i++)
            {
                FourColorData infoManager = new FourColorData(
                    correctionData.DataList[i]);
                predictData.DataList.Add(infoManager);

                this.CalculateRegionMean(
                    imgX,
                    imgY,
                    imgZ,
                    correctionData.RegionMethod,
                    ref infoManager);
            }
        }
        #endregion

        #region --- PredictTristimulus ---
        public void PredictTristimulus(ref MeasureData predictData)
        {
            // 使用計算好的Coeff與GrayMean，預測Tristimulus
            for (int i = 0; i < predictData.DataList.Count; i++)
            {
                FourColorData infoManager = predictData.DataList[i];
                this.PredictTristimulus(predictData.ExposureTime, ref infoManager);
            }
        }
        #endregion

        #region --- PredictTristimulus ---
        public void PredictTristimulus(ThreeDimensionalInfo time, ref FourColorData infoManager)
        {
            ThreeDimensionalInfo grayMaena = infoManager.GrayMean;
            ThreeDimensionalInfo coefficient = infoManager.Coefficient;
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;

            tristimulus.X = (time.X > 0) ?
                (grayMaena.X * coefficient.X / time.X) : 0;

            tristimulus.Y = (time.Y > 0) ?
                (grayMaena.Y * coefficient.Y / time.Y) : 0;

            tristimulus.Z = (time.Z > 0) ?
                (grayMaena.Z * coefficient.Z / time.Z) : 0;
        }
        #endregion

        #region --- PredictTristimulusTpat ---
        public void PredictTristimulusTpat(ThreeDimensionalInfo time, ref FourColorData infoManager)
        {
            // y = ax + b, x = mean/time
            ThreeDimensionalInfo grayMaena = infoManager.GrayMean;
            ThreeDimensionalInfo factorA = infoManager.FactorA;
            ThreeDimensionalInfo factorB = infoManager.FactorB;
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;

            tristimulus.X = (time.X > 0) ?
                factorA.X * (grayMaena.X / time.X) + factorB.X : 0;

            tristimulus.Y = (time.Y > 0) ?
                factorA.Y * (grayMaena.Y / time.Y) + factorB.Y : 0;

            tristimulus.Z = (time.Z > 0) ?
                factorA.Z * (grayMaena.Z / time.Z) + factorB.Z : 0;

            //
            tristimulus.X = tristimulus.X * infoManager.XdY / (tristimulus.X / tristimulus.Y);
            //tristimulus.Y = tristimulus.Y;
            tristimulus.Z = tristimulus.Z * infoManager.ZdY / (tristimulus.Z / tristimulus.Y);
        }
        #endregion

        #region --- PredictChroma ---
        public void PredictChroma(ref MeasureData predictData)
        {
            // 使用計算好的Tristimulus，預測色度x & y
            for (int i = 0; i < predictData.DataList.Count; i++)
            {
                FourColorData infoManager = predictData.DataList[i];
                this.PredictChroma(ref infoManager);
            }
        }
        #endregion

        #region --- PredictChroma ---
        public void PredictChroma(ref FourColorData infoManager)
        {
            ThreeDimensionalInfo tristimulus = infoManager.Tristimulus;
            CieChromaInfo cieChromaInfo = infoManager.CieChroma;

            this.TristimulusToChroma(tristimulus, ref cieChromaInfo);
        }
        #endregion

        /// <summary>
        ///  This function use chroma calculate tristimulus.
        /// </summary>
        /// 
        #region --- ChromaToTristimulus ---
        public void ChromaToTristimulus(
            CieChromaInfo cieChroma,
            ref ThreeDimensionalInfo tristimulus)
        {
            double x = 0;
            double y = 0;
            double z = 0;

            this.ChromaToTristimulus(
                cieChroma.Luminance,
                cieChroma.Cx,
                cieChroma.Cy,
                ref x,
                ref y,
                ref z);

            tristimulus.X = x;
            tristimulus.Y = y;
            tristimulus.Z = z;
        }
        #endregion

        /// <summary>
        ///  This function use chroma calculate tristimulus.
        /// </summary>
        /// 
        #region --- ChromaToTristimulus ---
        public void ChromaToTristimulus(
            double luminance,
            double chromaX,
            double chromaY,
            ref double tristimulusX,
            ref double tristimulusY,
            ref double tristimulusZ)
        {
            tristimulusY = luminance;

            tristimulusX = 0;
            tristimulusZ = 0;

            // Avoid divide 0
            if (chromaY > 0)
            {
                tristimulusX = luminance * chromaX / chromaY;

                double tmpChromaZ = (1 - chromaX - chromaY);
                tmpChromaZ = tmpChromaZ < 0 ? 0 : tmpChromaZ;

                tristimulusZ =
                    tmpChromaZ * luminance / chromaY;
            }
        }
        #endregion

        /// <summary>
        ///  This function use luminance calculate tristimulus.
        /// </summary>
        /// 
        #region --- LuminanceToTristimulusY ---
        public void LuminanceToTristimulusY(
            CieChromaInfo cieChroma,
            ref ThreeDimensionalInfo tristimulus)
        {
            tristimulus.X = 0;
            tristimulus.Y = cieChroma.Luminance;
            tristimulus.Z = 0;
        }
        #endregion

        /// <summary>
        ///  This function use tristimulus calculate chroma.
        /// </summary>
        /// 
        #region --- TristimulusToChroma ---
        public void TristimulusToChroma(
            ThreeDimensionalInfo tristimulus,
            ref CieChromaInfo cieChroma)
        {
            double lv = 0;
            double x = 0;
            double y = 0;

            this.TristimulusToChroma(
                tristimulus.X,
                tristimulus.Y,
                tristimulus.Z,
                ref lv,
                ref x,
                ref y);

            cieChroma.Luminance = lv;
            cieChroma.Cx = x;
            cieChroma.Cy = y;
        }
        #endregion

        /// <summary>
        ///  This function use tristimulus calculate chroma.
        /// </summary>
        /// 
        #region --- TristimulusToChroma ---
        public void TristimulusToChroma(
            double tristimulusX,
            double tristimulusY,
            double tristimulusZ,
            ref double luminance,
            ref double chromaX,
            ref double chromaY)
        {
            luminance = tristimulusY;

            chromaX = 0;
            chromaY = 0;

            // Avoid divide 0
            double denominator = tristimulusX + tristimulusY + tristimulusZ;
            if (denominator > 0)
            {
                chromaX = tristimulusX / denominator;
                chromaY = tristimulusY / denominator;
            }
        }
        #endregion

        /// <summary>
        ///  This function use mil calculate the mean value within a circular ROI
        /// </summary>
        /// 
        #region --- StatCircleMean ---
        public double StatCircleMean(
            MIL_ID img,
            CircleRegionInfo regionInfo,
            double BypassPercent = 0.0)
        {
            double mean = 0;
            MIL_ID mask = MIL.M_NULL;
            MIL_ID BinaryImage = MIL.M_NULL;
            MIL_ID BinaryChildImage = MIL.M_NULL;
            MIL_ID MaskChildImage = MIL.M_NULL;

            try
            {
                MIL_INT sizeX = MIL.MbufInquire(img, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT sizeY = MIL.MbufInquire(img, MIL.M_SIZE_Y, MIL.M_NULL);

                MIL.MbufAlloc2d(
                    this.milSystem,
                    sizeX,
                    sizeY,
                    1 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC,
                    ref mask);
                MIL.MbufClear(mask, 0);

                MIL.MgraControl(MIL.M_DEFAULT, MIL.M_COLOR, MIL.M_COLOR_WHITE);

                MIL.MgraArcFill(
                    MIL.M_DEFAULT,
                    mask,
                    (double)regionInfo.CenterX,
                    (double)regionInfo.CenterY,
                    (double)regionInfo.Radius,
                    (double)regionInfo.Radius,
                    0,
                    360);

                if (BypassPercent > 0)
                {
                    System.Drawing.Rectangle childrect = new System.Drawing.Rectangle(regionInfo.CenterX - regionInfo.Radius, regionInfo.CenterY - regionInfo.Radius, regionInfo.Radius * 2 + 1, regionInfo.Radius * 2 + 1);
                    MIL.MbufAlloc2d(milSystem, sizeX, sizeY, 1 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref BinaryImage);
                    MIL.MbufChild2d(mask, childrect.X, childrect.Y, childrect.Width, childrect.Height, ref MaskChildImage);
                    MIL.MimBinarize(img, BinaryImage, MIL.M_PERCENTILE_VALUE + MIL.M_LESS, 100 - BypassPercent, MIL.M_NULL);
                    MIL.MbufChild2d(BinaryImage, childrect.X, childrect.Y, childrect.Width, childrect.Height, ref BinaryChildImage);
                    MIL.MimArith(MaskChildImage, BinaryChildImage, MaskChildImage, MIL.M_MULT);
                }

                Directory.CreateDirectory(@"D:\Test\");
                MIL.MbufSave(@"D:\Test\maskCircle.tif", mask);

                mean = this.StatMaskMean(img, mask);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[StatCircleMean] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref MaskChildImage);
                MilNetHelper.MilBufferFree(ref BinaryChildImage);
                MilNetHelper.MilBufferFree(ref mask);
                MilNetHelper.MilBufferFree(ref BinaryImage);
            }

            return mean;
        }
        #endregion

        /// <summary>
        ///  This function use mil calculate the mean value within a rect ROI
        /// </summary>
        /// 
        #region --- StatRectMean ---
        public double StatRectMean(
            MIL_ID img,
            RectRegionInfo regionInfo)
        {
            double mean = 0;
            MIL_ID mask = MIL.M_NULL;

            try
            {
                MIL_INT sizeX = MIL.MbufInquire(img, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT sizeY = MIL.MbufInquire(img, MIL.M_SIZE_Y, MIL.M_NULL);

                MIL.MbufAlloc2d(
                    this.milSystem,
                    sizeX,
                    sizeY,
                    1 + MIL.M_UNSIGNED,
                    MIL.M_IMAGE + MIL.M_PROC,
                    ref mask);
                MIL.MbufClear(mask, 0);

                MIL.MgraRectFill(
                    MIL.M_DEFAULT,
                    mask,
                    (double)regionInfo.StartX,
                    (double)regionInfo.StartY,
                    (double)(regionInfo.StartX + regionInfo.Width),
                    (double)(regionInfo.StartY + regionInfo.Height));

                //MIL.MbufSave(@"D:\Test\maskRect.tif", mask);

                mean = this.StatMaskMean(img, mask);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[StatCircleMean] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref mask);
            }

            return mean;
        }
        #endregion

        /// <summary>
        ///  This function use mil calculate the mean value within a mask
        /// </summary>
        /// 
        #region --- StatMaskMean ---
        public double StatMaskMean(
            MIL_ID img,
            MIL_ID mask)
        {
            double mean = 0;
            MIL_ID statContext = MIL.M_NULL;
            MIL_ID statResult = MIL.M_NULL;

            try
            {
                MIL.MimAlloc(
                    this.milSystem,
                    MIL.M_STATISTICS_CONTEXT,
                    MIL.M_DEFAULT,
                    ref statContext);
                MIL.MimControl(statContext, MIL.M_STAT_MEAN, MIL.M_ENABLE);

                MIL.MimAllocResult(
                    this.milSystem,
                    MIL.M_DEFAULT,
                    MIL.M_STATISTICS_RESULT,
                    ref statResult);

                MIL.MbufSetRegion(img, mask, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
                MIL.MimStatCalculate(statContext, img, statResult, MIL.M_DEFAULT);
                MIL.MimGetResult(statResult, MIL.M_STAT_MEAN, ref mean);
                MIL.MbufSetRegion(img, MIL.M_NULL, MIL.M_DEFAULT, MIL.M_DELETE, MIL.M_DEFAULT);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[StatMaskMean] {0}",
                        ex.Message));
            }
            finally
            {
                if (statResult != MIL.M_NULL)
                {
                    MIL.MimFree(statResult);
                    statResult = MIL.M_NULL;
                }

                if (statContext != MIL.M_NULL)
                {
                    MIL.MimFree(statContext);
                    statContext = MIL.M_NULL;
                }
            }

            return mean;
        }
        #endregion

        #endregion

        #region ---Expansion Coeff---

        #region --- CreateCoeffImage ---
        public MIL_ID CreateCoeffImage(
            MeasureData correctionData,
            string type)
        {
            int length_row = correctionData.MeasureMatrixInfo.Row;
            int length_col = correctionData.MeasureMatrixInfo.Column;

            MIL_ID outImage = MIL.M_NULL;

            if (length_row == 1 && length_col == 1)
            {
                outImage = this.CreateSingleCoeffImage(correctionData, type);
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImage = this.CreateInterpolationCoeffImage(correctionData, type);
            }

            return outImage;
        }
        #endregion

        #region --- CreateSingleCoeffImage ---
        public MIL_ID CreateSingleCoeffImage(
            MeasureData correctionData,
            string type)
        {
            int width = correctionData.ImageInfo.Width;
            int height = correctionData.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = correctionData.DataList[0].Coefficient.X;
            if (type == "Y")
            {
                pixelValue = correctionData.DataList[0].Coefficient.Y;
            }
            else if (type == "Z")
            {
                pixelValue = correctionData.DataList[0].Coefficient.Z;
            }

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- CreateInterpolationCoeffImage ---
        public MIL_ID CreateInterpolationCoeffImage(
            MeasureData correctionData,
            string type)
        {
            int width = correctionData.ImageInfo.Width;
            int height = correctionData.ImageInfo.Height;
            int length_row = correctionData.MeasureMatrixInfo.Row;
            int length_col = correctionData.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = correctionData.DataList[index];

                        double pixelValue = infoManager.Coefficient.X;
                        if (type == "Y")
                        {
                            pixelValue = infoManager.Coefficient.Y;
                        }
                        else if (type == "Z")
                        {
                            pixelValue = infoManager.Coefficient.Z;
                        }

                        PointData p = null;
                        if (correctionData.RegionMethod == EnumRegionMethod.Circle)
                        {
                            p = new PointData(
                                infoManager.CircleRegionInfo.CenterX,
                                infoManager.CircleRegionInfo.CenterY,
                                pixelValue);
                        }
                        else
                        {
                            p = new PointData(
                                infoManager.RectRegionInfo.StartX + infoManager.RectRegionInfo.Width / 2,
                                infoManager.RectRegionInfo.StartY + infoManager.RectRegionInfo.Height / 2,
                                pixelValue);
                        }
                        points.Add(p);
                    }

                    RegionData regionData = null;

                    if (correctionData.RegionMethod == EnumRegionMethod.Circle)
                    {
                        regionData = assistant.GetRegion(
                            i,
                            correctionData.DataList[positionIndex[0]].CircleRegionInfo,
                            correctionData.DataList[positionIndex[1]].CircleRegionInfo,
                            correctionData.DataList[positionIndex[2]].CircleRegionInfo,
                            correctionData.DataList[positionIndex[3]].CircleRegionInfo);
                    }
                    else
                    {
                        regionData = assistant.GetRectRegion(
                            i,
                            correctionData.DataList[positionIndex[0]].RectRegionInfo,
                            correctionData.DataList[positionIndex[1]].RectRegionInfo,
                            correctionData.DataList[positionIndex[2]].RectRegionInfo,
                            correctionData.DataList[positionIndex[3]].RectRegionInfo);
                    }

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);

                    //MIL.MbufSave(@"D:\Test\Mil32FImgPart.tif", image);
                }

                //MIL.MbufSave(@"D:\Test\Mil32FImg.tif", image);

                // Circle region data recover
                //for (int i = 0; i < correctionData.DataList.Count; i++)
                //{
                //    FourColorInfo infoManager = correctionData.DataList[i];


                //    MIL.MgraColor(MIL.M_DEFAULT, infoManager.Coefficient.X);
                //    MIL.MgraArcFill(
                //        MIL.M_DEFAULT,
                //        image,
                //        (double)infoManager.CircleRegionInfo.CenterX,
                //        (double)infoManager.CircleRegionInfo.CenterY,
                //        (double)infoManager.CircleRegionInfo.Radius,
                //        (double)infoManager.CircleRegionInfo.Radius,
                //        0,
                //        360);
                //}
                //MIL.MbufSave(@"D:\Test\Mil32F01Img.tif", image);

                //// check data
                //List<PointData> aa = new List<PointData>
                //{
                //    new PointData(548, 1222, 0),
                //    new PointData(2420, 1334, 0),
                //    new PointData(4356, 1414, 0),
                //    new PointData(6228, 1462, 0),
                //    new PointData(8100, 1494, 0),

                //    new PointData(452, 2921, 0),
                //    new PointData(2356, 3017, 0),
                //    new PointData(4436, 3145, 0),
                //    new PointData(6196, 3113, 0),
                //    new PointData(8116, 3145, 0),

                //    new PointData(340, 4652, 0),
                //    new PointData(2308, 4684, 0),
                //    new PointData(4388, 4780, 0),
                //    new PointData(6196, 4828, 0),
                //    new PointData(8228, 4876, 0)
                //};

                //for (int i = 0; i < aa.Count; i++)
                //{
                //    int index = aa[i].Y * width + aa[i].X;
                //    Console.WriteLine("i : " + i + ", " + imageData[index]);
                //}
                //Console.WriteLine("");

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CreateInterpolationCoeffImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion

        #region --- RoiInterpolation ---
        public void RoiInterpolation(
            int width,
            RegionData regionData,
            List<PointData> pointDatas,
            ref float[] imageData)
        {
            int offsetX = regionData.StartX;
            int offsetY = regionData.StartY;
            int roiSizeX = regionData.SizeX;
            int roiSizeY = regionData.SizeY;

            for (int y = offsetY; y < offsetY + roiSizeY; y++)
            {
                for (int x = offsetX; x < offsetX + roiSizeX; x++)
                {
                    int index = y * width + x;
                    //imageData[index] = 0f;

                    PointData point = new PointData
                    {
                        X = x,
                        Y = y
                    };

                    this.InterpolationCalculator(pointDatas, ref point);

                    //if (point.PixelValue > 0.0)
                    {
                        //imageData[index] = (float)point.PixelValue;

                        if (imageData[index] != 0f)
                        {
                            double tmpPixelValue = point.PixelValue > 0 ? (float)point.PixelValue : 0;

                            // average
                            double tmpValue = ((double)imageData[index] + tmpPixelValue) / 2.0;
                            imageData[index] = (float)tmpValue;
                        }
                        else
                        {
                            imageData[index] = point.PixelValue > 0 ? (float)point.PixelValue : 0;
                        }

                    }
                }
            }
        }
        #endregion

        #region --- InterpolationCalculator ---
        public void InterpolationCalculator(
            List<PointData> data,
            ref PointData p)
        {
            PointData p0 = data[0];
            PointData p1 = data[1];
            PointData p2 = data[2];
            PointData p3 = data[3];

            int width = p1.X - p0.X;
            int height = p2.Y - p0.Y;

            int deltaX = p.X - p0.X;
            int deltaY = p.Y - p0.Y;

            double coeff0 = (width - deltaX) * (height - deltaY) * p0.PixelValue;
            double coeff1 = deltaX * (height - deltaY) * p1.PixelValue;
            double coeff2 = deltaY * (width - deltaX) * p2.PixelValue;
            double coeff3 = deltaX * deltaY * p3.PixelValue;

            double numerator = coeff0 + coeff1 + coeff2 + coeff3;
            double denominator = width * height;

            if (denominator == 0.0)
            {
                p.PixelValue = 0;
            }
            else
            {
                p.PixelValue = numerator / denominator;
            }
        }
        #endregion

        #region --- InterpolationCalculatorTest ---
        public void InterpolationCalculatorTest(
            List<PointData> data,
            ref PointData p)
        {
            PointData p0 = data[0];
            PointData p1 = data[1];
            PointData p2 = data[2];
            PointData p3 = data[3];

            int width = p1.X - p0.X;
            int height = p2.Y - p0.Y;

            int deltaX = p.X - p0.X;
            int deltaY = p.Y - p0.Y;

            double coeff0 = (width - deltaX) * (height - deltaY) * p0.PixelValue;
            double coeff1 = deltaX * (height - deltaY) * p1.PixelValue;
            double coeff2 = deltaY * (width - deltaX) * p2.PixelValue;
            double coeff3 = deltaX * deltaY * p3.PixelValue;

            double numerator = coeff0 + coeff1 + coeff2 + coeff3;
            double denominator = width * height;

            if (denominator == 0.0)
            {
                p.PixelValue = 0;
            }
            else
            {
                p.PixelValue = numerator / denominator;
            }
        }
        #endregion


        // Two pattern
        /// <summary>
        /// Fits a line to a collection of (x,y) points.
        /// </summary>
        /// <param name="xVals">The x-axis values.</param>
        /// <param name="yVals">The y-axis values.</param>
        /// <param name="rSquared">The r^2 value of the line.</param>
        /// <param name="yIntercept">The y-intercept value of the line (i.e. y = ax + b, yIntercept is b).</param>
        /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
        /// 
        #region --- _linearRegression ---
        public void _linearRegression(
            double[] xVals,
            double[] yVals,
            ref double rSquared,
            ref double yIntercept,
            ref double slope)
        {
            if (xVals.Length != yVals.Length)
            {
                throw new Exception("Input values should be with the same length.");
            }

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (var i = 0; i < xVals.Length; i++)
            {
                var x = xVals[i];
                var y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            var count = xVals.Length;
            var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            var meanX = sumOfX / count;
            var meanY = sumOfY / count;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR;
            yIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
        #endregion

        // A
        #region --- _createFactorAImageTpat ---
        private MIL_ID _createFactorAImageTpat(
            MeasureData correctionDataTartget,
            string type)
        {
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            MIL_ID outImage = MIL.M_NULL;

            if (length_row == 1 && length_col == 1)
            {
                outImage = this._createSingleFactorAImageTpat(correctionDataTartget, type);
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImage = this._createInterpolationFactorAImageTpat(correctionDataTartget, type);
            }

            return outImage;
        }
        #endregion

        #region --- _createSingleFactorAImageTpat ---
        public MIL_ID _createSingleFactorAImageTpat(
            MeasureData correctionDataTarget,
            string type)
        {
            int width = correctionDataTarget.ImageInfo.Width;
            int height = correctionDataTarget.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = correctionDataTarget.DataList[0].FactorA.X;
            if (type == "Y")
            {
                pixelValue = correctionDataTarget.DataList[0].FactorA.Y;
            }
            else if (type == "Z")
            {
                pixelValue = correctionDataTarget.DataList[0].FactorA.Z;
            }

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- _createInterpolationFactorAImageTpat ---
        private MIL_ID _createInterpolationFactorAImageTpat(
            MeasureData correctionDataTartget,
            string type)
        {
            int width = correctionDataTartget.ImageInfo.Width;
            int height = correctionDataTartget.ImageInfo.Height;
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = correctionDataTartget.DataList[index];

                        double pixelValue = infoManager.FactorA.X;
                        if (type == "Y")
                        {
                            pixelValue = infoManager.FactorA.Y;
                        }
                        else if (type == "Z")
                        {
                            pixelValue = infoManager.FactorA.Z;
                        }

                        PointData p = new PointData(
                            infoManager.CircleRegionInfo.CenterX,
                            infoManager.CircleRegionInfo.CenterY,
                            pixelValue);
                        points.Add(p);
                    }

                    RegionData regionData =
                        assistant.GetRegion(
                            i,
                            correctionDataTartget.DataList[positionIndex[0]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[1]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[2]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[3]].CircleRegionInfo);

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);

                }

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CreateInterpolationCoeffImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion

        // B
        #region --- _createFactorBImageTpat ---
        private MIL_ID _createFactorBImageTpat(
            MeasureData correctionDataTartget,
            string type)
        {
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            MIL_ID outImage = MIL.M_NULL;

            if (length_row == 1 && length_col == 1)
            {
                outImage = this._createSingleFactorBImageTpat(correctionDataTartget, type);
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImage = this._createInterpolationFactorBImageTpat(correctionDataTartget, type);
            }


            return outImage;
        }
        #endregion

        #region --- _createSingleFactorBImageTpat ---
        public MIL_ID _createSingleFactorBImageTpat(
            MeasureData correctionDataTarget,
            string type)
        {
            int width = correctionDataTarget.ImageInfo.Width;
            int height = correctionDataTarget.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = correctionDataTarget.DataList[0].FactorB.X;
            if (type == "Y")
            {
                pixelValue = correctionDataTarget.DataList[0].FactorB.Y;
            }
            else if (type == "Z")
            {
                pixelValue = correctionDataTarget.DataList[0].FactorB.Z;
            }

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- _createInterpolationFactorBImageTpat ---
        private MIL_ID _createInterpolationFactorBImageTpat(
            MeasureData correctionDataTartget,
            string type)
        {
            int width = correctionDataTartget.ImageInfo.Width;
            int height = correctionDataTartget.ImageInfo.Height;
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = correctionDataTartget.DataList[index];

                        double pixelValue = infoManager.FactorB.X;
                        if (type == "Y")
                        {
                            pixelValue = infoManager.FactorB.Y;
                        }
                        else if (type == "Z")
                        {
                            pixelValue = infoManager.FactorB.Z;
                        }

                        PointData p = new PointData(
                            infoManager.CircleRegionInfo.CenterX,
                            infoManager.CircleRegionInfo.CenterY,
                            pixelValue);
                        points.Add(p);
                    }

                    RegionData regionData =
                        assistant.GetRegion(
                            i,
                            correctionDataTartget.DataList[positionIndex[0]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[1]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[2]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[3]].CircleRegionInfo);

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);

                }

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[_createInterpolationFactorBImageTpat] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion

        // XdY

        #region --- _createXdYImageTpat ---
        private MIL_ID _createXdYImageTpat(
            MeasureData correctionDataTartget)
        {
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            
            MIL_ID outImage = MIL.M_NULL;

            if (length_col == 1 && length_row == 1)
            {
                outImage = this._createSingleXdYImageTpat(correctionDataTartget);
            }
            else if (length_col * length_row < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_col, length_row));
            }
            else
            {
                outImage = this._createInterpolationXdYImageTpat(correctionDataTartget);
            }

            return outImage;
        }
        #endregion

        #region --- _createSingleXdYImageTpat ---
        public MIL_ID _createSingleXdYImageTpat(
            MeasureData correctionDataTarget)
        {
            int width = correctionDataTarget.ImageInfo.Width;
            int height = correctionDataTarget.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = correctionDataTarget.DataList[0].XdY;

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- _createInterpolationXdYImageTpat ---
        private MIL_ID _createInterpolationXdYImageTpat(
            MeasureData correctionDataTartget)
        {
            int width = correctionDataTartget.ImageInfo.Width;
            int height = correctionDataTartget.ImageInfo.Height;
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = correctionDataTartget.DataList[index];

                        double pixelValue = infoManager.XdY;

                        PointData p = new PointData(
                            infoManager.CircleRegionInfo.CenterX,
                            infoManager.CircleRegionInfo.CenterY,
                            pixelValue);
                        points.Add(p);
                    }

                    RegionData regionData =
                        assistant.GetRegion(
                            i,
                            correctionDataTartget.DataList[positionIndex[0]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[1]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[2]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[3]].CircleRegionInfo);

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);

                }

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[_createInterpolationXdYImageTpat] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion

        // ZdY

        #region --- _createZdYImageTpat ---
        private MIL_ID _createZdYImageTpat(
            MeasureData correctionDataTartget)
        {
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            MIL_ID outImage = MIL.M_NULL;

            if (length_row == 1 && length_col == 1)
            {
                outImage = this._createSingleZdYImageTpat(correctionDataTartget);
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImage = this._createInterpolationZdYImageTpat(correctionDataTartget);
            }

            return outImage;
        }
        #endregion

        #region --- _createSingleZdYImageTpat ---
        public MIL_ID _createSingleZdYImageTpat(
            MeasureData correctionDataTarget)
        {
            int width = correctionDataTarget.ImageInfo.Width;
            int height = correctionDataTarget.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = correctionDataTarget.DataList[0].ZdY;

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- _createInterpolationZdYImageTpat ---
        private MIL_ID _createInterpolationZdYImageTpat(
            MeasureData correctionDataTartget)
        {
            int width = correctionDataTartget.ImageInfo.Width;
            int height = correctionDataTartget.ImageInfo.Height;
            int length_row = correctionDataTartget.MeasureMatrixInfo.Row;
            int length_col = correctionDataTartget.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = correctionDataTartget.DataList[index];

                        double pixelValue = infoManager.ZdY;

                        PointData p = new PointData(
                            infoManager.CircleRegionInfo.CenterX,
                            infoManager.CircleRegionInfo.CenterY,
                            pixelValue);
                        points.Add(p);
                    }

                    RegionData regionData =
                        assistant.GetRegion(
                            i,
                            correctionDataTartget.DataList[positionIndex[0]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[1]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[2]].CircleRegionInfo,
                            correctionDataTartget.DataList[positionIndex[3]].CircleRegionInfo);

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);

                }

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[_createInterpolationZdYImageTpat] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion

        #endregion

        #region ---User Calibration---
        /// <summary>
        ///  This function is used to generate the user calibration correction image.
        /// </summary>
        /// <param name="coeffImage">Coefficient X, Y, Z image</param>
        /// <param name="rColorImage">User Calibration R: X, Y, Z image</param>
        /// <param name="gColorImage">User Calibration G: X, Y, Z image</param>
        /// <param name="bColorImage">User Calibration B: X, Y, Z image</param>
        /// <param name="wColorImage">User Calibration W: X, Y, Z image</param>
        /// <param name="rCorrectionData">User Correction data R</param>
        /// <param name="gCorrectionData">User Correction data G</param>
        /// <param name="bCorrectionData">User Correction data B</param>
        /// <param name="wCorrectionData">User Correction data W</param>
        /// <param name="outCoeffImage">Coefficien X, Y, Z image</param>
        /// <param name="outConstKImage">Const factor K image</param>
        /// 
        #region --- CreateUserCalibrationCoeffDataImage ---
        public void CreateUserCalibrationCoeffDataImage(
            MilColorImage coeffImage,
            MilColorImage rColorImage, MilColorImage gColorImage, MilColorImage bColorImage, MilColorImage wColorImage,
            MeasureData rCorrectionData, MeasureData gCorrectionData, MeasureData bCorrectionData, ref MeasureData wCorrectionData,
            ref MilMonoImage outConstKImage, ref MilMatrixImage outMatrixImage)
        {
            MIL_ID outConstK = MIL.M_NULL;

            MIL_ID outMatrix00 = MIL.M_NULL;
            MIL_ID outMatrix01 = MIL.M_NULL;
            MIL_ID outMatrix02 = MIL.M_NULL;
            MIL_ID outMatrix10 = MIL.M_NULL;
            MIL_ID outMatrix11 = MIL.M_NULL;
            MIL_ID outMatrix12 = MIL.M_NULL;
            MIL_ID outMatrix20 = MIL.M_NULL;
            MIL_ID outMatrix21 = MIL.M_NULL;
            MIL_ID outMatrix22 = MIL.M_NULL;

            try
            {
                // Check
                if (coeffImage == null)
                {
                    throw new Exception("colorImage is null.");
                }
                coeffImage.Check();

                if (rColorImage == null)
                {
                    throw new Exception("rColorImage is null.");
                }
                rColorImage.Check();

                if (gColorImage == null)
                {
                    throw new Exception("gColorImage is null.");
                }
                gColorImage.Check();

                if (bColorImage == null)
                {
                    throw new Exception("bColorImage is null.");
                }
                bColorImage.Check();

                if (wColorImage == null)
                {
                    throw new Exception("wColorImage is null.");
                }
                wColorImage.Check();

                // 
                if (rCorrectionData == null)
                {
                    throw new Exception("rCorrectionData is null.");
                }
                if (gCorrectionData == null)
                {
                    throw new Exception("gCorrectionData is null.");
                }
                if (bCorrectionData == null)
                {
                    throw new Exception("bCorrectionData is null.");
                }
                if (wCorrectionData == null)
                {
                    throw new Exception("wCorrectionData is null.");
                }

                if (rCorrectionData.DataList.Count != gCorrectionData.DataList.Count ||
                    rCorrectionData.DataList.Count != bCorrectionData.DataList.Count ||
                    rCorrectionData.DataList.Count != wCorrectionData.DataList.Count)
                {
                    throw new Exception("four correctionData DataList not equal");
                }

                if (rCorrectionData.DataList.Count < 1)
                {
                    throw new Exception("correctionData DataList count is 0.");
                }

                MeasureData rPredictData = new MeasureData(); ;
                MeasureData gPredictData = new MeasureData(); ;
                MeasureData bPredictData = new MeasureData(); ;
                MeasureData wPredictData = new MeasureData(); ;

                // Predict AMC Tristimulus
                this.PredictAmcMeasureData(rColorImage, coeffImage, rCorrectionData, ref rPredictData);
                this.PredictAmcMeasureData(gColorImage, coeffImage, gCorrectionData, ref gPredictData);
                this.PredictAmcMeasureData(bColorImage, coeffImage, bCorrectionData, ref bPredictData);
                this.PredictAmcMeasureData(wColorImage, coeffImage, wCorrectionData, ref wPredictData);

                // 4 Color Correction
                for (int i = 0; i < rPredictData.DataList.Count; i++)
                {
                    // Amc
                    double RXmCie = rPredictData.DataList[i].CieChroma.Cx;
                    double RYmCie = rPredictData.DataList[i].CieChroma.Cy;
                    double RZmCie = (1 - RXmCie - RYmCie);

                    double GXmCie = gPredictData.DataList[i].CieChroma.Cx;
                    double GYmCie = gPredictData.DataList[i].CieChroma.Cy;
                    double GZmCie = (1 - GXmCie - GYmCie);

                    double BXmCie = bPredictData.DataList[i].CieChroma.Cx;
                    double BYmCie = bPredictData.DataList[i].CieChroma.Cy;
                    double BZmCie = (1 - BXmCie - BYmCie);

                    double WXmCie = wPredictData.DataList[i].CieChroma.Cx;
                    double WYmCie = wPredictData.DataList[i].CieChroma.Cy;
                    double WZmCie = (1 - WXmCie - WYmCie);

                    double[][] matrixAmc33 = new double[][]
                    {
                        new double[] { RXmCie, GXmCie, BXmCie },
                        new double[] { RYmCie, GYmCie, BYmCie },
                        new double[] { RZmCie, GZmCie, BZmCie }
                    };
                    Console.WriteLine("matrixAmc33");
                    MatrixAssistant.PrintMatrix(matrixAmc33);
                    Console.WriteLine();

                    double[][] matrixAmcW31 = new double[][]
                    {
                          new double[] { WXmCie },
                          new double[] { WYmCie },
                          new double[] { WZmCie }
                    };
                    Console.WriteLine("matrixAmcW31");
                    MatrixAssistant.PrintMatrix(matrixAmcW31);
                    Console.WriteLine();

                    double[][] matrixAmcInv33 = MatrixAssistant.InverseMatrix(matrixAmc33);
                    Console.WriteLine("matrixAmcInv33");
                    MatrixAssistant.PrintMatrix(matrixAmcInv33);
                    Console.WriteLine();

                    double[][] matrixKm31 = MatrixAssistant.MatrixMult(matrixAmcInv33, matrixAmcW31);
                    Console.WriteLine("matrixKm31");
                    MatrixAssistant.PrintMatrix(matrixKm31);
                    Console.WriteLine();

                    double[][] matrixKm33 = new double[][]
                    {
                          new double[] { matrixKm31[0][0], 0, 0 },
                          new double[] { 0, matrixKm31[1][0], 0 },
                          new double[] { 0, 0, matrixKm31[2][0] }
                    };
                    Console.WriteLine("matrixKm33");
                    MatrixAssistant.PrintMatrix(matrixKm33);
                    Console.WriteLine();


                    double[][] matrixMrgb33 = MatrixAssistant.MatrixMult(matrixAmc33, matrixKm33);
                    Console.WriteLine("matrixMrgb33");
                    MatrixAssistant.PrintMatrix(matrixMrgb33);
                    Console.WriteLine();

                    // Measuring instruments = Mi
                    double RXrCie = rCorrectionData.DataList[i].CieChroma.Cx;
                    double RYrCie = rCorrectionData.DataList[i].CieChroma.Cy;
                    double RZrCie = (1 - RXrCie - RYrCie);

                    double GXrCie = gCorrectionData.DataList[i].CieChroma.Cx;
                    double GYrCie = gCorrectionData.DataList[i].CieChroma.Cy;
                    double GZrCie = (1 - GXrCie - GYrCie);

                    double BXrCie = bCorrectionData.DataList[i].CieChroma.Cx;
                    double BYrCie = bCorrectionData.DataList[i].CieChroma.Cy;
                    double BZrCie = (1 - BXrCie - BYrCie);

                    double WXrCie = wCorrectionData.DataList[i].CieChroma.Cx;
                    double WYrCie = wCorrectionData.DataList[i].CieChroma.Cy;
                    double WZrCie = (1 - WXrCie - WYrCie);

                    double[][] matrixMi33 = new double[][]
                    {
                        new double[] { RXrCie, GXrCie, BXrCie },
                        new double[] { RYrCie, GYrCie, BYrCie },
                        new double[] { RZrCie, GZrCie, BZrCie }
                    };
                    Console.WriteLine("matrixMi33");
                    MatrixAssistant.PrintMatrix(matrixMi33);
                    Console.WriteLine();

                    double[][] matrixMiW31 = new double[][]
                    {
                          new double[] { WXrCie },
                          new double[] { WYrCie },
                          new double[] { WZrCie }
                    };
                    Console.WriteLine("matrixMiW31");
                    MatrixAssistant.PrintMatrix(matrixMiW31);
                    Console.WriteLine();

                    double[][] matrixMiInv33 = MatrixAssistant.InverseMatrix(matrixMi33);
                    Console.WriteLine("matrixMiInv33");
                    MatrixAssistant.PrintMatrix(matrixMiInv33);
                    Console.WriteLine();

                    // KrRGB 3x1
                    double[][] matrixKr31 = MatrixAssistant.MatrixMult(matrixMiInv33, matrixMiW31);
                    Console.WriteLine("matrixKr31");
                    MatrixAssistant.PrintMatrix(matrixKr31);
                    Console.WriteLine();

                    // KrRGB 3x3
                    double[][] matrixKr33 = new double[][]
                    {
                          new double[] { matrixKr31[0][0], 0, 0 },
                          new double[] { 0, matrixKr31[1][0], 0 },
                          new double[] { 0, 0, matrixKr31[2][0] }
                    };
                    Console.WriteLine("matrixKr33");
                    MatrixAssistant.PrintMatrix(matrixKr33);
                    Console.WriteLine();

                    // Nrgb
                    double[][] matrixNrgb33 = MatrixAssistant.MatrixMult(matrixMi33, matrixKr33);
                    Console.WriteLine("matrixNrgb33");
                    MatrixAssistant.PrintMatrix(matrixNrgb33);
                    Console.WriteLine();

                    // Inv(Mrgb)
                    double[][] matrixMrgbInv33 = MatrixAssistant.InverseMatrix(matrixMrgb33);
                    Console.WriteLine("matrixMrgbInv33");
                    MatrixAssistant.PrintMatrix(matrixMrgbInv33);
                    Console.WriteLine();

                    // R = Nrgb * Inv(Mrgb)
                    double[][] matrixR33 = MatrixAssistant.MatrixMult(matrixNrgb33, matrixMrgbInv33);
                    Console.WriteLine("matrixR33");
                    MatrixAssistant.PrintMatrix(matrixR33);
                    Console.WriteLine();

                    // YCxCy to XYZ
                    ThreeDimensionalInfo tmpThreeDimensionalInfo = new ThreeDimensionalInfo();
                    this.ChromaToTristimulus(wCorrectionData.DataList[i].CieChroma,
                        ref tmpThreeDimensionalInfo);

                    double WXmTri = tmpThreeDimensionalInfo.X;
                    double WYmTri = tmpThreeDimensionalInfo.Y;
                    double WZmTri = tmpThreeDimensionalInfo.Z;

                    double[][] matrixAmcWTrist31 = new double[][]
                    {
                          new double[] { wPredictData.DataList[i].Tristimulus.X },
                          new double[] { wPredictData.DataList[i].Tristimulus.Y },
                          new double[] { wPredictData.DataList[i].Tristimulus.Z }
                    };
                    Console.WriteLine("matrixAmcWTrist31");
                    MatrixAssistant.PrintMatrix(matrixAmcWTrist31);
                    Console.WriteLine();

                    // M' = M * R
                    double[][] matricUserWTrist31 = MatrixAssistant.MatrixMult(matrixR33, matrixAmcWTrist31);
                    wCorrectionData.DataList[i].UcMatrix.Copy(matrixR33);

                    Console.WriteLine("matricUserWTrist31");
                    MatrixAssistant.PrintMatrix(matricUserWTrist31);
                    Console.WriteLine();

                    // factorK = Mi(W128) / Amc(W128)
                    double factorKx = WXmTri / matricUserWTrist31[0][0];
                    double factorKy = WYmTri / matricUserWTrist31[1][0];
                    double factorKz = WZmTri / matricUserWTrist31[2][0];
                    Console.WriteLine("factorKx : " + factorKx);
                    Console.WriteLine("factorKy : " + factorKy);
                    Console.WriteLine("factorKz : " + factorKz);

                    //
                    wCorrectionData.DataList[i].Tristimulus.X = matricUserWTrist31[0][0];
                    wCorrectionData.DataList[i].Tristimulus.Y = matricUserWTrist31[1][0];
                    wCorrectionData.DataList[i].Tristimulus.Z = matricUserWTrist31[2][0];

                    //
                    wCorrectionData.DataList[i].FactorKx = factorKx;
                    wCorrectionData.DataList[i].FactorKy = factorKy;
                    wCorrectionData.DataList[i].FactorKz = factorKz;

                    double valConstK = WYmTri / wPredictData.DataList[i].Tristimulus.Y;
                    wCorrectionData.DataList[i].ConstK = valConstK;
                }

                //
                this._createUserConstKImage(wCorrectionData, ref outConstK);
                outConstKImage = new MilMonoImage(outConstK);

                this._createUserMatrixImage(wCorrectionData,
                    ref outMatrix00, ref outMatrix01, ref outMatrix02,
                    ref outMatrix10, ref outMatrix11, ref outMatrix12,
                    ref outMatrix20, ref outMatrix21, ref outMatrix22);

                outMatrixImage = new MilMatrixImage(
                    outMatrix00, outMatrix01, outMatrix02,
                    outMatrix10, outMatrix11, outMatrix12,
                    outMatrix20, outMatrix21, outMatrix22);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref outConstK);

                MilNetHelper.MilBufferFree(ref outMatrix00);
                MilNetHelper.MilBufferFree(ref outMatrix01);
                MilNetHelper.MilBufferFree(ref outMatrix02);

                MilNetHelper.MilBufferFree(ref outMatrix10);
                MilNetHelper.MilBufferFree(ref outMatrix11);
                MilNetHelper.MilBufferFree(ref outMatrix12);

                MilNetHelper.MilBufferFree(ref outMatrix20);
                MilNetHelper.MilBufferFree(ref outMatrix21);
                MilNetHelper.MilBufferFree(ref outMatrix22);

                throw new Exception(
                    string.Format(
                        "[CreateUserCalibrationCorrectionData] {0}",
                        ex.Message));
            }
        }
        #endregion

        #region --- _createUserFactorKyImage ---
        private void _createUserFactorKyImage(
            MeasureData userData,
            ref MIL_ID outImageFactorKy)
        {
            int length_row = userData.MeasureMatrixInfo.Row;
            int length_col = userData.MeasureMatrixInfo.Column;

            if (length_row == 1 && length_col == 1)
            {
                outImageFactorKy = this._createSingleFactorImage(userData, "FactorKy");
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImageFactorKy = this._createInterpolationFactorImage(userData, "FactorKy");
            }
        }
        #endregion

        #region --- _createUserConstKImage ---
        private void _createUserConstKImage(
            MeasureData userData,
            ref MIL_ID outImageConstK)
        {
            int length_row = userData.MeasureMatrixInfo.Row;
            int length_col = userData.MeasureMatrixInfo.Column;

            if (length_row == 1 && length_col == 1)
            {
                outImageConstK = this._createSingleFactorImage(userData, "ConstK");
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImageConstK = this._createInterpolationFactorImage(userData, "ConstK");
            }
        }
        #endregion

        #region --- _createUserMatrixImage ---
        private void _createUserMatrixImage(
            MeasureData userData,
            ref MIL_ID outImageMatrix00, ref MIL_ID outImageMatrix01, ref MIL_ID outImageMatrix02,
            ref MIL_ID outImageMatrix10, ref MIL_ID outImageMatrix11, ref MIL_ID outImageMatrix12,
            ref MIL_ID outImageMatrix20, ref MIL_ID outImageMatrix21, ref MIL_ID outImageMatrix22)
        {
            int length_row = userData.MeasureMatrixInfo.Row;
            int length_col = userData.MeasureMatrixInfo.Column;

            if (length_row == 1 && length_col == 1)
            {
                outImageMatrix00 = this._createSingleMatrixImage(userData, "M00");
                outImageMatrix01 = this._createSingleMatrixImage(userData, "M01");
                outImageMatrix02 = this._createSingleMatrixImage(userData, "M02");

                outImageMatrix10 = this._createSingleMatrixImage(userData, "M10");
                outImageMatrix11 = this._createSingleMatrixImage(userData, "M11");
                outImageMatrix12 = this._createSingleMatrixImage(userData, "M12");

                outImageMatrix20 = this._createSingleMatrixImage(userData, "M20");
                outImageMatrix21 = this._createSingleMatrixImage(userData, "M21");
                outImageMatrix22 = this._createSingleMatrixImage(userData, "M22");
            }
            else if (length_row * length_col < 4)
            {
                throw new Exception(
                    string.Format(
                        "MeasureMatrix length {0}x{1} not support.",
                        length_row, length_col));
            }
            else
            {
                outImageMatrix00 = this._createInterpolationMatrixImage(userData, "M00");
                outImageMatrix01 = this._createInterpolationMatrixImage(userData, "M01");
                outImageMatrix02 = this._createInterpolationMatrixImage(userData, "M02");

                outImageMatrix10 = this._createInterpolationMatrixImage(userData, "M10");
                outImageMatrix11 = this._createInterpolationMatrixImage(userData, "M11");
                outImageMatrix12 = this._createInterpolationMatrixImage(userData, "M12");

                outImageMatrix20 = this._createInterpolationMatrixImage(userData, "M20");
                outImageMatrix21 = this._createInterpolationMatrixImage(userData, "M21");
                outImageMatrix22 = this._createInterpolationMatrixImage(userData, "M22");
            }

        }
        #endregion

        #region --- _createSingleFactorImage ---
        private MIL_ID _createSingleFactorImage(
             MeasureData userData, 
             string type)
        {
            int width = userData.ImageInfo.Width;
            int height = userData.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = userData.DataList[0].FactorKx;
            if (type == "FactorKy")
            {
                pixelValue = userData.DataList[0].FactorKy;
            }
            else if (type == "FactorKz")
            {
                pixelValue = userData.DataList[0].FactorKz;
            }
            else if (type == "XdY")
            {
                pixelValue = userData.DataList[0].XdY;
            }
            else if (type == "ZdY")
            {
                pixelValue = userData.DataList[0].ZdY;
            }
            else if (type == "ConstK")
            {
                pixelValue = userData.DataList[0].ConstK;
            }

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- _createInterpolationFactorImage ---
        private MIL_ID _createInterpolationFactorImage(
             MeasureData userData,
             string type)
        {
            int width = userData.ImageInfo.Width;
            int height = userData.ImageInfo.Height;
            int length_row = userData.MeasureMatrixInfo.Row;
            int length_col = userData.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = userData.DataList[index];

                        double pixelValue = infoManager.FactorKx;
                        if (type == "FactorKy")
                        {
                            pixelValue = infoManager.FactorKy;
                        }
                        else if (type == "FactorKz")
                        {
                            pixelValue = infoManager.FactorKz;
                        }
                        else if (type == "XdY")
                        {
                            pixelValue = infoManager.XdY;
                        }
                        else if (type == "ZdY")
                        {
                            pixelValue = infoManager.ZdY;
                        }
                        else if (type == "ConstK")
                        {
                            pixelValue = infoManager.ConstK;
                        }

                        PointData p = new PointData(
                            infoManager.CircleRegionInfo.CenterX,
                            infoManager.CircleRegionInfo.CenterY,
                            pixelValue);
                        points.Add(p);
                    }

                    RegionData regionData =
                        assistant.GetRegion(
                            i,
                            userData.DataList[positionIndex[0]].CircleRegionInfo,
                            userData.DataList[positionIndex[1]].CircleRegionInfo,
                            userData.DataList[positionIndex[2]].CircleRegionInfo,
                            userData.DataList[positionIndex[3]].CircleRegionInfo);

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);

                }

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[_createInterpolationFactorImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion

        #region --- _createSingleMatrixImage ---
        private MIL_ID _createSingleMatrixImage(
             MeasureData userData,
             string type)
        {
            int width = userData.ImageInfo.Width;
            int height = userData.ImageInfo.Height;
            MIL_ID outImage = MIL.M_NULL;

            double pixelValue = userData.DataList[0].UcMatrix.M00;
            if (type == "M01")
            {
                pixelValue = userData.DataList[0].UcMatrix.M01;
            }
            else if (type == "M02")
            {
                pixelValue = userData.DataList[0].UcMatrix.M02;
            }
            else if (type == "M10")
            {
                pixelValue = userData.DataList[0].UcMatrix.M10;
            }
            else if (type == "M11")
            {
                pixelValue = userData.DataList[0].UcMatrix.M11;
            }
            else if (type == "M12")
            {
                pixelValue = userData.DataList[0].UcMatrix.M12;
            }
            else if (type == "M20")
            {
                pixelValue = userData.DataList[0].UcMatrix.M20;
            }
            else if (type == "M21")
            {
                pixelValue = userData.DataList[0].UcMatrix.M21;
            }
            else if (type == "M22")
            {
                pixelValue = userData.DataList[0].UcMatrix.M22;
            }

            MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                MIL.M_IMAGE + MIL.M_PROC, ref outImage);
            MIL.MbufClear(outImage, pixelValue);

            return outImage;
        }
        #endregion

        #region --- _createInterpolationMatrixImage ---
        private MIL_ID _createInterpolationMatrixImage(
             MeasureData userData,
             string type)
        {
            int width = userData.ImageInfo.Width;
            int height = userData.ImageInfo.Height;
            int length_row = userData.MeasureMatrixInfo.Row;
            int length_col = userData.MeasureMatrixInfo.Column;

            float[] imageData = null;
            GCHandle userDataHandle;
            MIL_ID image = MIL.M_NULL;
            MIL_ID outImage = MIL.M_NULL;

            imageData = new float[height * width];
            userDataHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            ulong imageAddr = (ulong)userDataHandle.AddrOfPinnedObject();

            try
            {
                MIL.MbufCreate2d(
                    this.milSystem,
                    (MIL_INT)width,
                    (MIL_INT)height,
                    32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC,
                    MIL.M_HOST_ADDRESS + MIL.M_PITCH,
                    MIL.M_DEFAULT,
                    imageAddr,
                    ref image);
                MIL.MbufClear(image, 0);

                MIL.MbufAlloc2d(this.milSystem, width, height, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outImage);
                MIL.MbufClear(outImage, 0);

                // length_row * length_col
                InterpolationAssistant assistant =
                    new InterpolationAssistant(width, height, length_row, length_col);

                for (int i = 0; i < assistant.Count; i++)
                {
                    int[] positionIndex = assistant.GetPositionIndex(i);

                    List<PointData> points = new List<PointData>();
                    for (int j = 0; j < 4; j++)
                    {
                        int index = positionIndex[j];
                        FourColorData infoManager = userData.DataList[index];

                        double pixelValue = infoManager.UcMatrix.M00;
                        if (type == "M01")
                        {
                            pixelValue = infoManager.UcMatrix.M01;
                        }
                        else if (type == "M02")
                        {
                            pixelValue = infoManager.UcMatrix.M02;
                        }
                        else if (type == "M10")
                        {
                            pixelValue = infoManager.UcMatrix.M10;
                        }
                        else if (type == "M11")
                        {
                            pixelValue = infoManager.UcMatrix.M11;
                        }
                        else if (type == "M12")
                        {
                            pixelValue = infoManager.UcMatrix.M12;
                        }
                        else if (type == "M20")
                        {
                            pixelValue = infoManager.UcMatrix.M20;
                        }
                        else if (type == "M21")
                        {
                            pixelValue = infoManager.UcMatrix.M21;
                        }
                        else if (type == "M22")
                        {
                            pixelValue = infoManager.UcMatrix.M22;
                        }

                        PointData p = new PointData(
                            infoManager.CircleRegionInfo.CenterX,
                            infoManager.CircleRegionInfo.CenterY,
                            pixelValue);
                        points.Add(p);
                    }

                    RegionData regionData =
                        assistant.GetRegion(
                            i,
                            userData.DataList[positionIndex[0]].CircleRegionInfo,
                            userData.DataList[positionIndex[1]].CircleRegionInfo,
                            userData.DataList[positionIndex[2]].CircleRegionInfo,
                            userData.DataList[positionIndex[3]].CircleRegionInfo);

                    this.RoiInterpolation(
                        width,
                        regionData,
                        points,
                        ref imageData);
                }

                MIL.MbufCopy(image, outImage);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[_createInterpolationMatrixImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref image);

                userDataHandle.Free();
            }

            return outImage;
        }
        #endregion


        #region --- UserPredictChromaImage ---
        public void UserPredictChromaImage(
            MilColorImage colorImage,
            MilColorImage coeffImage,
            double expTimeX, double expTimeY, double expTimeZ,
            MilMatrixImage userMatrixImage,
            MilMonoImage userConstKImage,
            MeasureData userCalibrationData,
            bool[] xyzEnable,
            ref MilColorImage tristimulusImage,
            ref MilChromaImage chromaImage)
        {
            // input
            MIL_ID imgX = MIL.M_NULL;
            MIL_ID imgY = MIL.M_NULL;
            MIL_ID imgZ = MIL.M_NULL;

            MIL_ID coeffX = MIL.M_NULL;
            MIL_ID coeffY = MIL.M_NULL;
            MIL_ID coeffZ = MIL.M_NULL;

            MIL_ID matrixM00 = MIL.M_NULL;
            MIL_ID matrixM01 = MIL.M_NULL;
            MIL_ID matrixM02 = MIL.M_NULL;
            MIL_ID matrixM10 = MIL.M_NULL;
            MIL_ID matrixM11 = MIL.M_NULL;
            MIL_ID matrixM12 = MIL.M_NULL;
            MIL_ID matrixM20 = MIL.M_NULL;
            MIL_ID matrixM21 = MIL.M_NULL;
            MIL_ID matrixM22 = MIL.M_NULL;

            MIL_ID constK = MIL.M_NULL;

            // output
            MIL_ID imgTristimulusX = MIL.M_NULL;
            MIL_ID imgTristimulusY = MIL.M_NULL;
            MIL_ID imgTristimulusZ = MIL.M_NULL;

            MIL_ID imgChromaX = MIL.M_NULL;
            MIL_ID imgChromaY = MIL.M_NULL;

            // temp
            MIL_ID outTmp4ColorImageX = MIL.M_NULL;
            MIL_ID outTmp4ColorImageY = MIL.M_NULL;
            MIL_ID outTmp4ColorImageZ = MIL.M_NULL;

            MIL_ID factorKy = MIL.M_NULL;
            MIL_ID factorXdYImage = MIL.M_NULL;
            MIL_ID factorZdYImage = MIL.M_NULL;
            MIL_ID imgTemp = MIL.M_NULL;

            //
            MIL_ID imgTemp0 = MIL.M_NULL;
            MIL_ID imgTemp1 = MIL.M_NULL;
            MIL_ID imgTemp2 = MIL.M_NULL;

            try
            {
                // Check
                if (colorImage == null)
                {
                    throw new Exception("colorImage is null.");
                }

                if (coeffImage == null)
                {
                    throw new Exception("coeffImage is null.");
                }

                colorImage.Check();
                imgX = colorImage.ImgX;
                imgY = colorImage.ImgY;
                imgZ = colorImage.ImgZ;

                coeffImage.Check();
                coeffX = coeffImage.ImgX;
                coeffY = coeffImage.ImgY;
                coeffZ = coeffImage.ImgZ;

                userMatrixImage.Check();
                matrixM00 = userMatrixImage.ImgM00;
                matrixM01 = userMatrixImage.ImgM01;
                matrixM02 = userMatrixImage.ImgM02;
                matrixM10 = userMatrixImage.ImgM10;
                matrixM11 = userMatrixImage.ImgM11;
                matrixM12 = userMatrixImage.ImgM12;
                matrixM20 = userMatrixImage.ImgM20;
                matrixM21 = userMatrixImage.ImgM21;
                matrixM22 = userMatrixImage.ImgM22;

                userConstKImage.Check();
                constK = userConstKImage.Img;

                MIL_INT sizeX = MIL.MbufInquire(imgX, MIL.M_SIZE_X);
                MIL_INT sizeY = MIL.MbufInquire(imgX, MIL.M_SIZE_Y);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTristimulusZ);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgChromaX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgChromaY);

                //
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outTmp4ColorImageX);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outTmp4ColorImageY);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref outTmp4ColorImageZ);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref factorXdYImage);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref factorZdYImage);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTemp);

                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTemp0);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTemp1);
                MIL.MbufAlloc2d(this.milSystem, sizeX, sizeY, 32 + MIL.M_FLOAT,
                    MIL.M_IMAGE + MIL.M_PROC, ref imgTemp2);

                MIL.MbufClear(imgTristimulusX, 0);
                MIL.MbufClear(imgTristimulusY, 0);
                MIL.MbufClear(imgTristimulusZ, 0);
                MIL.MbufClear(imgChromaX, 0);
                MIL.MbufClear(imgChromaY, 0);

                MIL.MbufClear(outTmp4ColorImageX, 0);
                MIL.MbufClear(outTmp4ColorImageY, 0);
                MIL.MbufClear(outTmp4ColorImageZ, 0);

                MIL.MbufClear(factorXdYImage, 0);
                MIL.MbufClear(factorZdYImage, 0);
                MIL.MbufClear(imgTemp, 0);

                MIL.MbufClear(imgTemp0, 0);
                MIL.MbufClear(imgTemp1, 0);
                MIL.MbufClear(imgTemp2, 0);

                // TristimulusX, Y, Z
                MIL.MimArith(imgX, expTimeX, imgTristimulusX, MIL.M_DIV_CONST);
                MIL.MimArith(imgY, expTimeY, imgTristimulusY, MIL.M_DIV_CONST);
                MIL.MimArith(imgZ, expTimeZ, imgTristimulusZ, MIL.M_DIV_CONST);
                MIL.MimArith(imgTristimulusX, coeffX, imgTristimulusX, MIL.M_MULT);
                MIL.MimArith(imgTristimulusY, coeffY, imgTristimulusY, MIL.M_MULT);
                MIL.MimArith(imgTristimulusZ, coeffZ, imgTristimulusZ, MIL.M_MULT);

                //MIL.MbufSave(string.Format("D:\\4Color\\0905\\ImgTristimulusX.tif"), imgTristimulusX);
                //MIL.MbufSave(string.Format("D:\\4Color\\0905\\ImgTristimulusY.tif"), imgTristimulusY);
                //MIL.MbufSave(string.Format("D:\\4Color\\0905\\ImgTristimulusZ.tif"), imgTristimulusZ);

                //
                //MIL_ID takeID = imgTristimulusX;
                //int width = 0;
                //int height = 0;
                //MIL_ID MilImagePtr = MIL.M_NULL;
                //MIL_ID MilImagePitch = MIL.M_NULL;

                //MIL.MbufControl(takeID, MIL.M_LOCK, MIL.M_DEFAULT);

                //// Retrieving buffer data pointer and pitch information.
                //MIL.MbufInquire(takeID, MIL.M_HOST_ADDRESS, ref MilImagePtr);
                //MIL.MbufInquire(takeID, MIL.M_PITCH, ref MilImagePitch);

                //MIL.MbufInquire(takeID, MIL.M_SIZE_X, ref width);
                //MIL.MbufInquire(takeID, MIL.M_SIZE_Y, ref height);

                //if (takeID != MIL.M_NULL)
                //{
                //    unsafe // Unsafe code block to allow manipulating memory addresses
                //    {
                //        IntPtr MilImagePtrIntPtr = MilImagePtr;
                //        float* MilImageAddr = (float*)MilImagePtrIntPtr;
                //        int total = height * width;

                //        //for (int i = 0; i < total; i++)
                //        //{
                //        //    int y = i / width;
                //        //    int x = i - y * width;
                //        //    short* Addr = MilImageAddr + y * MilImagePitch;
                //        //    short value = Addr[x];

                //        //    if (value > maxValue)
                //        //    {
                //        //        deltaX = x;
                //        //        deltaY = y;
                //        //        maxValue = value;
                //        //    }
                //        //}

                //        float* Addr = MilImageAddr + 2142 * MilImagePitch;
                //        float value = Addr[3156];

                //        // Signals MIL that the buffer data has been updated.
                //        MIL.MbufControl(takeID, MIL.M_MODIFIED, MIL.M_DEFAULT);
                //        // Unlock buffer.
                //        MIL.MbufControl(takeID, MIL.M_UNLOCK, MIL.M_DEFAULT);
                //    }
                //}
                // ---- AMC Prediction Done ----

                // ---- 4 Color Prediction Start
                MeasureData userPredictData = new MeasureData(); ;

                // Predict AMC Tristimulus
                this.PredictAmcMeasureData(colorImage, coeffImage, userCalibrationData, ref userPredictData);

                for (int i = 0; i < userCalibrationData.DataList.Count; i++)
                {
                    // R 3x3
                    double[][] matrixR33 = new double[][]
                    {
                          new double[] {
                              userCalibrationData.DataList[i].UcMatrix.M00,
                              userCalibrationData.DataList[i].UcMatrix.M01,
                              userCalibrationData.DataList[i].UcMatrix.M02 },

                          new double[] {
                              userCalibrationData.DataList[i].UcMatrix.M10,
                              userCalibrationData.DataList[i].UcMatrix.M11,
                              userCalibrationData.DataList[i].UcMatrix.M12 },

                          new double[] {
                              userCalibrationData.DataList[i].UcMatrix.M20,
                              userCalibrationData.DataList[i].UcMatrix.M21,
                              userCalibrationData.DataList[i].UcMatrix.M22 },
                    };

                    double[][] matrixAmcWTrist31 = new double[][]
                    {
                          new double[] { userPredictData.DataList[i].Tristimulus.X },
                          new double[] { userPredictData.DataList[i].Tristimulus.Y },
                          new double[] { userPredictData.DataList[i].Tristimulus.Z }
                    };

                    // M' = M * R
                    double[][] matricUserWTrist31 = MatrixAssistant.MatrixMult(matrixR33, matrixAmcWTrist31);

                    double valFactorKx = userPredictData.DataList[i].Tristimulus.X / matricUserWTrist31[0][0];
                    double valFactorKy = userPredictData.DataList[i].Tristimulus.Y / matricUserWTrist31[1][0];
                    double valFactorKz = userPredictData.DataList[i].Tristimulus.Z / matricUserWTrist31[2][0];

                    double valFactorXdy = matricUserWTrist31[0][0] / matricUserWTrist31[1][0];
                    double valFactorZdy = matricUserWTrist31[2][0] / matricUserWTrist31[1][0];

                    userPredictData.DataList[i].FactorKx = valFactorKx;
                    userPredictData.DataList[i].FactorKy = valFactorKy;
                    userPredictData.DataList[i].FactorKz = valFactorKz;

                    userPredictData.DataList[i].XdY = valFactorXdy;
                    userPredictData.DataList[i].ZdY = valFactorZdy;

                    userPredictData.DataList[i].ConstK = userCalibrationData.DataList[i].ConstK;
                }
                this._createUserFactorKyImage(userPredictData, ref factorKy);

                // 4 Color Correction
                MilColorImage tmpTristimulusImage = new MilColorImage(imgTristimulusX, imgTristimulusY, imgTristimulusZ);

                // x' = M00*x + M01*y + M02*z 
                // y' = M10*x + M11*y + M12*z
                // z' = M20*x + M21*y + M22*z
                MIL.MimArith(matrixM00, imgTristimulusX, imgTemp0, MIL.M_MULT);
                MIL.MimArith(matrixM01, imgTristimulusY, imgTemp1, MIL.M_MULT);
                MIL.MimArith(matrixM02, imgTristimulusZ, imgTemp2, MIL.M_MULT);
                MIL.MimArith(imgTemp0, imgTemp1, outTmp4ColorImageX, MIL.M_ADD);
                MIL.MimArith(outTmp4ColorImageX, imgTemp2, outTmp4ColorImageX, MIL.M_ADD);

                MIL.MimArith(matrixM10, imgTristimulusX, imgTemp0, MIL.M_MULT);
                MIL.MimArith(matrixM11, imgTristimulusY, imgTemp1, MIL.M_MULT);
                MIL.MimArith(matrixM12, imgTristimulusZ, imgTemp2, MIL.M_MULT);
                MIL.MimArith(imgTemp0, imgTemp1, outTmp4ColorImageY, MIL.M_ADD);
                MIL.MimArith(outTmp4ColorImageY, imgTemp2, outTmp4ColorImageY, MIL.M_ADD);

                MIL.MimArith(matrixM20, imgTristimulusX, imgTemp0, MIL.M_MULT);
                MIL.MimArith(matrixM21, imgTristimulusY, imgTemp1, MIL.M_MULT);
                MIL.MimArith(matrixM22, imgTristimulusZ, imgTemp2, MIL.M_MULT);
                MIL.MimArith(imgTemp0, imgTemp1, outTmp4ColorImageZ, MIL.M_ADD);
                MIL.MimArith(outTmp4ColorImageZ, imgTemp2, outTmp4ColorImageZ, MIL.M_ADD);

                //MIL.MimArith(imgTristimulusY, outTmp4ColorImageY, factorKy, MIL.M_DIV);

                // 
                MIL.MimArith(outTmp4ColorImageX, outTmp4ColorImageY, factorXdYImage, MIL.M_DIV);
                MIL.MimArith(outTmp4ColorImageZ, outTmp4ColorImageY, factorZdYImage, MIL.M_DIV);

                // correct to original 
                MIL.MimArith(outTmp4ColorImageY, factorKy, imgTristimulusY, MIL.M_MULT);

                MIL.MimArith(imgTristimulusY, constK, imgTristimulusY, MIL.M_MULT);
                MIL.MimArith(imgTristimulusY, factorXdYImage, imgTristimulusX, MIL.M_MULT);
                MIL.MimArith(imgTristimulusY, factorZdYImage, imgTristimulusZ, MIL.M_MULT);

                // X + Y + Z
                MIL.MimArith(imgTristimulusX, imgTristimulusY, imgTemp, MIL.M_ADD);
                MIL.MimArith(imgTemp, imgTristimulusZ, imgTemp, MIL.M_ADD);

                // ChromaX, Y
                MIL.MimArith(imgTristimulusX, imgTemp, imgChromaX, MIL.M_DIV);
                MIL.MimArith(imgTristimulusY, imgTemp, imgChromaY, MIL.M_DIV);

                chromaImage = new MilChromaImage(imgChromaX, imgChromaY);

                //
                tristimulusImage = new MilColorImage(
                    imgTristimulusX, imgTristimulusY, imgTristimulusZ);
            }
            catch (Exception ex)
            {
                MilNetHelper.MilBufferFree(ref imgTristimulusX);
                MilNetHelper.MilBufferFree(ref imgTristimulusY);
                MilNetHelper.MilBufferFree(ref imgTristimulusZ);

                MilNetHelper.MilBufferFree(ref imgChromaX);
                MilNetHelper.MilBufferFree(ref imgChromaY);

                if (tristimulusImage != null)
                {
                    tristimulusImage.Free();
                    tristimulusImage = null;
                }

                if (chromaImage != null)
                {
                    chromaImage.Free();
                    chromaImage = null;
                }

                throw new Exception(
                    string.Format(
                        "[UserPredictChromaImage] {0}",
                        ex.Message));
            }
            finally
            {
                MilNetHelper.MilBufferFree(ref outTmp4ColorImageX);
                MilNetHelper.MilBufferFree(ref outTmp4ColorImageY);
                MilNetHelper.MilBufferFree(ref outTmp4ColorImageZ);

                MilNetHelper.MilBufferFree(ref factorKy);
                MilNetHelper.MilBufferFree(ref factorXdYImage);
                MilNetHelper.MilBufferFree(ref factorZdYImage);
                MilNetHelper.MilBufferFree(ref imgTemp);

                MilNetHelper.MilBufferFree(ref imgTemp0);
                MilNetHelper.MilBufferFree(ref imgTemp1);
                MilNetHelper.MilBufferFree(ref imgTemp2);
            }
        }
        #endregion

        #endregion

    }
}
