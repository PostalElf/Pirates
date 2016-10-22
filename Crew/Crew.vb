﻿Public Class Crew
    Public Sub New()
        For Each cs In [Enum].GetValues(GetType(CrewSkill))
            Skills.Add(cs, 0)
            SkillsXP.Add(cs, 0)
        Next
    End Sub
    Public Shared Function Generate(ByVal aRace As CrewRace, Optional ByRef rng As Random = Nothing) As Crew
        If rng Is Nothing Then rng = New Random

        Dim crew As New Crew
        With crew
            .Name = GenerateName(aRace, rng)
            .Race = aRace
            .Health = 100
            .Morale = 100

            Dim pin As New CrewBonus
            With pin
                .Name = "Belaying Pin"
                .Skill = CrewSkill.Melee
                .Damage = 10
                .DamageType = DamageType.Blunt
                .AmmoUse = 0
                .Slot = "Right Hand"
            End With
            .AddBonus("equipment", pin)

            Dim skills As New List(Of CrewSkill)([Enum].GetValues(GetType(CrewSkill)))
            For n = 1 To 2
                Dim cs As CrewSkill = Dev.GrabRandom(Of CrewSkill)(skills, rng)
                .AddSkillXP(cs, SkillThresholds(n))
                If n = 2 Then .addtalent(GenerateTalent(aRace, cs))
            Next
        End With
        Return crew
    End Function
    Private Shared Function GenerateName(ByVal race As CrewRace, ByRef rng As Random) As String
        If rng Is Nothing Then rng = New Random
        If NamePrefixes.Count = 0 Then NamePrefixes = IO.SimpleFilegetAll("namePrefixes.txt")
        If NameSuffixes.Count = 0 Then NameSuffixes = IO.SimpleFilegetAll("nameSuffixes.txt")

        Dim prefix As String = Dev.GrabRandom(Of String)(NamePrefixes, rng)
        Dim suffix As String = Dev.GrabRandom(Of String)(NameSuffixes, rng)
        Return prefix & " " & suffix
    End Function
    Private Shared Function GenerateTalent(ByVal aRace As CrewRace, ByVal bestSkill As CrewSkill) As CrewTalent
        Dim possibleTalents As New List(Of CrewTalent)
        For Each talent In [Enum].GetValues(GetType(CrewTalent))
            'anything above 100 is a trained talent
            If talent < 100 Then possibleTalents.Add(talent)
        Next
        Return Dev.GetRandom(Of CrewTalent)(possibleTalents, World.Rng)
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
    Public Function CheckAddBonus(ByVal listName As String, ByVal effect As CrewBonus) As Boolean
        'check slot against all lists
        For Each cbl In CrewBonuses
            If effect.Slot Is Nothing = False Then
                If GetSlot(cbl, effect.Slot) Is Nothing = False Then Return False
            End If
        Next

        Return True
    End Function
    Public Sub AddBonus(ByVal listName As String, ByVal effect As CrewBonus)
        If CheckAddBonus(listName, effect) = False Then Exit Sub

        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
        targetList.Add(effect)
    End Sub
    Public Function CheckRemoveBonus(ByVal listname As String, ByVal effect As CrewBonus)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listname)
        If targetList.Contains(effect) = False Then Return False
        Return True
    End Function
    Public Function CheckRemoveBonus(ByVal listname As String, ByVal slot As String)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listname)
        Dim target As CrewBonus = GetSlot(targetList, slot)
        If target Is Nothing Then Return False

        Return True
    End Function
    Public Sub RemoveBonus(ByVal listName As String, ByVal effect As CrewBonus)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
        If targetList.Contains(effect) Then targetList.Remove(effect)
    End Sub
    Public Sub RemoveBonus(ByVal listName As String, ByVal slot As String)
        Dim targetList As List(Of CrewBonus) = GetBonusList(listName)
        Dim target As CrewBonus = GetSlot(targetList, slot)
        If target Is Nothing = False Then targetList.Remove(target)
    End Sub
    Public Function GetBonus(ByVal listName As String, ByVal name As String) As CrewBonus
        Dim cbl As List(Of CrewBonus) = GetBonusList(listName)
        For Each cb In cbl
            If cb.Name = name Then Return cb
        Next
        Return Nothing
    End Function
    Public Function GetBonusList(ByVal listName As String) As List(Of CrewBonus)
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
    Private Function GetArmour(ByVal damageType As DamageType) As Integer
        Dim total As Integer = 0
        For Each cbl As List(Of CrewBonus) In CrewBonuses
            For Each cb As CrewBonus In cbl
                If cb.Armour.ContainsKey(damageType) Then total += cb.Armour(damageType)
            Next
        Next
        Return total
    End Function

    Private Function GenerateScar(ByVal damage As Damage, ByVal fleshshapingBonus As Integer) As CrewBonus
        Dim scarNames As List(Of String) = GenerateScarNames()
        Dim scarName As String = Dev.GetRandom(Of String)(scarNames, World.Rng, fleshshapingBonus)

        Dim total As New CrewBonus
        With total
            .Name = scarName
            Select Case scarName
                Case "Hook Hand"
                    .Slot = "Right Hand"
                    .Skill = CrewSkill.Melee
                    .Damage = 10
                    .DamageType = DamageType.Blade

                Case "Gun Hand"
                    .Slot = "Left Hand"
                    .Skill = CrewSkill.Firearms
                    .Damage = 25
                    .DamageType = DamageType.Firearms
                    .AmmoUse = 1

                Case "Dimwitted"
                    .SkillBonuses.Add(CrewSkill.Leadership, -2)
                    .SkillBonuses.Add(CrewSkill.Medicine, -2)

                Case "Half-Blind"
                    .SkillBonuses.Add(CrewSkill.Navigation, -2)
                    .SkillBonuses.Add(CrewSkill.Gunnery, -2)

                Case "Anosmia"
                    .SkillBonuses.Add(CrewSkill.Alchemy, -2)
                    .SkillBonuses.Add(CrewSkill.Cooking, -2)

                Case "Lungshot"
                    .SkillBonuses.Add(CrewSkill.Melee, -2)
                    .SkillBonuses.Add(CrewSkill.Sailing, -2)

                Case "Pegleg"
                    .Slot = "Feet"
                    .SkillBonuses.Add(CrewSkill.Athletics, -2)

                Case "Touch of Death"
                    .Slot = "Talisman"
                    .Skill = CrewSkill.Alchemy
                    .Damage = 10
                    .DamageType = DamageType.Necromancy

                Case "Tentacled Arm"
                    .Slot = "Left Hand"
                    .Skill = CrewSkill.Melee
                    .Damage = 20
                    .DamageType = DamageType.Blunt

                Case "Crabclaw"
                    .Slot = "Right Hand"
                    .Skill = CrewSkill.Melee
                    .Damage = 10
                    .DamageType = DamageType.Blade
                    .Armour.Add(DamageType.Blade, 5)

                Case "Angler's Light"
                    .Slot = "Head"
                    .Armour.Add(DamageType.Necromancy, 15)

                Case "Sharkskin"
                    .Slot = "Body"
                    .Armour.Add(DamageType.Blunt, 20)

                Case "Greenskin"
                    .Slot = "Body"

                Case "Hardened Carapace"
                    .Slot = "Body"
                    .Armour.Add(DamageType.Blade, 10)

                Case "Writhing Mass"
                    .Slot = "Feet"
                    .SkillBonuses.Add(CrewSkill.Athletics, +1)

                Case Else : Throw New Exception("Out of roll range")
            End Select
        End With
        Return total
    End Function
    Private Function GenerateScarNames() As List(Of String)
        If Me.Race = CrewRace.Unrelinquished Then Return Nothing
        Dim scarNames As New List(Of String)
        scarNames.AddRange({"Hook Hand", "Gun Hand", "Dimwitted", "Half-Blind", "Anosmia", "Lungshot", "Pegleg", "Touch of Death"})
        If Me.Race = CrewRace.Seatouched Then
            scarNames.AddRange({"Tentacled Arm", "Crabclaw", "Angler's Light", "Sharkskin", "Greenskin", "Hardened Carapace", "Writhing Mass"})
        End If

        For Each thing In Scars
            If scarNames.Contains(thing.Name) Then scarNames.Remove(thing.Name)
        Next
        If scarNames.Count <= 0 Then Return Nothing Else Return scarNames
    End Function
