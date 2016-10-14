Public Class ShipWeapon
    Public Name As String
    Public Damage As Damage
    Public AmmoUse As Good
    Public Range As Integer
    Public Ship As Ship
    Public Quarter As ShipQuarter
    Public CrewCount As Integer
    Public HullCost As Integer

    Private Sub Cooldown(ByVal value As Integer)
        CooldownCounter -= value
    End Sub
    Public ReadOnly Property IsReady As Boolean
        Get
            If Ship Is Nothing Then Return False
            If IgnoresCooldown = False Then
                If CooldownCounter > 0 Then Return False
            End If
            If IgnoresCrewCount = False Then
                If Ship.GetCrews(Quarter, CrewSkill.Gunnery).Count < CrewCount Then Return False
            End If
            If IgnoresAmmo = False Then
                If Ship.GetGood(AmmoUse.Type) < AmmoUse.Qty Then Return False
            End If
            Return True
        End Get
    End Property
    Public CooldownCounter As Integer
    Public CooldownMax As Integer

#Region "Cheaterbug"
    Private IgnoresCooldown As Boolean = False
    Private IgnoresAmmo As Boolean = False
    Private IgnoresCrewCount As Boolean = False
    Public Sub Cheaterbug()
        IgnoresCooldown = True
        IgnoresAmmo = True
        IgnoresCrewCount = True
    End Sub
#End Region

    Public Sub Attack(ByVal attackDirection As BattleDirection, ByVal attackTarget As BattlefieldObject, ByVal crews As List(Of Crew))
        If attackTarget Is Nothing Then Exit Sub

        Dim targetQuarter As ShipQuarter = attackTarget.GetTargetQuarter(attackDirection)
        Dim accuracy As Double = 0
        For Each c In crews
            accuracy += c.GetSkill(CrewSkill.Gunnery)
        Next
        accuracy = Math.Round(accuracy / crews.Count, 0, MidpointRounding.AwayFromZero)

        attackTarget.Damage(Damage, targetQuarter, accuracy)
        CooldownCounter = CooldownMax + 1
    End Sub
    Public Sub Tick()
        Cooldown(1)
    End Sub
    Public ReadOnly Property Heuristic As Double
        Get
            Dim total As Double = Damage.CrewDamage + Damage.ShipDamage
            Select Case Range
                Case 1
                Case 2 : total += 40
                Case Is >= 3 : total += 100
            End Select
            total -= CooldownMax
            Return total
        End Get
    End Property

    Public Sub New()
    End Sub
    Public Sub New(ByVal aName As String, ByVal aShipDamage As Damage, ByVal aRange As Integer, ByVal aAmmoType As GoodType, ByVal aAmmoPerShot As Integer, ByVal aCrewCount As Integer, ByVal aCoolDown As Integer)
        Name = aName
        Damage = aShipDamage
        Damage.Sender = Name
        Range = aRange
        AmmoUse = Good.Generate(aAmmoType, aAmmoPerShot)
        CrewCount = aCrewCount
        CooldownMax = aCoolDown
    End Sub
    Public Sub New(ByVal aName As String, _
                   ByVal dShipDamage As Integer, ByVal dCrewDamage As Integer, ByVal dType As DamageType, _
                   ByVal aRange As Integer, ByVal aAmmoType As GoodType, ByVal aAmmoPerShot As Integer, ByVal aCrewCount As Integer, ByVal aCoolDown As Integer)
        Me.New(aName, New Damage(dShipDamage, dCrewDamage, dType, aName), aRange, aAmmoType, aAmmoPerShot, aCrewCount, aCoolDown)
    End Sub
    Public Overrides Function ToString() As String
        Return Name & " - Range " & Range & " - Damage " & Damage.ShipDamage
    End Function
    Public Function Clone() As ShipWeapon
        Dim w As New ShipWeapon
        With w
            .Name = Name
            .Damage = Damage.Clone
            .AmmoUse = AmmoUse
            .Range = Range
            .Ship = Ship
            .Quarter = Quarter
            .CrewCount = CrewCount
            .HullCost = HullCost

            .CooldownCounter = CooldownCounter
            .CooldownMax = CooldownMax
        End With
        Return w
    End Function
End Class
