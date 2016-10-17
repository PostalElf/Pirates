Public Class CrewBonus
    Public Name As String
    Public Slot As String
    Public SkillBonuses As New Dictionary(Of CrewSkill, Integer)

    Public Skill As CrewSkill
    Public Damage As Integer
    Public DamageType As DamageType
    Public ReadOnly Property IsReady(ByVal ship As Ship) As Boolean
        Get
            If AmmoUse > 0 Then
                Dim AmmoType As GoodType = GetAmmoType()
                If ship.GoodsFreeForConsumption(AmmoType) = False Then Return False
                If ship.GetGood(AmmoType) < AmmoUse Then Return False
            End If
            Return True
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