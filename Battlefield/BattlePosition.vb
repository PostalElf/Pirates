Public Class BattlePosition
    Public Square As Battlesquare
    Public Facing As BattleDirection
    Public ParentMove As MoveToken
    Public PathingParent As BattlePosition

    Public Sub New(ByVal sq As Battlesquare, ByVal f As BattleDirection, ByVal moves As MoveToken)
        Square = sq
        Facing = f
        ParentMove = moves
    End Sub
    Public Overrides Function ToString() As String
        Return "(" & Square.X & "," & Square.Y & ")"
    End Function
End Class
