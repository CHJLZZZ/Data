using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class PointData
    {
        public int X;
        public int Y;
        public double PixelValue;

        public PointData()
        {
            this.X = -1;
            this.Y = -1;
            this.PixelValue = -1;
        }

        public PointData(int x, int y, double pixelValue)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.PixelValue = pixelValue;
        }

        public PointData(PointData data)
            : this()
        {
            this.Copy(data);
        }

        public void Copy(PointData data)
        {
            this.X = data.X;
            this.Y = data.Y;
            this.PixelValue = data.PixelValue;
        }
    }
}
