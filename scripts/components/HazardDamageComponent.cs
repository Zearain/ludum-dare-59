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

    [Export]
    public Area2D DamageArea { get; set; } = null!;

    [Export]
    public CollisionPolygon2D DamageCollision { get; set; } = null!;

    [Export]
    public Polygon2D Visual { get; set; } = null!;

    public bool IsDamageEnabled { get; set; } = true;

    public Vector2 DamageOrigin => DamageArea?.GlobalPosition ?? Vector2.Zero;

    private PlayerShip? _playerInside;

    public override void _Ready()
    {
        var missingReferences = false;
        if (DamageArea is null)
        {
            GD.PrintErr("HazardDamageComponent is missing a reference to its DamageArea.");
            missingReferences = true;
        }

        if (DamageCollision is null)
        {
            GD.PrintErr("HazardDamageComponent is missing a reference to its DamageCollision.");
            missingReferences = true;
        }

        if (Visual is null)
        {
            GD.PrintErr("HazardDamageComponent is missing a reference to its Visual.");
            missingReferences = true;
        }

        if (missingReferences)
        {
            QueueFree();
            return;
        }

        Area2D damageArea = DamageArea!;
        damageArea.BodyEntered += OnBodyEntered;
        damageArea.BodyExited += OnBodyExited;
        SyncCollisionToVisual();
    }

    public override void _ExitTree()
    {
        if (DamageArea is null)
        {
            return;
        }

        DamageArea.BodyEntered -= OnBodyEntered;
        DamageArea.BodyExited -= OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerInside is null || _playerInside.IsDead)
        {
            return;
        }

        if (!IsDamageEnabled)
        {
            return;
        }

        _playerInside.ApplyDamage(DamagePerSecond * (float)delta);
    }

    public void Configure(float radius, float damagePerSecond)
    {
        DamageRadius = radius;
        DamagePerSecond = damagePerSecond;
        UpdateVisualScaleFromRadius();
        SyncCollisionToVisual();
    }

    public void SyncCollisionToVisual()
    {
        if (Visual is null || DamageArea is null || DamageCollision is null)
        {
            return;
        }

        DamageCollision.Polygon = Visual.Polygon;
        DamageArea.Position = Visual.Position;
        DamageArea.Rotation = Visual.Rotation;
        DamageArea.Scale = Visual.Scale;
    }

    private void UpdateVisualScaleFromRadius()
    {
        if (Visual is null)
        {
            return;
        }

        float scale = DamageRadius / 84.0f;
        Visual.Scale = new Vector2(scale, scale);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerShip playerShip)
        {
            _playerInside = playerShip;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is PlayerShip playerShip && playerShip == _playerInside)
        {
            _playerInside = null;
        }
    }
}
