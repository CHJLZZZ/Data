using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DllUrRobot
{
    public class ClsUrStatus
    {
        // Ur Robot
        public double[] TcpPose; // m
        public double[] MotorAngle;
        public double[] MotorCurrent;
        public double[] MotorVoltage;
        public double[] MotorTemperature;

        public ClsUrStatus()
        {
            //
            this.TcpPose = new double[6] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            this.MotorAngle = new double[6] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            this.MotorCurrent = new double[6] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            this.MotorVoltage = new double[6] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            this.MotorTemperature = new double[6] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
        }
    }

    public class ClsUrRobotInterface
    {
        private string ipAddress;
        private int checkMoveStatusTimeOut;

        private Socket socket29999;
        private Socket socket30003;

        private byte[] dataByte29999;
        private byte[] dataByte30003;

        public ClsUrStatus m_ClsUrStatus;

        private Task getRobotParams;
        private bool getActRobotParams;

        //
        private bool isMoveing;
        private bool isStopClick;
        private bool isRobotConnected;

        private AutoResetEvent isUpdateStatus;

        public bool IsMoving()
        {
          return this.isMoveing;
        }

        public ClsUrRobotInterface()
        {
            this.ipAddress = "192.168.1.2";
            this.checkMoveStatusTimeOut = 60000;

            this.socket29999 = null;
            this.socket30003 = null;

            this.dataByte29999 = new byte[1024];
            this.dataByte30003 = new byte[2048];

            this.m_ClsUrStatus = new ClsUrStatus();

            this.getActRobotParams = false;
            this.getRobotParams = null;

            this.isMoveing = false;
            this.isStopClick = false;

            this.isUpdateStatus = new AutoResetEvent(false);
        }

        private void _taskGetRobotParams()
        {
            int isInitial = 0;
            while (this.getActRobotParams)
            {
                try
                {
                    if (this.socket30003 != null)
                    {
                        int dataLength = this.socket30003.Receive(this.dataByte30003);

                        // TcpPose -> 444
                        for (int i = 0; i < this.m_ClsUrStatus.TcpPose.Length; i++)
                        {
                            // mm
                            if (i == 0 || i == 1 || i == 2)
                            {
                                this.m_ClsUrStatus.TcpPose[i] =
                                    this._bytesToDouble(this.dataByte30003, 444 + i * 8) * 1000;
                            }
                            else
                            {
                                this.m_ClsUrStatus.TcpPose[i] =
                                   this._bytesToDouble(this.dataByte30003, 444 + i * 8);
                            }
                        }

                        // MotorAngle -> 252
                        for (int i = 0; i < this.m_ClsUrStatus.MotorAngle.Length; i++)
                        {
                            this.m_ClsUrStatus.MotorAngle[i] =
                                this._bytesToDouble(this.dataByte30003, 252 + i * 8) * 180 / Math.PI;
                        }

                        // MotorCurrent -> 348
                        for (int i = 0; i < this.m_ClsUrStatus.MotorCurrent.Length; i++)
                        {
                            this.m_ClsUrStatus.MotorCurrent[i] =
                                this._bytesToDouble(this.dataByte30003, 348 + i * 8);
                        }

                        // MotorVoltage -> 996
                        for (int i = 0; i < this.m_ClsUrStatus.MotorVoltage.Length; i++)
                        {
                            this.m_ClsUrStatus.MotorVoltage[i] =
                                this._bytesToDouble(this.dataByte30003, 996 + i * 8);
                        }

                        // MotorTemperature -> 692
                        for (int i = 0; i < this.m_ClsUrStatus.MotorTemperature.Length; i++)
                        {
                            this.m_ClsUrStatus.MotorTemperature[i] =
                                this._bytesToDouble(this.dataByte30003, 692 + i * 8);
                        }

                        if (isInitial > 10)
                        {
                            if (isInitial == 11)
                            {
                                this.isUpdateStatus.Set();
                            }
                        }
                        else
                        {
                            isInitial++;
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            //
            this._disconnect30003();
        }

        private int _connect29999()
        {
            int iRet = 0;
            string msg = "";
            try
            {

                if (this.socket29999 == null)
                {
                    this.socket29999 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.socket29999.Connect(this.ipAddress, 29999);
                }

                if (this.socket29999.Connected)
                {
                    Console.WriteLine("29999 Success");

                    string instruct = "PolyscopeVersion" + Environment.NewLine;
                    this.socket29999.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                    //this.socket29999.Send(Encoding.Default.GetBytes("PolyscopeVersion".ToCharArray()));
                    //this.socket29999.Send(Encoding.Default.GetBytes("\n".ToCharArray()));

                    this.socket29999.Receive(this.dataByte29999);
                    msg = this._byteToString(this.dataByte29999);

                    Console.WriteLine("[_connect29999] : " + msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                iRet = 1;
            }
            return iRet;
        }

        private int _connect30003()
        {
            int iRet = 1;
            try
            {
                if (this.socket30003 == null)
                {
                    this.socket30003 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.socket30003.Connect(this.ipAddress, 30003);
                }

                if (this.socket30003.Connected)
                {
                    Console.WriteLine("30003 Success");

                    iRet = 0;
                    if (this.getRobotParams == null)
                    {
                        this.getActRobotParams = true;
                        this.getRobotParams = new Task(this._taskGetRobotParams);
                        this.getRobotParams.Start();

                        this.isUpdateStatus.WaitOne();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                iRet = 1;
            }
            return iRet;
        }

        private int _disconnect29999()
        {
            int iRet = 0;
            string msg = "";
            try
            {
                if (this.socket29999 != null)
                {
                    if (this.socket29999.Connected)
                    {
                        string instruct = "quit" + Environment.NewLine;
                        this.socket29999.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        //this.socket29999.Send(Encoding.Default.GetBytes("quit".ToCharArray()));
                        //this.socket29999.Send(Encoding.Default.GetBytes("\n".ToCharArray()));

                        this.socket29999.Receive(this.dataByte29999);
                        msg = this._byteToString(this.dataByte29999);

                        Console.WriteLine("[_disconnect29999] : " + msg);

                        this.socket29999.Disconnect(true);
                        this.socket29999.Dispose();
                        this.socket29999 = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                iRet = 1;
            }
            return iRet;
        }

        private int _disconnect30003()
        {
            int iRet = 0;
            try
            {
                if (this.socket30003 != null)
                {
                    if (this.socket30003.Connected)
                    {
                        this.socket30003.Dispose();
                        this.socket30003 = null;

                        //this.getRobotParams.Dispose();
                        this.getRobotParams = null;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                iRet = 1;
            }
            return iRet;
        }

        private double _bytesToDouble(byte[] dataBytes, int startIndex)
        {
            int byteLength = 8;
            byte tVal;
            byte[] tmpByte = new byte[byteLength];
            for (int i = 0; i < byteLength - 1; i++)
            {
                tVal = dataBytes[startIndex + i];
                tmpByte[byteLength - 1 - i] = tVal;
            }

            double rtn = BitConverter.ToDouble(tmpByte, 0);
            return rtn;
        }

        private string _byteToString(byte[] dataByte)
        {
            string tmpString = "";
            try
            {
                int count = 0;
                foreach (var str in dataByte)
                {
                    if (str != 10)
                    {
                        count++;
                    }
                    else
                        break;
                }
                tmpString = new ASCIIEncoding().GetString(dataByte, 0, count);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return tmpString;
        }

        private double _degreeToRadians(double degree)
        {
            return (degree * Math.PI / 180.0);
        }

        private void _rotationVectorToRollPitchYaw(
            double rx, double ry, double rz,
            ref double roll, ref double pitch, ref double yaw)
        {
            if (rx == 0 && ry == 0 && rz == 0)
                return;


            double theta = Math.Sqrt(rx * rx + ry * ry + rz * rz);
            double kx = rx / theta;
            double ky = ry / theta;
            double kz = rz / theta;
            double cth = Math.Cos(theta);
            double sth = Math.Sin(theta);
            double vth = 1 - Math.Cos(theta);

            double r11 = kx * kx * vth + cth;
            double r12 = kx * ky * vth - kz * sth;
            double r13 = kx * kz * vth + ky * sth;
            double r21 = kx * ky * vth + kz * sth;
            double r22 = ky * ky * vth + cth;
            double r23 = ky * kz * vth - kx * sth;
            double r31 = kx * kz * vth - ky * sth;
            double r32 = ky * kz * vth + kx * sth;
            double r33 = kz * kz * vth + cth;

            double beta = Math.Atan2(-r31, Math.Sqrt(r11 * r11 + r21 * r21));

            double alpha, gamma;

            if (beta > this._degreeToRadians(89.99))
            {
                beta = this._degreeToRadians(89.99);
                alpha = 0;
                gamma = Math.Atan2(r12, r22);
            }
            else if (beta < -this._degreeToRadians(89.99))
            {
                beta = -this._degreeToRadians(89.99);
                alpha = 0;
                gamma = -Math.Atan2(r12, r22);
            }
            else
            {
                double cb = Math.Cos(beta);
                if (cb == 0)
                {
                    alpha = 0;
                    gamma = 0;
                }
                else
                {
                    alpha = Math.Atan2(r21 / cb, r11 / cb);
                    gamma = Math.Atan2(r32 / cb, r33 / cb);
                }

            }

            roll = gamma;
            pitch = beta;
            yaw = alpha;
            return;
        }

        /// <summary>
        /// Converts a roll pitch yaw vector to a rotation vector.
        /// </summary>
        /// <param name="d">A vector where X=Roll, Y=Pitch, Z=Yaw</param>
        /// <returns>A rotation vector with rx, ry and rz used to rotate the TCP of UR10</returns>
        private void _rollPitchYawToRotationVector(double roll, double pitch, double yaw,
            ref double rx, ref double ry, ref double rz)
        {
            double alpha = yaw;
            double beta = pitch;
            double gamma = roll;


            double ca = Math.Cos(alpha);
            double cb = Math.Cos(beta);
            double cg = Math.Cos(gamma);
            double sa = Math.Sin(alpha);
            double sb = Math.Sin(beta);
            double sg = Math.Sin(gamma);


            double r11 = ca * cb;
            double r12 = ca * sb * sg - sa * cg;
            double r13 = ca * sb * cg + sa * sg;
            double r21 = sa * cb;
            double r22 = sa * sb * sg + ca * cg;
            double r23 = sa * sb * cg - ca * sg;
            double r31 = -sb;
            double r32 = cb * sg;
            double r33 = cb * cg;


            double theta = Math.Acos((r11 + r22 + r33 - 1) / 2);
            double sth = Math.Sin(theta);
            double kx = (r32 - r23) / (2 * sth);
            double ky = (r13 - r31) / (2 * sth);
            double kz = (r21 - r12) / (2 * sth);

            rx = theta * kx;
            ry = theta * ky;
            rz = theta * kz;
        }

        private void _isRobotMoveTcpOk(double x, double y, double z,
            double rx, double ry, double rz,
            int timeout)
        {
            double X_diff = 0.0;
            double Y_diff = 0.0;
            double Z_diff = 0.0;
            double Roll_diff = 0.0;
            double Pitch_diff = 0.0;
            double Yaw_diff = 0.0;

            try
            {
                System.Diagnostics.Stopwatch tmpStopwatch = new System.Diagnostics.Stopwatch();
                tmpStopwatch.Restart();

                double roll = 0;
                double pitch = 0;
                double yaw = 0;
                double Cnt = 0;

                this._rotationVectorToRollPitchYaw(
                    rx, ry, rz,
                    ref roll, ref pitch, ref yaw);

                while (true)
                {
                    if (this.isStopClick)
                    {
                        this.isStopClick = false;
                        break;
                    }

                    if (tmpStopwatch.ElapsedMilliseconds > timeout)
                    {
                        Console.WriteLine("[_isRobotMoveTcpOk] Timeout : " + timeout);
                        break;
                    }

                    X_diff = Math.Abs(this.m_ClsUrStatus.TcpPose[0] - x);
                    Y_diff = Math.Abs(this.m_ClsUrStatus.TcpPose[1] - y);
                    Z_diff = Math.Abs(this.m_ClsUrStatus.TcpPose[2] - z);
                    Roll_diff = Math.Abs(this.m_ClsUrStatus.TcpPose[3] - roll);
                    Pitch_diff = Math.Abs(this.m_ClsUrStatus.TcpPose[4] - pitch);
                    Yaw_diff = Math.Abs(this.m_ClsUrStatus.TcpPose[5] - yaw);

                    //if (Cnt % 10 == 0)
                    //{
                    //    Console.WriteLine("================================");
                    //    Console.WriteLine($"[X_diff] {X_diff}");
                    //    Console.WriteLine($"[Y_diff] {Y_diff}");
                    //    Console.WriteLine($"[Z_diff] {Z_diff}");
                    //    Console.WriteLine($"[Roll_diff] {Roll_diff}");
                    //    Console.WriteLine($"[Pitch_diff] {Pitch_diff}");
                    //    Console.WriteLine($"[Yaw_diff] {Yaw_diff}");
                    //}
                    //Cnt = Cnt + 1;

                    if (X_diff < 0.1 &&
                        Y_diff < 0.1 &&
                        Z_diff < 0.1 &&

                        Roll_diff < 0.5 &&
                        Pitch_diff < 0.5 &&
                        Yaw_diff < 0.5)
                    {
                        //Console.WriteLine("[_isRobotMoveTcpOk] Move Done");
                        //Console.WriteLine("================================");
                        break;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void _isRobotMoveAngleOk(
            double baseAngle, double shoulderAngle, double elbowAngle,
            double wrist1Angle, double wrist2Angle, double wrist3Angle,
            int timeout)
        {
            try
            {
                System.Diagnostics.Stopwatch tmpStopwatch = new System.Diagnostics.Stopwatch();
                tmpStopwatch.Restart();

                while (true)
                {
                    if (this.isStopClick)
                    {
                        this.isStopClick = false;
                        break;
                    }

                    if (tmpStopwatch.ElapsedMilliseconds > timeout)
                    {
                        Console.WriteLine("[_isRobotMoveAngleOk] timeout : " + timeout);
                        break;
                    }

                    if (Math.Abs(this.m_ClsUrStatus.MotorAngle[0] - baseAngle) < 1 &&
                        Math.Abs(this.m_ClsUrStatus.MotorAngle[1] - shoulderAngle) < 1 &&
                        Math.Abs(this.m_ClsUrStatus.MotorAngle[2] - elbowAngle) < 1 &&

                        Math.Abs(this.m_ClsUrStatus.MotorAngle[3] - wrist1Angle) < 0.1 &&
                        Math.Abs(this.m_ClsUrStatus.MotorAngle[4] - wrist2Angle) < 0.1 &&
                        Math.Abs(this.m_ClsUrStatus.MotorAngle[5] - wrist3Angle) < 0.1)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        //
        public bool IsConnected()
        {
            return this.isRobotConnected;
        }

        public bool Connect(string o_IpAddress, int o_CheckStatusTimeOut)
        {
            int iRet = 0;

            try
            {
                this.ipAddress = o_IpAddress;
                this.checkMoveStatusTimeOut = o_CheckStatusTimeOut;

                // _connect30003
                iRet += this._connect30003();

                // _connect29999
                iRet += this._connect29999();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                iRet = 1;
            }

            if (iRet == 0)
            {
                this.isRobotConnected = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Disconnect()
        {
            int iRet = 0;

            try
            {
                // _connect30003
                this.getActRobotParams = false;

                // _connect29999
                iRet += this._disconnect29999();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                iRet = 1;
            }

            if (iRet == 0)
            {
                this.isRobotConnected = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PowerOn()
        {
            bool bRet = true;
            string msg = "";
            try
            {
                if (this.socket29999 != null)
                {
                    if (this.socket29999.Connected)
                    {
                        string instruct = "power on" + Environment.NewLine;
                        this.socket29999.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        //this.socket29999.Send(Encoding.Default.GetBytes("power on".ToCharArray()));
                        //this.socket29999.Send(Encoding.Default.GetBytes("\n".ToCharArray()));

                        this.socket29999.Receive(this.dataByte29999);
                        msg = this._byteToString(this.dataByte29999);

                        Console.WriteLine("[PowerOn] : " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
            }
            return bRet;
        }

        public bool PowerOff()
        {
            bool bRet = true;
            string msg = "";
            try
            {
                if (this.socket29999 != null)
                {
                    if (this.socket29999.Connected)
                    {
                        string instruct = "power off" + Environment.NewLine;
                        this.socket29999.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        //this.socket29999.Send(Encoding.Default.GetBytes("power off".ToCharArray()));
                        //this.socket29999.Send(Encoding.Default.GetBytes("\n".ToCharArray()));

                        this.socket29999.Receive(this.dataByte29999);
                        msg = this._byteToString(this.dataByte29999);

                        Console.WriteLine("[PowerOff] : " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
            }
            return bRet;
        }

        public bool BrakeRelease()
        {
            bool bRet = true;
            string msg = "";
            try
            {
                if (this.socket29999 != null)
                {
                    if (this.socket29999.Connected)
                    {
                        string instruct = "brake release" + Environment.NewLine;
                        this.socket29999.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        //this.socket29999.Send(Encoding.Default.GetBytes("brake release".ToCharArray()));
                        //this.socket29999.Send(Encoding.Default.GetBytes("\n".ToCharArray()));

                        this.socket29999.Receive(this.dataByte29999);
                        msg = this._byteToString(this.dataByte29999);

                        Console.WriteLine("[BrakeRelease] : " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// this is ur robot to moveL function,
        /// x, y, z : mm
        /// roll, pitch, yaw : degree
        /// acclerate : rad/(s^2). velocity : rad/s
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="roll"></param>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <param name="acclerate"></param>
        /// <param name="velocity"></param>
        /// <param name="time"></param>
        /// <param name="blendradius"></param>
        /// <returns></returns>
        public bool MoveL(
            double x, double y, double z, double roll, double pitch, double yaw,
            double acclerate = 0.3, double velocity = 0.1, double time = 0.0, double blendradius = 0.0)
        {
            bool bRet = true;

            try
            {

                // rpy to rv
                double rx = 0.0;
                double ry = 0.0;
                double rz = 0.0;
                this._rollPitchYawToRotationVector(roll, pitch, yaw, ref rx, ref ry, ref rz);

                // mm to m
                double mX = x / 1000.0;
                double mY = y / 1000.0;
                double mZ = z / 1000.0;

                // 
                string instruct = String.Format("movel(p[{0}, {1}, {2}, {3}, {4}, {5}], a = {6}, v = {7}, t = {8}, r = {9})" + Environment.NewLine,
                    mX, mY, mZ, rx, ry, rz, acclerate, velocity, time, blendradius);

                if (this.socket30003 != null)
                {
                    if (this.socket30003.Connected)
                    {
                        this.socket30003.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        this.isMoveing = true;

                        this._isRobotMoveTcpOk(x, y, z, rx, ry, rz, this.checkMoveStatusTimeOut);

                        this.isMoveing = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
                this.isMoveing = false;
            }

            return bRet;
        }

        public bool MoveL_rv(
            double x, double y, double z, double rX, double rY, double rZ,
            double acclerate = 0.3, double velocity = 0.1, double time = 0.0, double blendradius = 0.0)
        {
            bool bRet = true;

            try
            {
                // mm to m
                double mX = x / 1000.0;
                double mY = y / 1000.0;
                double mZ = z / 1000.0;

                //
                string instruct = String.Format("movel(p[{0}, {1}, {2}, {3}, {4}, {5}], a = {6}, v = {7}, t = {8}, r = {9})" + Environment.NewLine,
                    mX, mY, mZ, rX, rY, rZ, acclerate, velocity, time, blendradius);

                if (this.socket30003 != null)
                {
                    if (this.socket30003.Connected)
                    {
                        this.socket30003.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        this.isMoveing = true;

                        this._isRobotMoveTcpOk(
                            x, y, z,
                            rX, rY, rZ,
                            this.checkMoveStatusTimeOut);

                        this.isMoveing = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
                this.isMoveing = false;
            }

            return bRet;
        }

        public bool MoveJ(
            double baseAngle, double shoulderAngle, double elbowAngle,
            double wrist1Angle, double wrist2Angle, double wrist3Angle,
            double acclerate = 0.3, double velocity = 0.1, double blendradius = 0.0)
        {
            bool bRet = true;

            try
            {
                //
                string instruct = String.Format("movej(p[{0}, {1}, {2}, {3}, {4}, {5}], a = {6}, v = {7}, r = {8})" + Environment.NewLine,
                    baseAngle, shoulderAngle, elbowAngle, wrist1Angle, wrist2Angle, wrist3Angle, acclerate, velocity, blendradius);

                if (this.socket30003 != null)
                {
                    if (this.socket30003.Connected)
                    {
                        this.socket30003.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                        this.isMoveing = true;

                        this._isRobotMoveAngleOk(
                            baseAngle, shoulderAngle, elbowAngle,
                            wrist1Angle, wrist2Angle, wrist3Angle,
                            this.checkMoveStatusTimeOut);

                        this.isMoveing = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
            }

            return bRet;
        }

        public bool Stop()
        {
            bool bRet = true;
            string msg = "";
            try
            {
                if (this.socket29999 != null)
                {
                    string instruct = "stop" + Environment.NewLine;
                    this.socket29999.Send(Encoding.Default.GetBytes(instruct.ToArray()));

                    //this.socket29999.Send(Encoding.Default.GetBytes("stop".ToCharArray()));
                    //this.socket29999.Send(Encoding.Default.GetBytes("\n".ToCharArray()));

                    this.socket29999.Receive(this.dataByte29999);
                    msg = _byteToString(this.dataByte29999);

                    if (this.isMoveing)
                    {
                        this.isStopClick = true;
                    }
                    Console.WriteLine("[Stop] : " + msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
            }
            return bRet;
        }

        public bool SetDigitalOutput(int channel, bool onOff)
        {
            bool bRet = true;

            try
            {
                string instruct = string.Format("set_digital_out({0},{1})" + Environment.NewLine,
                    channel, onOff.ToString());

                this.socket30003.Send(Encoding.Default.GetBytes(instruct.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                bRet = false;
            }
            return bRet;
        }

        public void RvToRpy(
            double rx, double ry, double rz,
            ref double roll, ref double pitch, ref double yaw)
        {
            this._rotationVectorToRollPitchYaw(rx, ry, rz, ref roll, ref pitch, ref yaw);
        }

        public void RpyToRv(
            double roll, double pitch, double yaw,
            ref double rx, ref double ry, ref double rz)
        {
            this._rollPitchYawToRotationVector(roll, pitch, yaw, ref rx, ref ry, ref rz);
        }

    }

}
