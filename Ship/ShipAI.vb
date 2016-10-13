﻿Public Class ShipAI
    Inherits Ship

    Public Sub New()
        MyBase.New()
    End Sub
    Public Shared Function Generate(ByVal type As ShipType, Optional ByVal faction As Faction = Nothing, Optional ByVal race As CrewRace = Nothing) As Ship
        Dim crate As New GoodCrate("Standard Shot Crate", GoodType.Shot, 50, 2)
        Dim bCrate As New GoodCrate("Standard Bullets Crate", GoodType.Bullets, 100, 2)
        Dim housing As New GoodCrate("Standard Quarters", GoodType.Crew, 10, 5)
        Dim cannons As New ShipWeapon("Cannons", 20, 10, DamageType.Cannon, 2, New Good(GoodType.Shot, 5), 2, 3)
        Dim swivel As New ShipWeapon("Swivelgun", 10, 10, DamageType.Firearms, 1, New Good(GoodType.Bullets, 5), 1, 2)
        cannons.HullCost = 5
        swivel.HullCost = 3

        Dim crewCount As New Dictionary(Of ShipQuarter, Integer)
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            crewCount.Add(q, 1)
        Next

        Dim ship As New ShipAI
        With ship
            If race = Nothing Then .Race = Dev.Rng.Next(1, 4) Else .Race = race
            .Faction = faction
            .Type = type
            .Name = GenerateName()
            .ID = GenerateID(.Name)

            Dim hullpoints As Integer
            Dim hullspace As Integer
            Select Case type
                Case ShipType.Sloop
                    hullpoints = 100
                    hullspace = 25
                    .AddCrate(housing.Clone)
                    .AddCrate(crate.Clone)
                    .AddGood(GoodType.Shot, 50)
                    .AddCrate(bCrate.Clone)
                    .AddGood(GoodType.Bullets, 50)
                    .AddWeapon(ShipQuarter.Starboard, cannons.Clone)
                    crewCount(ShipQuarter.Starboard) += 2
                    .AddWeapon(ShipQuarter.Fore, swivel.Clone)
                    crewCount(ShipQuarter.Fore) += 1

                Case ShipType.Schooner
                    hullpoints = 120
                    hullspace = 30


                Case ShipType.Brigantine
                    hullpoints = 200
                    hullspace = 50
                Case ShipType.Brig
                    hullpoints = 250
                    hullspace = 100
                Case ShipType.Frigate
                    hullpoints = 300
                    hullspace = 120
            End Select

            For Each q In [Enum].GetValues(GetType(ShipQuarter))
                .HullPoints(q) = hullpoints
                For n = 1 To crewCount(q)
                    Dim role As CrewSkill = CrewSkill.Gunnery
                    If n = 1 Then role = CrewSkill.Sailing
                    .AddCrew(q, Crew.Generate(.Race), role)
                Next
            Next
            .HullSpace = hullspace
        End With
        Return ship
    End Function

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
        For Each mArray As BattleMove() In AvailableMoves
            Dim firstPosition As BattlePosition() = BattleSquare.GetPathables(Facing, mArray)
            firstPositions.Add(firstPosition)
        Next

        'for all first-order positions, get second-order positions
        For Each firstPosition In firstPositions
            secondPositions.Add(firstPosition, New List(Of BattlePosition()))
            For Each mArray As BattleMove() In AvailableMoves
                Dim firstPositionReference As BattlePosition = firstPosition(firstPosition.Length - 1)
                Dim secondPosition As BattlePosition() = firstPositionReference.Square.GetPathables(firstPositionReference.Facing, mArray)
                secondPositions(firstPosition).Add(secondPosition)
            Next
        Next

        'get cheapest (closest) second-order position
        'once found, work back up the chain to get first-order position
        'from first-order position, get the move required
        Dim targetFirstPosition As BattlePosition() = Nothing
        Dim targetPathCost As Integer = Integer.MaxValue
        Dim targetMoves As BattleMove() = Nothing
        Dim targetMoves2 As BattleMove() = Nothing
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
    Private Sub ExecuteMoves(ByVal targetMoves As BattleMove())
        If targetMoves Is Nothing = False Then
            Dim moveReport As String = ""
            For n = 0 To targetMoves.Length - 1
                Dim m As BattleMove = targetMoves(n)
                moveReport &= m.ToString
                If n < targetMoves.Length - 1 Then moveReport &= " + "
            Next
            Debug.Print(Name & ": " & moveReport)
            MyBase.Move(targetMoves)
        End If
    End Sub
#End Region
End Class
