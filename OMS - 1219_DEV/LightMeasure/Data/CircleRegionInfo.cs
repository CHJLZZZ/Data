using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class CircleRegionInfo : ObjectBase
    {
        private int centerX;
        public int CenterX
        {
            get
            {
                return this.centerX;
            }

            set
            {
                this.centerX = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CenterX"));
            }
        }

        private int centerY;
        public int CenterY
        {
            get
            {
                return this.centerY;
            }

            set
            {
                this.centerY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CenterY"));
            }
        }

        private int radius;
        public int Radius
        {
            get
            {
                return this.radius;
            }

            set
            {
                this.radius = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Radius"));
            }
        }

        public CircleRegionInfo()
        {
            this.centerX = -1;
            this.centerY = -1;
            this.radius = -1;
        }

        public CircleRegionInfo(CircleRegionInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(CircleRegionInfo obj)
        {
            this.centerX = obj.CenterX;
            this.centerY = obj.CenterY;
            this.radius = obj.Radius;
        }

    }
}
