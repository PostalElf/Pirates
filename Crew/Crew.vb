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
                .Slot = "Right Hand"
            End With
            .AddBonus("equipment", cutlass)

            Dim skills As New List(Of CrewSkill)([Enum].GetValues(GetType(CrewSkill)))
            For n = 1 To 2
                Dim cs As CrewSkill = Dev.GrabRandom(Of CrewSkill)(skills, rng)
                .AddSkillXP(cs, SkillThresholds(n))
            Next
        End With
        Return crew
    End Function
    Private Shared Function GenerateName(ByVal race As CrewRace, Optional ByRef rng As Random = Nothing) As String
        If rng Is Nothing Then rng = New Random
        If NamePrefixes.Count = 0 Then NamePrefixes = IO.SimpleFilegetAll("namePrefixes.txt")
        If NameSuffixes.Count = 0 Then NameSuffixes = IO.SimpleFilegetAll("nameSuffixes.txt")

        Dim prefix As String = Dev.GrabRandom(Of String)(NamePrefixes, rng)
        Dim suffix As String = Dev.GrabRandom(Of String)(NameSuffixes, rng)
        Return prefix & " " & suffix
    End Function
    Private Shared NamePrefixes As New List(Of String)
    Private Shared NameSuffixes As New List(Of String)
    Public Overrides Function ToString() As String
        Return Name
    End Function

    Public Name As String
    Public Race As CrewRace

#Region "Bonuses"
    Private Scars As New List(Of CrewBonus)
    Private Equipment As New List(Of CrewBonus)
    Private CrewBonuses As List(Of CrewBonus)() = {Scars, Equipment}
    Private Function CheckAddBonus(ByVal listName As String, ByVal effect As CrewBonus) As Boolean
        'check slot against all lists
        For Each cbl In CrewBonuses
            If GetSlot(cbl, effect.Slot) Is Nothing = False Then Return False
        Next

        Return True
    End Function
    Private Sub AddBonus(ByVal listName As String, ByVal effect As CrewBonus)
        If CheckAddBonus(listName, effect) = False Then Exit Sub

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

    Private Function GenerateScar(ByVal damage As Damage, ByVal exclude As List(Of CrewBonus)) As CrewBonus
        Dim scarNames As New List(Of String)({"Hook Hand", "Gun Hand", "Cracked Skull", "Fractured Ribs", "Pegleg", _
                                              "Touch of Death"})
        For Each thing In exclude
            If scarNames.Contains(thing.Name) Then scarNames.remove(thing.Name)
        Next
        If scarNames.Count <= 0 Then Return Nothing

        Dim roll As Integer = Dev.Rng.Next(0, scarNames.Count)
        Dim scarName As String = scarNames(roll)
        Dim total As New CrewBonus
        With total
            .Name = scarName
            Select Case scarName
                Case "Hook Hand"
                    .Slot = "Right Hand"
                    .Skill = CrewSkill.Melee
                    .Damage = 10
                    .DamageType = DamageType.Blade
                    .AmmoUse = 0

                Case "Gun Hand"
                    .Slot = "Left Hand"
                    .Skill = CrewSkill.Firearms
                    .Damage = 25
                    .DamageType = DamageType.Firearms
                    .AmmoUse = 1

                Case "Cracked Skull"
                    .Slot = "Head"
                    .Skill = Nothing
                    .Damage = 0
                    .DamageType = Nothing
                    .AmmoUse = 0

                Case "Fractured Ribs"
                    .Slot = "Body"
                    .Skill = Nothing
                    .Damage = 0
                    .DamageType = Nothing
                    .AmmoUse = 0

                Case "Pegleg"
                    .Slot = "Feet"
                    .Skill = Nothing
                    .Damage = 0
                    .DamageType = Nothing
                    .AmmoUse = 0

                Case "Touch of Death"
                    .Slot = "Talisman"
                    .Skill = CrewSkill.Alchemy
                    .Damage = 10
                    .DamageType = DamageType.Necromancy
                    .AmmoUse = 0

                Case Else : Throw New Exception("Out of roll range")
            End Select
        End With
        Return total
    End Function
#End Region

