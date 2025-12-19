using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

using CommonBase.Config;

namespace LightMeasure
{
    public class LightMeasureConfig :
    BaseConfig<LightMeasureConfig>,
    IDisposable
    {
        public MeasureData CorrectionData;

        public LightMeasureConfig()
        {
            this.Name = "PatternName";
            this.CorrectionData = new MeasureData();
        }

        public LightMeasureConfig(LightMeasureConfig obj)
            : this()
        {
            this.Copy(obj);
        }

        public void Copy(LightMeasureConfig obj)
        {
            this.Name = obj.Name;
            this.CorrectionData.Copy(obj.CorrectionData);
        }

        protected override bool CheckValue(LightMeasureConfig tmpConfig)
        {
            try
            {
                this.Copy(tmpConfig);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }


        // IDisposable
        // Flag: Has Dispose already been called?
        private bool disposed = false;
        // Instantiate a SafeHandle instance.
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.handle.Dispose();
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
