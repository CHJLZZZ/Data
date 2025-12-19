using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class RegionData
    {
        public int StartX;
        public int StartY;
        public int EndX;
        public int EndY;
        public int SizeX;
        public int SizeY;

        private RegionData()
        {
            this.StartX = 0;
            this.StartY = 0;
            this.EndX = 1;
            this.EndY = 1;
            this.SizeX = 1;
            this.SizeY = 1;
        }

        public RegionData(
            int stX,
            int stY,
            int edX,
            int edY)
            : this()
        {
            this.StartX = stX;
            this.StartY = stY;
            this.EndX = edX;
            this.EndY = edY;

            this.CalculateSize();
        }

        private void CalculateSize()
        {
            this.SizeX = this.EndX - this.StartX + 1;
            this.SizeY = this.EndY - this.StartY + 1;
        }
    }
}
