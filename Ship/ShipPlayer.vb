Public Class ShipPlayer
    Inherits Ship

#Region "Specials"
    Protected IgnoresMoveTokens As Boolean = False
    Public Overloads Sub Cheaterbug()
        MyBase.Cheaterbug()
        IgnoresMoveTokens = True
    End Sub
#End Region

#Region "Move Tokens"
    Private MoveTokens As New List(Of BattleMove())
    Private MoveTokenProgress As New Dictionary(Of ShipQuarter, Integer)
    Private AdvancedMoveTokenProgress As New Dictionary(Of ShipQuarter, Integer)
    Private Const MoveTokenThreshold As Integer = 5
    Private Const AdvancedMoveTokenThreshold As Integer = 10
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

    Public Function CheckSpendMoveToken(ByVal moveToken As BattleMove()) As Boolean
        If IgnoresMoveTokens = True Then Return True
        If MovesIndexOf(MoveTokens, moveToken) = -1 Then Return False
        Return True
    End Function
    Public Sub SpendMoveToken(ByVal moveToken As BattleMove())
        If CheckSpendMoveToken(moveToken) = False Then Exit Sub
        If IgnoresMoveTokens = False Then MoveTokens.RemoveAt(MovesIndexOf(MoveTokens, moveToken))
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
    Private Sub GainMoveTokens()
        Dim bf As Battlefield = BattleSquare.Battlefield
        Dim newMoves As New List(Of BattleMove())

        'generate wind progress
        If Facing = bf.Wind Then MoveTokenProgress(ShipQuarter.Aft) += Battlefield.WindMoveTokenProgress

        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            Dim sailTotal As Integer = 0
            Dim advancedSailTotal As Integer = 0
            For Each Crew In GetCrews(q, CrewSkill.Sailing)
                Dim skill As Integer = Crew.GetSkill(CrewSkill.Sailing)
                sailTotal += Dev.Constrain(skill, 1, 10)
                If skill >= 3 Then advancedSailTotal += Dev.Constrain(skill, 1, 10)
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
                If newMoveToken Is Nothing = False Then newMoves.Add(newMoveToken)
            End While

            AdvancedMoveTokenProgress(q) += advancedSailTotal
            While AdvancedMoveTokenProgress(q) > AdvancedMoveTokenThreshold
                AdvancedMoveTokenProgress(q) -= AdvancedMoveTokenThreshold
                Dim newMoveToken As BattleMove() = Nothing
                Select Case q
                    Case ShipQuarter.Fore : newMoveToken = {BattleMove.Forward, BattleMove.Forward}
                    Case ShipQuarter.Starboard : newMoveToken = {BattleMove.TurnRight}
                    Case ShipQuarter.Aft : newMoveToken = {BattleMove.Backwards}
                    Case ShipQuarter.Port : newMoveToken = {BattleMove.TurnLeft}
                    Case Else : Throw New Exception("Unrecognised ship quarter")
                End Select
                If newMoveToken Is Nothing = False Then newMoves.Add(newMoveToken)
            End While
        Next

        For Each newMoveToken In newMoves
            MoveTokens.Add(newMoveToken)

            Dim rep As String = Name & " gained a new sailing token: " & GetMoveTokenString(newMoveToken)
            Report.Add(rep)
        Next
    End Sub
    Private Function GetMoveTokenString(ByVal movetoken As BattleMove()) As String
        If MovesMatch(movetoken, {BattleMove.Forward, BattleMove.Forward}) Then : Return "Full Sails"
        ElseIf MovesMatch(movetoken, {BattleMove.Forward}) Then : Return "Half Sails"
        ElseIf MovesMatch(movetoken, {BattleMove.Forward, BattleMove.TurnLeft}) Then : Return "Port"
        ElseIf MovesMatch(movetoken, {BattleMove.Forward, BattleMove.TurnRight}) Then : Return "Starboard"
        ElseIf MovesMatch(movetoken, {BattleMove.TurnLeft}) Then : Return "Hard to Port"
        ElseIf MovesMatch(movetoken, {BattleMove.TurnRight}) Then : Return "Hard to Starboard"
        ElseIf MovesMatch(movetoken, {BattleMove.Backwards}) Then : Return "Tack Aft"
        Else : Return Nothing
        End If
    End Function
#End Region

#Region "Commands"
    Private Commands As New List(Of Command)
    Public Sub AddCommand(ByVal type As String, ByVal target As Object, ByVal destination As Object, Optional ByVal secondary As Object = Nothing)
        Commands.Add(New Command(type, target, destination, secondary))
    End Sub
    Private Sub RunCommands()
        While Commands.Count > 0
            Dim command As Command = Commands(0)
            With command
                Select Case .Type
                    Case "Move"
                        Dim crew As Crew = CType(.Target, Crew)
                        Dim quarter As ShipQuarter = CType(.Destination, ShipQuarter)
                        Dim role As CrewSkill = CType(.Secondary, CrewSkill)
                        crew.Move(quarter, role)
                End Select
            End With
            Commands.RemoveAt(0)
        End While
    End Sub

    Public Class Command
        Public Type As String
        Public Target As Object
        Public Destination As Object
        Public Secondary As Object

        Public Sub New(ByVal aType As String, ByVal aTarget As Object, ByVal aDestination As Object, Optional ByVal aSecondary As Object = Nothing)
            Type = aType
            Target = aTarget
            Destination = aDestination
            Secondary = aSecondary
        End Sub
    End Class
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
        AdvancedMoveTokenProgress.Clear()
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            MoveTokenProgress.Add(q, 0)
            AdvancedMoveTokenProgress.Add(q, 0)
        Next
    End Sub
    Public Overloads Sub Tick()
        MyBase.Tick()

        GainMoveTokens()
        RunCommands()
    End Sub
End Class
