Public Class Report
    Private Shared Reports As New Dictionary(Of ReportType, List(Of Report))
    Private Shared Ignores As New List(Of ReportType)
    Public Shared Sub Add(ByVal s As String, ByVal type As ReportType)
        Dim report As New Report(s, type)
        If Reports.ContainsKey(type) = False Then Reports.Add(type, New List(Of Report))
        Reports(type).Add(report)
    End Sub
    Public Shared Sub ToggleIgnore(ByVal t As ReportType)
        If Ignores.Contains(t) Then
            Ignores.Remove(t)
        Else
            Ignores.Add(t)
        End If
    End Sub
    Public Shared Sub ConsoleReport()
        For Each k In Reports.Keys
            Dim repList As List(Of Report) = Reports(k)
            If Ignores.Contains(k) = False Then
                For Each Report In repList
                    Report.ConsoleWrite()
                Next
            End If
            repList.Clear()
        Next
    End Sub

    Private Value As String
    Private Type As ReportType
    Private Sub New(ByVal s As String, ByVal t As ReportType)
        Value = s
        Type = t
    End Sub
    Private Sub ConsoleWrite()
        Console.WriteLine(Value)
    End Sub
End Class

Public Enum ReportType
    Null = 0

    Melee
    CrewMove
    CrewAttack
    CrewDeath

    PlayerShipAttack
    EnemyShipAttack
    ShipDamage
    ShipDeath
    WindMoveToken
    MoveToken
End Enum
