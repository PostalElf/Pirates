Public Class World
    Public Shared Rng As New Random(5)
    Public Shared Calendar As Calendar
    Public WorldWind As BattleDirection
    Public ShipPlayer As ShipPlayer

    Public Sub New()
        Calendar = New Calendar(1, 4, 7, 106)
    End Sub

#Region "Battlefield"
    Public Sub EnterCombat(ByVal enemies As List(Of ShipAI))
        Dim battlefield As Battlefield = battlefield.Generate(15, 15, 5, WorldWind, Rng)
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
