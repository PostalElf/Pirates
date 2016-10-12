Public Class ShipWeapon
    Public Name As String
    Public ShipDamage As Damage
    Public Range As Integer
    Public Ship As Ship
    Public Quarter As ShipQuarter
    Public CrewCount As Integer

    Private Sub Cooldown(ByVal value As Integer)
        CooldownCounter -= value
    End Sub
    Public ReadOnly Property IsReady As Boolean
        Get
            If CooldownCounter > 0 Then Return False
            If Ship Is Nothing Then Return False
            If Ship.GetCrews(Quarter).Count < CrewCount Then Return False
            Return True
        End Get
    End Property
    Private CooldownCounter As Integer
    Private CooldownMax As Integer

    Public Sub Attack(ByVal attackDirection As BattleDirection, ByVal attackTarget As BattlefieldObject)
        Dim targetQuarter As ShipQuarter = attackTarget.GetTargetQuarter(attackDirection)
        attackTarget.Damage(ShipDamage, targetQuarter)
        CooldownCounter = CooldownMax
    End Sub
    Public Sub Tick()
        Cooldown(1)
    End Sub

    Public Sub New()
    End Sub
    Public Sub New(ByVal aName As String, ByVal aShipDamage As Damage, ByVal aRange As Integer, ByVal aCrewCount As Integer, ByVal aCoolDown As Integer)
        Name = aName
        ShipDamage = aShipDamage
        ShipDamage.Sender = Name
        Range = aRange
        CrewCount = aCrewCount
        CooldownMax = aCoolDown
    End Sub
    Public Sub New(ByVal aName As String, ByVal dShipDamage As Integer, ByVal dCrewDamage As Integer, ByVal dType As DamageType, ByVal aRange As Integer, ByVal aCrewCount As Integer, ByVal aCoolDown As Integer)
        Name = aName
        Range = aRange
        CrewCount = aCrewCount
        CooldownMax = aCoolDown

        Dim damage As New Damage(dShipDamage, dCrewDamage, dType, Name)
        ShipDamage = damage
    End Sub
    Public Overrides Function ToString() As String
        Return Name & " - Range " & Range & " - Damage " & ShipDamage.ShipDamage
    End Function
    Public Shared Function Clone(ByRef weapon As ShipWeapon) As ShipWeapon
        Dim w As New ShipWeapon
        With weapon
            w.Name = .Name
            w.ShipDamage = Damage.Clone(.ShipDamage)
            w.Range = .Range
            w.Ship = .Ship
            w.Quarter = .Quarter
            w.CrewCount = .CrewCount
            w.CooldownMax = .CooldownMax
        End With
        Return w
    End Function
End Class
