Public Class Melee
    Public Battlefield As Battlefield
    Private _IsOver As Boolean = False
    Public Property IsOver As Boolean
        Get
            Return _IsOver
        End Get
        Set(ByVal value As Boolean)
            _IsOver = value
            If value = True Then
                AttackerHost.InMelee = False
                DefenderHost.InMelee = False
            End If
        End Set
    End Property

    Private AttackerHost As MeleeHost
    Private Attackers As New List(Of Crew)
    Private AttackersRe As New List(Of Crew)
    Private DefenderHost As MeleeHost
    Private Defenders As New List(Of Crew)
    Private DefendersRe As New List(Of Crew)
    Public Sub New(ByRef attacker As MeleeHost, ByVal attackingQuarter As ShipQuarter, ByRef defender As MeleeHost, ByVal defendingQuarter As ShipQuarter)
        AttackerHost = attacker
        AttackerHost.InMelee = True
        Attackers.AddRange(AttackerHost.GetCrews(attackingQuarter, Nothing))
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            If q <> attackingQuarter Then AttackersRe.AddRange(attacker.GetCrews(q, Nothing))
        Next

        DefenderHost = defender
        DefenderHost.InMelee = True
        Defenders.AddRange(DefenderHost.GetCrews(defendingQuarter, Nothing))
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            If q <> defendingQuarter Then DefendersRe.AddRange(defender.GetCrews(q, Nothing))
        Next
    End Sub

    Public Sub CombatTick()
        If Attackers.Count = 0 Then Lose(AttackerHost) : Exit Sub
        If Defenders.Count = 0 Then Lose(DefenderHost) : Exit Sub

        'attackers attack
        Attack(Attackers, Defenders)
        Attack(Defenders, Attackers)

        'reinforcements
        Reinforce(AttackersRe, Attackers)
        Reinforce(DefendersRe, Defenders)
    End Sub
    Private Sub Attack(ByRef offenders As List(Of Crew), ByRef targetList As List(Of Crew))
        Dim targets As New List(Of Crew)(targetList)
        For Each Crew In offenders
            Crew.tickcombat()
            Crew.MeleeAttack(targets)
            If targets.Count = 0 Then targets = New List(Of Crew)(targetList)
        Next
    End Sub
    Private Sub Reinforce(ByRef offenders As List(Of Crew), ByRef destination As List(Of Crew))
        Const ReinforcementCount As Integer = 2
        If offenders.Count > 0 Then
            For n = 1 To ReinforcementCount
                Dim c As Crew = Dev.GrabRandom(Of Crew)(offenders, World.Rng)
                If c Is Nothing = False Then
                    destination.Add(c)
                    Report.Add("[" & c.Ship.ID & "] " & c.Name & " joins the melee.", ReportType.Melee)
                End If
            Next
        End If
    End Sub
    Public Function Contains(ByVal host As MeleeHost) As Boolean
        If AttackerHost.Equals(host) OrElse DefenderHost.Equals(host) Then Return True Else Return False
    End Function
    Public Function Contains(ByVal crew As Crew) As Boolean
        If Attackers.Contains(crew) Then Return True
        If Defenders.Contains(crew) Then Return True
        Return False
    End Function
    Public Sub Remove(ByVal crew As Crew)
        If Attackers.Contains(crew) Then Attackers.Remove(crew)
        If Defenders.Contains(crew) Then Defenders.Remove(crew)
    End Sub
    Public Sub Lose(ByVal host As MeleeHost)
        If TypeOf host Is Ship Then
            Dim ship As Ship = CType(host, Ship)
            Report.Add(ship.Name & " has been sunk in the melee!", ReportType.ShipDeath)
            Battlefield.AddDead(ship)
        ElseIf TypeOf host Is Brawl Then

        End If

        IsOver = True
    End Sub
End Class
