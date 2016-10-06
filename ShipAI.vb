Public Class ShipAI
    Inherits Ship

    Public Sub New()
        MyBase.New()
    End Sub

#Region "Movement"
    Public Sub PrimitiveRouting(ByVal goal As Battlesquare)
        Dim firstPositions As New List(Of BattlePosition)
        Dim secondPositions As New Dictionary(Of BattlePosition, List(Of BattlePosition))

        'get all first-order positions (eg squares the ship can reach in one move)
        For Each mArray As BattleMove() In AvailableMoves
            Dim firstPosition As BattlePosition = BattleSquare.GetPathable(Facing, mArray)
            firstPositions.Add(firstPosition)
        Next

        'for all first-order positions, get second-order positions
        For Each firstPosition In firstPositions
            secondPositions.Add(firstPosition, New List(Of BattlePosition))
            For Each mArray As BattleMove() In AvailableMoves
                Dim secondPosition As BattlePosition = firstPosition.Square.GetPathable(firstPosition.Facing, mArray)
                secondPositions(firstPosition).Add(secondPosition)
            Next
        Next

        'get cheapest (closest) second-order position
        'once found, work back up the chain to get first-order position
        'from first-order position, get the move required
        Dim targetFirstPosition As BattlePosition = Nothing
        Dim targetFirstPositionCost As Integer = Integer.MaxValue
        Dim targetMoves As BattleMove() = Nothing
        For Each fp In firstPositions
            For Each sp In secondPositions(fp)
                Dim cost As Integer = GetHeuristicDistance(sp, goal)
                If cost < targetFirstPositionCost Then
                    targetFirstPosition = fp
                    targetFirstPositionCost = cost
                    targetMoves = fp.ParentMove
                End If
            Next
        Next

        'execute move
        If targetMoves Is Nothing = False Then
            For n = 0 To targetMoves.Length - 1
                Dim m As BattleMove = targetMoves(n)
                Console.Write(m.ToString)
                If n < targetMoves.Length - 1 Then Console.Write(" + ")
            Next
            Console.WriteLine()
            MyBase.Move(targetMoves)
        End If
    End Sub
    Private Function GetHeuristicDistance(ByVal start As BattlePosition, ByVal goal As Battlesquare) As Double
        Dim dx As Integer = Math.Abs(start.Square.X - goal.X)
        Dim dy As Integer = Math.Abs(start.Square.Y - goal.Y)
        Dim raw As Integer = dx + dy

        'TODO: heuristic should consider ship position as well
        'broadside facing enemy is more valuable

        Return raw
    End Function
#End Region
End Class
