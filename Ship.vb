﻿Public Class Ship
    Implements BattlefieldObject
    Public Name As String

    Public Sub New()
        AttackReady.Add(ShipQuarter.Port, True)
        AttackReady.Add(ShipQuarter.Starboard, True)
        AttackRanges(ShipQuarter.Port) = 1
        AttackRanges(ShipQuarter.Starboard) = 1

        'note: ties in heuristic distance are broken by how high the move is up in the list
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.Forward})
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.TurnLeft})
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.TurnRight})
        _AvailableMoves.Add({BattleMove.Forward})
    End Sub

#Region "Specials"
    Protected IgnoresJustTurned As Boolean = False

    Public Sub Cheaterbug()
        IgnoresJustTurned = True
    End Sub
#End Region

#Region "Movement"
    Protected JustTurned As Boolean = False
    Private _AvailableMoves As New List(Of BattleMove())
    Public ReadOnly Property AvailableMoves As List(Of BattleMove())
        Get
            If IgnoresJustTurned = False AndAlso JustTurned = True Then
                Dim total As New List(Of BattleMove())
                For Each moves In _AvailableMoves
                    If moves.Length = 1 Then total.Add(moves)
                Next
                Return total
            Else
                Return _AvailableMoves
            End If
        End Get
    End Property

    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Protected Function TurnFacing(ByVal move As BattleMove, Optional ByVal initialFacing As BattleDirection = Nothing) As BattleDirection
        Dim f As BattleDirection = Facing
        If initialfacing <> Nothing Then f = initialFacing
        Select Case move
            Case BattleMove.TurnLeft : f -= 1
            Case BattleMove.TurnRight : f += 1
        End Select
        If f < 0 Then f = 3
        If f > 3 Then f = 0
        Return f
    End Function
    Protected Function TurnFacing(ByVal moves As BattleMove(), Optional ByVal initialFacing As BattleDirection = Nothing) As BattleDirection
        Dim f As BattleDirection = Facing
        If initialFacing <> Nothing Then f = initialFacing

        For Each m In moves
            f = TurnFacing(m, f)
        Next
        Return f
    End Function
    Public ReadOnly Property PathingCost As Integer Implements BattlefieldObject.PathingCost
        Get
            Return 10
        End Get
    End Property

    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Sub Move(ByVal move As BattleMove()) Implements BattlefieldObject.Move
        Dim turn As Boolean = False
        For Each d In move
            'turn if necessary
            Facing = TurnFacing(d)
            If d = BattleMove.TurnLeft OrElse d = BattleMove.TurnRight Then turn = True

            'set targetsquare if necessary
            Dim targetSquare As Battlesquare = Nothing
            If d = BattleMove.Forward Then
                targetSquare = BattleSquare.GetAdjacent(Facing, 1)
            ElseIf d = BattleMove.Backwards Then
                targetSquare = BattleSquare.GetSubjectiveAdjacent(Facing, ShipQuarter.Aft, 1)
            End If

            'if targetsquare is ticked, move
            If targetSquare Is Nothing = False Then
                If targetSquare.Contents Is Nothing Then SetSquare(targetSquare) Else targetSquare.Contents.MovedInto(Me)
            End If
        Next

        JustTurned = turn
    End Sub
    Public Sub SetSquare(ByVal targetSquare As Battlesquare)
        If BattleSquare Is Nothing = False Then BattleSquare.Contents = Nothing
        BattleSquare = targetSquare
        BattleSquare.Contents = Me
    End Sub
    Public Sub MovedInto(ByRef ship As Ship) Implements BattlefieldObject.MovedInto
        'TODO
    End Sub
#End Region

#Region "Attack"
    Protected AttackRanges As New Dictionary(Of ShipQuarter, Integer)
    Protected AttackReady As New Dictionary(Of ShipQuarter, Boolean)
    Public Function AttackSquare(ByVal quarter As ShipQuarter) As Battlesquare
        If AttackRanges.ContainsKey(quarter) = False Then Return Nothing
        If AttackReady.ContainsKey(quarter) = False Then Return Nothing
        If AttackReady(quarter) = False Then Return Nothing

        Dim range As Integer = AttackRanges(quarter)
        Return BattleSquare.GetSubjectiveAdjacent(Facing, quarter, range)
    End Function
#End Region

#Region "Console Display"
    Public Property ConsoleColour As ConsoleColor Implements BattlefieldObject.ConsoleColour
    Public Sub ConsoleWrite() Implements BattlefieldObject.ConsoleWrite
        Console.ForegroundColor = ConsoleColour
        Select Case Facing
            Case BattleDirection.North : Console.Write("↑")
            Case BattleDirection.South : Console.Write("↓")
            Case BattleDirection.East : Console.Write("→")
            Case BattleDirection.West : Console.Write("←")
        End Select
    End Sub
#End Region
End Class
