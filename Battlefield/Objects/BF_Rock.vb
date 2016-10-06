Public Class BF_Rock
    Implements BattlefieldObject

    Public ReadOnly Property PathingCost As Integer Implements BattlefieldObject.PathingCost
        Get
            Return 100
        End Get
    End Property
    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Public Sub Move(ByVal move() As BattleMove) Implements BattlefieldObject.Move
        'do nothing
    End Sub
    Public Sub MovedInto(ByRef ship As Ship) Implements BattlefieldObject.MovedInto
        Dim damage As New ShipDamage(10, DamageType.Ramming, "Rocks")
        ship.Damage(damage, ship.Facing)
    End Sub
    Public Sub Damage(ByVal damage As ShipDamage, ByVal targetQuarter As ShipQuarter) Implements BattlefieldObject.Damage
        'immune
    End Sub
    Private Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter Implements BattlefieldObject.GetTargetQuarter
        Return ShipQuarter.Fore
    End Function

    Public Sub ConsoleWrite() Implements BattlefieldObject.ConsoleWrite
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("&")
    End Sub
End Class
