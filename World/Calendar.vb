Public Class Calendar
    Private _Day As Integer
    Public Property Day As Integer
        Get
            Return _Day
        End Get
        Set(ByVal value As Integer)
            _Day = value
            While _Day > 7
                _Day -= 7
                Week += 1
            End While
            If _Day <= 0 Then _Day = 1
        End Set
    End Property
    Private _Week As Integer
    Public Property Week As Integer
        Get
            Return _Week
        End Get
        Set(ByVal value As Integer)
            _Week = value
            While Week > 4
                _Week -= 4
                Month += 1
            End While
            If _Week <= 0 Then _Week = 1
        End Set
    End Property
    Private _Month As Integer
    Public Property Month As Integer
        Get
            Return _Month
        End Get
        Set(ByVal value As Integer)
            _Month = value
            While _Month > 13
                _Month -= 13
                Year += 1
            End While
            If _Month <= 0 Then _Month = 1
        End Set
    End Property
    Public ReadOnly Property Season As CalendarSeason
        Get
            Select Case Month
                Case 1, 2, 3 : Return CalendarSeason.Spring
                Case 4, 5, 6 : Return CalendarSeason.Summer
                Case 7, 8, 9 : Return CalendarSeason.Autumn
                Case 10, 11, 12 : Return CalendarSeason.Winter
                Case Else : Throw New Exception("Invalid month.")
            End Select
        End Get
    End Property
    Public Property Year As Integer

    Public Sub New(ByVal aDay As Integer, ByVal aWeek As Integer, ByVal aMonth As Integer, ByVal aYear As Integer)
        Day = aDay
        Week = aWeek
        Month = aMonth
        Year = aYear
    End Sub

    Public Enum CalendarSeason
        Spring = 1
        Summer
        Autumn
        Winter
    End Enum
End Class
