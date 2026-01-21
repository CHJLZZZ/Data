using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class RectRegionInfo : ObjectBase
    {
        private int startX;
        public int StartX
        {
            get
            {
                return this.startX;
            }

            set
            {
                this.startX = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartX"));
            }
        }

        private int startY;
        public int StartY
        {
            get
            {
                return this.startY;
            }

            set
            {
                this.startY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("StartY"));
            }
        }

        private int width;
        public int Width
        {
            get
            {
                return this.width;
            }

            set
            {
                this.width = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Width"));
            }
        }

        private int height;
        public int Height
        {
            get
            {
                return this.height;
            }

            set
            {
                this.height = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Height"));
            }
        }

        public RectRegionInfo()
        {
            this.startX = -1;
            this.startY = -1;
            this.width = -1;
            this.height = -1;
        }

        public RectRegionInfo(RectRegionInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(RectRegionInfo obj)
        {
            this.startX = obj.StartX;
            this.startY = obj.StartY;
            this.width = obj.Width;
            this.height = obj.Height;
        }

    }
}
