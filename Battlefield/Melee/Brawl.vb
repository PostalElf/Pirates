Public Class Brawl
    Implements MeleeHost

    Public Property InMelee As Boolean = True Implements MeleeHost.InMelee
    Private Crews As List(Of Crew) = Nothing
    Public Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As List(Of Crew) Implements MeleeHost.GetCrews
        'ignore quarter and role
        Return Crews
    End Function

    Private Ship As Ship
    Private Goods As New Dictionary(Of GoodType, Integer)
    Public Function CheckGoodsFreeForConsumption(ByVal gt As GoodType) As Boolean Implements MeleeHost.CheckGoodsFreeForConsumption
        If Ship Is Nothing = False Then Return Ship.CheckGoodsFreeForConsumption(gt)
        Return True
    End Function
    Public Function CheckAddGood(ByVal gt As GoodType, ByVal qty As Integer) As Boolean Implements MeleeHost.CheckAddGood
        If Ship Is Nothing = False Then Return Ship.CheckAddGood(gt, qty)

        If qty < 0 AndAlso Goods(gt) + qty < 0 Then Return False
        Return True
    End Function
    Public Sub AddGood(ByVal gt As GoodType, ByVal qty As Integer) Implements MeleeHost.AddGood
        If Ship Is Nothing = False Then Ship.AddGood(gt, qty) : Exit Sub
        If Goods.ContainsKey(gt) = False Then Goods.Add(gt, 0)
        Goods(gt) += qty
    End Sub

    Public Sub New(ByVal crewlist As List(Of Crew), ByVal aShip As Ship)
        Crews = crewlist
        Ship = aShip
    End Sub
    Public Sub New(ByVal crewlist As List(Of Crew), ByVal aGoods As Dictionary(Of GoodType, Integer))
        Crews = crewlist
        Goods = aGoods
    End Sub
    Public Shared Function Generate(ByVal difficulty As Integer) As Brawl
        'difficulty 1 to 10
        Dim crewlist As New List(Of Crew)
        Select Case difficulty

        End Select
    End Function
End Class
