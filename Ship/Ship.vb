Public MustInherit Class Ship
    Implements BattlefieldObject, MeleeHost
    Public Property Name As String Implements BattlefieldObject.Name
    Private _ID As String
    Public Property ID As String
        Get
            If _ID = Nothing Then _ID = GenerateID(Name)
            Return _ID
        End Get
        Set(ByVal value As String)
            If value.Length > 3 Then value.Remove(3, value.Length - 3)
            _ID = value
        End Set
    End Property
    Protected Type As ShipType
    Public Race As CrewRace

    Public Sub New()
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            Weapons.Add(quarter, New List(Of ShipWeapon))

            DamageLog.Add(quarter, New List(Of Damage))
            DamageSustained.Add(quarter, 0)
            HullPoints.Add(quarter, 100)
            Crews.Add(quarter, New List(Of Crew))
            Modules.Add(quarter, New List(Of ShipModule))
            JustFired.Add(quarter, False)
        Next

        For Each gt In [Enum].GetValues(GetType(GoodType))
            Goods.Add(gt, Good.Generate(gt))
            GoodsFreeForConsumption.Add(gt, True)
        Next

        'note: ties in heuristic distance are broken by how high the move is up in the list
        _AvailableMoves.Add(New MoveToken({BattleMove.Forward, BattleMove.Forward}))
        _AvailableMoves.Add(New MoveToken({BattleMove.Forward, BattleMove.TurnLeft}))
        _AvailableMoves.Add(New MoveToken({BattleMove.Forward, BattleMove.TurnRight}))
        _AvailableMoves.Add(New MoveToken({BattleMove.Forward}))
        _AvailableMoves.Add(New MoveToken({BattleMove.Halt}))
    End Sub
    Public Overrides Function ToString() As String
        Dim total As String = Name
        If TypeOf Me Is ShipAI Then total &= " (AI)"
        Return total
    End Function
    Private Shared NamePrefixes As New List(Of String)
    Private Shared NameSuffixes As New List(Of String)
    Protected Shared Function GenerateName(ByRef rng As Random) As String
        If NamePrefixes.Count = 0 Then NamePrefixes = IO.SimpleFilegetAll("shipPrefixes.txt")
        If NameSuffixes.Count = 0 Then NameSuffixes = IO.SimpleFilegetAll("shipSuffixes.txt")

        Dim prefix As String = Dev.GrabRandom(NamePrefixes, rng)
        Dim suffix As String = Dev.GrabRandom(NameSuffixes, rng)
        Return prefix & " " & suffix
    End Function
    Protected Shared Function GenerateID(ByVal aName As String) As String
        Dim total As String = aName.Remove(3, aName.Length - 3)
        total = total.ToUpper
        Return total
    End Function
    Public Sub GenerateBaselines(ByVal aType As ShipType)
        Type = aType

        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            HullPoints(q) = GenerateHullPoints(aType)
        Next
        HullSpaceMax = GenerateHullSpace(aType)
        TonnageMax = GenerateTonnageMax(aType)
        Rigging = GenerateRigging(aType)
    End Sub
    Private Shared Function GenerateHullPoints(ByVal type As ShipType) As Integer
        Dim total As Integer() = {0, 100, 120, 150, 200, 250}
        Return total(type)
    End Function
    Private Shared Function GenerateHullSpace(ByVal type As ShipType) As Integer
        Dim total As Integer() = {0, 40, 75, 100, 150, 180}
        Return total(type)
    End Function
    Private Shared Function GenerateTonnageMax(ByVal type As ShipType) As Integer
        Dim total As Integer() = {0, 220, 250, 300, 350, 400}
        Return total(type)
    End Function
    Private Shared Function GenerateRigging(ByVal type As ShipType) As ShipRigging
        Select Case type
            Case ShipType.Sloop : Return New ShipRigging(1, ShipRigging.ShipRig.ForeAft)
            Case ShipType.Schooner : Return New ShipRigging(2, ShipRigging.ShipRig.ForeAft)
            Case ShipType.Brig : Return New ShipRigging(2, ShipRigging.ShipRig.Square)
            Case ShipType.Brigantine : Return New ShipRigging(2, ShipRigging.ShipRig.Mixed)
            Case ShipType.Frigate : Return New ShipRigging(3, ShipRigging.ShipRig.Square)
        End Select
    End Function

