Public Class GoodCrate
    Public Name As String
    Public GoodType As GoodType
    Public Capacity As Integer
    Public HullCost As Integer

    Public Sub New()
    End Sub
    Public Sub New(ByVal aName As String, ByVal aGoodType As GoodType, ByVal aCapacity As Integer, ByVal aHullCost As Integer)
        Name = aName
        GoodType = aGoodType
        Capacity = aCapacity
        HullCost = aHullCost
    End Sub
    Public Function Clone() As GoodCrate
        Dim c As New GoodCrate
        c.Name = Name
        c.GoodType = GoodType
        c.Capacity = Capacity
        c.HullCost = HullCost
        Return c
    End Function
End Class
