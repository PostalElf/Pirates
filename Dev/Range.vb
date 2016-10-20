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
End Structure
