Public Structure ShipBuff
    Public Name As String
    Public Duration As Integer

    Public Sub New(ByVal aName As String, ByVal aDuration As Integer)
        Name = aName
        Duration = aDuration
    End Sub
    Public Shared Operator +(ByVal b As ShipBuff, ByVal v As Integer) As ShipBuff
        Return New ShipBuff(b.Name, b.Duration + v)
    End Operator
    Public Shared Operator -(ByVal b As ShipBuff, ByVal v As Integer) As ShipBuff
        Return New ShipBuff(b.Name, b.Duration - v)
    End Operator
    Public Shared Operator =(ByVal b1 As ShipBuff, ByVal b2 As ShipBuff) As Boolean
        If b1.Name = b2.Name Then Return True Else Return False
    End Operator
    Public Shared Operator <>(ByVal b1 As ShipBuff, ByVal b2 As ShipBuff) As Boolean
        If b1 = b2 Then Return False Else Return True
    End Operator
    Public Shared Operator =(ByVal b As ShipBuff, ByVal v As Integer) As Boolean
        If b.Duration = v Then Return True Else Return False
    End Operator
    Public Shared Operator <>(ByVal b As ShipBuff, ByVal v As Integer) As Boolean
        If b = v Then Return False Else Return True
    End Operator
End Structure
