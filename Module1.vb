Module Module1
    Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))

    Sub Main()
        Console.SetWindowSize(Console.WindowWidth, 50)
        Battle()
    End Sub

    Private Sub Battle()
        Dim rng As New Random(5)
        Dim battlefield As Battlefield = SetupBattlefield(rng)
        Dim playerShip As ShipPlayer = battlefield.playership

        While battlefield.IsOver = False
            Console.Clear()
            battlefield.ConsoleWrite()
            Console.WriteLine()
            Report.ConsoleReport()
            Console.WriteLine()

            Dim AITurn As Boolean = PlayerInput(playerShip, battlefield)
            battlefield.Tick(AITurn)
        End While

        Console.Clear()
        battlefield.ConsoleWrite()
        Console.WriteLine()
        Report.ConsoleReport()
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("WINNER WINNER CHICKEN DINNER")
        Console.ReadKey()
    End Sub
    Private Function SetupBattlefield(ByRef rng As Random) As Battlefield
        Dim battlefield As Battlefield = battlefield.Generate(15, 15, 0)
        Dim hooks As New ShipWeapon("Grappling Hooks", 0, 0, DamageType.Cannon, 1, GoodType.Grapples, 5, 2, 5)
        Dim cannon As New ShipWeapon("Cannons", 30, 10, DamageType.Cannon, 2, GoodType.Shot, 3, 1, 3)
        Dim grapeshot As New ShipWeapon("Grapeshot", 10, 25, DamageType.Firearms, 1, GoodType.Grapeshot, 5, 2, 5)

        Dim ship As New ShipPlayer
        With ship
            .ConsoleColour = ConsoleColor.Cyan
            .Name = "Baron's Spear"
            .AddWeapon(ShipQuarter.Port, cannon.Clone)
            .AddWeapon(ShipQuarter.Starboard, grapeshot.Clone)
            .AddWeapon(ShipQuarter.Port, hooks.Clone)
            .AddWeapon(ShipQuarter.Starboard, hooks.Clone)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewRole.Gunner)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewRole.Gunner)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewRole.Gunner)
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewRole.Sailor)
            .Cheaterbug(True, True, True, True)
        End With
        battlefield.AddCombatant(ship, 5, 5, BattleDirection.East)

        Dim ai1 As ShipAI = ShipAI.Generate(ShipType.Sloop, Nothing, Nothing, rng)
        ai1.ConsoleColour = ConsoleColor.Green
        'ai1.Cheaterbug(True, True, False, False)
        battlefield.AddCombatant(ai1, 1, 1, BattleDirection.East)

        Dim ai2 As ShipAI = ShipAI.Generate(ShipType.Sloop, Nothing, Nothing, rng)
        ai2.ConsoleColour = ConsoleColor.Green
        ai2.Cheaterbug(True, True, False, False)
        battlefield.AddCombatant(ai2, 8, 8, BattleDirection.West)

        Return battlefield
    End Function
    Private Function PlayerInput(ByRef ship As ShipPlayer, ByRef battlefield As Battlefield) As Boolean
        'return true when player ends turn

        Dim targetMove As MoveToken = Nothing
        Console.ResetColor()
        Dim input As ConsoleKeyInfo = Console.ReadKey()
        Select Case input.Key
            Case ConsoleKey.NumPad3, ConsoleKey.L : GetPlayerAttack(ship, ShipQuarter.Starboard)
            Case ConsoleKey.NumPad1, ConsoleKey.J : GetPlayerAttack(ship, ShipQuarter.Port)
            Case ConsoleKey.NumPad8, ConsoleKey.I : targetMove = New MoveToken({BattleMove.Forward, BattleMove.Forward})
            Case ConsoleKey.NumPad5, ConsoleKey.K : targetMove = New MoveToken({BattleMove.Forward})
            Case ConsoleKey.NumPad9, ConsoleKey.O : targetMove = New MoveToken({BattleMove.Forward, BattleMove.TurnRight})
            Case ConsoleKey.NumPad7, ConsoleKey.U : targetMove = New MoveToken({BattleMove.Forward, BattleMove.TurnLeft})
            Case ConsoleKey.NumPad4 : targetMove = New MoveToken({BattleMove.TurnLeft})
            Case ConsoleKey.NumPad6 : targetMove = New MoveToken({BattleMove.TurnRight})
            Case ConsoleKey.NumPad2, ConsoleKey.OemComma : targetMove = New MoveToken({BattleMove.Backwards})
            Case ConsoleKey.V
                ViewBattlefield(battlefield)
                Return False
            Case ConsoleKey.C
                viewSelf(ship)
                Return False
            Case ConsoleKey.Spacebar : Return True
            Case ConsoleKey.Escape : End
        End Select

        If ship.InMelee = True Then Return True

        If targetMove Is Nothing = False AndAlso ship.CheckSpendMoveToken(targetMove) = True Then
            ship.SpendMoveToken(targetMove)
            Return True
        End If
        Return False
    End Function
    Private Sub ViewBattlefield(ByVal battlefield As Battlefield)
        Console.WriteLine()
        battlefield.consoleReportCombatants()
        Console.ReadKey()
    End Sub
    Private Sub ViewSelf(ByVal ship As ShipPlayer)
        Dim t As Integer = 12
        Dim s As String = Dev.vbSpace(1)
        Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))
        Console.WriteLine()

        For Each role In [Enum].GetValues(GetType(CrewRole))
            Dim crews As New List(Of Crew)
            For Each q In quarters
                Dim cList As List(Of Crew) = ship.GetCrews(q, role)
                If cList.Count > 0 Then crews.AddRange(cList)
            Next

            If crews.Count > 0 Then
                Console.WriteLine(role.ToString & ":")
                For Each Crew In crews
                    Console.WriteLine(s & Dev.vbTab(Crew.ShipQuarter.ToString & ":", t) & Crew.Name & " (" & Crew.GetSkillFromRole() & ")")
                Next
                Console.WriteLine()
            End If
        Next
        Console.WriteLine("Weapons:")
        For Each q In quarters
            For Each weapon In ship.GetWeapons(q)
                With weapon
                    Console.Write(s & Dev.vbTab(q.ToString & ":", t) & weapon.ToString & " - ")
                    If .CooldownCounter <= 0 Then Console.Write("OK") Else Console.Write("Reloading in " & .CooldownCounter)
                    Console.WriteLine()
                End With
            Next
        Next
        Console.WriteLine()

        Select Case Menu.getListChoice(New List(Of String) From {"Move Crew", "Examine Crew"}, 0)
            Case "Move Crew" : MoveCrew(ship)
            Case "Examine Crew" : ExamineCrew(ship)
            Case Else : Exit Sub
        End Select
    End Sub
    Private Sub MoveCrew(ByRef ship As ShipPlayer)
        Dim target As Crew = GetCrew(ship)
        Dim destination As ShipQuarter = Menu.getListChoice(Of ShipQuarter)(quarters, 0, "To where?")
        Dim newrole As CrewRole = Menu.getListChoice(ship.GetAvailableRoles(destination), 0, "Select new role:")
        ship.AddCommand("Move", target, destination, newrole)
    End Sub
    Private Sub ExamineCrew(ByRef ship As ShipPlayer)
        Dim target As Crew = GetCrew(ship)
        Console.WriteLine()
        target.ConsoleReport()
        Console.WriteLine()
        Console.ReadKey()
    End Sub
    Private Function GetCrew(ByRef ship As ShipPlayer) As Crew
        Dim roles As New List(Of CrewRole)([Enum].GetValues(GetType(CrewRole)))
        Dim role As CrewRole = Menu.getListChoice(Of CrewRole)(roles, 0, "From which role?")
        Dim choiceList As New List(Of Crew)
        For Each q In quarters
            choiceList.AddRange(ship.GetCrews(q, role))
        Next

        Dim target As Crew = Menu.getListChoice(choiceList, 0, "Which crew member?")
        Return target
    End Function
    Private Sub GetPlayerAttack(ByRef ship As Ship, ByVal quarter As ShipQuarter)
        Dim weaponList As List(Of ShipWeapon) = ship.GetWeapons(quarter)
        If weaponList.Count = 0 Then Exit Sub

        Dim target As ShipWeapon = Nothing
        If weaponList.Count = 1 Then target = weaponList(0) Else target = Menu.getListChoice(weaponList, 0, vbCrLf & "Select weapon:")
        If target Is Nothing Then Exit Sub
        ship.Attack(target)
    End Sub
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
        battlefield.AddDead(target)
    End Sub
End Module
