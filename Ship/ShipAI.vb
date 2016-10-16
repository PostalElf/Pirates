Public Class ShipAI
    Inherits Ship

    Public Sub New()
        MyBase.New()
    End Sub
    Public Shared Function Generate(ByVal type As ShipType, Optional ByVal faction As Faction = Nothing, Optional ByVal race As CrewRace = Nothing, Optional ByRef rng As Random = Nothing) As Ship
        If rng Is Nothing Then rng = New Random
        Dim cannons As New ShipWeapon("Cannons", 20, 10, DamageType.Cannon, 2, GoodType.Shot, 5, 2, 3)
        cannons.HullCost = 5
        Dim swivel As New ShipWeapon("Swivelgun", 10, 10, DamageType.Firearms, 1, GoodType.Bullets, 5, 1, 2)
        swivel.HullCost = 3
        Dim hailshot As New ShipWeapon("Hailshot", 20, 0, DamageType.Cannon, 2, GoodType.Shot, 2, 2, 4)
        hailshot.HullCost = 3
        Dim bombard As New ShipWeapon("Bombard", 30, 10, DamageType.Cannon, 2, GoodType.Explosive, 10, 3, 5)
        bombard.HullCost = 10
        Dim races As CrewRace() = [Enum].GetValues(GetType(CrewRace))
        Dim factions As Faction() = [Enum].GetValues(GetType(Faction))

        Dim ship As New ShipAI
        With ship
            If race = Nothing Then .Race = Dev.Rng.Next(1, races.Length + 1) Else .Race = race
            If faction = Nothing Then .Faction = Dev.Rng.Next(1, factions.Length + 1) Else .Faction = faction
            .Type = type
            .Name = GenerateName(rng)
            .ID = GenerateID(.Name)

            Select Case type
                Case ShipType.Sloop
                    GenerateWeapon(ship, cannons, ShipQuarter.Starboard)
                    GenerateWeapon(ship, swivel, ShipQuarter.Fore)

                Case ShipType.Schooner
                    GenerateWeapon(ship, cannons, ShipQuarter.Starboard)
                    GenerateWeapon(ship, cannons, ShipQuarter.Port)
                    GenerateWeapon(ship, hailshot, ShipQuarter.Fore)

                Case ShipType.Brigantine
                Case ShipType.Brig
                Case ShipType.Frigate
            End Select

            .HullSpace = Pirates.Ship.GenerateHullSpace(.Type)
            For Each q In [Enum].GetValues(GetType(ShipQuarter))
                .HullPoints(q) = Pirates.Ship.GenerateHullPoints(.Type)
                .MaxHullUse(q) = 10000
                .AddCrew(q, Crew.Generate(.Race), CrewRole.Sailor)
            Next
            GenerateStandardModules(ship, rng)

            'add loot
            For n = 1 To 3
                GenerateLoot(ship, rng)
            Next
        End With
        Return ship
    End Function
    Private Shared Sub GenerateStandardModules(ByRef ship As ShipAI, ByRef rng As Random)
        With ship
            Dim crewCount As Integer = ship.GetCrews(Nothing, Nothing).Count
            For n = 1 To Math.Ceiling(crewCount / 5)
                Dim quarter As ShipQuarter = rng.Next(1, 5)
                .AddModule(quarter, New ShipModule("Crew Quarters", ShipModule.ModuleType.Crew, 5, 0, False))
            Next

            If rng.Next(1, 3) = 1 Then
                .AddModule(ShipQuarter.Aft, New ShipModule("Aftcastle", ShipModule.ModuleType.Quarterdeck, 1, 0, True))
                .AddModule(ShipQuarter.Fore, New ShipModule("Helm", ShipModule.ModuleType.Helm, 1, 0, True))
            Else
                .AddModule(ShipQuarter.Fore, New ShipModule("Forecastle", ShipModule.ModuleType.Quarterdeck, 1, 0, True))
                .AddModule(ShipQuarter.Aft, New ShipModule("Helm", ShipModule.ModuleType.Helm, 1, 0, True))
            End If

            .AddModule(rng.Next(1, 5), New ShipModule("Maproom", ShipModule.ModuleType.Maproom, 1, 0, True))
        End With
    End Sub
    Private Shared Sub GenerateWeapon(ByRef ship As ShipAI, ByVal weaponTemplate As ShipWeapon, ByVal quarter As ShipQuarter)
        Dim weapon As ShipWeapon = weaponTemplate.Clone
        ship.AddWeapon(quarter, weapon)

        Dim ammoAmt As Integer = weapon.AmmoUse.Qty * 10
        Dim ammoType As GoodType = weapon.AmmoUse.Type
        If ammoType <> Nothing Then ship.AddGood(ammoType, ammoAmt)

        If weapon.CrewCount > 0 Then
            For n = 1 To weapon.CrewCount
                ship.AddCrew(quarter, Crew.Generate(ship.Race), CrewRole.Gunner)
            Next
        End If
    End Sub
    Private Shared Sub GenerateLoot(ByRef ship As ShipAI, ByRef rng As Random)
        Dim goods As New List(Of GoodType)([Enum].GetValues(GetType(GoodType)))
        Dim gt As GoodType = Dev.GetRandom(Of GoodType)(goods, rng)
        ship.AddGood(Good.Generate(gt, rng.Next(10, 30)))
    End Sub

#Region "Movement"
    Public Overloads Sub Tick(ByVal playerShip As ShipPlayer)
        MyBase.Tick()
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
