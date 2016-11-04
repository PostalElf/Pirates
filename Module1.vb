Module Module1
    Dim world As World
    Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))

    Sub Main()
        Console.SetWindowSize(100, 43)
        world = Pirates.World.Generate
        world.ShipPlayer = SetupPlayerShip(world.Rng)
        world.ShipPlayer.Teleport(world.GetIsle("Deathless Kingdom"))

        TestSnippets(world.ShipPlayer)

        While True
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.White
            Console.WriteLine(world.Calendar.ToString)
            world.ShipPlayer.ConsoleReportTravelStatus()

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
            .Race = CrewRace.Human
            .GenerateBaselines(ShipType.Sloop)
            .AvailableMoves.Add(New MoveToken({BattleMove.Halt}))

            For n = 1 To 3
                .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Quarters, ShipModule.ModuleQuality.Average, .Race))
            Next

            .AddCrew(ShipQuarter.Fore, Crew.Generate(.Race, rng, Nothing, CrewSkill.Sailing), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Aft, Crew.Generate(.Race, rng, Nothing, CrewSkill.Sailing), CrewRole.Sailor)
            .AddCrew(ShipQuarter.Port, Crew.Generate(.Race, rng, Nothing, CrewSkill.Gunnery), CrewRole.Gunner)
            .AddCrew(ShipQuarter.Port, Crew.Generate(.Race, rng, Nothing, CrewSkill.Gunnery), CrewRole.Gunner)

            .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Quarterdeck, ShipModule.ModuleQuality.Average, Nothing))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(.Race, rng, Nothing, CrewSkill.Leadership), CrewRole.Captain)
            .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Maproom, ShipModule.ModuleQuality.Average, CrewRace.Human))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(.Race, rng, Nothing, CrewSkill.Navigation), CrewRole.Navigator)
            .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Helm, ShipModule.ModuleQuality.Average, CrewRace.Human))
            .AddCrew(ShipQuarter.Fore, Crew.Generate(.Race, rng, Nothing, CrewSkill.Steering), CrewRole.Helmsman)

            .AddWeapon(ShipQuarter.Port, ShipWeapon.Generate("cannon"))
            .AddModule(ShipQuarter.Starboard, ShipModule.Generate(ShipModule.ModuleType.Hold, ShipModule.ModuleQuality.Excellent, Nothing))

            'Dim shrine As ShipModule = ShipModule.Generate(ShipModule.ModuleType.Shrine, ShipModule.ModuleQuality.Nice, CrewRace.Seatouched)
            '.AddModule(ShipQuarter.Starboard, shrine)
            'For Each Crew In .GetCrews(CrewRace.Seatouched)
            '    shrine.AddCrew(Crew)
            'Next

            .AddModule(ShipQuarter.Starboard, ShipModule.Generate(ShipModule.ModuleType.Hold, ShipModule.ModuleQuality.Poor, Nothing))
            .AddGood(GoodType.Rations, 100)
            .AddGood(GoodType.Water, 100)
            .AddGood(GoodType.Shot, 25)
            .AddGood(GoodType.Grapples, 5)
        End With
        Return ship
    End Function
    Private Sub TestSnippets(ByRef player As ShipPlayer)
        player.AddEquipment(CrewBonus.Generate("Flintlock Pistol"))
        player.AddWeapon(ShipQuarter.Port, ShipWeapon.Generate("hooks"))

        For Each Route In world.BasicRoutes
            player.AddRoute(Route)
        Next

        Dim isle As Isle = world("Deathless Kingdom")
        isle.AddReputationXP(IsleFaction.Church, -51)
        isle.AddReputationXP(IsleFaction.Church, 1)
        isle.AddBuilding("Crypt")
        isle.AddBuilding("Clinic")
        isle.AddBuilding("Temple")
        isle.AddBuilding("Guild")
        isle.AddBuilding("Shipyard")

        player.AddCoins(WorldFaction.Deathless, 1000)
        Dim damage As New Damage(0, 25, DamageType.Blunt, "God")
        Dim captain As Crew = player.GetCrew(Nothing, CrewRole.Captain)
        captain.ShipAttack(100, damage)
        captain.AddBonus("equipment", CrewBonus.Generate("Bullwhip"))
        damage = New Damage(10, 0, DamageType.Cannon, "God")
        player.AddDamage(damage, ShipQuarter.Fore, 100)
        player.AddDamage(damage.Clone, ShipQuarter.Aft, 100)
    End Sub

    Private Function MainPlayerInput() As Boolean
        'return true when player ends turn

        Dim player As ShipPlayer = world.ShipPlayer
        Dim choices As New Dictionary(Of Char, String)
        With choices
            If world.ShipPlayer.IsAtSea = True Then
                .Add("a"c, "Attack")
            Else
                .Add("s"c, "Set Course")
                .Add("b"c, "Buy/Sell Goods")
                If player.Isle.GetBuilding("Guild") = True Then .Add("x"c, "Exchange Money")
                If player.Isle.GetBuilding("Shipyard") = True Then .Add("r"c, "Repair Ship") : .Add("u"c, "Upgrade Ship")
            End If
            .Add("v"c, "View Ship")
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
                SetCourse(world.ShipPlayer)
            Case "b"c
                BuySell(world.ShipPlayer.Isle)
            Case "x"c
                ExchangeMoney(world.ShipPlayer)
            Case "r"c
                RepairShip(player)
            Case "u"c
                Dim choices1 As New Dictionary(Of Char, String)
                choices1.Add("b"c, "Buy")
                choices1.Add("s"c, "Sell")
                choices1.Add("u"c, "Upgrade")
                Select Case Menu.getListChoice(choices1, 0)
                    Case "b"c : BuyModule(player)
                    Case "s"c : SellModule(player)
                    Case "u"c : UpgradeModule(player)
                End Select
            Case "v"c
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
        Dim enemies As New List(Of ShipAI) From {ShipAI.Generate(ShipType.Sloop, WorldFaction.Neutral, CrewRace.Human)}
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
    Private Sub SetCourse(ByVal ship As ShipPlayer)
        Dim routes As List(Of Route) = ship.GetRoutesFromLocation
        Dim destinations As New List(Of Isle)
        For Each r In routes
            destinations.Add(r - ship.isle)
        Next

        Dim targetDestination As Isle = Menu.getListChoice(Of Isle)(destinations, 0, "Select a destination:")
        Dim index As Integer = destinations.IndexOf(targetDestination)
        Dim targetRoute As Route = routes(index)
        If ship.CheckSetTravelRoute(targetRoute) = False Then Exit Sub

        Dim distance As Double = targetRoute.GetDistance
        Dim speed As Double = ship.GetTravelSpeed()
        Dim days As Integer = Math.Ceiling(distance / speed)
        Console.WriteLine()
        Console.WriteLine(Dev.vbTab("Distance:", 11) & distance.ToString("0.0") & " miles")
        Console.WriteLine(Dev.vbTab("Speed:", 11) & speed.ToString("0.0") & " miles per day")
        Console.WriteLine("This journey will take approximately " & days & " days.")
        If Menu.confirmChoice(0) = False Then Exit Sub

        ship.SetTravelRoute(targetRoute)
    End Sub
    Private Sub ExchangeMoney(ByVal ship As ShipPlayer)
        If ship.Isle.GetBuilding("Guild") = False Then Exit Sub
        Console.WriteLine()

        Dim currencies As New Dictionary(Of Integer, String)
        Dim n As Integer = 0
        For Each c In ([Enum].GetValues(GetType(WorldFaction)))
            n += 1
            currencies.Add(n, c.ToString)
        Next
        Dim currency As WorldFaction = Menu.getListChoice(currencies, 0, "Which curency? ")

        Dim money As Double = ship.GetCoins(currency)
        If money <= 0 Then Exit Sub
        Console.WriteLine("You have $" & money.ToString("0.00") & " in " & currency.ToString & " credit.")
        Dim change As Integer = Menu.getNumInput(0, 10, Math.Floor(money), "Change how much? ")

        Dim currency2 As WorldFaction = Menu.getListChoice(currencies, 0, "To which currency? ")
        If currency = currency2 Then Exit Sub
        Dim changer As Double = Math.Round(change * 0.05, 2)
        Console.WriteLine("Guild moneychangers will take $" & changer & " for services rendered.")
        If Menu.confirmChoice(0) = False Then Exit Sub

        ship.AddCoins(currency, -change)
        ship.AddCoins(currency2, change - changer)
        Console.WriteLine()
        Console.WriteLine("You now have $" & ship.GetCoins(currency) & " in " & currency.ToString & " credit.")
        Console.WriteLine("You now have $" & ship.GetCoins(currency2) & " in " & currency2.ToString & " credit.")
        Console.ReadKey()
    End Sub
    Private Sub RepairShip(ByVal ship As ShipPlayer)
        Dim dmg As Damage = ship.GetWorstDamage
        If dmg Is Nothing Then Exit Sub
        If ship.HasHealed = True Then Exit Sub

        Dim cost As Double = dmg.ShipDamage * 2
        Console.Write("It will cost you $" & cost.ToString("0.00") & " to repair ")
        Console.Write(dmg.ShipDamage & " " & dmg.Type.ToString & " damage from " & dmg.Sender & ".")
        Console.WriteLine()
        If Menu.confirmChoice(0) = False Then Exit Sub
        If ship.CheckAddCoins(ship.Isle.WorldFaction, -cost) = False Then Exit Sub

        ship.AddCoins(ship.Isle.WorldFaction, -cost)
        ship.RepairDamage(dmg)
    End Sub
    Private Sub BuyModule(ByVal player As ShipPlayer)
        Console.WriteLine()
        Dim choices As New List(Of String)
        For Each mt As ShipModule.ModuleType In [Enum].GetValues(GetType(ShipModule.ModuleType))
            choices.Add(mt.ToString)
        Next
        Dim type As ShipModule.ModuleType = Nothing
        Dim choice As String = Menu.getListChoice(Of String)(choices, 0, "Select a module:")
        For Each mt In [Enum].GetValues(GetType(ShipModule.ModuleType))
            If mt.ToString = choice Then type = mt : Exit For
        Next
        If type = Nothing Then Exit Sub
        choices.Clear()
        For Each mq As ShipModule.ModuleQuality In [Enum].GetValues(GetType(ShipModule.ModuleQuality))
            choices.Add(mq.ToString)
        Next
        Dim quality As ShipModule.ModuleQuality = Nothing
        choice = Menu.getListChoice(Of String)(choices, 0, "Select quality:")
        For Each mq In [Enum].GetValues(GetType(ShipModule.ModuleQuality))
            If mq.ToString = choice Then quality = mq : Exit For
        Next
        If quality = Nothing Then Exit Sub

        Dim m As ShipModule = ShipModule.Generate(type, quality, player.Race)
        If m.IsExclusive = True AndAlso player.GetModule(type) Is Nothing = False Then
            Console.WriteLine("You already have a " & type.ToString & ".")
            Console.WriteLine("Choose the upgrade option instead of buying another one.")
            Console.ReadKey()
            Exit Sub
        End If
        Dim cost As Double = m.Cost
        Console.WriteLine("Adding a " & type.ToString & " will cost you $" & cost.ToString("0.00") & ".")
        If Menu.confirmChoice(0) = False Then Exit Sub
        If player.CheckAddCoins(player.Isle.WorldFaction, -cost) = False Then Exit Sub

        Dim q As ShipQuarter = Menu.getListChoice(Of ShipQuarter)(quarters, 0)
        player.AddCoins(player.Isle.WorldFaction, -cost)
        player.AddModule(q, m)
        Console.WriteLine(m.Name & " successfully added to " & player.Name & "'s " & q.ToString & ".")
        Console.ReadKey()
    End Sub
    Private Sub SellModule(ByVal player As ShipPlayer)
        Dim m As ShipModule = Menu.getListChoice(Of ShipModule)(player.GetModules(Nothing, Nothing), 0, "Select a module:")
        If m Is Nothing Then Exit Sub
        Dim cost As Double = Math.Round(m.Cost * 0.8, 2)
        Console.WriteLine("Selling " & m.Name & " will give you $" & cost & ".")
        If m.Type = ShipModule.ModuleType.Helm OrElse m.Type = ShipModule.ModuleType.Maproom OrElse m.Type = ShipModule.ModuleType.Quarterdeck Then
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("WARNING: selling this module will render your ship un-seaworthy.")
            Console.ForegroundColor = ConsoleColor.Gray
        End If
        If Menu.confirmChoice(0) = False Then Exit Sub

        player.RemoveModule(Nothing, m)
        player.AddCoins(player.Isle.WorldFaction, cost)
    End Sub
    Private Sub UpgradeModule(ByVal player As ShipPlayer)
        Console.WriteLine()
        Dim m As ShipModule = Menu.getListChoice(Of ShipModule)(player.GetModules(Nothing, Nothing), 0, "Select a module:")
        If m Is Nothing Then Exit Sub
        Dim qualities As New List(Of ShipModule.ModuleQuality)
        For Each q As ShipModule.ModuleQuality In ([Enum].GetValues(GetType(ShipModule.ModuleQuality)))
            If q > m.Quality Then qualities.Add(q)
        Next
        If qualities.Count = 0 Then Console.WriteLine(m.Name & " is already fully upgraded.") : Console.ReadKey() : Exit Sub
        Dim quality As ShipModule.ModuleQuality = Menu.getListChoice(Of ShipModule.ModuleQuality)(qualities, 0)

        Dim newM As ShipModule = ShipModule.Generate(m.Type, quality, player.Race)
        Dim cost As Double = newM.Cost - m.Cost
        Console.WriteLine("It will cost you $" & cost & " to upgrade your " & m.Name & ".")
        If Menu.confirmChoice(0) = False Then Exit Sub
        If player.CheckAddCoins(player.Isle.WorldFaction, -cost) = False Then Exit Sub

        player.AddCoins(player.Isle.WorldFaction, -cost)
        Console.WriteLine()
        Console.WriteLine("Your " & m.Name & " are now " & newM.Quality.ToString & ".")
        m.Quality = newM.Quality
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
                gears.Add(CrewBonus.Generate("Belaying Pin"))
                Dim targetGear As CrewBonus = Menu.getListChoice(Of CrewBonus)(gears, 0)
                If targetGear Is Nothing Then Exit Sub
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
    Private Sub BuySell(ByRef isle As Isle)
        Const stockColour As ConsoleColor = ConsoleColor.Gray
        Const buyColour As ConsoleColor = ConsoleColor.Yellow
        Const sellColour As ConsoleColor = ConsoleColor.Green
        Const nullColour As ConsoleColor = ConsoleColor.DarkRed

        Console.WriteLine()
        Console.ForegroundColor = stockColour
        Console.Write("                  Qty")
        Console.ForegroundColor = buyColour
        Console.Write("    Buy")
        Console.ForegroundColor = stockColour
        Console.Write("        Ship")
        Console.ForegroundColor = sellColour
        Console.Write("    Sell")
        Console.ForegroundColor = ConsoleColor.Gray
        Console.WriteLine()
        Console.Write("                  ---")
        Console.ForegroundColor = buyColour
        Console.Write("    ---")
        Console.ForegroundColor = stockColour
        Console.Write("        ----")
        Console.ForegroundColor = sellColour
        Console.Write("    ----")
        Console.WriteLine()

        Dim gtList As New List(Of GoodType)
        Dim n = 0
        For Each gt In [Enum].GetValues(GetType(GoodType))
            n += 1
            gtList.Add(gt)

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(Dev.vbSpace(1))
            Console.Write(n.ToString("00") & ") ")
            Console.Write(Dev.vbTab(gt.ToString, 12))

            Dim qty As Integer = isle.GetGoodQty(gt)
            If qty <= 0 Then Console.ForegroundColor = nullColour Else Console.ForegroundColor = stockColour
            Console.Write(Dev.vbTab(qty, 7))

            Dim buyPrice As Double = isle.GetGoodPrice(gt, True)
            If buyPrice <= 0 Then Console.ForegroundColor = nullColour Else Console.ForegroundColor = buyColour
            Console.Write(Dev.vbTab("$" & buyPrice.ToString("0.00"), 11))

            qty = world.ShipPlayer.GetGood(gt).Qty
            If qty <= 0 Then Console.ForegroundColor = nullColour Else Console.ForegroundColor = stockColour
            Console.Write(Dev.vbTab(qty, 8))

            Dim sellPrice As Double = isle.GetGoodPrice(gt, False)
            If sellPrice <= 0 Then Console.ForegroundColor = nullColour Else Console.ForegroundColor = sellColour
            Console.Write("$" & sellPrice.ToString("0.00"))
            Console.WriteLine()
        Next
        Console.ResetColor()
        Console.WriteLine()

        Dim input As Integer = Menu.getNumInput(0, 0, gtList.Count, "> ")
        If input = 0 Then Exit Sub
        Dim choice As GoodType = gtList(input - 1)
        Console.Write("Buy or sell? ")
        Dim input2 As ConsoleKeyInfo = Console.ReadKey()
        Console.WriteLine()
        If input2.KeyChar = "b"c Then
            'buy
            Dim price As Double = isle.GetGoodPrice(choice, True)
            Dim qty As Integer = isle.GetGoodQty(choice)
            If qty = 0 Then Exit Sub
            Console.WriteLine("Buying " & choice.ToString & " at $" & price.ToString("0.00") & " each.")
            Dim input3 As Integer = Menu.getNumInput(0, 0, qty, "How much? ")
            If input3 = 0 Then Exit Sub

            Dim totalCost As Double = Math.Round(input3 * price, 2)
            If Menu.confirmChoice(0, "Buy " & input3 & " " & choice.ToString & " for $" & totalCost.ToString("0.00") & "? ") = False Then Exit Sub
            If isle.CheckSellGood(choice, input3, world.ShipPlayer) = False Then Exit Sub
            isle.SellGood(choice, input3, world.ShipPlayer)
        ElseIf input2.KeyChar = "s"c Then
            'sell
            Dim price As Double = isle.GetGoodPrice(choice, False)
            Dim qty As Integer = world.ShipPlayer.GetGood(choice).Qty
            If qty = 0 OrElse price = 0 Then Exit Sub
            Console.WriteLine("Selling " & choice.ToString & " at $" & price.ToString("0.00") & " each.")
            Dim input3 As Integer = Menu.getNumInput(0, 0, isle.GetGoodQty(choice), "How much? ")
            If input3 = 0 Then Exit Sub

            Dim totalCost As Double = Math.Round(input3 * price, 2)
            If Menu.confirmChoice(0, "Sell " & input3 & " " & choice.ToString & " for $" & totalCost.ToString("0.00") & "? ") = False Then Exit Sub
            If isle.CheckBuyGood(choice, input3, world.ShipPlayer) = False Then Exit Sub
            isle.BuyGood(choice, input3, world.ShipPlayer)
        Else : Exit Sub
        End If
    End Sub
    Private Function GetCrew(ByRef ship As ShipPlayer) As Crew
        Dim roles As New List(Of CrewRole)([Enum].GetValues(GetType(CrewRole)))
        Dim role As CrewRole = Menu.getListChoice(Of CrewRole)(roles, 0, "Select crew role:")

        Dim choiceList As List(Of Crew) = ship.GetCrews(Nothing, role)
        If choiceList.Count = 1 Then Return choiceList(0)
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
End Module
