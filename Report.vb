Public Class Report
    Private Shared Reports As New List(Of Report)
    Private Shared Ignores As New List(Of ReportType)
    Public Shared Sub Add(ByVal s As String, ByVal type As ReportType)
        Dim report As New Report(s, type)
        Reports.Add(report)
    End Sub
    Public Shared Sub ToggleIgnore(ByVal t As ReportType)
        If Ignores.Contains(t) Then
            Ignores.Remove(t)
        Else
            Ignores.Add(t)
        End If
    End Sub
    Public Shared Sub ConsoleReport()
        For Each Report In Reports
            If Ignores.Contains(Report.Type) = False Then Report.ConsoleWrite()
        Next
        Reports.Clear()
    End Sub

    Private Value As String
    Private Type As ReportType
    Private ReadOnly Property ConsoleColour As ConsoleColor
        Get
            Select Case Type
                Case ReportType.EnemyShipAttack, ReportType.PlayerShipAttack : Return ConsoleColor.DarkRed
                Case ReportType.ShipDeath, ReportType.CrewDeath : Return ConsoleColor.Red
                Case Else : Return ConsoleColor.Gray
            End Select
        End Get
    End Property
    Private Sub New(ByVal s As String, ByVal t As ReportType)
        Value = s
        Type = t
    End Sub
    Private Sub ConsoleWrite()
        Console.ForegroundColor = ConsoleColour
        Console.WriteLine(Value)
    End Sub
End Class

Public Enum ReportType
    Null = 0

    Melee
    CrewMove
    CrewAttack
    CrewDamage
    CrewDeath
    EnemyCrewDamage
    EnemyCrewDeath
    Doctor

    PlayerShipAttack
    EnemyShipAttack
    ShipDamage
    ShipDeath
    WindMoveToken
    MoveToken

    Politics
    PoliticsMain
    Commerce
    CrewConsumption
    CrewMorale
    WindChange
    TravelMain
    TravelProgress
End Enum
