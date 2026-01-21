Imports System.Threading
Imports System.Drawing
Imports System.Windows.Forms
Imports AUO.LogRecorder
Imports UrRobotForm.Test
Imports HardwareManager

Public Class RobotUnitForm
    Private _RadToTheta As Double = 180D / Math.PI
    Private _ThetaToRad As Double = Math.PI / 180D
    Private _SysLog As CLogRecorder
    Private m_ClsUrRobotInterface As ClsUrRobotInterface



    Public Sub SetMachine(ByRef RobotInterface As ClsUrRobotInterface)
        Me.m_ClsUrRobotInterface = RobotInterface
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Text = $"=== AIC Control Center v{Application.ProductVersion} ==="
        '=============================
        '      建立LOG物件
        '=============================
        Try
            _SysLog = New CLogRecorder()
            If Not IO.Directory.Exists("..\Log") Then
                IO.Directory.CreateDirectory("..\Log")
            End If
            _SysLog.Open("..\Log\", "AUO_Log")

            _SysLog.WriteLog($" ============== LOG START v{Application.ProductVersion} ============== ")
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try


        Dim pitLeft() As Point = {New Point(50, 15), New Point(20, 15), New Point(20, 5), New Point(0, 25), New Point(20, 45), New Point(20, 35), New Point(50, 35)}
        Dim pitRight() As Point = {New Point(5, 15), New Point(35, 15), New Point(35, 5), New Point(55, 25), New Point(35, 45), New Point(35, 35), New Point(5, 35)}

        OverwriteBtn(Button1, pitLeft)
        OverwriteBtn(Button2, pitRight)
        OverwriteBtn(Button3, pitLeft)
        OverwriteBtn(Button4, pitRight)
        OverwriteBtn(Button5, pitLeft)
        OverwriteBtn(Button6, pitRight)
        OverwriteBtn(Button7, pitLeft)
        OverwriteBtn(Button8, pitRight)
        OverwriteBtn(Button9, pitLeft)
        OverwriteBtn(Button10, pitRight)
        OverwriteBtn(Button11, pitLeft)
        OverwriteBtn(Button12, pitRight)

        Me.Timer1.Enabled = True


    End Sub

    Private Sub OverwriteBtn(ByRef Button1 As Windows.Forms.Button, ByVal pts As Point())
        Dim polygon_path As New Drawing2D.GraphicsPath(Drawing2D.FillMode.Winding)
        polygon_path.AddPolygon(pts)

        Dim polygon_Region As New Region(polygon_path)
        Button1.Region = polygon_Region
        Button1.SetBounds(Button1.Location.X, Button1.Location.Y, 60, 50)
    End Sub
    Private Sub Button_Connect_Click(sender As Object, e As EventArgs) Handles Button_Connect.Click
        Try

            If Me.m_ClsUrRobotInterface.Connect(txtIpAddress.Text, nudCheckMoveStatusTimeOut.Value) Then

                Me.Button_Connect.Enabled = False

                Me.Button_DisConnect.Enabled = True

                Me.Button_Stop.Enabled = True

                '
                Me.btnGetMotorAngle.Enabled = True
                Me.btnGetTcpPosition.Enabled = True

                Me.Timer1.Enabled = True

                ' get current position from robot =>
                If (Me.m_ClsUrRobotInterface.IsConnected()) Then

                    Me.nudTcpX.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(0))
                    Me.nudTcpY.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(1))
                    Me.nudTcpZ.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(2))
                    Me.nudTcpRx.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(3))
                    Me.nudTcpRy.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(4))
                    Me.nudTcpRz.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(5))

                Else

                    MsgBox("Please Connect Robot first!", "Warning")
                End If
                'm_MainLoop = True
                'Dim ts As New ThreadStart(AddressOf MainLoop)
                'Dim t As New Thread(ts)
                't.Start()
            End If
        Catch ex As Exception
            InfoLog($"[Button_Connect_Click] {ex.ToString}", True)
        End Try


    End Sub

    Private Sub Button_DisConnect_Click(sender As Object, e As EventArgs) Handles Button_DisConnect.Click
        Try
            If Me.m_ClsUrRobotInterface.Disconnect() Then

                Me.Button_Connect.Enabled = True

                Me.Button_DisConnect.Enabled = False

                Me.Button_Stop.Enabled = False

                '//
                Me.btnGetTcpPosition.Enabled = False
                Me.btnMoveTcpTest.Enabled = False
                Me.btnMoveTcpRpyTest.Enabled = False

                Me.btnGetMotorAngle.Enabled = False
                Me.btnMoveAngleTest.Enabled = False
                'm_MainLoop = False
                '//
                Me.Timer1.Enabled = False
            End If

        Catch ex As Exception
            InfoLog($"[Button_DisConnect] : {ex.Message}", True)
        End Try

    End Sub

    Private Sub Button_Stop_Click(sender As Object, e As EventArgs) Handles Button_Stop.Click
        Try
            Me.m_ClsUrRobotInterface.Stop()
        Catch ex As Exception
            InfoLog($"[Button_Stop] : {ex.Message}", True)
        End Try
    End Sub


    'Private m_MainLoop As Boolean = False
    'Private Sub MainLoop()
    '    While (m_MainLoop)
    '        Try
    '            If (Me.m_ClsUrRobotInterface Is Nothing) Then
    '                Exit Sub
    '            End If
    '            If Not Me.m_ClsUrRobotInterface.IsConnected() Then
    '                Exit Sub
    '            End If
    '            '// Tcp
    '            Dim tmpDouble(5) As Double
    '            Dim i As Integer
    '            For i = 0 To tmpDouble.Length - 1
    '                tmpDouble(i) = Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(i)
    '            Next

    '            Dim rx As Double = 0.0D
    '            Dim ry As Double = 0.0D
    '            Dim rz As Double = 0.0D

    '            Me.m_ClsUrRobotInterface.RpyToRv(
    '                    Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(3),
    '                        Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(4),
    '                        Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(5),
    '                         rx,
    '                         ry,
    '                         rz)


    '            UpdateLabelStatus(Me.lblTcpX, tmpDouble(0).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblTcpY, tmpDouble(1).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblTcpZ, tmpDouble(2).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblTcprX, rx.ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblTcprY, ry.ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblTcprZ, rz.ToString("#0.000"))

    '            '// Angle
    '            For i = 0 To tmpDouble.Length - 1
    '                tmpDouble(i) = Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(i)
    '            Next

    '            UpdateLabelStatus(Me.lblAngleBase, tmpDouble(0).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblAngleShoulder, tmpDouble(1).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblAngleElbow, tmpDouble(2).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblAngleWrist1, tmpDouble(3).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblAngleWrist2, tmpDouble(4).ToString("#0.000"))
    '            UpdateLabelStatus(Me.lblAngleWrist3, tmpDouble(5).ToString("#0.000"))

    '            'InfoLog($"[Timer1_Tick] : {Now}")
    '        Catch ex As Exception
    '            InfoLog($"[Timer1_Tick] : {ex.Message}", True)
    '        End Try

    '        Threading.Thread.Sleep(500)
    '    End While

    'End Sub


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            If (Me.m_ClsUrRobotInterface Is Nothing) Then
                Exit Sub
            End If
            If Not Me.m_ClsUrRobotInterface.IsConnected() Then
                Exit Sub
            End If
            '// Tcp
            Dim tmpDouble(5) As Double
            Dim i As Integer
            For i = 0 To tmpDouble.Length - 1
                tmpDouble(i) = Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(i)
            Next



            'Dim rx As Double = 0.0D
            'Dim ry As Double = 0.0D
            'Dim rz As Double = 0.0D

            'Me.m_ClsUrRobotInterface.RpyToRv(
            '        Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(3),
            '            Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(4),
            '            Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(5),
            '             rx,
            '             ry,
            '             rz)

            '只顯示roll/pitch,yaw in degree
            Me.lblTcpX.Text = tmpDouble(0).ToString("#0.000")
            Me.lblTcpY.Text = tmpDouble(1).ToString("#0.000")
            Me.lblTcpZ.Text = tmpDouble(2).ToString("#0.000")
            Me.lblTcpRoll.Text = (tmpDouble(3) * _RadToTheta).ToString("#0.000")
            Me.lblTcpPitch.Text = (tmpDouble(4) * _RadToTheta).ToString("#0.000")
            Me.lblTcpYaw.Text = (tmpDouble(5) * _RadToTheta).ToString("#0.000")

            '// Angle
            For i = 0 To tmpDouble.Length - 1
                tmpDouble(i) = Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(i)
            Next

            Me.lblAngleBase.Text = tmpDouble(0).ToString("#0.000")
            Me.lblAngleShoulder.Text = tmpDouble(1).ToString("#0.000")
            Me.lblAngleElbow.Text = tmpDouble(2).ToString("#0.000")
            Me.lblAngleWrist1.Text = tmpDouble(3).ToString("#0.000")
            Me.lblAngleWrist2.Text = tmpDouble(4).ToString("#0.000")
            Me.lblAngleWrist3.Text = tmpDouble(5).ToString("#0.000")

            'InfoLog($"[Timer1_Tick] : {Now}")
        Catch ex As Exception
            InfoLog($"[Timer1_Tick] : {ex.Message}", True)
        End Try

    End Sub

    Private Sub btnGetTcpPosition_Click(sender As Object, e As EventArgs) Handles btnGetTcpPosition.Click

        If Me.m_ClsUrRobotInterface Is Nothing Then InfoLog($"m_ClsUrRobotInterface Is Nothing", True) : Exit Sub

        Try

            If Me.m_ClsUrRobotInterface.IsConnected() Then
                'Me.nudTcpY.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(1))

                Me.nudTcpX.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(0))
                Me.nudTcpY.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(1))
                Me.nudTcpZ.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(2))
                Me.nudTcpRoll.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(3)) * _RadToTheta
                Me.nudTcpPitch.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(4)) * _RadToTheta
                Me.nudTcpYaw.Value = Convert.ToDouble(Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(5)) * _RadToTheta

                '只顯示roll/pitch,yaw in degree
                'Dim rx As Double = 0.0D
                'Dim ry As Double = 0.0D
                'Dim rz As Double = 0.0D

                'Me.m_ClsUrRobotInterface.RpyToRv(
                '            Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(3),
                '            Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(4),
                '            Me.m_ClsUrRobotInterface.m_ClsUrStatus.TcpPose(5),
                '             rx,
                '             ry,
                '             rz)

                'Me.nudTcpRx.Value = Convert.ToDecimal(rx)
                'Me.nudTcpRy.Value = Convert.ToDecimal(ry)
                'Me.nudTcpRz.Value = Convert.ToDecimal(rz)

                Me.btnMoveTcpTest.Enabled = True
                Me.btnMoveTcpRpyTest.Enabled = True

            Else

                MsgBox("Please Connect Robot first!", "Warning")
            End If

        Catch ex As Exception
            InfoLog($"[btnGetTcpPosition] {ex.ToString}", True)
        Finally
            ' Timer1.Enabled = True
        End Try

    End Sub

    Private Sub btnMoveTcpTest_Click(sender As Object, e As EventArgs) Handles btnMoveTcpTest.Click

        'Dim ts As New ThreadStart(AddressOf MoveTcp)
        'Dim t As New Thread(ts)
        't.Start()
    End Sub

    Private Sub btnMoveTcpRpyTest_Click(sender As Object, e As EventArgs) Handles btnMoveTcpRpyTest.Click

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub btnMoveAngleTest_Click(sender As Object, e As EventArgs) Handles btnMoveAngleTest.Click

        Dim ts As New ThreadStart(AddressOf MoveToAngle)
        Dim t As New Thread(ts)
        t.Start()
    End Sub
    'Private Sub MoveTcp()
    '    Try
    '        If (Me.m_ClsUrRobotInterface.IsConnected()) Then

    '            Me.UpdateBtnStatus(Me.btnMoveTcpTest, False)

    '            Dim x As Double = CDbl(Me.nudTcpX.Value)
    '            Dim y As Double = CDbl(Me.nudTcpY.Value)
    '            Dim z As Double = CDbl(Me.nudTcpZ.Value)
    '            Dim rx As Double = CDbl(Me.nudTcpRx.Value)
    '            Dim ry As Double = CDbl(Me.nudTcpRy.Value)
    '            Dim rz As Double = CDbl(Me.nudTcpRz.Value)

    '            Dim tmpAcc As Double = CDbl(Me.txtAcc.Text)
    '            Dim tmpSpeed As Double = CDbl(Me.txtSpeed.Text)
    '            Dim tmpTime As Double = 0.0D
    '            Dim tmpBlendRadius As Double = 0.0D

    '            Me.m_ClsUrRobotInterface.MoveL_rv(
    '                           x, y, z, rx, ry, rz, tmpAcc, tmpSpeed, tmpTime, tmpBlendRadius)


    '        Else

    '            MsgBox("Please Connect Robot first!", "Warning")
    '        End If
    '    Catch ex As Exception
    '        InfoLog($"[MoveTcp] {ex.ToString}", True)
    '    Finally
    '        Me.UpdateBtnStatus(Me.btnMoveTcpTest, True)
    '    End Try

    'End Sub

    Private Sub MoveToPos()
        If Me.m_ClsUrRobotInterface Is Nothing Then InfoLog($"m_ClsUrRobotInterface Is Nothing", True) : Exit Sub

        Try

            If (Me.m_ClsUrRobotInterface.IsConnected()) Then

                Me.UpdateBtnStatus(Me.btnMoveTcpRpyTest, False)

                Dim x As Double = CDbl(Me.nudTcpX.Value)
                Dim y As Double = CDbl(Me.nudTcpY.Value)
                Dim z As Double = CDbl(Me.nudTcpZ.Value)
                Dim roll As Double = CDbl(Me.nudTcpRoll.Value) * _ThetaToRad
                Dim pitch As Double = CDbl(Me.nudTcpPitch.Value) * _ThetaToRad
                Dim yaw As Double = CDbl(Me.nudTcpYaw.Value) * _ThetaToRad

                Dim tmpAcc As Double = CDbl(Me.txtAcc.Text)
                Dim tmpSpeed As Double = CDbl(Me.txtSpeed.Text)
                Dim tmpTime As Double = 0.0D
                Dim tmpBlendRadius As Double = 0.0D

                Me.m_ClsUrRobotInterface.MoveL(x, y, z, roll, pitch, yaw, tmpAcc, tmpSpeed, tmpTime, tmpBlendRadius)

            Else

                MsgBox("Please Connect Robot first!", "Warning")
            End If
        Catch ex As Exception
            InfoLog($"[MoveTcp] {ex.ToString}", True)
        Finally
            Me.UpdateBtnStatus(Me.btnMoveTcpRpyTest, True)
        End Try

    End Sub

    Private Sub btnGetMotorAngle_Click(sender As Object, e As EventArgs) Handles btnGetMotorAngle.Click
        Try
            If Me.m_ClsUrRobotInterface.IsConnected() Then

                Me.nudAngleX.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(0))
                Me.nudAngleY.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(1))
                Me.nudAngleZ.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(2))
                Me.nudAnglerX.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(3))
                Me.nudAnglerY.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(4))
                Me.nudAnglerZ.Value = Convert.ToDecimal(Me.m_ClsUrRobotInterface.m_ClsUrStatus.MotorAngle(5))

                Me.btnMoveAngleTest.Enabled = True

            Else

                MsgBox("Please Connect Robot first!", "Warning")
            End If
        Catch ex As Exception
            InfoLog($"[btnGetMotorAngle_Click] {ex.ToString}", True)
        End Try

    End Sub

    Private Sub MoveToAngle()
        Try
            If (Me.m_ClsUrRobotInterface.IsConnected()) Then
                UpdateBtnStatus(Me.btnMoveAngleTest, False)

                Dim x As Double = CDbl(Me.nudAngleX.Value)
                Dim y As Double = CDbl(Me.nudAngleY.Value)
                Dim z As Double = CDbl(Me.nudAngleZ.Value)
                Dim rx As Double = CDbl(Me.nudAnglerX.Value)
                Dim ry As Double = CDbl(Me.nudAnglerY.Value)
                Dim rz As Double = CDbl(Me.nudAnglerZ.Value)

                Dim tmpAcc As Double = CDbl(Me.txtAcc.Text)
                Dim tmpSpeed As Double = CDbl(Me.txtSpeed.Text)
                Dim tmpBlendRadius As Double = 0.0D

                Me.m_ClsUrRobotInterface.MoveJ(
                                x, y, z, rx, ry, rz, tmpAcc, tmpSpeed, tmpBlendRadius)



            Else

                MsgBox("Please Connect Robot first!", "Warning")
            End If
        Catch ex As Exception
            InfoLog($"[MoveToAngle] {ex.ToString}", True)
        Finally
            UpdateBtnStatus(Me.btnMoveAngleTest, True)
        End Try

    End Sub


    Delegate Sub InfoLogCallback(ByVal msg As String, ByVal Err As Boolean)   'OK
    Public Sub InfoLog(ByVal msg As String, Optional ByVal Err As Boolean = False)
        Static InfoCounter As Integer = 0

        If richTextBox1.InvokeRequired Then
            Me.Invoke(New InfoLogCallback(AddressOf InfoLog), New Object() {msg, Err})
        Else
            If Err Then richTextBox1.SelectionColor = Color.Red

            _SysLog.WriteLog(msg)
            If InfoCounter > 1000 Then
                richTextBox1.Text = Now.ToString("MM/dd-HH:mm:ss  ") & msg & vbCrLf
                InfoCounter = 1
            Else
                richTextBox1.AppendText(Now.ToString("MM/dd-HH:mm:ss  ") & msg & vbCrLf)
                InfoCounter += 1
            End If
            richTextBox1.ScrollToCaret()
            Me.Update()
        End If
    End Sub



    Delegate Sub UpdateLabelStatusCallback(ByVal lb As Label, ByVal Status As String)
    Public Sub UpdateLabelStatus(ByVal lb As Label, ByVal Status As String)

        If lb.InvokeRequired Then
            Me.Invoke(New UpdateLabelStatusCallback(AddressOf UpdateLabelStatus), New Object() {lb, Status})
        Else
            lb.Text = Status
        End If
    End Sub
    Delegate Sub UpdateBtnStatusCallback(ByVal btn As Button, ByVal Status As Boolean)
    Public Sub UpdateBtnStatus(ByVal btn As Button, ByVal Status As Boolean)

        If btn.InvokeRequired Then
            Me.Invoke(New UpdateBtnStatusCallback(AddressOf UpdateBtnStatus), New Object() {btn, Status})
        Else
            btn.Enabled = Status
        End If
    End Sub

    Private Sub Form1_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If Me.m_ClsUrRobotInterface IsNot Nothing Then

            Me.m_ClsUrRobotInterface.Stop()
            'Me.m_ClsUrRobotInterface.Disconnect()
        End If

        _SysLog.WriteLog($" ============== LOG END v{Application.ProductVersion} ============== ")
        If _SysLog IsNot Nothing Then
            _SysLog.Close()

        End If
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.nudTcpX.Value -= 0.1 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.nudTcpX.Value += 0.1 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.nudTcpY.Value -= 0.1 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Me.nudTcpY.Value += 0.1 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Me.nudTcpZ.Value -= 0.1 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Me.nudTcpZ.Value += 0.1 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Function MoveStepMutiply() As Integer
        Dim RtnValue As Integer = 1
        If Me.RadioButton1.Checked Then
            RtnValue = 1
        End If
        If Me.RadioButton2.Checked Then
            RtnValue = 10
        End If
        If Me.RadioButton3.Checked Then
            RtnValue = 100
        End If
        Return RtnValue
    End Function

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Me.nudTcpRoll.Value -= 0.01 * MoveStepMutiply()  '角度輸入

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Me.nudTcpRoll.Value += 0.01 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Me.nudTcpPitch.Value -= 0.01 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Me.nudTcpPitch.Value += 0.01 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        Me.nudTcpYaw.Value -= 0.01 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        Me.nudTcpYaw.Value += 0.01 * MoveStepMutiply()

        Dim ts As New ThreadStart(AddressOf MoveToPos)
        Dim t As New Thread(ts)
        t.Start()
    End Sub

