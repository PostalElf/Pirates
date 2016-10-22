Public Class Route
    Private isle1 As Isle
    Private isle2 As Isle
    Private Quality As Integer

    Public Sub New(ByVal i1 As Isle, ByVal i2 As Isle, ByVal aQuality As Integer)
        isle1 = i1
        isle2 = i2
        Quality = aQuality
    End Sub
    Public Shared Operator =(ByVal r1 As Route, ByVal r2 As Route)
        If r1.isle1 <> r2.isle1 AndAlso r1.isle1 <> r2.isle2 Then Return False
        If r1.isle2 <> r2.isle1 AndAlso r1.isle2 <> r2.isle2 Then Return False
        Return True
    End Operator
    Public Shared Operator <>(ByVal r1 As Route, ByVal r2 As Route)
        If r1 = r2 Then Return False Else Return True
    End Operator
    Public Shared Operator -(ByVal route As Route, ByVal isle As Isle) As Isle
        If route.isle1 = isle Then Return route.isle2
        If route.isle2 = isle Then Return route.isle1
        Return Nothing
    End Operator
    Public Shared Operator >(ByVal i As Integer, ByVal route As Route) As Boolean
        If i > route.Quality Then Return True Else Return False
    End Operator
    Public Shared Operator <(ByVal i As Integer, ByVal route As Route) As Boolean
        If i < route.Quality Then Return True Else Return False
    End Operator
    Public Shared Operator +(ByVal route As Route, ByVal i As Integer) As Route
        Return New Route(route.isle1, route.isle2, route.Quality + i)
    End Operator

    Public Function Contains(ByVal isle As Isle) As Boolean
        If isle1 = isle OrElse isle2 = isle Then Return True
        Return False
    End Function
    Public Function GetDistance() As Double
        Dim modifier As Double
        Select Case Quality
            Case 0 : modifier = 2
            Case 1 : modifier = 1.75
            Case 2 : modifier = 1.5
            Case 3 : modifier = 1.25
            Case 4 : modifier = 1
            Case 5 : modifier = 0.75
            Case Else : Throw New Exception("Route quality out of range")
        End Select

        'pythogoras theorem
        Dim dx As Integer = Math.Abs(isle1.X - isle2.X)
        Dim dy As Integer = Math.Abs(isle1.Y - isle2.Y)
        Dim c As Double = Math.Sqrt((dx * dx) + (dy * dy))

        Dim total As Double = Math.Round(c * modifier, 2)
        Return total
    End Function
    Public Overrides Function ToString() As String
        Return isle1.Name & " - " & isle2.Name
    End Function
End Class
