Public Class Damage
    Public Amt As Integer
    Public Type As DamageType
    Public Sender As String

    Public Sub New()
    End Sub
    Public Sub New(ByVal pAmt As Integer, ByVal pType As DamageType, ByVal pSender As String)
        Amt = pAmt
        Type = pType
        Sender = pSender
    End Sub
    Public Shared Function Clone(ByRef shipDamage As Damage) As Damage
        Dim d As New Damage
        With shipDamage
            d.Amt = .Amt
            d.Type = .Type
            d.Sender = .Sender
        End With
        Return d
    End Function

    Public Shared Operator =(ByVal d1 As Damage, ByVal d2 As Damage)
        If d1.Type <> d2.Type Then Return False
        If d1.Amt <> d2.Amt Then Return False

        Return True
    End Operator
    Public Shared Operator <>(ByVal d1 As Damage, ByVal d2 As Damage)
        If d1 = d2 Then Return False Else Return True
    End Operator
End Class