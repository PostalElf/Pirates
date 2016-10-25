Public Structure Range
    Public Min As Integer
    Public Max As Integer

    Public Sub New(ByVal aMin As Integer, ByVal aMax As Integer)
        Min = aMin
        Max = aMax
    End Sub
    Public Function Roll(ByRef rng As Random) As Integer
        Return rng.Next(Min, Max + 1)
    End Function
    Public Shared Operator *(ByVal r As Range, ByVal value As Integer) As Range
        Return New Range(r.Min * value, r.Max * value)
    End Operator
    Public Shared Operator =(ByVal r As Range, ByVal value As Integer) As Boolean
        If value = 0 Then
            If r.Min = 0 AndAlso r.Max = 0 Then Return True Else Return False
        Else
            If r.Min = value OrElse r.Max = value Then Return True Else Return False
        End If
    End Operator
    Public Shared Operator <>(ByVal r As Range, ByVal value As Integer) As Boolean
        If r = value Then Return False Else Return True
    End Operator
End Structure