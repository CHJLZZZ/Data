using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseTool;
using CommonBase.Logger;
using Matrox.MatroxImagingLibrary;
namespace FrameGrabber
{
    public class MilDigitizer : CameraBase
    {
        public ASICamera Grabber_ASI;
        public MatroxCamera Grabber_Matrox;

        private MIL_ID milSystem;
        public MIL_ID grabImage;

        public InfoManager infoManager;

        public MilDigParam DigPara = new MilDigParam();
        public MilDigitizer(MIL_ID milSys, MilDigParam DigPara, InfoManager info)
        {
            this.milSystem = milSys;
            this.DigPara = DigPara;
            this.infoManager = info;
        }

        #region --- 方法函式 ---

        #region --- Initial ---

        public void Initial()
        {
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            if (this.Grabber_Matrox != null)
                                this.Grabber_Matrox.Dispose();

                            this.Grabber_Matrox = new MatroxCamera();
                            this.Grabber_Matrox.Initial(this.milSystem, this.DigPara);
                            this.grabImage = this.Grabber_Matrox.GrabImage;

                            this.MaxWidth = this.Grabber_Matrox.MaxWidth;
                            this.MaxHeight = this.Grabber_Matrox.MaxHeight;
                            this.ImageBits = this.Grabber_Matrox.ImageBits;
                            this.MaxGrayScale = (int)Math.Pow(2.0, (double)this.ImageBits) - 1;

                            if (this.DigPara.Camera_GainControl_IsOpen)
                            {
                                this.Grabber_Matrox.OpenComPort(this.DigPara.Camera_GainControl_ComPort,
                                    this.DigPara.Camera_GainControl_BaudRate);

                                if (!this.Grabber_Matrox.IsComportConnect)
                                    infoManager.Error(string.Format("Camera Gain Control [{0}] was not work ! \r\n", this.DigPara.Camera_GainControl_ComPort));
                                else
                                    infoManager.General(string.Format("[Hardware Status] Camera Gain Control - {0}", this.DigPara.Camera_GainControl_ComPort));
                            }
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            if (this.Grabber_ASI != null)
                                this.Grabber_ASI.Dispose();

                            int iNumofConnectCameras = ASICameraCtrl.ASIGetNumOfConnectedCameras();

                            if (iNumofConnectCameras <= 0)
                            {
                                throw new Exception("There is no Z_SYSTEM device.");
                            }

                            this.Grabber_ASI = new ASICamera();
                            this.Grabber_ASI.Initial(this.milSystem, "0", this.DigPara.Camera_Bin.ToString());
                            this.grabImage = this.Grabber_ASI.GrabImage;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion --- Initial ---

        #region --- SetExposureTime ---

        public void SetExposureTime(double exposureTime)
        {   // Exposure Time 單位 : ns
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            this.Grabber_Matrox.SetGrabExposureTime(exposureTime);
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            this.Grabber_ASI.SetGrabExposureTime(exposureTime);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion --- SetExposureTime ---

        #region --- SetExposureTime ---

        public double GetExposureTime()
        {   // Exposure Time 單位 : ns
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                          return  this.Grabber_Matrox.GetGrabExposureTime();
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            return 0;
                        }
                        break;
                }
                return 0;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion --- SetExposureTime ---

        #region --- SetExposureTime ---

        public double GetGain()
        {   // Exposure Time 單位 : ns
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            return this.Grabber_Matrox.GetGain();
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            return 0;
                        }
                        break;
                }
                return 0;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion --- SetExposureTime ---


        #region --- Grab(ref MIL_ID milSource, ref MIL_ID milDark, string outputFolder, string patternName) ---

        public void Grab(ref MIL_ID milSource, ref MIL_ID milDark, string outputFolder, string patternName, bool GrabSubDark)
        {
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                        {
                            this.Grabber_Matrox.Capture(ref milSource, outputFolder, patternName);
                        }
                        break;

                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            this.Grabber_Matrox.Capture(ref milSource, ref milDark, GrabSubDark);
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            this.Grabber_ASI.Capture(ref milSource, ref milDark, GrabSubDark);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion --- Grab ---

        #region --- Grab() ---

        public void Grab()
        {
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                        {
                            this.Grabber_Matrox.Capture();
                        }
                        break;

                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            this.Grabber_Matrox.Capture();
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            this.Grabber_ASI.Capture();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion --- Grab ---

        #region --- SetCameraGain ---

        public void SetCameraGain(double gain)
        {
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            this.Grabber_Matrox.SetGain(gain);

                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            this.Grabber_ASI.SetGain(gain);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion --- SetCameraGain ---

        #region --- CAMDispose ---

        public void CAMDispose()
        {
            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                    case "M_SYSTEM_RAPIXOCXP":
                        {
                            this.Grabber_Matrox.Dispose();
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            this.Grabber_ASI.Dispose();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion --- CAMDispose ---

        #region --- GetTemperature() ---

        public new double GetTemperature()
        {
            double DeviceTemperature = 0;

            try
            {
                switch (this.DigPara.Camera_Type)
                {
                    case "M_SYSTEM_HOST":
                        {
                            DeviceTemperature = 0;
                        }
                        break;

                    case "M_SYSTEM_DEFAULT":
                    case "M_SYSTEM_SOLIOS":
                    case "M_SYSTEM_RADIENTPRO":
                    case "M_SYSTEM_RADIENTEVCL":
                    case "M_SYSTEM_RADIENTCXP":
                        {
                            DeviceTemperature = this.Grabber_Matrox.GetTemperature();
                        }
                        break;

                    case "Z_SYSTEM":
                        {
                            DeviceTemperature = this.Grabber_ASI.GetTemperature();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return DeviceTemperature;


        }
        #endregion

        #endregion --- 方法函式 ---
    }

    public class MilDigParam
    {
        public string Camera_Type;
        public int Camera_No;
        public string Camera_CamFile;
        public int Camera_Bin;

        public bool Camera_GainControl_IsOpen;

        public string Camera_GainControl_ComPort;

        public int Camera_GainControl_BaudRate;
    }
}