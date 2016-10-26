Public Interface MeleeHost
    Property InMelee As Boolean
    Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As List(Of Crew)
    Function CheckGoodsFreeForConsumption(ByVal gt As GoodType) As Boolean
    Function CheckAddGood(ByVal gt As GoodType, ByVal value As Integer) As Boolean
    Sub AddGood(ByVal gt As GoodType, ByVal value As Integer)
End Interface
