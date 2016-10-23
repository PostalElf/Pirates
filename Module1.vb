Module Module1
    Dim world As World
    Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))

    Sub Main()
        Console.SetWindowSize(Console.WindowWidth, 50)
        world = Pirates.World.Generate
        world.ShipPlayer = SetupPlayerShip(world.Rng)
        world.ShipPlayer.teleport(world.Item("Commonwealth"))

        TestSnippets(world.ShipPlayer)

        While True
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.White
            Console.WriteLine(world.Calendar.ToString)
            Console.ForegroundColor = ConsoleColor.Gray
            Report.ConsoleReport()
            Console.WriteLine()
            Console.WriteLine()
            If MainPlayerInput() = True Then world.Tick()
        End While
    End Sub
    Private Function SetupPlayerShip(ByRef rng As Random) As ShipPlayer
        Dim ship As New ShipPlayer
        With ship
            .ConsoleColour = ConsoleColor.Cyan
            .Name = "Baron's Spear"
            .GenerateBaselines(ShipType.Sloop)
            .AvailableMoves.Add(New MoveToken({BattleMove.Halt}))

            For n = 1 To 3
                .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Crew, ShipModule.ModuleQuality.Average, CrewRace.Human))
            Next

            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng, CrewSkill.Sailing), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Aft, Crew.Generate(CrewRace.Human, rng, CrewSkill.Sailing), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng, CrewSkill.Gunnery), CrewRole.Gunner)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng, CrewSkill.Gunnery), CrewRole.Gunner)

            .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Quarterdeck, ShipModule.ModuleQuality.Average, Nothing))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng, CrewSkill.Leadership), CrewRole.Captain)
            .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Maproom, ShipModule.ModuleQuality.Average, CrewRace.Human))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng, CrewSkill.Navigation), CrewRole.Navigator)
            .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Helm, ShipModule.ModuleQuality.Average, CrewRace.Human))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng, CrewSkill.Steering), CrewRole.Helmsman)

            .AddWeapon(ShipQuarter.Port, ShipWeapon.Generate("cannon"))
            .AddModule(ShipQuarter.Starboard, ShipModule.Generate(ShipModule.ModuleType.Hold, ShipModule.ModuleQuality.Excellent, Nothing))

            .AddModule(ShipQuarter.Starboard, ShipModule.Generate(ShipModule.ModuleType.Hold, ShipModule.ModuleQuality.Poor, Nothing))
            .AddGood(GoodType.Rations, 100)
            .AddGood(GoodType.Water, 100)
        End With
        Return ship
    End Function
    Private Sub TestSnippets(ByRef player As ShipPlayer)
        Dim pistol As New CrewBonus
        With pistol
            .Name = "Pistol"
            .Damage = 25
            .DamageType = DamageType.Firearms
            .AmmoUse = 1
            .Slot = "Left Hand"
        End With
        player.AddEquipment(pistol)

        For Each Route In world.BasicRoutes
            player.AddRoute(Route)
        Next
        Dim r As Route = player.GetRoute(world.Item("Commonwealth"), world.Item("Hallowsreach"))
        player.SetTravelRoute(r)
    End Sub

    Private Function MainPlayerInput() As Boolean
        'return true when player ends turn

        Dim choices As New Dictionary(Of Char, String)
        With choices
            .Add("a"c, "Attack")
            .Add("s"c, "View Ship")
            .Add("c"c, "View Crew")
            .Add("g"c, "View Goods")
            .Add("m"c, "View Modules")
            .Add("z"c, "Tick")
        End With
        Dim input As Char = Menu.getListChoice(choices, 0)
        Console.WriteLine()
        Select Case input
            Case "a"c
                EnterBattle()
            Case "s"c
                world.ShipPlayer.ConsoleReport()
                Console.ReadKey()
            Case "c"c
                ManageCrews(world.ShipPlayer)
            Case "g"c
                Console.WriteLine()
                world.ShipPlayer.ConsoleReportGoods()
                Console.WriteLine()
                Console.ReadKey()
            Case "m"c
                ViewModules(world.ShipPlayer)
            Case "z"c : Return True
        End Select

        Return False
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

        Dim AITurn As Boolean = BattlePlayerInput(playerShip, battlefield)
        battlefield.CombatTick(AITurn)
    End Sub
    Private Function BattlePlayerInput(ByRef ship As ShipPlayer, ByRef battlefield As Battlefield) As Boolean
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
            Case ConsoleKey.Spacebar : Return True
            Case ConsoleKey.NumPad2, ConsoleKey.OemComma : targetMove = New MoveToken({BattleMove.Backwards})
            Case ConsoleKey.W : ViewWeapons(ship) : Return False
            Case ConsoleKey.V : ViewBattlefield(battlefield) : Return False
            Case ConsoleKey.C : ManageCrews(ship) : Return False
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
    Private Sub ManageCrews(ByVal ship As ShipPlayer)
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
        Console.WriteLine()

        Dim target As Crew = GetCrew(ship)
        If target Is Nothing Then Exit Sub
        Console.WriteLine()
        target.ConsoleReport()
        Console.WriteLine()

        Dim choices As New Dictionary(Of Integer, String)
        With choices
            .Add(1, "Move")
            .Add(2, "Set Sail Station")
            .Add(3, "Set Battle Station")
            .Add(4, "Add Gear")
            .Add(5, "Remove Gear")
        End With
        Dim input As Integer = Menu.getListChoice(choices, 0)
        Console.WriteLine()
        Select Case input
            Case 1, 2, 3
                Dim destination As ShipQuarter = Menu.getListChoice(Of ShipQuarter)(quarters, 0, "To where?")
                If destination = Nothing Then Exit Sub
                Dim newrole As CrewRole = Menu.getListChoice(ship.GetAvailableRoles(destination), 0, "Select new role:")
                If newrole = Nothing Then Exit Sub
                Select Case input
                    Case 1
                        If ship.InCombat = True Then ship.AddCommand("Move", target, destination, newrole) Else ship.MoveCrew(target, destination, newrole)
                    Case 2
                        Dim station As New CrewStation(destination, newrole)
                        target.SetStation(station, False)
                        If ship.InCombat = False Then ship.MoveCrewToStation(target)
                    Case 3
                        Dim station As New CrewStation(destination, newrole)
                        target.SetStation(station, True)
                        If ship.InCombat = True Then ship.AddCommand("Move", target, destination, newrole)
                End Select
            Case 4
                Dim gears As List(Of CrewBonus) = ship.GetEquipments()
                If gears.Count = 0 Then
                    Console.WriteLine("No spare equipment!")
                    Console.ReadKey()
                    Exit Sub
                End If
                Dim targetGear As CrewBonus = Menu.getListChoice(Of CrewBonus)(gears, 0)
                Console.WriteLine()
                If target.CheckAddBonus("equipment", targetGear) = False Then
                    Console.WriteLine("Unable to add equipment!")
                    Console.ReadKey()
                    Exit Sub
                End If
                target.AddBonus("equipment", targetGear)
            Case 5
                Dim gears As List(Of CrewBonus) = target.GetBonusList("equipment")
                If gears.Count = 0 Then
                    Console.WriteLine("No equipment to remove!")
                    Console.ReadKey()
                    Exit Sub
                End If
                Dim targetGear As CrewBonus = Menu.getListChoice(Of CrewBonus)(gears, 0)
                Console.WriteLine()
                If target.CheckRemoveBonus("equipment", targetGear) = False Then
                    Console.WriteLine("Invalid equipment!")
                    Console.ReadKey()
                    Exit Sub
                End If
                target.RemoveBonus("equipment", targetGear)
            Case 5

        End Select
    End Sub
    Private Sub ViewWeapons(ByVal ship As ShipPlayer)
        Dim s As String = Dev.vbSpace(1)
        Dim t As Integer = 12

        Console.WriteLine()
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
        Console.ReadKey()
    End Sub
    Private Sub ViewModules(ByVal ship As ShipPlayer)
        For Each ShipModule In ship.GetModules(Nothing, Nothing)
            ShipModule.ConsoleReport()
            Console.WriteLine()
        Next
        Console.WriteLine()
        Console.ReadKey()
    End Sub
    Private Function GetCrew(ByRef ship As ShipPlayer) As Crew
        Dim roles As New List(Of CrewRole)([Enum].GetValues(GetType(CrewRole)))
        Dim role As CrewRole = Menu.getListChoice(Of CrewRole)(roles, 0, "Select crew role:")
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

            Dim st As Crew = Crew.Generate(CrewRace.Seatouched, New Random)
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
