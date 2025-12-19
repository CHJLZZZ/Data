using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManager
{
    public class MotorInfo
    {
        public double Speed;
        public int Position;
        public bool PinHomeState;
        public bool HomeState;
        public bool OverrideState;
        public int Limit;
    }
}
