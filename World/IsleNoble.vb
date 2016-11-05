Public Class IsleNoble
    Public Name As String
    Public Sex As Gender
    Public Title As Rank
    Public ReadOnly Property TitleString As String
        Get
            If Sex = Gender.Male Then Return Title.ToString
            Select Case Title
                Case Rank.Baron : Return "Baroness"
                Case Rank.Viscount : Return "Viscountess"
                Case Rank.Earl : Return "Countess"
                Case Rank.Marquis : Return "Marquise"
                Case Rank.Duke : Return "Duchess"
                Case Else : Throw New Exception("Title out of range.")
            End Select
        End Get
    End Property
    Public Race As CrewRace
    Public Isle As Isle
    Public SupportedIsleFaction As IsleFaction
    Public SupportedWorldFaction As WorldFaction

    Public Overrides Function ToString() As String
        Return TitleString & " " & Name
    End Function

    Public Shared Function Generate(ByVal aTitle As Rank, ByVal race As CrewRace, ByRef rng As Random, Optional ByVal worldFaction As WorldFaction = Nothing) As IsleNoble
        Dim noble As New IsleNoble
        With noble
            .Sex = rng.Next(0, 2)
            .Race = race
            .Name = GenerateName(rng, .Sex)
            .Title = aTitle
            If IsleFactions.Count = 0 Then IsleFactions = New List(Of IsleFaction)([Enum].GetValues(GetType(IsleFaction)))
            .SupportedIsleFaction = Dev.GrabRandom(Of IsleFaction)(IsleFactions, rng)
            If worldFaction = Nothing Then
                If WorldFactions.Count = 0 Then WorldFactions = New List(Of WorldFaction)([Enum].GetValues(GetType(WorldFaction)))
                worldFaction = Dev.GrabRandom(Of WorldFaction)(WorldFactions, World.Rng)
            End If
            .SupportedWorldFaction = worldFaction
        End With
        Return noble
    End Function
    Private Shared WorldFactions As New List(Of WorldFaction)
    Private Shared IsleFactions As New List(Of IsleFaction)
    Private Shared MaleNames As New List(Of String)
    Private Shared FemaleNames As New List(Of String)
    Public Shared Function GenerateName(ByRef rng As Random, ByVal g As Gender) As String
        Dim namelist As List(Of String)
        If g = Gender.Male Then namelist = MaleNames Else namelist = FemaleNames
        If namelist.Count = 0 Then
            If g = Gender.Male Then
                MaleNames = IO.SimpleFilegetAll("nobleMale.txt")
                namelist = MaleNames
            Else
                FemaleNames = IO.SimpleFilegetAll("nobleFemale.txt")
                namelist = FemaleNames
            End If
        End If
        Return Dev.GrabRandom(Of String)(namelist, rng)
    End Function

    Public Enum Rank
        Baron = 1
        Viscount
        Earl
        Marquis
        Duke
    End Enum
    Public Enum Gender
        Male
        Female
    End Enum
End Class