#Region "Specials"
    Protected IgnoresJustTurned As Boolean = False
    Protected IgnoresWaterline As Boolean = False
    Protected IgnoresDamage As Boolean = False

    Public Sub Cheaterbug(ByVal turn As Boolean, ByVal waterline As Boolean, ByVal damage As Boolean, ByVal weapon As Boolean)
        IgnoresJustTurned = turn
        IgnoresWaterline = waterline
        IgnoresDamage = damage
        For Each wlist In Weapons.Values
            For Each w In wlist
                If weapon = True Then w.Cheaterbug()
            Next
        Next
    End Sub
#End Region

#Region "Movement"
    Protected JustTurned As Boolean = False
    Protected _AvailableMoves As New List(Of MoveToken)
    Public Overridable ReadOnly Property AvailableMoves As List(Of MoveToken)
        Get
            Dim turn As Boolean
            Dim wline As ShipWaterline
            If IgnoresJustTurned = True Then turn = False Else turn = JustTurned
            If IgnoresWaterline = True Then wline = ShipWaterline.Unladen Else wline = Waterline
            Return TrimAvailableMoves(_AvailableMoves, turn, wline)
        End Get
    End Property
    Protected Shared Function TrimAvailableMoves(ByVal targetList As List(Of MoveToken), ByVal aJustTurned As Boolean, ByVal aWaterline As ShipWaterline) As List(Of MoveToken)
        Dim total As New List(Of MoveToken)
        For Each moves In targetList
            If aJustTurned = True OrElse aWaterline = ShipWaterline.Overladen Then
                If moves.Length = 1 Then total.Add(moves)
            Else
                total.Add(moves)
            End If
        Next
        Return total
    End Function

    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Protected Function TurnFacing(ByVal move As BattleMove, Optional ByVal initialFacing As BattleDirection = Nothing) As BattleDirection
        Dim f As BattleDirection = Facing
        If initialFacing <> Nothing Then f = initialFacing
        Select Case move
            Case BattleMove.TurnLeft : f -= 1
            Case BattleMove.TurnRight : f += 1
        End Select
        If f < 1 Then f = 4
        If f > 4 Then f = 1
        Return f
    End Function
    Protected Function TurnFacing(ByVal moves As MoveToken, Optional ByVal initialFacing As BattleDirection = Nothing) As BattleDirection
        Dim f As BattleDirection = Facing
        If initialFacing <> Nothing Then f = initialFacing

        For n = 0 To moves.Length - 1
            Dim m As BattleMove = moves(n)
            f = TurnFacing(m, f)
        Next
        Return f
    End Function
    Public ReadOnly Property PathingCost As Integer Implements BattlefieldObject.PathingCost
        Get
            Return 100
        End Get
    End Property

    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Sub Move(ByVal move As MoveToken) Implements BattlefieldObject.Move
        Dim turn As Boolean = False
        For n = 0 To move.Length - 1
            Dim d As BattleMove = move(n)
            Dim continueMovement As Boolean = True

            'turn if necessary
            Facing = TurnFacing(d)
            If d = BattleMove.TurnLeft OrElse d = BattleMove.TurnRight Then turn = True

            'set targetsquare if necessary
            Dim targetSquare As Battlesquare = Nothing
            If d = BattleMove.Forward Then
                targetSquare = BattleSquare.GetAdjacent(Facing, 1)
            ElseIf d = BattleMove.Backwards Then
                targetSquare = BattleSquare.GetSubjectiveAdjacent(Facing, ShipQuarter.Aft, 1)
            End If

            'if targetsquare is ticked, move
            If targetSquare Is Nothing = False Then
                If targetSquare.Contents Is Nothing Then SetSquare(targetSquare) Else continueMovement = targetSquare.Contents.MovedInto(Me)
            End If

            'check if continueMovement was flagged
            'disabled to allow for better movement
            'If continueMovement = False Then Exit For
        Next

        JustTurned = turn
    End Sub
    Public Sub SetSquare(ByVal targetSquare As Battlesquare)
        If BattleSquare Is Nothing = False Then BattleSquare.Contents = Nothing
        BattleSquare = targetSquare
        BattleSquare.Contents = Me
    End Sub
    Public Function MovedInto(ByRef bo As BattlefieldObject) As Boolean Implements BattlefieldObject.MovedInto
        'prevent running into self
        If bo.Equals(Me) Then Return False

        'bo is the attacker
        'this ship is the defender
        Dim rammedDamage As New Damage(20, 0, DamageType.Ramming, bo.Name)
        Dim attackDirection As BattleDirection
        For Each direction In [Enum].GetValues(GetType(BattleDirection))
            Dim current As BattlefieldObject = BattleSquare.GetAdjacent(direction, 1).Contents
            If current Is Nothing = False AndAlso current.Equals(bo) Then
                attackDirection = direction
                Exit For
            End If
        Next
        Dim targetQuarter As ShipQuarter = GetTargetQuarter(attackDirection)
        AddDamage(rammedDamage, targetQuarter, 5)

        Dim rammerDamage As New Damage(10, 0, DamageType.Ramming, Name)
        bo.Damage(rammerDamage, ShipQuarter.Fore, 5)

        Return False
    End Function

    Protected Rigging As ShipRigging
    Public Structure ShipRigging
        Public Masts As Integer
        Public Rig As ShipRig
        Public Sub New(ByVal aMasts As Integer, ByVal aRig As ShipRig)
            Masts = aMasts
            Rig = aRig
        End Sub
        Public Overrides Function ToString() As String
            Dim total As String = ""
            Select Case Masts
                Case 1 : total &= "Single-Masted"
                Case 2 : total &= "Double-Masted"
                Case 3 : total &= "Triple-Masted"
            End Select
            total &= " "
            Select Case Rig
                Case ShipRig.ForeAft : total &= "Fore-and-Aft Rig"
                Case ShipRig.Square : total &= "Square Rig"
                Case ShipRig.Mixed : total &= "Mixed Rig"
            End Select
            Return total
        End Function

        Public Enum ShipRig
            ForeAft
            Square
            Mixed
        End Enum
    End Structure
