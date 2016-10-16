Public Class BF_Rock
    Implements BattlefieldObject

    Public Property Name As String Implements BattlefieldObject.Name
        Get
            Return "Rock"
        End Get
        Set(ByVal value As String)
            'do nothing
        End Set
    End Property
    Public ReadOnly Property PathingCost As Integer Implements BattlefieldObject.PathingCost
        Get
            Return 100
        End Get
    End Property
    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Public Sub Move(ByVal move As MoveToken) Implements BattlefieldObject.Move
        'do nothing
    End Sub
    Public Function MovedInto(ByRef bo As BattlefieldObject) As Boolean Implements BattlefieldObject.MovedInto
        Dim damage As New Damage(1, 0, DamageType.Ramming, "Rocks")
        bo.Damage(damage, bo.Facing, 5)
        Return True
    End Function
    Public Sub Damage(ByVal damage As Damage, ByVal targetQuarter As ShipQuarter, ByVal accuracy As Integer) Implements BattlefieldObject.Damage
        'immune
    End Sub
    Public Sub CombatTick() Implements BattlefieldObject.CombatTick
        'do nothing
    End Sub
    Private Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter Implements BattlefieldObject.GetTargetQuarter
        Return ShipQuarter.Fore
    End Function

    Public Sub ConsoleWrite() Implements BattlefieldObject.ConsoleWrite
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("&")
    End Sub
End Class
