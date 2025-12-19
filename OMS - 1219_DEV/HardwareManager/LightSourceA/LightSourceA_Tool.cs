using BaseTool;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HardwareManager
{
    public partial class LightSourceA_Tool : MaterialForm
    {
        private LightSourceA_Ctrl LightSourceA;
        public LightSourceA_Tool(ref LightSourceA_Ctrl LightSourceA)
        {
            InitializeComponent();

            this.LightSourceA = LightSourceA;
        }                          

        private void Rbx_CheckedChanged(object sender, EventArgs e)
        {
            Num_Step.Enabled = (Rbx_X.Checked);
            Btn_SetStep.Enabled = (Rbx_X.Checked);
            Btn_Home.Enabled = (Rbx_X.Checked);
        }

        private void Btn_Enable_Click(object sender, EventArgs e)
        {
            LightSourceA_Ctrl.MotorType Motor = LightSourceA_Ctrl.MotorType.X;
            
            if (Rbx_X.Checked) Motor = LightSourceA_Ctrl.MotorType.X;
            if (Rbx_Y.Checked) Motor = LightSourceA_Ctrl.MotorType.Y;
            if (Rbx_Z.Checked) Motor = LightSourceA_Ctrl.MotorType.Z;

            LightSourceA.EnableMotor(Motor, true);
        }

        private void Btn_Disable_Click(object sender, EventArgs e)
        {
            LightSourceA_Ctrl.MotorType Motor = LightSourceA_Ctrl.MotorType.X;

            if (Rbx_X.Checked) Motor = LightSourceA_Ctrl.MotorType.X;
            if (Rbx_Y.Checked) Motor = LightSourceA_Ctrl.MotorType.Y;
            if (Rbx_Z.Checked) Motor = LightSourceA_Ctrl.MotorType.Z;

            LightSourceA.EnableMotor(Motor, false);
        }

        private void Btn_Forward_Click(object sender, EventArgs e)
        {
            LightSourceA_Ctrl.MotorType Motor = LightSourceA_Ctrl.MotorType.X;

            if (Rbx_X.Checked) Motor = LightSourceA_Ctrl.MotorType.X;
            if (Rbx_Y.Checked) Motor = LightSourceA_Ctrl.MotorType.Y;
            if (Rbx_Z.Checked) Motor = LightSourceA_Ctrl.MotorType.Z;

            LightSourceA.MoveForward(Motor);
        }

        private void Btn_Backward_Click(object sender, EventArgs e)
        {
            LightSourceA_Ctrl.MotorType Motor = LightSourceA_Ctrl.MotorType.X;

            if (Rbx_X.Checked) Motor = LightSourceA_Ctrl.MotorType.X;
            if (Rbx_Y.Checked) Motor = LightSourceA_Ctrl.MotorType.Y;
            if (Rbx_Z.Checked) Motor = LightSourceA_Ctrl.MotorType.Z;

            LightSourceA.MoveBackward(Motor);
        }

        private void Btn_Home_Click(object sender, EventArgs e)
        {
            LightSourceA_Ctrl.MotorType Motor = LightSourceA_Ctrl.MotorType.X;

            if (Rbx_X.Checked) Motor = LightSourceA_Ctrl.MotorType.X;
            if (Rbx_Y.Checked) Motor = LightSourceA_Ctrl.MotorType.Y;
            if (Rbx_Z.Checked) Motor = LightSourceA_Ctrl.MotorType.Z;

            LightSourceA.MoveHome(Motor);
        }

        private void Btn_SetStep_Click(object sender, EventArgs e)
        {
            LightSourceA_Ctrl.MotorType Motor = LightSourceA_Ctrl.MotorType.X;

            if (Rbx_X.Checked) Motor = LightSourceA_Ctrl.MotorType.X;
            if (Rbx_Y.Checked) Motor = LightSourceA_Ctrl.MotorType.Y;
            if (Rbx_Z.Checked) Motor = LightSourceA_Ctrl.MotorType.Z;

            int Step = (int)Num_Step.Value;
            LightSourceA.SetSteps(Motor, Step);
        }
    }
}