#End Region

#Region "Goods"
    Private Goods As New Dictionary(Of GoodType, Good)
    Private GoodsFreeForConsumption As New Dictionary(Of GoodType, Boolean)
    Public Function CheckGoodsFreeForConsumption(ByVal gt As GoodType) As Boolean Implements MeleeHost.CheckGoodsFreeForConsumption
        Return GoodsFreeForConsumption(gt)
    End Function
    Public Sub SetGoodsFreeForConsumption(ByVal gt As GoodType, ByVal value As Boolean)
        GoodsFreeForConsumption(gt) = value
    End Sub
    Private ReadOnly Property HoldSpaceMax As Double
        Get
            Dim total As Double = 0
            For Each hold In GetModules(ShipModule.ModuleType.Hold)
                total += hold.Capacity
            Next
            Return total
        End Get
    End Property
    Private ReadOnly Property HoldSpaceUsed As Double
        Get
            Dim total As Integer = 0
            For Each g In Goods.Values
                total += g.TotalMass
            Next
            Return total
        End Get
    End Property
    Public Function CheckAddGood(ByVal gt As GoodType, ByVal qty As Integer) As Boolean Implements MeleeHost.CheckAddGood
        Return CheckAddGood(Good.Generate(gt, qty))
    End Function
    Public Function CheckAddGood(ByVal good As Good) As Boolean
        If good.Qty > 0 Then
            If good.TotalMass + HoldSpaceUsed > HoldSpaceMax Then Return False
        ElseIf good.Qty < 0 Then
            If Goods(good.Type).Qty + good.Qty < 0 Then Return False
        End If
        Return True
    End Function
    Public Sub AddGood(ByVal gt As GoodType, ByVal qty As Integer) Implements MeleeHost.AddGood
        AddGood(Good.Generate(gt, qty))
    End Sub
    Public Sub AddGood(ByVal good As Good)
        Goods(good.Type) += good
    End Sub
    Public Function GetGood(ByVal gt As GoodType) As Good
        Return Goods(gt)
    End Function
    Public Function GetGoodConsumption(ByVal gt As GoodType) As Integer
        'return per diem consumption
        Dim total As Integer = 0
        For Each r As CrewRace In [Enum].GetValues(GetType(CrewRace))
            Dim crewCount As Integer = GetCrews(r).Count
            Select Case gt
                Case GoodType.Rations, GoodType.Water : If r <> CrewRace.Unrelinquished Then total += crewCount
                Case GoodType.Coffee, GoodType.Liqour : If r = CrewRace.Human Then total += crewCount
                Case GoodType.Salt : If r = CrewRace.Seatouched Then total += crewCount
                Case GoodType.Tobacco, GoodType.Spice : If r = CrewRace.Windsworn Then total += crewCount
                Case GoodType.Mordicus : If r = CrewRace.Unrelinquished Then total += crewCount
            End Select
        Next
        Return total
    End Function

    Protected TonnageMax As Integer
    Protected ReadOnly Property Tonnage As Double
        Get
            Dim total As Double = 0
            For Each wlist In Weapons.Values
                For Each w In wlist
                    total += w.Weight
                Next
            Next
            For Each mlist In Modules.Values
                For Each m In mlist
                    total += m.Weight
                Next
            Next
            For Each k In Goods.Keys
                total += Goods(k).TotalWeight
            Next
            For Each clist In Crews.Values
                total += (10 * clist.Count)
            Next
            Return total
        End Get
    End Property
    Protected ReadOnly Property Waterline As ShipWaterline
        Get
            Dim ratio As Double = Tonnage / TonnageMax * 100
            Select Case ratio
                Case Is <= 25 : Return ShipWaterline.Unladen
                Case Is <= 50 : Return ShipWaterline.Light
                Case Is <= 75 : Return ShipWaterline.Medium
                Case Is <= 100 : Return ShipWaterline.Heavy
                Case Else : Return ShipWaterline.Overladen
            End Select
        End Get
    End Property
