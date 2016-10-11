Public Interface BattlefieldObject
    Property Name As String
    Property BattleSquare As Battlesquare
    ReadOnly Property PathingCost As Integer
    Property Facing As BattleDirection
    Sub Move(ByVal move As BattleMove())
    Sub MovedInto(ByRef target As BattlefieldObject)

    Sub ConsoleWrite()

    Sub Damage(ByVal damage As ShipDamage, ByVal targetQuarter As ShipQuarter)
    Sub Tick()
    Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter
End Interface
