Public MustInherit Class Ship
    Implements BattlefieldObject
    Public Property Name As String Implements BattlefieldObject.Name
    Private _ID As String
    Public Property ID As String
        Get
            If _ID = Nothing Then
                _ID = Name.Remove(3, Name.Length - 3)
                _ID = _ID.ToUpper
            End If
            Return _ID
        End Get
        Set(ByVal value As String)
            If value.Length > 3 Then value.Remove(3, value.Length - 3)
            _ID = value
        End Set
    End Property
    Public Faction As faction

    Public Sub New()
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            Weapons.Add(quarter, New List(Of ShipWeapon))

            DamageSustained.Add(quarter, 0)
            HullPoints.Add(quarter, 100)
            Crews.Add(quarter, New List(Of Crew))
        Next

        For Each gt In [Enum].GetValues(GetType(GoodType))
            Goods.Add(gt, 0)
        Next

        'note: ties in heuristic distance are broken by how high the move is up in the list
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.Forward})
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.TurnLeft})
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.TurnRight})
        _AvailableMoves.Add({BattleMove.Forward})
        _AvailableMoves.Add({BattleMove.Halt})
    End Sub

#Region "Specials"
    Protected IgnoresJustTurned As Boolean = False

    Public Sub Cheaterbug()
        IgnoresJustTurned = True
        For Each wlist In Weapons.Values
            For Each w In wlist
                w.Cheaterbug()
            Next
        Next
    End Sub
#End Region

#Region "Movement"
    Protected JustTurned As Boolean = False
    Private _AvailableMoves As New List(Of BattleMove())
    Public ReadOnly Property AvailableMoves As List(Of BattleMove())
        Get
            If IgnoresJustTurned = False AndAlso JustTurned = True Then
                Dim total As New List(Of BattleMove())
                For Each moves In _AvailableMoves
                    If moves.Length = 1 Then total.Add(moves)
                Next
                Return total
            Else
                Return _AvailableMoves
            End If
        End Get
    End Property

    Public Property Facing As BattleDirection Implements BattlefieldObject.Facing
    Protected Function TurnFacing(ByVal move As BattleMove, Optional ByVal initialFacing As BattleDirection = Nothing) As BattleDirection
        Dim f As BattleDirection = Facing
        If initialFacing <> Nothing Then f = initialFacing
        Select Case move
            Case BattleMove.TurnLeft : f -= 1
            Case BattleMove.TurnRight : f += 1
        End Select
        If f < 0 Then f = 3
        If f > 3 Then f = 0
        Return f
    End Function
    Protected Function TurnFacing(ByVal moves As BattleMove(), Optional ByVal initialFacing As BattleDirection = Nothing) As BattleDirection
        Dim f As BattleDirection = Facing
        If initialFacing <> Nothing Then f = initialFacing

        For Each m In moves
            f = TurnFacing(m, f)
        Next
        Return f
    End Function
    Public ReadOnly Property PathingCost As Integer Implements BattlefieldObject.PathingCost
        Get
            Return 10
        End Get
    End Property

    Public Property BattleSquare As Battlesquare Implements BattlefieldObject.BattleSquare
    Public Sub Move(ByVal move As BattleMove()) Implements BattlefieldObject.Move
        Dim turn As Boolean = False
        For Each d In move
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
            If continueMovement = False Then Exit For
        Next

        JustTurned = turn
    End Sub
    Public Sub SetSquare(ByVal targetSquare As Battlesquare)
        If BattleSquare Is Nothing = False Then BattleSquare.Contents = Nothing
        BattleSquare = targetSquare
        BattleSquare.Contents = Me
    End Sub
    Public Function MovedInto(ByRef bo As BattlefieldObject) As Boolean Implements BattlefieldObject.MovedInto
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
    Private HullSpace As Integer
    Private Crates As New List(Of Crate)
    Private Function GetCarryingCapacity(ByVal gt As GoodType) As Integer
        Dim total As Integer = 0
        For Each Crate In Crates
            If Crate.GoodType = gt Then total += Crate.Capacity
        Next
        Return total
    End Function
    Public Function CheckAddCrate(ByVal crate As Crate) As Boolean
        If HullSpace - crate.HullCost < 0 Then Return False
        Return True
    End Function
    Public Sub AddCrate(ByVal crate As Crate)
        Crates.Add(crate)
        HullSpace -= crate.HullCost
    End Sub
    Public Sub RemoveCrate(ByVal crate As Crate)
        If Crates.Contains(crate) = False Then Exit Sub
        Crates.Remove(crate)
        HullSpace += crate.HullCost
    End Sub

    Private Goods As New Dictionary(Of GoodType, Integer)
    Public Function GetGood(ByVal gt As GoodType) As Integer
        Return Goods(gt)
    End Function
    Public Function CheckAddGood(ByVal gt As GoodType, ByVal value As Integer) As Boolean
        If Goods(gt) + value > GetCarryingCapacity(gt) Then Return False
        Return True
    End Function
    Public Sub AddGood(ByVal gt As GoodType, ByVal value As Integer)
        Goods(gt) += value
        Dev.Constrain(Goods(gt), 0, GetCarryingCapacity(gt))
    End Sub

    Public Class Crate
        Public Name As String
        Public GoodType As GoodType
        Public Capacity As Integer
        Public HullCost As Integer
    End Class
#End Region

