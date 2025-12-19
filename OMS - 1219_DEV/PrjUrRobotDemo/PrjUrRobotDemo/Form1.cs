using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrjUrRobotDemo
{
    public partial class Form1 : Form
    {
        private const string AppTitle = "UrRobotDemo_V001_20231128.1145";

        private DllUrRobot.ClsUrRobotInterface m_ClsUrRobotInterface;

        public Form1()
        {
            InitializeComponent();
        }

        private void updateBtnStatus(Button o_Button, bool o_Enable)
        {
            try
            {
                if (o_Button == null) { return; }

                if (o_Button.InvokeRequired)
                {
                    o_Button.Invoke(new MethodInvoker(delegate ()
                    {
                        this.updateBtnStatus(o_Button, o_Enable);
                    }));
                }
                else
                {
                    o_Button.Enabled = o_Enable;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void updataRtxMsg(RichTextBox o_RichTextBox, string o_Msg)
        {
            try
            {
                if (o_RichTextBox == null)
                {
                    return;
                }

                if (o_RichTextBox.InvokeRequired)
                {
                    o_RichTextBox.Invoke(new MethodInvoker(delegate ()
                    {
                        this.updataRtxMsg(o_RichTextBox, o_Msg);
                    }));
                }
                else
                {
                    if (o_RichTextBox.Lines.Length > 1000)
                    {
                        o_RichTextBox.Clear();
                    }

                    if (o_RichTextBox.Lines.Length > 0)
                    {
                        o_RichTextBox.AppendText(Environment.NewLine);
                    }
                    o_RichTextBox.AppendText(o_Msg);
                    o_RichTextBox.SelectionStart = o_RichTextBox.TextLength;
                    o_RichTextBox.ScrollToCaret();
                    o_RichTextBox.Update();
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //
            this.Text = AppTitle;

            this.txtIpAddress.Text = Properties.Settings.Default.IpAddress;
            this.nudCheckMoveStatusTimeOut.Value = Properties.Settings.Default.CheckStatusTimeOut;
            this.txtAcc.Text = Properties.Settings.Default.Acc;
            this.txtSpeed.Text = Properties.Settings.Default.Speed;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ////
            //this.m_ClsUrRobotInterface = new DllUrRobot.ClsUrRobotInterface();

            ////double rx = -0.001220983;
            ////double ry = 3.1162765;
            ////double rz = 0.038891915;

            //double rx = 0.059;
            //double ry = 0.042;
            //double rz = 4.734;

            //double roll = 0;
            //double pitch = 0;
            //double yaw = 0;

            //this.m_ClsUrRobotInterface.RvToRpy(
            //    rx, ry, rz, ref roll, ref pitch, ref yaw);

            //this.updataRtxMsg(this.richTextBox1, $"roll : {roll}");
            //this.updataRtxMsg(this.richTextBox1, $"pitch : {pitch}");
            //this.updataRtxMsg(this.richTextBox1, $"yaw : {yaw}");

            ////double roll = -0.004;
            ////double pitch = -0.021;
            ////double yaw = -1.549;

            //double rxnew = 0;
            //double rynew = 0;
            //double rznew = 0;

            //this.m_ClsUrRobotInterface.RpyToRv(
            //    roll, pitch, yaw, ref rxnew, ref rynew, ref rznew);

            //this.updataRtxMsg(this.richTextBox1, $"rxnew : {rxnew}");
            //this.updataRtxMsg(this.richTextBox1, $"rynew : {rynew}");
            //this.updataRtxMsg(this.richTextBox1, $"rznew : {rznew}");
            //this.updataRtxMsg(this.richTextBox1, $"");
            //return;

            try
            {
                if (this.m_ClsUrRobotInterface == null)
                {
                    this.m_ClsUrRobotInterface = new DllUrRobot.ClsUrRobotInterface();
                }

                if (this.m_ClsUrRobotInterface.Connect(this.txtIpAddress.Text, (int)this.nudCheckMoveStatusTimeOut.Value))
                {
                    this.btnConnect.Enabled = false;

                    this.btnDisconnect.Enabled = true;

                    this.btnStop.Enabled = true;

                    //
                    this.btnGetMotorAngle.Enabled = true;
                    this.btnGetTcpPosition.Enabled = true;

                    //
                    this.timer1.Start();

                    // get current position from robot =>
                    if (this.m_ClsUrRobotInterface.IsConnected())
                    {
                        this.nudTcpX.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[0]);
                        this.nudTcpY.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[1]);
                        this.nudTcpZ.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[2]);
                        this.nudTcpRx.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[3]);
                        this.nudTcpRy.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[4]);
                        this.nudTcpRz.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[5]);
                    }
                    else
                    {
                        MessageBox.Show("Please Connect Robot first!", "Warning");
                    }


                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.m_ClsUrRobotInterface.Disconnect())
                {
                    this.btnConnect.Enabled = true;

                    this.btnDisconnect.Enabled = false;

                    this.btnStop.Enabled = false;

                    //
                    this.btnGetTcpPosition.Enabled = false;
                    this.btnMoveTcpTest.Enabled = false;
                    this.btnMoveTcpRpyTest.Enabled = false;

                    this.btnGetMotorAngle.Enabled = false;
                    this.btnMoveAngleTest.Enabled = false;

                    //
                    this.timer1.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                this.m_ClsUrRobotInterface.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnGetTcpPosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.m_ClsUrRobotInterface.IsConnected())
                {
                    this.nudTcpX.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[0]);
                    this.nudTcpY.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[1]);
                    this.nudTcpZ.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[2]);
                    this.nudTcpRoll.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[3]);
                    this.nudTcpPitch.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[4]);
                    this.nudTcpYaw.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[5]);

                    double rx = 0;
                    double ry = 0; 
                    double rz = 0;
                    this.m_ClsUrRobotInterface.RpyToRv(
                        this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[3],
                        this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[4],
                        this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[5],
                        ref rx,
                        ref ry,
                        ref rz);

                    this.nudTcpRx.Value = Convert.ToDecimal(rx);
                    this.nudTcpRy.Value = Convert.ToDecimal(ry);
                    this.nudTcpRz.Value = Convert.ToDecimal(rz);

                    this.btnMoveTcpTest.Enabled = true;
                    this.btnMoveTcpRpyTest.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Please Connect Robot first!", "Warning");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnMoveTcpTest_Click(object sender, EventArgs e)
        {
            try
            {
               new Thread(() =>
               {
                   if (this.m_ClsUrRobotInterface.IsConnected())
                   {
                       this.updateBtnStatus(this.btnMoveTcpTest, false);

                       double x = (double)this.nudTcpX.Value;
                       double y = (double)this.nudTcpY.Value;
                       double z = (double)this.nudTcpZ.Value;
                       double rx = (double)this.nudTcpRx.Value;
                       double ry = (double)this.nudTcpRy.Value;
                       double rz = (double)this.nudTcpRz.Value;

                       double tmpAcc = Convert.ToDouble(this.txtAcc.Text);
                       double tmpSpeed = Convert.ToDouble(this.txtSpeed.Text);
                       double tmpTime = 0;
                       double tmpBlendRadius = 0;

                       this.m_ClsUrRobotInterface.MoveL_rv(
                           x, y, z, rx, ry, rz, tmpAcc, tmpSpeed, tmpTime, tmpBlendRadius);

                       this.updateBtnStatus(this.btnMoveTcpTest, true);
                   }
                   else
                   {
                       MessageBox.Show("Please Connect Robot first!", "Warning");
                   }
               }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnMoveTcpRpyTest_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(() =>
                {
                    if (this.m_ClsUrRobotInterface.IsConnected())
                    {
                        this.updateBtnStatus(this.btnMoveTcpRpyTest, false);

                        double x = (double)this.nudTcpX.Value;
                        double y = (double)this.nudTcpY.Value;
                        double z = (double)this.nudTcpZ.Value;
                        double roll = (double)this.nudTcpRoll.Value;
                        double pitch = (double)this.nudTcpPitch.Value;
                        double yaw = (double)this.nudTcpYaw.Value;

                        double tmpAcc = Convert.ToDouble(this.txtAcc.Text);
                        double tmpSpeed = Convert.ToDouble(this.txtSpeed.Text);
                        double tmpTime = 0;
                        double tmpBlendRadius = 0;

                        this.m_ClsUrRobotInterface.MoveL(
                            x, y, z, roll, pitch, yaw, tmpAcc, tmpSpeed, tmpTime, tmpBlendRadius);

                        this.updateBtnStatus(this.btnMoveTcpRpyTest, true);
                    }
                    else
                    {
                        MessageBox.Show("Please Connect Robot first!", "Warning");
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnGetMotorAngle_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.m_ClsUrRobotInterface.IsConnected())
                {
                    this.nudAngleX.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[0]);
                    this.nudAngleY.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[1]);
                    this.nudAngleZ.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[2]);
                    this.nudAnglerX.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[3]);
                    this.nudAnglerY.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[4]);
                    this.nudAnglerZ.Value = Convert.ToDecimal(this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[5]);

                    this.btnMoveAngleTest.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Please Connect Robot first!", "Warning");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void btnMoveAngleTest_Click(object sender, EventArgs e)
        {
            try
            {

                new Thread(() =>
                {
                    this.updateBtnStatus(this.btnMoveAngleTest, false);

                    if (this.m_ClsUrRobotInterface.IsConnected())
                    {
                        double x = (double)this.nudAngleX.Value;
                        double y = (double)this.nudAngleY.Value;
                        double z = (double)this.nudAngleZ.Value;
                        double rx = (double)this.nudAnglerX.Value;
                        double ry = (double)this.nudAnglerY.Value;
                        double rz = (double)this.nudAnglerZ.Value;

                        double tmpAcc = Convert.ToDouble(this.txtAcc.Text);
                        double tmpSpeed = Convert.ToDouble(this.txtSpeed.Text);
                        double tmpBlendRadius = 0;

                        this.m_ClsUrRobotInterface.MoveJ(
                            x, y, z, rx, ry, rz, tmpAcc, tmpSpeed, tmpBlendRadius);

                        this.updateBtnStatus(this.btnMoveAngleTest, true);
                    }
                    else
                    {
                        MessageBox.Show("Please Connect Robot first!", "Warning");
                    }

                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.m_ClsUrRobotInterface == null)
                {
                    return;
                }

                // Tcp
                double[] tmpDouble = new double[6];
                for (int i = 0; i < tmpDouble.Length; i++)
                {
                    tmpDouble[i] = this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[i];
                }

                double rx = 0;
                double ry = 0;
                double rz = 0;

                this.m_ClsUrRobotInterface.RpyToRv(
                    this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[3],
                    this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[4],
                    this.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose[5],
                    ref rx,
                    ref ry,
                    ref rz);

                this.lblTcpX.Text = tmpDouble[0].ToString("#0.000");
                this.lblTcpY.Text = tmpDouble[1].ToString("#0.000");
                this.lblTcpZ.Text = tmpDouble[2].ToString("#0.000");
                this.lblTcprX.Text = rx.ToString("#0.000");
                this.lblTcprY.Text = ry.ToString("#0.000");
                this.lblTcprZ.Text = rz.ToString("#0.000");

                // Angle
                for (int i = 0; i < tmpDouble.Length; i++)
                {
                    tmpDouble[i] = this.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle[i];
                }

                this.lblAngleBase.Text = tmpDouble[0].ToString("#0.000");
                this.lblAngleShoulder.Text = tmpDouble[1].ToString("#0.000");
                this.lblAngleElbow.Text = tmpDouble[2].ToString("#0.000");
                this.lblAngleWrist1.Text = tmpDouble[3].ToString("#0.000");
                this.lblAngleWrist2.Text = tmpDouble[4].ToString("#0.000");
                this.lblAngleWrist3.Text = tmpDouble[5].ToString("#0.000");
            }
            catch (Exception ex)
            {
                this.updataRtxMsg(this.richTextBox1, $"[{MethodBase.GetCurrentMethod().Name}] : {ex.Message}");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.m_ClsUrRobotInterface != null)
            {
                this.m_ClsUrRobotInterface.Stop();

                this.m_ClsUrRobotInterface.Disconnect();
            }

            //
            Properties.Settings.Default.IpAddress = this.txtIpAddress.Text;
            Properties.Settings.Default.CheckStatusTimeOut = this.nudCheckMoveStatusTimeOut.Value;
            Properties.Settings.Default.Acc = this.txtAcc.Text;
            Properties.Settings.Default.Speed = this.txtSpeed.Text;
            Properties.Settings.Default.Save();
        }

    }
}
