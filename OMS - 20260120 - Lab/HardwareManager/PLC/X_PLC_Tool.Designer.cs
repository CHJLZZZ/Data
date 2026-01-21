
namespace HardwareManager
{
    partial class X_PLC_Tool
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Btn_JogR = new System.Windows.Forms.Button();
            this.Btn_SetJogSpeed = new System.Windows.Forms.Button();
            this.Btn_SetMovePos = new System.Windows.Forms.Button();
            this.Num_JogSpeed = new System.Windows.Forms.NumericUpDown();
            this.Btn_JogF = new System.Windows.Forms.Button();
            this.Btn_SetMoveSpeed = new System.Windows.Forms.Button();
            this.Btn_ServoOff = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Btn_ServoOn = new System.Windows.Forms.Button();
            this.Btn_IncMove = new System.Windows.Forms.Button();
            this.Btn_AbsMove = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Num_MoveSpeed = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.Num_MovePos = new System.Windows.Forms.NumericUpDown();
            this.Btn_AlmReset = new System.Windows.Forms.Button();
            this.Btn_Home = new System.Windows.Forms.Button();
            this.Btn_Stop = new System.Windows.Forms.Button();
            this.DGV_PLC2PC_Data = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_JogSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MoveSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MovePos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_PLC2PC_Data)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DGV_PLC2PC_Data, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 64);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1030, 691);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Btn_JogR);
            this.panel1.Controls.Add(this.Btn_SetJogSpeed);
            this.panel1.Controls.Add(this.Btn_SetMovePos);
            this.panel1.Controls.Add(this.Num_JogSpeed);
            this.panel1.Controls.Add(this.Btn_JogF);
            this.panel1.Controls.Add(this.Btn_SetMoveSpeed);
            this.panel1.Controls.Add(this.Btn_ServoOff);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.Btn_ServoOn);
            this.panel1.Controls.Add(this.Btn_IncMove);
            this.panel1.Controls.Add(this.Btn_AbsMove);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.Num_MoveSpeed);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.Num_MovePos);
            this.panel1.Controls.Add(this.Btn_AlmReset);
            this.panel1.Controls.Add(this.Btn_Home);
            this.panel1.Controls.Add(this.Btn_Stop);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(220, 691);
            this.panel1.TabIndex = 0;
            // 
            // Btn_JogR
            // 
            this.Btn_JogR.BackColor = System.Drawing.Color.Blue;
            this.Btn_JogR.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_JogR.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_JogR.Location = new System.Drawing.Point(109, 319);
            this.Btn_JogR.Name = "Btn_JogR";
            this.Btn_JogR.Size = new System.Drawing.Size(95, 50);
            this.Btn_JogR.TabIndex = 12;
            this.Btn_JogR.Text = "Jog -";
            this.Btn_JogR.UseVisualStyleBackColor = false;
            this.Btn_JogR.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Btn_JogR_MouseDown);
            this.Btn_JogR.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Btn_JogR_MouseUp);
            // 
            // Btn_SetJogSpeed
            // 
            this.Btn_SetJogSpeed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_SetJogSpeed.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_SetJogSpeed.ForeColor = System.Drawing.Color.Blue;
            this.Btn_SetJogSpeed.Location = new System.Drawing.Point(153, 254);
            this.Btn_SetJogSpeed.Name = "Btn_SetJogSpeed";
            this.Btn_SetJogSpeed.Size = new System.Drawing.Size(64, 59);
            this.Btn_SetJogSpeed.TabIndex = 17;
            this.Btn_SetJogSpeed.Text = "SET";
            this.Btn_SetJogSpeed.UseVisualStyleBackColor = false;
            this.Btn_SetJogSpeed.Click += new System.EventHandler(this.Btn_SetJogSpeed_Click);
            // 
            // Btn_SetMovePos
            // 
            this.Btn_SetMovePos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_SetMovePos.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_SetMovePos.ForeColor = System.Drawing.Color.Blue;
            this.Btn_SetMovePos.Location = new System.Drawing.Point(153, 461);
            this.Btn_SetMovePos.Name = "Btn_SetMovePos";
            this.Btn_SetMovePos.Size = new System.Drawing.Size(64, 59);
            this.Btn_SetMovePos.TabIndex = 19;
            this.Btn_SetMovePos.Text = "SET";
            this.Btn_SetMovePos.UseVisualStyleBackColor = false;
            this.Btn_SetMovePos.Click += new System.EventHandler(this.Btn_SetMovePos_Click);
            // 
            // Num_JogSpeed
            // 
            this.Num_JogSpeed.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Num_JogSpeed.Location = new System.Drawing.Point(5, 278);
            this.Num_JogSpeed.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.Num_JogSpeed.Name = "Num_JogSpeed";
            this.Num_JogSpeed.Size = new System.Drawing.Size(123, 35);
            this.Num_JogSpeed.TabIndex = 5;
            // 
            // Btn_JogF
            // 
            this.Btn_JogF.BackColor = System.Drawing.Color.Blue;
            this.Btn_JogF.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_JogF.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_JogF.Location = new System.Drawing.Point(8, 319);
            this.Btn_JogF.Name = "Btn_JogF";
            this.Btn_JogF.Size = new System.Drawing.Size(95, 50);
            this.Btn_JogF.TabIndex = 11;
            this.Btn_JogF.Text = "Jog +";
            this.Btn_JogF.UseVisualStyleBackColor = false;
            this.Btn_JogF.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Btn_JogF_MouseDown);
            this.Btn_JogF.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Btn_JogF_MouseUp);
            // 
            // Btn_SetMoveSpeed
            // 
            this.Btn_SetMoveSpeed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_SetMoveSpeed.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_SetMoveSpeed.ForeColor = System.Drawing.Color.Blue;
            this.Btn_SetMoveSpeed.Location = new System.Drawing.Point(153, 396);
            this.Btn_SetMoveSpeed.Name = "Btn_SetMoveSpeed";
            this.Btn_SetMoveSpeed.Size = new System.Drawing.Size(64, 59);
            this.Btn_SetMoveSpeed.TabIndex = 18;
            this.Btn_SetMoveSpeed.Text = "SET";
            this.Btn_SetMoveSpeed.UseVisualStyleBackColor = false;
            this.Btn_SetMoveSpeed.Click += new System.EventHandler(this.Btn_SetMoveSpeed_Click);
            // 
            // Btn_ServoOff
            // 
            this.Btn_ServoOff.BackColor = System.Drawing.Color.Red;
            this.Btn_ServoOff.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_ServoOff.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_ServoOff.Location = new System.Drawing.Point(103, 4);
            this.Btn_ServoOff.Name = "Btn_ServoOff";
            this.Btn_ServoOff.Size = new System.Drawing.Size(95, 75);
            this.Btn_ServoOff.TabIndex = 16;
            this.Btn_ServoOff.Text = "Servo OFF";
            this.Btn_ServoOff.UseVisualStyleBackColor = false;
            this.Btn_ServoOff.Click += new System.EventHandler(this.Btn_ServoOff_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(4, 254);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 21);
            this.label1.TabIndex = 6;
            this.label1.Text = "Jog Speed";
            // 
            // Btn_ServoOn
            // 
            this.Btn_ServoOn.BackColor = System.Drawing.Color.Green;
            this.Btn_ServoOn.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_ServoOn.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_ServoOn.Location = new System.Drawing.Point(5, 4);
            this.Btn_ServoOn.Name = "Btn_ServoOn";
            this.Btn_ServoOn.Size = new System.Drawing.Size(95, 75);
            this.Btn_ServoOn.TabIndex = 15;
            this.Btn_ServoOn.Text = "Servo ON";
            this.Btn_ServoOn.UseVisualStyleBackColor = false;
            this.Btn_ServoOn.Click += new System.EventHandler(this.Btn_ServoOn_Click);
            // 
            // Btn_IncMove
            // 
            this.Btn_IncMove.BackColor = System.Drawing.Color.Blue;
            this.Btn_IncMove.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_IncMove.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_IncMove.Location = new System.Drawing.Point(106, 526);
            this.Btn_IncMove.Name = "Btn_IncMove";
            this.Btn_IncMove.Size = new System.Drawing.Size(95, 75);
            this.Btn_IncMove.TabIndex = 14;
            this.Btn_IncMove.Text = "INC Move";
            this.Btn_IncMove.UseVisualStyleBackColor = false;
            this.Btn_IncMove.Click += new System.EventHandler(this.Btn_IncMove_Click);
            // 
            // Btn_AbsMove
            // 
            this.Btn_AbsMove.BackColor = System.Drawing.Color.Blue;
            this.Btn_AbsMove.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_AbsMove.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_AbsMove.Location = new System.Drawing.Point(5, 526);
            this.Btn_AbsMove.Name = "Btn_AbsMove";
            this.Btn_AbsMove.Size = new System.Drawing.Size(95, 75);
            this.Btn_AbsMove.TabIndex = 13;
            this.Btn_AbsMove.Text = "ABS Move";
            this.Btn_AbsMove.UseVisualStyleBackColor = false;
            this.Btn_AbsMove.Click += new System.EventHandler(this.Btn_AbsMove_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(2, 396);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 21);
            this.label3.TabIndex = 10;
            this.label3.Text = "Move Speed";
            // 
            // Num_MoveSpeed
            // 
            this.Num_MoveSpeed.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Num_MoveSpeed.Location = new System.Drawing.Point(6, 420);
            this.Num_MoveSpeed.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.Num_MoveSpeed.Name = "Num_MoveSpeed";
            this.Num_MoveSpeed.Size = new System.Drawing.Size(141, 35);
            this.Num_MoveSpeed.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(5, 461);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 21);
            this.label2.TabIndex = 8;
            this.label2.Text = "Move Pos";
            // 
            // Num_MovePos
            // 
            this.Num_MovePos.DecimalPlaces = 1;
            this.Num_MovePos.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Num_MovePos.Location = new System.Drawing.Point(6, 485);
            this.Num_MovePos.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.Num_MovePos.Name = "Num_MovePos";
            this.Num_MovePos.Size = new System.Drawing.Size(141, 35);
            this.Num_MovePos.TabIndex = 7;
            this.Num_MovePos.Value = new decimal(new int[] {
            2000000,
            0,
            0,
            0});
            // 
            // Btn_AlmReset
            // 
            this.Btn_AlmReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Btn_AlmReset.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_AlmReset.ForeColor = System.Drawing.Color.Red;
            this.Btn_AlmReset.Location = new System.Drawing.Point(5, 85);
            this.Btn_AlmReset.Name = "Btn_AlmReset";
            this.Btn_AlmReset.Size = new System.Drawing.Size(193, 50);
            this.Btn_AlmReset.TabIndex = 4;
            this.Btn_AlmReset.Text = "Alarm Reset";
            this.Btn_AlmReset.UseVisualStyleBackColor = false;
            this.Btn_AlmReset.Click += new System.EventHandler(this.Btn_AlmReset_Click);
            // 
            // Btn_Home
            // 
            this.Btn_Home.BackColor = System.Drawing.Color.Blue;
            this.Btn_Home.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Home.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_Home.Location = new System.Drawing.Point(5, 164);
            this.Btn_Home.Name = "Btn_Home";
            this.Btn_Home.Size = new System.Drawing.Size(105, 50);
            this.Btn_Home.TabIndex = 2;
            this.Btn_Home.Text = "Home";
            this.Btn_Home.UseVisualStyleBackColor = false;
            this.Btn_Home.Click += new System.EventHandler(this.Btn_Home_Click);
            // 
            // Btn_Stop
            // 
            this.Btn_Stop.BackColor = System.Drawing.Color.Red;
            this.Btn_Stop.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Btn_Stop.ForeColor = System.Drawing.Color.Yellow;
            this.Btn_Stop.Location = new System.Drawing.Point(5, 629);
            this.Btn_Stop.Name = "Btn_Stop";
            this.Btn_Stop.Size = new System.Drawing.Size(193, 59);
            this.Btn_Stop.TabIndex = 1;
            this.Btn_Stop.Text = "Stop";
            this.Btn_Stop.UseVisualStyleBackColor = false;
            this.Btn_Stop.Click += new System.EventHandler(this.Btn_Stop_Click);
            // 
            // DGV_PLC2PC_Data
            // 
            this.DGV_PLC2PC_Data.AllowUserToAddRows = false;
            this.DGV_PLC2PC_Data.AllowUserToDeleteRows = false;
            this.DGV_PLC2PC_Data.AllowUserToResizeColumns = false;
            this.DGV_PLC2PC_Data.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.DGV_PLC2PC_Data.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.DGV_PLC2PC_Data.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DGV_PLC2PC_Data.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.DGV_PLC2PC_Data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_PLC2PC_Data.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn11});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_PLC2PC_Data.DefaultCellStyle = dataGridViewCellStyle4;
            this.DGV_PLC2PC_Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV_PLC2PC_Data.Location = new System.Drawing.Point(223, 3);
            this.DGV_PLC2PC_Data.Name = "DGV_PLC2PC_Data";
            this.DGV_PLC2PC_Data.ReadOnly = true;
            this.DGV_PLC2PC_Data.RowHeadersVisible = false;
            this.DGV_PLC2PC_Data.RowHeadersWidth = 51;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.DGV_PLC2PC_Data.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.DGV_PLC2PC_Data.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.DGV_PLC2PC_Data.RowTemplate.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.DGV_PLC2PC_Data.RowTemplate.Height = 25;
            this.DGV_PLC2PC_Data.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV_PLC2PC_Data.Size = new System.Drawing.Size(804, 685);
            this.DGV_PLC2PC_Data.TabIndex = 110;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewTextBoxColumn2.HeaderText = "PLC >>> PC";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn11.HeaderText = "Value";
            this.dataGridViewTextBoxColumn11.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            this.dataGridViewTextBoxColumn11.ReadOnly = true;
            // 
            // X_PLC_Tool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1036, 758);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "X_PLC_Tool";
            this.Text = "X PLC Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.X_PLC_Tool_FormClosing);
            this.Load += new System.EventHandler(this.X_PLC_Tool_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Num_JogSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MoveSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Num_MovePos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_PLC2PC_Data)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView DGV_PLC2PC_Data;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button Btn_IncMove;
        private System.Windows.Forms.Button Btn_AbsMove;
        private System.Windows.Forms.Button Btn_JogR;
        private System.Windows.Forms.Button Btn_JogF;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown Num_MoveSpeed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown Num_MovePos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown Num_JogSpeed;
        private System.Windows.Forms.Button Btn_AlmReset;
        private System.Windows.Forms.Button Btn_Home;
        private System.Windows.Forms.Button Btn_Stop;
        private System.Windows.Forms.Button Btn_ServoOff;
        private System.Windows.Forms.Button Btn_ServoOn;
        private System.Windows.Forms.Button Btn_SetMovePos;
        private System.Windows.Forms.Button Btn_SetMoveSpeed;
        private System.Windows.Forms.Button Btn_SetJogSpeed;
    }
}