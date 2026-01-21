using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class BlobData
    {
        public double CogX;
        public double CogY;
        public double CogArea;
        public bool UsePitch;
        public bool Valid;
        private double Dis;

        public BlobData()
        {
            this.CogX = 0;
            this.CogY = 0;
            this.CogArea = 0;
            this.UsePitch = false;
            this.Valid = true;
            this.Dis = 0;
        }

        public BlobData(BlobData obj)
            : this()
        {
            this.Copy(obj);
        }

        public void SetDis(double value)
        {
            this.Dis = value;
        }

        public double GetDis()
        {
            return this.Dis;
        }

        public void Copy(BlobData obj)
        {
            this.CogX = obj.CogX;
            this.CogY = obj.CogY;
            this.CogArea = obj.CogArea;
            this.UsePitch = obj.UsePitch;
            this.Valid = obj.Valid;
            this.Dis = obj.Dis;
        }
    }
}
