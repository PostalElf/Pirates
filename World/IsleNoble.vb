Public Class IsleNoble
    Public Name As String
    Public Title As Rank
    Public Isle As Isle
    Public Overrides Function ToString() As String
        Return Title.ToString & " " & Name
    End Function

    Public Enum Rank
        Baron = 1
        Viscount
        Earl
        Marquis
        Duke
    End Enum
End Class
