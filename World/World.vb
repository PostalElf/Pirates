Public Class World
    Public Shared Rng As New Random(5)
    Public Calendar As Calendar
    Public WorldWind As BattleDirection
    Public ShipPlayer As ShipPlayer

    Private Isles As New List(Of Isle)

    Public Shared Function Generate() As World
        Dim world As New World
        With world
            .Calendar = New Calendar(Calendar.CalendarDay.Duke, 4, Calendar.CalendarSeason.Shore, 106)
            .WorldWind = BattleDirection.North

            'generate isles
            Dim free As New Isle.MapData(3, 3, 3, 3)
            .Isles.Add(Isle.Generate("Kingdom of the Dead", 2, 1, free))
            .Isles.Add(Isle.Generate("Forsworn Islands", 1, 2, free))
            .Isles.Add(Isle.Generate("Seatouched Dominion", 3, 2, free))
            .Isles.Add(Isle.Generate("Commonwealth", 2, 3, free))
        End With
        Return world
    End Function

    Public Sub Tick()
        Calendar.Tick()
        ShipPlayer.Tick()
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
