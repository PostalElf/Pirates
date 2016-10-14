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
            .Health = 100

            Dim cutlass As New CrewBonus
            With cutlass
                .Name = "Belaying Pin"
                .Skill = CrewSkill.Melee
                .Damage = 10
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

        Dim prefix As String = Dev.GetRandom(Of String)(NamePrefixes)
        Dim suffix As String = Dev.GetRandom(Of String)(NameSuffixes)
        Return prefix & " " & suffix
    End Function
    Private Shared NamePrefixes As New List(Of String)
    Private Shared NameSuffixes As New List(Of String)
    Public Overrides Function ToString() As String
        Return Name
    End Function

    Public Name As String
    Private Race As CrewRace

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
#End Region

#Region "Skills"
    Public Role As CrewSkill
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
#End Region

#Region "Combat"
    Private DamageLog As New List(Of Damage)
    Private Health As Integer
    Private DamageSustained As Integer

    Public Sub MeleeAttack(ByRef targets As List(Of Crew))
        Dim target As Crew = Dev.GetRandom(targets)
        MeleeAttack(target)
    End Sub
    Public Sub MeleeAttack(ByRef target As Crew)
        Dim weapon As CrewBonus = GetBestWeapon()
        If weapon Is Nothing Then Exit Sub
        Dim skill As CrewSkill = weapon.Skill

        'roll skill vs skill
        Dim attSkill As Integer = GetSkill(skill) + Dev.FateRoll()
        Dim defSkill As Integer = target.GetSkill(skill) + Dev.FateRoll()
        If attSkill > defSkill Then
            'success, damage
            Dim damage As New Damage(0, weapon.Damage, weapon.DamageType, Name)
            target.Damage(damage)
            If weapon.AmmoUse > 0 Then
                Ship.AddGood(weapon.GetAmmoType, -weapon.AmmoUse)
            End If
        ElseIf attSkill = defSkill Then
            'glancing hit
            Dim dmgValue As Integer = Dev.Constrain(weapon.Damage / 2, 1)
            Dim damage As New Damage(0, dmgValue, weapon.DamageType, Name)
            target.Damage(damage)
            If weapon.AmmoUse > 0 Then
                Ship.AddGood(weapon.GetAmmoType, -weapon.AmmoUse)
            End If
        Else
            'miss
            Report.Add("[" & target.Ship.ID & "] " & target.Name & " fended off an attack from " & Name & ".")
        End If
    End Sub
    Public Sub ShipAttack(ByVal accuracy As Integer, ByVal damage As Damage)
        If damage.CrewDamage <= 0 Then Exit Sub

        Dim skill As Integer = GetSkill(CrewSkill.Bracing) + Dev.FateRoll()
        If skill > accuracy + Dev.FateRoll() Then
            'Dim nuDamage As Damage = damage.Clone(damage)
            'nuDamage.CrewDamage = Dev.Constrain(nuDamage.CrewDamage / 2, 1)
            damage.CrewDamage = Dev.Constrain(damage.CrewDamage / 2, 1)
            Me.Damage(damage)
        Else
            Me.Damage(damage)
        End If
    End Sub
    Private Sub Damage(ByVal damage As Damage)
        If Ship Is Nothing Then Exit Sub
        If damage.CrewDamage <= 0 Then Exit Sub

        DamageSustained += damage.CrewDamage
        DamageLog.Add(damage)
        Report.Add("[" & Ship.ID & "] " & Name & " was struck for " & damage.CrewDamage & " damage by " & damage.Sender & ".")

        If DamageSustained >= Health Then
            Dim battlefield As Battlefield = Ship.BattleSquare.Battlefield
            battlefield.AddDead(Me)
            Report.Add("[" & Ship.ID & "] " & Name & " has perished in battle!")
        End If
    End Sub
#End Region

#Region "Movement"
    Public Ship As Ship
    Public ShipQuarter As ShipQuarter

    Public Sub Move(ByVal quarter As ShipQuarter, Optional ByVal role As CrewSkill = Nothing)
        Dim keepShip As Ship = Ship
        Ship.RemoveCrew(ShipQuarter, Me)
        keepShip.AddCrew(quarter, Me)
        ShipQuarter = quarter
        If role <> Nothing Then Me.Role = role
    End Sub
#End Region

#Region "Console"
    Public Sub ConsoleReport()
        Dim s As String = Dev.vbSpace(1)
        Dim ss As String = Dev.vbSpace(2)
        Dim t As Integer = 13

        Console.WriteLine(Name)
        Console.WriteLine(s & "Race:   " & Race.ToString)

        Console.WriteLine(s & "Skills: ")
        For Each k In Skills.Keys
            Console.WriteLine(ss & Dev.vbTab(k.ToString & ":", t) & GetSkill(k))
        Next

        If Equipment.Count > 0 Then ConsoleReportList(Equipment, "Gear:")
        If Mutations.Count > 0 Then ConsoleReportList(Mutations, "Mutations:")
    End Sub
    Private Sub ConsoleReportList(ByVal l As List(Of CrewBonus), ByVal title As String)
        Console.WriteLine(Dev.vbSpace(1) & title)
        For Each i In l
            With i
                Console.Write(Dev.vbSpace(2) & "(" & .Slot & ") " & .Name)
                If .Damage > 0 Then Console.Write(" - " & .Damage & " " & .DamageType.ToString)
                If .AmmoUse > 0 Then Console.Write(" - " & .AmmoUse & " " & .GetAmmoType)
                Console.WriteLine()
            End With
        Next
    End Sub
#End Region
End Class
