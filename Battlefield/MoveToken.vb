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

    Public Sub New(ByVal bm As BattleMove())
        BattleMoves = bm
    End Sub

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
