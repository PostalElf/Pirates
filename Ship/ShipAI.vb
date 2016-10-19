Public Class ShipAI
    Inherits Ship

    Public Sub New()
        MyBase.New()
    End Sub
    Public Shared Function Generate(ByVal type As ShipType, Optional ByVal faction As Faction = Nothing, Optional ByVal race As CrewRace = Nothing, Optional ByRef rng As Random = Nothing) As Ship
        If rng Is Nothing Then rng = New Random
        Dim races As CrewRace() = [Enum].GetValues(GetType(CrewRace))
        Dim factions As Faction() = [Enum].GetValues(GetType(Faction))
        Dim newCrews As New List(Of CrewPosition)

        Dim ship As New ShipAI
        With ship
            If race = Nothing Then .Race = World.Rng.Next(1, races.Length + 1) Else .Race = race
            If faction = Nothing Then .Faction = World.Rng.Next(1, factions.Length + 1) Else .Faction = faction
            .Type = type
            .Name = GenerateName(rng)
            .ID = GenerateID(.Name)
            .ConsoleColour = ConsoleColor.Red

            Select Case type
                Case ShipType.Sloop
                    GenerateWeapon(ship, "cannon", ShipQuarter.Starboard, newCrews)
                    GenerateWeapon(ship, "swivel", ShipQuarter.Fore, newCrews)

                Case ShipType.Schooner
                    GenerateWeapon(ship, "cannons", ShipQuarter.Starboard, newCrews)
                    GenerateWeapon(ship, "cannons", ShipQuarter.Port, newCrews)
                    GenerateWeapon(ship, "hailshot", ShipQuarter.Fore, newCrews)

                Case ShipType.Brigantine
                Case ShipType.Brig
                Case ShipType.Frigate
            End Select

            For Each q In [Enum].GetValues(GetType(ShipQuarter))
                newCrews.Add(New CrewPosition(q, CrewRole.Sailor))
            Next
            .GenerateBaselines(.Type)
            GenerateStandardModules(ship, newCrews, rng)

            'add loot
            For n = 1 To 3
                GenerateLoot(ship, rng)
            Next
        End With
        Return ship
    End Function
    Private Shared Sub GenerateStandardModules(ByRef ship As ShipAI, ByRef newCrews As List(Of CrewPosition), ByRef rng As Random)
        With ship
            If rng.Next(1, 3) = 1 Then
                .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Quarterdeck, 1, .Race))
                newCrews.Add(New CrewPosition(ShipQuarter.Aft, CrewRole.Captain))
                .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Helm, 1, .Race))
                newCrews.Add(New CrewPosition(ShipQuarter.Fore, CrewRole.Helmsman))
            Else
                .AddModule(ShipQuarter.Fore, ShipModule.Generate(ShipModule.ModuleType.Quarterdeck, 1, .Race))
                newCrews.Add(New CrewPosition(ShipQuarter.Fore, CrewRole.Captain))
                .AddModule(ShipQuarter.Aft, ShipModule.Generate(ShipModule.ModuleType.Helm, 1, .Race))
                newCrews.Add(New CrewPosition(ShipQuarter.Aft, CrewRole.Helmsman))
            End If

            Dim q As ShipQuarter = rng.Next(1, 5)
            .AddModule(q, ShipModule.Generate(ShipModule.ModuleType.Maproom, 1, .Race))
            newCrews.Add(New CrewPosition(q, CrewRole.Navigator))


            'generate crewquarters
            Dim crewCount As Integer = newCrews.Count
            For n = 1 To Math.Ceiling(crewCount / 5)
                Dim quarter As ShipQuarter = rng.Next(1, 5)
                .AddModule(quarter, ShipModule.Generate(ShipModule.ModuleType.Crew, 1, .Race))
            Next

            'add crew
            For Each cp In newCrews
                .AddCrew(cp.quarter, Crew.Generate(.Race), cp.role)
            Next
            newCrews.Clear()
        End With
    End Sub
    Private Shared Sub GenerateWeapon(ByRef ship As ShipAI, ByVal weaponTemplate As String, ByVal quarter As ShipQuarter, ByRef newCrews As List(Of CrewPosition))
        Dim weapon As ShipWeapon = ShipWeapon.Generate(weaponTemplate)
        ship.AddWeapon(quarter, weapon)

        Dim ammoAmt As Integer = weapon.AmmoUse.Qty * 10
        Dim ammoType As GoodType = weapon.AmmoUse.Type
        If ammoType <> Nothing Then ship.AddGood(ammoType, ammoAmt)

        If weapon.CrewCount > 0 Then
            For n = 1 To weapon.CrewCount
                newCrews.Add(New CrewPosition(quarter, CrewRole.Gunner))
            Next
        End If
    End Sub
    Private Shared Sub GenerateLoot(ByRef ship As ShipAI, ByRef rng As Random)
        Dim goods As New List(Of GoodType)([Enum].GetValues(GetType(GoodType)))
        Dim gt As GoodType = Dev.GetRandom(Of GoodType)(goods, rng)
        ship.AddGood(Good.Generate(gt, rng.Next(10, 30)))
    End Sub
    Private Class CrewPosition
        Public quarter As ShipQuarter
        Public role As CrewRole
        Public Sub New(ByVal aQuarter As ShipQuarter, ByVal aRole As CrewRole)
            quarter = aQuarter
            role = aRole
        End Sub
    End Class

    Public Function GetLoot(ByRef rng As Random) As List(Of Good)
        Dim total As New List(Of Good)
        For Each gt In [Enum].GetValues(GetType(GoodType))
            Dim qty As Integer = GetGood(gt)
            If qty > 0 Then
                Select Case Dev.FateRoll(World.Rng)
                    Case -4, -3 : qty = 0
                    Case -2 : qty *= 0.25
                    Case -1, 0, 1, 2 : qty *= 0.5
                    Case 3 : qty *= 0.75
                    Case 4
                End Select
                If qty > 0 Then total.Add(Good.Generate(gt, qty))
            End If
        Next
        Return total
    End Function

