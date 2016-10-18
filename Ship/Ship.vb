Public MustInherit Class Ship
    Implements BattlefieldObject
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
    Protected Race As CrewRace
    Public Faction As faction

    Public Sub New()
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            Weapons.Add(quarter, New List(Of ShipWeapon))

            DamageSustained.Add(quarter, 0)
            HullPoints.Add(quarter, 100)
            Crews.Add(quarter, New List(Of Crew))
            Modules.Add(quarter, New List(Of ShipModule))
            JustFired.Add(quarter, False)
        Next

        For Each gt In [Enum].GetValues(GetType(GoodType))
            Goods.Add(gt, Good.Generate(gt))
            GoodsFreeForConsumption.Add(gt, False)
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
    End Sub
    Protected Shared Function GenerateHullPoints(ByVal type As ShipType) As Integer
        Dim total As Integer() = {0, 100, 120, 150, 200, 250}
        Return total(type)
    End Function
    Protected Shared Function GenerateHullSpace(ByVal type As ShipType) As Integer
        Dim total As Integer() = {0, 40, 75, 100, 150, 180}
        Return total(type)
    End Function
    Protected Shared Function GenerateTonnageMax(ByVal type As ShipType) As Integer
        Dim total As Integer() = {0, 100, 120, 150, 200, 225}
        Return total(type)
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
        Damage(rammedDamage, targetQuarter, 5)

        Dim rammerDamage As New Damage(10, 0, DamageType.Ramming, Name)
        bo.Damage(rammerDamage, ShipQuarter.Fore, 5)

        Return False
    End Function
#End Region

#Region "Goods"
    Protected Goods As New Dictionary(Of GoodType, Good)
    Public GoodsFreeForConsumption As New Dictionary(Of GoodType, Boolean)
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
    Public Function CheckAddGood(ByVal gt As GoodType, ByVal qty As Integer) As Boolean
        Return CheckAddGood(Good.Generate(gt, qty))
    End Function
    Public Function CheckAddGood(ByVal good As Good) As Boolean
        If good.TotalMass + HoldSpaceUsed > HoldSpaceMax Then Return False
        Return True
    End Function
    Public Sub AddGood(ByVal good As Good)
        Goods(good.Type) += good
    End Sub
    Public Sub AddGood(ByVal gt As GoodType, ByVal qty As Integer)
        AddGood(Good.Generate(gt, qty))
    End Sub
    Public Function GetGood(ByVal gt As GoodType) As Integer
        Return Goods(gt).Qty
    End Function

    Protected TonnageMax As Integer
    Private ReadOnly Property Tonnage As Double
        Get
            Dim total As Double = 0
            For Each wlist In Weapons.Values
                For Each w In wlist
                    total += w.Weight
                Next
            Next
            For Each mlist In Modules.Values
                For Each m In mlist
                    total += m.weight
                Next
            Next
            For Each k In Goods.Keys
                total += Goods(k).TotalWeight
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
    Private ReadOnly Property HullSpaceUsed As Integer
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
    Private ReadOnly Property IsSeaworthy As Boolean
        Get
            'check to ensure that ship has helm, quarterdeck, maproom, and at least one quarter
            If GetModules(ShipModule.ModuleType.Helm).Count = 0 Then Return False
            If GetModules(ShipModule.ModuleType.Quarterdeck).Count = 0 Then Return False
            If GetModules(ShipModule.ModuleType.Maproom).Count = 0 Then Return False
            If GetModules(ShipModule.ModuleType.Crew).Count = 0 Then Return False

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
        If GetModules(ShipModule.ModuleType.Kitchen, quarter).Count > 0 Then total.Add(CrewRole.Cook)
        If GetModules(ShipModule.ModuleType.Maproom, quarter).Count > 0 Then total.Add(CrewRole.Navigator)
        If GetModules(ShipModule.ModuleType.Helm, quarter).Count > 0 Then total.AddRange({CrewRole.Helmsman, CrewRole.FirstMate})
        If GetModules(ShipModule.ModuleType.Quarterdeck, quarter).Count > 0 Then total.Add(CrewRole.Captain)
        If GetModules(ShipModule.ModuleType.Laboratory, quarter).Count > 0 Then total.Add(CrewRole.Alchemist)
        Return total
    End Function
    Public Function CheckAddCrew(ByVal quarter As ShipQuarter, ByVal crew As Crew, Optional ByVal role As CrewRole = Nothing) As Boolean
        Dim qlist As List(Of ShipModule) = GetModulesFree(ShipModule.ModuleType.Crew, crew.Race, Nothing)
        If qlist.Count = 0 Then Return False
        If qlist(0).CapacityFree - 1 < 0 Then Return False

        Return True
    End Function
    Public Sub AddCrew(ByVal quarter As ShipQuarter, ByRef crew As Crew, Optional ByVal role As CrewRole = Nothing)
        Crews(quarter).Add(crew)
        crew.Ship = Me
        crew.ShipQuarter = quarter
        If role <> Nothing Then crew.Role = role

        Dim qlist As List(Of ShipModule) = GetModulesFree(ShipModule.ModuleType.Crew, crew.Race, Nothing)
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
    End Sub
    Public Sub RemoveCrew(ByRef crew As Crew)
        For Each k In Crews.Keys
            If Crews(k).Contains(crew) Then
                Crews(k).Remove(crew)
                crew.Ship = Nothing
                crew.Quarters = Nothing
                crew.Shrine = Nothing
                crew.Role = Nothing
                Exit Sub
            End If
        Next
    End Sub
    Public Sub MoveCrew(ByRef crew As Crew, ByVal targetQuarter As ShipQuarter, Optional ByVal newRole As CrewRole = Nothing)
        Crews(crew.ShipQuarter).Remove(crew)
        Crews(targetQuarter).Add(crew)
        If newRole <> Nothing Then crew.Role = newRole
    End Sub
    Public Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As List(Of Crew)
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
    Public Function GetCrewsByRace(ByVal r As CrewRace) As List(Of Crew)
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
    Public Function GetSkill(ByVal quarter As ShipQuarter, ByVal skill As CrewSkill) As Integer
        Dim total As Integer = 0
        Select Case skill
            Case CrewSkill.Leadership
                For Each c In GetCrews(quarter, CrewRole.Captain)
                    total += c.GetSkillFromRole * 2
                Next
                For Each c In GetCrews(quarter, CrewRole.FirstMate)
                    total += c.GetSkillFromRole
                Next
            Case Else
                Dim role As CrewRole = Crew.ConvertSkillToRole(skill)
                For Each c In GetCrews(quarter, role)
                    total += c.GetSkillFromRole
                Next
        End Select
        Return total
    End Function

    Public Sub Tick()
        Dim doctor As Crew = GetBestCrew(Nothing, CrewRole.Doctor)

        For Each CrewList In Crews.Values
            For Each Crew In CrewList
                Crew.Tick(doctor)
            Next
        Next
    End Sub
