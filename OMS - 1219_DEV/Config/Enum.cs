using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BaseTool
{
    public enum UnitStatus
    {
        Idle = 0,
        Running,
        Finish,
        Alarm
    }

 

    public enum EnumRegionShape
    {
        [Description("Circle")]
        Circle = 0,
        [Description("Rect")]
        Rect
    }

    public enum EnumRepairType
    {
        [Description("None")]
        None = 0,
        [Description("Top")]
        Top = 1,
        [Description("RightTop_LeftTop")]
        RightTop_LeftTop = 2,
    }

    public enum EnumBinaryMethod
    {
        [Description("SimpleBin")]
        SimpleBin = 0,
        [Description("AdaptBin")]
        AdaptBin
    }

    public enum EnumBPStatisticalShape
    {
        Circle = 0,
        Rectangle
    }

    public enum EnumAlignDirection
    {
        Vertical = 0,
        Horizontal
    }

    public enum EnumMotoControlPackage
    {
        [Description("Ascom-Wheel_Airy-Motor")]
        Ascom_Wheel_Airy_Motor = 0,

        [Description("Airy-4In1_Wheel_Motor")]
        Airy_4In1_Wheel_Motor = 1,

        [Description("Airy-4In1_TCPIP_Wheel_Motor")]
        Airy_4In1_TCPIP_Wheel_Motor = 2,

        [Description("Airy-211_USB_Wheel_Motor")]
        Airy_211_USB_Wheel_Motor = 3,

        [Description("Ascom-Wheel_Auo-Motor")]
        Ascom_Wheel_Auo_Motor = 4,

        [Description("MS5515M")]
        MS5515M = 5,

        [Description("順詮達")]
        CircleTac = 6,

        [Description("順詮達(Old)")]
        CircleTac_Old = 7,
    }

    public enum EnumXYZFilter_PositionMode
    {
        [Description("AIC_4_Positions")]
        AIC_4_Positions = 0,
        [Description("AIL_4_Positions")]
        AIL_4_Positions = 1,
        [Description("AIL_2_Positions")]
        AIL_2_Positions = 2,
    }

    public enum EnumMeasureMode
    {
        Luminance = 0,
        Luminance_Chroma
    }

    public enum MotorMember
    {
        Focuser,
        Aperture,
        FW1,
        FW2,
        All,
    }
}
