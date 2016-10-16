Public Structure Good
    Public Type As GoodType
    Public Qty As Integer
    Public ReadOnly Property Weight As Double
        Get
            Select Case Type
                Case GoodType.Grapples : Return 0.1
                Case GoodType.Bullets : Return 0.01
                Case GoodType.Shot : Return 0.1
                Case GoodType.Explosive : Return 0.2
                Case GoodType.Grapeshot : Return 0.1

                Case GoodType.Coin : Return 0.001
                Case GoodType.Jewellery : Return 0.1
                Case Else : Return 0.1
            End Select
        End Get
    End Property
    Public ReadOnly Property TotalWeight As Double
        Get
            Return Qty * Weight
        End Get
    End Property
    Public ReadOnly Property Mass As Double
        Get
            Return 0.1
        End Get
    End Property
    Public ReadOnly Property TotalMass As Double
        Get
            Return Qty * Mass
        End Get
    End Property

    Public Sub New(ByVal aType As GoodType, ByVal aQty As Integer)
        Type = aType
        Qty = aQty
    End Sub
    Public Shared Operator +(ByVal g1 As Good, ByVal g2 As Good) As Good
        If g1.Type <> g2.Type Then Return g1
        Return New Good(g1.Type, g1.Qty + g2.Qty)
    End Operator
    Public Shared Function Generate(ByVal gt As GoodType, Optional ByVal aQty As Integer = 0) As Good
        Return New Good(gt, aQty)
    End Function
End Structure
