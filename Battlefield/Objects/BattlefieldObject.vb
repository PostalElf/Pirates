Public Interface BattlefieldObject
    Property Name As String
    Property BattleSquare As Battlesquare
    ReadOnly Property PathingCost As Integer
    Property Facing As BattleDirection
    Sub Move(ByVal move As BattleMove())
    Function MovedInto(ByRef target As BattlefieldObject) As Boolean                'return false to stop all queued movement

    Sub ConsoleWrite()

    Sub Damage(ByVal damage As Damage, ByVal targetQuarter As ShipQuarter)
    Sub Tick()
    Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter
End Interface