#Region "Movement"
    Public Overloads Sub CombatTick(ByVal playerShip As ShipPlayer)
        MyBase.TickCombat()
        PrimitiveRouting(playerShip.BattleSquare)
        PrimitiveAttack(playerShip)
    End Sub
    Private Sub PrimitiveAttack(ByRef playership As Ship)
        For Each weapon In CheckTarget(BattleSquare, playership.BattleSquare)
            Attack(weapon)
        Next
    End Sub
    Private Function CheckTarget(ByVal start As Battlesquare, ByVal playershipSquare As Battlesquare, Optional ByVal aFacing As BattleDirection = Nothing) As List(Of ShipWeapon)
        If aFacing = Nothing Then aFacing = Facing

        Dim total As New List(Of ShipWeapon)
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            For Each weapon In GetWeapons(quarter)
                If weapon.IsReady = False Then Continue For
                Dim targetSquares As Queue(Of Battlesquare) = start.GetSubjectiveAdjacents(aFacing, quarter, weapon.Range)
                While targetSquares.Count > 0
                    If targetSquares.Dequeue.Equals(playershipSquare) Then
                        total.Add(weapon)
                        Continue For
                    End If
                End While
            Next
        Next
        Return total
    End Function
    Private Function CheckTarget(ByVal start As BattlePosition(), ByVal playershipSquare As Battlesquare) As List(Of ShipWeapon)
        Dim bp As BattlePosition = start(start.Length - 1)
        Return CheckTarget(bp.Square, playershipSquare, bp.Facing)
    End Function
    Private Sub PrimitiveRouting(ByVal goal As Battlesquare)
        Dim firstPositions As New List(Of BattlePosition())
        Dim secondPositions As New Dictionary(Of BattlePosition(), List(Of BattlePosition()))

        'get all first-order positions (eg squares the ship can reach in one move)
        For Each mArray As MoveToken In AvailableMoves
            Dim firstPosition As BattlePosition() = BattleSquare.GetPathables(Facing, mArray)
            firstPositions.Add(firstPosition)
        Next

        'for all first-order positions, get second-order positions
        For Each firstPosition In firstPositions
            secondPositions.Add(firstPosition, New List(Of BattlePosition()))
            Dim fpReference As BattlePosition = firstPosition(firstPosition.Length - 1)

            Dim turn As Boolean = False
            If IgnoresJustTurned = False AndAlso (fpReference.ParentMove.Contains(BattleMove.TurnLeft) OrElse fpReference.ParentMove.Contains(BattleMove.TurnRight)) Then turn = True
            Dim availableMovesTrimmed As List(Of MoveToken) = TrimAvailableMoves(_AvailableMoves, turn, Waterline)
            For Each mArray As MoveToken In availableMovesTrimmed
                Dim secondPosition As BattlePosition() = fpReference.Square.GetPathables(fpReference.Facing, mArray)
                secondPositions(firstPosition).Add(secondPosition)
            Next
        Next

        'get cheapest (closest) second-order position
        'once found, work back up the chain to get first-order position
        'from first-order position, get the move required
        Dim targetFirstPosition As BattlePosition() = Nothing
        Dim targetPathCost As Double = Integer.MaxValue
        Dim targetMoves As MoveToken = Nothing
        Dim targetMoves2 As MoveToken = Nothing
        For Each fp As BattlePosition() In firstPositions
            For Each sp In secondPositions(fp)
                Dim pathCost As Double = GetHeuristicDistance(fp, sp, goal)
                If pathCost < targetPathCost Then
                    targetFirstPosition = fp
                    targetPathCost = pathCost
                    targetMoves = fp(0).ParentMove
                    targetMoves2 = sp(0).ParentMove
                End If
            Next
        Next

        'check to make sure that it's not halting randomly
        If targetMoves(0) = BattleMove.Halt Then
            If targetMoves2(0) <> BattleMove.Halt Then targetMoves = targetMoves2
        End If

        ExecuteMoves(targetMoves)
    End Sub
    Private Function GetHeuristicDistance(ByVal start As BattlePosition, ByVal goal As Battlesquare) As Double
        'manhattan distance as base
        Dim dx As Integer = Math.Abs(start.Square.X - goal.X)
        Dim dy As Integer = Math.Abs(start.Square.Y - goal.Y)
        Dim raw As Integer = (dx + dy) * 2

        'add terrain cost
        raw += start.PathingCost + goal.PathingCost

        Return raw * 2
    End Function
    Private Function GetHeuristicDistance(ByVal fp As BattlePosition(), ByVal sp As BattlePosition(), ByVal goal As Battlesquare) As Double
        Dim total As Double = 0
        Dim chain As New Queue(Of BattlePosition)
        For Each bp In fp
            chain.Enqueue(bp)
        Next
        For Each bp In sp
            chain.Enqueue(bp)
        Next
        Dim chainLength As Integer = chain.Count
        If fp(0).ParentMove = {BattleMove.Halt} Then chainLength -= 1
        If sp(0).ParentMove = {BattleMove.Halt} Then chainLength -= 1
        If chainLength <= 0 Then chainLength = 1

        While chain.Count > 1
            Dim current As BattlePosition = chain.Dequeue
            Dim nextCurrent As BattlePosition = chain.Peek
            total += GetHeuristicDistance(current, nextCurrent.Square)
        End While

        'consider ship position at the end of each move
        'facing with ready weapon is more valuable
        Dim weaponDiscount As Double = 0
        For Each weapon In CheckTarget(fp, goal)
            weaponDiscount += weapon.Heuristic
        Next
        For Each weapon In CheckTarget(sp, goal)
            weaponDiscount += weapon.Heuristic
        Next
        total -= weaponDiscount

        'divide total by chainlength, then...
        total /= chainLength
        total += GetHeuristicDistance(chain.Dequeue, goal)                  'add heuristic for sp(last)
        total += (GetHeuristicDistance(fp(fp.Length - 1), goal) / 3)        'and add heuristic for fp(last)

        'previous calculations divided at the end, which diminished the importance of the actual distance to the goal
        'by adding after the divison, the heuristic is on average 4x more imporant, as it should be

        Return total
    End Function
    Private Sub ExecuteMoves(ByVal targetMoves As MoveToken)
        If targetMoves.Length = 0 Then Exit Sub

        Dim moveReport As String = ""
        For n = 0 To targetMoves.Length - 1
            Dim m As BattleMove = targetMoves(n)
            moveReport &= m.ToString
            If n < targetMoves.Length - 1 Then moveReport &= " + "
        Next
        Debug.Print(Name & ": " & moveReport)
        MyBase.Move(targetMoves)
    End Sub
#End Region
End Class
