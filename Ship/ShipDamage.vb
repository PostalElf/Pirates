Public Class ShipDamage
    Public Amt As Integer
    Public Type As DamageType
    Public Sender As String

    Public Sub New(ByVal pAmt As Integer, ByVal pType As DamageType, ByVal pSender As String)
        Amt = pAmt
        Type = pType
        Sender = pSender
    End Sub

    Public Shared Operator =(ByVal d1 As ShipDamage, ByVal d2 As ShipDamage)
        If d1.Type <> d2.Type Then Return False
        If d1.Amt <> d2.Amt Then Return False

        Return True
    End Operator
    Public Shared Operator <>(ByVal d1 As ShipDamage, ByVal d2 As ShipDamage)
        If d1 = d2 Then Return False Else Return True
    End Operator
End Class