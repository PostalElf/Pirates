Imports System.IO

<DebuggerStepThrough()> Public Class IO
    Public Shared Function BracketFilegetAll(ByVal pathname As String) As List(Of Queue(Of String))
        Dim total As New List(Of Queue(Of String))
        Try
            Dim line As String
            Using sr As New StreamReader(pathname)
                Dim current As New Queue(Of String)
                While sr.Peek <> -1
                    line = sr.ReadLine

                    'ignore blank lines and lines that start with -
                    If line = "" Then Continue While
                    If line.StartsWith("-") Then Continue While

                    If line.StartsWith("[") Then
                        'remove brackets
                        line = line.Remove(0, 1)
                        line = line.Remove(line.Length - 1, 1)

                        'if current is filled, add to total
                        If current.Count > 0 Then total.Add(current)

                        'start new current with bracketstring as header
                        current = New Queue(Of String)
                        current.Enqueue(line)
                    Else
                        If line <> "" Then current.Enqueue(line)
                    End If
                End While

                'add last entry
                If current.Count > 0 Then total.Add(current)
            End Using
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return Nothing
        End Try
        Return total
    End Function
    Public Shared Function BracketFileget(ByVal pathname As String, ByVal targetName As String) As Queue(Of String)
        Dim total As New Queue(Of String)
        Try
            Using sr As New StreamReader(pathname)
                Dim matchFound As Boolean = False
                While True
                    If sr.Peek = -1 Then Exit While
                    Dim line As String = sr.ReadLine

                    'ignore blank lines and lines that start with -
                    If line = "" Then Continue While
                    If line.StartsWith("-") Then Continue While

                    If line.StartsWith("[") Then
                        'if already writing, end at next bracket
                        If matchFound = True Then Exit While

                        'remove brackets
                        line = line.Remove(0, 1)
                        line = line.Remove(line.Length - 1, 1)

                        'if match found, flag boolean to begin writing
                        If line = targetName Then matchFound = True
                    End If

                    If matchFound = True Then total.Enqueue(line)
                End While
            End Using
            If total.Count = 0 Then Return Nothing Else Return total
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return Nothing
        End Try
    End Function
    Public Shared Sub BracketFilesave(ByVal pathname As String, ByVal raw As Queue(Of String))
        Dim matchFound As Boolean = False
        Dim bracketName As String = raw.Peek
        Dim all As List(Of Queue(Of String)) = BracketFilegetAll(pathname)

        'replace existing data with raw if same name is found
        If all Is Nothing = False Then
            For n = 0 To all.Count - 1
                Dim q As Queue(Of String) = all(n)
                If q.Peek = bracketName Then
                    all(n) = raw
                    matchFound = True
                    Exit For
                End If
            Next
        Else
            all = New List(Of Queue(Of String))
            matchFound = False
        End If

        'if match not found, add raw to all
        If matchFound = False Then all.Add(raw)

        'write all
        Try
            Using sw As New StreamWriter(pathname, False)
                For Each q As Queue(Of String) In all
                    sw.WriteLine("[" & q.Dequeue & "]")
                    While q.Count > 0
                        sw.WriteLine(q.Dequeue)
                    End While
                    sw.WriteLine()
                    sw.WriteLine()
                Next
            End Using
        Catch ex As Exception
            MsgBox(ex.ToString)
            Exit Sub
        End Try
    End Sub

    Public Shared Function SimpleFilegetAll(ByVal pathname As String) As List(Of String)
        Dim total As New List(Of String)
        Try
            Using sr As New StreamReader(pathname)
                Dim line As String = ""
                While sr.Peek <> -1
                    line = sr.ReadLine
                    If line <> "" Then total.Add(line)
                End While
            End Using
            Return total
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return Nothing
        End Try
    End Function
End Class
