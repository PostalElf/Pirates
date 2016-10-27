Public Class Isle
    Public Name As String
    Public X As Integer
    Public XSector As Integer
    Public XSubSector As Integer
    Public Y As Integer
    Public YSector As Integer
    Public YSubSector As Integer
    Public Race As CrewRace
    Private World As World

#Region "Politics"
    Public Faction As WorldFaction
    Private Reputation As New Dictionary(Of IsleFaction, Integer)
    Private ReputationXP As New Dictionary(Of IsleFaction, Double)
    Private Shared ReputationThresholds As Double() = {0, 10, 20, 50, 100, 150, 200, 400, 600, 1000, 1500}
    Public Sub AddReputationXP(ByVal fac As IsleFaction, ByVal value As Double)
        ReputationXP(fac) += value

        Dim maxLevel As Integer = ReputationThresholds.Count - 1
        Dim maxThreshold As Integer = ReputationThresholds(maxLevel)
        If ReputationXP(fac) > maxThreshold Then ReputationXP(fac) = maxThreshold
        If ReputationXP(fac) < 0 Then ReputationXP(fac) = 0

        Dim level As Integer = Reputation(fac)
        While ReputationXP(fac) > ReputationThresholds(level)
            level += 1
            Reputation(fac) += 1
        End While
        If level > 0 Then
            While ReputationXP(fac) < ReputationThresholds(level)
                level -= 1
                Reputation(fac) -= 1
            End While
        End If
    End Sub
#End Region

#Region "Sale Goods"
    Private SaleGoodDemand As New Dictionary(Of GoodType, SaleDemand)
    Private SaleGoodProduction As New Dictionary(Of GoodType, SaleDemand)
    Private SaleGoodQty As New Dictionary(Of GoodType, Integer)
    Private ReadOnly Property SaleGoodQtyMax(ByVal gt As GoodType) As Integer
        Get
            Return GetSaleGoodProductionRange(gt).Max * 5
        End Get
    End Property
    Private ReadOnly Property SaleGoodQtyMin(ByVal gt As GoodType) As Integer
        Get
            Return GetSaleGoodProductionRange(gt).Min * 2
        End Get
    End Property
    Private SaleGoodPriceModifier As New Dictionary(Of GoodType, Double)
    Private Function GetSaleGoodProductionRange(ByVal gt As GoodType) As Range
        Select Case SaleGoodProduction(gt)
            Case SaleDemand.None : Return Nothing
            Case SaleDemand.Illegal, SaleDemand.Rare : Return New Range(5, 10)
            Case SaleDemand.Uncommon : Return New Range(10, 25)
            Case SaleDemand.Common : Return New Range(25, 40)
            Case SaleDemand.Abundant : Return New Range(40, 50)
        End Select
    End Function
    Private Function GetSaleGoodDemandRange(ByVal gt As GoodType) As Range
        Select Case SaleGoodDemand(gt)
            Case SaleDemand.None : Return Nothing
            Case SaleDemand.Illegal, SaleDemand.Rare : Return New Range(10, 15)
            Case SaleDemand.Uncommon : Return New Range(5, 10)
            Case SaleDemand.Common : Return New Range(-5, 5)
            Case SaleDemand.Abundant : Return New Range(-10, 5)
        End Select
    End Function
    Public Function GetGoodQty(ByVal gt As GoodType) As Integer
        Return SaleGoodQty(gt)
    End Function
    Public Function GetGoodPrice(ByVal gt As GoodType, ByVal ShipIsBuying As Boolean) As Double
        If ShipIsBuying = False AndAlso SaleGoodDemand(gt) = SaleDemand.None Then Return 0

        Dim total As Double = Good.GetBasePrice(gt)
        Dim totalModifier As Double = SaleGoodPriceModifier(gt)
        If ShipIsBuying = True Then totalModifier += 0.1

        total = Math.Round(total * totalModifier, 2)
        Return total
    End Function
    Public Function CheckSellGood(ByVal gt As GoodType, ByVal value As Integer, ByRef ship As ShipPlayer) As Boolean
        If SaleGoodQty(gt) < value Then Return False
        If SaleGoodDemand(gt) = SaleDemand.None Then Return False
        If SaleGoodDemand(gt) = SaleDemand.Illegal AndAlso Reputation(IsleFaction.Smuggler) < 2 Then Return False

        Dim totalCost As Double = Math.Round(GetGoodPrice(gt, True) * value, 2)
        If ship.CheckAddCoins(Faction, -totalCost) = False Then Return False

        Return True
    End Function
    Public Sub SellGood(ByVal gt As GoodType, ByVal value As Integer, ByRef ship As ShipPlayer)
        'ship buy good
        Dim totalCost As Double = Math.Round(GetGoodPrice(gt, True) * value, 2)
        ship.AddCoins(Faction, -totalCost)
        SaleGoodQty(gt) -= value
        ship.AddGood(gt, value)
    End Sub
    Public Function CheckBuyGood(ByVal gt As GoodType, ByVal value As Integer, ByRef ship As ShipPlayer) As Boolean
        If SaleGoodDemand(gt) = SaleDemand.None Then Return False
        If SaleGoodDemand(gt) = SaleDemand.Illegal AndAlso Reputation(IsleFaction.Smuggler) < 3 Then Return False

        If ship.GetGood(gt).Qty < value Then Return False

        Return True
    End Function
    Public Sub BuyGood(ByVal gt As GoodType, ByVal value As Integer, ByRef ship As ShipPlayer)
        'ship sell good
        Dim totalCost As Double = Math.Round(GetGoodPrice(gt, False) * value, 2)
        ship.AddGood(gt, -value)
        SaleGoodQty(gt) += value
        ship.AddCoins(Faction, totalCost)
    End Sub
    Private Shared Function ConvertDemandToPricePercentage(ByVal demand As SaleDemand, ByRef rng As Random) As Integer
        Select Case demand
            Case SaleDemand.None : Return 0
            Case SaleDemand.Illegal, SaleDemand.Rare : Return rng.Next(10, 16)
            Case SaleDemand.Uncommon : Return rng.Next(5, 11)
            Case SaleDemand.Common : Return rng.Next(-5, 6)
            Case SaleDemand.Abundant : Return rng.Next(-10, -6)
            Case Else : Throw New Exception("SaleDemand out of range")
        End Select
    End Function

    Private Sub TickMarket()
        For Each gt As GoodType In [Enum].GetValues(GetType(GoodType))
            'set to min if under min; else add stock to max
            If SaleGoodQty(gt) < SaleGoodQtyMin(gt) Then
                SaleGoodQty(gt) = SaleGoodQtyMin(gt)
            Else
                Dim prodRange As Range = GetSaleGoodProductionRange(gt)
                If prodRange <> 0 Then
                    If (gt = GoodType.Rations OrElse gt = GoodType.Water) Then prodRange *= 2
                    SaleGoodQty(gt) += prodRange.Roll(World.Rng)
                    If SaleGoodQty(gt) > SaleGoodQtyMax(gt) Then SaleGoodQty(gt) = SaleGoodQtyMax(gt)
                End If
            End If

            'change price modifier
            Dim demRange As Range = GetSaleGoodDemandRange(gt)
            If demRange <> 0 Then
                Dim min As Double = 1 + (demRange.Min / 100)
                Dim max As Double = 1 + (demRange.Max / 100)
                Dim change As Double = (Dev.FateRoll(World.Rng) / 100)
                Dim modifier As Double = SaleGoodPriceModifier(gt) + change
                If modifier < min Then modifier = min
                If modifier > max Then modifier = max
                SaleGoodPriceModifier(gt) = modifier
            End If
        Next

        If World.ShipPlayer.IsAtSea = False AndAlso World.ShipPlayer.Isle.Equals(Me) Then
            Report.Add("The markets have restocked.", ReportType.Commerce)
        End If
    End Sub
