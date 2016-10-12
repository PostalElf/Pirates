Public Class Damage
    Public ShipDamage As Integer
    Public CrewDamage As Integer
    Public Type As DamageType
    Public Sender As String

    Public Sub New()
    End Sub
    Public Sub New(ByVal pShipDamage As Integer, ByVal pCrewDamage As Integer, ByVal pType As DamageType, ByVal pSender As String)
        ShipDamage = pShipDamage
        CrewDamage = pCrewDamage
        Type = pType
        Sender = pSender
    End Sub
    Public Shared Function Clone(ByRef shipDamage As Damage) As Damage
        Dim d As New Damage
        With shipDamage
            d.ShipDamage = .ShipDamage
            d.CrewDamage = .CrewDamage
            d.Type = .Type
            d.Sender = .Sender
        End With
        Return d
    End Function

    Public Shared Operator =(ByVal d1 As Damage, ByVal d2 As Damage)
        If d1.Type <> d2.Type Then Return False
        If d1.ShipDamage <> d2.ShipDamage Then Return False
        If d1.CrewDamage <> d2.CrewDamage Then Return False

        Return True
    End Operator
    Public Shared Operator <>(ByVal d1 As Damage, ByVal d2 As Damage)
        If d1 = d2 Then Return False Else Return True
    End Operator
End Class