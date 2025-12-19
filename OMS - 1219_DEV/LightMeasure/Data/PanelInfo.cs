using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class PanelInfo : ObjectBase
    {
        private int resolutionX;
        public int ResolutionX
        {
            get
            {
                return this.resolutionX;
            }

            set
            {
                this.resolutionX = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ResolutionX"));
            }
        }

        private int resolutionY;
        public int ResolutionY
        {
            get
            {
                return this.resolutionY;
            }

            set
            {
                this.resolutionY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ResolutionY"));
            }
        }

        public PanelInfo()
        {
            this.resolutionX = 1;
            this.resolutionY = 1;
        }

        public PanelInfo(int resX, int resY)
        {
            this.resolutionX = resX;
            this.resolutionY = resY;
        }

        public PanelInfo(PanelInfo obj)
        {
            this.Copy(obj);
        }

        public void Copy(PanelInfo obj)
        {
            this.resolutionX = obj.ResolutionX;
            this.resolutionY = obj.ResolutionY;
        }
    }
}
