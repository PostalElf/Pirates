Public Class Isle
    Public Name As String
    Public X As Integer
    Public XSector As Integer
    Public XSubSector As Integer
    Public Y As Integer
    Public YSector As Integer
    Public YSubSector As Integer

    Public Shared Function Generate(ByVal aName As String, ByVal xSector As Integer, ByVal ySector As Integer, ByRef free As MapData)
        Dim isle As New Isle
        With isle
            .Name = aName

            .XSector = xSector
            .YSector = ySector
            Dim subSector As MapDataPoint = free.Grab(xSector, ySector, World.Rng)
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
        Return Name & " (" & XSector & "-" & XSubSector & ", " & YSector & "-" & YSubSector & ")"
    End Function
    Public Shared Operator =(ByVal i1 As Isle, ByVal i2 As Isle)
        Return i1.Equals(i2)
    End Operator
    Public Shared Operator <>(ByVal i1 As Isle, ByVal i2 As Isle)
        If i1 = i2 Then Return False Else Return True
    End Operator

    Public Class MapData
        Private Points As New Dictionary(Of MapDataPoint, List(Of MapDataPoint))
        Default Public Property Item(ByVal x As Integer, ByVal y As Integer) As List(Of MapDataPoint)
            Get
                Dim key As New MapDataPoint(x, y)
                Return Points(key)
            End Get
            Set(ByVal value As List(Of MapDataPoint))
                Points(New MapDataPoint(x, y)) = value
            End Set
        End Property
        Public Sub New(ByVal maxX As Integer, ByVal maxY As Integer, ByVal maxSubsectorX As Integer, ByVal maxSubsectorY As Integer)
            For aX = 1 To maxX
                For aY = 1 To maxY
                    Dim key As New MapDataPoint(aX, aY)
                    Points.Add(key, New List(Of MapDataPoint))
                    For pX = 1 To maxSubsectorX
                        For pY = 1 To maxSubsectorY
                            Points(key).Add(New MapDataPoint(pX, pY))
                        Next
                    Next
                Next
            Next
        End Sub
        Public Sub Add(ByVal sector As MapDataPoint, ByVal subsector As MapDataPoint)
            Points(sector).Add(subsector)
        End Sub
        Public Sub Remove(ByVal x As Integer, ByVal y As Integer)
            Dim selected As MapDataPoint = Nothing
            For Each plist In Points.Values
                For Each p In plist
                    If p.X = x AndAlso p.Y = y Then selected = p
                Next
                If selected.X > 0 Then
                    plist.Remove(selected)
                    Exit Sub
                End If
            Next
        End Sub
        Public Function Roll(ByVal sectorX As Integer, ByVal sectorY As Integer, ByRef rng As Random) As MapDataPoint
            Dim plist As List(Of MapDataPoint) = Points(New MapDataPoint(sectorX, sectorY))
            Return Dev.GrabRandom(Of MapDataPoint)(plist, rng)
        End Function
        Public Function Grab(ByVal sectorX As Integer, ByVal sectorY As Integer, ByRef rng As Random) As MapDataPoint
            Dim plist As List(Of MapDataPoint) = Points(New MapDataPoint(sectorX, sectorY))
            Return Dev.GrabRandom(Of MapDataPoint)(plist, rng)
        End Function
    End Class
    Public Structure MapDataPoint
        Public X As Integer
        Public Y As Integer
        Public Sub New(ByVal aX As Integer, ByVal aY As Integer)
            X = aX
            Y = aY
        End Sub
        Public Overrides Function ToString() As String
            Return "(" & X & ", " & Y & ")"
        End Function
        Public Shared Operator =(ByVal p1 As MapDataPoint, ByVal p2 As MapDataPoint)
            If p1.X = p2.X AndAlso p1.Y = p2.Y Then Return True Else Return False
        End Operator
        Public Shared Operator <>(ByVal p1 As MapDataPoint, ByVal p2 As MapDataPoint)
            If p1 = p2 Then Return False Else Return True
        End Operator
    End Structure
End Class
