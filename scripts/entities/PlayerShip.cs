namespace LudumDare59.Entities;

using System;

using Godot;

using LudumDare59.Components;

public partial class PlayerShip : CharacterBody2D
{
    public ShipInputComponent InputComponent { get; private set; } = null!;

    public ShipMovementComponent MovementComponent { get; private set; } = null!;

    public ShipHullComponent HullComponent { get; private set; } = null!;

    public ScannerComponent ScannerComponent { get; private set; } = null!;

    public CollisionPolygon2D CollisionPolygon { get; private set; } = null!;

    public Polygon2D Visual { get; private set; } = null!;

    public bool IsDead => HullComponent.IsDepleted;

    public float HullCurrent => HullComponent.CurrentHull;

    public float HullMax => HullComponent.MaxHull;

    public float TotalDamageTaken => HullComponent.TotalDamageTaken;

    public event Action? Died;

    public event Action<float, float>? HullChanged;

    public override void _Ready()
    {
        InputComponent = GetNode<ShipInputComponent>("%ShipInputComponent");
        MovementComponent = GetNode<ShipMovementComponent>("%ShipMovementComponent");
        HullComponent = GetNode<ShipHullComponent>("%ShipHullComponent");
        ScannerComponent = GetNode<ScannerComponent>("%ScannerComponent");
        CollisionPolygon = GetNode<CollisionPolygon2D>("%CollisionPolygon2D");
        Visual = GetNode<Polygon2D>("%Visual");


        HullComponent.HullChanged += OnHullChanged;
        HullComponent.HullDepleted += OnHullDepleted;
        AddToGroup("player_ship");
    }

    public override void _ExitTree()
    {
        if (HullComponent is not null)
        {
            HullComponent.HullChanged -= OnHullChanged;
            HullComponent.HullDepleted -= OnHullDepleted;
        }

        RemoveFromGroup("player_ship");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 inputDirection = InputComponent.GetMovementInput();
        MovementComponent.ProcessMovement(inputDirection, delta);

        Vector2 velocity = MovementComponent.Velocity;
        if (velocity.LengthSquared() <= 1.0f)
        {
            return;
        }

        float facingAngle = velocity.Angle() + (Mathf.Pi / 2.0f);
        Visual.Rotation = facingAngle;
        CollisionPolygon.Rotation = facingAngle;
    }

    public void ResetForRun(Vector2 spawnPoint)
    {
        GlobalPosition = spawnPoint;
        MovementComponent.ResetMotion();
        ScannerComponent.ResetScanner();
        HullComponent.ResetHull();
    }

    public void PrepareForSector(Vector2 spawnPoint)
    {
        GlobalPosition = spawnPoint;
        MovementComponent.ResetMotion();
    }

    public void ApplyDamage(float amount)
    {
        HullComponent.ApplyDamage(amount);
    }

    private void OnHullChanged(float currentHull, float maxHull)
    {
        HullChanged?.Invoke(currentHull, maxHull);
    }

    private void OnHullDepleted()
    {
        Died?.Invoke();
    }
}
