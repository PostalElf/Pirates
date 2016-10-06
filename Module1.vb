Module Module1

    Sub Main()
        Dim battlefield As Battlefield = SetupBattlefield()
        Dim playerShip As Ship = Nothing
        For Each Ship In battlefield.Combatants
            If TypeOf Ship Is ShipPlayer Then playerShip = Ship
        Next

        Dim SkipAiTurn As Boolean = False
        While True
            Console.Clear()
            battlefield.ConsoleWrite()
            Console.WriteLine()

            PlayerInput(playerShip, SkipAiTurn)
            For Each combatant In battlefield.Combatants
                If TypeOf combatant Is ShipAI AndAlso SkipAiTurn = False Then
                    Dim aiShip As ShipAI = CType(combatant, ShipAI)
                    aiShip.PrimitiveRouting(playerShip.BattleSquare)
                End If
            Next

            battlefield.CleanDeadObjects()
        End While
    End Sub
    Private Function SetupBattlefield() As Battlefield
        Dim battlefield As Battlefield = battlefield.Generate(10, 10, 2)

        Dim ship As New ShipPlayer
        With ship
            .ConsoleColour = ConsoleColor.White
            .Facing = BattleDirection.East
            .Weapons(ShipQuarter.Port) = New ShipWeapon("Cannons", 3, DamageType.Cannon, 1)
            .Weapons(ShipQuarter.Starboard) = New ShipWeapon("Cannons", 3, DamageType.Cannon, 1)
            .SetSquare(battlefield(5, 5))
            .Cheaterbug()
        End With
        battlefield.Combatants.Add(ship)

        Dim ai1 As New ShipAI
        With ai1
            .ConsoleColour = ConsoleColor.Green
            .Facing = BattleDirection.East
            .Name = "AI1"
            .SetSquare(battlefield(1, 1))
        End With
        battlefield.Combatants.Add(ai1)

        Dim ai2 As New ShipAI
        With ai2
            .ConsoleColour = ConsoleColor.Green
            .Facing = BattleDirection.North
            .Name = "AI2"
            .SetSquare(battlefield(2, 2))
        End With
        battlefield.Combatants.Add(ai2)

        Return battlefield
    End Function
    Private Sub PlayerInput(ByRef ship As Ship, ByRef SkipAiTurn As Boolean)
        Dim targetMove As BattleMove() = Nothing
        Dim input As ConsoleKeyInfo = Console.ReadKey()
        Select Case input.Key
            Case ConsoleKey.NumPad3, ConsoleKey.L : ship.Attack(ShipQuarter.Starboard)
            Case ConsoleKey.NumPad1, ConsoleKey.J : ship.Attack(ShipQuarter.Port)
            Case ConsoleKey.NumPad8, ConsoleKey.I : targetMove = {BattleMove.Forward, BattleMove.Forward}
            Case ConsoleKey.NumPad5, ConsoleKey.K : targetMove = {BattleMove.Forward}
            Case ConsoleKey.NumPad9, ConsoleKey.O : targetMove = {BattleMove.Forward, BattleMove.TurnRight}
            Case ConsoleKey.NumPad7, ConsoleKey.U : targetMove = {BattleMove.Forward, BattleMove.TurnLeft}
            Case ConsoleKey.NumPad4 : targetMove = {BattleMove.TurnLeft}
            Case ConsoleKey.NumPad6 : targetMove = {BattleMove.TurnRight}
            Case ConsoleKey.NumPad2, ConsoleKey.OemComma : targetMove = {BattleMove.Backwards}
            Case ConsoleKey.Spacebar
                SkipAiTurn = False
                Exit Sub
            Case ConsoleKey.Escape : End
        End Select

        If targetMove Is Nothing = False Then
            If MovesContain(ship.AvailableMoves, targetMove) Then
                ship.Move(targetMove)
                SkipAiTurn = False
            Else
                SkipAiTurn = True
            End If
        Else
            SkipAiTurn = True
        End If
    End Sub
    Private Function MovesContain(ByVal m1 As List(Of BattleMove()), ByVal m2 As BattleMove()) As Boolean
        For Each mm In m1
            If MovesMatch(mm, m2) Then Return True
        Next
        Return False
    End Function
    Private Function MovesMatch(ByVal m1 As BattleMove(), ByVal m2 As BattleMove()) As Boolean
        If m1.Length <> m2.Length Then Return False

        For n = 0 To m1.Length - 1
            If m1(n) <> m2(n) Then Return False
        Next
        Return True
    End Function
    Private Sub TestSnippet(ByVal battlefield As Battlefield)
        Dim targetSquare As Battlesquare = battlefield(4, 4)
        Dim target As New ShipPlayer
        With target
            .SetSquare(targetSquare)
        End With

        For Each facing In [Enum].GetValues(GetType(BattleDirection))
            target.Facing = facing
            Console.WriteLine("Facing " & facing.ToString)
            For Each direction In [Enum].GetValues(GetType(BattleDirection))
                Dim t As BattlefieldObject = target
                Dim targetQuarter As ShipQuarter = t.GetTargetQuarter(direction)
                Console.WriteLine("  " & direction.ToString & ": " & targetQuarter.ToString)
            Next
            Console.WriteLine()
        Next

        Console.ReadLine()
        battlefield.DeadObjects.Add(target)
    End Sub
End Module
