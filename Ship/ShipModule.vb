﻿Public Class ShipModule
    Public Name As String
    Public Type As ModuleType
    Public Quality As ModuleQuality
    Public Capacity As Integer
    Public HullCost As Integer
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
    Public Sub AddCrew(ByVal Crew As Crew)
        Crews.Add(Crew)
        Select Case Type
            Case ModuleType.Crew
                Crew.Quarters = Me
            Case ModuleType.Shrine
                Crew.Shrine = Me
        End Select
    End Sub
    Public Race As CrewRace

    Private Sub New()
    End Sub
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

                Case ModuleType.Quarterdeck
                    .Capacity = 1
                    .Race = Nothing
                    .HullCost = quality
                    .IsExclusive = True

                Case ModuleType.Helm
                    .Capacity = 1
                    .HullCost = quality
                    .IsExclusive = True

                Case ModuleType.Maproom
                    .Name = quality.ToString & " Map Room"
                    .Capacity = 1
                    .HullCost = quality
                    .IsExclusive = True

                Case ModuleType.Kitchen
                    .Capacity = 1
                    .HullCost = quality
                    .IsExclusive = True

                Case ModuleType.Laboratory
                    .Capacity = 3
                    .HullCost = quality * 2

                Case ModuleType.Shrine
                    .Capacity = quality * 3
                    .HullCost = 5

                Case ModuleType.Hold
                    .Capacity = quality * 10
                    .HullCost = 5

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