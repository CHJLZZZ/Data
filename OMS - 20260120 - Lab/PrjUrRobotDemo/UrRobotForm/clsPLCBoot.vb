Imports System.ComponentModel

Namespace Test
    Public Class clsBase
        Inherits clsBaseConfig(Of clsBase)





        <DisplayName("1.1 "),
        Description("")>
        Public Property Order As Integer

        <DisplayName("1.2 "),
        Description("")>
        Public Property X As Double

        <DisplayName("1.3 "),
        Description("")>
        Public Property Y As Double

        'MX
        <DisplayName("1.4 "),
        Description("")>
        Public Property Z As Double

        <DisplayName("1.5 PLC Station Number"),
        Description("")>
        Public Property Roll As Double

        <DisplayName("1.6 PC Network Number"),
        Description("。")>
        Public Property Pitch As Double

        <DisplayName("1.7 PC Station Number"),
        Description("。")>
        Public Property Yaw As Double

        <DisplayName("1.8 Timeout"),
        Description("。")>
        Public Property Comment As String


        Public Sub New()
            _Order = 1
            _X = 0
            _Y = 0
            _Z = 0
            _Roll = 0
            _Pitch = 0
            _Yaw = 0
            _Comment = ""
        End Sub


        Public Overrides Sub Clone(ByRef Source_Config As clsBase, ByRef Destination_Config As clsBase)
            With Destination_Config
                ._Order = Source_Config._Order
                ._X = Source_Config._X
                ._Y = Source_Config._Y
                ._Y = Source_Config._Y
                ._Roll = Source_Config._Roll
                ._Pitch = Source_Config._Pitch
                ._Yaw = Source_Config._Yaw
                ._Comment = Source_Config._Comment
            End With
        End Sub
    End Class

    Public Class clsBaseArray
        Inherits clsBaseConfig(Of clsBaseArray)


        <DisplayName("1.1 "),
        Description("")>
        Public Property BaseArray As Collections.Generic.List(Of clsBase)

        Public Sub New()
            _BaseArray = New Collections.Generic.List(Of clsBase)
        End Sub
        Public Overrides Sub Clone(ByRef Source_Config As clsBaseArray, ByRef Destination_Config As clsBaseArray)
            Destination_Config._BaseArray.Clear()
            For Each objOne As clsBase In Source_Config._BaseArray
                Destination_Config._BaseArray.Add(objOne)
            Next
        End Sub

    End Class
End Namespace


