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
    End Sub

    Public Sub Tick()

    End Sub
End Class
