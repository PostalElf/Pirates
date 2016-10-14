Public Class ShipAI
    Inherits Ship

    Public Sub New()
        MyBase.New()
    End Sub
    Public Shared Function Generate(ByVal type As ShipType, Optional ByVal faction As Faction = Nothing, Optional ByVal race As CrewRace = Nothing) As Ship
        Dim cannons As New ShipWeapon("Cannons", 20, 10, DamageType.Cannon, 2, GoodType.Shot, 5, 2, 3)
        cannons.HullCost = 5
        Dim swivel As New ShipWeapon("Swivelgun", 10, 10, DamageType.Firearms, 1, GoodType.Bullets, 5, 1, 2)
        swivel.HullCost = 3
        Dim hailshot As New ShipWeapon("Hailshot", 20, 0, DamageType.Cannon, 2, GoodType.Shot, 2, 2, 4)
        hailshot.HullCost = 3
        Dim bombard As New ShipWeapon("Bombard", 30, 10, DamageType.Cannon, 2, GoodType.Explosive, 10, 3, 5)
        bombard.HullCost = 10

        Dim ship As New ShipAI
        With ship
            If race = Nothing Then .Race = Dev.Rng.Next(1, 4) Else .Race = race
            .Faction = faction
            .Type = type
            .Name = GenerateName()
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
            .AddModule(ShipQuarter.Fore, New ShipModule("Quarters", ShipModule.ModuleType.Crew, 5, 1))

            For Each q In [Enum].GetValues(GetType(ShipQuarter))
                .HullPoints(q) = Pirates.Ship.GenerateHullPoints(.Type)
                .AddCrew(q, Crew.Generate(.Race), CrewSkill.Sailing)
            Next

            'add loot
            For n = 1 To 3
                Dim gt As GoodType = Dev.Rng.Next(1, [Enum].GetValues(GetType(GoodType)).Length)
                .AddGood(Good.Generate(gt, Dev.Rng.Next(10, 30)))
            Next
        End With
        Return ship
    End Function
    Private Shared Sub GenerateWeapon(ByRef ship As ShipAI, ByVal weaponTemplate As ShipWeapon, ByVal quarter As ShipQuarter)
        Dim weapon As ShipWeapon = weaponTemplate.Clone
        ship.AddWeapon(quarter, weapon)

        Const ammoAmt As Integer = 50
        Dim ammoType As GoodType = weapon.AmmoUse.Type
        If ammoType <> Nothing Then ship.AddGood(ammoType, ammoAmt)

        If weapon.CrewCount > 0 Then
            For n = 1 To weapon.CrewCount
                ship.AddCrew(quarter, Crew.Generate(ship.Race), CrewSkill.Gunnery)
            Next
        End If
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
    Private Function CheckTarget(ByVal start As Battlesquare, ByVal playershipSquare As Battlesquare) As List(Of ShipWeapon)
        Dim total As New List(Of ShipWeapon)
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            For Each weapon In GetWeapons(quarter)
                If weapon.IsReady = False Then Continue For
                Dim targetSquares As Queue(Of Battlesquare) = start.GetSubjectiveAdjacents(Facing, quarter, weapon.Range)
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
        Dim targetPathCost As Integer = Integer.MaxValue
        Dim targetMoves As MoveToken = Nothing
        Dim targetMoves2 As MoveToken = Nothing
        For Each fp As BattlePosition() In firstPositions
            For Each sp In secondPositions(fp)
                Dim pathCost As Integer = GetHeuristicDistance(fp, sp(sp.Length - 1).Square) + GetHeuristicDistance(sp, goal)
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
        Dim raw As Integer = dx + dy

        'add terrain cost
        raw += start.Square.PathingCost + goal.PathingCost

        'consider ship position; facing with ready weapon is more valuable
        For Each weapon In CheckTarget(start.Square, goal)
            raw -= 10
        Next

        Return raw
    End Function
    Private Function GetHeuristicDistance(ByVal start As BattlePosition(), ByVal goal As Battlesquare) As Double
        Dim total As Integer = 0
        For Each bp In start
            total += GetHeuristicDistance(bp, goal)
        Next

        If start.Length = 1 Then total *= 2
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