#Region "DataGrid Control"
    Private Sub Button_AddToRecord_Click(sender As Object, e As EventArgs) Handles Button_AddToRecord.Click
        Dim row(8) As String
        row(0) = "false"
        row(1) = nudTcpX.Value.ToString("F4")
        row(2) = nudTcpY.Value.ToString("F4")
        row(3) = nudTcpZ.Value.ToString("F4")
        row(4) = nudTcpRoll.Value.ToString("F4")
        row(5) = nudTcpPitch.Value.ToString("F4")
        row(6) = nudTcpYaw.Value.ToString("F4")
        row(7) = TextBox_Comment.Text
        row(8) = "GO"
        TextBox_Comment.Text = ""

        DataGridView1.Rows.Add(row)
    End Sub

    Private Sub Button_RemoveUnchecked_Click(sender As Object, e As EventArgs) Handles Button_RemoveUnchecked.Click
        For I As Integer = DataGridView1.Rows.Count - 1 To 0 Step -1
            If DataGridView1.Rows(I).Cells(0).Value Then

            Else
                DataGridView1.Rows.RemoveAt(I)
            End If
        Next
    End Sub

    Private Sub Button_SaveRecord_Click(sender As Object, e As EventArgs) Handles Button_SaveRecord.Click
        Dim _BaseList As New clsBaseArray

        For I As Integer = 0 To DataGridView1.Rows.Count - 1
            Dim _Base As New clsBase
            _Base.Order = I
            _Base.X = DataGridView1.Rows(I).Cells(1).Value
            _Base.Y = DataGridView1.Rows(I).Cells(2).Value
            _Base.Z = DataGridView1.Rows(I).Cells(3).Value
            _Base.Roll = DataGridView1.Rows(I).Cells(4).Value
            _Base.Pitch = DataGridView1.Rows(I).Cells(5).Value
            _Base.Yaw = DataGridView1.Rows(I).Cells(6).Value
            _Base.Comment = DataGridView1.Rows(I).Cells(7).Value
            _BaseList.BaseArray.Add(_Base)
        Next
        If Not IO.Directory.Exists("..\Recipe") Then
            IO.Directory.CreateDirectory("..\Recipe")
        End If
        _BaseList.WriteXML(_BaseList, "..\Recipe\BaseList.xml")

    End Sub

    Private Sub Button_LoadRecord_Click(sender As Object, e As EventArgs) Handles Button_LoadRecord.Click
        Dim _BaseList As New clsBaseArray

        If IO.File.Exists("..\Recipe\BaseList.xml") Then
            _BaseList.ReadXML(_BaseList, "..\Recipe\BaseList.xml")

        End If

        DataGridView1.Rows.Clear()

        For Each obj As clsBase In _BaseList.BaseArray
            Dim row(8) As String
            row(0) = "false"
            row(1) = obj.X.ToString("F4")
            row(2) = obj.Y.ToString("F4")
            row(3) = obj.Z.ToString("F4")
            row(4) = obj.Roll.ToString("F4")
            row(5) = obj.Pitch.ToString("F4")
            row(6) = obj.Yaw.ToString("F4")
            row(7) = obj.Comment
            row(8) = "GO"
            DataGridView1.Rows.Add(row)
        Next
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        If e.ColumnIndex > 7 Then
            If MsgBox("Confirm?", MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                Exit Sub
            End If
            Me.nudTcpX.Value = DataGridView1.Rows(e.RowIndex).Cells(1).Value
            Me.nudTcpY.Value = DataGridView1.Rows(e.RowIndex).Cells(2).Value
            Me.nudTcpZ.Value = DataGridView1.Rows(e.RowIndex).Cells(3).Value
            Me.nudTcpRoll.Value = DataGridView1.Rows(e.RowIndex).Cells(4).Value
            Me.nudTcpPitch.Value = DataGridView1.Rows(e.RowIndex).Cells(5).Value
            Me.nudTcpYaw.Value = DataGridView1.Rows(e.RowIndex).Cells(6).Value

            Dim ts As New ThreadStart(AddressOf MoveToPos)
            Dim t As New Thread(ts)
            t.Start()

        End If

    End Sub
#End Region


    'Private Sub Button_DegreeToRad_Click(sender As Object, e As EventArgs) Handles Button_DegreeToRad.Click
    '    nudRad.Value = nudDegree.Value * Math.PI / 180D
    '   nudDegree.Value  =  nudRad.Value*180D/ Math.PI  
    'End Sub
End Class