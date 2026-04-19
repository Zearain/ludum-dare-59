namespace LudumDare59.Components;

using Godot;

using LudumDare59.Entities;
using LudumDare59.Interfaces;

public partial class HazardDamageComponent : Node, IDamageSource
{
    [Export]
    public float DamagePerSecond { get; set; } = 10.0f;

    [Export]
    public float DamageRadius { get; set; } = 84.0f;

    public bool IsDamageEnabled { get; set; } = true;

    public Vector2 DamageOrigin => _hazardNode?.GlobalPosition ?? Vector2.Zero;

    private Node2D? _hazardNode;
    private PlayerShip? _playerShip;

    public override void _PhysicsProcess(double delta)
    {
        if (_hazardNode is null)
        {
            return;
        }

        if (_playerShip is null || _playerShip.IsDead)
        {
            return;
        }

        if (!IsDamageEnabled)
        {
            return;
        }

        float distance = _hazardNode.GlobalPosition.DistanceTo(_playerShip.GlobalPosition);
        if (distance > DamageRadius)
        {
            return;
        }

        _playerShip.ApplyDamage(DamagePerSecond * (float)delta);
    }

    public void Configure(float radius, float damagePerSecond)
    {
        DamageRadius = radius;
        DamagePerSecond = damagePerSecond;
    }

    public void Initialize(Node2D hazardNode, PlayerShip playerShip)
    {
        _hazardNode = hazardNode;
        _playerShip = playerShip;
    }
}
