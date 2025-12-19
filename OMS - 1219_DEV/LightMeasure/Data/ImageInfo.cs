using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class ImageInfo : ObjectBase
    {
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

        public ImageInfo()
        {
            this.width = -1;
            this.height = -1;
        }

        public ImageInfo(ImageInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(ImageInfo obj)
        {
            this.width = obj.Width;
            this.height = obj.Height;
        }

    }
}