#End Region

#Region "Modules"
    Private Modules As New Dictionary(Of ShipQuarter, List(Of ShipModule))
    Public Function CheckAddModule(ByVal quarter As ShipQuarter, ByVal m As ShipModule) As Boolean
        If m.HullCost + HullSpaceUsed > HullSpaceMax Then Return False
        If m.IsExclusive = False Then
            If GetModules(m.Type).Count > 0 Then Return False
        End If

        Return True
    End Function
    Public Sub AddModule(ByVal quarter As ShipQuarter, ByVal m As ShipModule)
        Modules(quarter).Add(m)
        m.Quarter = quarter
        m.Ship = Me
    End Sub
    Public Sub RemoveModule(ByVal quarter As ShipQuarter, ByRef m As ShipModule)
        If Modules(quarter).Contains(m) = False Then Exit Sub
        Modules(quarter).Remove(m)
        m.Quarter = Nothing
        m.Ship = Nothing
    End Sub
    Public Function GetModule(ByVal type As ShipModule.ModuleType, Optional ByVal quarter As ShipQuarter = Nothing) As ShipModule
        Dim mlist As List(Of ShipModule) = GetModules(type, quarter)
        If mlist.Count = 0 Then Return Nothing Else Return mlist(0)
    End Function
    Public Function GetModules(ByVal type As ShipModule.ModuleType, Optional ByVal quarter As ShipQuarter = Nothing) As List(Of ShipModule)
        Dim total As New List(Of ShipModule)
        For Each q In Modules.Keys
            If quarter = Nothing OrElse quarter = q Then
                For Each m In Modules(q)
                    If type = Nothing OrElse m.Type = type Then total.Add(m)
                Next
            End If
        Next
        Return total
    End Function
    Public Function GetModulesFree(ByVal type As ShipModule.ModuleType, ByVal aRace As CrewRace, Optional ByVal quarter As ShipQuarter = Nothing) As List(Of ShipModule)
        Dim total As New List(Of ShipModule)
        For Each m In GetModules(type, quarter)
            If (aRace = Nothing OrElse aRace = m.Race) AndAlso m.CapacityFree > 0 Then total.Add(m)
        Next
        Return total
    End Function

    Protected HullSpaceMax As Integer
    Protected ReadOnly Property HullSpaceUsed As Integer
        Get
            Dim total As Integer = 0
            For Each m In GetModules(Nothing, Nothing)
                total += m.HullCost
            Next
            For Each wList In Weapons.Values
                For Each w In wList
                    total += w.HullCost
                Next
            Next
            Return total
        End Get
    End Property
    Protected ReadOnly Property IsSeaworthy As Boolean
        Get
            'check to ensure that ship has helm, quarterdeck, maproom, and at least one quarter
            If GetModules(ShipModule.ModuleType.Helm).Count = 0 Then Return False
            If GetModules(ShipModule.ModuleType.Quarterdeck).Count = 0 Then Return False
            If GetModules(ShipModule.ModuleType.Maproom).Count = 0 Then Return False
            If GetModules(ShipModule.ModuleType.Quarters).Count = 0 Then Return False

            If GetCrews(Nothing, CrewRole.Helmsman).Count = 0 Then Return False
            If GetCrews(Nothing, CrewRole.Captain).Count = 0 Then Return False
            If GetCrews(Nothing, CrewRole.Navigator).Count = 0 Then Return False

            Return True
        End Get
    End Property
