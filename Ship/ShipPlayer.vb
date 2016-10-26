Public Class ShipPlayer
    Inherits Ship

    Public Sub New()
        MyBase.New()
        For Each fac As WorldFaction In [Enum].GetValues(GetType(WorldFaction))
            Coins.Add(fac, 0)
        Next
    End Sub


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

        'split based on rigging
        'fore-aft only requires fore and aft
        'all other rigs require full
        For Each q As ShipQuarter In [Enum].GetValues(GetType(ShipQuarter))
            Dim sailTotal As Integer = 0
            Dim advancedSailTotal As Integer = 0
            For Each Crew In GetCrews(q, CrewRole.Sailor)
                Dim skill As Integer = Crew.GetSkill(CrewSkill.Sailing)
                sailTotal += Dev.Constrain(skill, 1, 10)
                If skill >= 3 Then advancedSailTotal += Dev.Constrain(skill, 1, 10)
            Next

            If Rigging.Rig = ShipRigging.ShipRig.ForeAft Then
                'fore-aft rigging
                If q = ShipQuarter.Fore Then
                    'fore generates full sail and half sail
                    AddMoveTokenProgress(False, ShipQuarter.Fore, sailTotal, newMoves)
                    AddMoveTokenProgress(False, ShipQuarter.Aft, sailTotal, newMoves)
                    AddMoveTokenProgress(True, ShipQuarter.Fore, advancedSailTotal, newMoves)
                    AddMoveTokenProgress(True, ShipQuarter.Aft, advancedSailTotal, newMoves)
                ElseIf q = ShipQuarter.Aft Then
                    'aft generates port and starboard
                    AddMoveTokenProgress(False, ShipQuarter.Port, sailTotal, newMoves)
                    AddMoveTokenProgress(False, ShipQuarter.Starboard, sailTotal, newMoves)
                    AddMoveTokenProgress(True, ShipQuarter.Port, advancedSailTotal, newMoves)
                    AddMoveTokenProgress(True, ShipQuarter.Starboard, advancedSailTotal, newMoves)
                End If
            Else
                'square and mixed rigging
                AddMoveTokenProgress(False, q, sailTotal, newMoves)
                AddMoveTokenProgress(True, q, advancedSailTotal, newMoves)
            End If
        Next

        'add movetokens
        For Each newMoveToken In newMoves
            MoveTokens.Add(newMoveToken)
            Report.Add(Name & " gained a new sailing token: " & newMoveToken.ToString, ReportType.MoveToken)
        Next
    End Sub
    Private Sub AddMoveTokenProgress(ByVal isAdvanced As Boolean, ByVal q As ShipQuarter, ByVal value As Integer, ByRef newMoves As List(Of MoveToken))
        Dim progress As Dictionary(Of ShipQuarter, Integer)
        Dim threshold As Integer
        If isAdvanced = True Then
            progress = AdvancedMoveTokenProgress
            threshold = AdvancedMoveTokenThreshold
        Else
            progress = MoveTokenProgress
            threshold = MoveTokenThreshold
        End If

        progress(q) += value
        While progress(q) > threshold
            progress(q) -= threshold
            Dim newMoveToken As MoveToken = ConvertQuarterToMoveToken(q, isAdvanced)
            If newMoveToken Is Nothing = False Then newMoves.Add(newMoveToken)
        End While
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

#Region "Crew and Equipment"
    Private Equipment As New List(Of CrewBonus)
    Public Function GetEquipments() As List(Of CrewBonus)
        Return Equipment
    End Function
    Public Sub AddEquipment(ByVal cb As CrewBonus)
        Equipment.Add(cb)
    End Sub

    Private Coins As New Dictionary(Of WorldFaction, Double)
    Public Function CheckAddCoins(ByVal faction As WorldFaction, ByVal value As Double) As Boolean
        If Coins(faction) - value < 0 Then Return False
        Return True
    End Function
    Public Sub AddCoins(ByVal faction As WorldFaction, ByVal value As Double)
        Coins(faction) += value
    End Sub
    Public Function GetCoins(ByVal faction As WorldFaction) As Double
        Return Coins(faction)
    End Function
#End Region