#End Region

#Region "Skills"
    Private Skills As New Dictionary(Of CrewSkill, Integer)
    Private SkillsXP As New Dictionary(Of CrewSkill, Double)
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
    Public Sub AddSkillXP(ByVal cs As CrewSkill, ByVal value As Double)
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

    Private Talents As New List(Of CrewTalent)
    Public Function GetTalent(ByVal t As CrewTalent) As Boolean
        Return Talents.Contains(t)
    End Function
    Public Function CheckAddTalent(ByVal t As CrewTalent) As Boolean
        If Talents.Contains(t) Then Return False
        Return True
    End Function
    Public Sub AddTalent(ByVal t As CrewTalent)
        If CheckAddTalent(t) = False Then Exit Sub
        Talents.Add(t)
    End Sub
#End Region

#Region "Combat"
    Private DamageLog As New List(Of Damage)
    Private Health As Integer
    Private DamageSustained As Integer

    Public Sub MeleeAttack(ByRef targets As List(Of Crew))
        Dim target As Crew = Dev.GrabRandom(targets, World.Rng)
        MeleeAttack(target)
    End Sub
    Public Sub MeleeAttack(ByRef target As Crew)
        Dim weapon As CrewBonus = GetBestWeapon()
        If weapon Is Nothing Then Exit Sub
        Dim skill As CrewSkill = weapon.Skill

        'roll skill vs skill
        Dim attSkill As Integer = GetSkill(skill) + Dev.FateRoll(World.Rng)
        Dim defSkill As Integer = target.GetSkill(skill) + Dev.FateRoll(World.Rng)
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

        'add xp
        Dim xp As Double = 0.5
        AddSkillXP(skill, xp)
        target.AddSkillXP(skill, xp)
    End Sub
    Public Sub ShipAttack(ByVal accuracy As Integer, ByVal damage As Damage)
        If damage.CrewDamage <= 0 Then Exit Sub

        Dim skill As Integer = GetSkill(CrewSkill.Bracing) + Dev.FateRoll(World.Rng)
        If skill > accuracy + Dev.FateRoll(World.Rng) Then
            'Dim nuDamage As Damage = damage.Clone(damage)
            'nuDamage.CrewDamage = Dev.Constrain(nuDamage.CrewDamage / 2, 1)
            damage.CrewDamage = Dev.Constrain(damage.CrewDamage / 2, 1)
            Me.Damage(damage)
        Else
            Me.Damage(damage)
        End If

        Dim xp As Double = 1
        AddSkillXP(CrewSkill.Bracing, xp)
    End Sub
    Private Sub Damage(ByVal damage As Damage)
        If Ship Is Nothing Then Exit Sub
        If damage.CrewDamage <= 0 Then Exit Sub

        DamageSustained += (damage.CrewDamage - GetArmour(damage.Type))
        DamageLog.Add(damage)
        Dim repType As ReportType
        If TypeOf Ship Is ShipPlayer Then repType = ReportType.CrewDamage Else repType = ReportType.EnemyCrewDamage
        Report.Add("[" & Ship.ID & "] " & Name & " was struck for " & damage.CrewDamage & " damage by " & damage.Sender & ".", repType)

        If DamageSustained >= Health Then Death()
    End Sub
    Private Function GetWorstDamage() As Damage
        Dim worstDmg As Damage = Nothing
        For Each dmg In DamageLog
            If dmg.CrewDamage < worstDmg.CrewDamage Then worstDmg = dmg
        Next
        Return worstDmg
    End Function
    Private Sub Death()
        Dim battlefield As Battlefield = Ship.BattleSquare.Battlefield
        If battlefield Is Nothing = False Then battlefield.AddDead(Me)
        Dim repType As ReportType
        If TypeOf Ship Is ShipPlayer Then repType = ReportType.CrewDeath Else repType = ReportType.EnemyCrewDeath
        Report.Add("[" & Ship.ID & "] " & Name & " has perished!", repType)
    End Sub
