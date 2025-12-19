using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class UserCalibrationMatrixInfo : ObjectBase
    {
        // m00 m01 m02
        // m10 m11 m12
        // m20 m21 m22
        private double m00 = 0.0;
        private double m01 = 0.0;
        private double m02 = 0.0;

        private double m10 = 0.0;
        private double m11 = 0.0;
        private double m12 = 0.0;

        private double m20 = 0.0;
        private double m21 = 0.0;
        private double m22 = 0.0;

        public double M00
        {
            get
            {
                return this.m00;
            }

            set
            {
                this.m00 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M00"));
            }
        }
        public double M01
        {
            get
            {
                return this.m01;
            }

            set
            {
                this.m01 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M01"));
            }
        }
        public double M02
        {
            get
            {
                return this.m02;
            }

            set
            {
                this.m02 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M02"));
            }
        }

        public double M10
        {
            get
            {
                return this.m10;
            }

            set
            {
                this.m10 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M10"));
            }
        }
        public double M11
        {
            get
            {
                return this.m11;
            }

            set
            {
                this.m11 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M11"));
            }
        }
        public double M12
        {
            get
            {
                return this.m12;
            }

            set
            {
                this.m12 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M12"));
            }
        }

        public double M20
        {
            get
            {
                return this.m20;
            }

            set
            {
                this.m20 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M20"));
            }
        }
        public double M21
        {
            get
            {
                return this.m21;
            }

            set
            {
                this.m21 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M21"));
            }
        }
        public double M22
        {
            get
            {
                return this.m22;
            }

            set
            {
                this.m22 = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("M22"));
            }
        }

        public UserCalibrationMatrixInfo()
        {
            this.m00 = 0.0;
            this.m01 = 0.0;
            this.m02 = 0.0;

            this.m10 = 0.0;
            this.m11 = 0.0;
            this.m12 = 0.0;

            this.m20 = 0.0;
            this.m21 = 0.0;
            this.m22 = 0.0;
        }

        public UserCalibrationMatrixInfo(UserCalibrationMatrixInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(UserCalibrationMatrixInfo obj)
        {           
            this.m00 = obj.M00;
            this.m01 = obj.M01;
            this.m02 = obj.M02;

            this.m10 = obj.M10;
            this.m11 = obj.M11;
            this.m12 = obj.M12;

            this.m20 = obj.M20;
            this.m21 = obj.M21;
            this.m22 = obj.M22;    
        }

        public void Copy(double[][] objMatrix)
        {
            try
            {
                if (objMatrix.Length != 3)
                {
                    throw new Exception("[UserCalibrationMatrixInfo][Copy] matrix not equal to 3x3");
                }

                for (int i = 0; i < objMatrix.Length; i++) 
                {
                    double[] tmpAry = objMatrix[i];

                    if (tmpAry.Length != 3)
                    {
                        throw new Exception("[UserCalibrationMatrixInfo][Copy] matrix not equal to 3x3");
                    }
                }

                this.m00 = objMatrix[0][0];
                this.m01 = objMatrix[0][1];
                this.m02 = objMatrix[0][2];

                this.m10 = objMatrix[1][0];
                this.m11 = objMatrix[1][1];
                this.m12 = objMatrix[1][2];

                this.m20 = objMatrix[2][0];
                this.m21 = objMatrix[2][1];
                this.m22 = objMatrix[2][2];
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "[CreateUserCalibrationCorrectionData] {0}",
                        ex.Message));
            }
        }

    }
}
