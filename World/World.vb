Public Class World
    Private Rng As Random
    Public WorldWind As BattleDirection
    Public ShipPlayer As ShipPlayer

    Public Sub New(ByVal aRng As Random)
        Rng = aRng
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
    End Sub
#End Region
End Class
