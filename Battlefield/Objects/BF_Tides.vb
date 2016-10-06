Public Class BF_Tides
    Implements BattlefieldObject

    Public Sub New(ByVal pFacing As BattleDirection)
        Facing = pFacing
    End Sub

    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Public Sub Move(ByVal move() As BattleMove) Implements BattlefieldObject.Move
        'ignore
    End Sub
    Public Sub MovedInto(ByRef ship As Ship) Implements BattlefieldObject.MovedInto
        Dim targetSquare As Battlesquare = BattleSquare.GetSubjectiveAdjacent(Facing, ShipQuarter.Fore, 1)
        ship.SetSquare(targetSquare)
    End Sub
    Public ReadOnly Property PathingCost As Integer Implements BattlefieldObject.PathingCost
        Get
            Return 1
        End Get
    End Property

    Public Sub ConsoleWrite() Implements BattlefieldObject.ConsoleWrite
        Console.ForegroundColor = ConsoleColor.Blue
        Select Case Facing
            Case BattleDirection.North : Console.Write("^")
            Case BattleDirection.East : Console.Write(">")
            Case BattleDirection.South : Console.Write(",")
            Case BattleDirection.West : Console.Write("<")
        End Select
    End Sub

    Public Sub Damage(ByVal damage As ShipDamage, ByVal targetQuarter As ShipQuarter) Implements BattlefieldObject.Damage
        'immune
    End Sub
    Private Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter Implements BattlefieldObject.GetTargetQuarter
        Return ShipQuarter.Fore
    End Function

End Class
