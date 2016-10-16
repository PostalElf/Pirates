Public Class ShipModule
    Public Name As String
    Public Type As ModuleType
    Public Capacity As Integer
    Public HullCost As Integer
    Public IsExclusive As Boolean

    Public Sub New(ByVal aName As String, ByVal aType As ModuleType, ByVal aCapacity As Integer, ByVal aHullCost As Integer, ByVal aIsExclusive As Boolean)
        Name = aName
        Type = aType
        Capacity = aCapacity
        HullCost = aHullCost
        IsExclusive = aIsExclusive
    End Sub

    Public Ship As Ship
    Public Quarter As ShipQuarter

    Public Enum ModuleType
        Crew = 1
        Officer
        Quarterdeck
        Helm
        Maproom
        Kitchen
        Laboratory
        Shrine
        Hold
    End Enum
End Class