using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMeasure
{
    public enum DirectionFindPixelType
    {
        RightThenDown = 0, 
        LeftThenDown,
        RightThenUp,    
        LeftThenUp,

        DownThenRight,
        DownThenLeft,
        UpThenRight,
        UpThenLeft
    }
}
