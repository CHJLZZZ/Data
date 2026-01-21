using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Matrox.MatroxImagingLibrary;

namespace LightMeasure
{
    public class ThreeDimensionalInfo : ObjectBase
    {
        private double x;
        public double X
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("X"));
            }
        }

        private double y;
        public double Y
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Y"));
            }
        }

        private double z;
        public double Z
        {
            get
            {
                return this.z;
            }

            set
            {
                this.z = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Z"));
            }
        }

        public ThreeDimensionalInfo()
        {
            this.x = -1;
            this.y = -1;
            this.z = -1;
        }

        public ThreeDimensionalInfo(ThreeDimensionalInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(ThreeDimensionalInfo obj)
        {
            this.x = obj.X;
            this.y = obj.Y;
            this.z = obj.Z;
        }

    }
}
