<DebuggerStepThrough()> Public Class Dev
    Public Shared Function Constrain(ByVal value As Integer, Optional ByVal min As Integer = 0, Optional ByVal max As Integer = 100) As Integer
        If value < min Then value = min
        If value > max Then value = max
        Return value
    End Function
    Private Shared Function Sign(ByVal value As Decimal) As String
        If value < 0 Then Return "-" Else Return "+"
    End Function
    Public Shared Function WithSign(ByVal value As Decimal, Optional ByVal prefix As String = "") As String
        Dim valueStr As String
        Select Case prefix
            Case "$" : valueStr = Math.Abs(value).ToString("N0")
            Case Else : valueStr = Math.Abs(value)
        End Select

        Return Sign(value) & prefix & valueStr
    End Function
    Public Shared Function RomanNumeral(ByVal n As Integer) As String
        If n = 0 Then Return 0
        ' there is no Roman symbol for 0, but we don't want to return an empty string

        Const r = "IVXLCDM" ' Roman symbols
        Dim i As Integer = Math.Abs(n)
        Dim s As String = ""

        For p As Integer = 1 To 5 Step 2
            Dim d As Integer = i Mod 10
            i = i \ 10
            Select Case d ' format a decimal digit
                Case 0 To 3 : s = s.PadLeft(d + Len(s), Mid(r, p, 1))
                Case 4 : s = Mid(r, p, 2) & s
                Case 5 To 8 : s = Mid(r, p + 1, 1) & s.PadLeft(d - 5 + Len(s), Mid(r, p, 1))
                Case 9 : s = Mid(r, p, 1) & Mid(r, p + 2, 1) & s
            End Select
        Next

        s = s.PadLeft(i + Len(s), "M") ' format thousands
        If n < 0 Then s = "-" & s ' insert sign if negative (non-standard)
        Return s
    End Function

    Public Shared Function vbTab(ByVal word As String, ByVal totalLength As Integer)
        Dim spaceCount As Integer = totalLength - word.Length
        Dim spaces As String = ""
        For n = 1 To spaceCount
            spaces &= " "
        Next
        Return word & spaces
    End Function
    Public Shared Function vbSpace(Optional ByVal indent As Integer = 1) As String
        Const space As String = "  "

        Dim total As String = ""
        For n = 1 To indent
            total &= space
        Next
        Return total
    End Function

    Public Shared Function ListWithCommas(ByVal inputList As List(Of String)) As String
        Dim total As String = ""
        For n = 0 To inputList.Count - 1
            total &= inputList(n)
            If n < inputList.Count - 1 Then total &= ", "
        Next
        Return total
    End Function
    Public Shared Function ParseCommaList(ByVal raw As String) As List(Of String)
        Dim r As String() = raw.Split(",")
        Dim total As New List(Of String)
        For Each entry In r
            total.Add(entry.Trim)
        Next
        Return total
    End Function
    Public Shared Function StringToEnum(ByVal raw As String, ByVal enumType As Type) As [Enum]
        Dim arr As Array = System.Enum.GetValues(enumType)
        For Each a In arr
            If a.ToString.ToLower = raw.ToLower Then Return a
        Next
        Return Nothing
    End Function

    Public Shared Function GetRandom(Of T)(ByRef targetList As List(Of T), Optional ByRef aRng As Random = Nothing) As T
        If aRng Is Nothing Then aRng = New Random
        If targetList.Count = 0 Then Return Nothing
        If targetList.Count = 1 Then Return targetList(0)

        Dim roll As Integer = aRng.Next(targetList.Count - 1)
        Return targetList(roll)
    End Function
    Public Shared Function GrabRandom(Of T)(ByRef targetList As List(Of T), Optional ByRef aRng As Random = Nothing) As T
        If aRng Is Nothing Then aRng = New Random
        If targetList.Count = 0 Then Return Nothing

        Dim roll As Integer = aRng.Next(targetList.Count - 1)
        GrabRandom = targetList(roll)
        targetList.RemoveAt(roll)
    End Function
    Public Shared Function FateRoll(ByRef rng As Random) As Integer
        'roll 4 fudge dice for bell-curved -4 to +4

        Dim total As Integer = 0
        For n = 1 To 4
            Dim roll As Integer = Rng.Next(1, 4)
            Select Case roll
                Case 1 ' do nothing
                Case 2 : total += 1
                Case 3 : total -= 1
            End Select
        Next
        Return total
    End Function
End Class
