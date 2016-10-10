Public Class Crew
    Public Sub New()
        For Each cs In [Enum].GetValues(GetType(CrewSkill))
            Skills.Add(cs, 0)
            SkillsXP.Add(cs, 0)
        Next
    End Sub
    Public Overrides Function ToString() As String
        Return Name
    End Function

    Private Name As String
    Private Race As CrewRace
    Public Enum CrewRace
        Human
        Seatouched
        Ghost
    End Enum

#Region "Bonuses"
    Private Mutations As New List(Of CrewBonus)
    Private Equipment As New List(Of CrewBonus)
    Private Sub AddBonus(ByVal listName As String, ByVal effect As CrewBonus)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
        If GetSlot(targetList, effect.Slot) Is Nothing = False Then Exit Sub
        targetList.Add(effect)
    End Sub
    Private Sub RemoveBonus(ByVal listName As String, ByVal effect As CrewBonus)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
        If targetList.Contains(effect) Then targetList.Remove(effect)
    End Sub
    Private Sub RemoveBonus(ByVal listName As String, ByVal slot As String)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
        Dim target As CrewBonus = GetSlot(targetList, slot)
        If target Is Nothing = False Then targetList.Remove(target)
    End Sub
    Private Function GetBonusList(ByVal listName As String) As List(Of CrewBonus)
        Select Case listName.ToLower
            Case "mutations", "mutation" : Return Mutations
            Case "equipment" : Return Equipment
        End Select
        Return Nothing
    End Function
    Private Function GetSlot(ByRef targetList As List(Of CrewBonus), ByVal slot As String) As CrewBonus
        For Each e In targetList
            If e.Slot = slot Then Return e
        Next
        Return Nothing
    End Function

    Public Class CrewBonus
        Public Name As String
        Public Slot As String
        Public SkillBonuses As New Dictionary(Of CrewSkill, Integer)
    End Class
#End Region

#Region "Skills"
    Private Skills As New Dictionary(Of CrewSkill, Integer)
    Private SkillsXP As New Dictionary(Of CrewSkill, Integer)
    Private Shared SkillThresholds As Integer() = {0, 100, 300, 600, 1000, 1500}
    Public Function GetSkill(ByVal cs As CrewSkill)
        Dim total As Integer = Skills(cs)
        For Each mutation In Mutations
            If mutation.SkillBonuses.ContainsKey(cs) Then total += mutation.SkillBonuses(cs)
        Next
        For Each e In Equipment
            If e.SkillBonuses.ContainsKey(cs) Then total += e.SkillBonuses(cs)
        Next
        Return total
    End Function
    Public Sub AddSkillXP(ByVal cs As CrewSkill, ByVal value As Integer)
        Dim maxLevel As Integer = SkillThresholds.Count - 1
        Dim maxThreshold As Integer = SkillThresholds(maxLevel)
        If Skills(cs) >= maxLevel Then Exit Sub

        SkillsXP(cs) += value
        If SkillsXP(cs) > maxThreshold Then SkillsXP(cs) = maxThreshold

        Dim level As Integer = Skills(cs)
        For n = maxLevel To level Step -1
            Dim threshold As Integer = SkillThresholds(n)
            If SkillsXP(cs) < threshold Then
                Continue For
            Else
                Skills(cs) = n
                Exit For
            End If
        Next
    End Sub

    Public Enum CrewSkill
        Leadership
        Sailing
        Navigation
        Gunnery
        Firearms
        Melee

    End Enum
#End Region

#Region "Movement"
    Private Ship As Ship
    Private ShipQuarter As ShipQuarter

    Public Sub Move(ByVal quarter As ShipQuarter)
        Ship.Crews(ShipQuarter).Remove(Me)
        Ship.Crews(quarter).Add(Me)
        ShipQuarter = quarter
    End Sub
#End Region
End Class
