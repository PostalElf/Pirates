Public Class Isle
    Public Name As String
    Public X As Integer
    Public XSector As Integer
    Public XSubSector As Integer
    Public Y As Integer
    Public YSector As Integer
    Public YSubSector As Integer

    Public Shared Function Generate(ByVal aName As String, ByVal xSector As Integer, ByVal ySector As Integer, ByRef free As World.MapData)
        Dim isle As New Isle
        With isle
            .Name = aName

            .XSector = xSector
            .YSector = ySector
            Dim subSector As World.MapDataPoint = free.Grab(xSector, ySector, World.Rng)
            .XSubSector = subSector.X
            .YSubSector = subSector.Y
            .X = ConvertSectorToRange(.XSector, .XSubSector).Roll(World.Rng)
            .Y = ConvertSectorToRange(.YSector, .YSubSector).Roll(World.Rng)
        End With
        Return isle
    End Function
    Private Shared Function ConvertSectorToRange(ByVal sector As Integer, ByVal subSector As Integer) As Range
        Dim max As Integer = sector * 300
        max -= (3 - subSector) * 100
        Dim min As Integer = max - 99
        Return New Range(min, max)
    End Function
    Public Overrides Function ToString() As String
        Return Name
    End Function
    Public Shared Operator =(ByVal i1 As Isle, ByVal i2 As Isle)
        Return i1.Equals(i2)
    End Operator
    Public Shared Operator <>(ByVal i1 As Isle, ByVal i2 As Isle)
        If i1 = i2 Then Return False Else Return True
    End Operator
End Class
