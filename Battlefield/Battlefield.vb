Public Class Battlefield
    Private _MaxX As Integer
    Private _MaxY As Integer
    Public ReadOnly Property MaxX As Integer
        Get
            Return _MaxX
        End Get
    End Property
    Public ReadOnly Property MaxY As Integer
        Get
            Return _MaxY
        End Get
    End Property
    Private Squares As Battlesquare(,)
    Default Public Property Square(ByVal x As Integer, ByVal y As Integer) As Battlesquare
        Get
            If CheckBounds(x, y) = False Then Return Nothing
            Return Squares(x, y)
        End Get
        Set(ByVal value As Battlesquare)
            If CheckBounds(x, y) = False Then Exit Property
            Squares(x, y) = value
        End Set
    End Property
    Public Function RandomSquare(ByVal isEmpty As Boolean, Optional ByRef rng As Random = Nothing) As Battlesquare
        If rng Is Nothing Then rng = New Random
        If isEmpty = False Then
            Dim X As Integer = rng.Next(0, MaxX + 1)
            Dim Y As Integer = rng.Next(0, MaxY + 1)
            Return Square(X, Y)
        Else
            Dim sl As List(Of Battlesquare) = EmptySquares
            Dim roll As Integer = rng.Next(0, sl.Count)
            Return sl(roll)
        End If
    End Function
    Private ReadOnly Property EmptySquares As List(Of Battlesquare)
        Get
            Dim total As New List(Of Battlesquare)
            For x = 0 To MaxX
                For y = 0 To MaxY
                    Dim s As Battlesquare = Square(x, y)
                    If s.Contents Is Nothing Then total.Add(s)
                Next
            Next
            Return total
        End Get
    End Property

    Public Sub New(ByVal X As Integer, ByVal Y As Integer)
        _MaxX = X - 1
        _MaxY = Y - 1
        ReDim Squares(MaxX, MaxY)
        For nX = 0 To MaxX
            For nY = 0 To MaxY
                Squares(nX, nY) = New Battlesquare(Me, nX, nY)
            Next
        Next
    End Sub
    Private Shared BattlefieldObjects As Type() = {GetType(BF_Rock), GetType(BF_Tides)}
    Public Shared Function Generate(ByVal X As Integer, ByVal Y As Integer, ByVal featureDensity As Integer, Optional ByVal aWind As BattleDirection = Nothing) As Battlefield
        Dim rng As New Random(5)
        Dim battlefield As New Battlefield(X, Y)
        For n = 0 To featureDensity - 1
            Dim s As Battlesquare = battlefield.RandomSquare(True, rng)
            GenerateFeature(s, rng)
        Next
        If aWind = Nothing Then battlefield.Wind = rng.Next(1, 5)
        Return battlefield
    End Function
    Private Shared Function GenerateFeature(ByRef square As Battlesquare, Optional ByRef rng As Random = Nothing)
        Dim roll As Integer = rng.Next(0, BattlefieldObjects.Length)
        Dim type As Type = BattlefieldObjects(roll)
        Dim bfo As BattlefieldObject = Nothing
        Select Case type
            Case GetType(BF_Rock) : bfo = New BF_Rock
            Case GetType(BF_Tides) : bfo = New BF_Tides(rng.Next(0, 4))
        End Select

        If bfo Is Nothing = False Then
            square.Contents = bfo
            bfo.BattleSquare = square
            Return bfo
        End If
        Return Nothing
    End Function
    Private Function CheckBounds(ByVal x As Integer, ByVal y As Integer) As Boolean
        If x > MaxX OrElse x < 0 Then Return False
        If y > MaxY OrElse y < 0 Then Return False
        Return True
    End Function

    Public IsOver As Boolean
    Public PlayerWins As Boolean

    Public Wind As BattleDirection
    Public Const WindMoveTokenProgress = 3

    Public Sub ConsoleWrite()
        For y = 0 To MaxY
            For x = 0 To MaxX
                Dim s As Battlesquare = Square(x, y)
                s.ConsoleWrite()
                Console.Write(" ")
            Next
            Console.WriteLine()
        Next
    End Sub
    Public Sub ConsoleReportCombatants()
        For Each combatant In Combatants
            If TypeOf combatant Is Ship Then
                Dim ship As Ship = CType(combatant, Ship)
                ship.ConsoleReport()
            End If
            Console.WriteLine()
        Next
    End Sub

    Private Combatants As New List(Of Ship)
    Public PlayerShip As ShipPlayer
    Private Melees As New List(Of Melee)
    Public Sub AddCombatant(ByRef combatant As Ship, ByVal x As Integer, ByVal y As Integer, ByVal facing As BattleDirection)
        combatant.Facing = facing
        combatant.SetSquare(Square(x, y))
        Select Case combatant.GetType
            Case GetType(ShipPlayer)
                CType(combatant, ShipPlayer).EnterCombat(Me, Combatants)
                PlayerShip = combatant

            Case GetType(ShipAI)
                CType(combatant, ShipAI).EnterCombat(Me, Combatants)

            Case Else
                combatant.EnterCombat(Me, Combatants)
        End Select
    End Sub
    Public Sub AddMelee(ByVal melee As Melee)
        melee.Battlefield = Me
        Melees.Add(melee)
    End Sub
    Public Sub CombatTick(ByVal AITurn As Boolean)
        If AITurn = True Then
            For Each combatant In Combatants
                If combatant.InMelee = True Then Continue For
                If TypeOf combatant Is ShipAI Then : CType(combatant, ShipAI).CombatTick(PlayerShip)
                ElseIf TypeOf combatant Is ShipPlayer Then : CType(combatant, ShipPlayer).CombatTick()
                Else : combatant.CombatTick()
                End If
            Next

            For Each Melee In Melees
                Melee.CombatTick()
            Next
        End If

        CleanUp()
    End Sub

