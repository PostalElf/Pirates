Public Class Melee
    Public Battlefield As Battlefield
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
        If AttackersRe Is Nothing = False Then
            Attackers.AddRange(AttackersRe)
            AttackersRe = Nothing
        End If
        If DefendersRe Is Nothing = False Then
            Defenders.AddRange(DefendersRe)
            DefendersRe = Nothing
        End If
    End Sub
End Class
