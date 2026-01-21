
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonBase.Logger;
using System.Runtime.InteropServices;
using BaseTool;

namespace HardwareManager
{

    public interface IMotorControl
    {
        event Action<MotorInfo, MotorInfo, MotorInfo, MotorInfo> UpdateStatus;
        EnumMotoControlPackage Type { get; }

        #region --- Property ---
        UnitStatus MoveStatus_Focuser();
        UnitStatus MoveStatus_Aperture();
        UnitStatus MoveStatus_FW1();
        UnitStatus MoveStatus_FW2();
        UnitStatus MoveFlowStatus();

        bool IsWork();
        bool IsMoveProc();
        int NowPos_Focuser();
        int NowPos_Aperture();
        int NowPos_FW1();
        int NowPos_FW2();
        double NowVelocity_Focuser();
        double NowVelocity_Aperture();
        bool IsHome_Focuser();
        bool IsHome_Aperture();
        int Focus_LimitF();
        int Focus_LimitR();
        #endregion --- Property ---

        #region --- 方法函式 ---"
        void Open(string Com_Port, string Com_Port_2, string Com_Port_3);

        void Open(string Com_Port);

        void Open();

        #endregion --- 方法函式 ---"

        #region --- Command ---
        int GetPosition(MotorMember Device);
        double GetVelocity(MotorMember device);
        void Reset(MotorMember device);
        bool Home(MotorMember device);
        void Jog(MotorMember device, int Jog_Dist);
        void Stop(MotorMember device);
        bool DirectMove(MotorMember device, int Pos);
        void Move(int FocuserPos, int AperturePos, int FW1Pos, int FW2Pos, int TimeOut = 60000);
        void SetBacklash(int Backlash);

        #endregion --- Command ---"



    }

}
