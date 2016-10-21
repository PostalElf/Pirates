Public Class Route
    Private isle1 As Isle
    Private isle2 As Isle
    Private Quality As Integer

    Public Sub New(ByVal i1 As Isle, ByVal i2 As Isle, ByVal aQuality As Integer)
        isle1 = i1
        isle2 = i2
        Quality = aQuality
    End Sub
    Public Function Contains(ByVal isle As Isle) As Boolean
        If isle1 = isle OrElse isle2 = isle Then Return True
        Return False
    End Function
    Public Shared Operator =(ByVal r1 As Route, ByVal r2 As Route)
        If r1.isle1 <> r2.isle1 AndAlso r1.isle1 <> r2.isle2 Then Return False
        If r1.isle2 <> r2.isle1 AndAlso r1.isle2 <> r2.isle2 Then Return False
        Return True
    End Operator
    Public Shared Operator <>(ByVal r1 As Route, ByVal r2 As Route)
        If r1 = r2 Then Return False Else Return True
    End Operator
End Class