#Region "Skills"
    Public Role As CrewRole
    Private Skills As New Dictionary(Of CrewSkill, Integer)
    Private SkillsXP As New Dictionary(Of CrewSkill, Integer)
    Private Shared SkillThresholds As Integer() = {0, 100, 300, 600, 1000, 1500}
    Public Function GetSkill(ByVal cs As CrewSkill)
        Dim total As Integer = Skills(cs)
        For Each s In Scars
            If s.SkillBonuses.ContainsKey(cs) Then total += s.SkillBonuses(cs)
        Next
        For Each e In Equipment
            If e.SkillBonuses.ContainsKey(cs) Then total += e.SkillBonuses(cs)
        Next

        'specific bonuses
        If cs = CrewSkill.Leadership AndAlso (Role = CrewRole.FirstMate OrElse Role = CrewRole.Captain) Then
            total += ConvertQualityToBonus(ShipModule.ModuleType.Quarterdeck)
        ElseIf cs = CrewSkill.Sailing AndAlso Role = CrewRole.Helmsman Then
            total += ConvertQualityToBonus(ShipModule.ModuleType.Helm)
        ElseIf cs = CrewSkill.Navigation AndAlso Role = CrewRole.Navigator Then
            total += ConvertQualityToBonus(ShipModule.ModuleType.Maproom)
        End If

        Return total
    End Function
    Private Function ConvertQualityToBonus(ByVal moduleType As ShipModule.ModuleType) As Integer
        Dim quality As Integer = Ship.GetModules(moduleType)(0).Quality
        Select Case quality
            Case 0, 1, 2 : Return 0
            Case 3, 4 : Return 1
            Case 5 : Return 2
            Case Else : Throw New Exception
        End Select
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
    Public Function GetSkillFromRole() As Integer
        Dim cs As CrewSkill = ConvertRoleToSkill(Role)
        If cs = Nothing Then Return -1
        Return GetSkill(cs)
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

    Public Shared Function ConvertSkillToRole(ByVal skill As CrewSkill) As CrewRole
        Select Case skill
            Case CrewSkill.Leadership : Return CrewRole.Captain
            Case CrewSkill.Cooking : Return CrewRole.Cook
            Case CrewSkill.Gunnery : Return CrewRole.Gunner
            Case CrewSkill.Sailing : Return CrewRole.Sailor
            Case CrewSkill.Navigation : Return CrewRole.Navigator
            Case CrewSkill.Alchemy : Return CrewRole.Alchemist
            Case CrewSkill.Medicine : Return CrewRole.Doctor
            Case Else : Return Nothing
        End Select
    End Function
    Public Shared Function ConvertRoleToSkill(ByVal role As CrewRole) As CrewSkill
        Select Case role
            Case CrewRole.Captain, CrewRole.FirstMate : Return CrewSkill.Leadership
            Case CrewRole.Cook : Return CrewSkill.Cooking
            Case CrewRole.Gunner : Return CrewSkill.Gunnery
            Case CrewRole.Sailor, CrewRole.Helmsman : Return CrewSkill.Sailing
            Case CrewRole.Navigator : Return CrewSkill.Navigation
            Case CrewRole.Alchemist : Return CrewSkill.Alchemy
            Case CrewRole.Doctor : Return CrewSkill.Medicine
            Case Else : Return Nothing
        End Select
    End Function
#End Region

#Region "Combat"
    Private DamageLog As New List(Of Damage)
    Private Health As Integer
    Private DamageSustained As Integer

    Public Sub MeleeAttack(ByRef targets As List(Of Crew))
        Dim target As Crew = Dev.GrabRandom(targets)
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
            Report.Add("[" & target.Ship.ID & "] " & target.Name & " fended off an attack from " & Name & ".", ReportType.CrewAttack)
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
        Dim repType As ReportType
        If TypeOf Ship Is ShipPlayer Then repType = ReportType.CrewDamage Else repType = ReportType.EnemyCrewDamage
        Report.Add("[" & Ship.ID & "] " & Name & " was struck for " & damage.CrewDamage & " damage by " & damage.Sender & ".", repType)

        If DamageSustained >= Health Then Death()
    End Sub
    Private Sub Death()
        Dim battlefield As Battlefield = Ship.BattleSquare.Battlefield
        If battlefield Is Nothing = False Then battlefield.AddDead(Me)
        Dim repType As ReportType
        If TypeOf Ship Is ShipPlayer Then repType = ReportType.CrewDeath Else repType = ReportType.EnemyCrewDeath
        Report.Add("[" & Ship.ID & "] " & Name & " has perished!", repType)
    End Sub
#End Region

#Region "Movement"
    Public Ship As Ship
    Public ShipQuarter As ShipQuarter
    Public Quarters As ShipModule
    Public Shrine As ShipModule
#End Region

