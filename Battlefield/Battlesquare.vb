Public Class Battlesquare
    Public Battlefield As Battlefield
    Public X As Integer
    Public Y As Integer
    Public Contents As BattlefieldObject

#Region "Pathing"
    Public ReadOnly Property PathingCost As Integer
        Get
            If Contents Is Nothing Then Return 2 Else Return Contents.PathingCost
        End Get
    End Property
#End Region

    Private Function GetShiftedSquare(ByVal xShift As Integer, ByVal yShift As Integer) As Battlesquare
        Dim pX As Integer = Dev.Constrain(X + xShift, 0, Battlefield.MaxX)
        Dim pY As Integer = Dev.Constrain(Y + yShift, 0, Battlefield.MaxY)
        Return Battlefield(pX, pY)
    End Function
    Public Function GetAdjacent(ByVal direction As BattleDirection, ByVal distance As Integer) As Battlesquare
        Select Case direction
            Case BattleDirection.North : Return GetShiftedSquare(0, -distance)
            Case BattleDirection.South : Return GetShiftedSquare(0, distance)
            Case BattleDirection.East : Return GetShiftedSquare(distance, 0)
            Case BattleDirection.West : Return GetShiftedSquare(-distance, 0)
            Case Else
                Throw New Exception("Invalid direction")
                Return Nothing
        End Select
    End Function
    Public Function GetAdjacents(ByVal direction As BattleDirection, ByVal distance As Integer) As Queue(Of Battlesquare)
        If distance <= 0 Then Return Nothing

        Dim total As New Queue(Of Battlesquare)
        For n = 1 To distance
            total.Enqueue(GetAdjacent(direction, n))
        Next
        Return total
    End Function
    Public Function GetAdjacents(ByVal distance As Integer) As List(Of Battlesquare)
        Dim total As New List(Of Battlesquare)
        For Each d As BattleDirection In [Enum].GetValues(GetType(BattleDirection))
            Dim sq As Battlesquare = GetAdjacent(d, distance)
            If sq Is Nothing = False Then total.Add(sq)
        Next
        Return total
    End Function
    Public Function GetSubjectiveAdjacent(ByVal facing As BattleDirection, ByVal quarter As ShipQuarter, ByVal distance As Integer) As Battlesquare
        'given facing, quarter and distance, return target square from current square

        Dim d As BattleDirection = GetSubjectiveDirection(facing, quarter)
        Return GetAdjacent(d, distance)
    End Function
    Public Function GetSubjectiveAdjacents(ByVal facing As BattleDirection, ByVal quarter As ShipQuarter, ByVal distance As Integer) As Queue(Of Battlesquare)
        If distance <= 0 Then Return Nothing

        Dim total As New Queue(Of Battlesquare)
        Dim d As BattleDirection = GetSubjectiveDirection(facing, quarter)
        For n = 1 To distance
            total.Enqueue(GetSubjectiveAdjacent(facing, quarter, n))
        Next
        Return total
    End Function
    Private Function GetSubjectiveDirection(ByVal facing As BattleDirection, ByVal quarter As ShipQuarter) As BattleDirection
        'given facing and quarter, get direction

        Dim f As BattleDirection = facing
        Select Case quarter
            Case ShipQuarter.Starboard : f += 1
            Case ShipQuarter.Port : f -= 1
            Case ShipQuarter.Aft : f += 2
        End Select
        If f < 1 Then f = 4
        If f > 4 Then f = 1
        Return f
    End Function
    Public Shared Function ReverseDirection(ByVal direction As BattleDirection) As BattleDirection
        For n = 1 To 2
            direction += 1
            If direction < 1 Then direction = 4
            If direction > 4 Then direction = 1
        Next
        Return direction
    End Function
    Public Function GetPathable(ByVal facing As BattleDirection, ByVal move As BattleMove) As BattlePosition
        Dim f As BattleDirection = facing
        Dim current As Battlesquare = Me

        Select Case move
            Case BattleMove.TurnLeft : f -= 1
            Case BattleMove.TurnRight : f += 1
            Case BattleMove.Forward : current = current.GetSubjectiveAdjacent(f, ShipQuarter.Fore, 1)
            Case BattleMove.Backwards : current = current.GetSubjectiveAdjacent(f, ShipQuarter.Aft, 1)
        End Select
        If f < 1 Then f = 4
        If f > 4 Then f = 1

        Return New BattlePosition(current, f, New MoveToken({move}))
    End Function
    Public Function GetPathables(ByVal facing As BattleDirection, ByVal moves As MoveToken) As BattlePosition()
        Dim f As BattleDirection = facing
        Dim current As Battlesquare = Me
        Dim total(moves.Length - 1) As BattlePosition

        For n = 0 To moves.Length - 1
            Dim move As BattleMove = moves(n)
            Select Case move
                Case BattleMove.TurnLeft : f -= 1
                Case BattleMove.TurnRight : f += 1
                Case BattleMove.Forward : current = current.GetSubjectiveAdjacent(f, ShipQuarter.Fore, 1)
                Case BattleMove.Backwards : current = current.GetSubjectiveAdjacent(f, ShipQuarter.Aft, 1)
            End Select

            If f < 1 Then f = 4
            If f > 4 Then f = 1

            Dim currentPosition As New BattlePosition(current, f, moves)
            total(n) = currentPosition
        Next

        Return total
    End Function

    Public Sub New(ByRef field As Battlefield, ByVal pX As Integer, ByVal pY As Integer)
        Battlefield = field
        X = pX
        Y = pY
    End Sub
    Public Overrides Function ToString() As String
        Return "(" & X & "," & Y & ")"
    End Function

    Public Sub ConsoleWrite()
        If Contents Is Nothing Then
            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.Write("~")
            Exit Sub
        Else
            Contents.ConsoleWrite()
        End If
    End Sub
End Class