Public Class World
    Public Shared Rng As New Random(5)
    Public Calendar As Calendar
    Public Wind As BattleDirection
    Public ShipPlayer As ShipPlayer

    Private Isles As New List(Of Isle)
    Private SeaPoints As New List(Of MapDataPoint)
    Private SeaNames As New List(Of String)
    Default Public Property GetIsle(ByVal isleName As String) As Isle
        Get
            For Each i In Isles
                If i.Name = isleName Then Return i
            Next
            Return Nothing
        End Get
        Set(ByVal value As Isle)
            For Each i In Isles
                If i.Name = isleName Then
                    i = value
                    Exit Property
                End If
            Next
        End Set
    End Property
    Public BasicRoutes As New List(Of Route)

    Private Sub New()
        'factions
        For Each IsleFaction In [Enum].GetValues(GetType(IsleFaction))
            Reputation.Add(IsleFaction, 0)
        Next
    End Sub
    Public Shared Function Generate() As World
        Dim world As New World
        With world
            .Calendar = New Calendar(Calendar.CalendarDay.Duke, 4, Calendar.CalendarSeason.Shore, 106)
            .Wind = BattleDirection.North

            'generate isles
            Dim free As New MapData(3, 3, 3, 3)
            .Isles.Add(Isle.Generate(world, "Deathless Kingdom", WorldFaction.Deathless, 1, 2, free))
            .Isles.Add(Isle.Generate(world, "Windsworn Exclave", WorldFaction.Windsworn, 2, 1, free))
            .Isles.Add(Isle.Generate(world, "Seatouched Dominion", WorldFaction.Seatouched, 3, 2, free))
            .Isles.Add(Isle.Generate(world, "Commonwealth", WorldFaction.Commonwealth, 2, 3, free))
            .Isles.Add(Isle.Generate(world, "Court of Dust", WorldFaction.Imperial, 2, 2, free))

            .Isles.Add(Isle.Generate(world, "Blasphemy Bay", WorldFaction.Neutral, 1, 1, free))
            .Isles.Add(Isle.Generate(world, "Brass Atoll", WorldFaction.Neutral, 3, 1, free))
            .Isles.Add(Isle.Generate(world, "Blackreef", WorldFaction.Neutral, 1, 3, free))
            .Isles.Add(Isle.Generate(world, "Hallowsreach", WorldFaction.Neutral, 3, 3, free))

            .Isles.Add(Isle.Generate(world, "Sanctuary", WorldFaction.Neutral, Rng.Next(1, 4), Rng.Next(1, 4), free))
            .Isles.Add(Isle.Generate(world, "Blackiron Ridge", WorldFaction.Neutral, Rng.Next(1, 4), Rng.Next(1, 4), free))
            .Isles.Add(Isle.Generate(world, "World's Spine", WorldFaction.Neutral, Rng.Next(1, 4), Rng.Next(1, 4), free))
            .Isles.Add(Isle.Generate(world, "Firefalls", WorldFaction.Neutral, Rng.Next(1, 4), Rng.Next(1, 4), free))

            'generate basic routes
            'this ensures that all isles are reachable from all other isles, albeit terribly
            For Each Isle1 In .Isles
                For Each Isle2 In .Isles
                    If Isle1 <> Isle2 Then
                        Dim contains As Boolean = False
                        For Each Route In .BasicRoutes
                            If Route.Contains(Isle1) AndAlso Route.Contains(Isle2) Then
                                contains = True
                                Exit For
                            End If
                        Next
                        If contains = False Then .BasicRoutes.Add(New Route(Isle1, Isle2, 0))
                    End If
                Next
            Next

            'wind
            .Wind = .WindChangeChanceBase(.Calendar.Season)
        End With
        Return world
    End Function

    Public Reputation As New Dictionary(Of IsleFaction, Integer)

