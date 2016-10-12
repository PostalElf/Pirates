Public Class Crew
    Public Sub New()
        For Each cs In [Enum].GetValues(GetType(CrewSkill))
            Skills.Add(cs, 0)
            SkillsXP.Add(cs, 0)
        Next
    End Sub
    Public Shared Function Generate(ByVal race As CrewRace, Optional ByRef rng As Random = Nothing) As Crew
        If rng Is Nothing Then rng = New Random

        Dim crew As New Crew
        With crew
            .Name = GenerateName(race)
            .Race = race
            .Health = 10

            Dim cutlass As New CrewBonus
            With cutlass
                .Name = "Belaying Pin"
                .Damage = 1
                .DamageType = DamageType.Blunt
                .AmmoUse = 0
                .Slot = "Weapon"
            End With
            .AddBonus("equipment", cutlass)

            Dim skills As New List(Of CrewSkill)([Enum].GetValues(GetType(CrewSkill)))
            For n = 1 To 2
                Dim cs As CrewSkill = Dev.GetRandom(Of CrewSkill)(skills)
                .AddSkillXP(cs, SkillThresholds(n))
            Next
        End With
        Return crew
    End Function
    Private Shared Function GenerateName(ByVal race As CrewRace, Optional ByRef rng As Random = Nothing) As String
        If rng Is Nothing Then rng = New Random
        If NamePrefixes.Count = 0 Then NamePrefixes = IO.SimpleFilegetAll("namePrefixes.txt")
        If NameSuffixes.Count = 0 Then NameSuffixes = IO.SimpleFilegetAll("nameSuffixes.txt")

        Dim prefix As String = GetNamePart(NamePrefixes, rng)
        Dim suffix As String = GetNamePart(NameSuffixes, rng)
        Return prefix & " " & suffix
    End Function
    Private Shared NamePrefixes As New List(Of String)
    Private Shared NameSuffixes As New List(Of String)
    Private Shared Function GetNamePart(ByRef targetList As List(Of String), ByRef rng As Random) As String
        Dim roll As Integer = rng.Next(targetList.Count)
        GetNamePart = targetList(roll)
        targetList.RemoveAt(roll)
    End Function
    Public Overrides Function ToString() As String
        Return Name
    End Function

    Public Name As String
    Private Race As CrewRace
    Public Enum CrewRace
        Human
        Seatouched
        Ghost
    End Enum

#Region "Bonuses"
    Private Scars As New List(Of CrewBonus)
    Private Mutations As New List(Of CrewBonus)
    Private Equipment As New List(Of CrewBonus)
    Private CrewBonuses As List(Of CrewBonus)() = {Scars, Mutations, Equipment}
    Private Sub AddBonus(ByVal listName As String, ByVal effect As CrewBonus)
        'check slot against all lists
        For Each cbl In CrewBonuses
            If GetSlot(cbl, effect.Slot) Is Nothing = False Then Exit Sub
        Next

        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
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
            Case "scars", "scar" : Return Scars
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

        Public Skill As CrewSkill
        Public Damage As Integer
        Public DamageType As DamageType
        Public ReadOnly Property IsReady(ByVal ship As Ship) As Boolean
            Get
                If AmmoUse <= 0 Then Return True

                Dim AmmoType As GoodType = GetAmmoType()
                If Ship.GetGood(AmmoType) >= AmmoUse Then Return True

                Return False
            End Get
        End Property

        Public AmmoUse As Integer
        Public Function GetAmmoType() As GoodType
            Select Case DamageType
                Case Pirates.DamageType.Firearms : Return GoodType.Bullets
                Case Else : Return Nothing
            End Select
        End Function
    End Class
#End Region

#Region "Skills"
    Private Skills As New Dictionary(Of CrewSkill, Integer)
    Private SkillsXP As New Dictionary(Of CrewSkill, Integer)
    Private Shared SkillThresholds As Integer() = {0, 100, 300, 600, 1000, 1500}
    Public Function GetSkill(ByVal cs As CrewSkill)
        Dim total As Integer = Skills(cs)
        For Each s In Scars
            If s.SkillBonuses.ContainsKey(cs) Then total += s.SkillBonuses(cs)
        Next
        For Each m In Mutations
            If m.SkillBonuses.ContainsKey(cs) Then total += m.SkillBonuses(cs)
        Next
        For Each e In Equipment
            If e.SkillBonuses.ContainsKey(cs) Then total += e.SkillBonuses(cs)
        Next
        Return total
    End Function
    Private Function GetBestSkill(ByVal meleeOnly As Boolean) As CrewSkill
        Dim bestSkill As CrewSkill = Nothing
        Dim bestSkillValue As Integer = -1
        For Each cs In [Enum].GetValues(GetType(CrewSkill))
            If meleeOnly = False OrElse cs > 100 Then
                Dim skill As CrewSkill = cs
                Dim skillValue As Integer = GetSkill(cs)
                If skillValue > bestSkillValue Then
                    bestSkill = skill
                    bestSkillValue = skillValue
                End If
            End If
        Next
        Return bestSkill
    End Function
    Private Function GetBestWeapon() As CrewBonus
        Dim bestWeapon As CrewBonus = Nothing
        Dim bestWeaponValue As Integer = -1
        For Each cbl In CrewBonuses
            For Each cb In cbl
                If cb.IsReady(Ship) = True AndAlso cb.Damage > bestWeaponValue Then
                    bestWeapon = cb
                    bestWeaponValue = cb.Damage
                End If
            Next
        Next
        Return bestWeapon
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
        Leadership = 1
        Sailing
        Navigation
        Gunnery

        Firearms = 101
        Melee
    End Enum
#End Region

#Region "Melee Combat"
    Private DamageLog As New List(Of Damage)
    Private Health As Integer
    Private DamageSustained As Integer

    Public Sub MeleeAttack(ByRef targets As List(Of Crew))
        Dim roll As Integer = Dev.Rng.Next(targets.Count - 1)
        Dim target As Crew = targets(roll)
        targets.RemoveAt(roll)

        MeleeAttack(target)
    End Sub
    Public Sub MeleeAttack(ByRef target As Crew)
        Dim weapon As CrewBonus = GetBestWeapon()
        If weapon Is Nothing Then Exit Sub

        Dim damage As New Damage(weapon.Damage, weapon.DamageType, Name)
        target.Damage(damage)
        If weapon.AmmoUse > 0 Then
            Ship.AddGood(weapon.getAmmoType, -weapon.AmmoUse)
        End If
    End Sub
    Private Sub Damage(ByVal damage As Damage)
        If Ship Is Nothing Then Exit Sub

        DamageSustained += damage.Amt
        DamageLog.Add(damage)
        Report.Add("[" & Ship.ID & "] " & Name & " was struck for " & damage.Amt & " damage.")

        If DamageSustained >= Health Then
            Dim battlefield As Battlefield = Ship.BattleSquare.Battlefield
            battlefield.DeadCrew.Add(Me)
            Report.Add(Name & " has perished in battle!")
        End If
    End Sub
#End Region

#Region "Movement"
    Public Ship As Ship
    Private ShipQuarter As ShipQuarter

    Public Sub Move(ByVal quarter As ShipQuarter)
        Ship.RemoveCrew(Pirates.ShipQuarter.Starboard, Me)
        Ship.AddCrew(quarter, Me)
        ShipQuarter = quarter
    End Sub
#End Region
End Class
