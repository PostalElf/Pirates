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
        Attackers.AddRange(AttackerShip.GetCrews(attackingQuarter, Nothing))
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            If q <> attackingQuarter Then AttackersRe.AddRange(attacker.GetCrews(q, Nothing))
        Next

        DefenderShip = defender
        DefenderShip.InMelee = True
        Defenders.AddRange(DefenderShip.GetCrews(defendingQuarter, Nothing))
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            If q <> defendingQuarter Then DefendersRe.AddRange(defender.GetCrews(q, Nothing))
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
        Attack(Attackers, Defenders)
        Attack(Defenders, Attackers)

        'reinforcements
        Reinforce(AttackersRe, Attackers)
        Reinforce(DefendersRe, Defenders)
    End Sub
    Private Sub Attack(ByRef offenders As List(Of Crew), ByRef targetList As List(Of Crew))
        Dim targets As New List(Of Crew)(targetList)
        For Each Crew In offenders
            Crew.MeleeAttack(targets)
            If targets.Count = 0 Then targets = New List(Of Crew)(targetList)
        Next
    End Sub
    Private Sub Reinforce(ByRef offenders As List(Of Crew), ByRef destination As List(Of Crew))
        Const ReinforcementCount As Integer = 2
        If offenders.Count < 0 Then
            For n = 1 To ReinforcementCount
                Dim c As Crew = Dev.GetRandom(Of Crew)(offenders)
                If c Is Nothing = False Then
                    destination.Add(c)
                    Report.Add(c.Name & " joins the melee.")
                End If
            Next
        End If
    End Sub
    Public Function Contains(ByVal ship As Ship) As Boolean
        If AttackerShip.Equals(ship) OrElse DefenderShip.Equals(ship) Then Return True Else Return False
    End Function
End Class
