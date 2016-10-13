Module Module1
    Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))

    Sub Main()
        Battle()
    End Sub

    Private Sub Battle()
        Dim rng As New Random(5)
        Dim battlefield As Battlefield = SetupBattlefield(rng)
        Dim playerShip As ShipPlayer = Nothing
        For Each Ship In battlefield.Combatants
            If TypeOf Ship Is ShipPlayer Then playerShip = Ship
        Next

        While battlefield.IsOver = False
            Console.Clear()
            battlefield.ConsoleWrite()
            Console.WriteLine()
            Report.ConsoleReport()
            Console.WriteLine()

            Dim AITurn As Boolean = PlayerInput(playerShip, battlefield)
            If AITurn = True Then
                For Each combatant In battlefield.Combatants
                    If combatant.InMelee = True Then Continue For
                    If TypeOf combatant Is ShipAI Then : CType(combatant, ShipAI).Tick(playerShip)
                    ElseIf TypeOf combatant Is ShipPlayer Then : CType(combatant, ShipPlayer).Tick()
                    Else : combatant.Tick()
                    End If
                Next

                For Each Melee In battlefield.Melees
                    Melee.Tick()
                Next
            End If

            battlefield.CleanUp()
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
        Dim battlefield As Battlefield = battlefield.Generate(15, 15, 3)
        Dim crate As New GoodCrate("Large Shot Crate", GoodType.Shot, 100, 0)
        Dim hooks As New ShipWeapon("Grappling Hooks", 0, 0, DamageType.Cannon, 1, New Good(GoodType.Grapplers, 1), 1, 1)
        Dim cannon As New ShipWeapon("Cannons", 30, 10, DamageType.Cannon, 2, New Good(GoodType.Shot, 1), 1, 3)
        Dim grapeshot As New ShipWeapon("Grapeshot", 10, 25, DamageType.Firearms, 1, New Good(GoodType.Grapeshot, 1), 2, 5)

        Dim ship As New ShipPlayer
        With ship
            .ConsoleColour = ConsoleColor.White
            .Name = "Baron's Spear"
            .AddWeapon(ShipQuarter.Port, cannon.Clone)
            .AddWeapon(ShipQuarter.Starboard, grapeshot.Clone)
            .AddWeapon(ShipQuarter.Port, hooks.Clone)
            .AddWeapon(ShipQuarter.Starboard, hooks.Clone)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewSkill.Gunnery)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewSkill.Gunnery)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewSkill.Gunnery)
            .AddCrew(ShipQuarter.Fore, Crew.Generate(CrewRace.Human, rng), CrewSkill.Sailing)
            .AddCrew(ShipQuarter.Starboard, Crew.Generate(CrewRace.Human, rng), CrewSkill.Sailing)
            .AddCrew(ShipQuarter.Port, Crew.Generate(CrewRace.Human, rng), CrewSkill.Sailing)
            .Cheaterbug()
        End With
        battlefield.AddCombatant(ship, 5, 5, BattleDirection.East)

        Dim ai1 As ShipAI = ShipAI.Generate(ShipType.Sloop)
        ai1.ConsoleColour = ConsoleColor.Green
        battlefield.AddCombatant(ai1, 1, 1, BattleDirection.East)

        Dim ai2 As ShipAI = ShipAI.Generate(ShipType.Sloop)
        ai2.ConsoleColour = ConsoleColor.Green
        battlefield.AddCombatant(ai2, 8, 8, BattleDirection.North)

        Return battlefield
    End Function
    Private Function PlayerInput(ByRef ship As ShipPlayer, ByRef battlefield As Battlefield) As Boolean
        'return true when player ends turn

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
        For Each combatant In battlefield.Combatants
            If TypeOf combatant Is Ship Then
                Dim ship As Ship = CType(combatant, Ship)
                ship.ConsoleReport()
            End If
            Console.WriteLine()
        Next
        Console.ReadKey()
    End Sub
    Private Sub ViewSelf(ByVal ship As ShipPlayer)
        Dim t As Integer = 12
        Dim s As String = Dev.vbSpace(1)
        Dim quarters As New List(Of ShipQuarter)([Enum].GetValues(GetType(ShipQuarter)))
        Console.WriteLine()

        Dim roles As CrewSkill() = {CrewSkill.Sailing, CrewSkill.Gunnery}
        For Each role In roles
            Console.WriteLine(role.ToString & ":")
            For Each q In quarters
                For Each Crew In ship.GetCrews(q, role)
                    Console.WriteLine(s & Dev.vbTab(q.ToString & ":", t) & Crew.Name & " (" & Crew.GetSkill(role) & ")")
                Next
            Next
            Console.WriteLine()
        Next
        Console.WriteLine("Weapons:")
        For Each q In quarters
            For Each weapon In ship.GetWeapons(q)
                Console.Write(s & Dev.vbTab(q.ToString & ":", t) & weapon.Name & " - ")
                If weapon.Damage.ShipDamage > 0 Then Console.Write(weapon.Damage.ShipDamage & " " & weapon.Damage.Type.ToString & " ship - ")
                If weapon.Damage.CrewDamage > 0 Then Console.Write(weapon.Damage.CrewDamage & " " & weapon.Damage.Type.ToString & " crew - ")
                If weapon.CooldownCounter <= 0 Then Console.Write("OK") Else Console.Write("Reloading in " & weapon.CooldownCounter)
                Console.WriteLine()
            Next
        Next

        Select Case Menu.getListChoice(New List(Of String) From {"Move Crew", "Examine Crew"}, 0)
            Case "Move Crew" : MoveCrew(ship)
            Case "Examine Crew" : ExamineCrew(ship)
            Case Else : Exit Sub
        End Select
    End Sub
    Private Sub MoveCrew(ByRef ship As ShipPlayer)
        Dim target As Crew = GetCrew(ship)
        Dim destination As ShipQuarter = Menu.getListChoice(Of ShipQuarter)(quarters, 0, "To where?")
        Dim newrole As CrewSkill = Nothing
        If Menu.confirmChoice(0, "Keep role? ") = True Then
            newrole = target.Role
        Else
            newrole = Menu.getListChoice(New List(Of CrewSkill) From {CrewSkill.Sailing, CrewSkill.Gunnery}, 0, "Select new role:")
        End If
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
        Dim role As CrewSkill = Menu.getListChoice(Of CrewSkill)(New List(Of CrewSkill)({CrewSkill.Sailing, CrewSkill.Gunnery}), 0, "From which role?")
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
        battlefield.DeadObjects.Add(target)
    End Sub
End Module
