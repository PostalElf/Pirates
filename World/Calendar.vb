Public Class Calendar
    Private _Day As CalendarDay
    Public Property Day As CalendarDay
        Get
            Return _Day
        End Get
        Set(ByVal value As CalendarDay)
            _Day = value
            While _Day > 7
                _Day -= 7
                If _Day <= 0 Then _Day = 1
                Week += 1
            End While
        End Set
    End Property
    Private _Week As Integer
    Public Property Week As Integer
        Get
            Return _Week
        End Get
        Set(ByVal value As Integer)
            _Week = value

            While _Week > 7
                _Week -= 7
                If _Week <= 0 Then _Week = 1
                Season += 1
            End While

        End Set
    End Property
    Private _Season As CalendarSeason
    Public Property Season As CalendarSeason
        Get
            Return _Season
        End Get
        Set(ByVal value As CalendarSeason)
            _Season = value

            While _Season > 9
                _Season -= 9
                If _Season < 0 Then _Season = 1
                Year += 1
            End While
        End Set
    End Property
    Public Property Year As Integer

    Public Sub New(ByVal aDay As CalendarDay, ByVal aWeek As Integer, ByVal aSeason As CalendarSeason, ByVal aYear As Integer)
        Day = aDay
        Week = aWeek
        Season = aSeason
        Year = aYear
    End Sub
    Public Overrides Function ToString() As String
        Return Day.ToString & "'s Day, Week " & Week & " of " & Season.ToString & " in " & Year
    End Function
    Public Sub Tick(Optional ByVal days As Integer = 1)
        Day += days
    End Sub

    Public Enum CalendarDay
        Baron = 1
        Viscount
        Earl
        Marquis
        Duke
        Queen
        King
    End Enum
    Public Enum CalendarSeason
        Secrets
        Silence
        Sail
        Salt
        Storms
        Shadow
        Scorn
        Sand
        Shore
    End Enum
End Class
