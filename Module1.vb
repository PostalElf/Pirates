Module Module1
    Dim world As World
    Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))

    Sub Main()
        Console.SetWindowSize(Console.WindowWidth, 50)
        world = New World
        world.ShipPlayer = SetupPlayerShip(world.Rng)

        While True
            Console.Clear()
            world.Tick()
            Console.ForegroundColor = ConsoleColor.White
            Console.WriteLine(world.Calendar.ToString)
            Report.ConsoleReport()
            Console.ReadKey()
        End While
    End Sub
    Private Function SetupPlayerShip(ByRef rng As Random) As ShipPlayer
        Dim ship As New ShipPlayer
        With ship
            .ConsoleColour = ConsoleColor.Cyan
            .Name = "Baron's Spear"
            .GenerateBaselines(ShipType.Sloop)

            For n = 1 To 3
                .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Crew, ShipModule.ModuleQuality.Average, CrewRace.Human))
            Next

            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewRole.Gunner)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewRole.Gunner)

            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng), CrewRole.Captain)
            .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Maproom, ShipModule.ModuleQuality.Average, CrewRace.Human))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng), CrewRole.Navigator)
            .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Helm, ShipModule.ModuleQuality.Average, CrewRace.Human))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng), CrewRole.Helmsman)

            .AddWeapon(ShipQuarter.Port, ShipWeapon.Generate("cannon"))
            .AddModule(ShipQuarter.Starboard, ShipModule.Generate(ShipModule.ModuleType.Hold, ShipModule.ModuleQuality.Excellent, Nothing))
        End With
        Return ship
    End Function

    Private Sub EnterBattle()
        Dim enemies As New List(Of ShipAI) From {ShipAI.Generate(ShipType.Sloop, Faction.Neutral, CrewRace.Human)}
        World.EnterCombat(enemies)
    End Sub
    Public Sub Battle(ByVal battlefield As Battlefield, ByVal playerShip As ShipPlayer)
        Console.Clear()
        battlefield.ConsoleWrite()
        Console.WriteLine()
        Report.ConsoleReport()
        Console.WriteLine()

        Dim AITurn As Boolean = PlayerInput(playerShip, battlefield)
        battlefield.CombatTick(AITurn)
    End Sub
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
        battlefield.ConsoleReportCombatants()
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

#Region "Retired Tests"
    Private Function SetupBattlefield(ByRef rng As Random) As Battlefield
        Dim battlefield As Battlefield = battlefield.Generate(15, 15, 0, BattleDirection.East)
        Dim ship As ShipPlayer = SetupPlayerShip(rng)
        With ship
            .ConsoleColour = ConsoleColor.Cyan
            .Name = "Baron's Spear"
            '.Cheaterbug(True, True, True, True)
        End With
        battlefield.AddCombatant(ship, 5, 5, BattleDirection.East)

        Dim ai1 As ShipAI = ShipAI.Generate(ShipType.Sloop, Nothing, Nothing)
        ai1.ConsoleColour = ConsoleColor.Green
        'ai1.Cheaterbug(True, True, False, False)
        battlefield.AddCombatant(ai1, 1, 1, BattleDirection.East)

        Dim ai2 As ShipAI = ShipAI.Generate(ShipType.Sloop, Nothing, Nothing)
        ai2.ConsoleColour = ConsoleColor.Green
        ai2.Cheaterbug(True, True, False, False)
        battlefield.AddCombatant(ai2, 8, 8, BattleDirection.West)

        Return battlefield
    End Function
    Private Sub TestPlayerShip()
        Dim ship = SetupPlayerShip((New Random(5)))
        With ship
            .AddGood(GoodType.Shot, 100)
            .GoodsFreeForConsumption(GoodType.Shot) = True
            .AddGood(GoodType.Rations, 100)
            .GoodsFreeForConsumption(GoodType.Rations) = True
            .AddGood(GoodType.Water, 100)
            .GoodsFreeForConsumption(GoodType.Water) = True

            Dim st As Crew = Crew.Generate(CrewRace.Seatouched)
            .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Crew, ShipModule.ModuleQuality.Average, CrewRace.Seatouched))
            .AddCrew(ShipQuarter.Fore, st)
            .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Shrine, ShipModule.ModuleQuality.Luxurious, CrewRace.Seatouched))
            st.Shrine = .GetModulesFree(ShipModule.ModuleType.Shrine, CrewRace.Seatouched)(0)

            .Tick()

            .ConsoleReport()
        End With
        Console.ReadKey()
        End
    End Sub
#End Region
End Module