#End Region

#Region "Buildings"
    Public Doctor As Crew = Nothing
    Private Buildings As New List(Of String)
    Public Function GetBuilding(ByVal str As String) As Boolean
        If Buildings.Contains(str) Then Return True Else Return False
    End Function
    Public Sub AddBuilding(ByVal str As String)
        If Buildings.Contains(str) Then Exit Sub

        Select Case str
            Case "Clinic"
                Doctor = Crew.Generate(Race, World.Rng, CrewSkill.Medicine)
                Doctor.Role = CrewRole.Doctor
            Case "Hospital"
                Doctor = Crew.Generate(Race, World.Rng, CrewSkill.Medicine, CrewSkill.Medicine)
                Doctor.Role = CrewRole.Doctor
        End Select

        Buildings.Add(str)
    End Sub
#End Region

#Region "Tick"
    Public Sub Tick()
        'update market on King's day
        If World.Calendar.Day = Calendar.CalendarDay.King Then TickMarket()
    End Sub
#End Region

    Private Sub New(ByVal aWorld As World)
        World = aWorld
        For Each fac In [Enum].GetValues(GetType(IsleFaction))
            Reputation.Add(fac, 5)
            ReputationXP.Add(fac, ReputationThresholds(5))
        Next
        For Each gt In [Enum].GetValues(GetType(GoodType))
            SaleGoodPriceModifier.Add(gt, 1)
            SaleGoodQty.Add(gt, 0)
        Next
    End Sub
    Public Shared Function Generate(ByRef aWorld As World, ByVal aName As String, ByVal aFaction As WorldFaction, ByVal xSector As Integer, ByVal ySector As Integer, ByRef free As World.MapData)
        Dim isle As New Isle(aWorld)
        With isle
            .Name = aName
            .Faction = aFaction
            .XSector = xSector
            .YSector = ySector
            Dim subSector As World.MapDataPoint = free.Grab(xSector, ySector, World.Rng)
            .XSubSector = subSector.X
            .YSubSector = subSector.Y
            .X = ConvertSectorToRange(.XSector, .XSubSector).Roll(World.Rng)
            .Y = ConvertSectorToRange(.YSector, .YSubSector).Roll(World.Rng)
            .GenerateSaleGoods()
        End With
        Return isle
    End Function
    Private Sub GenerateSaleGoods()
        'rarer the demand, the higher the price
        'demand at none = will not buy; demand at illegal = will buy at rare prices
        'sale price always 10% higher than purchase price
        'production determines quantity available for sale
        'production at none = no restock; production at illegal = sell at rare prices


        For Each gt In [Enum].GetValues(GetType(GoodType))
            If gt > 200 AndAlso gt < 300 Then
                'set reagent default to no production, uncommon demand
                SaleGoodProduction.Add(gt, SaleDemand.None)
                SaleGoodDemand.Add(gt, SaleDemand.Uncommon)
                SaleGoodQty(gt) = 0
            Else
                'set default production
                SaleGoodProduction.Add(gt, SaleDemand.Common)
                SaleGoodDemand.Add(gt, SaleDemand.Common)
            End If
        Next
        Select Case Name
            Case "Deathless Kingdom"
                'west
                SetSaleGood(GoodType.Gold, SaleDemand.None, SaleDemand.None)
                SetSaleGood(GoodType.Silver, SaleDemand.Uncommon, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Jewellery, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Metal, SaleDemand.Common, SaleDemand.Abundant)
                SetSaleGood(GoodType.Mordicus, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Rations, SaleDemand.None, SaleDemand.None)
                SetSaleGood(GoodType.Water, SaleDemand.Common, SaleDemand.Common)
                SetSaleGood(GoodType.Salt, SaleDemand.None, SaleDemand.None)
                SetSaleGood(GoodType.Liqour, SaleDemand.None, SaleDemand.Abundant)
                SetSaleGood(GoodType.Coffee, SaleDemand.Rare, SaleDemand.Abundant)
                SetSaleGood(GoodType.Spice, SaleDemand.Uncommon, SaleDemand.Abundant)
                SetSaleGood(GoodType.Tobacco, SaleDemand.None, SaleDemand.None)
                Race = CrewRace.Unrelinquished

            Case "Windsworn Exclave"
                'north
                SetSaleGood(GoodType.Gold, SaleDemand.None, SaleDemand.Rare)
                SetSaleGood(GoodType.Silver, SaleDemand.Common, SaleDemand.Abundant)
                SetSaleGood(GoodType.Jewellery, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Cloth, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Lumber, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Metal, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Boricus, SaleDemand.Uncommon, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Salt, SaleDemand.None, SaleDemand.Illegal)
                SetSaleGood(GoodType.Spice, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Tobacco, SaleDemand.Rare, SaleDemand.Rare)
                Race = CrewRace.Windsworn

            Case "Seatouched Dominion"
                'east
                SetSaleGood(GoodType.Grapeshot, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Shot, SaleDemand.Illegal, SaleDemand.Illegal)
                SetSaleGood(GoodType.Explosive, SaleDemand.Illegal, SaleDemand.Illegal)
                SetSaleGood(GoodType.Grapeshot, SaleDemand.Illegal, SaleDemand.Illegal)
                SetSaleGood(GoodType.Bullets, SaleDemand.Illegal, SaleDemand.Illegal)
                SetSaleGood(GoodType.Gold, SaleDemand.None, SaleDemand.Common)
                SetSaleGood(GoodType.Silver, SaleDemand.None, SaleDemand.Common)
                SetSaleGood(GoodType.Jewellery, SaleDemand.None, SaleDemand.Common)
                SetSaleGood(GoodType.Cloth, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Metal, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Triaicus, SaleDemand.Uncommon, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Rations, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Water, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Salt, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Liqour, SaleDemand.None, SaleDemand.Illegal)
                SetSaleGood(GoodType.Coffee, SaleDemand.None, SaleDemand.Illegal)
                SetSaleGood(GoodType.Spice, SaleDemand.None, SaleDemand.Illegal)
                SetSaleGood(GoodType.Tobacco, SaleDemand.None, SaleDemand.Illegal)
                Race = CrewRace.Seatouched

            Case "Commonwealth"
                'south
                SetSaleGood(GoodType.Shot, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Explosive, SaleDemand.Uncommon, SaleDemand.Common)
                SetSaleGood(GoodType.Grapeshot, SaleDemand.Uncommon, SaleDemand.Common)
                SetSaleGood(GoodType.Bullets, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Lumber, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Incantus, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Rations, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Water, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Salt, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Spice, SaleDemand.Rare, SaleDemand.Common)
                SetSaleGood(GoodType.Tobacco, SaleDemand.Abundant, SaleDemand.Common)
                Race = CrewRace.Human

            Case "Court of Dust"
                'mid
                SetSaleGood(GoodType.Gold, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Silver, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Jewellery, SaleDemand.Abundant, SaleDemand.Common)
                SetSaleGood(GoodType.Cloth, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Metal, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Boricus, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Triaicus, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Incantus, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Mordicus, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Liqour, SaleDemand.Common, SaleDemand.Abundant)
                SetSaleGood(GoodType.Coffee, SaleDemand.Common, SaleDemand.Abundant)
                SetSaleGood(GoodType.Spice, SaleDemand.None, SaleDemand.Rare)
                SetSaleGood(GoodType.Tobacco, SaleDemand.None, SaleDemand.Uncommon)
                Race = CrewRace.Human

            Case "Blasphemy Bay"
                'north-west
                SetSaleGood(GoodType.Gold, SaleDemand.None, SaleDemand.Rare)
                SetSaleGood(GoodType.Silver, SaleDemand.None, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Jewellery, SaleDemand.None, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Cloth, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Lumber, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Triaicus, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Salt, SaleDemand.Illegal, SaleDemand.Illegal)
                SetSaleGood(GoodType.Liqour, SaleDemand.Uncommon, SaleDemand.Common)
                SetSaleGood(GoodType.Coffee, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Spice, SaleDemand.Common, SaleDemand.Rare)
                SetSaleGood(GoodType.Tobacco, SaleDemand.Common, SaleDemand.Rare)
                Race = CrewRace.Seatouched

            Case "Brass Atoll"
                'north-east
                SetSaleGood(GoodType.Lumber, SaleDemand.Uncommon, SaleDemand.Common)
                SetSaleGood(GoodType.Metal, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Boricus, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Triaicus, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Rations, SaleDemand.Uncommon, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Water, SaleDemand.Uncommon, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Salt, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Liqour, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Coffee, SaleDemand.None, SaleDemand.Rare)
                Race = CrewRace.Human

            Case "Blackreef"
                'south-west
                SetSaleGood(GoodType.Shot, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Explosive, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Grapeshot, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Bullets, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Cloth, SaleDemand.None, SaleDemand.Rare)
                SetSaleGood(GoodType.Lumber, SaleDemand.None, SaleDemand.Rare)
                SetSaleGood(GoodType.Metal, SaleDemand.Uncommon, SaleDemand.Common)
                SetSaleGood(GoodType.Incantus, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Rations, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Water, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Liqour, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Coffee, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Spice, SaleDemand.Rare, SaleDemand.Rare)
                SetSaleGood(GoodType.Tobacco, SaleDemand.Rare, SaleDemand.Rare)
                Race = CrewRace.Human

            Case "Hallowsreach"
                'south-east
                SetSaleGood(GoodType.Spice, SaleDemand.Abundant, SaleDemand.Abundant)


            Case "Sanctuary"
            Case "Blackiron Ridge"
            Case "World's Spine"
            Case "Firefalls"
        End Select

        'set initial values
        For Each gt In [Enum].GetValues(GetType(GoodType))
            SaleGoodQty(gt) = GetSaleGoodProductionRange(gt).Roll(World.Rng) * 5
            SaleGoodPriceModifier(gt) = Math.Round(1 + (GetSaleGoodDemandRange(gt).Roll(World.Rng) / 100), 2)
        Next
    End Sub
    Private Sub SetSaleGood(ByVal gt As GoodType, ByVal production As SaleDemand, ByVal demand As SaleDemand)
        SaleGoodProduction(gt) = production
        SaleGoodDemand(gt) = demand
    End Sub
    Private Shared Function ConvertSectorToRange(ByVal sector As Integer, ByVal subSector As Integer) As Range
        Dim max As Integer = sector * 300
        max -= (3 - subSector) * 100
        Dim min As Integer = max - 99
        Return New Range(min, max)
    End Function
    Public Overrides Function ToString() As String
        Return Name
    End Function
    Public Shared Operator =(ByVal i1 As Isle, ByVal i2 As Isle)
        Return i1.Equals(i2)
    End Operator
    Public Shared Operator <>(ByVal i1 As Isle, ByVal i2 As Isle)
        If i1 = i2 Then Return False Else Return True
    End Operator
End Class
