Public Class ShipPlayer
    Inherits Ship

#Region "Specials"
    Protected IgnoresMoveTokens As Boolean = False
    Public Overloads Sub Cheaterbug(ByVal turn As Boolean, ByVal waterline As Boolean, ByVal damage As Boolean, ByVal weapon As Boolean)
        MyBase.Cheaterbug(turn, waterline, Damage, weapon)
        IgnoresMoveTokens = True
    End Sub
#End Region

#Region "Move Tokens"
    Private MoveTokens As New List(Of MoveToken)
    Private MoveTokenProgress As New Dictionary(Of ShipQuarter, Integer)
    Private AdvancedMoveTokenProgress As New Dictionary(Of ShipQuarter, Integer)
    Private ReadOnly Property MoveTokenThreshold As Integer
        Get
            Dim total As Integer = 5
            If IgnoresWaterline = False Then
                If Waterline = ShipWaterline.Medium Then total += 1
                If Waterline = ShipWaterline.Heavy Then total += 2
            End If
            Return total
        End Get
    End Property
    Private ReadOnly Property AdvancedMoveTokenThreshold As Integer
        Get
            Dim total As Integer = 10
            If IgnoresWaterline = False Then
                If Waterline = ShipWaterline.Medium Then total += 1
                If Waterline = ShipWaterline.Heavy Then total += 2
            End If
            Return total
        End Get
    End Property

    Public Function CheckSpendMoveToken(ByVal moveToken As MoveToken) As Boolean
        If IgnoresMoveTokens = True Then Return True
        If MovesIndexOf(MoveTokens, moveToken) = -1 Then Return False
        Return True
    End Function
    Public Sub SpendMoveToken(ByVal moveToken As MoveToken)
        If CheckSpendMoveToken(moveToken) = False Then Exit Sub
        If IgnoresMoveTokens = False Then MoveTokens.RemoveAt(MovesIndexOf(MoveTokens, moveToken))
        Move(moveToken)
    End Sub
    Private Function MovesIndexOf(ByVal m1 As List(Of MoveToken), ByVal m2 As MoveToken) As Integer
        For n = 0 To m1.Count - 1
            Dim mm = m1(n)
            If mm = m2 Then Return n
        Next
        Return -1
    End Function
    Private Sub GainMoveTokens()
        Dim bf As Battlefield = BattleSquare.Battlefield
        Dim newMoves As New List(Of MoveToken)

        'generate wind progress
        If Facing = bf.Wind Then
            MoveTokenProgress(ShipQuarter.Aft) += Battlefield.WindMoveTokenProgress
            Report.Add(Name & " is facing the wind!", ReportType.WindMoveToken)
        End If

        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            Dim sailTotal As Integer = 0
            Dim advancedSailTotal As Integer = 0
            For Each Crew In GetCrews(q, CrewRole.Sailor)
                Dim skill As Integer = Crew.GetSkill(CrewSkill.Sailing)
                sailTotal += Dev.Constrain(skill, 1, 10)
                If skill >= 3 Then advancedSailTotal += Dev.Constrain(skill, 1, 10)
            Next

            MoveTokenProgress(q) += sailTotal
            While MoveTokenProgress(q) > MoveTokenThreshold
                MoveTokenProgress(q) -= MoveTokenThreshold
                Dim newMoveToken As MoveToken = ConvertQuarterToMoveToken(q, False)
                If newMoveToken Is Nothing = False Then newMoves.Add(newMoveToken)
            End While

            AdvancedMoveTokenProgress(q) += advancedSailTotal
            While AdvancedMoveTokenProgress(q) > AdvancedMoveTokenThreshold
                AdvancedMoveTokenProgress(q) -= AdvancedMoveTokenThreshold
                Dim newMoveToken As MoveToken = ConvertQuarterToMoveToken(q, True)
                If newMoveToken Is Nothing = False Then newMoves.Add(newMoveToken)
            End While
        Next

        For Each newMoveToken In newMoves
            MoveTokens.Add(newMoveToken)
            Report.Add(Name & " gained a new sailing token: " & newMoveToken.ToString, ReportType.MoveToken)
        Next
    End Sub
    Private Function ConvertQuarterToMoveToken(ByVal q As ShipQuarter, ByVal isAdvanced As Boolean) As MoveToken
        Dim newMoveToken As MoveToken = Nothing
        Select Case q
            Case ShipQuarter.Fore
                If isAdvanced = False Then
                    newMoveToken = New MoveToken({BattleMove.Forward, BattleMove.Forward})
                Else
                    newMoveToken = New MoveToken({BattleMove.Forward, BattleMove.Forward})
                End If

            Case ShipQuarter.Starboard
                If isAdvanced = False Then
                    newMoveToken = New MoveToken({BattleMove.Forward, BattleMove.TurnRight})
                Else
                    newMoveToken = New MoveToken({BattleMove.TurnRight})
                End If

            Case ShipQuarter.Aft
                If isAdvanced = False Then
                    newMoveToken = New MoveToken({BattleMove.Forward})
                Else
                    newMoveToken = New MoveToken({BattleMove.Backwards})
                End If

            Case ShipQuarter.Port
                If isAdvanced = False Then
                    newMoveToken = New MoveToken({BattleMove.Forward, BattleMove.TurnLeft})
                Else
                    newMoveToken = New MoveToken({BattleMove.TurnLeft})
                End If
            Case Else : Throw New Exception("Unrecognised ship quarter")
        End Select
        Return newMoveToken
    End Function
    Private Function ConvertMoveTokenToQuarter(ByVal mt As MoveToken) As ShipQuarter
        Select Case mt.ToString
            Case "Full Sails" : Return ShipQuarter.Fore
            Case "Starboard", "Hard to Starboard" : Return ShipQuarter.Starboard
            Case "Port", "Hard to Port" : Return ShipQuarter.Port
            Case "Half Sails", "Tack Aft" : Return ShipQuarter.Aft
            Case Else : Throw New Exception("Unrecognised movetoken")
        End Select
    End Function
    Private Function CountMoveTokens(ByVal mt As MoveToken) As Integer
        Dim total As Integer = 0
        For Each m In MoveTokens
            If m = mt Then total += 1
        Next
        Return total
    End Function
