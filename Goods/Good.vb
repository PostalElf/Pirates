Public Structure Good
    Public Type As GoodType
    Public Qty As Integer
    Public HullCost As Double
    Public ReadOnly Property TotalHullCost As Double
        Get
            Return Qty * HullCost
        End Get
    End Property

    Public Sub New(ByVal aType As GoodType, ByVal aQty As Integer, ByVal aHullCost As Double)
        Type = aType
        Qty = aQty
        HullCost = aHullCost
    End Sub
    Public Shared Operator +(ByVal g1 As Good, ByVal g2 As Good) As Good
        If g1.Type <> g2.Type Then Return g1
        Return New Good(g1.Type, g1.Qty + g2.Qty, g1.HullCost)
    End Operator
    Public Shared Function Generate(ByVal gt As GoodType, Optional ByVal aQty As Integer = 0) As Good
        Dim hc As Double
        Select Case gt
            Case GoodType.Grapplers : hc = 0.1
            Case GoodType.Bullets : hc = 0.01
            Case GoodType.Shot : hc = 0.1
            Case GoodType.Explosive : hc = 0.2
            Case GoodType.Grapeshot : hc = 0.1

            Case GoodType.Coin : hc = 0.001
            Case GoodType.Treasure : hc = 0.1
            Case Else : hc = 0.1
        End Select

        Return New Good(gt, aQty, hc)
    End Function
End Structure
