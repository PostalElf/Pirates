Public Class Menu
    Friend Shared Function getListChoice(ByVal objList As System.Collections.Generic.IEnumerable(Of Object), ByVal indent As Integer, Optional ByVal str As String = "", Optional ByVal prompt As String = "> ") As Object
        If objList.Count = 0 Then Return Nothing

        Dim ind As String = Dev.vbSpace(indent)
        If str <> "" Then
            Console.WriteLine(str)
        End If

        For n = 0 To objList.Count - 1
            Dim obj As Object = objList(n)
            Console.WriteLine(ind & n + 1 & ") " & obj.ToString)
        Next

        While True
            Console.WriteLine()
            Console.Write(ind & prompt)
            Dim input As String = Console.ReadLine

            If IsNumeric(input) = True Then
                Dim num As Integer = CInt(input) - 1
                If num > objList.Count OrElse num < -1 Then
                    Console.WriteLine("Invalid input!")
                ElseIf num = -1 Then
                    'escape clause when user input 0
                    Return Nothing
                Else
                    Return objList(num)
                End If
            Else
                Console.WriteLine("Invalid input!")
            End If
        End While

        Return Nothing
    End Function
    Friend Shared Function getListChoice(ByVal objList As Dictionary(Of Integer, String), ByVal indent As Integer, Optional ByVal str As String = "", Optional ByVal prompt As String = "> ") As Integer
        If objList.Count = 0 Then Return Nothing

        Dim ind As String = Dev.vbSpace(indent)
        If str <> "" Then Console.WriteLine(str)

        For Each kvp In objList
            Console.WriteLine(ind & kvp.Key & ") " & kvp.Value)
        Next

        While True
            Console.WriteLine()
            Console.Write(ind & prompt)
            Dim input As String = Console.ReadLine
            Dim inputNum As Integer = CInt(input)

            If objList.ContainsKey(inputNum) OrElse inputNum = 0 Then
                Return inputNum
            Else
                Console.WriteLine()
                Console.WriteLine("Invalid input!")
            End If
        End While

        Return Nothing
    End Function
    Friend Shared Function getListChoice(ByVal objList As Dictionary(Of Char, String), ByVal indent As Integer, Optional ByVal str As String = "", Optional ByVal prompt As String = "> ") As Char
        If objList.Count = 0 Then Return Nothing

        Dim ind As String = Dev.vbSpace(indent)
        If str <> "" Then Console.WriteLine(str)

        For Each kvp In objList
            Console.WriteLine(ind & kvp.Key & ") " & kvp.Value)
        Next

        While True
            Console.WriteLine()
            Console.Write(ind & prompt)
            Dim input As ConsoleKeyInfo = Console.ReadKey

            If objList.ContainsKey(input.KeyChar) OrElse objList.ContainsKey(Char.ToUpperInvariant(input.KeyChar)) _
                OrElse input.Key = ConsoleKey.Spacebar OrElse input.Key = ConsoleKey.Enter _
                Then
                Return Char.ToLowerInvariant(input.KeyChar)
            Else
                Console.WriteLine()
                Console.WriteLine("Invalid input!")
            End If
        End While

        Return Nothing
    End Function
    Friend Shared Function confirmChoice(ByVal indent As Integer, Optional ByVal str As String = "Are you sure? ") As Boolean
        Dim ind As String = Dev.vbSpace(indent)

        Console.Write(ind & str)
        Dim input As ConsoleKeyInfo = Console.ReadKey()
        If input.Key = ConsoleKey.Y Then Return True Else Return False
    End Function
    Friend Shared Function getNumInput(ByVal indent As Integer, ByVal min As Integer, ByVal max As Integer, ByVal str As String) As Integer
        Dim input As String = ""
        Dim ind As String = Dev.vbSpace(indent)

        While True
            Console.Write(ind & str)
            input = Console.ReadLine
            If IsNumeric(input) = True Then
                Dim num As Integer = CInt(input)
                If num >= min AndAlso num <= max Then Return num Else Console.WriteLine(ind & "Number must be between " & min & " and " & max & ".")
            Else
                Console.WriteLine(ind & "Numbers only please!")
            End If
        End While
        Return Nothing
    End Function
    Friend Shared Function getCharInput(ByVal objList As List(Of Char), ByVal str As String) As Char
        While True
            Console.Write(str)
            Dim input As ConsoleKeyInfo = Console.ReadKey

            If input.Key = ConsoleKey.Escape Then
                Return Nothing
            ElseIf objList.Contains(Char.ToLower(input.KeyChar)) OrElse objList.Contains(Char.ToUpper(input.KeyChar)) Then
                Return input.KeyChar
            Else
                Console.WriteLine()
                Console.WriteLine("Invalid input.")
            End If
        End While
        Return Nothing
    End Function
End Class