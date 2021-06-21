Imports System.Data.SqlClient
Imports System.IO
Module Module1
    Public SAPSrvr As String = String.Empty
    Public DbCommon As String = String.Empty

    Public Sub GETSRVR()
        Try
            Dim txtpath As String = AppDomain.CurrentDomain.BaseDirectory & "\config.dll"
            If System.IO.File.Exists(txtpath) Then
                Dim objRdr As New System.IO.StreamReader(txtpath)
                SAPSrvr = objRdr.ReadLine()
                DbCommon = objRdr.ReadLine()
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            End
        End Try
    End Sub

    Public Function CountCharacter(ByVal value As String, ByVal ch As Char) As Integer
        Return value.Count(Function(c As Char) c = ch)
    End Function
End Module
