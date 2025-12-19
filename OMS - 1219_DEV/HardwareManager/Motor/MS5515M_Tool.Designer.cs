
namespace HardwareManager
{
    partial class MS5515M_Tool
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
            this.label1 = new System.Windows.Forms.Label();
            this.Btn_SetSpeed = new System.Windows.Forms.Button();
            this.Num_Speed = new System.Windows.Forms.NumericUpDown();
            this.Num_Angle = new System.Windows.Forms.NumericUpDown();
            this.Num_Pos = new System.Windows.Forms.NumericUpDown();
            this.Btn_SearchHome = new System.Windows.Forms.Button();
            this.Btn_GoHome = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.Btn_TeachOff = new System.Windows.Forms.Button();
            this.Btn_TeachOn = new System.Windows.Forms.Button();
            this.Rbtn_Pos = new System.Windows.Forms.RadioButton();
            this.Rbtn_Angle = new System.Windows.Forms.RadioButton();
            this.Btn_MoveF = new System.Windows.Forms.Button();
            this.Btn_MoveR = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.Tbx_Status = new System.Windows.Forms.TextBox();
            this.Tbx_AbsPos = new System.Windows.Forms.TextBox();
            this.Tbx_RelPos = new System.Windows.Forms.TextBox();
            this.Tbx_Frequecy = new System.Windows.Forms.TextBox();
            this.Cbx_Read = new System.Windows.Forms.CheckBox();
            this.Btn_Move = new System.Windows.Forms.Button();
            this.Num_AbsPos = new System.Windows.Forms.NumericUpDown();
            this.Btn_Read = new System.Windows.Forms.Button();
            this.Btn_Stop = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Speed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Angle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Pos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_AbsPos)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 156);
            this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "轉速";
            // 
            // Btn_SetSpeed
            // 
            this.Btn_SetSpeed.Location = new System.Drawing.Point(217, 144);
            this.Btn_SetSpeed.Name = "Btn_SetSpeed";
            this.Btn_SetSpeed.Size = new System.Drawing.Size(83, 50);
            this.Btn_SetSpeed.TabIndex = 2;
            this.Btn_SetSpeed.Text = "設定";
            this.Btn_SetSpeed.UseVisualStyleBackColor = true;
            this.Btn_SetSpeed.Click += new System.EventHandler(this.Btn_SetSpeed_Click);
            // 
            // Num_Speed
            // 
            this.Num_Speed.Location = new System.Drawing.Point(91, 154);
            this.Num_Speed.Name = "Num_Speed";
            this.Num_Speed.Size = new System.Drawing.Size(120, 35);
            this.Num_Speed.TabIndex = 9;
            // 
            // Num_Angle
            // 
            this.Num_Angle.Location = new System.Drawing.Point(91, 259);
            this.Num_Angle.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.Num_Angle.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.Num_Angle.Name = "Num_Angle";
            this.Num_Angle.Size = new System.Drawing.Size(120, 35);
            this.Num_Angle.TabIndex = 10;
            // 
            // Num_Pos
            // 
            this.Num_Pos.Location = new System.Drawing.Point(91, 218);
            this.Num_Pos.Maximum = new decimal(new int[] {
            270000,
            0,
            0,
            0});
            this.Num_Pos.Name = "Num_Pos";
            this.Num_Pos.Size = new System.Drawing.Size(120, 35);
            this.Num_Pos.TabIndex = 11;
            // 
            // Btn_SearchHome
            // 
            this.Btn_SearchHome.Location = new System.Drawing.Point(10, 75);
            this.Btn_SearchHome.Name = "Btn_SearchHome";
            this.Btn_SearchHome.Size = new System.Drawing.Size(125, 50);
            this.Btn_SearchHome.TabIndex = 12;
            this.Btn_SearchHome.Text = "尋找原點";
            this.Btn_SearchHome.UseVisualStyleBackColor = true;
            this.Btn_SearchHome.Click += new System.EventHandler(this.Btn_SearchHome_Click);
            // 
            // Btn_GoHome
            // 
            this.Btn_GoHome.Location = new System.Drawing.Point(141, 75);
            this.Btn_GoHome.Name = "Btn_GoHome";
            this.Btn_GoHome.Size = new System.Drawing.Size(125, 50);
            this.Btn_GoHome.TabIndex = 13;
            this.Btn_GoHome.Text = "回原點";
            this.Btn_GoHome.UseVisualStyleBackColor = true;
            this.Btn_GoHome.Click += new System.EventHandler(this.Btn_GoHome_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(397, 87);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 26);
            this.label4.TabIndex = 14;
            this.label4.Text = "調試打印";
            // 
            // Btn_TeachOff
            // 
            this.Btn_TeachOff.Location = new System.Drawing.Point(579, 75);
            this.Btn_TeachOff.Name = "Btn_TeachOff";
            this.Btn_TeachOff.Size = new System.Drawing.Size(70, 50);
            this.Btn_TeachOff.TabIndex = 15;
            this.Btn_TeachOff.Text = "關";
            this.Btn_TeachOff.UseVisualStyleBackColor = true;
            this.Btn_TeachOff.Click += new System.EventHandler(this.Btn_TeachOff_Click);
            // 
            // Btn_TeachOn
            // 
            this.Btn_TeachOn.Location = new System.Drawing.Point(503, 75);
            this.Btn_TeachOn.Name = "Btn_TeachOn";
            this.Btn_TeachOn.Size = new System.Drawing.Size(70, 50);
            this.Btn_TeachOn.TabIndex = 16;
            this.Btn_TeachOn.Text = "開";
            this.Btn_TeachOn.UseVisualStyleBackColor = true;
            this.Btn_TeachOn.Click += new System.EventHandler(this.Btn_TeachOn_Click);
            // 
            // Rbtn_Pos
            // 
            this.Rbtn_Pos.AutoSize = true;
            this.Rbtn_Pos.Location = new System.Drawing.Point(13, 218);
            this.Rbtn_Pos.Name = "Rbtn_Pos";
            this.Rbtn_Pos.Size = new System.Drawing.Size(72, 30);
            this.Rbtn_Pos.TabIndex = 17;
            this.Rbtn_Pos.TabStop = true;
            this.Rbtn_Pos.Text = "位置";
            this.Rbtn_Pos.UseVisualStyleBackColor = true;
            // 
            // Rbtn_Angle
            // 
            this.Rbtn_Angle.AutoSize = true;
            this.Rbtn_Angle.Location = new System.Drawing.Point(13, 259);
            this.Rbtn_Angle.Name = "Rbtn_Angle";
            this.Rbtn_Angle.Size = new System.Drawing.Size(72, 30);
            this.Rbtn_Angle.TabIndex = 18;
            this.Rbtn_Angle.TabStop = true;
            this.Rbtn_Angle.Text = "角度";
            this.Rbtn_Angle.UseVisualStyleBackColor = true;
            // 
            // Btn_MoveF
            // 
            this.Btn_MoveF.Location = new System.Drawing.Point(217, 226);
            this.Btn_MoveF.Name = "Btn_MoveF";
            this.Btn_MoveF.Size = new System.Drawing.Size(85, 50);
            this.Btn_MoveF.TabIndex = 19;
            this.Btn_MoveF.Tag = "+";
            this.Btn_MoveF.Text = "正轉";
            this.Btn_MoveF.UseVisualStyleBackColor = true;
            this.Btn_MoveF.Click += new System.EventHandler(this.Btn_Move_Click);
            // 
            // Btn_MoveR
            // 
            this.Btn_MoveR.Location = new System.Drawing.Point(308, 226);
            this.Btn_MoveR.Name = "Btn_MoveR";
            this.Btn_MoveR.Size = new System.Drawing.Size(85, 50);
            this.Btn_MoveR.TabIndex = 20;
            this.Btn_MoveR.Tag = "-";
            this.Btn_MoveR.Text = "反轉";
            this.Btn_MoveR.UseVisualStyleBackColor = true;
            this.Btn_MoveR.Click += new System.EventHandler(this.Btn_Move_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(443, 240);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 26);
            this.label2.TabIndex = 21;
            this.label2.Text = "電機狀態";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(443, 281);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 26);
            this.label3.TabIndex = 22;
            this.label3.Text = "絕對位置";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(443, 322);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 26);
            this.label5.TabIndex = 23;
            this.label5.Text = "相對位置";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(443, 363);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 26);
            this.label6.TabIndex = 24;
            this.label6.Text = "驅動頻率";
            // 
            // Tbx_Status
            // 
            this.Tbx_Status.Location = new System.Drawing.Point(545, 237);
            this.Tbx_Status.Name = "Tbx_Status";
            this.Tbx_Status.ReadOnly = true;
            this.Tbx_Status.Size = new System.Drawing.Size(100, 35);
            this.Tbx_Status.TabIndex = 25;
            // 
            // Tbx_AbsPos
            // 
            this.Tbx_AbsPos.Location = new System.Drawing.Point(545, 278);
            this.Tbx_AbsPos.Name = "Tbx_AbsPos";
            this.Tbx_AbsPos.ReadOnly = true;
            this.Tbx_AbsPos.Size = new System.Drawing.Size(100, 35);
            this.Tbx_AbsPos.TabIndex = 26;
            // 
            // Tbx_RelPos
            // 
            this.Tbx_RelPos.Location = new System.Drawing.Point(545, 319);
            this.Tbx_RelPos.Name = "Tbx_RelPos";
            this.Tbx_RelPos.ReadOnly = true;
            this.Tbx_RelPos.Size = new System.Drawing.Size(100, 35);
            this.Tbx_RelPos.TabIndex = 27;
            // 
            // Tbx_Frequecy
            // 
            this.Tbx_Frequecy.Location = new System.Drawing.Point(545, 360);
            this.Tbx_Frequecy.Name = "Tbx_Frequecy";
            this.Tbx_Frequecy.ReadOnly = true;
            this.Tbx_Frequecy.Size = new System.Drawing.Size(100, 35);
            this.Tbx_Frequecy.TabIndex = 28;
            // 
            // Cbx_Read
            // 
            this.Cbx_Read.AutoSize = true;
            this.Cbx_Read.Location = new System.Drawing.Point(448, 202);
            this.Cbx_Read.Name = "Cbx_Read";
            this.Cbx_Read.Size = new System.Drawing.Size(115, 30);
            this.Cbx_Read.TabIndex = 29;
            this.Cbx_Read.Text = "讀取狀態";
            this.Cbx_Read.UseVisualStyleBackColor = true;
            this.Cbx_Read.CheckedChanged += new System.EventHandler(this.Cbx_Read_CheckedChanged);
            // 
            // Btn_Move
            // 
            this.Btn_Move.Location = new System.Drawing.Point(132, 371);
            this.Btn_Move.Name = "Btn_Move";
            this.Btn_Move.Size = new System.Drawing.Size(159, 50);
            this.Btn_Move.TabIndex = 32;
            this.Btn_Move.Tag = "+";
            this.Btn_Move.Text = "移動 (含背隙)";
            this.Btn_Move.UseVisualStyleBackColor = true;
            this.Btn_Move.Click += new System.EventHandler(this.Btn_Move_Click_1);
            // 
            // Num_AbsPos
            // 
            this.Num_AbsPos.Location = new System.Drawing.Point(6, 381);
            this.Num_AbsPos.Maximum = new decimal(new int[] {
            280000,
            0,
            0,
            0});
            this.Num_AbsPos.Name = "Num_AbsPos";
            this.Num_AbsPos.Size = new System.Drawing.Size(120, 35);
            this.Num_AbsPos.TabIndex = 30;
            // 
            // Btn_Read
            // 
            this.Btn_Read.Location = new System.Drawing.Point(569, 182);
            this.Btn_Read.Name = "Btn_Read";
            this.Btn_Read.Size = new System.Drawing.Size(83, 50);
            this.Btn_Read.TabIndex = 33;
            this.Btn_Read.Text = "讀取";
            this.Btn_Read.UseVisualStyleBackColor = true;
            this.Btn_Read.Click += new System.EventHandler(this.Btn_Read_Click);
            // 
            // Btn_Stop
            // 
            this.Btn_Stop.BackColor = System.Drawing.Color.Red;
            this.Btn_Stop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_Stop.Location = new System.Drawing.Point(66, 310);
            this.Btn_Stop.Name = "Btn_Stop";
            this.Btn_Stop.Size = new System.Drawing.Size(94, 50);
            this.Btn_Stop.TabIndex = 34;
            this.Btn_Stop.Text = "停止";
            this.Btn_Stop.UseVisualStyleBackColor = false;
            this.Btn_Stop.Click += new System.EventHandler(this.Btn_Stop_Click);
            // 
            // MS5515M_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 26F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 460);
            this.Controls.Add(this.Btn_Stop);
            this.Controls.Add(this.Btn_Read);
            this.Controls.Add(this.Btn_Move);
            this.Controls.Add(this.Num_AbsPos);
            this.Controls.Add(this.Cbx_Read);
            this.Controls.Add(this.Tbx_Frequecy);
            this.Controls.Add(this.Tbx_RelPos);
            this.Controls.Add(this.Tbx_AbsPos);
            this.Controls.Add(this.Tbx_Status);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Btn_MoveR);
            this.Controls.Add(this.Btn_MoveF);
            this.Controls.Add(this.Rbtn_Angle);
            this.Controls.Add(this.Rbtn_Pos);
            this.Controls.Add(this.Btn_TeachOn);
            this.Controls.Add(this.Btn_TeachOff);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Btn_GoHome);
            this.Controls.Add(this.Btn_SearchHome);
            this.Controls.Add(this.Num_Pos);
            this.Controls.Add(this.Num_Angle);
            this.Controls.Add(this.Num_Speed);
            this.Controls.Add(this.Btn_SetSpeed);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(7);
            this.Name = "MS5515M_Tool";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 7, 7);
            this.Text = "MS5515M_Tool";
            this.Load += new System.EventHandler(this.MS5515M_Tool_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Num_Speed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Angle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_Pos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_AbsPos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Btn_SetSpeed;
        private System.Windows.Forms.NumericUpDown Num_Speed;
        private System.Windows.Forms.NumericUpDown Num_Angle;
        private System.Windows.Forms.NumericUpDown Num_Pos;
        private System.Windows.Forms.Button Btn_SearchHome;
        private System.Windows.Forms.Button Btn_GoHome;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button Btn_TeachOff;
        private System.Windows.Forms.Button Btn_TeachOn;
        private System.Windows.Forms.RadioButton Rbtn_Pos;
        private System.Windows.Forms.RadioButton Rbtn_Angle;
        private System.Windows.Forms.Button Btn_MoveF;
        private System.Windows.Forms.Button Btn_MoveR;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox Tbx_Status;
        private System.Windows.Forms.TextBox Tbx_AbsPos;
        private System.Windows.Forms.TextBox Tbx_RelPos;
        private System.Windows.Forms.TextBox Tbx_Frequecy;
        private System.Windows.Forms.CheckBox Cbx_Read;
        private System.Windows.Forms.Button Btn_Move;
        private System.Windows.Forms.NumericUpDown Num_AbsPos;
        private System.Windows.Forms.Button Btn_Read;
        private System.Windows.Forms.Button Btn_Stop;
    }
}