#End Region

#Region "Crew"
    Private Crews As New Dictionary(Of ShipQuarter, List(Of Crew))
    Public Function GetAvailableRoles(ByVal quarter As ShipQuarter) As List(Of CrewRole)
        Dim total As New List(Of CrewRole) From {CrewRole.Sailor}
        If GetWeapons(quarter).Count > 0 Then total.Add(CrewRole.Gunner)

        Dim types As ShipModule.ModuleType() = {ShipModule.ModuleType.Kitchen, ShipModule.ModuleType.Maproom, ShipModule.ModuleType.Helm, ShipModule.ModuleType.Laboratory, ShipModule.ModuleType.Apothecary}
        Dim roles As CrewRole() = {CrewRole.Cook, CrewRole.Navigator, CrewRole.Helmsman, CrewRole.Alchemist, CrewRole.Doctor}
        For n = 0 To types.Count - 1
            Dim type As ShipModule.ModuleType = types(n)
            Dim role As CrewRole = roles(n)
            Dim m As ShipModule = GetModule(type, quarter)
            If m Is Nothing = False Then total.Add(role)
        Next

        'quarterdeck special
        Dim q As ShipModule = GetModule(ShipModule.ModuleType.Quarterdeck, quarter)
        If q Is Nothing = False AndAlso q.CapacityFree > 0 Then total.AddRange({CrewRole.Captain, CrewRole.FirstMate})

        Return total
    End Function
    Public Function CheckAddCrew(ByVal quarter As ShipQuarter, ByVal crew As Crew, Optional ByVal role As CrewRole = Nothing) As Boolean
        Dim qlist As List(Of ShipModule) = GetModulesFree(ShipModule.ModuleType.Quarters, crew.Race, Nothing)
        If qlist.Count = 0 Then Return False
        If qlist(0).CapacityFree - 1 < 0 Then Return False

        Return True
    End Function
    Public Sub AddCrew(ByVal quarter As ShipQuarter, ByRef crew As Crew, Optional ByVal role As CrewRole = Nothing)
        Crews(quarter).Add(crew)
        crew.Ship = Me
        crew.ShipQuarter = quarter
        If role <> Nothing Then
            crew.SetStation(New CrewStation(quarter, role), True)
            crew.SetStation(New CrewStation(quarter, role), False)
            crew.Role = role
        End If

        Dim qlist As List(Of ShipModule) = GetModulesFree(ShipModule.ModuleType.Quarters, crew.Race, Nothing)
        If qlist.Count = 0 Then Exit Sub
        qlist(0).AddCrew(crew)
    End Sub
    Public Sub RemoveCrew(ByVal quarter As ShipQuarter, ByRef crew As Crew)
        If Crews(quarter).Contains(crew) = False Then Exit Sub
        Crews(quarter).Remove(crew)
        crew.Ship = Nothing
        crew.Quarters = Nothing
        crew.Shrine = Nothing
        crew.Role = Nothing
        crew.SetStation(Nothing, True)
        crew.SetStation(Nothing, False)
    End Sub
    Public Sub RemoveCrew(ByRef crew As Crew)
        For Each k In Crews.Keys
            If Crews(k).Contains(crew) Then
                RemoveCrew(k, crew)
                Exit Sub
            End If
        Next
    End Sub
    Public Sub MoveCrew(ByRef crew As Crew, ByVal targetQuarter As ShipQuarter, Optional ByVal newRole As CrewRole = Nothing)
        Crews(crew.ShipQuarter).Remove(crew)
        crew.ShipQuarter = targetQuarter
        Crews(targetQuarter).Add(crew)
        If newRole <> Nothing Then
            crew.Role = newRole
        End If
    End Sub
    Public Sub MoveCrew(ByRef crew As Crew, ByVal targetStation As CrewStation)
        Dim shipQuarter As ShipQuarter = targetStation.ShipQuarter
        Dim role As CrewRole = targetStation.Role
        MoveCrew(crew, shipQuarter, role)
    End Sub
    Public Sub MoveCrewToStation(ByRef crew As Crew)
        If InCombat = True Then MoveCrew(crew, crew.BattleStation) Else MoveCrew(crew, crew.Station)
    End Sub
    Public Function GetLeadership() As Integer
        'captain leadership x2, firstmate just by itself

        Dim leadership As Integer = 0
        Dim captain As Crew = GetCrew(Nothing, CrewRole.Captain)
        If captain Is Nothing = False Then leadership += captain.GetSkillFromRole * 2
        Dim firstmate As Crew = GetCrew(Nothing, CrewRole.FirstMate)
        If firstmate Is Nothing = False Then leadership += firstmate.GetSkillFromRole
        Return leadership
    End Function
    Public Function GetCrew(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As Crew
        Dim crewlist As List(Of Crew) = GetCrews(quarter, role)
        If crewlist.Count = 0 Then Return Nothing Else Return crewlist(0)
    End Function
    Public Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As List(Of Crew) Implements MeleeHost.GetCrews
        Dim total As New List(Of Crew)
        If quarter = Nothing Then
            For Each q In [Enum].GetValues(GetType(ShipQuarter))
                total.AddRange(GetCrews(q, role))
            Next
        ElseIf role = Nothing Then
            Return Crews(quarter)
        Else
            For Each c In Crews(quarter)
                If c.Role = role Then total.Add(c)
            Next
        End If
        Return total
    End Function
    Public Function GetCrews(ByVal r As CrewRace) As List(Of Crew)
        Dim total As New List(Of Crew)
        For Each crewlist In Crews.Values
            For Each Crew In crewlist
                If Crew.Race = r Then total.Add(Crew)
            Next
        Next
        Return total
    End Function
    Public Function GetBestCrew(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As Crew
        Dim total As List(Of Crew) = GetCrews(quarter, role)
        Dim bestCrew As Crew = Nothing
        Dim bestSkill As Integer = -10
        For Each c In total
            If c.GetSkillFromRole > bestSkill Then
                bestCrew = c
                bestSkill = c.GetSkillFromRole
            End If
        Next
        Return bestCrew
    End Function

    Public Mascot As ShipMascot
#End Region

#Region "Attack"
    Public JustFired As New Dictionary(Of ShipQuarter, Boolean)
    Protected Weapons As New Dictionary(Of ShipQuarter, List(Of ShipWeapon))
    Public Function CheckAddWeapon(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon) As Boolean
        If weapon.HullCost + HullSpaceUsed > HullSpaceMax Then Return False
        Dim matchedQuarter As Boolean = False
        For Each q In weapon.AvailableQuarters
            If q = quarter Then matchedQuarter = True : Exit For
        Next
        If matchedQuarter = False Then Return False

        Return True
    End Function
    Public Sub AddWeapon(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon)
        weapon.Ship = Me
        weapon.Quarter = quarter
        Weapons(quarter).Add(weapon)
    End Sub
    Public Sub RemoveWeapon(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon)
        weapon.Ship = Nothing
        weapon.Quarter = Nothing
        Weapons(quarter).Remove(weapon)
    End Sub
    Public Function GetWeapons(ByVal quarter As ShipQuarter) As List(Of ShipWeapon)
        Return Weapons(quarter)
    End Function
    Public Sub Attack(ByVal weapon As ShipWeapon)
        Dim quarter As ShipQuarter = weapon.Quarter
        If Weapons(quarter).Contains(weapon) = False Then Exit Sub
        If weapon.IsReady = False Then Exit Sub

        Dim attackTarget As BattlefieldObject = weapon.GetAttackTarget(Facing)
        If attackTarget Is Nothing Then Exit Sub
        Dim attackDirection As BattleDirection = GetAttackDirection(weapon, attackTarget)

        Dim repType As ReportType
        If TypeOf Me Is ShipPlayer Then repType = ReportType.PlayerShipAttack Else repType = ReportType.EnemyShipAttack
        Report.Add(Name & " fires its " & quarter.ToString & " " & weapon.Name & " at " & attackTarget.Name & ".", repType)
        JustFired(quarter) = True

        If weapon.Name <> "Grappling Hooks" Then
            weapon.Attack(attackDirection, attackTarget, GetCrews(quarter, CrewRole.Gunner))
        Else
            If TypeOf attackTarget Is Ship = False Then Exit Sub
            Dim attackShip As Ship = CType(attackTarget, Ship)
            Dim attackQuarter As ShipQuarter = BattleSquare.ReverseDirection(attackShip.GetTargetQuarter(attackDirection))

            Dim melee As New Melee(Me, quarter, attackShip, attackQuarter)
            Dim battlefield As Battlefield = BattleSquare.Battlefield
            battlefield.AddMelee(melee)

            Report.Add(Name & " and " & attackTarget.Name & " are joined in melee!", ReportType.Melee)
        End If
    End Sub
    Private Function GetAttackDirection(ByVal weapon As ShipWeapon, ByVal attackTarget As BattlefieldObject) As BattleDirection
        Dim attackDirection As BattleDirection
        For Each direction In [Enum].GetValues(GetType(BattleDirection))
            Dim targetSquares As Queue(Of Battlesquare) = attackTarget.BattleSquare.GetAdjacents(direction, weapon.Range)
            While targetSquares.Count > 0
                If targetSquares.Dequeue.Equals(BattleSquare) Then
                    attackDirection = direction
                    Exit For
                End If
            End While
        Next
        Return attackDirection
    End Function
    Private Function GetTargetQuarter(ByVal attackDirection As BattleDirection) As ShipQuarter Implements BattlefieldObject.GetTargetQuarter
        'determine target quarter by ship facing and attackDirection
        Select Case Facing
            Case BattleDirection.North
                Select Case attackDirection
                    Case BattleDirection.North : Return ShipQuarter.Fore
                    Case BattleDirection.East : Return ShipQuarter.Starboard
                    Case BattleDirection.South : Return ShipQuarter.Aft
                    Case BattleDirection.West : Return ShipQuarter.Port
                End Select
            Case BattleDirection.East
                Select Case attackDirection
                    Case BattleDirection.North : Return ShipQuarter.Port
                    Case BattleDirection.East : Return ShipQuarter.Fore
                    Case BattleDirection.South : Return ShipQuarter.Starboard
                    Case BattleDirection.West : Return ShipQuarter.Aft
                End Select
            Case BattleDirection.South
                Select Case attackDirection
                    Case BattleDirection.North : Return ShipQuarter.Aft
                    Case BattleDirection.East : Return ShipQuarter.Port
                    Case BattleDirection.South : Return ShipQuarter.Fore
                    Case BattleDirection.West : Return ShipQuarter.Starboard
                End Select
            Case BattleDirection.West
                Select Case attackDirection
                    Case BattleDirection.North : Return ShipQuarter.Starboard
                    Case BattleDirection.East : Return ShipQuarter.Aft
                    Case BattleDirection.South : Return ShipQuarter.Port
                    Case BattleDirection.West : Return ShipQuarter.Fore
                End Select
        End Select

        Throw New Exception("Battledirection or Facing invalid.")
        Return Nothing
    End Function

    Public InCombat As Boolean = False
    Public HasHealed As Boolean = False
    Public Property InMelee As Boolean = False Implements MeleeHost.InMelee
    Private DamageSustained As New Dictionary(Of ShipQuarter, Integer)
    Protected HullPoints As New Dictionary(Of ShipQuarter, Integer)
    Private DamageLog As New Dictionary(Of ShipQuarter, List(Of Damage))
    Public Sub AddDamage(ByVal damage As Damage, ByVal targetQuarter As ShipQuarter, ByVal accuracy As Integer) Implements BattlefieldObject.Damage
        If IgnoresDamage = True Then Exit Sub

        If damage.ShipDamage > 0 Then
            Report.Add(Name & " " & targetQuarter.ToString & " suffered " & damage.ShipDamage & " damage.", ReportType.ShipDamage)
            DamageSustained(targetQuarter) += damage.ShipDamage
            DamageLog(targetQuarter).Add(damage)
        End If
        If damage.CrewDamage > 0 Then
            For Each Crew In GetCrews(targetQuarter, Nothing)
                Crew.ShipAttack(accuracy, damage.Clone())
            Next
        End If

        If DamageSustained(targetQuarter) >= HullPoints(targetQuarter) Then
            BattleSquare.Battlefield.AddDead(Me)
            Report.Add(Name & " has been destroyed!", ReportType.ShipDeath)
        End If
    End Sub
    Public Sub RepairDamage(ByVal dmg As Damage, Optional ByVal quarter As ShipQuarter = Nothing)
        If quarter = Nothing Then
            For Each q In [Enum].GetValues(GetType(ShipQuarter))
                If DamageLog(q).Contains(dmg) Then quarter = q : Exit For
            Next
        End If
        If quarter = Nothing Then Exit Sub

        DamageLog(quarter).Remove(dmg)
        DamageSustained(quarter) -= dmg.ShipDamage
        HasHealed = True
    End Sub
    Public Function GetWorstDamage() As Damage
        If DamageLog.Count = 0 Then Return Nothing

        Dim worstDamage As New Damage(-1, -1, Nothing, "")
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            For Each dmg In DamageLog(q)
                If dmg.ShipDamage > worstDamage.ShipDamage Then worstDamage = dmg
            Next
        Next
        If worstDamage.ShipDamage = -1 Then Return Nothing Else Return worstDamage
    End Function
    Public Sub EnterCombat()
        InCombat = True
    End Sub
    Public Sub TickCombat() Implements BattlefieldObject.CombatTick
        For Each q In Weapons.Keys
            For Each w In Weapons(q)
                w.TickCombat()
            Next
            JustFired(q) = False
        Next
    End Sub
    Public Sub EndCombat()
        InCombat = False
    End Sub
#End Region

#Region "Console Display"
    Public Property ConsoleColour As ConsoleColor
    Public Sub ConsoleWrite() Implements BattlefieldObject.ConsoleWrite
        Console.ForegroundColor = ConsoleColour
        Select Case Facing
            Case BattleDirection.North : Console.Write("↑")
            Case BattleDirection.South : Console.Write("↓")
            Case BattleDirection.East : Console.Write("→")
            Case BattleDirection.West : Console.Write("←")
        End Select
    End Sub
    Public Overridable Sub ConsoleReport()
        Const s As Integer = 12
        Dim t As String = Dev.vbSpace(1)

        Console.WriteLine("[" & ID & "] " & Name)
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            Console.Write(t & Dev.vbTab(q.ToString & ":", s))
            Console.Write("Damage " & DamageSustained(q) & "/" & HullPoints(q))
            Console.Write(" - Sails " & GetCrews(q, CrewRole.Sailor).Count)
            Console.Write(" - Guns " & GetCrews(q, CrewRole.Gunner).Count)
            Dim commandCount As Integer = 0
            For Each r In {CrewRole.Captain, CrewRole.FirstMate, CrewRole.Helmsman}
                commandCount += GetCrews(q, r).Count
            Next
            Console.Write(" - Command " & commandCount)
            Console.WriteLine()
        Next
        Console.WriteLine(t & Dev.vbTab("Leadership:", s) & GetLeadership())
        Console.WriteLine(t & Dev.vbTab("Hullspace:", s) & HullSpaceUsed & "/" & HullSpaceMax)
        Console.Write(t & Dev.vbTab("Tonnage:", s) & Tonnage.ToString("0.0") & "/" & TonnageMax)
        Dim ratio As Double = Tonnage / TonnageMax * 100
        Console.WriteLine(" (" & ratio.ToString("0.0") & "% - " & Waterline.ToString & ")")
    End Sub
#End Region
End Class