#Region "Tick"
    Public Sub Tick()
        Calendar.Tick()
        TickWind()
        For Each Isle In Isles
            Isle.Tick()
        Next
        ShipPlayer.Tick(Me)
    End Sub
    Private Sub TickWind()
        WindChangeChance += WindChangeChanceDaily(Calendar.Season)
        Dim roll As Integer = Rng.Next(1, 101)
        If roll <= WindChangeChance Then
            WindChangeChance = WindChangeChanceBase(Calendar.Season)
            Dim newWind As BattleDirection = Wind
            While newWind = Wind
                newWind = Rng.Next(1, 5)
            End While
            Wind = newWind
            Report.Add("The wind has changed to " & newWind.ToString & ".", ReportType.WindChange)
        End If
    End Sub
    Private WindChangeChanceBase As Integer() = {5, 1, 3, 3, 8, 10, 5, 10, 5}
    Private WindChangeChanceDaily As Integer() = {1, 1, 2, 1, 2, 3, 3, 2, 2}
    Private WindChangeChance As Integer
#End Region

#Region "Battlefield"
    Public Sub EnterCombat(ByVal enemies As List(Of ShipAI))
        Dim battlefield As Battlefield = battlefield.Generate(15, 15, 5, Wind)
        battlefield.AddCombatant(ShipPlayer, battlefield.GetRandomSquare(True, 2, Rng), Rng.Next(1, 5))
        For Each enemy In enemies
            battlefield.AddCombatant(enemy, battlefield.GetRandomSquare(True, 2, Rng), Rng.Next(1, 5))
        Next

        While battlefield.IsOver = False
            Module1.Battle(battlefield, ShipPlayer)
        End While

        If battlefield.PlayerWins = False Then End Else ShipPlayer.EndCombat()

        'loot

    End Sub
#End Region

    Public Class MapData
        Private Points As New Dictionary(Of MapDataPoint, List(Of MapDataPoint))
        Default Public Property Item(ByVal x As Integer, ByVal y As Integer) As List(Of MapDataPoint)
            Get
                Dim key As New MapDataPoint(x, y)
                Return Points(key)
            End Get
            Set(ByVal value As List(Of MapDataPoint))
                Points(New MapDataPoint(x, y)) = value
            End Set
        End Property
        Public Sub New(ByVal maxX As Integer, ByVal maxY As Integer, ByVal maxSubsectorX As Integer, ByVal maxSubsectorY As Integer)
            For aX = 1 To maxX
                For aY = 1 To maxY
                    Dim key As New MapDataPoint(aX, aY)
                    Points.Add(key, New List(Of MapDataPoint))
                    For pX = 1 To maxSubsectorX
                        For pY = 1 To maxSubsectorY
                            Points(key).Add(New MapDataPoint(pX, pY))
                        Next
                    Next
                Next
            Next
        End Sub
        Public Sub Add(ByVal sector As MapDataPoint, ByVal subsector As MapDataPoint)
            Points(sector).Add(subsector)
        End Sub
        Public Sub Remove(ByVal x As Integer, ByVal y As Integer)
            Dim selected As MapDataPoint = Nothing
            For Each plist In Points.Values
                For Each p In plist
                    If p.X = x AndAlso p.Y = y Then selected = p
                Next
                If selected.X > 0 Then
                    plist.Remove(selected)
                    Exit Sub
                End If
            Next
        End Sub
        Public Function Roll(ByVal sectorX As Integer, ByVal sectorY As Integer, ByRef rng As Random) As MapDataPoint
            Dim plist As List(Of MapDataPoint) = Points(New MapDataPoint(sectorX, sectorY))
            Return Dev.GrabRandom(Of MapDataPoint)(plist, rng)
        End Function
        Public Function Grab(ByVal sectorX As Integer, ByVal sectorY As Integer, ByRef rng As Random) As MapDataPoint
            Dim plist As List(Of MapDataPoint) = Points(New MapDataPoint(sectorX, sectorY))
            Return Dev.GrabRandom(Of MapDataPoint)(plist, rng)
        End Function
    End Class
    Public Structure MapDataPoint
        Public X As Integer
        Public Y As Integer
        Public Sub New(ByVal aX As Integer, ByVal aY As Integer)
            X = aX
            Y = aY
        End Sub
        Public Overrides Function ToString() As String
            Return "(" & X & ", " & Y & ")"
        End Function
        Public Shared Operator =(ByVal p1 As MapDataPoint, ByVal p2 As MapDataPoint)
            If p1.X = p2.X AndAlso p1.Y = p2.Y Then Return True Else Return False
        End Operator
        Public Shared Operator <>(ByVal p1 As MapDataPoint, ByVal p2 As MapDataPoint)
            If p1 = p2 Then Return False Else Return True
        End Operator
    End Structure
End Class
