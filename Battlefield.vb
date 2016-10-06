Public Class Battlefield
    Private _MaxX As Byte
    Private _MaxY As Byte
    Public ReadOnly Property MaxX As Byte
        Get
            Return _MaxX
        End Get
    End Property
    Public ReadOnly Property MaxY As Byte
        Get
            Return _MaxY
        End Get
    End Property
    Private Squares As Battlesquare(,)
    Default Public Property Square(ByVal x As Byte, ByVal y As Byte) As Battlesquare
        Get
            If CheckBounds(x, y) = False Then Return Nothing
            Return Squares(x, y)
        End Get
        Set(ByVal value As Battlesquare)
            If CheckBounds(x, y) = False Then Exit Property
            Squares(x, y) = value
        End Set
    End Property
    Public Function RandomSquare(ByVal isEmpty As Boolean, Optional ByRef rng As Random = Nothing) As Battlesquare
        If rng Is Nothing Then rng = New Random
        If isEmpty = False Then
            Dim X As Byte = rng.Next(0, MaxX + 1)
            Dim Y As Byte = rng.Next(0, MaxY + 1)
            Return Square(X, Y)
        Else
            Dim sl As List(Of Battlesquare) = EmptySquares
            Dim roll As Byte = rng.Next(0, sl.Count)
            Return sl(roll)
        End If
    End Function
    Private ReadOnly Property EmptySquares As List(Of Battlesquare)
        Get
            Dim total As New List(Of Battlesquare)
            For x = 0 To MaxX
                For y = 0 To MaxY
                    Dim s As Battlesquare = Square(x, y)
                    If s.Contents Is Nothing Then total.Add(s)
                Next
            Next
            Return total
        End Get
    End Property

    Public Sub New(ByVal X As Byte, ByVal Y As Byte)
        _MaxX = X - 1
        _MaxY = Y - 1
        ReDim Squares(MaxX, MaxY)
        For nX = 0 To MaxX
            For nY = 0 To MaxY
                Squares(nX, nY) = New Battlesquare(Me, nX, nY)
            Next
        Next
    End Sub
    Public Shared Function Generate(ByVal X As Byte, ByVal Y As Byte, ByVal featureDensity As Byte) As Battlefield
        Dim battlefield As New Battlefield(X, Y)
        For n = 0 To featureDensity - 1
            Dim s As Battlesquare = battlefield.RandomSquare(True)
            s.GenerateFeature()
        Next
        Return battlefield
    End Function
    Private Function CheckBounds(ByVal x As Byte, ByVal y As Byte) As Boolean
        If x > MaxX OrElse x < 0 Then Return False
        If y > MaxY OrElse y < 0 Then Return False
        Return True
    End Function

    Public Sub ConsoleWrite()
        For y = 0 To MaxY
            For x = 0 To MaxX
                Dim s As Battlesquare = Square(x, y)
                s.ConsoleWrite()
                Console.Write(" ")
            Next
            Console.WriteLine()
        Next
    End Sub
End Class

Public Class Battlesquare
    Private Battlefield As Battlefield
    Private X As Byte
    Private Y As Byte
    Public Contents As BattlefieldObject
    Private Function GetAdjacent(ByVal direction As BattleDirection, ByVal distance As Byte) As Battlesquare
        Dim pX As Byte
        Dim pY As Byte

        Select Case direction
            Case BattleDirection.North
                pX = X
                pY = Y - distance
            Case BattleDirection.South
                pX = X
                pY = Y + distance
            Case BattleDirection.East
                pX = X + distance
                pY = Y
            Case BattleDirection.West
                pX = X - distance
                pY = Y
            Case Else
                Throw New Exception("Invalid direction")
                Return Nothing
        End Select

        pX = Dev.Constrain(pX, 0, Battlefield.MaxX)
        pY = Dev.Constrain(pY, 0, Battlefield.MaxY)
        Return Battlefield(pX, pY)
    End Function

    Public Sub New(ByRef field As Battlefield, ByVal pX As Byte, ByVal pY As Byte)
        Battlefield = field
        X = pX
        Y = pY
    End Sub
    Public Sub GenerateFeature()

    End Sub

    Public Sub ConsoleWrite()
        If Contents Is Nothing Then
            Console.Write("~")
            Exit Sub
        End If

        Select Case Contents.GetType
            'TODO
        End Select
    End Sub
End Class

Public Class BattlefieldObject

End Class

Public Enum BattleDirection
    North = 1
    East
    South
    West
End Enum
