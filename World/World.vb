Public Class World
    Public Shared Rng As New Random(5)
    Public Calendar As Calendar
    Public WorldWind As BattleDirection
    Public ShipPlayer As ShipPlayer

    Private Isles As New List(Of Isle)
    Default Public Property Item(ByVal isleName As String) As Isle
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

    Public Shared Function Generate() As World
        Dim world As New World
        With world
            .Calendar = New Calendar(Calendar.CalendarDay.Duke, 4, Calendar.CalendarSeason.Shore, 106)
            .WorldWind = BattleDirection.North

            'generate isles
            Dim free As New Isle.MapData(3, 3, 3, 3)
            .Isles.Add(Isle.Generate("Kingdom of the Dead", 1, 2, free))
            .Isles.Add(Isle.Generate("Forsworn Exclave", 2, 1, free))
            .Isles.Add(Isle.Generate("Seatouched Dominion", 3, 2, free))
            .Isles.Add(Isle.Generate("Commonwealth", 2, 3, free))
            .Isles.Add(Isle.Generate("World's Spine", 2, 2, free))

            .Isles.Add(Isle.Generate("Blasphemy Bay", 1, 1, free))
            .Isles.Add(Isle.Generate("Brass Atoll", 3, 1, free))
            .Isles.Add(Isle.Generate("Blackreef", 1, 3, free))
            .Isles.Add(Isle.Generate("Hallowsreach", 3, 3, free))

            .Isles.Add(Isle.Generate("Sanctuary", Rng.Next(1, 4), Rng.Next(1, 4), free))
            .Isles.Add(Isle.Generate("Blackiron Ridge", Rng.Next(1, 4), Rng.Next(1, 4), free))
            .Isles.Add(Isle.Generate("Coral Island", Rng.Next(1, 4), Rng.Next(1, 4), free))
            .Isles.Add(Isle.Generate("Firefalls", Rng.Next(1, 4), Rng.Next(1, 4), free))

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
        End With
        Return world
    End Function

    Public Sub Tick()
        Calendar.Tick()
        ShipPlayer.Tick(Me)
    End Sub

#Region "Battlefield"
    Public Sub EnterCombat(ByVal enemies As List(Of ShipAI))
        Dim battlefield As Battlefield = battlefield.Generate(15, 15, 5, WorldWind)
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
End Class
