
namespace HardwareManager
{
    partial class AuoMotor_Tool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Num_SetBacklash = new System.Windows.Forms.NumericUpDown();
            this.Btn_SetBacklash = new System.Windows.Forms.Button();
            this.Tbx_GetBacklash = new System.Windows.Forms.TextBox();
            this.Btn_GetBacklash = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Btn_Enable = new System.Windows.Forms.Button();
            this.Btn_Disable = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bntSearchHome = new System.Windows.Forms.Button();
            this.Label16 = new System.Windows.Forms.Label();
            this.Num_HomeSpeed = new System.Windows.Forms.NumericUpDown();
            this.Num_HomeTimeout = new System.Windows.Forms.NumericUpDown();
            this.Label13 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.Btn_GoHome = new System.Windows.Forms.Button();
            this.Num_RelPos = new System.Windows.Forms.NumericUpDown();
            this.Btn_AbsMove = new System.Windows.Forms.Button();
            this.Label15 = new System.Windows.Forms.Label();
            this.Btn_RelMove = new System.Windows.Forms.Button();
            this.Label14 = new System.Windows.Forms.Label();
            this.Num_AbsPos = new System.Windows.Forms.NumericUpDown();
            this.Num_MoveTimeout = new System.Windows.Forms.NumericUpDown();
            this.Num_MoveSpeed = new System.Windows.Forms.NumericUpDown();
            this.Cbx_Read = new System.Windows.Forms.CheckBox();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.Btn_ReadStatus = new System.Windows.Forms.Button();
            this.Label9 = new System.Windows.Forms.Label();
            this.Tbx_Speed = new System.Windows.Forms.TextBox();
            this.Tbx_TargetPos = new System.Windows.Forms.TextBox();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Tbx_CurrentPos = new System.Windows.Forms.TextBox();
            this.Label12 = new System.Windows.Forms.Label();
            this.Pbx_Moving = new System.Windows.Forms.PictureBox();
            this.Label11 = new System.Windows.Forms.Label();
            this.Pbx_Limit = new System.Windows.Forms.PictureBox();
            this.Label10 = new System.Windows.Forms.Label();
            this.Pbx_Home = new System.Windows.Forms.PictureBox();
            this.Pbx_Enable = new System.Windows.Forms.PictureBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Cbx_MotorIdx = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.Btn_SetLimitF = new System.Windows.Forms.Button();
            this.Btn_GetLimitF = new System.Windows.Forms.Button();
            this.Num_SetLimitF = new System.Windows.Forms.NumericUpDown();
            this.Tbx_GetLimitF = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.Btn_SetLimitR = new System.Windows.Forms.Button();
            this.Btn_GetLimitR = new System.Windows.Forms.Button();
            this.Num_SetLimitR = new System.Windows.Forms.NumericUpDown();
            this.Tbx_GetLimitR = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Num_SetBacklash)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_HomeSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_HomeTimeout)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_RelPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_AbsPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MoveTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MoveSpeed)).BeginInit();
            this.grpStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Moving)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Limit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Home)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Enable)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_SetLimitF)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_SetLimitR)).BeginInit();
            this.SuspendLayout();
            // 
            // Num_SetBacklash
            // 
            this.Num_SetBacklash.Location = new System.Drawing.Point(95, 44);
            this.Num_SetBacklash.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.Num_SetBacklash.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.Num_SetBacklash.Name = "Num_SetBacklash";
            this.Num_SetBacklash.Size = new System.Drawing.Size(100, 35);
            this.Num_SetBacklash.TabIndex = 130;
            // 
            // Btn_SetBacklash
            // 
            this.Btn_SetBacklash.Location = new System.Drawing.Point(6, 34);
            this.Btn_SetBacklash.Name = "Btn_SetBacklash";
            this.Btn_SetBacklash.Size = new System.Drawing.Size(83, 50);
            this.Btn_SetBacklash.TabIndex = 131;
            this.Btn_SetBacklash.Text = "Set";
            this.Btn_SetBacklash.UseVisualStyleBackColor = true;
            this.Btn_SetBacklash.Click += new System.EventHandler(this.Btn_SetBacklash_Click);
            // 
            // Tbx_GetBacklash
            // 
            this.Tbx_GetBacklash.BackColor = System.Drawing.Color.White;
            this.Tbx_GetBacklash.Location = new System.Drawing.Point(95, 99);
            this.Tbx_GetBacklash.Name = "Tbx_GetBacklash";
            this.Tbx_GetBacklash.ReadOnly = true;
            this.Tbx_GetBacklash.Size = new System.Drawing.Size(100, 35);
            this.Tbx_GetBacklash.TabIndex = 133;
            // 
            // Btn_GetBacklash
            // 
            this.Btn_GetBacklash.Location = new System.Drawing.Point(6, 90);
            this.Btn_GetBacklash.Name = "Btn_GetBacklash";
            this.Btn_GetBacklash.Size = new System.Drawing.Size(83, 50);
            this.Btn_GetBacklash.TabIndex = 134;
            this.Btn_GetBacklash.Text = "Get";
            this.Btn_GetBacklash.UseVisualStyleBackColor = true;
            this.Btn_GetBacklash.Click += new System.EventHandler(this.Btn_GetBacklash_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Btn_SetBacklash);
            this.groupBox1.Controls.Add(this.Btn_GetBacklash);
            this.groupBox1.Controls.Add(this.Num_SetBacklash);
            this.groupBox1.Controls.Add(this.Tbx_GetBacklash);
            this.groupBox1.Location = new System.Drawing.Point(386, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 149);
            this.groupBox1.TabIndex = 135;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Backlash";
            // 
            // Btn_Enable
            // 
            this.Btn_Enable.Location = new System.Drawing.Point(10, 74);
            this.Btn_Enable.Name = "Btn_Enable";
            this.Btn_Enable.Size = new System.Drawing.Size(99, 50);
            this.Btn_Enable.TabIndex = 137;
            this.Btn_Enable.Text = "Enable";
            this.Btn_Enable.UseVisualStyleBackColor = true;
            this.Btn_Enable.Click += new System.EventHandler(this.Btn_Enable_Click);
            // 
            // Btn_Disable
            // 
            this.Btn_Disable.Location = new System.Drawing.Point(115, 74);
            this.Btn_Disable.Name = "Btn_Disable";
            this.Btn_Disable.Size = new System.Drawing.Size(99, 50);
            this.Btn_Disable.TabIndex = 136;
            this.Btn_Disable.Text = "Disable";
            this.Btn_Disable.UseVisualStyleBackColor = true;
            this.Btn_Disable.Click += new System.EventHandler(this.Btn_Disable_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bntSearchHome);
            this.groupBox2.Controls.Add(this.Label16);
            this.groupBox2.Controls.Add(this.Num_HomeSpeed);
            this.groupBox2.Controls.Add(this.Num_HomeTimeout);
            this.groupBox2.Controls.Add(this.Label13);
            this.groupBox2.Location = new System.Drawing.Point(10, 118);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(370, 105);
            this.groupBox2.TabIndex = 138;
            this.groupBox2.TabStop = false;
            // 
            // bntSearchHome
            // 
            this.bntSearchHome.BackColor = System.Drawing.SystemColors.Control;
            this.bntSearchHome.Location = new System.Drawing.Point(6, 24);
            this.bntSearchHome.Name = "bntSearchHome";
            this.bntSearchHome.Size = new System.Drawing.Size(129, 69);
            this.bntSearchHome.TabIndex = 140;
            this.bntSearchHome.Text = "Search Home";
            this.bntSearchHome.UseVisualStyleBackColor = false;
            this.bntSearchHome.Click += new System.EventHandler(this.bntSearchHome_Click);
            // 
            // Label16
            // 
            this.Label16.AutoSize = true;
            this.Label16.Location = new System.Drawing.Point(161, 25);
            this.Label16.Name = "Label16";
            this.Label16.Size = new System.Drawing.Size(97, 26);
            this.Label16.TabIndex = 142;
            this.Label16.Text = "Timeout";
            // 
            // Num_HomeSpeed
            // 
            this.Num_HomeSpeed.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.Num_HomeSpeed.Location = new System.Drawing.Point(264, 64);
            this.Num_HomeSpeed.Maximum = new decimal(new int[] {
            60000000,
            0,
            0,
            0});
            this.Num_HomeSpeed.Name = "Num_HomeSpeed";
            this.Num_HomeSpeed.Size = new System.Drawing.Size(100, 35);
            this.Num_HomeSpeed.TabIndex = 141;
            this.Num_HomeSpeed.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // Num_HomeTimeout
            // 
            this.Num_HomeTimeout.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.Num_HomeTimeout.Location = new System.Drawing.Point(264, 23);
            this.Num_HomeTimeout.Maximum = new decimal(new int[] {
            6000000,
            0,
            0,
            0});
            this.Num_HomeTimeout.Name = "Num_HomeTimeout";
            this.Num_HomeTimeout.Size = new System.Drawing.Size(100, 35);
            this.Num_HomeTimeout.TabIndex = 143;
            this.Num_HomeTimeout.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // Label13
            // 
            this.Label13.AutoSize = true;
            this.Label13.Location = new System.Drawing.Point(182, 66);
            this.Label13.Name = "Label13";
            this.Label13.Size = new System.Drawing.Size(76, 26);
            this.Label13.TabIndex = 139;
            this.Label13.Text = "Speed";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.Btn_GoHome);
            this.groupBox3.Controls.Add(this.Num_RelPos);
            this.groupBox3.Controls.Add(this.Btn_AbsMove);
            this.groupBox3.Controls.Add(this.Label15);
            this.groupBox3.Controls.Add(this.Btn_RelMove);
            this.groupBox3.Controls.Add(this.Label14);
            this.groupBox3.Controls.Add(this.Num_AbsPos);
            this.groupBox3.Controls.Add(this.Num_MoveTimeout);
            this.groupBox3.Controls.Add(this.Num_MoveSpeed);
            this.groupBox3.Location = new System.Drawing.Point(10, 229);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(585, 160);
            this.groupBox3.TabIndex = 139;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Move";
            // 
            // Btn_GoHome
            // 
            this.Btn_GoHome.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_GoHome.Location = new System.Drawing.Point(482, 53);
            this.Btn_GoHome.Name = "Btn_GoHome";
            this.Btn_GoHome.Size = new System.Drawing.Size(94, 67);
            this.Btn_GoHome.TabIndex = 145;
            this.Btn_GoHome.Text = "Go Home";
            this.Btn_GoHome.UseVisualStyleBackColor = false;
            this.Btn_GoHome.Click += new System.EventHandler(this.Btn_GoHome_Click);
            // 
            // Num_RelPos
            // 
            this.Num_RelPos.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.Num_RelPos.Location = new System.Drawing.Point(141, 99);
            this.Num_RelPos.Maximum = new decimal(new int[] {
            60000000,
            0,
            0,
            0});
            this.Num_RelPos.Minimum = new decimal(new int[] {
            60000000,
            0,
            0,
            -2147483648});
            this.Num_RelPos.Name = "Num_RelPos";
            this.Num_RelPos.Size = new System.Drawing.Size(100, 35);
            this.Num_RelPos.TabIndex = 147;
            this.Num_RelPos.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // Btn_AbsMove
            // 
            this.Btn_AbsMove.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_AbsMove.Location = new System.Drawing.Point(6, 34);
            this.Btn_AbsMove.Name = "Btn_AbsMove";
            this.Btn_AbsMove.Size = new System.Drawing.Size(129, 50);
            this.Btn_AbsMove.TabIndex = 140;
            this.Btn_AbsMove.Text = "Abs. Move";
            this.Btn_AbsMove.UseVisualStyleBackColor = false;
            this.Btn_AbsMove.Click += new System.EventHandler(this.Btn_AbsMove_Click);
            // 
            // Label15
            // 
            this.Label15.AutoSize = true;
            this.Label15.Location = new System.Drawing.Point(281, 94);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(76, 26);
            this.Label15.TabIndex = 148;
            this.Label15.Text = "Speed";
            // 
            // Btn_RelMove
            // 
            this.Btn_RelMove.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_RelMove.Location = new System.Drawing.Point(6, 90);
            this.Btn_RelMove.Name = "Btn_RelMove";
            this.Btn_RelMove.Size = new System.Drawing.Size(129, 50);
            this.Btn_RelMove.TabIndex = 146;
            this.Btn_RelMove.Text = "Rel. Move";
            this.Btn_RelMove.UseVisualStyleBackColor = false;
            this.Btn_RelMove.Click += new System.EventHandler(this.Btn_RelMove_Click);
            // 
            // Label14
            // 
            this.Label14.AutoSize = true;
            this.Label14.Location = new System.Drawing.Point(260, 53);
            this.Label14.Name = "Label14";
            this.Label14.Size = new System.Drawing.Size(97, 26);
            this.Label14.TabIndex = 141;
            this.Label14.Text = "Timeout";
            // 
            // Num_AbsPos
            // 
            this.Num_AbsPos.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.Num_AbsPos.Location = new System.Drawing.Point(141, 44);
            this.Num_AbsPos.Maximum = new decimal(new int[] {
            60000000,
            0,
            0,
            0});
            this.Num_AbsPos.Minimum = new decimal(new int[] {
            60000000,
            0,
            0,
            -2147483648});
            this.Num_AbsPos.Name = "Num_AbsPos";
            this.Num_AbsPos.Size = new System.Drawing.Size(100, 35);
            this.Num_AbsPos.TabIndex = 142;
            this.Num_AbsPos.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // Num_MoveTimeout
            // 
            this.Num_MoveTimeout.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.Num_MoveTimeout.Location = new System.Drawing.Point(363, 51);
            this.Num_MoveTimeout.Maximum = new decimal(new int[] {
            6000000,
            0,
            0,
            0});
            this.Num_MoveTimeout.Name = "Num_MoveTimeout";
            this.Num_MoveTimeout.Size = new System.Drawing.Size(100, 35);
            this.Num_MoveTimeout.TabIndex = 143;
            this.Num_MoveTimeout.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // Num_MoveSpeed
            // 
            this.Num_MoveSpeed.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.Num_MoveSpeed.Location = new System.Drawing.Point(363, 92);
            this.Num_MoveSpeed.Maximum = new decimal(new int[] {
            60000000,
            0,
            0,
            0});
            this.Num_MoveSpeed.Name = "Num_MoveSpeed";
            this.Num_MoveSpeed.Size = new System.Drawing.Size(100, 35);
            this.Num_MoveSpeed.TabIndex = 144;
            this.Num_MoveSpeed.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // Cbx_Read
            // 
            this.Cbx_Read.AutoSize = true;
            this.Cbx_Read.Location = new System.Drawing.Point(180, 49);
            this.Cbx_Read.Name = "Cbx_Read";
            this.Cbx_Read.Size = new System.Drawing.Size(103, 30);
            this.Cbx_Read.TabIndex = 140;
            this.Cbx_Read.Text = "Polling";
            this.Cbx_Read.UseVisualStyleBackColor = true;
            this.Cbx_Read.CheckedChanged += new System.EventHandler(this.Cbx_Read_CheckedChanged);
            // 
            // grpStatus
            // 
            this.grpStatus.Controls.Add(this.Btn_ReadStatus);
            this.grpStatus.Controls.Add(this.Label9);
            this.grpStatus.Controls.Add(this.Tbx_Speed);
            this.grpStatus.Controls.Add(this.Tbx_TargetPos);
            this.grpStatus.Controls.Add(this.Label7);
            this.grpStatus.Controls.Add(this.Label6);
            this.grpStatus.Controls.Add(this.Tbx_CurrentPos);
            this.grpStatus.Controls.Add(this.Label12);
            this.grpStatus.Controls.Add(this.Pbx_Moving);
            this.grpStatus.Controls.Add(this.Label11);
            this.grpStatus.Controls.Add(this.Pbx_Limit);
            this.grpStatus.Controls.Add(this.Label10);
            this.grpStatus.Controls.Add(this.Pbx_Home);
            this.grpStatus.Controls.Add(this.Pbx_Enable);
            this.grpStatus.Controls.Add(this.Label8);
            this.grpStatus.Controls.Add(this.Cbx_Read);
            this.grpStatus.Location = new System.Drawing.Point(10, 395);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Size = new System.Drawing.Size(608, 172);
            this.grpStatus.TabIndex = 141;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "Driver Status";
            // 
            // Btn_ReadStatus
            // 
            this.Btn_ReadStatus.BackColor = System.Drawing.SystemColors.Control;
            this.Btn_ReadStatus.Location = new System.Drawing.Point(22, 34);
            this.Btn_ReadStatus.Name = "Btn_ReadStatus";
            this.Btn_ReadStatus.Size = new System.Drawing.Size(144, 61);
            this.Btn_ReadStatus.TabIndex = 142;
            this.Btn_ReadStatus.Text = "Read Status";
            this.Btn_ReadStatus.UseVisualStyleBackColor = false;
            this.Btn_ReadStatus.Click += new System.EventHandler(this.Btn_ReadStatus_Click);
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Location = new System.Drawing.Point(420, 47);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(76, 26);
            this.Label9.TabIndex = 153;
            this.Label9.Text = "Speed";
            // 
            // Tbx_Speed
            // 
            this.Tbx_Speed.BackColor = System.Drawing.Color.White;
            this.Tbx_Speed.Location = new System.Drawing.Point(502, 44);
            this.Tbx_Speed.Name = "Tbx_Speed";
            this.Tbx_Speed.ReadOnly = true;
            this.Tbx_Speed.Size = new System.Drawing.Size(100, 35);
            this.Tbx_Speed.TabIndex = 154;
            // 
            // Tbx_TargetPos
            // 
            this.Tbx_TargetPos.BackColor = System.Drawing.Color.White;
            this.Tbx_TargetPos.Location = new System.Drawing.Point(502, 129);
            this.Tbx_TargetPos.Name = "Tbx_TargetPos";
            this.Tbx_TargetPos.ReadOnly = true;
            this.Tbx_TargetPos.Size = new System.Drawing.Size(100, 35);
            this.Tbx_TargetPos.TabIndex = 152;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(329, 132);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(167, 26);
            this.Label7.TabIndex = 151;
            this.Label7.Text = "Target Position";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(319, 91);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(177, 26);
            this.Label6.TabIndex = 149;
            this.Label6.Text = "Current Position";
            // 
            // Tbx_CurrentPos
            // 
            this.Tbx_CurrentPos.BackColor = System.Drawing.Color.White;
            this.Tbx_CurrentPos.Location = new System.Drawing.Point(502, 88);
            this.Tbx_CurrentPos.Name = "Tbx_CurrentPos";
            this.Tbx_CurrentPos.ReadOnly = true;
            this.Tbx_CurrentPos.Size = new System.Drawing.Size(100, 35);
            this.Tbx_CurrentPos.TabIndex = 150;
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Location = new System.Drawing.Point(175, 141);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(91, 26);
            this.Label12.TabIndex = 148;
            this.Label12.Text = "Moving";
            // 
            // Pbx_Moving
            // 
            this.Pbx_Moving.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pbx_Moving.Location = new System.Drawing.Point(272, 142);
            this.Pbx_Moving.Name = "Pbx_Moving";
            this.Pbx_Moving.Size = new System.Drawing.Size(25, 25);
            this.Pbx_Moving.TabIndex = 147;
            this.Pbx_Moving.TabStop = false;
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Location = new System.Drawing.Point(36, 142);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(63, 26);
            this.Label11.TabIndex = 146;
            this.Label11.Text = "Limit";
            // 
            // Pbx_Limit
            // 
            this.Pbx_Limit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pbx_Limit.Location = new System.Drawing.Point(105, 142);
            this.Pbx_Limit.Name = "Pbx_Limit";
            this.Pbx_Limit.Size = new System.Drawing.Size(25, 25);
            this.Pbx_Limit.TabIndex = 145;
            this.Pbx_Limit.TabStop = false;
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Location = new System.Drawing.Point(191, 107);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(75, 26);
            this.Label10.TabIndex = 144;
            this.Label10.Text = "Home";
            // 
            // Pbx_Home
            // 
            this.Pbx_Home.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pbx_Home.Location = new System.Drawing.Point(272, 108);
            this.Pbx_Home.Name = "Pbx_Home";
            this.Pbx_Home.Size = new System.Drawing.Size(25, 25);
            this.Pbx_Home.TabIndex = 143;
            this.Pbx_Home.TabStop = false;
            // 
            // Pbx_Enable
            // 
            this.Pbx_Enable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pbx_Enable.Location = new System.Drawing.Point(105, 108);
            this.Pbx_Enable.Name = "Pbx_Enable";
            this.Pbx_Enable.Size = new System.Drawing.Size(25, 25);
            this.Pbx_Enable.TabIndex = 142;
            this.Pbx_Enable.TabStop = false;
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Location = new System.Drawing.Point(18, 107);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(81, 26);
            this.Label8.TabIndex = 141;
            this.Label8.Text = "Enable";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(233, 86);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(77, 26);
            this.Label3.TabIndex = 145;
            this.Label3.Text = "Motor";
            // 
            // Cbx_MotorIdx
            // 
            this.Cbx_MotorIdx.FormattingEnabled = true;
            this.Cbx_MotorIdx.Items.AddRange(new object[] {
            "1",
            "2"});
            this.Cbx_MotorIdx.Location = new System.Drawing.Point(316, 83);
            this.Cbx_MotorIdx.Name = "Cbx_MotorIdx";
            this.Cbx_MotorIdx.Size = new System.Drawing.Size(47, 34);
            this.Cbx_MotorIdx.TabIndex = 146;
            this.Cbx_MotorIdx.Text = "1";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.Btn_SetLimitF);
            this.groupBox4.Controls.Add(this.Btn_GetLimitF);
            this.groupBox4.Controls.Add(this.Num_SetLimitF);
            this.groupBox4.Controls.Add(this.Tbx_GetLimitF);
            this.groupBox4.Location = new System.Drawing.Point(601, 74);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(209, 149);
            this.groupBox4.TabIndex = 147;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Limit +";
            // 
            // Btn_SetLimitF
            // 
            this.Btn_SetLimitF.Location = new System.Drawing.Point(6, 34);
            this.Btn_SetLimitF.Name = "Btn_SetLimitF";
            this.Btn_SetLimitF.Size = new System.Drawing.Size(83, 50);
            this.Btn_SetLimitF.TabIndex = 131;
            this.Btn_SetLimitF.Text = "Set";
            this.Btn_SetLimitF.UseVisualStyleBackColor = true;
            this.Btn_SetLimitF.Click += new System.EventHandler(this.Btn_SetLimitF_Click);
            // 
            // Btn_GetLimitF
            // 
            this.Btn_GetLimitF.Location = new System.Drawing.Point(6, 90);
            this.Btn_GetLimitF.Name = "Btn_GetLimitF";
            this.Btn_GetLimitF.Size = new System.Drawing.Size(83, 50);
            this.Btn_GetLimitF.TabIndex = 134;
            this.Btn_GetLimitF.Text = "Get";
            this.Btn_GetLimitF.UseVisualStyleBackColor = true;
            this.Btn_GetLimitF.Click += new System.EventHandler(this.Btn_GetLimitF_Click);
            // 
            // Num_SetLimitF
            // 
            this.Num_SetLimitF.Location = new System.Drawing.Point(95, 44);
            this.Num_SetLimitF.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.Num_SetLimitF.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.Num_SetLimitF.Name = "Num_SetLimitF";
            this.Num_SetLimitF.Size = new System.Drawing.Size(100, 35);
            this.Num_SetLimitF.TabIndex = 130;
            // 
            // Tbx_GetLimitF
            // 
            this.Tbx_GetLimitF.BackColor = System.Drawing.Color.White;
            this.Tbx_GetLimitF.Location = new System.Drawing.Point(95, 99);
            this.Tbx_GetLimitF.Name = "Tbx_GetLimitF";
            this.Tbx_GetLimitF.ReadOnly = true;
            this.Tbx_GetLimitF.Size = new System.Drawing.Size(100, 35);
            this.Tbx_GetLimitF.TabIndex = 133;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.Btn_SetLimitR);
            this.groupBox5.Controls.Add(this.Btn_GetLimitR);
            this.groupBox5.Controls.Add(this.Num_SetLimitR);
            this.groupBox5.Controls.Add(this.Tbx_GetLimitR);
            this.groupBox5.Location = new System.Drawing.Point(816, 74);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(209, 149);
            this.groupBox5.TabIndex = 148;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Limit -";
            // 
            // Btn_SetLimitR
            // 
            this.Btn_SetLimitR.Location = new System.Drawing.Point(6, 34);
            this.Btn_SetLimitR.Name = "Btn_SetLimitR";
            this.Btn_SetLimitR.Size = new System.Drawing.Size(83, 50);
            this.Btn_SetLimitR.TabIndex = 131;
            this.Btn_SetLimitR.Text = "Set";
            this.Btn_SetLimitR.UseVisualStyleBackColor = true;
            this.Btn_SetLimitR.Click += new System.EventHandler(this.Btn_SetLimitR_Click);
            // 
            // Btn_GetLimitR
            // 
            this.Btn_GetLimitR.Location = new System.Drawing.Point(6, 90);
            this.Btn_GetLimitR.Name = "Btn_GetLimitR";
            this.Btn_GetLimitR.Size = new System.Drawing.Size(83, 50);
            this.Btn_GetLimitR.TabIndex = 134;
            this.Btn_GetLimitR.Text = "Get";
            this.Btn_GetLimitR.UseVisualStyleBackColor = true;
            this.Btn_GetLimitR.Click += new System.EventHandler(this.Btn_GetLimitR_Click);
            // 
            // Num_SetLimitR
            // 
            this.Num_SetLimitR.Location = new System.Drawing.Point(95, 44);
            this.Num_SetLimitR.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.Num_SetLimitR.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.Num_SetLimitR.Name = "Num_SetLimitR";
            this.Num_SetLimitR.Size = new System.Drawing.Size(100, 35);
            this.Num_SetLimitR.TabIndex = 130;
            // 
            // Tbx_GetLimitR
            // 
            this.Tbx_GetLimitR.BackColor = System.Drawing.Color.White;
            this.Tbx_GetLimitR.Location = new System.Drawing.Point(95, 99);
            this.Tbx_GetLimitR.Name = "Tbx_GetLimitR";
            this.Tbx_GetLimitR.ReadOnly = true;
            this.Tbx_GetLimitR.Size = new System.Drawing.Size(100, 35);
            this.Tbx_GetLimitR.TabIndex = 133;
            // 
            // AuoMotor_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 26F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1032, 575);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.Cbx_MotorIdx);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.grpStatus);
            this.Controls.Add(this.Btn_Disable);
            this.Controls.Add(this.Btn_Enable);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(7);
            this.Name = "AuoMotor_Tool";
            this.Padding = new System.Windows.Forms.Padding(7, 139, 7, 7);
            this.Text = "AuoMotorTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AuoMotor_Tool_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.Num_SetBacklash)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_HomeSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_HomeTimeout)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_RelPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_AbsPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MoveTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MoveSpeed)).EndInit();
            this.grpStatus.ResumeLayout(false);
            this.grpStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Moving)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Limit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Home)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx_Enable)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_SetLimitF)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_SetLimitR)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.NumericUpDown Num_SetBacklash;
        private System.Windows.Forms.Button Btn_SetBacklash;
        private System.Windows.Forms.TextBox Tbx_GetBacklash;
        private System.Windows.Forms.Button Btn_GetBacklash;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button Btn_Enable;
        private System.Windows.Forms.Button Btn_Disable;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.Button bntSearchHome;
        internal System.Windows.Forms.Label Label16;
        internal System.Windows.Forms.NumericUpDown Num_HomeSpeed;
        internal System.Windows.Forms.NumericUpDown Num_HomeTimeout;
        internal System.Windows.Forms.Label Label13;
        private System.Windows.Forms.GroupBox groupBox3;
        internal System.Windows.Forms.Button Btn_GoHome;
        internal System.Windows.Forms.NumericUpDown Num_RelPos;
        internal System.Windows.Forms.Button Btn_AbsMove;
        internal System.Windows.Forms.Label Label15;
        internal System.Windows.Forms.Button Btn_RelMove;
        internal System.Windows.Forms.Label Label14;
        internal System.Windows.Forms.NumericUpDown Num_AbsPos;
        internal System.Windows.Forms.NumericUpDown Num_MoveTimeout;
        internal System.Windows.Forms.NumericUpDown Num_MoveSpeed;
        private System.Windows.Forms.CheckBox Cbx_Read;
        internal System.Windows.Forms.GroupBox grpStatus;
        internal System.Windows.Forms.Label Label12;
        internal System.Windows.Forms.PictureBox Pbx_Moving;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.PictureBox Pbx_Limit;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.PictureBox Pbx_Home;
        internal System.Windows.Forms.PictureBox Pbx_Enable;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.TextBox Tbx_Speed;
        internal System.Windows.Forms.TextBox Tbx_TargetPos;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.TextBox Tbx_CurrentPos;
        internal System.Windows.Forms.Button Btn_ReadStatus;
        internal System.Windows.Forms.Label Label3;
        private System.Windows.Forms.ComboBox Cbx_MotorIdx;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button Btn_SetLimitF;
        private System.Windows.Forms.Button Btn_GetLimitF;
        internal System.Windows.Forms.NumericUpDown Num_SetLimitF;
        private System.Windows.Forms.TextBox Tbx_GetLimitF;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button Btn_SetLimitR;
        private System.Windows.Forms.Button Btn_GetLimitR;
        internal System.Windows.Forms.NumericUpDown Num_SetLimitR;
        private System.Windows.Forms.TextBox Tbx_GetLimitR;
    }
}