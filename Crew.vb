Public Class Crew
    Private Name As String
    Private Race As CrewRace

    Public Sub New()
        For Each cs In [Enum].GetValues(GetType(CrewSkill))
            Skills.Add(cs, 0)
            SkillsXP.Add(cs, 0)
        Next
    End Sub

#Region "Skills"
    Private Skills As New Dictionary(Of CrewSkill, Integer)
    Private SkillsXP As New Dictionary(Of CrewSkill, Integer)
    Private Shared SkillThresholds As Integer() = {0, 100, 300, 600, 1000, 1500}
    Public Function GetSkill(ByVal cs As CrewSkill)
        Return Skills(cs)
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


    Public Enum CrewRace
        Human
        Seatouched
        Ghost
    End Enum
End Class