#End Region

#Region "Movement"
    Public Station As New CrewStation
    Public BattleStation As New CrewStation
    Public Ship As Ship
    Public ShipQuarter As ShipQuarter
    Public Role As CrewRole
    Public Sub SetStation(ByVal aStation As CrewStation, ByVal inCombat As Boolean)
        If inCombat = True Then BattleStation = aStation Else Station = aStation
    End Sub

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
    Private Function ConsumeGoods(ByVal goodType As GoodType, ByVal qty As Integer, ByVal positiveChange As Integer, ByVal negativeChange As Integer) As Integer
        Dim good As Good = good.Generate(goodType, -qty)

        'shortcircuit for Greenskin mutation
        If goodType = Pirates.GoodType.Rations AndAlso GetBonus("scar", "Greenskin") Is Nothing = False Then Return positiveChange


        If Ship.GoodsFreeForConsumption(goodType) = False OrElse Ship.CheckAddGood(good) = False Then
            Return negativeChange
        Else
            Dim ps As ShipPlayer = CType(Ship, ShipPlayer)
            ps.AddGood(good)
            If ps.GoodsConsumed.ContainsKey(goodType) = False Then ps.GoodsConsumed.Add(goodType, good.Generate(goodType))
            ps.GoodsConsumed(goodType) += good
            Return positiveChange
        End If
    End Function

    Public Enum CrewMorale
        Motivated
        Content
        Neutral
        Unhappy
        Mutinous
    End Enum
