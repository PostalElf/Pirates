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
            Case ModuleType.Crew
                Crew.Quarters = Me
            Case ModuleType.Shrine
                Crew.Shrine = Me
        End Select
    End Sub
    Public Sub RemoveCrew(ByRef crew As Crew)
        If Crews.Contains(crew) = False Then Exit Sub

        Crews.Remove(crew)
        Select Case Type
            Case ModuleType.Crew
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
                Case ModuleType.Crew
                    .Name = quality.ToString & " Crew Quarters"
                    .Capacity = 5
                    .HullCost = 2
                    .Weight = 5

                Case ModuleType.Quarterdeck
                    .Capacity = 1
                    .Race = Nothing
                    .HullCost = quality
                    .Weight = 0
                    .IsExclusive = True

                Case ModuleType.Helm
                    .Capacity = 1
                    .HullCost = quality
                    .Weight = 0
                    .IsExclusive = True

                Case ModuleType.Maproom
                    .Name = quality.ToString & " Map Room"
                    .Capacity = 1
                    .HullCost = quality
                    .Weight = quality
                    .IsExclusive = True

                Case ModuleType.Kitchen
                    .Capacity = 1
                    .HullCost = quality
                    .Weight = quality
                    .IsExclusive = True

                Case ModuleType.Laboratory
                    .Capacity = 3
                    .HullCost = quality * 2
                    .Weight = quality

                Case ModuleType.Shrine
                    .Capacity = quality * 3
                    .HullCost = 5
                    .Weight = 5

                Case ModuleType.Hold
                    .Capacity = quality * 100
                    .HullCost = 5
                    .Weight = 0

                Case Else : Throw New Exception
            End Select
        End With
        Return total
    End Function

    Public Enum ModuleType
        Crew = 1
        Quarterdeck
        Helm
        Maproom
        Kitchen
        Laboratory
        Shrine
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