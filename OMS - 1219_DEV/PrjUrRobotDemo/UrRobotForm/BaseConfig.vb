Imports System.IO
Imports System.Xml.Serialization



Public MustInherit Class clsBaseConfig(Of T)
    Private _Msg As String = String.Empty


    Public Sub WriteXML(ByRef XML_Handler As T, ByVal File_Name As String)
        Dim objXmlSerializer As XmlSerializer = Nothing
        Dim objStreamWriter As StreamWriter = Nothing

        Try
            '寫入XML前，要將參數編碼
            StartingWriteXML()

            objXmlSerializer = New XmlSerializer(GetType(T))
            objStreamWriter = New StreamWriter(File_Name) '如果執行到這邊就中斷，檔案也會儲存，但沒有內容。
            objXmlSerializer.Serialize(objStreamWriter, XML_Handler)

            '寫入XML後，要將參數再解碼回來
            FinishedWriteXML()
        Catch ex As Exception
            _Msg = String.Format("XML[{0}]無法寫到路徑[{1}]", XML_Handler.GetType.FullName, File_Name)
            Windows.Forms.MessageBox.Show(ex.ToString, _Msg)
            Throw New Exception(_Msg, ex) '如果ReadXML執行WriteXML當掉時，ReadXML面的動作就不需要執行了。
        Finally
            objStreamWriter.Close()
        End Try
    End Sub


    Public Sub ReadXML(ByRef XML_Handler As T, ByVal File_Name As String)
        Dim objXmlSerializer As XmlSerializer = Nothing
        Dim objFileStream As FileStream = Nothing
        Dim objConfig As T = Nothing

        Try
            '如果要讀取的檔案不存在，以預設值建立。
            If File.Exists(File_Name) = False Then
                _Msg = String.Format("檔案[{0}]不存在，{1}程式會以預設值建立新檔案", File_Name, vbNewLine)

                If System.Windows.Forms.MessageBox.Show(_Msg, "警告") = Windows.Forms.DialogResult.OK Then
                    If Directory.Exists(Path.GetDirectoryName(File_Name)) = False Then
                        Directory.CreateDirectory(Path.GetDirectoryName(File_Name))
                    End If
                    WriteXML(XML_Handler, File_Name)
                End If
            End If

            objXmlSerializer = New XmlSerializer(GetType(T))
            objFileStream = New FileStream(File_Name, FileMode.Open, FileAccess.Read)
            objConfig = CType(objXmlSerializer.Deserialize(objFileStream), T)
            Clone(objConfig, XML_Handler)

            '讀出XML後，要將參數解碼
            FinishedReadXML()
        Catch ex As Exception
            _Msg = String.Format("XML[{0}]無法被讀取[{1}]", XML_Handler.GetType.FullName, File_Name)
            Windows.Forms.MessageBox.Show(ex.ToString, _Msg)
            Throw New Exception(_Msg, ex)
        Finally
            objFileStream.Close()
        End Try
    End Sub


    Public MustOverride Sub Clone(ByRef Source_Config As T, ByRef Destination_Config As T)


    '讀出XML後，要將參數解碼
    Protected Overridable Sub FinishedReadXML()
    End Sub


    '寫入XML前，要將參數編碼
    Protected Overridable Sub StartingWriteXML()
    End Sub


    '寫入XML後，要將參數再解碼回來
    Protected Overridable Sub FinishedWriteXML()
    End Sub
End Class



