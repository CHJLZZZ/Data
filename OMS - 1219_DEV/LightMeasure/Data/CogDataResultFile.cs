using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonBase.Config;

namespace LightMeasure
{
    public class CogDataResultFile :
      BaseConfig<CogDataResultFile>
    {
        public PanelInfo PanelInfo;
        public List<BlobData> CogList;

        public CogDataResultFile()
        {
            PanelInfo = new PanelInfo();
            CogList = new List<BlobData>();
        }

        public CogDataResultFile(CogDataResultFile obj)
            : this()
        {
            Copy(obj);
        }

        public void Copy(CogDataResultFile obj)
        {
            PanelInfo.Copy(obj.PanelInfo);
            CogList.Clear();
            foreach (BlobData b in obj.CogList)
            {
                CogList.Add(b);
            }
        }

        public void AddData(PanelInfo infoManager, BlobData[,] blobDatas)
        {
            PanelInfo.Copy(infoManager);
            CogList.Clear();
            for (int y = 0; y < infoManager.ResolutionY; y++)
            {
                for (int x = 0; x < infoManager.ResolutionX; x++)
                {
                    CogList.Add(blobDatas[y, x]);
                }
            }
        }

        public void ConvertToArray(out BlobData[,] blobDatas)
        {
            int width = PanelInfo.ResolutionX;
            int height = PanelInfo.ResolutionY;
            blobDatas = new BlobData[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    blobDatas[y, x] = CogList[index];
                }
            }
        }

        protected override bool CheckValue(CogDataResultFile obj)
        {
            try
            {
                Copy(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

    }

}
