namespace LudumDare59.Entities;

using System;

using Godot;

using LudumDare59.Components;

public partial class PlayerShip : CharacterBody2D
{
    private const float DamageAlarmHoldSeconds = 0.2f;

    public ShipInputComponent InputComponent { get; private set; } = null!;

    public ShipMovementComponent MovementComponent { get; private set; } = null!;

    public ShipHullComponent HullComponent { get; private set; } = null!;

    public ScannerComponent ScannerComponent { get; private set; } = null!;

    public CollisionPolygon2D CollisionPolygon { get; private set; } = null!;

    public Polygon2D Visual { get; private set; } = null!;

    public AudioStreamPlayer DamageAlarmPlayer { get; private set; } = null!;

    public AudioStreamPlayer DeathExplosionPlayer { get; private set; } = null!;

    public AudioStreamPlayer ScanActivationPlayer { get; private set; } = null!;

    public bool IsDead => HullComponent.IsDepleted;

    public float HullCurrent => HullComponent.CurrentHull;

    public float HullMax => HullComponent.MaxHull;

    public float TotalDamageTaken => HullComponent.TotalDamageTaken;

    public event Action? Died;

    public event Action<float, float>? HullChanged;

    private float _damageAlarmTimeRemaining;

    public override void _Ready()
    {
        InputComponent = GetNode<ShipInputComponent>("%ShipInputComponent");
        MovementComponent = GetNode<ShipMovementComponent>("%ShipMovementComponent");
        HullComponent = GetNode<ShipHullComponent>("%ShipHullComponent");
        ScannerComponent = GetNode<ScannerComponent>("%ScannerComponent");
        CollisionPolygon = GetNode<CollisionPolygon2D>("%CollisionPolygon2D");
        Visual = GetNode<Polygon2D>("%Visual");
        DamageAlarmPlayer = GetNode<AudioStreamPlayer>("%DamageAlarmPlayer");
        DeathExplosionPlayer = GetNode<AudioStreamPlayer>("%DeathExplosionPlayer");
        ScanActivationPlayer = GetNode<AudioStreamPlayer>("%ScanActivationPlayer");

        HullComponent.HullChanged += OnHullChanged;
        HullComponent.HullDepleted += OnHullDepleted;
        ScannerComponent.ScanTriggered += OnScanTriggered;
        AddToGroup("player_ship");
    }

    public override void _ExitTree()
    {
        if (HullComponent is not null)
        {
            HullComponent.HullChanged -= OnHullChanged;
            HullComponent.HullDepleted -= OnHullDepleted;
        }

        if (ScannerComponent is not null)
        {
            ScannerComponent.ScanTriggered -= OnScanTriggered;
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

    public override void _Process(double delta)
    {
        if (_damageAlarmTimeRemaining <= 0.0f)
        {
            return;
        }

        _damageAlarmTimeRemaining = Mathf.Max(0.0f, _damageAlarmTimeRemaining - (float)delta);
        if (_damageAlarmTimeRemaining > 0.0f)
        {
            if (!DamageAlarmPlayer.Playing)
            {
                DamageAlarmPlayer.Play();
            }

            return;
        }

        if (DamageAlarmPlayer.Playing)
        {
            DamageAlarmPlayer.Stop();
        }
    }

    public void ResetForRun(Vector2 spawnPoint)
    {
        GlobalPosition = spawnPoint;
        MovementComponent.ResetMotion();
        ScannerComponent.ResetScanner();
        HullComponent.ResetHull();
        _damageAlarmTimeRemaining = 0.0f;
        DamageAlarmPlayer.Stop();
        DeathExplosionPlayer.Stop();
        ScanActivationPlayer.Stop();
    }

    public void PrepareForSector(Vector2 spawnPoint)
    {
        GlobalPosition = spawnPoint;
        MovementComponent.ResetMotion();
    }

    public void ApplyDamage(float amount)
    {
        if (amount <= 0.0f || IsDead)
        {
            return;
        }

        float hullBeforeDamage = HullCurrent;
        HullComponent.ApplyDamage(amount);
        if (HullCurrent < hullBeforeDamage && !IsDead)
        {
            _damageAlarmTimeRemaining = DamageAlarmHoldSeconds;
            if (!DamageAlarmPlayer.Playing)
            {
                DamageAlarmPlayer.Play();
            }
        }
    }

    private void OnHullChanged(float currentHull, float maxHull)
    {
        HullChanged?.Invoke(currentHull, maxHull);
    }

    private void OnHullDepleted()
    {
        _damageAlarmTimeRemaining = 0.0f;
        DamageAlarmPlayer.Stop();
        DeathExplosionPlayer.Stop();
        DeathExplosionPlayer.Play();
        Died?.Invoke();
    }

    private void OnScanTriggered()
    {
        ScanActivationPlayer.Stop();
        ScanActivationPlayer.Play();
    }
}
