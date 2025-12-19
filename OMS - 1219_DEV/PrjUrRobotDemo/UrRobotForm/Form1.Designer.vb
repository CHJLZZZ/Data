<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RobotUnitForm
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請勿使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Button_Connect = New System.Windows.Forms.Button()
        Me.Button_DisConnect = New System.Windows.Forms.Button()
        Me.Button_Stop = New System.Windows.Forms.Button()
        Me.gbxTcpPosition = New System.Windows.Forms.GroupBox()
        Me.nudTcpYaw = New System.Windows.Forms.NumericUpDown()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.RadioButton3 = New System.Windows.Forms.RadioButton()
        Me.lblTpRollTitle = New System.Windows.Forms.Label()
        Me.nudTcpPitch = New System.Windows.Forms.NumericUpDown()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.nudTcpRoll = New System.Windows.Forms.NumericUpDown()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.lblTpYawTitle = New System.Windows.Forms.Label()
        Me.Button12 = New System.Windows.Forms.Button()
        Me.lblTpPitchTitle = New System.Windows.Forms.Label()
        Me.Button11 = New System.Windows.Forms.Button()
        Me.gbxMoveAbs = New System.Windows.Forms.GroupBox()
        Me.btnGetTcpPosition = New System.Windows.Forms.Button()
        Me.btnMoveTcpRpyTest = New System.Windows.Forms.Button()
        Me.Button10 = New System.Windows.Forms.Button()
        Me.Button9 = New System.Windows.Forms.Button()
        Me.Button8 = New System.Windows.Forms.Button()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.nudTcpZ = New System.Windows.Forms.NumericUpDown()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.nudTcpY = New System.Windows.Forms.NumericUpDown()
        Me.nudTcpX = New System.Windows.Forms.NumericUpDown()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.lblTpYTitle = New System.Windows.Forms.Label()
        Me.lblTpZTitle = New System.Windows.Forms.Label()
        Me.lblTpXTitle = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnMoveTcpTest = New System.Windows.Forms.Button()
        Me.nudTcpRz = New System.Windows.Forms.NumericUpDown()
        Me.nudTcpRy = New System.Windows.Forms.NumericUpDown()
        Me.nudTcpRx = New System.Windows.Forms.NumericUpDown()
        Me.lblTpRyTitle = New System.Windows.Forms.Label()
        Me.lblTpRzTitle = New System.Windows.Forms.Label()
        Me.lblTpRxTitle = New System.Windows.Forms.Label()
        Me.gbxAnglePosition = New System.Windows.Forms.GroupBox()
        Me.btnGetMotorAngle = New System.Windows.Forms.Button()
        Me.btnMoveAngleTest = New System.Windows.Forms.Button()
        Me.lblApWrist3Title = New System.Windows.Forms.Label()
        Me.lblApWrist2Title = New System.Windows.Forms.Label()
        Me.lblApWrist1Title = New System.Windows.Forms.Label()
        Me.lblApElbowTitle = New System.Windows.Forms.Label()
        Me.lblApShoulderTitle = New System.Windows.Forms.Label()
        Me.lblApBaseTitle = New System.Windows.Forms.Label()
        Me.nudAnglerZ = New System.Windows.Forms.NumericUpDown()
        Me.nudAnglerY = New System.Windows.Forms.NumericUpDown()
        Me.nudAnglerX = New System.Windows.Forms.NumericUpDown()
        Me.nudAngleZ = New System.Windows.Forms.NumericUpDown()
        Me.nudAngleY = New System.Windows.Forms.NumericUpDown()
        Me.nudAngleX = New System.Windows.Forms.NumericUpDown()
        Me.tableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.lblTcpY = New System.Windows.Forms.Label()
        Me.lblTcpZ = New System.Windows.Forms.Label()
        Me.lblTcpX = New System.Windows.Forms.Label()
        Me.lblrZWrist3Title = New System.Windows.Forms.Label()
        Me.lblTcptitle = New System.Windows.Forms.Label()
        Me.lblrYWrist2Title = New System.Windows.Forms.Label()
        Me.lblrXWrist1Title = New System.Windows.Forms.Label()
        Me.lblYShoulderTitle = New System.Windows.Forms.Label()
        Me.lblZElbowTitle = New System.Windows.Forms.Label()
        Me.lblAngletitle = New System.Windows.Forms.Label()
        Me.lblTcpRoll = New System.Windows.Forms.Label()
        Me.lblTcpPitch = New System.Windows.Forms.Label()
        Me.lblTcpYaw = New System.Windows.Forms.Label()
        Me.lblAngleBase = New System.Windows.Forms.Label()
        Me.lblAngleShoulder = New System.Windows.Forms.Label()
        Me.lblAngleElbow = New System.Windows.Forms.Label()
        Me.lblAngleWrist1 = New System.Windows.Forms.Label()
        Me.lblAngleWrist2 = New System.Windows.Forms.Label()
        Me.lblAngleWrist3 = New System.Windows.Forms.Label()
        Me.lblXBaseTitle = New System.Windows.Forms.Label()
        Me.richTextBox1 = New System.Windows.Forms.RichTextBox()
        Me.lblCheckMoveStatusTimeOut = New System.Windows.Forms.Label()
        Me.lblIpAddress = New System.Windows.Forms.Label()
        Me.nudCheckMoveStatusTimeOut = New System.Windows.Forms.NumericUpDown()
        Me.txtIpAddress = New System.Windows.Forms.TextBox()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.txtSpeed = New System.Windows.Forms.TextBox()
        Me.txtAcc = New System.Windows.Forms.TextBox()
        Me.lblSpeedTitle = New System.Windows.Forms.Label()
        Me.lblAccTitle = New System.Windows.Forms.Label()
        Me.gbxTcpRV = New System.Windows.Forms.GroupBox()
        Me.gbxRealtime = New System.Windows.Forms.GroupBox()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column9 = New System.Windows.Forms.DataGridViewButtonColumn()
        Me.GroupBox_Record = New System.Windows.Forms.GroupBox()
        Me.Button_LoadRecord = New System.Windows.Forms.Button()
        Me.Button_SaveRecord = New System.Windows.Forms.Button()
        Me.Button_RemoveUnchecked = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBox_Comment = New System.Windows.Forms.TextBox()
        Me.Button_AddToRecord = New System.Windows.Forms.Button()
        Me.gbxTcpPosition.SuspendLayout()
        CType(Me.nudTcpYaw, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpPitch, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpRoll, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbxMoveAbs.SuspendLayout()
        CType(Me.nudTcpZ, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpX, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpRz, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpRy, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudTcpRx, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbxAnglePosition.SuspendLayout()
        CType(Me.nudAnglerZ, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAnglerY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAnglerX, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAngleZ, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAngleY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudAngleX, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tableLayoutPanel1.SuspendLayout()
        CType(Me.nudCheckMoveStatusTimeOut, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbxTcpRV.SuspendLayout()
        Me.gbxRealtime.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox_Record.SuspendLayout()
        Me.SuspendLayout()
        '
        'Button_Connect
        '
        Me.Button_Connect.Font = New System.Drawing.Font("Arial Narrow", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_Connect.ForeColor = System.Drawing.SystemColors.MenuHighlight
        Me.Button_Connect.Location = New System.Drawing.Point(285, 6)
        Me.Button_Connect.Name = "Button_Connect"
        Me.Button_Connect.Size = New System.Drawing.Size(96, 40)
        Me.Button_Connect.TabIndex = 0
        Me.Button_Connect.Text = "Connect"
        Me.Button_Connect.UseVisualStyleBackColor = True
        Me.Button_Connect.Visible = False
        '
        'Button_DisConnect
        '
        Me.Button_DisConnect.Font = New System.Drawing.Font("Arial Narrow", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_DisConnect.ForeColor = System.Drawing.SystemColors.MenuHighlight
        Me.Button_DisConnect.Location = New System.Drawing.Point(387, 6)
        Me.Button_DisConnect.Name = "Button_DisConnect"
        Me.Button_DisConnect.Size = New System.Drawing.Size(96, 40)
        Me.Button_DisConnect.TabIndex = 1
        Me.Button_DisConnect.Text = "Disconnect"
        Me.Button_DisConnect.UseVisualStyleBackColor = True
        Me.Button_DisConnect.Visible = False
        '
        'Button_Stop
        '
        Me.Button_Stop.BackColor = System.Drawing.Color.OrangeRed
        Me.Button_Stop.Font = New System.Drawing.Font("Arial Narrow", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button_Stop.ForeColor = System.Drawing.SystemColors.Window
        Me.Button_Stop.Location = New System.Drawing.Point(387, 52)
        Me.Button_Stop.Name = "Button_Stop"
        Me.Button_Stop.Size = New System.Drawing.Size(96, 40)
        Me.Button_Stop.TabIndex = 2
        Me.Button_Stop.Text = "STOP"
        Me.Button_Stop.UseVisualStyleBackColor = False
        '
        'gbxTcpPosition
        '
        Me.gbxTcpPosition.Controls.Add(Me.nudTcpYaw)
        Me.gbxTcpPosition.Controls.Add(Me.Label1)
        Me.gbxTcpPosition.Controls.Add(Me.RadioButton3)
        Me.gbxTcpPosition.Controls.Add(Me.lblTpRollTitle)
        Me.gbxTcpPosition.Controls.Add(Me.nudTcpPitch)
        Me.gbxTcpPosition.Controls.Add(Me.RadioButton2)
        Me.gbxTcpPosition.Controls.Add(Me.nudTcpRoll)
        Me.gbxTcpPosition.Controls.Add(Me.RadioButton1)
        Me.gbxTcpPosition.Controls.Add(Me.lblTpYawTitle)
        Me.gbxTcpPosition.Controls.Add(Me.Button12)
        Me.gbxTcpPosition.Controls.Add(Me.lblTpPitchTitle)
        Me.gbxTcpPosition.Controls.Add(Me.Button11)
        Me.gbxTcpPosition.Controls.Add(Me.gbxMoveAbs)
        Me.gbxTcpPosition.Controls.Add(Me.Button10)
        Me.gbxTcpPosition.Controls.Add(Me.Button9)
        Me.gbxTcpPosition.Controls.Add(Me.Button8)
        Me.gbxTcpPosition.Controls.Add(Me.Button7)
        Me.gbxTcpPosition.Controls.Add(Me.nudTcpZ)
        Me.gbxTcpPosition.Controls.Add(Me.Button6)
        Me.gbxTcpPosition.Controls.Add(Me.Button5)
        Me.gbxTcpPosition.Controls.Add(Me.nudTcpY)
        Me.gbxTcpPosition.Controls.Add(Me.nudTcpX)
        Me.gbxTcpPosition.Controls.Add(Me.Button4)
        Me.gbxTcpPosition.Controls.Add(Me.Button3)
        Me.gbxTcpPosition.Controls.Add(Me.Button2)
        Me.gbxTcpPosition.Controls.Add(Me.Button1)
        Me.gbxTcpPosition.Controls.Add(Me.lblTpYTitle)
        Me.gbxTcpPosition.Controls.Add(Me.lblTpZTitle)
        Me.gbxTcpPosition.Controls.Add(Me.lblTpXTitle)
        Me.gbxTcpPosition.Controls.Add(Me.Label3)
        Me.gbxTcpPosition.Font = New System.Drawing.Font("新細明體", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.gbxTcpPosition.Location = New System.Drawing.Point(29, 98)
        Me.gbxTcpPosition.Name = "gbxTcpPosition"
        Me.gbxTcpPosition.Size = New System.Drawing.Size(324, 391)
        Me.gbxTcpPosition.TabIndex = 46
        Me.gbxTcpPosition.TabStop = False
        Me.gbxTcpPosition.Text = "TCP Position"
        '
        'nudTcpYaw
        '
        Me.nudTcpYaw.DecimalPlaces = 4
        Me.nudTcpYaw.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpYaw.Location = New System.Drawing.Point(143, 257)
        Me.nudTcpYaw.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudTcpYaw.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudTcpYaw.Name = "nudTcpYaw"
        Me.nudTcpYaw.Size = New System.Drawing.Size(90, 27)
        Me.nudTcpYaw.TabIndex = 66
        Me.nudTcpYaw.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(19, 299)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(69, 16)
        Me.Label1.TabIndex = 42
        Me.Label1.Text = "Jog Step"
        '
        'RadioButton3
        '
        Me.RadioButton3.AutoSize = True
        Me.RadioButton3.Location = New System.Drawing.Point(250, 297)
        Me.RadioButton3.Name = "RadioButton3"
        Me.RadioButton3.Size = New System.Drawing.Size(55, 20)
        Me.RadioButton3.TabIndex = 87
        Me.RadioButton3.TabStop = True
        Me.RadioButton3.Text = "Fast"
        Me.RadioButton3.UseVisualStyleBackColor = True
        '
        'lblTpRollTitle
        '
        Me.lblTpRollTitle.AutoSize = True
        Me.lblTpRollTitle.Location = New System.Drawing.Point(103, 174)
        Me.lblTpRollTitle.Name = "lblTpRollTitle"
        Me.lblTpRollTitle.Size = New System.Drawing.Size(38, 16)
        Me.lblTpRollTitle.TabIndex = 61
        Me.lblTpRollTitle.Text = "Roll"
        '
        'nudTcpPitch
        '
        Me.nudTcpPitch.DecimalPlaces = 4
        Me.nudTcpPitch.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpPitch.Location = New System.Drawing.Point(141, 215)
        Me.nudTcpPitch.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudTcpPitch.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudTcpPitch.Name = "nudTcpPitch"
        Me.nudTcpPitch.Size = New System.Drawing.Size(90, 27)
        Me.nudTcpPitch.TabIndex = 65
        Me.nudTcpPitch.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Location = New System.Drawing.Point(166, 297)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(84, 20)
        Me.RadioButton2.TabIndex = 86
        Me.RadioButton2.TabStop = True
        Me.RadioButton2.Text = "Medium"
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'nudTcpRoll
        '
        Me.nudTcpRoll.DecimalPlaces = 4
        Me.nudTcpRoll.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpRoll.Location = New System.Drawing.Point(143, 171)
        Me.nudTcpRoll.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudTcpRoll.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudTcpRoll.Name = "nudTcpRoll"
        Me.nudTcpRoll.Size = New System.Drawing.Size(90, 27)
        Me.nudTcpRoll.TabIndex = 64
        Me.nudTcpRoll.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Checked = True
        Me.RadioButton1.Location = New System.Drawing.Point(106, 297)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(61, 20)
        Me.RadioButton1.TabIndex = 85
        Me.RadioButton1.TabStop = True
        Me.RadioButton1.Text = "Slow"
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'lblTpYawTitle
        '
        Me.lblTpYawTitle.AutoSize = True
        Me.lblTpYawTitle.Location = New System.Drawing.Point(102, 259)
        Me.lblTpYawTitle.Name = "lblTpYawTitle"
        Me.lblTpYawTitle.Size = New System.Drawing.Size(40, 16)
        Me.lblTpYawTitle.TabIndex = 62
        Me.lblTpYawTitle.Text = "Yaw"
        '
        'Button12
        '
        Me.Button12.BackColor = System.Drawing.Color.DarkSeaGreen
        Me.Button12.Location = New System.Drawing.Point(237, 245)
        Me.Button12.Name = "Button12"
        Me.Button12.Size = New System.Drawing.Size(55, 45)
        Me.Button12.TabIndex = 84
        Me.Button12.Text = "+"
        Me.Button12.UseVisualStyleBackColor = False
        '
        'lblTpPitchTitle
        '
        Me.lblTpPitchTitle.AutoSize = True
        Me.lblTpPitchTitle.Location = New System.Drawing.Point(101, 217)
        Me.lblTpPitchTitle.Name = "lblTpPitchTitle"
        Me.lblTpPitchTitle.Size = New System.Drawing.Size(44, 16)
        Me.lblTpPitchTitle.TabIndex = 63
        Me.lblTpPitchTitle.Text = "Pitch"
        '
        'Button11
        '
        Me.Button11.BackColor = System.Drawing.Color.DarkSeaGreen
        Me.Button11.Location = New System.Drawing.Point(47, 245)
        Me.Button11.Name = "Button11"
        Me.Button11.Size = New System.Drawing.Size(55, 45)
        Me.Button11.TabIndex = 83
        Me.Button11.Text = "-"
        Me.Button11.UseVisualStyleBackColor = False
        '
        'gbxMoveAbs
        '
        Me.gbxMoveAbs.Controls.Add(Me.btnGetTcpPosition)
        Me.gbxMoveAbs.Controls.Add(Me.btnMoveTcpRpyTest)
        Me.gbxMoveAbs.Font = New System.Drawing.Font("新細明體", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.gbxMoveAbs.ForeColor = System.Drawing.Color.DarkGreen
        Me.gbxMoveAbs.Location = New System.Drawing.Point(0, 322)
        Me.gbxMoveAbs.Name = "gbxMoveAbs"
        Me.gbxMoveAbs.Size = New System.Drawing.Size(322, 69)
        Me.gbxMoveAbs.TabIndex = 72
        Me.gbxMoveAbs.TabStop = False
        Me.gbxMoveAbs.Text = "Move Absolutely Position"
        '
        'btnGetTcpPosition
        '
        Me.btnGetTcpPosition.Location = New System.Drawing.Point(6, 22)
        Me.btnGetTcpPosition.Name = "btnGetTcpPosition"
        Me.btnGetTcpPosition.Size = New System.Drawing.Size(140, 40)
        Me.btnGetTcpPosition.TabIndex = 41
        Me.btnGetTcpPosition.Text = "Get TCP Pos"
        Me.btnGetTcpPosition.UseVisualStyleBackColor = True
        '
        'btnMoveTcpRpyTest
        '
        Me.btnMoveTcpRpyTest.Location = New System.Drawing.Point(166, 22)
        Me.btnMoveTcpRpyTest.Name = "btnMoveTcpRpyTest"
        Me.btnMoveTcpRpyTest.Size = New System.Drawing.Size(140, 40)
        Me.btnMoveTcpRpyTest.TabIndex = 60
        Me.btnMoveTcpRpyTest.Text = "Move To Pos (rpy)"
        Me.btnMoveTcpRpyTest.UseVisualStyleBackColor = True
        '
        'Button10
        '
        Me.Button10.BackColor = System.Drawing.Color.Thistle
        Me.Button10.Location = New System.Drawing.Point(237, 203)
        Me.Button10.Name = "Button10"
        Me.Button10.Size = New System.Drawing.Size(55, 45)
        Me.Button10.TabIndex = 82
        Me.Button10.Text = "+"
        Me.Button10.UseVisualStyleBackColor = False
        '
        'Button9
        '
        Me.Button9.BackColor = System.Drawing.Color.Thistle
        Me.Button9.Location = New System.Drawing.Point(47, 203)
        Me.Button9.Name = "Button9"
        Me.Button9.Size = New System.Drawing.Size(55, 45)
        Me.Button9.TabIndex = 81
        Me.Button9.Text = "-"
        Me.Button9.UseVisualStyleBackColor = False
        '
        'Button8
        '
        Me.Button8.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.Button8.Location = New System.Drawing.Point(237, 160)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(55, 45)
        Me.Button8.TabIndex = 80
        Me.Button8.Text = "+"
        Me.Button8.UseVisualStyleBackColor = False
        '
        'Button7
        '
        Me.Button7.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.Button7.Location = New System.Drawing.Point(47, 160)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(55, 45)
        Me.Button7.TabIndex = 79
        Me.Button7.Text = "-"
        Me.Button7.UseVisualStyleBackColor = False
        '
        'nudTcpZ
        '
        Me.nudTcpZ.DecimalPlaces = 4
        Me.nudTcpZ.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpZ.Location = New System.Drawing.Point(143, 115)
        Me.nudTcpZ.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.nudTcpZ.Minimum = New Decimal(New Integer() {99999, 0, 0, -2147483648})
        Me.nudTcpZ.Name = "nudTcpZ"
        Me.nudTcpZ.Size = New System.Drawing.Size(90, 27)
        Me.nudTcpZ.TabIndex = 14
        Me.nudTcpZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Button6
        '
        Me.Button6.BackColor = System.Drawing.Color.DarkSeaGreen
        Me.Button6.Location = New System.Drawing.Point(237, 103)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(55, 45)
        Me.Button6.TabIndex = 78
        Me.Button6.Text = "+"
        Me.Button6.UseVisualStyleBackColor = False
        '
        'Button5
        '
        Me.Button5.BackColor = System.Drawing.Color.DarkSeaGreen
        Me.Button5.Location = New System.Drawing.Point(47, 106)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(55, 45)
        Me.Button5.TabIndex = 77
        Me.Button5.Text = "-"
        Me.Button5.UseVisualStyleBackColor = False
        '
        'nudTcpY
        '
        Me.nudTcpY.DecimalPlaces = 4
        Me.nudTcpY.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpY.Location = New System.Drawing.Point(143, 73)
        Me.nudTcpY.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.nudTcpY.Minimum = New Decimal(New Integer() {99999, 0, 0, -2147483648})
        Me.nudTcpY.Name = "nudTcpY"
        Me.nudTcpY.Size = New System.Drawing.Size(90, 27)
        Me.nudTcpY.TabIndex = 13
        Me.nudTcpY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudTcpX
        '
        Me.nudTcpX.DecimalPlaces = 4
        Me.nudTcpX.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpX.Location = New System.Drawing.Point(143, 30)
        Me.nudTcpX.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.nudTcpX.Minimum = New Decimal(New Integer() {99999, 0, 0, -2147483648})
        Me.nudTcpX.Name = "nudTcpX"
        Me.nudTcpX.Size = New System.Drawing.Size(90, 27)
        Me.nudTcpX.TabIndex = 12
        Me.nudTcpX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Button4
        '
        Me.Button4.BackColor = System.Drawing.Color.Thistle
        Me.Button4.Location = New System.Drawing.Point(237, 61)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(55, 45)
        Me.Button4.TabIndex = 76
        Me.Button4.Text = "+"
        Me.Button4.UseVisualStyleBackColor = False
        '
        'Button3
        '
        Me.Button3.BackColor = System.Drawing.Color.Thistle
        Me.Button3.Location = New System.Drawing.Point(47, 64)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(55, 45)
        Me.Button3.TabIndex = 75
        Me.Button3.Text = "-"
        Me.Button3.UseVisualStyleBackColor = False
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.Button2.Location = New System.Drawing.Point(237, 18)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(55, 45)
        Me.Button2.TabIndex = 74
        Me.Button2.Text = "+"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.SystemColors.ActiveCaption
        Me.Button1.Location = New System.Drawing.Point(47, 21)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(55, 45)
        Me.Button1.TabIndex = 73
        Me.Button1.Text = "-"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'lblTpYTitle
        '
        Me.lblTpYTitle.AutoSize = True
        Me.lblTpYTitle.Location = New System.Drawing.Point(108, 80)
        Me.lblTpYTitle.Name = "lblTpYTitle"
        Me.lblTpYTitle.Size = New System.Drawing.Size(20, 16)
        Me.lblTpYTitle.TabIndex = 9
        Me.lblTpYTitle.Text = "Y"
        '
        'lblTpZTitle
        '
        Me.lblTpZTitle.AutoSize = True
        Me.lblTpZTitle.Location = New System.Drawing.Point(108, 118)
        Me.lblTpZTitle.Name = "lblTpZTitle"
        Me.lblTpZTitle.Size = New System.Drawing.Size(18, 16)
        Me.lblTpZTitle.TabIndex = 7
        Me.lblTpZTitle.Text = "Z"
        '
        'lblTpXTitle
        '
        Me.lblTpXTitle.AutoSize = True
        Me.lblTpXTitle.Location = New System.Drawing.Point(108, 37)
        Me.lblTpXTitle.Name = "lblTpXTitle"
        Me.lblTpXTitle.Size = New System.Drawing.Size(20, 16)
        Me.lblTpXTitle.TabIndex = 6
        Me.lblTpXTitle.Text = "X"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Verdana", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.SystemColors.MenuHighlight
        Me.Label3.Location = New System.Drawing.Point(152, 154)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(61, 16)
        Me.Label3.TabIndex = 74
        Me.Label3.Text = "Degree°"
        '
        'btnMoveTcpTest
        '
        Me.btnMoveTcpTest.Enabled = False
        Me.btnMoveTcpTest.Location = New System.Drawing.Point(40, 133)
        Me.btnMoveTcpTest.Name = "btnMoveTcpTest"
        Me.btnMoveTcpTest.Size = New System.Drawing.Size(140, 40)
        Me.btnMoveTcpTest.TabIndex = 18
        Me.btnMoveTcpTest.Text = "Move To Pos (rv)"
        Me.btnMoveTcpTest.UseVisualStyleBackColor = True
        '
        'nudTcpRz
        '
        Me.nudTcpRz.DecimalPlaces = 4
        Me.nudTcpRz.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpRz.Location = New System.Drawing.Point(118, 103)
        Me.nudTcpRz.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudTcpRz.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudTcpRz.Name = "nudTcpRz"
        Me.nudTcpRz.Size = New System.Drawing.Size(90, 22)
        Me.nudTcpRz.TabIndex = 17
        Me.nudTcpRz.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudTcpRy
        '
        Me.nudTcpRy.DecimalPlaces = 4
        Me.nudTcpRy.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpRy.Location = New System.Drawing.Point(118, 62)
        Me.nudTcpRy.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudTcpRy.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudTcpRy.Name = "nudTcpRy"
        Me.nudTcpRy.Size = New System.Drawing.Size(90, 22)
        Me.nudTcpRy.TabIndex = 16
        Me.nudTcpRy.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudTcpRx
        '
        Me.nudTcpRx.DecimalPlaces = 4
        Me.nudTcpRx.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudTcpRx.Location = New System.Drawing.Point(118, 21)
        Me.nudTcpRx.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudTcpRx.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudTcpRx.Name = "nudTcpRx"
        Me.nudTcpRx.Size = New System.Drawing.Size(90, 22)
        Me.nudTcpRx.TabIndex = 15
        Me.nudTcpRx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblTpRyTitle
        '
        Me.lblTpRyTitle.AutoSize = True
        Me.lblTpRyTitle.Location = New System.Drawing.Point(90, 64)
        Me.lblTpRyTitle.Name = "lblTpRyTitle"
        Me.lblTpRyTitle.Size = New System.Drawing.Size(17, 12)
        Me.lblTpRyTitle.TabIndex = 11
        Me.lblTpRyTitle.Text = "rY"
        '
        'lblTpRzTitle
        '
        Me.lblTpRzTitle.AutoSize = True
        Me.lblTpRzTitle.Location = New System.Drawing.Point(90, 105)
        Me.lblTpRzTitle.Name = "lblTpRzTitle"
        Me.lblTpRzTitle.Size = New System.Drawing.Size(16, 12)
        Me.lblTpRzTitle.TabIndex = 10
        Me.lblTpRzTitle.Text = "rZ"
        '
        'lblTpRxTitle
        '
        Me.lblTpRxTitle.AutoSize = True
        Me.lblTpRxTitle.Location = New System.Drawing.Point(90, 23)
        Me.lblTpRxTitle.Name = "lblTpRxTitle"
        Me.lblTpRxTitle.Size = New System.Drawing.Size(17, 12)
        Me.lblTpRxTitle.TabIndex = 8
        Me.lblTpRxTitle.Text = "rX"
        '
        'gbxAnglePosition
        '
        Me.gbxAnglePosition.Controls.Add(Me.btnGetMotorAngle)
        Me.gbxAnglePosition.Controls.Add(Me.btnMoveAngleTest)
        Me.gbxAnglePosition.Controls.Add(Me.lblApWrist3Title)
        Me.gbxAnglePosition.Controls.Add(Me.lblApWrist2Title)
        Me.gbxAnglePosition.Controls.Add(Me.lblApWrist1Title)
        Me.gbxAnglePosition.Controls.Add(Me.lblApElbowTitle)
        Me.gbxAnglePosition.Controls.Add(Me.lblApShoulderTitle)
        Me.gbxAnglePosition.Controls.Add(Me.lblApBaseTitle)
        Me.gbxAnglePosition.Controls.Add(Me.nudAnglerZ)
        Me.gbxAnglePosition.Controls.Add(Me.nudAnglerY)
        Me.gbxAnglePosition.Controls.Add(Me.nudAnglerX)
        Me.gbxAnglePosition.Controls.Add(Me.nudAngleZ)
        Me.gbxAnglePosition.Controls.Add(Me.nudAngleY)
        Me.gbxAnglePosition.Controls.Add(Me.nudAngleX)
        Me.gbxAnglePosition.Location = New System.Drawing.Point(991, 200)
        Me.gbxAnglePosition.Name = "gbxAnglePosition"
        Me.gbxAnglePosition.Size = New System.Drawing.Size(188, 292)
        Me.gbxAnglePosition.TabIndex = 47
        Me.gbxAnglePosition.TabStop = False
        Me.gbxAnglePosition.Text = "Angle Position"
        '
        'btnGetMotorAngle
        '
        Me.btnGetMotorAngle.Enabled = False
        Me.btnGetMotorAngle.Location = New System.Drawing.Point(9, 195)
        Me.btnGetMotorAngle.Name = "btnGetMotorAngle"
        Me.btnGetMotorAngle.Size = New System.Drawing.Size(153, 40)
        Me.btnGetMotorAngle.TabIndex = 41
        Me.btnGetMotorAngle.Text = "Get Motor Angle"
        Me.btnGetMotorAngle.UseVisualStyleBackColor = True
        '
        'btnMoveAngleTest
        '
        Me.btnMoveAngleTest.Enabled = False
        Me.btnMoveAngleTest.Location = New System.Drawing.Point(9, 239)
        Me.btnMoveAngleTest.Name = "btnMoveAngleTest"
        Me.btnMoveAngleTest.Size = New System.Drawing.Size(153, 40)
        Me.btnMoveAngleTest.TabIndex = 40
        Me.btnMoveAngleTest.Text = "Move To Angle"
        Me.btnMoveAngleTest.UseVisualStyleBackColor = True
        '
        'lblApWrist3Title
        '
        Me.lblApWrist3Title.AutoSize = True
        Me.lblApWrist3Title.Location = New System.Drawing.Point(16, 167)
        Me.lblApWrist3Title.Name = "lblApWrist3Title"
        Me.lblApWrist3Title.Size = New System.Drawing.Size(39, 12)
        Me.lblApWrist3Title.TabIndex = 39
        Me.lblApWrist3Title.Text = "Wrist 3"
        '
        'lblApWrist2Title
        '
        Me.lblApWrist2Title.AutoSize = True
        Me.lblApWrist2Title.Location = New System.Drawing.Point(16, 136)
        Me.lblApWrist2Title.Name = "lblApWrist2Title"
        Me.lblApWrist2Title.Size = New System.Drawing.Size(39, 12)
        Me.lblApWrist2Title.TabIndex = 38
        Me.lblApWrist2Title.Text = "Wrist 2"
        '
        'lblApWrist1Title
        '
        Me.lblApWrist1Title.AutoSize = True
        Me.lblApWrist1Title.Location = New System.Drawing.Point(16, 108)
        Me.lblApWrist1Title.Name = "lblApWrist1Title"
        Me.lblApWrist1Title.Size = New System.Drawing.Size(39, 12)
        Me.lblApWrist1Title.TabIndex = 37
        Me.lblApWrist1Title.Text = "Wrist 1"
        '
        'lblApElbowTitle
        '
        Me.lblApElbowTitle.AutoSize = True
        Me.lblApElbowTitle.Location = New System.Drawing.Point(24, 80)
        Me.lblApElbowTitle.Name = "lblApElbowTitle"
        Me.lblApElbowTitle.Size = New System.Drawing.Size(35, 12)
        Me.lblApElbowTitle.TabIndex = 36
        Me.lblApElbowTitle.Text = "Elbow"
        '
        'lblApShoulderTitle
        '
        Me.lblApShoulderTitle.AutoSize = True
        Me.lblApShoulderTitle.Location = New System.Drawing.Point(7, 52)
        Me.lblApShoulderTitle.Name = "lblApShoulderTitle"
        Me.lblApShoulderTitle.Size = New System.Drawing.Size(47, 12)
        Me.lblApShoulderTitle.TabIndex = 35
        Me.lblApShoulderTitle.Text = "Shoulder"
        '
        'lblApBaseTitle
        '
        Me.lblApBaseTitle.AutoSize = True
        Me.lblApBaseTitle.Location = New System.Drawing.Point(31, 24)
        Me.lblApBaseTitle.Name = "lblApBaseTitle"
        Me.lblApBaseTitle.Size = New System.Drawing.Size(27, 12)
        Me.lblApBaseTitle.TabIndex = 34
        Me.lblApBaseTitle.Text = "Base"
        '
        'nudAnglerZ
        '
        Me.nudAnglerZ.DecimalPlaces = 4
        Me.nudAnglerZ.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudAnglerZ.Location = New System.Drawing.Point(78, 165)
        Me.nudAnglerZ.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudAnglerZ.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudAnglerZ.Name = "nudAnglerZ"
        Me.nudAnglerZ.Size = New System.Drawing.Size(90, 22)
        Me.nudAnglerZ.TabIndex = 17
        Me.nudAnglerZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudAnglerY
        '
        Me.nudAnglerY.DecimalPlaces = 4
        Me.nudAnglerY.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudAnglerY.Location = New System.Drawing.Point(78, 136)
        Me.nudAnglerY.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudAnglerY.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudAnglerY.Name = "nudAnglerY"
        Me.nudAnglerY.Size = New System.Drawing.Size(90, 22)
        Me.nudAnglerY.TabIndex = 16
        Me.nudAnglerY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudAnglerX
        '
        Me.nudAnglerX.DecimalPlaces = 4
        Me.nudAnglerX.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudAnglerX.Location = New System.Drawing.Point(78, 107)
        Me.nudAnglerX.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudAnglerX.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudAnglerX.Name = "nudAnglerX"
        Me.nudAnglerX.Size = New System.Drawing.Size(90, 22)
        Me.nudAnglerX.TabIndex = 15
        Me.nudAnglerX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudAngleZ
        '
        Me.nudAngleZ.DecimalPlaces = 4
        Me.nudAngleZ.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudAngleZ.Location = New System.Drawing.Point(78, 78)
        Me.nudAngleZ.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudAngleZ.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudAngleZ.Name = "nudAngleZ"
        Me.nudAngleZ.Size = New System.Drawing.Size(90, 22)
        Me.nudAngleZ.TabIndex = 14
        Me.nudAngleZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudAngleY
        '
        Me.nudAngleY.DecimalPlaces = 4
        Me.nudAngleY.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudAngleY.Location = New System.Drawing.Point(78, 50)
        Me.nudAngleY.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudAngleY.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudAngleY.Name = "nudAngleY"
        Me.nudAngleY.Size = New System.Drawing.Size(90, 22)
        Me.nudAngleY.TabIndex = 13
        Me.nudAngleY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'nudAngleX
        '
        Me.nudAngleX.DecimalPlaces = 4
        Me.nudAngleX.Increment = New Decimal(New Integer() {1, 0, 0, 131072})
        Me.nudAngleX.Location = New System.Drawing.Point(78, 22)
        Me.nudAngleX.Maximum = New Decimal(New Integer() {360, 0, 0, 0})
        Me.nudAngleX.Minimum = New Decimal(New Integer() {360, 0, 0, -2147483648})
        Me.nudAngleX.Name = "nudAngleX"
        Me.nudAngleX.Size = New System.Drawing.Size(90, 22)
        Me.nudAngleX.TabIndex = 12
        Me.nudAngleX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'tableLayoutPanel1
        '
        Me.tableLayoutPanel1.BackColor = System.Drawing.Color.Silver
        Me.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.[Single]
        Me.tableLayoutPanel1.ColumnCount = 3
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.tableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcpY, 1, 2)
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcpZ, 1, 3)
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcpX, 1, 1)
        Me.tableLayoutPanel1.Controls.Add(Me.lblrZWrist3Title, 0, 6)
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcptitle, 1, 0)
        Me.tableLayoutPanel1.Controls.Add(Me.lblrYWrist2Title, 0, 5)
        Me.tableLayoutPanel1.Controls.Add(Me.lblrXWrist1Title, 0, 4)
        Me.tableLayoutPanel1.Controls.Add(Me.lblYShoulderTitle, 0, 2)
        Me.tableLayoutPanel1.Controls.Add(Me.lblZElbowTitle, 0, 3)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngletitle, 2, 0)
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcpRoll, 1, 4)
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcpPitch, 1, 5)
        Me.tableLayoutPanel1.Controls.Add(Me.lblTcpYaw, 1, 6)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngleBase, 2, 1)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngleShoulder, 2, 2)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngleElbow, 2, 3)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngleWrist1, 2, 4)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngleWrist2, 2, 5)
        Me.tableLayoutPanel1.Controls.Add(Me.lblAngleWrist3, 2, 6)
        Me.tableLayoutPanel1.Controls.Add(Me.lblXBaseTitle, 0, 1)
        Me.tableLayoutPanel1.Location = New System.Drawing.Point(12, 27)
        Me.tableLayoutPanel1.Name = "tableLayoutPanel1"
        Me.tableLayoutPanel1.RowCount = 7
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571!))
        Me.tableLayoutPanel1.Size = New System.Drawing.Size(470, 131)
        Me.tableLayoutPanel1.TabIndex = 59
        '
        'lblTcpY
        '
        Me.lblTcpY.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcpY.AutoSize = True
        Me.lblTcpY.Location = New System.Drawing.Point(227, 39)
        Me.lblTcpY.Name = "lblTcpY"
        Me.lblTcpY.Size = New System.Drawing.Size(14, 13)
        Me.lblTcpY.TabIndex = 61
        Me.lblTcpY.Text = "0"
        '
        'lblTcpZ
        '
        Me.lblTcpZ.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcpZ.AutoSize = True
        Me.lblTcpZ.Location = New System.Drawing.Point(227, 57)
        Me.lblTcpZ.Name = "lblTcpZ"
        Me.lblTcpZ.Size = New System.Drawing.Size(14, 13)
        Me.lblTcpZ.TabIndex = 62
        Me.lblTcpZ.Text = "0"
        '
        'lblTcpX
        '
        Me.lblTcpX.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcpX.AutoSize = True
        Me.lblTcpX.Location = New System.Drawing.Point(227, 21)
        Me.lblTcpX.Name = "lblTcpX"
        Me.lblTcpX.Size = New System.Drawing.Size(14, 13)
        Me.lblTcpX.TabIndex = 60
        Me.lblTcpX.Text = "0"
        '
        'lblrZWrist3Title
        '
        Me.lblrZWrist3Title.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblrZWrist3Title.AutoSize = True
        Me.lblrZWrist3Title.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblrZWrist3Title.Location = New System.Drawing.Point(35, 113)
        Me.lblrZWrist3Title.Name = "lblrZWrist3Title"
        Me.lblrZWrist3Title.Size = New System.Drawing.Size(87, 13)
        Me.lblrZWrist3Title.TabIndex = 64
        Me.lblrZWrist3Title.Text = "Yaw | Wrist 3"
        '
        'lblTcptitle
        '
        Me.lblTcptitle.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcptitle.AutoSize = True
        Me.lblTcptitle.Location = New System.Drawing.Point(196, 3)
        Me.lblTcptitle.Name = "lblTcptitle"
        Me.lblTcptitle.Size = New System.Drawing.Size(76, 13)
        Me.lblTcptitle.TabIndex = 65
        Me.lblTcptitle.Text = "TCP(mm, °)"
        '
        'lblrYWrist2Title
        '
        Me.lblrYWrist2Title.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblrYWrist2Title.AutoSize = True
        Me.lblrYWrist2Title.Location = New System.Drawing.Point(33, 93)
        Me.lblrYWrist2Title.Name = "lblrYWrist2Title"
        Me.lblrYWrist2Title.Size = New System.Drawing.Size(90, 13)
        Me.lblrYWrist2Title.TabIndex = 63
        Me.lblrYWrist2Title.Text = "Pitch | Wrist 2"
        '
        'lblrXWrist1Title
        '
        Me.lblrXWrist1Title.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblrXWrist1Title.AutoSize = True
        Me.lblrXWrist1Title.Location = New System.Drawing.Point(36, 75)
        Me.lblrXWrist1Title.Name = "lblrXWrist1Title"
        Me.lblrXWrist1Title.Size = New System.Drawing.Size(85, 13)
        Me.lblrXWrist1Title.TabIndex = 62
        Me.lblrXWrist1Title.Text = "Roll | Wrist 1"
        '
        'lblYShoulderTitle
        '
        Me.lblYShoulderTitle.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblYShoulderTitle.AutoSize = True
        Me.lblYShoulderTitle.Location = New System.Drawing.Point(39, 39)
        Me.lblYShoulderTitle.Name = "lblYShoulderTitle"
        Me.lblYShoulderTitle.Size = New System.Drawing.Size(79, 13)
        Me.lblYShoulderTitle.TabIndex = 60
        Me.lblYShoulderTitle.Text = "Y | Shoulder"
        '
        'lblZElbowTitle
        '
        Me.lblZElbowTitle.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblZElbowTitle.AutoSize = True
        Me.lblZElbowTitle.Location = New System.Drawing.Point(47, 57)
        Me.lblZElbowTitle.Name = "lblZElbowTitle"
        Me.lblZElbowTitle.Size = New System.Drawing.Size(62, 13)
        Me.lblZElbowTitle.TabIndex = 61
        Me.lblZElbowTitle.Text = "Z | Elbow"
        '
        'lblAngletitle
        '
        Me.lblAngletitle.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngletitle.AutoSize = True
        Me.lblAngletitle.Location = New System.Drawing.Point(363, 3)
        Me.lblAngletitle.Name = "lblAngletitle"
        Me.lblAngletitle.Size = New System.Drawing.Size(55, 13)
        Me.lblAngletitle.TabIndex = 66
        Me.lblAngletitle.Text = "Angle(°)"
        '
        'lblTcpRoll
        '
        Me.lblTcpRoll.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcpRoll.AutoSize = True
        Me.lblTcpRoll.Location = New System.Drawing.Point(227, 75)
        Me.lblTcpRoll.Name = "lblTcpRoll"
        Me.lblTcpRoll.Size = New System.Drawing.Size(14, 13)
        Me.lblTcpRoll.TabIndex = 67
        Me.lblTcpRoll.Text = "0"
        '
        'lblTcpPitch
        '
        Me.lblTcpPitch.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcpPitch.AutoSize = True
        Me.lblTcpPitch.Location = New System.Drawing.Point(227, 93)
        Me.lblTcpPitch.Name = "lblTcpPitch"
        Me.lblTcpPitch.Size = New System.Drawing.Size(14, 13)
        Me.lblTcpPitch.TabIndex = 68
        Me.lblTcpPitch.Text = "0"
        '
        'lblTcpYaw
        '
        Me.lblTcpYaw.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblTcpYaw.AutoSize = True
        Me.lblTcpYaw.Location = New System.Drawing.Point(227, 113)
        Me.lblTcpYaw.Name = "lblTcpYaw"
        Me.lblTcpYaw.Size = New System.Drawing.Size(14, 13)
        Me.lblTcpYaw.TabIndex = 69
        Me.lblTcpYaw.Text = "0"
        '
        'lblAngleBase
        '
        Me.lblAngleBase.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngleBase.AutoSize = True
        Me.lblAngleBase.Location = New System.Drawing.Point(384, 21)
        Me.lblAngleBase.Name = "lblAngleBase"
        Me.lblAngleBase.Size = New System.Drawing.Size(14, 13)
        Me.lblAngleBase.TabIndex = 70
        Me.lblAngleBase.Text = "0"
        '
        'lblAngleShoulder
        '
        Me.lblAngleShoulder.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngleShoulder.AutoSize = True
        Me.lblAngleShoulder.Location = New System.Drawing.Point(384, 39)
        Me.lblAngleShoulder.Name = "lblAngleShoulder"
        Me.lblAngleShoulder.Size = New System.Drawing.Size(14, 13)
        Me.lblAngleShoulder.TabIndex = 71
        Me.lblAngleShoulder.Text = "0"
        '
        'lblAngleElbow
        '
        Me.lblAngleElbow.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngleElbow.AutoSize = True
        Me.lblAngleElbow.Location = New System.Drawing.Point(384, 57)
        Me.lblAngleElbow.Name = "lblAngleElbow"
        Me.lblAngleElbow.Size = New System.Drawing.Size(14, 13)
        Me.lblAngleElbow.TabIndex = 72
        Me.lblAngleElbow.Text = "0"
        '
        'lblAngleWrist1
        '
        Me.lblAngleWrist1.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngleWrist1.AutoSize = True
        Me.lblAngleWrist1.Location = New System.Drawing.Point(384, 75)
        Me.lblAngleWrist1.Name = "lblAngleWrist1"
        Me.lblAngleWrist1.Size = New System.Drawing.Size(14, 13)
        Me.lblAngleWrist1.TabIndex = 73
        Me.lblAngleWrist1.Text = "0"
        '
        'lblAngleWrist2
        '
        Me.lblAngleWrist2.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngleWrist2.AutoSize = True
        Me.lblAngleWrist2.Location = New System.Drawing.Point(384, 93)
        Me.lblAngleWrist2.Name = "lblAngleWrist2"
        Me.lblAngleWrist2.Size = New System.Drawing.Size(14, 13)
        Me.lblAngleWrist2.TabIndex = 74
        Me.lblAngleWrist2.Text = "0"
        '
        'lblAngleWrist3
        '
        Me.lblAngleWrist3.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblAngleWrist3.AutoSize = True
        Me.lblAngleWrist3.Location = New System.Drawing.Point(384, 113)
        Me.lblAngleWrist3.Name = "lblAngleWrist3"
        Me.lblAngleWrist3.Size = New System.Drawing.Size(14, 13)
        Me.lblAngleWrist3.TabIndex = 75
        Me.lblAngleWrist3.Text = "0"
        '
        'lblXBaseTitle
        '
        Me.lblXBaseTitle.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.lblXBaseTitle.AutoSize = True
        Me.lblXBaseTitle.Location = New System.Drawing.Point(51, 21)
        Me.lblXBaseTitle.Name = "lblXBaseTitle"
        Me.lblXBaseTitle.Size = New System.Drawing.Size(55, 13)
        Me.lblXBaseTitle.TabIndex = 59
        Me.lblXBaseTitle.Text = "X | Base"
        '
        'richTextBox1
        '
        Me.richTextBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.richTextBox1.BackColor = System.Drawing.SystemColors.Info
        Me.richTextBox1.Location = New System.Drawing.Point(12, 495)
        Me.richTextBox1.Name = "richTextBox1"
        Me.richTextBox1.Size = New System.Drawing.Size(915, 121)
        Me.richTextBox1.TabIndex = 60
        Me.richTextBox1.Text = ""
        '
        'lblCheckMoveStatusTimeOut
        '
        Me.lblCheckMoveStatusTimeOut.AutoSize = True
        Me.lblCheckMoveStatusTimeOut.Location = New System.Drawing.Point(27, 41)
        Me.lblCheckMoveStatusTimeOut.Name = "lblCheckMoveStatusTimeOut"
        Me.lblCheckMoveStatusTimeOut.Size = New System.Drawing.Size(73, 12)
        Me.lblCheckMoveStatusTimeOut.TabIndex = 64
        Me.lblCheckMoveStatusTimeOut.Text = "Time Out (ms)"
        Me.lblCheckMoveStatusTimeOut.Visible = False
        '
        'lblIpAddress
        '
        Me.lblIpAddress.AutoSize = True
        Me.lblIpAddress.Location = New System.Drawing.Point(27, 9)
        Me.lblIpAddress.Name = "lblIpAddress"
        Me.lblIpAddress.Size = New System.Drawing.Size(55, 12)
        Me.lblIpAddress.TabIndex = 63
        Me.lblIpAddress.Text = "IP Address"
        Me.lblIpAddress.Visible = False
        '
        'nudCheckMoveStatusTimeOut
        '
        Me.nudCheckMoveStatusTimeOut.Location = New System.Drawing.Point(135, 39)
        Me.nudCheckMoveStatusTimeOut.Maximum = New Decimal(New Integer() {999999, 0, 0, 0})
        Me.nudCheckMoveStatusTimeOut.Name = "nudCheckMoveStatusTimeOut"
        Me.nudCheckMoveStatusTimeOut.Size = New System.Drawing.Size(144, 22)
        Me.nudCheckMoveStatusTimeOut.TabIndex = 62
        Me.nudCheckMoveStatusTimeOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.nudCheckMoveStatusTimeOut.Value = New Decimal(New Integer() {30000, 0, 0, 0})
        Me.nudCheckMoveStatusTimeOut.Visible = False
        '
        'txtIpAddress
        '
        Me.txtIpAddress.Location = New System.Drawing.Point(135, 6)
        Me.txtIpAddress.Name = "txtIpAddress"
        Me.txtIpAddress.Size = New System.Drawing.Size(144, 22)
        Me.txtIpAddress.TabIndex = 61
        Me.txtIpAddress.Text = "192.168.1.4"
        Me.txtIpAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtIpAddress.Visible = False
        '
        'Timer1
        '
        Me.Timer1.Interval = 500
        '
        'txtSpeed
        '
        Me.txtSpeed.Location = New System.Drawing.Point(565, 54)
        Me.txtSpeed.Name = "txtSpeed"
        Me.txtSpeed.Size = New System.Drawing.Size(71, 22)
        Me.txtSpeed.TabIndex = 70
        Me.txtSpeed.Text = "0.15"
        Me.txtSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtAcc
        '
        Me.txtAcc.Location = New System.Drawing.Point(565, 21)
        Me.txtAcc.Name = "txtAcc"
        Me.txtAcc.Size = New System.Drawing.Size(71, 22)
        Me.txtAcc.TabIndex = 69
        Me.txtAcc.Text = "0.15"
        Me.txtAcc.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblSpeedTitle
        '
        Me.lblSpeedTitle.AutoSize = True
        Me.lblSpeedTitle.Location = New System.Drawing.Point(489, 57)
        Me.lblSpeedTitle.Name = "lblSpeedTitle"
        Me.lblSpeedTitle.Size = New System.Drawing.Size(66, 12)
        Me.lblSpeedTitle.TabIndex = 68
        Me.lblSpeedTitle.Text = "Speed (rad/s)"
        '
        'lblAccTitle
        '
        Me.lblAccTitle.AutoSize = True
        Me.lblAccTitle.Location = New System.Drawing.Point(489, 24)
        Me.lblAccTitle.Name = "lblAccTitle"
        Me.lblAccTitle.Size = New System.Drawing.Size(75, 12)
        Me.lblAccTitle.TabIndex = 67
        Me.lblAccTitle.Text = "Acc (rad/(s^2))"
        '
        'gbxTcpRV
        '
        Me.gbxTcpRV.Controls.Add(Me.btnMoveTcpTest)
        Me.gbxTcpRV.Controls.Add(Me.lblTpRxTitle)
        Me.gbxTcpRV.Controls.Add(Me.nudTcpRz)
        Me.gbxTcpRV.Controls.Add(Me.lblTpRzTitle)
        Me.gbxTcpRV.Controls.Add(Me.lblTpRyTitle)
        Me.gbxTcpRV.Controls.Add(Me.nudTcpRy)
        Me.gbxTcpRV.Controls.Add(Me.nudTcpRx)
        Me.gbxTcpRV.Location = New System.Drawing.Point(991, 9)
        Me.gbxTcpRV.Name = "gbxTcpRV"
        Me.gbxTcpRV.Size = New System.Drawing.Size(256, 189)
        Me.gbxTcpRV.TabIndex = 71
        Me.gbxTcpRV.TabStop = False
        Me.gbxTcpRV.Text = "TCP Position(rv)"
        '
        'gbxRealtime
        '
        Me.gbxRealtime.Controls.Add(Me.tableLayoutPanel1)
        Me.gbxRealtime.Font = New System.Drawing.Font("新細明體", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.gbxRealtime.ForeColor = System.Drawing.SystemColors.HotTrack
        Me.gbxRealtime.Location = New System.Drawing.Point(359, 98)
        Me.gbxRealtime.Name = "gbxRealtime"
        Me.gbxRealtime.Size = New System.Drawing.Size(506, 168)
        Me.gbxRealtime.TabIndex = 73
        Me.gbxRealtime.TabStop = False
        Me.gbxRealtime.Text = "Realtime Status"
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3, Me.Column4, Me.Column5, Me.Column6, Me.Column7, Me.Column8, Me.Column9})
        Me.DataGridView1.Location = New System.Drawing.Point(6, 13)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowTemplate.Height = 24
        Me.DataGridView1.Size = New System.Drawing.Size(557, 163)
        Me.DataGridView1.TabIndex = 60
        '
        'Column1
        '
        Me.Column1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Column1.HeaderText = "Check"
        Me.Column1.Name = "Column1"
        Me.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Column1.Width = 50
        '
        'Column2
        '
        Me.Column2.HeaderText = "X"
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        Me.Column2.Width = 50
        '
        'Column3
        '
        Me.Column3.HeaderText = "Y"
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        Me.Column3.Width = 50
        '
        'Column4
        '
        Me.Column4.HeaderText = "Z"
        Me.Column4.Name = "Column4"
        Me.Column4.ReadOnly = True
        Me.Column4.Width = 50
        '
        'Column5
        '
        Me.Column5.HeaderText = "Roll(°)"
        Me.Column5.Name = "Column5"
        Me.Column5.ReadOnly = True
        Me.Column5.Width = 50
        '
        'Column6
        '
        Me.Column6.HeaderText = "Pitch(°)"
        Me.Column6.Name = "Column6"
        Me.Column6.ReadOnly = True
        Me.Column6.Width = 50
        '
        'Column7
        '
        Me.Column7.HeaderText = "Yaw(°)"
        Me.Column7.Name = "Column7"
        Me.Column7.ReadOnly = True
        Me.Column7.Width = 50
        '
        'Column8
        '
        Me.Column8.HeaderText = "Comment"
        Me.Column8.Name = "Column8"
        '
        'Column9
        '
        Me.Column9.HeaderText = "Goto"
        Me.Column9.Name = "Column9"
        Me.Column9.Text = "Move"
        Me.Column9.Width = 50
        '
        'GroupBox_Record
        '
        Me.GroupBox_Record.Controls.Add(Me.Button_LoadRecord)
        Me.GroupBox_Record.Controls.Add(Me.Button_SaveRecord)
        Me.GroupBox_Record.Controls.Add(Me.Button_RemoveUnchecked)
        Me.GroupBox_Record.Controls.Add(Me.Label2)
        Me.GroupBox_Record.Controls.Add(Me.TextBox_Comment)
        Me.GroupBox_Record.Controls.Add(Me.Button_AddToRecord)
        Me.GroupBox_Record.Controls.Add(Me.DataGridView1)
        Me.GroupBox_Record.Font = New System.Drawing.Font("新細明體", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(136, Byte))
        Me.GroupBox_Record.ForeColor = System.Drawing.Color.DarkGreen
        Me.GroupBox_Record.Location = New System.Drawing.Point(359, 267)
        Me.GroupBox_Record.Name = "GroupBox_Record"
        Me.GroupBox_Record.Size = New System.Drawing.Size(569, 222)
        Me.GroupBox_Record.TabIndex = 73
        Me.GroupBox_Record.TabStop = False
        Me.GroupBox_Record.Text = "Record"
        '
        'Button_LoadRecord
        '
        Me.Button_LoadRecord.ForeColor = System.Drawing.Color.MidnightBlue
        Me.Button_LoadRecord.Location = New System.Drawing.Point(434, 179)
        Me.Button_LoadRecord.Name = "Button_LoadRecord"
        Me.Button_LoadRecord.Size = New System.Drawing.Size(66, 37)
        Me.Button_LoadRecord.TabIndex = 78
        Me.Button_LoadRecord.Text = "LOAD"
        Me.Button_LoadRecord.UseVisualStyleBackColor = True
        '
        'Button_SaveRecord
        '
        Me.Button_SaveRecord.ForeColor = System.Drawing.Color.MidnightBlue
        Me.Button_SaveRecord.Location = New System.Drawing.Point(364, 179)
        Me.Button_SaveRecord.Name = "Button_SaveRecord"
        Me.Button_SaveRecord.Size = New System.Drawing.Size(66, 37)
        Me.Button_SaveRecord.TabIndex = 77
        Me.Button_SaveRecord.Text = "SAVE"
        Me.Button_SaveRecord.UseVisualStyleBackColor = True
        '
        'Button_RemoveUnchecked
        '
        Me.Button_RemoveUnchecked.ForeColor = System.Drawing.Color.MidnightBlue
        Me.Button_RemoveUnchecked.Location = New System.Drawing.Point(283, 179)
        Me.Button_RemoveUnchecked.Name = "Button_RemoveUnchecked"
        Me.Button_RemoveUnchecked.Size = New System.Drawing.Size(76, 37)
        Me.Button_RemoveUnchecked.TabIndex = 76
        Me.Button_RemoveUnchecked.Text = "Remove Unchecked"
        Me.Button_RemoveUnchecked.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.Label2.Location = New System.Drawing.Point(10, 189)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(55, 13)
        Me.Label2.TabIndex = 75
        Me.Label2.Text = "Comment"
        '
        'TextBox_Comment
        '
        Me.TextBox_Comment.Location = New System.Drawing.Point(71, 186)
        Me.TextBox_Comment.Name = "TextBox_Comment"
        Me.TextBox_Comment.Size = New System.Drawing.Size(125, 23)
        Me.TextBox_Comment.TabIndex = 74
        '
        'Button_AddToRecord
        '
        Me.Button_AddToRecord.ForeColor = System.Drawing.Color.MidnightBlue
        Me.Button_AddToRecord.Location = New System.Drawing.Point(201, 180)
        Me.Button_AddToRecord.Name = "Button_AddToRecord"
        Me.Button_AddToRecord.Size = New System.Drawing.Size(76, 35)
        Me.Button_AddToRecord.TabIndex = 61
        Me.Button_AddToRecord.Text = "Add Record"
        Me.Button_AddToRecord.UseVisualStyleBackColor = True
        '
        'RobotUnitForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(939, 607)
        Me.Controls.Add(Me.GroupBox_Record)
        Me.Controls.Add(Me.gbxRealtime)
        Me.Controls.Add(Me.gbxTcpRV)
        Me.Controls.Add(Me.txtSpeed)
        Me.Controls.Add(Me.lblCheckMoveStatusTimeOut)
        Me.Controls.Add(Me.txtAcc)
        Me.Controls.Add(Me.lblIpAddress)
        Me.Controls.Add(Me.lblSpeedTitle)
        Me.Controls.Add(Me.nudCheckMoveStatusTimeOut)
        Me.Controls.Add(Me.lblAccTitle)
        Me.Controls.Add(Me.txtIpAddress)
        Me.Controls.Add(Me.richTextBox1)
        Me.Controls.Add(Me.gbxAnglePosition)
        Me.Controls.Add(Me.gbxTcpPosition)
        Me.Controls.Add(Me.Button_Stop)
        Me.Controls.Add(Me.Button_DisConnect)
        Me.Controls.Add(Me.Button_Connect)
        Me.Name = "RobotUnitForm"
        Me.Text = "Form1"
        Me.gbxTcpPosition.ResumeLayout(False)
        Me.gbxTcpPosition.PerformLayout()
        CType(Me.nudTcpYaw, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpPitch, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpRoll, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbxMoveAbs.ResumeLayout(False)
        CType(Me.nudTcpZ, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpX, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpRz, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpRy, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudTcpRx, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbxAnglePosition.ResumeLayout(False)
        Me.gbxAnglePosition.PerformLayout()
        CType(Me.nudAnglerZ, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAnglerY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAnglerX, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAngleZ, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAngleY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudAngleX, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tableLayoutPanel1.ResumeLayout(False)
        Me.tableLayoutPanel1.PerformLayout()
        CType(Me.nudCheckMoveStatusTimeOut, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbxTcpRV.ResumeLayout(False)
        Me.gbxTcpRV.PerformLayout()
        Me.gbxRealtime.ResumeLayout(False)
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox_Record.ResumeLayout(False)
        Me.GroupBox_Record.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button_Connect As Windows.Forms.Button
    Friend WithEvents Button_DisConnect As Windows.Forms.Button
    Friend WithEvents Button_Stop As Windows.Forms.Button
    Friend WithEvents gbxTcpPosition As Windows.Forms.GroupBox
    Friend WithEvents nudTcpYaw As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpPitch As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpRoll As Windows.Forms.NumericUpDown
    Friend WithEvents lblTpPitchTitle As Windows.Forms.Label
    Friend WithEvents lblTpYawTitle As Windows.Forms.Label
    Friend WithEvents lblTpRollTitle As Windows.Forms.Label
    Friend WithEvents btnMoveTcpRpyTest As Windows.Forms.Button
    Friend WithEvents btnGetTcpPosition As Windows.Forms.Button
    Friend WithEvents btnMoveTcpTest As Windows.Forms.Button
    Friend WithEvents nudTcpRz As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpRy As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpRx As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpZ As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpY As Windows.Forms.NumericUpDown
    Friend WithEvents nudTcpX As Windows.Forms.NumericUpDown
    Friend WithEvents lblTpRyTitle As Windows.Forms.Label
    Friend WithEvents lblTpRzTitle As Windows.Forms.Label
    Friend WithEvents lblTpYTitle As Windows.Forms.Label
    Friend WithEvents lblTpRxTitle As Windows.Forms.Label
    Friend WithEvents lblTpZTitle As Windows.Forms.Label
    Friend WithEvents lblTpXTitle As Windows.Forms.Label
    Friend WithEvents gbxAnglePosition As Windows.Forms.GroupBox
    Friend WithEvents btnGetMotorAngle As Windows.Forms.Button
    Friend WithEvents btnMoveAngleTest As Windows.Forms.Button
    Friend WithEvents lblApWrist3Title As Windows.Forms.Label
    Friend WithEvents lblApWrist2Title As Windows.Forms.Label
    Friend WithEvents lblApWrist1Title As Windows.Forms.Label
    Friend WithEvents lblApElbowTitle As Windows.Forms.Label
    Friend WithEvents lblApShoulderTitle As Windows.Forms.Label
    Friend WithEvents lblApBaseTitle As Windows.Forms.Label
    Friend WithEvents nudAnglerZ As Windows.Forms.NumericUpDown
    Friend WithEvents nudAnglerY As Windows.Forms.NumericUpDown
    Friend WithEvents nudAnglerX As Windows.Forms.NumericUpDown
    Friend WithEvents nudAngleZ As Windows.Forms.NumericUpDown
    Friend WithEvents nudAngleY As Windows.Forms.NumericUpDown
    Friend WithEvents nudAngleX As Windows.Forms.NumericUpDown
    Private WithEvents tableLayoutPanel1 As Windows.Forms.TableLayoutPanel
    Private WithEvents lblTcpY As Windows.Forms.Label
    Private WithEvents lblTcpZ As Windows.Forms.Label
    Private WithEvents lblTcpX As Windows.Forms.Label
    Private WithEvents lblrZWrist3Title As Windows.Forms.Label
    Private WithEvents lblTcptitle As Windows.Forms.Label
    Private WithEvents lblrYWrist2Title As Windows.Forms.Label
    Private WithEvents lblXBaseTitle As Windows.Forms.Label
    Private WithEvents lblrXWrist1Title As Windows.Forms.Label
    Private WithEvents lblYShoulderTitle As Windows.Forms.Label
    Private WithEvents lblZElbowTitle As Windows.Forms.Label
    Private WithEvents lblAngletitle As Windows.Forms.Label
    Private WithEvents lblTcpRoll As Windows.Forms.Label
    Private WithEvents lblTcpPitch As Windows.Forms.Label
    Private WithEvents lblTcpYaw As Windows.Forms.Label
    Private WithEvents lblAngleBase As Windows.Forms.Label
    Private WithEvents lblAngleShoulder As Windows.Forms.Label
    Private WithEvents lblAngleElbow As Windows.Forms.Label
    Private WithEvents lblAngleWrist1 As Windows.Forms.Label
    Private WithEvents lblAngleWrist2 As Windows.Forms.Label
    Private WithEvents lblAngleWrist3 As Windows.Forms.Label
    Private WithEvents richTextBox1 As Windows.Forms.RichTextBox
    Private WithEvents lblCheckMoveStatusTimeOut As Windows.Forms.Label
    Private WithEvents lblIpAddress As Windows.Forms.Label
    Private WithEvents nudCheckMoveStatusTimeOut As Windows.Forms.NumericUpDown
    Private WithEvents txtIpAddress As Windows.Forms.TextBox
    Friend WithEvents Timer1 As Windows.Forms.Timer
    Private WithEvents txtSpeed As Windows.Forms.TextBox
    Private WithEvents txtAcc As Windows.Forms.TextBox
    Friend WithEvents lblSpeedTitle As Windows.Forms.Label
    Friend WithEvents lblAccTitle As Windows.Forms.Label
    Friend WithEvents gbxTcpRV As Windows.Forms.GroupBox
    Friend WithEvents gbxMoveAbs As Windows.Forms.GroupBox
    Friend WithEvents Button2 As Windows.Forms.Button
    Friend WithEvents Button1 As Windows.Forms.Button
    Friend WithEvents Button12 As Windows.Forms.Button
    Friend WithEvents Button11 As Windows.Forms.Button
    Friend WithEvents Button10 As Windows.Forms.Button
    Friend WithEvents Button9 As Windows.Forms.Button
    Friend WithEvents Button8 As Windows.Forms.Button
    Friend WithEvents Button7 As Windows.Forms.Button
    Friend WithEvents Button6 As Windows.Forms.Button
    Friend WithEvents Button5 As Windows.Forms.Button
    Friend WithEvents Button4 As Windows.Forms.Button
    Friend WithEvents Button3 As Windows.Forms.Button
    Friend WithEvents RadioButton3 As Windows.Forms.RadioButton
    Friend WithEvents RadioButton2 As Windows.Forms.RadioButton
    Friend WithEvents RadioButton1 As Windows.Forms.RadioButton
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents gbxRealtime As Windows.Forms.GroupBox
    Friend WithEvents DataGridView1 As Windows.Forms.DataGridView
    Friend WithEvents GroupBox_Record As Windows.Forms.GroupBox
    Friend WithEvents Button_LoadRecord As Windows.Forms.Button
    Friend WithEvents Button_SaveRecord As Windows.Forms.Button
    Friend WithEvents Button_RemoveUnchecked As Windows.Forms.Button
    Private WithEvents Label2 As Windows.Forms.Label
    Private WithEvents TextBox_Comment As Windows.Forms.TextBox
    Friend WithEvents Button_AddToRecord As Windows.Forms.Button
    Friend WithEvents Column1 As Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents Column2 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column3 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column4 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column5 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column6 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column7 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column8 As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column9 As Windows.Forms.DataGridViewButtonColumn
    Private WithEvents Label3 As Windows.Forms.Label
End Class
