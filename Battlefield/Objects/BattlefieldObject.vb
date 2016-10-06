﻿Public Interface BattlefieldObject
    Property BattleSquare As Battlesquare
    ReadOnly Property PathingCost As Integer
    Property Facing As BattleDirection
    Sub Move(ByVal move As BattleMove())
    Sub MovedInto(ByRef ship As Ship)

    Sub ConsoleWrite()

    Sub Damage(ByVal damage As ShipDamage, ByVal targetQuarter As ShipQuarter)
    Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter
End Interface
