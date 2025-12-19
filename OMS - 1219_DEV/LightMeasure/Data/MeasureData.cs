using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace LightMeasure
{
    public class MeasureData : ObjectBase
    {
        private string patternName;
        public string PatternName
        {
            get
            {
                return this.patternName;
            }

            set
            {
                this.patternName = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("PatternName"));
            }
        }

        private ImageInfo imageInfo;
        public ImageInfo ImageInfo
        {
            get
            {
                return this.imageInfo;
            }

            set
            {
                this.imageInfo = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageInfo"));
            }
        }

        private MatrixInfo measureMatrixInfo;
        public MatrixInfo MeasureMatrixInfo
        {
            get
            {
                return this.measureMatrixInfo;
            }

            set
            {
                this.measureMatrixInfo = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("MeasureMatrix"));
            }
        }

        private ThreeDimensionalInfo exposureTime;
        public ThreeDimensionalInfo ExposureTime
        {
            get
            {
                return this.exposureTime;
            }

            set
            {
                this.exposureTime = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ExposureTime"));
            }
        }

        private EnumRegionMethod regionMethod;
        public EnumRegionMethod RegionMethod
        {
            get
            {
                return this.regionMethod;
            }

            set
            {
                this.regionMethod = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("RegionMethod"));
            }
        }

        public ObservableCollection<FourColorData> DataList;

        public MeasureData()
        {
            this.patternName = "Lxxx";
            this.imageInfo = new ImageInfo();
            this.measureMatrixInfo = new MatrixInfo();
            this.exposureTime = new ThreeDimensionalInfo();
            this.regionMethod = EnumRegionMethod.Circle;
            
            this.DataList = new ObservableCollection<FourColorData>();
        }

        public MeasureData(MeasureData obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(MeasureData obj)
        {
            this.patternName = obj.PatternName;
            this.imageInfo.Copy(obj.ImageInfo);
            this.measureMatrixInfo.Copy(obj.MeasureMatrixInfo);
            this.exposureTime.Copy(obj.ExposureTime);
            this.regionMethod = obj.RegionMethod;

            this.DataList.Clear();
            foreach (FourColorData infoManager in obj.DataList)
            {
                this.DataList.Add(
                    new FourColorData(infoManager));
            }
        }

        public void Free()
        {
            this.imageInfo = null;
            this.measureMatrixInfo = null;
            this.exposureTime = null;
            this.DataList = null;
        }

    }
}
