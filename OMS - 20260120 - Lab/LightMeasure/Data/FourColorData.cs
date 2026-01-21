using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class FourColorData : ObjectBase
    {
        private int index;
        public int Index
        {
            get
            {
                return this.index;
            }

            set
            {
                this.index = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Index"));
            }
        }

        private CieChromaInfo cieChroma;
        public CieChromaInfo CieChroma
        {
            get
            {
                return this.cieChroma;
            }

            set
            {
                this.cieChroma = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CieChroma"));
            }
        }

        private CircleRegionInfo circleRegionInfo;
        public CircleRegionInfo CircleRegionInfo
        {
            get
            {
                return this.circleRegionInfo;
            }

            set
            {
                this.circleRegionInfo = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CircleRegionInfo"));
            }
        }

        private RectRegionInfo rectRegionInfo;
        public RectRegionInfo RectRegionInfo
        {
            get
            {
                return this.rectRegionInfo;
            }

            set
            {
                this.rectRegionInfo = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("RectRegionInfo"));
            }
        }

        private ThreeDimensionalInfo tristimulus;
        public ThreeDimensionalInfo Tristimulus
        {
            get
            {
                return this.tristimulus;
            }

            set
            {
                this.tristimulus = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Tristimulus"));
            }
        }

        private ThreeDimensionalInfo grayMean;
        public ThreeDimensionalInfo GrayMean
        {
            get
            {
                return this.grayMean;
            }

            set
            {
                this.grayMean = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("GrayMean"));
            }
        }

        private ThreeDimensionalInfo coefficient;
        public ThreeDimensionalInfo Coefficient
        {
            get
            {
                return this.coefficient;
            }

            set
            {
                this.coefficient = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CorrectionCoeff"));
            }
        }

        // Target pattern
        private double xdY;
        public double XdY
        {
            get
            {
                return this.xdY;
            }

            set
            {
                this.xdY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("XdY"));
            }
        }

        private double zdY;
        public double ZdY
        {
            get
            {
                return this.zdY;
            }

            set
            {
                this.zdY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ZdY"));
            }
        }

        private ThreeDimensionalInfo factorA;
        public ThreeDimensionalInfo FactorA
        {
            get
            {
                return this.factorA;
            }

            set
            {
                this.factorA = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("FactorA"));
            }
        }

        private ThreeDimensionalInfo factorB;
        public ThreeDimensionalInfo FactorB
        {
            get
            {
                return this.factorB;
            }

            set
            {
                this.factorB = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("FactorB"));
            }
        }

        // UserCalibration K
        private double constK;
        public double ConstK
        {
            get
            {
                return this.constK;
            }

            set
            {
                this.constK = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ConstK"));
            }
        }

        private double factorKx;
        public double FactorKx
        {
            get
            {
                return this.factorKx;
            }

            set
            {
                this.factorKx = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("FactorKx"));
            }
        }

        private double factorKy;
        public double FactorKy
        {
            get
            {
                return this.factorKy;
            }

            set
            {
                this.factorKy = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("FactorKy"));
            }
        }

        private double factorKz;
        public double FactorKz
        {
            get
            {
                return this.factorKz;
            }

            set
            {
                this.factorKz = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("FactorKz"));
            }
        }

        // UserCalibration Matrix
        private UserCalibrationMatrixInfo ucMatrix;
        public UserCalibrationMatrixInfo UcMatrix
        {
            get
            {
                return this.ucMatrix;
            }

            set
            {
                this.ucMatrix = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("UcMatrix"));
            }
        }


        public FourColorData()
        {
            this.index = -1;
            this.cieChroma = new CieChromaInfo();
            this.circleRegionInfo = new CircleRegionInfo();
            this.rectRegionInfo = new RectRegionInfo();
            this.tristimulus = new ThreeDimensionalInfo();
            this.grayMean = new ThreeDimensionalInfo();
            this.coefficient = new ThreeDimensionalInfo();

            // two pattern
            this.xdY = 0.0;
            this.zdY = 0.0;
            this.factorA = new ThreeDimensionalInfo();
            this.factorB = new ThreeDimensionalInfo();

            // user calibration
            this.constK = 0.0;
            this.factorKx = 0.0;
            this.factorKy = 0.0;
            this.factorKz = 0.0;

            this.ucMatrix = new UserCalibrationMatrixInfo();

        }

        public FourColorData(FourColorData obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(FourColorData obj)
        {
            this.index = obj.Index;
            this.cieChroma.Copy(obj.CieChroma);
            this.circleRegionInfo.Copy(obj.CircleRegionInfo);
            this.rectRegionInfo.Copy(obj.RectRegionInfo);
            this.tristimulus.Copy(obj.Tristimulus);
            this.grayMean.Copy(obj.GrayMean);
            this.coefficient.Copy(obj.Coefficient);

            //
            this.xdY = obj.XdY;
            this.zdY = obj.ZdY;
            this.factorA.Copy(obj.FactorA);
            this.factorB.Copy(obj.FactorB);

            //
            this.constK = obj.ConstK;
            this.factorKx = obj.FactorKx;
            this.factorKy = obj.FactorKy;
            this.factorKz = obj.FactorKz;

            this.ucMatrix = obj.ucMatrix;
        }

    }
}
