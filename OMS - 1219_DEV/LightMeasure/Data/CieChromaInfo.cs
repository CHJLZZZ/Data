using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class CieChromaInfo : ObjectBase
    {
        private double luminance;
        public double Luminance
        {
            get
            {
                return this.luminance;
            }

            set
            {
                this.luminance = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Luminance"));
            }
        }

        private double cx;
        public double Cx
        {
            get
            {
                return this.cx;
            }

            set
            {
                this.cx = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Cx"));
            }
        }

        private double cy;
        public double Cy
        {
            get
            {
                return this.cy;
            }

            set
            {
                this.cy = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Cy"));
            }
        }

        public CieChromaInfo()
        {
            this.luminance = -1;
            this.cx = -1;
            this.cy = -1;
        }

        public CieChromaInfo(CieChromaInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(CieChromaInfo obj)
        {
            this.luminance = obj.Luminance;
            this.cx = obj.Cx;
            this.cy = obj.Cy;
        }

    }
}
