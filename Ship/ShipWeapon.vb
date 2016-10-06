Public Class ShipWeapon
    Public Name As String
    Public ShipDamage As ShipDamage
    Public Range As Integer

    Public Sub Cooldown(ByVal value As Integer)
        CooldownCounter -= value
    End Sub
    Public ReadOnly Property IsReady As Boolean
        Get
            If CooldownCounter <= 0 Then Return True Else Return False
        End Get
    End Property
    Public CooldownCounter As Integer
    Public CooldownMax As Integer

    Public Sub New(ByVal aName As String, ByVal aShipDamage As ShipDamage, ByVal aRange As Integer)
        Name = aName
        ShipDamage = aShipDamage
        Range = aRange
    End Sub
    Public Sub New(ByVal aName As String, ByVal dAmt As Integer, ByVal dType As DamageType, ByVal aRange As Integer)
        Name = aName
        Range = aRange

        Dim damage As New ShipDamage(dAmt, dType, Name)
        ShipDamage = damage
    End Sub
End Class
