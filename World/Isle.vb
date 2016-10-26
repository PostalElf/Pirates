Public Class Isle
    Public Name As String
    Public X As Integer
    Public XSector As Integer
    Public XSubSector As Integer
    Public Y As Integer
    Public YSector As Integer
    Public YSubSector As Integer
    Private World As World

#Region "Politics"
    Public Faction As WorldFaction
    Private Reputation As New Dictionary(Of IsleFaction, Integer)
    Private ReputationXP As New Dictionary(Of IsleFaction, Double)
    Private Shared ReputationThresholds As Integer() = {0, 100, 300, 600, 1000, 1500}
    Public Sub AddReputationXP(ByVal fac As IsleFaction, ByVal value As Double)
        Dim maxLevel As Integer = ReputationThresholds.Count - 1
        Dim maxThreshold As Integer = ReputationThresholds(maxLevel)
        If Reputation(fac) >= maxLevel Then Exit Sub

        ReputationXP(fac) += value
        If ReputationXP(fac) > maxThreshold Then ReputationXP(fac) = maxThreshold

        Dim level As Integer = Reputation(fac)
        While ReputationXP(fac) > ReputationThresholds(level)
            level += 1
            Reputation(fac) += 1
        End While
    End Sub
#End Region

#Region "Sale Goods"
    Private SaleGoodDemand As New Dictionary(Of GoodType, SaleDemand)
    Private SaleGoodProduction As New Dictionary(Of GoodType, SaleDemand)
    Private SaleGoodQty As New Dictionary(Of GoodType, Integer)
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
        Return total * totalModifier
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
#End Region

#Region "Tick"
    Public Sub Tick()
        'update market on King's day
        If World.Calendar.Day = Calendar.CalendarDay.King Then TickMarket()
    End Sub
    Private Sub TickMarket()
        For Each gt As GoodType In [Enum].GetValues(GetType(GoodType))
            'add stock
            Dim range As Range = GetSaleGoodProductionRange(gt)
            If range <> 0 Then
                If (gt = GoodType.Rations OrElse gt = GoodType.Water) Then range *= 2
                SaleGoodQty(gt) += range.Roll(World.Rng)
            End If

            'change price modifier
            range = GetSaleGoodDemandRange(gt)
            If range <> 0 Then
                Dim min As Double = 1 + (range.Min / 100)
                Dim max As Double = 1 + (range.Max / 100)
                Dim change As Double = (Dev.FateRoll(World.Rng) / 100)
                Dim modifier As Double = SaleGoodPriceModifier(gt) + change
                If modifier < min Then modifier = min
                If modifier > max Then modifier = max
                SaleGoodPriceModifier(gt) = modifier
            End If
        Next
    End Sub
#End Region

    Private Sub New(ByVal aWorld As World)
        World = aWorld
        For Each fac In [Enum].GetValues(GetType(IsleFaction))
            Reputation.Add(fac, 0)
            ReputationXP.Add(fac, 0)
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
                SetSaleGood(GoodType.Gold, SaleDemand.None, SaleDemand.None)
                SetSaleGood(GoodType.Silver, SaleDemand.Uncommon, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Jewellery, SaleDemand.Rare, SaleDemand.Uncommon)
                SetSaleGood(GoodType.Metal, SaleDemand.Common, SaleDemand.Abundant)
                SetSaleGood(GoodType.Mordicus, SaleDemand.Abundant, SaleDemand.Abundant)
                SetSaleGood(GoodType.Rations, SaleDemand.None, SaleDemand.None)
                SetSaleGood(GoodType.Water, SaleDemand.Common, SaleDemand.Common)
                SetSaleGood(GoodType.Salt, SaleDemand.None, SaleDemand.None)
                SetSaleGood(GoodType.Liqour, SaleDemand.None, SaleDemand.Illegal)
                SetSaleGood(GoodType.Coffee, SaleDemand.Rare, SaleDemand.Abundant)
                SetSaleGood(GoodType.Spice, SaleDemand.Uncommon, SaleDemand.Abundant)
                SetSaleGood(GoodType.Tobacco, SaleDemand.None, SaleDemand.None)

            Case "Forsworn Exclave"
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

            Case "Seatouched Dominion"
            Case "Commonwealth"
                SetSaleGood(GoodType.Gold, SaleDemand.Rare, SaleDemand.None)

            Case "Court of Dust"
            Case "Blasphemy Bay"
            Case "Brass Atoll"
            Case "Blackreef"
            Case "Hallowsreach"
            Case "Sanctuary"
            Case "Blackiron Ridge"
            Case "World's Spine"
            Case "Firefalls"
        End Select

        'set initials
        For Each gt In [Enum].GetValues(GetType(GoodType))
            SaleGoodQty(gt) = GetSaleGoodProductionRange(gt).Roll(World.Rng) * 5
            SaleGoodPriceModifier(gt) = 1 + (GetSaleGoodDemandRange(gt).Roll(World.Rng) / 100)
        Next
        TickMarket()
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