#End Region

#Region "World"
    Public Sub Tick(ByVal doctor As Crew)
        TickMorale()
        TickHeal(doctor)
    End Sub
    Private Sub TickMorale()
        'morale ranges from 1 to 100
        Dim change As Integer = 0
        Dim hasEaten As Boolean = True

        Select Case Race
            Case CrewRace.Human
                'humans need rations and water; desire coffee and liqour
                change += ConsumeGoods(GoodType.Rations, 1, 0, -5)
                If change = -5 Then hasEaten = False
                change += ConsumeGoods(GoodType.Water, 1, 0, -10)
                If change >= -5 Then
                    change += ConsumeGoods(GoodType.Coffee, 1, 1, 0)
                    change += ConsumeGoods(GoodType.Liqour, 1, 2, 0)
                End If

            Case CrewRace.Seatouched
                'seatouched need rations, water and prayer; desire salt
                change += ConsumeGoods(GoodType.Rations, 1, 0, -5)
                If change = -5 Then hasEaten = False
                change += ConsumeGoods(GoodType.Water, 1, 0, -10)
                If Shrine Is Nothing Then change -= 5 : Exit Select
                If change >= -5 Then
                    change += ConsumeGoods(GoodType.Salt, 1, 1, 0)
                    change += Math.Ceiling(Shrine.Quality / 2)
                End If

            Case CrewRace.Windsworn
                'windsworn need rations and water; desire tobacco and spice
                change += ConsumeGoods(GoodType.Rations, 1, 0, -5)
                If change = -5 Then hasEaten = False
                change += ConsumeGoods(GoodType.Water, 1, 0, -5)
                If change >= -5 Then
                    change += ConsumeGoods(GoodType.Tobacco, 1, 2, 0)
                    change += ConsumeGoods(GoodType.Spice, 1, 1, 0)
                End If

            Case CrewRace.Unrelinquished
                'unrelinquished need mordicus; desire leadership
                hasEaten = False
                change += ConsumeGoods(GoodType.Mordicus, 1, 0, -10)
                If change > -10 Then
                    If Ship.GetSkill(Nothing, CrewSkill.Leadership) >= 7 Then change += 1
                End If
        End Select

        'other bonuses
        If change >= 0 Then
            If Quarters Is Nothing Then Throw New Exception("Crew has no quarters.")
            change += (Quarters.Quality - 2)
            change = Dev.Constrain(change, 0, 100)

            'cooking
            If hasEaten = True Then
                Dim cooks As List(Of Crew) = Ship.GetCrews(Nothing, CrewRole.Cook)
                If cooks.Count = 1 Then
                    Dim cook As Crew = cooks(0)
                    Dim cooking As Integer = cook.GetSkillFromRole
                    change += Math.Ceiling(cooking / 2)

                    Dim xp As Double = 0.25
                    cook.AddSkillXP(CrewSkill.Cooking, xp)
                End If
            End If
        End If

        'apply
        Morale += change
        CType(Ship, ShipPlayer).MoraleChange += change
    End Sub
    Private Sub TickHeal(ByVal doctor As Crew)
        'doctors have disadvantage when treating patients not of their race
        'failure to treat will deal damage
        'unrelinquished do not gain scars but are harder to treat
        'seatouched have a chance to gain mutation instead of scar

        Dim currentDamage As Damage = GetWorstDamage()
        If currentDamage Is Nothing Then Exit Sub

        Dim skill As Integer = doctor.GetSkillFromRole + Dev.FateRoll(World.Rng)
        If doctor.Race <> Me.Race Then skill -= 2
        If Me.Race = CrewRace.Unrelinquished Then skill -= 1

        If skill > currentDamage.CrewDamage Then
            DamageLog.Remove(currentDamage)
            DamageSustained -= currentDamage.CrewDamage
            Report.Add("The ship doctor successfully treated " & Name & "'s worst injuries.", ReportType.Doctor)
        ElseIf skill = currentDamage.CrewDamage Then
            DamageLog.Remove(currentDamage)
            DamageSustained -= currentDamage.CrewDamage
            Report.Add("The ship doctor treated " & Name & "'s worst injuries.", ReportType.Doctor)

            If Me.Race <> CrewRace.Unrelinquished Then
                Dim scarRollBonus As Integer = 0
                If doctor.GetTalent(CrewTalent.Fleshshaper) = True AndAlso Me.Race = CrewRace.Seatouched Then scarRollBonus += 4
                Dim scar As CrewBonus = GenerateScar(currentDamage, scarRollBonus)
                If scar Is Nothing Then Exit Sub
                AddBonus("scar", scar)
                Report.Add(Name & " gains a new scar: " & scar.Name, ReportType.Doctor)

                'check for old scars and overwrite if necessary
                If scar.Slot <> Nothing Then
                    Dim oldScar As CrewBonus = GetSlot(GetBonusList("scar"), scar.Slot)
                    If oldScar Is Nothing = False Then
                        RemoveBonus("scar", scar.Slot)
                        Report.Add(Name & "'s new scar replaces an old scar: " & oldScar.Name, ReportType.Doctor)
                    End If
                End If
            End If
        Else
            Report.Add("The ship doctor failed to treat " & Name & "'s worst injuries.", ReportType.Doctor)
            DamageSustained += 5
            If DamageSustained > Health Then Death()
        End If

    End Sub
#End Region

#Region "Console"
    Public Sub ConsoleReport()
        Dim s As String = Dev.vbSpace(1)
        Dim ss As String = Dev.vbSpace(2)
        Dim t As Integer = 13

        Console.WriteLine(Name)
        Console.WriteLine(s & "Race:   " & Race.ToString)
        Console.WriteLine(s & "Morale: " & Morale & "/100 (" & MoraleLevel.ToString & ")")

        Console.WriteLine(s & "Skills: ")
        For Each k In Skills.Keys
            Console.WriteLine(ss & Dev.vbTab(k.ToString & ":", t) & GetSkill(k))
        Next

        Console.WriteLine(s & "Talents: ")
        For Each talent In Talents
            Console.WriteLine(ss & talent.ToString)
        Next

        If Equipment.Count > 0 Then
            For Each cb In Equipment
                Console.WriteLine(s & "Equipment:")
                Console.WriteLine(ss & cb.ToString)
            Next
        End If
    End Sub
#End Region
End Class
