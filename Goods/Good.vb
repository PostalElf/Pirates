Public Structure Good
    Public Type As GoodType
    Public Qty As Integer

    Public Sub New(ByVal aType As GoodType, ByVal aQty As Integer)
        Type = aType
        Qty = aQty
    End Sub
End Structure
