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
                AttackerShip.InMelee = False
                DefenderShip.InMelee = False
            End If
        End Set
    End Property

    Private AttackerShip As Ship
    Private Attackers As New List(Of Crew)
    Private AttackersRe As New List(Of Crew)
    Private DefenderShip As Ship
    Private Defenders As New List(Of Crew)
    Private DefendersRe As New List(Of Crew)
    Public Sub New(ByRef attacker As Ship, ByVal attackingQuarter As ShipQuarter, ByRef defender As Ship, ByVal defendingQuarter As ShipQuarter)
        AttackerShip = attacker
        AttackerShip.InMelee = True
        Attackers.AddRange(AttackerShip.GetCrews(attackingQuarter))
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            If q <> attackingQuarter Then AttackersRe.AddRange(attacker.GetCrews(q))
        Next

        DefenderShip = defender
        DefenderShip.InMelee = True
        Defenders.AddRange(DefenderShip.GetCrews(defendingQuarter))
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            If q <> defendingQuarter Then DefendersRe.AddRange(defender.GetCrews(q))
        Next

        Dim cls As List(Of Crew)() = {Attackers, AttackersRe, Defenders, DefendersRe}
        For Each cl In cls
            For Each c In cl
                c.ReloadWeapon()
            Next
        Next
    End Sub

    Public Sub Tick()
        Dim winner As Ship = Nothing
        Dim loser As Ship = Nothing
        If Attackers.Count = 0 Then
            winner = DefenderShip
            loser = AttackerShip
        ElseIf Defenders.Count = 0 Then
            'attackers win
            winner = AttackerShip
            loser = DefenderShip
        End If
        If winner Is Nothing = False Then
            Report.Add(loser.Name & " has been sunk in the melee!")
            winner.InMelee = False
            loser.InMelee = False
            Battlefield.DeadObjects.Add(loser)

            IsOver = True
            Exit Sub
        End If


        'attackers attack
        Dim targets As New List(Of Crew)(Defenders)
        For Each Crew In Attackers
            Crew.MeleeAttack(targets)
            If targets.Count = 0 Then targets = New List(Of Crew)(Defenders)
        Next

        'defenders attack
        targets = New List(Of Crew)(Attackers)
        For Each Crew In Defenders
            Crew.MeleeAttack(targets)
            If targets.Count = 0 Then targets = New List(Of Crew)(Attackers)
        Next

        'reinforcements
        Const ReinforcementCount As Integer = 2
        If AttackersRe.Count > 0 Then
            For n = 1 To ReinforcementCount
                Dim c As Crew = Dev.GetRandom(Of Crew)(AttackersRe)
                If c Is Nothing = False Then Attackers.Add(c)
            Next
        End If
        If DefendersRe.Count > 0 Then
            For n = 1 To ReinforcementCount
                Dim c As Crew = Dev.GetRandom(Of Crew)(DefendersRe)
                If c Is Nothing = False Then Defenders.Add(c)
            Next
        End If
    End Sub
    Public Function Contains(ByVal ship As Ship) As Boolean
        If AttackerShip.Equals(ship) OrElse DefenderShip.Equals(ship) Then Return True Else Return False
    End Function
End Class
