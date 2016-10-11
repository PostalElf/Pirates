﻿Public MustInherit Class Ship
    Implements BattlefieldObject
    Public Name As String

    Public Sub New()
        For Each quarter In [Enum].GetValues(GetType(ShipQuarter))
            Weapons.Add(quarter, New List(Of ShipWeapon))
            DamageSustained.Add(quarter, 0)
            HullPoints.Add(quarter, 10)
            Crews.Add(quarter, New List(Of Crew))
        Next

        'note: ties in heuristic distance are broken by how high the move is up in the list
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.Forward})
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.TurnLeft})
        _AvailableMoves.Add({BattleMove.Forward, BattleMove.TurnRight})
        _AvailableMoves.Add({BattleMove.Forward})
    End Sub

#Region "Specials"
    Protected IgnoresJustTurned As Boolean = False

    Public Sub Cheaterbug()
        IgnoresJustTurned = True
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
                If targetSquare.Contents Is Nothing Then SetSquare(targetSquare) Else targetSquare.Contents.MovedInto(Me)
            End If
        Next

        JustTurned = turn
    End Sub
    Public Sub SetSquare(ByVal targetSquare As Battlesquare)
        If BattleSquare Is Nothing = False Then BattleSquare.Contents = Nothing
        BattleSquare = targetSquare
        BattleSquare.Contents = Me
    End Sub
    Public Sub MovedInto(ByRef bo As BattlefieldObject) Implements BattlefieldObject.MovedInto
        'TODO
    End Sub
#End Region

#Region "Crew"
    Public Crews As New Dictionary(Of ShipQuarter, List(Of Crew))
#End Region

#Region "Attack"
    Protected Weapons As New Dictionary(Of ShipQuarter, List(Of ShipWeapon))
    Public Sub AddWeapon(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon)
        Weapons(quarter).Add(weapon)
    End Sub
    Public Function GetWeapons(ByVal quarter As ShipQuarter) As List(Of ShipWeapon)
        Return Weapons(quarter)
    End Function
    Public Function Attack(ByVal quarter As ShipQuarter, ByVal weapon As ShipWeapon) As Battlesquare
        If Weapons(quarter).Contains(weapon) = False Then Return Nothing
        If weapon.IsReady = False Then Return Nothing

        Dim range As Integer = weapon.Range
        Dim attackSquare As Battlesquare = BattleSquare.GetSubjectiveAdjacent(Facing, quarter, range)
        If attackSquare.Contents Is Nothing Then Return Nothing Else Attack = attackSquare
        Dim attackTarget As BattlefieldObject = attackSquare.Contents

        Dim damage As ShipDamage = weapon.ShipDamage
        Dim attackDirection As BattleDirection = BattleSquare.ReverseDirection(BattleSquare.GetSubjectiveDirection(Facing, quarter))
        Dim targetQuarter As ShipQuarter = attackTarget.GetTargetQuarter(attackDirection)
        attackTarget.Damage(damage, targetQuarter)
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

    Private DamageSustained As New Dictionary(Of ShipQuarter, Integer)
    Private HullPoints As New Dictionary(Of ShipQuarter, Integer)
    Private DamageLog As New List(Of ShipDamage)
    Private Sub Damage(ByVal damage As ShipDamage, ByVal targetQuarter As ShipQuarter) Implements BattlefieldObject.Damage
        DamageSustained(targetQuarter) += damage.Amt
        DamageLog.Add(damage)
        Report.Add(Name & "'s " & targetQuarter.ToString & " suffered " & damage.Amt & " damage.")

        If DamageSustained(targetQuarter) >= HullPoints(targetQuarter) Then
            BattleSquare.Battlefield.DeadObjects.Add(Me)
            Report.Add(Name & " has been destroyed!")
        End If
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
#End Region
End Class
