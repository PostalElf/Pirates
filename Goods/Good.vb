Public Structure Good
    Public Type As GoodType
    Public Qty As Integer
    Public ReadOnly Property Weight As Double
        Get
            Select Case Type
                Case GoodType.Grapples, GoodType.Grapeshot, GoodType.Shot : Return 0.1
                Case GoodType.Bullets : Return 0.01
                Case GoodType.Explosive : Return 0.2
                Case GoodType.Gold, GoodType.Silver : Return 0.001
                Case GoodType.Jewellery, GoodType.Cloth, GoodType.Lumber, GoodType.Metal : Return 0.1
                Case GoodType.Boricus, GoodType.Triaicus, GoodType.Incantus, GoodType.Mordicus : Return 0.01
                Case GoodType.Rations, GoodType.Water : Return 0.01
                Case GoodType.Salt, GoodType.Liqour, GoodType.Coffee, GoodType.Spice, GoodType.Tobacco : Return 0.01
                Case Else : Throw New Exception("Unrecognised goodtype")
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
            Select Case Type
                Case GoodType.Grapples, GoodType.Grapeshot, GoodType.Shot : Return 0.1
                Case GoodType.Bullets : Return 0.01
                Case GoodType.Explosive : Return 0.2
                Case GoodType.Gold, GoodType.Silver : Return 0.001
                Case GoodType.Jewellery, GoodType.Cloth, GoodType.Lumber, GoodType.Metal : Return 1
                Case GoodType.Boricus, GoodType.Triaicus, GoodType.Incantus, GoodType.Mordicus : Return 0.01
                Case GoodType.Rations, GoodType.Water : Return 0.01
                Case GoodType.Salt, GoodType.Liqour, GoodType.Coffee, GoodType.Spice, GoodType.Tobacco : Return 0.01
                Case Else : Throw New Exception("Unrecognised goodtype")
            End Select
        End Get
    End Property
    Public ReadOnly Property TotalMass As Double
        Get
            Return Qty * Mass
        End Get
    End Property
    Public Shared Function GetBasePrice(ByVal gt As GoodType) As Double
        Select Case gt
            Case GoodType.Grapples : Return 50
            Case GoodType.Shot : Return 50
            Case GoodType.Explosive : Return 85
            Case GoodType.Grapeshot : Return 70
            Case GoodType.Bullets : Return 5
            Case GoodType.Gold : Return 150
            Case GoodType.Silver : Return 100
            Case GoodType.Jewellery : Return 150
            Case GoodType.Cloth : Return 25
            Case GoodType.Lumber : Return 25
            Case GoodType.Metal : Return 25
            Case GoodType.Boricus : Return 100
            Case GoodType.Triaicus : Return 100
            Case GoodType.Incantus : Return 100
            Case GoodType.Mordicus : Return 100
            Case GoodType.Rations : Return 5
            Case GoodType.Water : Return 5
            Case GoodType.Salt : Return 35
            Case GoodType.Liqour : Return 15
            Case GoodType.Coffee : Return 15
            Case GoodType.Spice : Return 45
            Case GoodType.Tobacco : Return 10
            Case Else : Throw New Exception("Goodtype out of range")
        End Select
    End Function

    Private Sub New(ByVal aType As GoodType, ByVal aQty As Integer)
        Type = aType
        Qty = aQty
    End Sub
    Public Overrides Function ToString() As String
        Return Type.ToString & " x" & Qty
    End Function
    Public Shared Operator +(ByVal g1 As Good, ByVal g2 As Good) As Good
        If g1.Type <> g2.Type Then Return g1
        Return New Good(g1.Type, g1.Qty + g2.Qty)
    End Operator
    Public Shared Operator -(ByVal g1 As Good, ByVal g2 As Good) As Good
        If g1.Type <> g2.Type Then Return g1
        Return New Good(g1.Type, g1.Qty - g2.Qty)
    End Operator
    <DebuggerStepThrough()> Public Shared Function Generate(ByVal gt As GoodType, Optional ByVal aQty As Integer = 0) As Good
        Return New Good(gt, aQty)
    End Function
End Structure
