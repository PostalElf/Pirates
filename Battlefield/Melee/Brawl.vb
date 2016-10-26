Public Class Brawl
    Implements MeleeHost

    Public Property InMelee As Boolean = True Implements MeleeHost.InMelee
    Private Crews As New List(Of Crew)
    Public Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As List(Of Crew) Implements MeleeHost.GetCrews
        'ignore quarter and role
        Return Crews
    End Function

    Private Goods As New Dictionary(Of GoodType, Integer)
    Public Function CheckGoodsFreeForConsumption(ByVal gt As GoodType) As Boolean Implements MeleeHost.CheckGoodsFreeForConsumption
        Return True
    End Function
    Public Function CheckAddGood(ByVal gt As GoodType, ByVal qty As Integer) As Boolean Implements MeleeHost.CheckAddGood
        If qty < 0 AndAlso Goods(gt) + qty < 0 Then Return False
        Return True
    End Function
    Public Sub AddGood(ByVal gt As GoodType, ByVal qty As Integer) Implements MeleeHost.AddGood
        If Goods.ContainsKey(gt) = False Then Goods.Add(gt, 0)
        Goods(gt) += qty
    End Sub

    Public Shared Function Generate() As Brawl

    End Function
End Class
