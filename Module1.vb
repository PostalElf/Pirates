Module Module1

    Sub Main()
        Battle()
    End Sub

    Private Sub Battle()
        Dim rng As New Random
        Dim battlefield As Battlefield = SetupBattlefield(rng)
        Dim playerShip As Ship = Nothing
        For Each Ship In battlefield.Combatants
            If TypeOf Ship Is ShipPlayer Then playerShip = Ship
        Next

        Dim SkipAiTurn As Boolean = False
        While True
            Console.Clear()
            battlefield.ConsoleWrite()
            Console.WriteLine()
            Report.ConsoleReport()
            Console.WriteLine()

            PlayerInput(playerShip, SkipAiTurn)
            For Each combatant In battlefield.Combatants
                If combatant.InMelee = True Then Continue For

                If TypeOf combatant Is ShipAI AndAlso SkipAiTurn = False Then
                    CType(combatant, ShipAI).Tick(playerShip)
                Else
                    combatant.Tick()
                End If
            Next

            For Each Melee In battlefield.Melees
                Melee.tick()
            Next

            battlefield.CleanUp()
        End While
    End Sub
    Private Function SetupBattlefield(ByRef rng As Random) As Battlefield
        Dim battlefield As Battlefield = battlefield.Generate(10, 10, 2)
        Dim cannon As New ShipWeapon("Cannons", 3, DamageType.Cannon, 1, 1, 3)

        Dim ship As New ShipPlayer
        With ship
            .ConsoleColour = ConsoleColor.White
            .Facing = BattleDirection.East
            .Name = "Barron's Spear"
            .AddWeapon(ShipQuarter.Port, ShipWeapon.Clone(cannon))
            .AddWeapon(ShipQuarter.Starboard, ShipWeapon.Clone(cannon))
            .AddCrew(ShipQuarter.Port, Crew.Generate(Crew.CrewRace.Human, rng))
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(Crew.CrewRace.Human, rng))
            .SetSquare(battlefield(5, 5))
            .Cheaterbug()
        End With
        battlefield.Combatants.Add(ship)

        Dim ai1 As New ShipAI
        With ai1
            .ConsoleColour = ConsoleColor.Green
            .Facing = BattleDirection.East
            .Name = "Her Majesty's Rook"
            .AddWeapon(ShipQuarter.Port, ShipWeapon.Clone(cannon))
            .AddWeapon(ShipQuarter.Starboard, ShipWeapon.Clone(cannon))
            .AddCrew(ShipQuarter.Port, Crew.Generate(Crew.CrewRace.Human, rng))
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(Crew.CrewRace.Human, rng))
            .SetSquare(battlefield(1, 1))
        End With
        battlefield.Combatants.Add(ai1)

        Dim ai2 As New ShipAI
        With ai2
            .ConsoleColour = ConsoleColor.Green
            .Facing = BattleDirection.North
            .Name = "His Lordship's Mistress"
            .AddWeapon(ShipQuarter.Port, ShipWeapon.Clone(cannon))
            .AddWeapon(ShipQuarter.Starboard, ShipWeapon.Clone(cannon))
            .AddCrew(ShipQuarter.Port, Crew.Generate(Crew.CrewRace.Human, rng))
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(Crew.CrewRace.Human, rng))
            .SetSquare(battlefield(2, 2))
        End With
        battlefield.Combatants.Add(ai2)

        Return battlefield
    End Function
    Private Sub PlayerInput(ByRef ship As Ship, ByRef SkipAiTurn As Boolean)
        Dim targetMove As BattleMove() = Nothing
        Dim input As ConsoleKeyInfo = Console.ReadKey()
        Select Case input.Key
            Case ConsoleKey.NumPad3, ConsoleKey.L : GetPlayerAttack(ship, ShipQuarter.Starboard)
            Case ConsoleKey.NumPad1, ConsoleKey.J : GetPlayerAttack(ship, ShipQuarter.Port)
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
    Private Sub GetPlayerAttack(ByRef ship As Ship, ByVal quarter As ShipQuarter)
        Dim weaponList As List(Of ShipWeapon) = ship.GetWeapons(quarter)
        If weaponList.Count = 0 Then Exit Sub

        Dim target As ShipWeapon = Nothing
        If weaponList.Count = 1 Then target = weaponList(0) Else target = Menu.getListChoice(weaponList, 0, vbCrLf & "Select weapon:")
        If target Is Nothing Then Exit Sub
        ship.Attack(quarter, target)
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
