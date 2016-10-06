Public Class ShipAI
    Inherits Ship

    Public Sub New()
        MyBase.New()
    End Sub

#Region "Movement"
    Public Sub PrimitiveRouting(ByVal goal As Battlesquare)
        Dim firstPositions As New List(Of BattlePosition())
        Dim secondPositions As New Dictionary(Of BattlePosition(), List(Of BattlePosition()))

        'get all first-order positions (eg squares the ship can reach in one move)
        For Each mArray As BattleMove() In AvailableMoves
            Dim firstPosition As BattlePosition() = BattleSquare.GetPathables(Facing, mArray)
            firstPositions.Add(firstPosition)
        Next

        'for all first-order positions, get second-order positions
        For Each firstPosition In firstPositions
            secondPositions.Add(firstPosition, New List(Of BattlePosition()))
            For Each mArray As BattleMove() In AvailableMoves
                Dim firstPositionReference As BattlePosition = firstPosition(firstPosition.Length - 1)
                Dim secondPosition As BattlePosition() = firstPositionReference.Square.GetPathables(firstPositionReference.Facing, mArray)
                secondPositions(firstPosition).Add(secondPosition)
            Next
        Next

        'get cheapest (closest) second-order position
        'once found, work back up the chain to get first-order position
        'from first-order position, get the move required
        Dim targetFirstPosition As BattlePosition() = Nothing
        Dim targetPathCost As Integer = Integer.MaxValue
        Dim targetMoves As BattleMove() = Nothing
        For Each fp As BattlePosition() In firstPositions
            For Each sp In secondPositions(fp)
                Dim pathCost As Integer = GetHeuristicDistance(fp, goal) + GetHeuristicDistance(sp, goal)
                If pathCost < targetPathCost Then
                    targetFirstPosition = fp
                    targetPathCost = pathCost
                    targetMoves = fp(0).ParentMove
                End If
            Next
        Next

        ExecuteMoves(targetMoves)
    End Sub
    Private Function GetHeuristicDistance(ByVal start As BattlePosition, ByVal goal As Battlesquare) As Double
        'manhattan distance as base
        Dim dx As Integer = Math.Abs(start.Square.X - goal.X)
        Dim dy As Integer = Math.Abs(start.Square.Y - goal.Y)
        Dim raw As Integer = dx + dy

        'add terrain cost
        raw += start.Square.PathingCost + goal.PathingCost

        'consider ship position; broadside facing enemy is more valuable
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            If AttackReady(quarter) = True Then
                Dim range As Integer = AttackRanges(quarter)
                Dim target As Battlesquare = start.Square.GetSubjectiveAdjacent(start.Facing, quarter, range)
                If target.Equals(goal) Then raw -= 10
            End If
        Next

        Return raw
    End Function
    Private Function GetHeuristicDistance(ByVal start As BattlePosition(), ByVal goal As Battlesquare) As Double
        Dim total As Integer = 0
        For Each bp In start
            total += GetHeuristicDistance(bp, goal)
        Next

        If start.Length = 1 Then total *= 2
        Return total
    End Function
    Private Sub ExecuteMoves(ByVal targetMoves As BattleMove())
        If targetMoves Is Nothing = False Then
            Dim moveReport As String = ""
            For n = 0 To targetMoves.Length - 1
                Dim m As BattleMove = targetMoves(n)
                moveReport &= m.ToString
                If n < targetMoves.Length - 1 Then moveReport &= " + "
            Next
            Debug.Print(Name & ": " & moveReport)
            MyBase.Move(targetMoves)
        End If
    End Sub
#End Region
End Class
