using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public class PointOffsetInfo
    {
        public double Cx;
        public double Cy;
        public double OffsetX;
        public double OffsetY;

        public PointOffsetInfo()
        {
            this.Cx = 0;
            this.Cy = 0;
            this.OffsetX = 0;
            this.OffsetY = 0;
        }
    }

    public class CoordinatePositionInfo
    {
        // 0 1
        // 2 3
        public int IndexX = 0;
        public int IndexY = 0;
        public PointOffsetInfo Poi0;
        public PointOffsetInfo Poi1;
        public PointOffsetInfo Poi2;
        public PointOffsetInfo Poi3;

        public CoordinatePositionInfo()
        {
            this.IndexX = 0;
            this.IndexY = 0;
            this.Poi0 = new PointOffsetInfo();
            this.Poi1 = new PointOffsetInfo();
            this.Poi2 = new PointOffsetInfo();
            this.Poi3 = new PointOffsetInfo();
        }
    }

    public class ScanInfo
    {
        public int ScanIndex;
        public List<CoordinatePositionInfo> CpiList; // rect record 4 points
        public List<PointOffsetInfo> PoiList; // each point

        public ScanInfo()
        {
            this.ScanIndex = 0;
            this.CpiList = new List<CoordinatePositionInfo>();
            this.PoiList = new List<PointOffsetInfo>();
        }
    }

    public class MappingTableInfo : CommonBase.Config.BaseConfig<MappingTableInfo>
    {
        public PanelInfo PanelStatus;
        public List<ScanInfo> ScanList;

        public MappingTableInfo()
        {
            this.PanelStatus = new PanelInfo();
            this.ScanList = new List<ScanInfo>();
        }

        public MappingTableInfo(MappingTableInfo obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(MappingTableInfo obj)
        {
            this.ScanList.Clear();
            foreach (ScanInfo b in obj.ScanList)
            {
                this.ScanList.Add(b);
            }
        }

        protected override bool CheckValue(MappingTableInfo obj)
        {
            try
            {
                this.Copy(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }


}
