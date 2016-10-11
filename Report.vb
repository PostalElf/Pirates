Public Class Report
    Private Shared Reports As New List(Of Report)
    Public Shared Sub Add(ByVal s As String)
        Dim report As New Report(s)
        Reports.Add(report)
    End Sub
    Public Shared Sub ConsoleReport()
        For Each Report In Reports
            Report.ConsoleWrite()
        Next
        Reports.Clear()
    End Sub

    Private Value As String
    Private Sub New(ByVal s As String)
        Value = s
    End Sub
    Private Sub ConsoleWrite()
        Console.WriteLine(Value)
    End Sub
End Class