#Region "Crew"
    Private Crews As New Dictionary(Of ShipQuarter, List(Of Crew))
    Public Sub AddCrew(ByVal quarter As ShipQuarter, ByRef crew As Crew, Optional ByVal role As CrewSkill = Nothing)
        Crews(quarter).Add(crew)
        crew.Ship = Me
        If role <> Nothing Then crew.Role = role
    End Sub
    Public Sub RemoveCrew(ByVal quarter As ShipQuarter, ByRef crew As Crew)
        If Crews(quarter).Contains(crew) = False Then Exit Sub
        Crews(quarter).Remove(crew)
        crew.Ship = Nothing
        crew.Role = Nothing
    End Sub
    Public Sub RemoveCrew(ByRef crew As Crew)
        For Each k In Crews.Keys
            If Crews(k).Contains(crew) Then
                Crews(k).Remove(crew)
                crew.Ship = Nothing
                crew.Role = Nothing
                Exit Sub
            End If
        Next
    End Sub
    Public Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewSkill) As List(Of Crew)
        If role = Nothing Then Return Crews(quarter)

        Dim total As New List(Of Crew)
        For Each c In Crews(quarter)
            If c.Role = role Then total.Add(c)
        Next
        Return total
    End Function
#End Region

#Region "Attack"
    Protected Weapons As New Dictionary(Of ShipQuarter, List(Of ShipWeapon))
    Public Sub AddWeapon(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon)
        weapon.Ship = Me
        weapon.Quarter = quarter
        Weapons(quarter).Add(weapon)
    End Sub
    Public Function GetWeapons(ByVal quarter As ShipQuarter) As List(Of ShipWeapon)
        Return Weapons(quarter)
    End Function
    Public Sub Attack(ByVal weapon As ShipWeapon)
        Dim quarter As ShipQuarter = weapon.Quarter
        If Weapons(quarter).Contains(weapon) = False Then Exit Sub
        If weapon.IsReady = False Then Exit Sub

        Dim range As Integer = weapon.Range
        Dim attackSquare As Battlesquare = BattleSquare.GetSubjectiveAdjacent(Facing, quarter, range)
        Dim attackTarget As BattlefieldObject = attackSquare.Contents
        Dim attackDirection As BattleDirection
        For Each direction In [Enum].GetValues(GetType(BattleDirection))
            Dim targetSquare As Battlesquare = attackSquare.GetAdjacent(direction, range)
            If targetSquare.Equals(BattleSquare) Then
                attackDirection = direction
                Exit For
            End If
        Next

        If weapon.Name <> "Grappling Hooks" Then
            weapon.Attack(attackDirection, attackTarget, GetCrews(quarter, CrewSkill.Gunnery))
        Else
            If TypeOf attackTarget Is Ship = False Then Exit Sub
            Dim attackShip As Ship = CType(attackTarget, Ship)
            Dim attackQuarter As ShipQuarter = BattleSquare.ReverseDirection(attackShip.GetTargetQuarter(attackDirection))

            Dim melee As New Melee(Me, quarter, attackShip, attackQuarter)
            Dim battlefield As Battlefield = BattleSquare.Battlefield
            melee.Battlefield = battlefield
            battlefield.Melees.Add(melee)

            Report.Add(Name & " (" & GetCrews(quarter, Nothing).Count & ") and " & attackTarget.Name & " (" & attackShip.GetCrews(attackQuarter, Nothing).Count & ") are joined in melee!")
        End If
    End Sub
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
    Private HullPoints As New Dictionary(Of ShipQuarter, Integer)
    Private DamageLog As New List(Of Damage)
    Private Sub Damage(ByVal damage As Damage, ByVal targetQuarter As ShipQuarter, ByVal accuracy As Integer) Implements BattlefieldObject.Damage
        If damage.ShipDamage > 0 Then
            Report.Add(Name & "'s " & targetQuarter.ToString & " suffered " & damage.ShipDamage & " damage.")
            DamageSustained(targetQuarter) += damage.ShipDamage
            DamageLog.Add(damage)
        End If
        If damage.CrewDamage > 0 Then
            For Each Crew In GetCrews(targetQuarter, Nothing)
                Crew.ShipAttack(accuracy, damage.Clone(damage))
            Next
        End If

        If DamageSustained(targetQuarter) >= HullPoints(targetQuarter) Then
            BattleSquare.Battlefield.DeadObjects.Add(Me)
            Report.Add(Name & " has been destroyed!")
        End If
    End Sub
    Public Sub Tick() Implements BattlefieldObject.Tick
        For Each q In Weapons.Keys
            For Each w In Weapons(q)
                w.Tick()
            Next
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
    Public Sub ConsoleReport()
        Const s As Integer = 8
        Dim t As String = Dev.vbSpace(1)

        Console.WriteLine("[" & ID & "] " & Name)
        Dim sus(2) As String
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            sus(0) &= DamageSustained(q).ToString("000") & "/"
            sus(1) &= HullPoints(q).ToString("000") & "/"
            sus(2) &= GetCrews(q, Nothing).Count.ToString("000") & "/"
        Next
        For n = 0 To sus.Length - 1
            sus(n) = sus(n).Remove(sus(n).Length - 1, 1)
        Next

        Console.WriteLine(t & Dev.vbTab("Damage:", s) & sus(0))
        Console.WriteLine(t & Dev.vbTab("Hull:", s) & sus(1))
        Console.WriteLine(t & Dev.vbTab("Crew:", s) & sus(2))
    End Sub
#End Region
End Class