#Region "World Travel"
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
    Public Function GetRoutesFromLocation() As List(Of Route)
        Dim total As New List(Of Route)
        For Each r In Routes
            If r.Contains(Isle) Then total.Add(r)
        Next
        Return total
    End Function
    Private Sub UpgradeRoute(ByVal isle1 As Isle, ByVal isle2 As Isle)
        For n = 0 To Routes.Count - 1
            If Routes(n).Contains(isle1) AndAlso Routes(n).Contains(isle2) Then
                Routes(n) += 1
                Report.Add("The ship's navigator has improved the maps for " & isle1.Name & " - " & isle2.Name & ".", ReportType.TravelProgress)
                Exit Sub
            End If
        Next
    End Sub

    Public Isle As Isle = Nothing
    Private TravelRoute As Route = Nothing
    Private TravelOrigin As Isle = Nothing
    Private TravelDestination As Isle = Nothing
    Private TravelProgress As Double = 0
    Private TravelTarget As Double = 0
    Public ReadOnly Property IsAtSea As Boolean
        Get
            If TravelRoute Is Nothing Then Return False Else Return True
        End Get
    End Property
    Protected ReadOnly Property BaseTravelSpeed() As Double
        Get
            Dim total As Double = 0
            Select Case Type
                Case ShipType.Sloop : total += 50
                Case ShipType.Schooner : total += 70
                Case ShipType.Brig : total += 100
                Case ShipType.Brigantine : total += 120
                Case ShipType.Frigate : total += 135
                Case Else : Throw New Exception("Invalid ship type")
            End Select
            Select Case Rigging.Masts
                Case 1 : total += 0
                Case 2 : total *= 1.5
                Case 3 : total *= 2
                Case Else : Throw New Exception("Masts out of range")
            End Select
            Return total
        End Get
    End Property
    Public Function GetTravelSpeed(Optional ByVal world As World = Nothing) As Double
        Dim total As Double = BaseTravelSpeed
        Dim sailSkillModifier As Double = 10
        Dim totalModifier As Double = 1

        For Each q In [Enum].GetValues(GetType(ShipQuarter))
            Dim sailors As List(Of Crew) = GetCrews(q, CrewRole.Sailor)
            If sailors.Count = 0 Then
                If Rigging.Rig = ShipRigging.ShipRig.ForeAft AndAlso (q = ShipQuarter.Port OrElse q = ShipQuarter.Starboard) Then
                    'do nothing
                    'fore-aft rigged ships only require someone in the fore and aft
                Else
                    'penalty for lacking men
                    If Rigging.Rig = ShipRigging.ShipRig.ForeAft Then totalModifier -= 0.5 Else totalModifier -= 0.25
                End If
            Else
                For Each sailor In sailors
                    total += sailor.GetSkillFromRole * sailSkillModifier
                Next
            End If
        Next
        If totalModifier <= 0 Then Return 0

        Select Case Waterline
            Case ShipWaterline.Unladen : totalModifier += 0.5
            Case ShipWaterline.Light : totalModifier += 0.25
            Case ShipWaterline.Medium : totalModifier += 0
            Case ShipWaterline.Heavy : totalModifier -= 0.25
            Case ShipWaterline.Overladen : totalModifier -= 0.5
        End Select

        If world Is Nothing = False Then
            If GetTravelRouteDirection(TravelOrigin, TravelDestination) = world.Wind Then totalModifier *= 1.1
        End If

        Return total * totalModifier
    End Function
    Public Function CheckSetTravelRoute(ByVal route As Route) As Boolean
        If Routes.Contains(route) = False Then Return False
        If Isle Is Nothing Then Return False
        If route.Contains(Isle) = False Then Return False
        Return True
    End Function
    Public Sub SetTravelRoute(ByVal route As Route)
        If CheckSetTravelRoute(route) = False Then Exit Sub
        TravelRoute = route
        TravelOrigin = Isle
        TravelDestination = route - TravelOrigin
        Isle = Nothing
        TravelProgress = 0
        TravelTarget = route.GetDistance
    End Sub
    Private Function GetTravelRouteDirection(ByVal origin As Isle, ByVal destination As Isle) As BattleDirection
        Dim xDif As Integer = origin.X - destination.X
        Dim yDif As Integer = origin.Y - destination.Y
        If Math.Abs(xDif) > Math.Abs(yDif) Then
            'check east-west
            If xDif < 0 Then Return BattleDirection.East Else Return BattleDirection.West
        Else
            'check north-south
            If yDif < 0 Then Return BattleDirection.South Else Return BattleDirection.North
        End If
    End Function
    Public Sub Teleport(ByVal target As Isle)
        Isle = target
        TravelRoute = Nothing
        TravelOrigin = Nothing
        TravelDestination = Nothing
        TravelProgress = 0
        TravelTarget = 0
    End Sub
#End Region

#Region "World"
    Public Sub Tick(ByVal world As World)
        'crew tick
        Dim doctor As Crew = GetBestCrew(Nothing, CrewRole.Doctor)
        Dim CrewList As List(Of Crew) = GetCrews(Nothing, Nothing)
        For Each Crew In CrewList
            Crew.Tick(doctor)
        Next
        If doctor Is Nothing = False AndAlso CrewList.Count > 0 Then
            Dim xp As Double = CrewList.Count / 2
            doctor.AddSkillXP(CrewSkill.Medicine, xp)
        End If


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

        TickTravel(world)
    End Sub
    Private Sub TickTravel(ByVal world As World)
        If MyBase.IsSeaworthy = False Then Exit Sub
        If TravelDestination Is Nothing Then Exit Sub

        Dim speed As Double = GetTravelSpeed(world)
        TravelProgress += speed
        If TravelProgress >= TravelTarget Then
            Dim navigator As Crew = GetCrew(Nothing, CrewRole.Navigator)
            If navigator.GetSkillFromRole > TravelRoute Then UpgradeRoute(TravelOrigin, TravelDestination)

            Teleport(TravelDestination)
            Report.Add(Name & " has arrived at " & Isle.Name & ".", ReportType.TravelMain)
        Else
            If GetTravelRouteDirection(TravelOrigin, TravelDestination) = world.Wind Then Report.Add(Name & " is sailing with the wind (+10% speed).", ReportType.TravelProgress)
            Report.Add(Name & " makes some progress towards " & TravelDestination.Name & " (+" & speed.ToString("0.0") & ").", ReportType.TravelProgress)
        End If
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
    Public Sub ConsoleReportTravelStatus()
        If IsAtSea = True Then
            Console.WriteLine("Travelling to " & TravelDestination.Name & "... (" & TravelProgress.ToString("0") & "/" & TravelTarget.ToString("0") & ")")
        Else
            Console.WriteLine("Docked at " & Isle.Name & ".")
        End If
    End Sub
#End Region
End Class
