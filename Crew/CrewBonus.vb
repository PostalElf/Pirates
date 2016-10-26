Public Class CrewBonus
    Public Name As String
    Public Slot As String
    Public Armour As New Dictionary(Of DamageType, Integer)
    Public SkillBonuses As New Dictionary(Of CrewSkill, Integer)

    Public Skill As CrewSkill = Nothing
    Public Damage As Integer = 0
    Public DamageType As DamageType = Nothing
    Public AmmoUse As Integer = 0
    Public Function GetAmmoType() As GoodType
        Select Case DamageType
            Case Pirates.DamageType.Firearms : Return GoodType.Bullets
            Case Else : Return Nothing
        End Select
    End Function
    Private CooldownCounter As Integer
    Private CooldownMax As Integer

    Public ReadOnly Property IsReady(ByVal host As MeleeHost) As Boolean
        Get
            If CooldownCounter > 0 Then Return False
            If AmmoUse > 0 Then
                Dim AmmoType As GoodType = GetAmmoType()
                If host.CheckGoodsFreeForConsumption(AmmoType) = False Then Return False
                If host.checkAddGood(AmmoType, -AmmoUse) = False Then Return False
            End If
            Return True
        End Get
    End Property
    Public Sub UseWeapon(ByRef host As MeleeHost)
        CooldownCounter = CooldownMax
        If AmmoUse > 0 Then
            host.AddGood(GetAmmoType, -AmmoUse)
        End If
    End Sub
    Public Sub TickCombat()
        If CooldownCounter > 0 Then CooldownCounter -= 1
    End Sub

    Public Shared Function Generate(ByVal aName As String) As CrewBonus
        Dim item As New CrewBonus
        With item
            Select Case aName
                Case "Belaying Pin"
                    .Name = aName
                    .Skill = CrewSkill.Melee
                    .Damage = 10
                    .DamageType = DamageType.Blunt
                    .AmmoUse = 0
                    .Slot = "Right Hand"

                Case "Bullwhip"
                    .Name = aName
                    .Skill = CrewSkill.Melee
                    .Damage = 15
                    .DamageType = Pirates.DamageType.Blunt
                    .AmmoUse = 0
                    .CooldownMax = 2
                    .Slot = "Left Hand"

                Case "Flintlock Pistol"
                    .Name = "Pistol"
                    .Damage = 25
                    .DamageType = DamageType.Firearms
                    .AmmoUse = 1
                    .CooldownMax = 3
                    .Slot = "Left Hand"

                Case Else
                    Return Nothing
            End Select
        End With
        Return item
    End Function
    Public Overrides Function ToString() As String
        Dim total As String = Name & " (" & Slot.ToString & ")"
        If Damage > 0 Then total &= " - " & Damage & " " & DamageType.ToString
        If AmmoUse > 0 Then total &= " - " & AmmoUse & " " & GetAmmoType().ToString
        Return total
    End Function
End Class