#Region "Death"
    Private DeadCrew As New List(Of Crew)
    Private DeadObjects As New List(Of BattlefieldObject)
    Public Sub AddDead(ByVal bfo As BattlefieldObject)
        If DeadObjects.Contains(bfo) = False Then DeadObjects.Add(bfo)
    End Sub
    Public Sub AddDead(ByVal crew As Crew)
        If DeadCrew.Contains(crew) = False Then DeadCrew.Add(crew)
    End Sub
    Private Sub CleanUp()
        CleanDeadCrew()
        CleanDeadObjects()
        CleanDeadMelees()

        Dim player As ShipPlayer = Nothing
        Dim aiCount As Integer = 0
        For Each combatant In Combatants
            If TypeOf combatant Is ShipPlayer Then player = combatant
            If TypeOf combatant Is ShipAI Then aiCount += 1
        Next
        If player Is Nothing Then
            'player dead
            IsOver = True
            PlayerWins = False
        ElseIf aiCount = 0 Then
            'player wins
            IsOver = True
            PlayerWins = True
        End If
    End Sub
    Private Sub CleanDeadObjects()
        For n = DeadObjects.Count - 1 To 0 Step -1
            If DeadObjects(n).BattleSquare Is Nothing = False Then
                DeadObjects(n).BattleSquare.Contents = Nothing
                DeadObjects(n).BattleSquare = Nothing
            End If

            If TypeOf DeadObjects(n) Is Ship Then
                Dim ship As Ship = CType(DeadObjects(n), Ship)
                For Each Melee In Melees
                    If Melee.Contains(ship) Then Melee.Lose(ship)
                Next
                Combatants.Remove(ship)
                For Each q In [Enum].GetValues(GetType(ShipQuarter))
                    For Each Crew In ship.GetCrews(q, Nothing)
                        DeadCrew.Add(Crew)
                        Crew.Ship = Nothing
                    Next
                Next
            End If

            DeadObjects(n) = Nothing
            DeadObjects.RemoveAt(n)
        Next
    End Sub
    Private Sub CleanDeadCrew()
        For n = DeadCrew.Count - 1 To 0 Step -1
            If DeadCrew(n).Ship Is Nothing = False Then
                DeadCrew(n).Ship.RemoveCrew(DeadCrew(n))
            End If
            For Each Melee In Melees
                If Melee.Contains(DeadCrew(n)) Then Melee.remove(DeadCrew(n))
            Next
            DeadCrew(n) = Nothing
            DeadCrew.RemoveAt(n)
        Next
    End Sub
    Private Sub CleanDeadMelees()
        For n = Melees.Count - 1 To 0 Step -1
            If Melees(n).IsOver = True Then
                Melees(n).Battlefield = Nothing
                Melees(n) = Nothing
                Melees.RemoveAt(n)
            End If
        Next
    End Sub
#End Region
End Class