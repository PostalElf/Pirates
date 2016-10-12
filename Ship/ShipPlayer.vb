Public Class ShipPlayer
    Inherits Ship

#Region "Move Tokens"
    Private MoveTokens As New List(Of BattleMove())
    Private MoveTokenProgress As New Dictionary(Of ShipQuarter, Integer)
    Private Const MoveTokenThreshold As Integer = 5
    Public Overrides ReadOnly Property AvailableMoves As System.Collections.Generic.List(Of BattleMove())
        Get
            If IgnoresJustTurned = False AndAlso JustTurned = True Then
                'if just turned, can only move one step
                Dim total As New List(Of BattleMove())
                For Each moves In MoveTokens
                    If moves.Length = 1 Then total.Add(moves)
                Next
                Return total
            Else
                Return MoveTokens
            End If
        End Get
    End Property

    Public Function CheckSpendMoveToken(ByVal moveToken As BattleMove())
        If MovesIndexOf(MoveTokens, moveToken) = -1 Then Return False
        Return True
    End Function
    Public Sub SpendMoveToken(ByVal moveToken As BattleMove())
        If CheckSpendMoveToken(moveToken) = False Then Exit Sub
        MoveTokens.RemoveAt(MovesIndexOf(MoveTokens, moveToken))
        Move(moveToken)
    End Sub
    Private Function MovesIndexOf(ByVal m1 As List(Of BattleMove()), ByVal m2 As BattleMove()) As Integer
        For n = 0 To m1.Count - 1
            Dim mm = m1(n)
            If MovesMatch(mm, m2) Then Return n
        Next
        Return -1
    End Function
    Private Function MovesMatch(ByVal m1 As BattleMove(), ByVal m2 As BattleMove()) As Boolean
        If m1.Length <> m2.Length Then Return False

        For n = 0 To m1.Length - 1
            If m1(n) <> m2(n) Then Return False
        Next
        Return True
    End Function
#End Region

    Public Overloads Sub EnterCombat(ByRef battlefield As Battlefield, ByRef combatantList As List(Of Ship))
        MyBase.EnterCombat(battlefield, combatantList)

        MoveTokens.Clear()
        For n = 1 To 2
            MoveTokens.Add({BattleMove.Forward})
            MoveTokens.Add({BattleMove.Forward, BattleMove.TurnLeft})
            MoveTokens.Add({BattleMove.Forward, BattleMove.TurnRight})
        Next
        MoveTokens.Add({BattleMove.Forward, BattleMove.Forward})

        MoveTokenProgress.Clear()
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            MoveTokenProgress.Add(q, 0)
        Next
    End Sub
    Public Overloads Sub Tick()
        MyBase.Tick()

        'add moveTokenProgress to gain move tokens
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            Dim sailTotal As Integer = 0
            For Each Crew In GetCrews(q, CrewSkill.Sailing)
                sailTotal += Dev.Constrain(Crew.GetSkill(CrewSkill.Sailing), 1, 10)
            Next

            MoveTokenProgress(q) += sailTotal
            While MoveTokenProgress(q) > MoveTokenThreshold
                MoveTokenProgress(q) -= MoveTokenThreshold
                Dim newMoveToken As BattleMove() = Nothing
                Select Case q
                    Case ShipQuarter.Fore : newMoveToken = {BattleMove.Forward, BattleMove.Forward}
                    Case ShipQuarter.Starboard : newMoveToken = {BattleMove.Forward, BattleMove.TurnRight}
                    Case ShipQuarter.Aft : newMoveToken = {BattleMove.Forward}
                    Case ShipQuarter.Port : newMoveToken = {BattleMove.Forward, BattleMove.TurnLeft}
                    Case Else : Throw New Exception("Unrecognised ship quarter")
                End Select
                MoveTokens.Add(newMoveToken)

                Dim rep As String = Name & " gained a new sailing token: "
                For n = 0 To newMoveToken.Length - 1
                    rep &= newMoveToken(n).ToString
                    If n <> newMoveToken.Length - 1 Then rep &= " + "
                Next
                Report.Add(rep)
            End While
        Next
    End Sub
End Class
