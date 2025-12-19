using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseTool;

namespace LightMeasure
{
    #region --- Class : FindPanelPixelSetting ---
    public class FindPanelPixelSetting : ObjectBase
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

        #region --- reverseLedCount ---
        private bool reverseLedCount;
        public bool ReverseLedCount
        {
            get
            {
                return this.reverseLedCount;
            }

            set
            {
                this.reverseLedCount = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ReverseLedCount"));
            }
        }
        #endregion

        #region --- firstPixelX ---
        private double firstPixelX;
        public double FirstPixelX
        {
            get { return this.firstPixelX; }

            set { this.firstPixelX = value; this.OnPropertyChanged(new PropertyChangedEventArgs("FirstPixelX")); }
        }
        #endregion

        #region --- firstPixelY ---
        private double firstPixelY;
        public double FirstPixelY
        {
            get { return this.firstPixelY; }

            set { this.firstPixelY = value; this.OnPropertyChanged(new PropertyChangedEventArgs("FirstPixelY")); }
        }
        #endregion

        #region --- endPixelX ---
        private double endPixelX;
        public double EndPixelX
        {
            get { return this.endPixelX; }

            set { this.endPixelX = value; this.OnPropertyChanged(new PropertyChangedEventArgs("EndPixelX")); }
        }
        #endregion

        #region --- endPixelY ---
        private double endPixelY;
        public double EndPixelY
        {
            get { return this.endPixelY; }

            set { this.endPixelY = value; this.OnPropertyChanged(new PropertyChangedEventArgs("EndPixelY")); }
        }
        #endregion 
    
        #region --- rowFindPitchX ---
        private double rowFindPitchX;
        public double RowFindPitchX
        {
            get
            {
                return this.rowFindPitchX;
            }

            set
            {
                this.rowFindPitchX = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("RowFindPitchX"));
            }
        }
        #endregion 

        #region --- rowFindPitchY ---
        private double rowFindPitchY;
        public double RowFindPitchY
        {
            get
            {
                return this.rowFindPitchY;
            }

            set
            {
                this.rowFindPitchY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("RowFindPitchY"));
            }
        }
        #endregion 

        #region --- colFindPitchX ---
        private double colFindPitchX;
        public double ColFindPitchX
        {
            get
            {
                return this.colFindPitchX;
            }

            set
            {
                this.colFindPitchX = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ColFindPitchX"));
            }
        }
        #endregion

        #region --- colFindPitchY ---
        private double colFindPitchY;
        public double ColFindPitchY
        {
            get
            {
                return this.colFindPitchY;
            }

            set
            {
                this.colFindPitchY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ColFindPitchY"));
            }
        }
        #endregion

        #region --- ledFilePath ---
        private String ledFilePath;
        public String LedFilePath
        {
            get
            {
                return this.ledFilePath;
            }

            set
            {
                this.ledFilePath = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("LedFilePath"));
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

        #region --- areaCutMax ---
        private int areaCutMax;
        public int AreaCutMax
        {
            get
            {
                return this.areaCutMax;
            }

            set
            {
                this.areaCutMax = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("AreaCutMax"));
            }
        }
        #endregion

        #region --- searchRangeYRatio ---
        private double searchRangeYRatio;
        public double SearchRangeYRatio
        {
            get
            {
                return this.searchRangeYRatio;
            }

            set
            {
                this.searchRangeYRatio = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SearchRangeYRatio"));
            }
        }
        #endregion

        #region --- seperateDistance ---
        private double seperateDistance;
        public double SeperateDistance
        {
            get
            {
                return this.seperateDistance;
            }

            set
            {
                this.seperateDistance = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SeperateDistance"));
            }
        }
        #endregion

        #region --- blobOpen ---
        private bool blobOpen;
        public bool BlobOpen
        {
            get
            {
                return this.blobOpen;
            }

            set
            {
                this.blobOpen = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("BlobOpen"));
            }
        }
        #endregion

        #region --- repairType ---
        private EnumRepairType repairType;
        public EnumRepairType RepairType
        {
            get
            {
                return this.repairType;
            }

            set
            {
                this.repairType = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("RepairType"));
            }
        }
        #endregion

        #region --- binaryMethod ---
        private EnumBinaryMethod binaryMethod;
        public EnumBinaryMethod BinaryMethod
        {
            get
            {
                return this.binaryMethod;
            }

            set
            {
                this.binaryMethod = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("BinaryMethod"));
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

        public FindPanelPixelSetting()
        {
            this.reverseLedCount = true;
            this.firstPixelX = 0;
            this.firstPixelY = 0;
            this.endPixelX = 0;
            this.endPixelY = 0;
            this.rowFindPitchX = 1.0;
            this.rowFindPitchY = 0.0;
            this.colFindPitchX = 1.0;
            this.colFindPitchY = 0.0;

            this.areaMin = 1;
            this.areaCutMax = 12;
            this.searchRangeYRatio = 0.6;
            this.seperateDistance = 2;
            this.blobOpen = false;

            this.repairType = EnumRepairType.None;
            this.binaryMethod = EnumBinaryMethod.AdaptBin;

            this.debugImage = false;
        }

        public FindPanelPixelSetting(FindPanelPixelSetting obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(FindPanelPixelSetting obj)
        {
            this.reverseLedCount = obj.ReverseLedCount;
            this.firstPixelX = obj.FirstPixelX;
            this.firstPixelY = obj.FirstPixelY;
            this.endPixelX = obj.EndPixelX;
            this.endPixelY = obj.EndPixelY;
            this.rowFindPitchX = obj.RowFindPitchX;
            this.rowFindPitchY = obj.RowFindPitchY;
            this.colFindPitchX = obj.ColFindPitchX;
            this.colFindPitchY = obj.ColFindPitchY;

            this.areaMin = obj.AreaMin;
            this.areaCutMax = obj.AreaCutMax;
            this.searchRangeYRatio = obj.SearchRangeYRatio;
            this.seperateDistance = obj.SeperateDistance;
            this.blobOpen = obj.BlobOpen;

            this.repairType = obj.RepairType;
            this.binaryMethod = obj.BinaryMethod;

            this.debugImage = obj.DebugImage;
        }
    }

    #endregion

}
