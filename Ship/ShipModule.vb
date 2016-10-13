Public Class ShipModule
    Public Name As String
    Public Type As ModuleType
    Public Capacity As Integer
    Public HullCost As Integer

    Public Sub New(ByVal aName As String, ByVal aType As ModuleType, ByVal aCapacity As Integer, ByVal aHullCost As Integer)
        Name = aName
        Type = aType
        Capacity = aCapacity
        HullCost = aHullCost
    End Sub

    Public Ship As Ship
    Public Quarter As ShipQuarter

    Public Enum ModuleType
        Crew = 1
    End Enum
End Class