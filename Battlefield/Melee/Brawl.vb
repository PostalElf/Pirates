Public Class Brawl
    Implements MeleeHost

    Public Property InMelee As Boolean = True Implements MeleeHost.InMelee
    Private Crews As List(Of Crew) = Nothing
    Public Function GetCrews(ByVal quarter As ShipQuarter, ByVal role As CrewRole) As List(Of Crew) Implements MeleeHost.GetCrews
        'ignore quarter and role
        Return Crews
    End Function

    Private Ship As Ship
    Private Goods As New Dictionary(Of GoodType, Integer)
    Public Function CheckGoodsFreeForConsumption(ByVal gt As GoodType) As Boolean Implements MeleeHost.CheckGoodsFreeForConsumption
        If Ship Is Nothing = False Then Return Ship.CheckGoodsFreeForConsumption(gt)
        Return True
    End Function
    Public Function CheckAddGood(ByVal gt As GoodType, ByVal qty As Integer) As Boolean Implements MeleeHost.CheckAddGood
        If Ship Is Nothing = False Then Return Ship.CheckAddGood(gt, qty)

        If qty < 0 AndAlso Goods(gt) + qty < 0 Then Return False
        Return True
    End Function
    Public Sub AddGood(ByVal gt As GoodType, ByVal qty As Integer) Implements MeleeHost.AddGood
        If Ship Is Nothing = False Then Ship.AddGood(gt, qty) : Exit Sub
        If Goods.ContainsKey(gt) = False Then Goods.Add(gt, 0)
        Goods(gt) += qty
    End Sub

    Public Sub New(ByVal crewlist As List(Of Crew), ByVal aShip As Ship)
        Crews = crewlist
        Ship = aShip
    End Sub
    Public Sub New(ByVal crewlist As List(Of Crew), ByVal aGoods As Dictionary(Of GoodType, Integer))
        Crews = crewlist
        Goods = aGoods
    End Sub
    Public Shared Function Generate(ByVal isle As Isle, ByVal difficulty As Integer, ByVal crewNumber As Integer) As Brawl
        'difficulty 1 to 5

        Dim aGoods As New Dictionary(Of GoodType, Integer)
        aGoods.Add(GoodType.Bullets, 0)

        Dim crewlist As New List(Of Crew)
        For n = 1 To crewNumber
            Dim crew As Crew = Nothing
            Select Case difficulty
                Case 1
                    crew = crew.Generate(isle.Race, World.Rng, Nothing, CrewSkill.Melee)
                Case 2
                    crew = crew.Generate(isle.Race, World.Rng, CrewSkill.Melee)
                    Dim weapons As New List(Of String) From {"Knife", "Bullwhip", "Brass Knuckles"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                Case 3
                    crew = crew.Generate(isle.Race, World.Rng, CrewSkill.Melee, CrewSkill.Firearms)
                    crew.RemoveBonus("equipment", "Right Hand")
                    Dim weapons As New List(Of String) From {"Cutlass", "Rapier", "Mace"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                    weapons = New List(Of String) From {"Flintlock Pistol", "Long Knife", "Small Sword"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                    aGoods(GoodType.Bullets) += 5
                Case 4
                    crew = crew.Generate(isle.Race, World.Rng, CrewSkill.Melee, CrewSkill.Firearms)
                    crew.AddSkillXP(CrewSkill.Firearms, 200)
                    crew.RemoveBonus("equipment", "Right Hand")
                    Dim weapons As New List(Of String) From {"Musket", "Sabre", "Mace", "Blunderbuss"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                    weapons = New List(Of String) From {"Wheellock Pistol", "Small Sword"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                    aGoods(GoodType.Bullets) += 10
                Case 5
                    crew = crew.Generate(isle.Race, World.Rng, CrewSkill.Melee, CrewSkill.Firearms)
                    crew.AddSkillXP(CrewSkill.Melee, 400)
                    crew.AddSkillXP(CrewSkill.Firearms, 300)
                    crew.RemoveBonus("equipment", "Right Hand")
                    Dim weapons As New List(Of String) From {"Rifle", "Blunderbuss", "Boarding Axe", "Skullcracker"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                    weapons = New List(Of String) From {"Sparklock Pistol", "Writhing Knife"}
                    crew.AddBonus("equipment", CrewBonus.Generate(Dev.GetRandom(Of String)(weapons, World.Rng)))
                    aGoods(GoodType.Bullets) += 15
                Case Else
                    Throw New Exception("Difficulty out of range")
            End Select
            crewlist.Add(crew)
        Next

        Return New Brawl(crewlist, aGoods)
    End Function
End Class
