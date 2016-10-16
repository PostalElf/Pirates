Public Class BF_Tides
    Implements BattlefieldObject

    Public Sub New(ByVal pFacing As BattleDirection)
        Facing = pFacing
    End Sub

    Public Property Name As String Implements BattlefieldObject.Name
        Get
            Return "Tides"
        End Get
        Set(ByVal value As String)
            'do nothing
        End Set
    End Property
    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Public Sub Move(ByVal move As MoveToken) Implements BattlefieldObject.Move
        'ignore
    End Sub
    Public Function MovedInto(ByRef bo As BattlefieldObject) As Boolean Implements BattlefieldObject.MovedInto
        Dim targetSquare As Battlesquare = BattleSquare.GetSubjectiveAdjacent(Facing, ShipQuarter.Fore, 1)
        If TypeOf bo Is Ship Then
            Dim ship As Ship = CType(bo, Ship)
            ship.SetSquare(targetSquare)
        End If
        Return True
    End Function
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

    Public Sub Damage(ByVal damage As Damage, ByVal targetQuarter As ShipQuarter, ByVal accuracy As Integer) Implements BattlefieldObject.Damage
        'immune
    End Sub
    Public Sub CombatTick() Implements BattlefieldObject.CombatTick
        'do nothing
    End Sub
    Private Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter Implements BattlefieldObject.GetTargetQuarter
        Return ShipQuarter.Fore
    End Function

End Class
