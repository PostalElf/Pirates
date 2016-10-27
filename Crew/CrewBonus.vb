Public Class CrewBonus
    Public Name As String
    Public Slot As String
    Public Armour As New Dictionary(Of DamageType, Integer)
    Public SkillBonuses As New Dictionary(Of CrewSkill, Integer)

    Public Skill As CrewSkill = Nothing
    Public Accuracy As Integer = 0
    Public Damage As Integer = 0
    Public DamageType As DamageType = Nothing
    Public AmmoUse As Integer = 0
    Private CooldownCounter As Integer
    Private CooldownMax As Integer

    Public ReadOnly Property IsReady(ByVal host As MeleeHost) As Boolean
        Get
            If CooldownCounter > 0 Then Return False
            If AmmoUse > 0 Then
                Dim AmmoType As GoodType = GoodType.Bullets
                If host.CheckGoodsFreeForConsumption(AmmoType) = False Then Return False
                If host.checkAddGood(AmmoType, -AmmoUse) = False Then Return False
            End If
            Return True
        End Get
    End Property
    Public Sub UseWeapon(ByRef host As MeleeHost)
        CooldownCounter = CooldownMax
        If AmmoUse > 0 Then
            host.AddGood(GoodType.Bullets, -AmmoUse)
        End If
    End Sub
    Public Sub TickCombat()
        If CooldownCounter > 0 Then CooldownCounter -= 1
    End Sub

    Public Shared Function Generate(ByVal aName As String) As CrewBonus
        Dim item As New CrewBonus
        With item
            .Name = aName
            Select Case aName
                Case "Belaying Pin"
                    .Skill = CrewSkill.Melee
                    .Damage = 10
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 0
                    .Slot = "Right Hand"

                Case "Cudgel"
                    .Skill = CrewSkill.Melee
                    .Damage = 20
                    .DamageType = Pirates.DamageType.Blunt
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Right Hand"

                Case "Mace"
                    .Skill = CrewSkill.Melee
                    .Damage = 30
                    .DamageType = Pirates.DamageType.Blunt
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Right Hand"

                Case "Skullcracker"
                    .Skill = CrewSkill.Melee
                    .Damage = 40
                    .DamageType = Pirates.DamageType.Blunt
                    .AmmoUse = 0
                    .Accuracy = -1
                    .CooldownMax = 3
                    .Slot = "Right Hand"

                Case "Cutlass"
                    .Skill = CrewSkill.Melee
                    .Damage = 30
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Right Hand"

                Case "Rapier"
                    .Skill = CrewSkill.Melee
                    .Damage = 25
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .Accuracy = 1
                    .CooldownMax = 2
                    .Slot = "Right Hand"

                Case "Sabre"
                    .Skill = CrewSkill.Melee
                    .Damage = 35
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Right Hand"

                Case "Boarding Axe"
                    .Skill = CrewSkill.Melee
                    .Damage = 45
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .Accuracy = -1
                    .CooldownMax = 3
                    .Slot = "Right Hand"

                Case "Brass Knuckles"
                    .Skill = CrewSkill.Melee
                    .Damage = 5
                    .DamageType = Pirates.DamageType.Blunt
                    .Accuracy = 1
                    .AmmoUse = 0
                    .Slot = "Left Hand"

                Case "Knife"
                    .Skill = CrewSkill.Melee
                    .Damage = 10
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .Slot = "Left Hand"

                Case "Bullwhip"
                    .Skill = CrewSkill.Melee
                    .Damage = 15
                    .DamageType = Pirates.DamageType.Blunt
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Left Hand"

                Case "Long Knife"
                    .Skill = CrewSkill.Melee
                    .Damage = 15
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .Accuracy = 1
                    .Slot = "Left Hand"

                Case "Small Sword"
                    .Skill = CrewSkill.Melee
                    .Damage = 20
                    .DamageType = Pirates.DamageType.Blade
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Left Hand"

                Case "Writhing Knife"
                    .Skill = CrewSkill.Melee
                    .Damage = 25
                    .DamageType = Pirates.DamageType.Blade
                    .Accuracy = -1
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Left Hand"

                Case "Flintlock Pistol"
                    .Damage = 25
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 1
                    .CooldownMax = 5
                    .Slot = "Left Hand"

                Case "Wheellock Pistol"
                    .Damage = 35
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 1
                    .CooldownMax = 5
                    .Slot = "Left Hand"

                Case "Sparklock Pistol"
                    .Damage = 40
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 2
                    .Accuracy = 1
                    .CooldownMax = 5
                    .Slot = "Left Hand"

                Case "Musket"
                    .Damage = 40
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 2
                    .CooldownMax = 5
                    .Slot = "Right Hand"

                Case "Rifle"
                    .Damage = 40
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 2
                    .Accuracy = 1
                    .CooldownMax = 5
                    .Slot = "Right Hand"

                Case "Blunderbuss"
                    .Damage = 55
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 3
                    .Accuracy = -1
                    .CooldownMax = 5
                    .Slot = "Right Hand"

                Case Else
                    Return Nothing
            End Select
        End With
        Return item
    End Function
    Public Overrides Function ToString() As String
        Dim total As String = Name & " (" & Slot.ToString & ")"
        If Damage > 0 Then total &= " - " & Damage & " " & DamageType.ToString
        If AmmoUse > 0 Then total &= " - " & AmmoUse & " bullets"
        Return total
    End Function
End Class