#End Region

#Region "Attack"
    Public JustFired As New Dictionary(Of ShipQuarter, Boolean)
    Protected Weapons As New Dictionary(Of ShipQuarter, List(Of ShipWeapon))
    Public Function CheckAddWeapon(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon) As Boolean
        If weapon.HullCost + HullSpaceUsed > HullSpaceMax Then Return False

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
        Dim attackDirection As BattleDirection = GetAttackDirection(weapon, attackTarget)

        Dim repType As ReportType
        If TypeOf Me Is ShipPlayer Then repType = ReportType.PlayerShipAttack Else repType = ReportType.EnemyShipAttack
        Report.Add(Name & " fires its " & quarter.ToString & " " & weapon.Name & " at " & attackTarget.Name & ".", repType)
        JustFired(quarter) = True

        If weapon.Name <> "Grappling Hooks" Then
            weapon.Attack(attackDirection, attackTarget, GetCrews(quarter, CrewSkill.Gunnery))
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

    Public InMelee As Boolean = False
    Private DamageSustained As New Dictionary(Of ShipQuarter, Integer)
    Protected HullPoints As New Dictionary(Of ShipQuarter, Integer)
    Private DamageLog As New List(Of Damage)
    Private Sub Damage(ByVal damage As Damage, ByVal targetQuarter As ShipQuarter, ByVal accuracy As Integer) Implements BattlefieldObject.Damage
        If IgnoresDamage = True Then Exit Sub

        If damage.ShipDamage > 0 Then
            Report.Add(Name & " suffered " & damage.ShipDamage & " damage (" & targetQuarter.ToString & ").", ReportType.ShipDamage)
            DamageSustained(targetQuarter) += damage.ShipDamage
            DamageLog.Add(damage)
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
    Public Sub EnterCombat(ByRef battlefield As Battlefield, ByRef combatantList As List(Of Ship))
        combatantList.Add(Me)
    End Sub
    Public Sub TickCombat() Implements BattlefieldObject.CombatTick
        For Each q In Weapons.Keys
            For Each w In Weapons(q)
                w.TickCombat()
            Next
            JustFired(q) = False
        Next
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
        Console.WriteLine(t & Dev.vbTab("Hullspace:", s) & HullSpaceUsed & "/" & HullSpaceMax)
        Console.Write(t & Dev.vbTab("Tonnage:", s) & Tonnage.ToString("0.0") & "/" & TonnageMax)
        Dim ratio As Double = Tonnage / TonnageMax * 100
        Console.WriteLine(" (" & ratio.ToString("0.0") & "% - " & Waterline.ToString & ")")
    End Sub
#End Region
End Class