#Region "Morale"
    Public Morale As Integer
    Public ReadOnly Property MoraleLevel As CrewMorale
        Get
            Select Case Morale
                Case Is <= 10 : Return CrewMorale.Mutinous
                Case Is <= 25 : Return CrewMorale.Unhappy
                Case Is <= 50 : Return CrewMorale.Neutral
                Case Is <= 75 : Return CrewMorale.Content
                Case Else : Return CrewMorale.Motivated
            End Select
        End Get
    End Property
    Public Sub Tick(ByVal doctor As Crew)
        TickMorale()
        TickHeal(doctor)
    End Sub
    Private Sub TickMorale()
        'morale ranges from 1 to 100
        Dim change As Integer = 0

        Select Case Race
            Case CrewRace.Human
                'humans need rations and water; desire coffee and liqour
                change += ConsumeGoods(GoodType.Rations, 1, 0, -5)
                change += ConsumeGoods(GoodType.Water, 1, 0, -10)
                If change >= -5 Then
                    change += ConsumeGoods(GoodType.Coffee, 1, 1, 0)
                    change += ConsumeGoods(GoodType.Liqour, 1, 2, 0)
                End If

            Case CrewRace.Seatouched
                'seatouched need rations, water and prayer; desire salt
                change += ConsumeGoods(GoodType.Rations, 1, 0, -5)
                change += ConsumeGoods(GoodType.Water, 1, 0, -10)
                If Shrine Is Nothing Then change -= 5 : Exit Select
                If change >= -5 Then
                    change += ConsumeGoods(GoodType.Salt, 1, 1, 0)
                    change += Math.Ceiling(Shrine.Quality / 2)
                End If

            Case CrewRace.Windsworn
                'windsworn need rations and water; desire tobacco and spice
                change += ConsumeGoods(GoodType.Rations, 1, 0, -5)
                change += ConsumeGoods(GoodType.Water, 1, 0, -5)
                If change >= -5 Then
                    change += ConsumeGoods(GoodType.Tobacco, 1, 2, 0)
                    change += ConsumeGoods(GoodType.Spice, 1, 1, 0)
                End If

            Case CrewRace.Unrelinquished
                'unrelinquished need mordicus; desire leadership
                change += ConsumeGoods(GoodType.Mordicus, 1, 0, -10)
                If change > -10 Then
                    If Ship.GetSkill(Nothing, CrewSkill.Leadership) >= 7 Then change += 1
                End If
        End Select

        'other bonuses
        If change >= 0 Then
            If Quarters Is Nothing = False Then
                change += (Quarters.Quality - 2)
                change = Dev.Constrain(change, 0, 100)
            End If
            If Race <> CrewRace.Unrelinquished Then
                Dim cooking As Integer = Ship.GetSkill(Nothing, CrewSkill.Cooking)
                change += Math.Ceiling(cooking / 2)
            End If
        End If

        'apply
        Morale += change
    End Sub
    Private Sub TickHeal(ByVal doctor As Crew)
        Dim currentDamage As Damage = GetWorstDamage()
        If currentDamage Is Nothing Then Exit Sub

        Dim damageHealed As Integer = 0
        Dim skill As Integer = doctor.GetSkillFromRole + Dev.FateRoll
        If skill > currentDamage.CrewDamage Then
            DamageLog.Remove(currentDamage)
            damageHealed -= currentDamage.CrewDamage
            Report.Add("The ship doctor successfully treated " & Name & "'s worst injuries.", ReportType.Doctor)
        ElseIf skill = currentDamage.CrewDamage Then
            DamageLog.Remove(currentDamage)
            DamageSustained -= currentDamage.CrewDamage
            Report.Add("The ship doctor sorta treated " & Name & "'s worst injuries.", ReportType.Doctor)

            Dim scar As CrewBonus = GenerateScar(currentDamage, Scars)
            AddBonus("scar", scar)
            Report.Add(Name & " gains a new scar: " & scar.Name, ReportType.Doctor)
        Else
            Report.Add("The ship doctor failed to treat " & Name & "'s worst injuries.", ReportType.Doctor)
            DamageSustained += 1
            If DamageSustained > Health Then Death()
        End If

    End Sub
    Private Function ConsumeGoods(ByVal goodType As GoodType, ByVal qty As Integer, ByVal positiveChange As Integer, ByVal negativeChange As Integer) As Integer
        Dim good As Good = good.Generate(goodType, -qty)
        If Ship.GoodsFreeForConsumption(goodType) = False OrElse Ship.CheckAddGood(good) = False Then Return negativeChange
        Ship.AddGood(good)
        Return positiveChange
    End Function
    Private Function GetWorstDamage() As Damage
        Dim worstDmg As Damage = Nothing
        For Each dmg In DamageLog
            If dmg.CrewDamage < worstDmg.CrewDamage Then worstDmg = dmg
        Next
        Return worstDmg
    End Function

    Public Enum CrewMorale
        Motivated
        Content
        Neutral
        Unhappy
        Mutinous
    End Enum
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
