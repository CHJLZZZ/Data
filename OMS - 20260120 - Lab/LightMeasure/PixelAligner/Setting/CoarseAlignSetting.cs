using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class CoarseAlignSetting : ObjectBase
    {
        #region --- threshold ---
        private int threshold;
        public int Threshold
        {
            get
            {
                return this.threshold;
            }

            set
            {
                this.threshold = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Threshold"));
            }
        }
        #endregion

        #region --- areaMin ---
        private int areaMin;
        public int AreaMin
        {
            get
            {
                return this.areaMin;
            }

            set
            {
                this.areaMin = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("AreaMin"));
            }
        }
        #endregion

        #region --- closeNums ---
        private int closeNums;
        public int CloseNums
        {
            get
            {
                return this.closeNums;
            }

            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                this.closeNums = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ClsoeNums"));
            }
        }
        #endregion

        #region --- debugImage ---
        private bool debugImage;
        public bool DebugImage
        {
            get
            {
                return this.debugImage;
            }

            set
            {
                this.debugImage = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("DebugImage"));
            }
        }
        #endregion

        #region --- rotationAlign ---
        private bool rotationAlign;

        public bool RotationAlign
        {
            get
            {
                return this.rotationAlign;
            }

            set
            {
                this.rotationAlign = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("RotaionAlign"));
            }
        }
        #endregion

        public CoarseAlignSetting()
        {
            this.threshold = 100;
            this.areaMin = 100;
            this.closeNums = 2;
            this.debugImage = false;
            this.RotationAlign = true;
        }

        public CoarseAlignSetting(CoarseAlignSetting obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(CoarseAlignSetting obj)
        {
            this.threshold = obj.Threshold;
            this.areaMin = obj.AreaMin;
            this.closeNums = obj.CloseNums;
            this.debugImage = obj.DebugImage;
        }

    }
}