#End Region

#Region "Combat"
    Public Overloads Sub EnterCombat()
        MyBase.EnterCombat()

        'add movetokens
        MoveTokens.Clear()
        For n = 1 To 2
            MoveTokens.Add(New MoveToken({BattleMove.Forward}))
            MoveTokens.Add(New MoveToken({BattleMove.Forward, BattleMove.TurnLeft}))
            MoveTokens.Add(New MoveToken({BattleMove.Forward, BattleMove.TurnRight}))
        Next
        MoveTokens.Add(New MoveToken({BattleMove.Forward, BattleMove.Forward}))

        'reset movetokenprogress
        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            MoveTokenProgress.Add(q, 0)
            AdvancedMoveTokenProgress.Add(q, 0)
        Next

        'battlestations
        For Each Crew In GetCrews(Nothing, Nothing)
            MoveCrewToStation(Crew)
        Next
    End Sub
    Public Overloads Sub CombatTick()
        MyBase.TickCombat()

        GainMoveTokens()
        RunCommands()
    End Sub
    Public Overloads Sub EndCombat()
        MyBase.EndCombat()

        'clear movetokenprogress
        MoveTokenProgress.Clear()
        AdvancedMoveTokenProgress.Clear()

        'stations
        For Each Crew In GetCrews(Nothing, Nothing)
            MoveCrewToStation(Crew)
        Next
    End Sub
#End Region

#Region "Commands"
    Private Commands As New List(Of Command)
    Public Class Command
        Public Type As String
        Public Target As Object
        Public Destination As Object
        Public Secondary As Object
        Public Sub New(ByVal aType As String, ByVal aTarget As Object, ByVal aDestination As Object, Optional ByVal aSecondary As Object = Nothing)
            Type = aType
            Target = aTarget
            Destination = aDestination
            Secondary = aSecondary
        End Sub
    End Class
    Public Sub AddCommand(ByVal type As String, ByVal target As Object, ByVal destination As Object, Optional ByVal secondary As Object = Nothing)
        Commands.Add(New Command(type, target, destination, secondary))
    End Sub
    Private Sub RunCommands()
        While Commands.Count > 0
            Dim command As Command = Commands(0)
            With command
                Select Case .Type
                    Case "Move"
                        Dim crew As Crew = CType(.Target, Crew)
                        Dim quarter As ShipQuarter = CType(.Destination, ShipQuarter)
                        Dim role As CrewRole = CType(.Secondary, CrewRole)
                        MoveCrew(crew, quarter, role)
                End Select
            End With
            Commands.RemoveAt(0)
        End While
    End Sub
#End Region

