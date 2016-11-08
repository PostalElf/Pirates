Public Class MoveToken
    Private BattleMoves As BattleMove()
    Default Public Property Item(ByVal index As Integer)
        Get
            Return BattleMoves(index)
        End Get
        Set(ByVal value)
            BattleMoves(index) = value
        End Set
    End Property
    Public ReadOnly Property Length As Integer
        Get
            Return BattleMoves.Length
        End Get
    End Property
    Public Function IndexOf(ByVal bm As BattleMove) As Integer
        For n = 0 To Length - 1
            If BattleMoves(n) = bm Then Return n
        Next
        Return -1
    End Function
    Public Function Contains(ByVal bm As BattleMove) As Boolean
        If IndexOf(bm) = -1 Then Return False Else Return True
    End Function

    Public Sub New(ByVal bm As BattleMove())
        BattleMoves = bm
    End Sub
    Public Overrides Function ToString() As String
        If Me = {BattleMove.Forward, BattleMove.Forward} Then : Return "Full Sails"
        ElseIf Me = {BattleMove.Forward} Then : Return "Half Sails"
        ElseIf Me = {BattleMove.Forward, BattleMove.TurnLeft} Then : Return "Port"
        ElseIf Me = {BattleMove.Forward, BattleMove.TurnRight} Then : Return "Starboard"
        ElseIf Me = {BattleMove.TurnLeft} Then : Return "Hard to Port"
        ElseIf Me = {BattleMove.TurnRight} Then : Return "Hard to Starboard"
        ElseIf Me = {BattleMove.Backwards} Then : Return "Tack Aft"
        ElseIf Me = {BattleMove.Halt} Then : Return "Halt"
        Else : Return Nothing
        End If
    End Function
    Public Shared Function ConvertStringToMoveToken(ByVal value As String) As MoveToken
        Select Case value
            Case "Full Sails" : Return New MoveToken({BattleMove.Forward, BattleMove.Forward})
            Case "Half Sails" : Return New MoveToken({BattleMove.Forward})
            Case "Port" : Return New MoveToken({BattleMove.Forward, BattleMove.TurnLeft})
            Case "Starboard" : Return New MoveToken({BattleMove.Forward, BattleMove.TurnRight})
            Case "Hard to Port" : Return New MoveToken({BattleMove.TurnLeft})
            Case "Hard to Starboard" : Return New MoveToken({BattleMove.TurnRight})
            Case "Tack Aft" : Return New MoveToken({BattleMove.Backwards})
            Case "Halt" : Return New MoveToken({BattleMove.Halt})
            Case Else : Return Nothing
        End Select
    End Function

    Public Shared Operator =(ByVal mt1 As MoveToken, ByVal mt2 As MoveToken) As Boolean
        If mt1.Length <> mt2.Length Then Return False

        For n = 0 To mt1.Length - 1
            If mt1(n) <> mt2(n) Then Return False
        Next
        Return True
    End Operator
    Public Shared Operator =(ByVal mt As MoveToken, ByVal bm As BattleMove()) As Boolean
        If mt.Length <> bm.Length Then Return False

        For n = 0 To mt.Length - 1
            If mt(n) <> bm(n) Then Return False
        Next
        Return True
    End Operator
    Public Shared Operator <>(ByVal mt1 As MoveToken, ByVal mt2 As MoveToken) As Boolean
        If mt1 = mt2 Then Return False Else Return True
    End Operator
    Public Shared Operator <>(ByVal mt As MoveToken, ByVal bm As BattleMove()) As Boolean
        If mt = bm Then Return False Else Return True
    End Operator
End Class
