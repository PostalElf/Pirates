Public Class CrewBonus
    Public Name As String
    Public Slot As String
    Public Armour As New Dictionary(Of DamageType, Integer)
    Public SkillBonuses As New Dictionary(Of CrewSkill, Integer)

    Public Skill As CrewSkill = Nothing
    Public Damage As Integer = 0
    Public DamageType As DamageType = Nothing
    Public ReadOnly Property IsReady(ByVal ship As Ship) As Boolean
        Get
            If AmmoUse > 0 Then
                Dim AmmoType As GoodType = GetAmmoType()
                If ship.GoodsFreeForConsumption(AmmoType) = False Then Return False
                If ship.GetGood(AmmoType).Qty < AmmoUse Then Return False
            End If
            Return True
        End Get
    End Property

    Public AmmoUse As Integer = 0
    Public Function GetAmmoType() As GoodType
        Select Case DamageType
            Case Pirates.DamageType.Firearms : Return GoodType.Bullets
            Case Else : Return Nothing
        End Select
    End Function

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