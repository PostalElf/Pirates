Public Class CrewStation
    Public Ship As Ship
    Public ShipQuarter As ShipQuarter
    Public Role As CrewRole

    Public Sub New()
    End Sub
    Public Sub New(ByVal aShip As Ship, ByVal aShipQuarter As ShipQuarter, ByVal aRole As CrewRole)
        Ship = aShip
        ShipQuarter = aShipQuarter
        Role = aRole
    End Sub
End Class
