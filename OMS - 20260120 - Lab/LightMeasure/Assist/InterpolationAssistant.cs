using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class InterpolationAssistant
    {
        public int Count;

        private int length_row;
        private int length_col;
        private int imageWidth;
        private int imageHeight;
        private int[,] pointIndexArray;

        private InterpolationAssistant()
        {
            this.Count = 0;

            this.length_row = 1;
            this.length_col = 1;
            this.imageWidth = 1;
            this.imageHeight = 1;
            this.pointIndexArray = null;
        }

        public InterpolationAssistant(
            int width,
            int height,
            int length_row,
            int length_col)
            : this()
        {
            this.length_row = length_row;
            this.length_col = length_col;
            this.imageWidth = width;
            this.imageHeight = height;

            this.CreatePointIndexArray(length_row, length_col);
        }

        public int[] GetPositionIndex(int n)
        {
            int[] oArray = null;

            try
            {
                int length_row = this.pointIndexArray.GetLength(0);
                int length_col = this.pointIndexArray.GetLength(1);

                int x = n % (length_col - 1);
                int y = n / (length_col - 1);

                oArray = new int[4];
                oArray[0] = this.pointIndexArray[y, x];
                oArray[1] = this.pointIndexArray[y, x + 1];
                oArray[2] = this.pointIndexArray[y + 1, x];
                oArray[3] = this.pointIndexArray[y + 1, x + 1];
            }
            catch (Exception)
            { }

            return oArray;
        }

        //public int[] GetPositionIndex(int n)
        //{
        //    int[] oArray = null;

        //    try
        //    {
        //        int length = this.pointIndexArray.GetLength(0);

        //        int x = n % (length - 1);
        //        int y = n / (length - 1);

        //        oArray = new int[4];
        //        oArray[0] = this.pointIndexArray[y, x];
        //        oArray[1] = this.pointIndexArray[y, x + 1];
        //        oArray[2] = this.pointIndexArray[y + 1, x];
        //        oArray[3] = this.pointIndexArray[y + 1, x + 1];
        //    }
        //    catch (Exception)
        //    { }

        //    return oArray;
        //}

        public RegionData GetRegion(
            int n,
            CircleRegionInfo p0,
            CircleRegionInfo p1,
            CircleRegionInfo p2,
            CircleRegionInfo p3)
        {
            RegionData regionData = null;

            int x = n % (this.length_col - 1);
            int y = n / (this.length_col - 1);

            // Confirm top, down, left and right
            int posX = p0.CenterX;
            int posY = p0.CenterY;
            posX = p1.CenterX < posX ? p1.CenterX : posX;
            posX = p2.CenterX < posX ? p2.CenterX : posX;
            posX = p3.CenterX < posX ? p3.CenterX : posX;

            posY = p1.CenterY < posY ? p1.CenterY : posY;
            posY = p2.CenterY < posY ? p2.CenterY : posY;
            posY = p3.CenterY < posY ? p3.CenterY : posY;

            int stX = (x - 1 >= 0) ? posX : 0;
            int stY = (y - 1 >= 0) ? posY : 0;

            //
            posX = p3.CenterX;
            posY = p3.CenterY;
            posX = p0.CenterX > posX ? p0.CenterX : posX;
            posX = p1.CenterX > posX ? p1.CenterX : posX;
            posX = p2.CenterX > posX ? p2.CenterX : posX;

            posY = p0.CenterY > posY ? p0.CenterY : posY;
            posY = p1.CenterY > posY ? p1.CenterY : posY;
            posY = p2.CenterY > posY ? p2.CenterY : posY;

            int edX = (x + 1 < this.length_col - 1) ? posX : this.imageWidth - 1;
            int edY = (y + 1 < this.length_row - 1) ? posY : this.imageHeight - 1;

            //
            regionData = new RegionData(stX, stY, edX, edY);

            return regionData;
        }

        public RegionData GetRectRegion(
            int n,
            RectRegionInfo p0,
            RectRegionInfo p1,
            RectRegionInfo p2,
            RectRegionInfo p3)
        {
            RegionData regionData = null;

            int x = n % (this.length_col - 1);
            int y = n / (this.length_col - 1);

            int p0x = p0.StartX + (p0.Width / 2);
            int p0y = p0.StartY + (p0.Height / 2);
            int p1x = p1.StartX + (p1.Width / 2);
            int p1y = p1.StartY + (p1.Height / 2);
            int p2x = p2.StartX + (p2.Width / 2);
            int p2y = p2.StartY + (p2.Height / 2);
            int p3x = p3.StartX + (p3.Width / 2);
            int p3y = p3.StartY + (p3.Height / 2);

            // Confirm top, down, left and right
            int posX = p0x;
            int posY = p0y;
            posX = p1x < posX ? p1x : posX;
            posX = p2x < posX ? p2x : posX;
            posX = p3x < posX ? p3x : posX;

            posY = p1y < posY ? p1y : posY;
            posY = p2y < posY ? p2y : posY;
            posY = p3y < posY ? p3y : posY;

            int stX = (x - 1 >= 0) ? posX : 0;
            int stY = (y - 1 >= 0) ? posY : 0;

            //
            posX = p3x;
            posY = p3y;
            posX = p0x > posX ? p0x : posX;
            posX = p1x > posX ? p1x : posX;
            posX = p2x > posX ? p2x : posX;

            posY = p0y > posY ? p0y : posY;
            posY = p1y > posY ? p1y : posY;
            posY = p2y > posY ? p2y : posY;

            int edX = (x + 1 < this.length_col - 1) ? posX : this.imageWidth - 1;
            int edY = (y + 1 < this.length_row - 1) ? posY : this.imageHeight - 1;

            //
            regionData = new RegionData(stX, stY, edX, edY);

            return regionData;
        }

        //public RegionData GetRegion(
        //    int n,
        //    CircleRegionInfo leftTop,
        //    CircleRegionInfo rightDown)
        //{
        //    RegionData regionData = null;

        //    int x = n % (this.length_col - 1);
        //    int y = n / (this.length_col - 1);

        //    // Confirm top, down, left and right
        //    int posX = leftTop.CenterX;
        //    int posY = leftTop.CenterY;
        //    int stX = (x - 1 >= 0) ? posX : 0;
        //    int stY = (y - 1 >= 0) ? posY : 0;

        //    posX = rightDown.CenterX;
        //    posY = rightDown.CenterY;
        //    int edX = (x + 1 < length_col - 1) ? posX : this.imageWidth - 1;
        //    int edY = (y + 1 < length_row - 1) ? posY : this.imageHeight - 1;

        //    regionData = new RegionData(stX, stY, edX, edY);

        //    return regionData;
        //}

        private void CreatePointIndexArray(int length_row, int length_col)
        {
            this.pointIndexArray = new int[length_row, length_col];

            int index = 0;
            for (int y = 0; y < length_row; y++)
            {
                for (int x = 0; x < length_col; x++)
                {
                    this.pointIndexArray[y, x] = index++;
                }
            }

            this.Count = (length_row - 1) * (length_col - 1);
        }

        //private void CreatePointIndexArray(int length)
        //{
        //    this.pointIndexArray = new int[length, length];

        //    int index = 0;
        //    for (int y = 0; y < length; y++)
        //    {
        //        for (int x = 0; x < length; x++)
        //        {
        //            this.pointIndexArray[y, x] = index++;
        //        }
        //    }

        //    this.Count = (length - 1) * (length - 1);
        //}

    }
}
