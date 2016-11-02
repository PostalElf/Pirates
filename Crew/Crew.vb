Public Class Crew
    Public Sub New()
        For Each cs In [Enum].GetValues(GetType(CrewSkill))
            Skills.Add(cs, 0)
            SkillsXP.Add(cs, 0)
        Next
    End Sub
    Public Shared Function Generate(ByVal aRace As CrewRace, ByVal rng As Random, Optional ByVal skillMain As CrewSkill = Nothing, Optional ByVal skillSub As CrewSkill = Nothing) As Crew
        Dim crew As New Crew
        With crew
            .Name = GenerateName(aRace, rng)
            .Race = aRace
            .Health = 100
            .Morale = 100

            .AddBonus("equipment", CrewBonus.Generate("Belaying Pin"))

            Dim s As New List(Of CrewSkill)([Enum].GetValues(GetType(CrewSkill)))
            If skillMain <> Nothing Then s.Remove(skillMain)
            If skillSub <> Nothing Then s.Remove(skillSub)
            Dim cs As CrewSkill = Nothing
            If skillMain <> Nothing Then cs = skillMain Else cs = Dev.GrabRandom(Of CrewSkill)(s, rng)
            .AddSkillXP(cs, SkillThresholds(2))
            If skillSub <> Nothing Then cs = skillSub Else cs = Dev.GrabRandom(Of CrewSkill)(s, rng)
            .AddSkillXP(cs, SkillThresholds(1))

            Dim selectTalent As CrewTalent = GenerateTalent(aRace)
            Select Case SelectTalent
                Case CrewTalent.Ironwilled : .Health += 10
            End Select
            .AddTalent(selectTalent)

            Select Case .Race
                Case CrewRace.Human
                    .MoraleDemands.Add(GoodType.Rations, -5)
                    .MoraleDemands.Add(GoodType.Water, -10)
                    .MoraleWants.Add(GoodType.Coffee, 1)
                    .MoraleWants.Add(GoodType.Liqour, 2)
                Case CrewRace.Windsworn
                    .MoraleDemands.Add(GoodType.Rations, -5)
                    .MoraleDemands.Add(GoodType.Water, -10)
                    .MoraleWants.Add(GoodType.Tobacco, 1)
                    .MoraleWants.Add(GoodType.Spice, 2)
                Case CrewRace.Seatouched
                    .MoraleDemands.Add(GoodType.Rations, -5)
                    .MoraleDemands.Add(GoodType.Water, -10)
                    .MoraleWants.Add(GoodType.Salt, 2)
                Case CrewRace.Unrelinquished
                    .MoraleDemands.Add(GoodType.Mordicus, -10)
            End Select
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
    Private Shared Function GenerateTalent(ByVal aRace As CrewRace) As CrewTalent
        Dim possibleTalents As New List(Of CrewTalent)
        For Each talent In [Enum].GetValues(GetType(CrewTalent))
            'anything above 100 is a trained talent
            If talent < 100 Then possibleTalents.Add(talent) Else Exit For
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
    Private Function GetWeapons() As List(Of CrewBonus)
        Dim total As New List(Of CrewBonus)
        For Each cbl In CrewBonuses
            For Each cb In cbl
                If cb.IsReady(Ship) = True AndAlso cb.Damage > 0 Then total.Add(cb)
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
                    .DamageType = DamageType.Blunt
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
                    .SkillBonuses.Add(CrewSkill.Bracing, -2)

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
                    .SkillBonuses.Add(CrewSkill.Bracing, +1)

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

        'shortcircuit for non-ship crew
        If Ship Is Nothing Then Return total

        'get mascot bonus
        If Ship.mascot Is Nothing = False Then
            If Ship.Mascot.SkillBonuses.ContainsKey(cs) Then total += Ship.Mascot.SkillBonuses(cs)
        End If

        'get module cap
        Dim m As ShipModule = Nothing
        Select Case cs
            Case CrewSkill.Steering : m = Ship.GetModule(ShipModule.ModuleType.Helm)
            Case CrewSkill.Navigation : m = Ship.GetModule(ShipModule.ModuleType.Maproom)
            Case CrewSkill.Cooking : m = Ship.GetModule(ShipModule.ModuleType.Kitchen)
            Case CrewSkill.Medicine : m = Ship.GetModule(ShipModule.ModuleType.Apothecary)
            Case CrewSkill.Alchemy : m = Ship.GetModule(ShipModule.ModuleType.Laboratory)
            Case CrewSkill.Leadership : m = Ship.GetModule(ShipModule.ModuleType.Quarterdeck)
        End Select
        If m Is Nothing = False Then
            Dim mCap As Integer = m.Quality + 1
            total = Math.Min(mCap, total)
        End If

        'get leadership bonus
        If cs <> CrewSkill.Leadership Then
            Dim leadership As Integer = Ship.GetLeadership
            total += Math.Floor(leadership / 5)
        End If

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
    Public Function GetSkillFromRole() As Integer
        Dim cs As CrewSkill = ConvertRoleToSkill(Role)
        If cs = Nothing Then Return -1
        Return GetSkill(cs)
    End Function
    Public Sub AddSkillXP(ByVal cs As CrewSkill, ByVal value As Double)
        Dim maxLevel As Integer = SkillThresholds.Count - 1
        Dim maxThreshold As Integer = SkillThresholds(maxLevel)
        If Skills(cs) >= maxLevel Then Exit Sub

        SkillsXP(cs) += value
        If SkillsXP(cs) > maxThreshold Then SkillsXP(cs) = maxThreshold

        Dim level As Integer = Skills(cs)
        While SkillsXP(cs) > SkillThresholds(level)
            level += 1
            Skills(cs) += 1
        End While
    End Sub

    Public Shared Function ConvertSkillToRole(ByVal skill As CrewSkill) As CrewRole
        Select Case skill
            Case CrewSkill.Leadership : Return CrewRole.Captain
            Case CrewSkill.Cooking : Return CrewRole.Cook
            Case CrewSkill.Steering : Return CrewRole.Helmsman
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
            Case CrewRole.Helmsman : Return CrewSkill.Steering
            Case CrewRole.Sailor : Return CrewSkill.Sailing
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
    Public ReadOnly Property IsInjured As Boolean
        Get
            If DamageSustained > 0 Then Return True Else Return False
        End Get
    End Property

    Public Sub MeleeAttack(ByRef targets As List(Of Crew))
        Dim target As Crew = Dev.GrabRandom(targets, World.Rng)
        MeleeAttack(target)
    End Sub
    Public Sub MeleeAttack(ByRef target As Crew)
        Dim weapons As List(Of CrewBonus) = getWeapons()
        For Each weapon In weapons
            Dim skill As CrewSkill = weapon.Skill

            'roll skill vs skill
            Dim attSkill As Integer = GetSkill(skill) + weapon.Accuracy + Dev.FateRoll(World.Rng)
            Dim defSkill As Integer = target.GetSkill(skill) + weapon.Accuracy + Dev.FateRoll(World.Rng)
            If attSkill > defSkill Then
                'success, damage
                Dim damage As New Damage(0, weapon.Damage, weapon.DamageType, Name & "'s " & weapon.Name)
                target.Damage(damage)
                weapon.UseWeapon(ship)
            ElseIf attSkill = defSkill Then
                'glancing hit
                Dim dmgValue As Integer = Dev.Constrain(weapon.Damage / 2, 1)
                Dim damage As New Damage(0, dmgValue, weapon.DamageType, Name & "'s " & weapon.Name)
                target.Damage(damage)
                weapon.UseWeapon(Ship)
            Else
                'miss
                Report.Add("[" & target.Ship.ID & "] " & target.Name & " fended off " & Name & "'s " & weapon.Name & ".", ReportType.CrewAttack)
            End If

            'add xp
            Dim xp As Double = 0.5
            AddSkillXP(skill, xp)
            target.AddSkillXP(skill, xp)
        Next
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
    Public Sub TickCombat()
        For Each cbl In CrewBonuses
            For Each cb In cbl
                cb.TickCombat()
            Next
        Next
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
    Public Sub TickHeal(ByVal doctor As Crew)
        'doctors have disadvantage when treating patients not of their race
        'failure to treat will deal damage
        'unrelinquished do not gain scars but are harder to treat
        'seatouched have a chance to gain mutation instead of scar

        Dim currentDamage As Damage = GetWorstDamage()
        If currentDamage.CrewDamage = 0 Then
            'no more damage to treat in damage log
            'check for lingering injuries to treat
            If doctor Is Nothing = False AndAlso DamageSustained > 0 Then
                Dim heal As Integer = World.Rng.Next(1, 6)
                Report.Add(Name & "'s injuries recover under the ministrations of the doctor. (-" & heal & " damage)", ReportType.Doctor)
                DamageSustained -= heal
                If DamageSustained < 0 Then DamageSustained = 0
            End If
            Exit Sub
        End If

        If doctor Is Nothing Then
            Report.Add(Name & "'s injuries worsen without a doctor. (+5 damage)", ReportType.CrewDamage)
            DamageSustained += 5
            If DamageSustained > Health Then Death()
            Exit Sub
        End If

        Dim skill As Integer = doctor.GetSkillFromRole
        skill += Dev.FateRoll(World.Rng)
        If doctor.Race <> Me.Race Then skill -= 1
        If Me.Race = CrewRace.Unrelinquished Then skill -= 1

        Dim difficulty As Integer
        Select Case currentDamage.CrewDamage
            Case Is <= 10 : difficulty = -1
            Case Is <= 20 : difficulty = 0
            Case Is <= 30 : difficulty = 1
            Case Is <= 40 : difficulty = 2
            Case Else : difficulty = 3
        End Select

        If skill > difficulty Then
            Dim heal As Integer = currentDamage.CrewDamage / 2
            DamageLog.Remove(currentDamage)
            DamageSustained -= heal
            Report.Add("The doctor successfully treated " & Name & "'s worst injuries. (-" & heal & " damage)", ReportType.Doctor)
        ElseIf skill = difficulty Then
            Dim heal As Integer = currentDamage.CrewDamage / 2
            DamageLog.Remove(currentDamage)
            DamageSustained -= heal
            Report.Add("The doctor treated " & Name & "'s worst injuries with some difficulty. (-" & heal & " damage)", ReportType.Doctor)

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
            Dim dmg As Integer = World.Rng.Next(1, 6)
            If GetTalent(CrewTalent.Tough) = True Then dmg = 1
            Report.Add("The doctor failed to treat " & Name & "'s worst injuries. (+" & dmg & " damage)", ReportType.Doctor)
            DamageSustained += dmg
            If DamageSustained > Health Then Death()
        End If

    End Sub
    Private Function GetWorstDamage() As Damage
        Dim worstDmg As Damage = New Damage(0, 0, Nothing, "")
        For Each dmg In DamageLog
            If dmg.CrewDamage > worstDmg.CrewDamage Then worstDmg = dmg
        Next
        Return worstDmg
    End Function
    Private Sub Death()
        If Ship.BattleSquare Is Nothing = False AndAlso Ship.BattleSquare.Battlefield Is Nothing = False Then Ship.BattleSquare.Battlefield.AddDead(Me)
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
    Public MoraleDemands As New Dictionary(Of GoodType, Integer)
    Public MoraleWants As New Dictionary(Of GoodType, Integer)
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
    Public Sub TickMorale(Optional ByVal shoreProvisors As List(Of GoodType) = Nothing)
        'morale ranges from 1 to 100
        Dim change As Integer = 0
        Dim hasEaten As Boolean = False
        Dim hasDrunk As Boolean = False

        For Each gt In MoraleDemands.Keys
            Dim newChange As Integer = ConsumeGoods(gt, 1, 0, MoraleDemands(gt), shoreProvisors)
            If gt = GoodType.Rations AndAlso newChange >= 0 Then hasEaten = True
            If gt = GoodType.Water AndAlso newChange >= 0 Then hasDrunk = True
            change += newChange
        Next
        If hasDrunk = True OrElse Race = CrewRace.Unrelinquished Then
            For Each gt In MoraleWants.Keys
                Dim newChange As Integer = ConsumeGoods(gt, 1, MoraleWants(gt), 0, shoreProvisors)
                change += newChange
            Next
        End If

        'special cases
        Select Case Race
            Case CrewRace.Seatouched
                If shoreProvisors Is Nothing = False AndAlso shoreProvisors.Contains(GoodType.Salt) Then
                    change += 1
                Else
                    If Shrine Is Nothing Then change -= 5 : Exit Select
                    change += Math.Ceiling(Shrine.Quality / 2)
                End If
            Case CrewRace.Unrelinquished
                hasEaten = False
                hasDrunk = False
                If Ship.GetLeadership >= 7 Then change += 1
        End Select


        'other bonuses
        If change >= 0 Then
            If Quarters Is Nothing Then Throw New Exception("Crew has no quarters.")
            change += (Quarters.Quality - 2)
            change = Dev.Constrain(change, 0, 100)

            'cooking
            If hasEaten = True Then
                If shoreProvisors Is Nothing = False AndAlso shoreProvisors.Contains(GoodType.Rations) Then
                    'eating at shore
                    change += 1
                Else
                    Dim cook As Crew = Ship.GetCrew(Nothing, CrewRole.Cook)
                    If cook Is Nothing = False Then
                        change += Math.Ceiling(cook.GetSkillFromRole / 2)
                        Dim xp As Double = 0.25
                        cook.AddSkillXP(CrewSkill.Cooking, xp)
                    End If
                End If
            End If
        End If

        'apply
        Morale = Dev.Constrain(Morale + change, 0, 100)
        CType(Ship, ShipPlayer).MoraleChange += change
    End Sub
    Private Function ConsumeGoods(ByVal goodType As GoodType, ByVal qty As Integer, ByVal positiveChange As Integer, ByVal negativeChange As Integer, ByVal shoreProvisors As List(Of GoodType)) As Integer
        If Not (TypeOf Ship Is ShipPlayer) Then Return 0
        If shoreProvisors Is Nothing = False andalso shoreProvisors.Contains(goodType) Then Return positiveChange

        Dim ps As ShipPlayer = CType(Ship, ShipPlayer)
        Dim good As Good = good.Generate(goodType, -qty)

        'shortcircuit for Greenskin mutation
        If goodType = Pirates.GoodType.Rations AndAlso GetBonus("scar", "Greenskin") Is Nothing = False Then Return positiveChange

        If ps.CheckGoodsFreeForConsumption(goodType) = False OrElse Ship.CheckAddGood(good) = False Then
            Return negativeChange
        Else
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

        'add xp for specialist roles
        Select Case Role
            Case CrewRole.Captain : AddSkillXP(CrewSkill.Leadership, 1)
            Case CrewRole.FirstMate : AddSkillXP(CrewSkill.Leadership, 0.5)
            Case CrewRole.Navigator : AddSkillXP(CrewSkill.Navigation, 1)
            Case CrewRole.Sailor : AddSkillXP(CrewSkill.Sailing, 1)
            Case CrewRole.Helmsman : AddSkillXP(CrewSkill.Sailing, 1)
            Case CrewRole.Gunner 'handled in ship.shipattack
            Case CrewRole.Cook 'handled in crew.tickmorale
            Case CrewRole.Doctor 'handled in shipplayer.tick
        End Select
    End Sub
#End Region

#Region "Console"
    Public Sub ConsoleReport()
        Dim s As String = Dev.vbSpace(1)
        Dim ss As String = Dev.vbSpace(2)
        Dim t As Integer = 13

        Console.WriteLine(Name)
        Console.WriteLine(s & "Race:   " & Race.ToString)
        Console.WriteLine(s & "Health: " & Health - DamageSustained & "/" & Health)
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