#Region "World"
    Private Routes As New List(Of Route)
    Public Function CheckAddRoute(ByVal route As Route)
        If Routes.Contains(route) Then Return False
        Return True
    End Function
    Public Sub AddRoute(ByVal Route As Route)
        Routes.Add(Route)
    End Sub
    Public Function CheckRemoveRoute(ByVal route As Route)
        If Routes.Contains(route) = False Then Return False
        Return True
    End Function
    Public Sub RemoveRoute(ByVal route As Route)
        Routes.Remove(route)
    End Sub
    Public Function GetRoute(ByVal isle1 As Isle, ByVal isle2 As Isle) As Route
        Dim r As New Route(isle1, isle2, 0)
        For Each Route In Routes
            If Route = r Then Return Route
        Next
        Return Nothing
    End Function

    Private Isle As Isle = Nothing
    Private TravelOrigin As Isle = Nothing
    Private TravelDestination As Isle = Nothing
    Private TravelProgress As Double = 0
    Private ReadOnly Property TravelSpeed As Double
        Get

        End Get
    End Property
    Public Function CheckTravelRoute(ByVal route As Route) As Boolean
        If Routes.Contains(route) = False Then Return False
        If Isle Is Nothing = False Then Return False
        If route.Contains(Isle) = False Then Return False
        If TravelProgress >= 0 Then Return False
        Return True
    End Function
    Public Sub TravelRoute(ByVal route As Route)
        If CheckTravelRoute(route) = False Then Exit Sub
        TravelOrigin = Isle
        TravelDestination = route - TravelOrigin
        Isle = Nothing
        TravelProgress = 0
    End Sub

    Public Sub Tick()
        'crew tick
        Dim doctor As Crew = GetBestCrew(Nothing, CrewRole.Doctor)
        For Each Crew In GetCrews(Nothing, Nothing)
            Crew.Tick(doctor)
        Next

        'report good consumption
        If GoodsConsumed.Values.Count > 0 Then
            Dim rep As String = "The crew consumed "
            For n = 0 To GoodsConsumed.Values.Count - 1
                Dim g As Good = GoodsConsumed.Values(n)
                If n = GoodsConsumed.Values.Count - 1 Then rep &= "and "
                If g.Qty < 0 Then
                    rep &= Math.Abs(g.Qty) & " " & g.Type.ToString & ", "
                End If
                If n = GoodsConsumed.Values.Count - 1 Then
                    rep = rep.Remove(rep.Count - 2, 2)
                    rep &= "."
                End If
            Next
            Report.Add(rep, ReportType.CrewConsumption)
            GoodsConsumed.Clear()
        End If

        'report morale change
        If MoraleChange <> 0 Then
            Dim rep As String = "The crew's morale"
            If MoraleChange > 0 Then rep &= " improves by " & MoraleChange
            If MoraleChange < 0 Then rep &= " worses by " & Math.Abs(MoraleChange)
            rep &= " in total "
            rep &= "(avg " & MoraleChange / GetCrews(Nothing, Nothing).Count & ")."
            Report.Add(rep, ReportType.CrewMorale)
            MoraleChange = 0
        End If

        TickTravel()
    End Sub
    Private Sub TickTravel()

    End Sub
    Public GoodsConsumed As New Dictionary(Of GoodType, Good)
    Public MoraleChange As Integer
#End Region

#Region "Console"
    Public Overrides Sub ConsoleReport()
        Dim s As String = Dev.vbSpace(2)
        Dim t As Integer = 20

        MyBase.ConsoleReport()
        If InCombat = True Then
            Console.WriteLine(Dev.vbSpace(1) & "Movement")
            For n = 1 To 2
                Dim isAdvanced As Boolean : If n = 1 Then isAdvanced = False Else isAdvanced = True
                For Each q In MoveTokenProgress.Keys
                    Dim mt As MoveToken = ConvertQuarterToMoveToken(q, isAdvanced)

                    Console.Write(s & Dev.vbTab(mt.ToString & ":", t))
                    Console.Write("[")
                    If isAdvanced = False Then
                        For p = 1 To MoveTokenThreshold
                            If p <= MoveTokenProgress(q) Then Console.Write("*") Else Console.Write("-")
                        Next
                    Else
                        For p = 1 To AdvancedMoveTokenThreshold
                            If p <= AdvancedMoveTokenProgress(q) Then Console.Write("*") Else Console.Write("-")
                        Next
                    End If
                    Console.Write("]  ")
                    Console.Write("x" & CountMoveTokens(mt))
                    Console.WriteLine()
                Next
            Next
        End If
    End Sub
    Public Sub ConsoleReportGoods()
        Dim s As Integer = 12

        For Each gt In [Enum].GetValues(GetType(GoodType))
            Dim g As Good = GetGood(gt)
            Console.Write(Dev.vbTab(gt.ToString & ":", s))
            Console.Write(Dev.vbTab(g.Qty, 5))
            Console.Write("(" & g.TotalWeight.ToString("0.0") & "t.)")
            Console.WriteLine()
        Next
        Console.WriteLine()
        Console.WriteLine(Dev.vbTab("Hullspace:", s) & HullSpaceUsed & "/" & HullSpaceMax)
        Console.Write(Dev.vbTab("Tonnage:", s) & Tonnage.ToString("0.0") & "/" & TonnageMax)
        Dim ratio As Double = Tonnage / TonnageMax * 100
        Console.WriteLine(" (" & ratio.ToString("0.0") & "% - " & Waterline.ToString & ")")
    End Sub
#End Region
End Class
