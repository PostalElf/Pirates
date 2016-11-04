Public Class ShipModule
    Public Name As String
    Public Type As ModuleType
    Public Quality As ModuleQuality
    Public Capacity As Integer
    Public ReadOnly Property CapacityFree As Integer
        Get
            Return Capacity - Crews.Count
        End Get
    End Property
    Public HullCost As Integer
    Public Weight As Integer
    Public IsExclusive As Boolean = False

    Public Ship As Ship
    Public Quarter As ShipQuarter
    Private Crews As New List(Of Crew)
    Public Function CheckAddCrew(ByVal crew As Crew) As Boolean
        If Race <> Nothing Then
            If crew.Race <> Race Then Return False
        End If
        If Crews.Count + 1 > Capacity Then Return False

        Return True
    End Function
    Public Sub AddCrew(ByRef Crew As Crew)
        Crews.Add(Crew)
        Select Case Type
            Case ModuleType.Quarters : Crew.Quarters = Me
            Case ModuleType.Shrine : Crew.Shrine = Me
        End Select
    End Sub
    Public Sub RemoveCrew(ByRef crew As Crew)
        If Crews.Contains(crew) = False Then Exit Sub

        Crews.Remove(crew)
        Select Case Type
            Case ModuleType.Quarters
                crew.Quarters = Nothing
            Case ModuleType.Shrine
                crew.Shrine = Nothing
        End Select
    End Sub
    Public Race As CrewRace

    Private Sub New()
    End Sub
    Public Overrides Function ToString() As String
        Return Name
    End Function
    Public Shared Function Generate(ByVal type As ModuleType, ByVal quality As ModuleQuality, ByVal race As CrewRace) As ShipModule
        quality = Dev.Constrain(quality, 0, 5)
        Dim total As New ShipModule
        With total
            .Name = quality.ToString & " " & type.ToString
            .Type = type
            .Quality = quality
            .Race = race

            Select Case type
                Case ModuleType.Quarters
                    .Name = quality.ToString & " " & .Race.ToString & " Quarters"
                    .Capacity = 5
                    .HullCost = 2
                    .Weight = 5

                Case ModuleType.Quarterdeck
                    .Race = Nothing
                    .HullCost = quality
                    .Weight = 0
                    .IsExclusive = True

                Case ModuleType.Helm
                    .HullCost = quality
                    .Weight = 0
                    .IsExclusive = True

                Case ModuleType.Maproom
                    .Name = quality.ToString & " Map Room"
                    .HullCost = quality
                    .Weight = quality
                    .IsExclusive = True

                Case ModuleType.Kitchen
                    .HullCost = quality
                    .Weight = quality
                    .IsExclusive = True

                Case ModuleType.Laboratory
                    .HullCost = quality * 2
                    .Weight = quality

                Case ModuleType.Shrine
                    Dim c As Integer = 0
                    Select Case quality
                        Case 0, 1
                            .Capacity = 3
                            .HullCost = 5
                            .Weight = 5
                        Case 2, 3
                            .Capacity = 3
                            .HullCost = 5
                            .Weight = 7
                        Case 4
                            .Capacity = 5
                            .HullCost = 5
                            .Weight = 10
                        Case 5
                            .Capacity = 5
                            .HullCost = 7
                            .Weight = 12
                    End Select

                Case ModuleType.Apothecary
                    .HullCost = quality * 2
                    .Weight = quality
                    .IsExclusive = True

                Case ModuleType.Hold
                    .Capacity = quality * 100
                    .HullCost = 5
                    .Weight = 0
                    .Race = Nothing

                Case Else : Throw New Exception
            End Select
        End With
        Return total
    End Function

    Public Sub ConsoleReport()
        Dim s As String = Dev.vbSpace(1)
        Dim t As Integer = 12
        Console.WriteLine(Name)
        Console.WriteLine(s & Dev.vbTab("Quarter:", t) & Quarter.ToString)
        If Type = ModuleType.Shrine OrElse Type = ModuleType.Quarters Then Console.WriteLine(s & Dev.vbTab("Capacity:", t) & Crews.Count & "/" & Capacity)
        If Type = ModuleType.Hold Then Console.WriteLine(s & Dev.vbTab("Capacity:", t) & Capacity)
        Console.WriteLine(s & Dev.vbTab("Hullspace:", t) & HullCost)
        If Weight > 0 Then Console.WriteLine(s & Dev.vbTab("Weight:", t) & Weight)
    End Sub

    Public Enum ModuleType
        Quarters = 1
        Quarterdeck
        Helm
        Maproom
        Kitchen
        Laboratory
        Shrine
        Apothecary
        Hold
    End Enum
    Public Enum ModuleQuality
        Shoddy = 0
        Poor
        Average
        Nice
        Excellent
        Luxurious
    End Enum
End Class