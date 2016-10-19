Public Class CrewStation
    Public ShipQuarter As ShipQuarter
    Public Role As CrewRole

    Public Sub New()
    End Sub
    Public Sub New(ByVal aShipQuarter As ShipQuarter, ByVal aRole As CrewRole)
        ShipQuarter = aShipQuarter
        Role = aRole
    End Sub
End Class
