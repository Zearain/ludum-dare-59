# Composition Architecture

This project uses **composition over inheritance** for all game entities. Entities are assembled from small, reusable component nodes — no deep class hierarchies.

## Principles

1. **Components are single-responsibility nodes.** Each component does one job (defense, movement, boost, mining, warp, etc.) and can be attached to any entity that needs it.
2. **`[Export]` typed references for wiring components.** Components receive parent or sibling references via `[Export]` fields. Orchestrators (root nodes) use `GetNode` for their direct children. This enables inspector-based drag-and-drop assembly while keeping components decoupled from scene-tree paths.
3. **Signals for decoupled communication.** Components emit signals when something happens (`ShieldChanged`, `CargoFull`). The parent orchestrator subscribes. Components **never** directly call methods on sibling components.
4. **Parent orchestrates, children execute.** The entity root script (e.g., `PlayerShip`) wires components together and manages high-level state. It delegates actual behaviour to components.
5. **Scenes are self-contained.** A component scene must function correctly without knowing what entity it belongs to. It receives configuration via `[Export]` properties and `Resource` definitions.

## Entity Tree Example

```
PlayerShip (CharacterBody3D)  ← orchestrator
├── CollisionShape3D
├── BoostComponent            ← GetNode in orchestrator
├── CargoComponent            ← GetNode in orchestrator
├── WarpDriveComponent        ← GetNode in orchestrator
├── MiningLaserComponent      ← GetNode in orchestrator
└── QuadrantDefenseComponent  ← GetNode in orchestrator
```

The same `BoostComponent` can be attached to enemies, drones, or any entity — no inheritance required.

## Component Template

```csharp
namespace VoidlineProspector.Components.Movement;

public partial class BoostComponent : Node
{
    [Signal]
    public delegate void BoostActivatedEventHandler();

    [Export]
    private float _speedMultiplier = 2.0f;

    [Export]
    private float _duration = 1.5f;

    [Export]
    private float _cooldown = 8.0f;

    public bool IsReady => _cooldownTimer <= 0f;
    public bool IsBoosting => _boostTimer > 0f;
    public float SpeedMultiplier => IsBoosting ? _speedMultiplier : 1.0f;

    private float _boostTimer;
    private float _cooldownTimer;

    public void Activate()
    {
        if (!IsReady)
            return;

        _boostTimer = _duration;
        _cooldownTimer = _cooldown;
        EmitSignal(SignalName.BoostActivated);
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (_cargo is null)
            return ["CargoComponent is required. Assign in the inspector."];
        return [];
    }
}
```

## Orchestrator Wiring

```csharp
public partial class PlayerShip : CharacterBody3D
{
    private BoostComponent _boost = null!;
    private CargoComponent _cargo = null!;

    public CargoComponent? Cargo => _cargo;

    public override void _Ready()
    {
        _boost = GetNode<BoostComponent>("BoostComponent");
        _cargo = GetNode<CargoComponent>("CargoComponent");

        _boost.BoostActivated += OnBoostActivated;
        _cargo.CargoFull += OnCargoFull;
    }

    public override void _ExitTree()
    {
        _boost.BoostActivated -= OnBoostActivated;
        _cargo.CargoFull -= OnCargoFull;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("boost"))
            _boost.Activate();
    }
}
```

## Testable Pure Math

Mark computationally intensive, stateless calculations as `internal static` methods. This makes them directly unit-testable without needing a scene tree.

```csharp
// In ShipMovementComponent.cs
internal static float CalculateTurnMultiplier(float speedRatio)
{
    return Mathf.Lerp(1.0f, 0.4f, speedRatio);
}
```

## Interface Contracts

When multiple component implementations share a contract (e.g., `IDefenseComponent`), define the interface and implement it on each variant. Components may expose both a Godot `[Signal]` (for inspector/UI wiring) and a C# `event` (for the interface contract) — emit both together.

```csharp
public interface IDefenseComponent
{
    event Action<int, int> HullDamaged;  // C# event for interface consumers
}

public partial class QuadrantDefenseComponent : Node, IDefenseComponent
{
    [Signal]
    public delegate void HullWasDamagedEventHandler(int current, int max);  // Godot signal for UI

    public event Action<int, int>? HullDamaged;  // C# event for IDefenseComponent

    private void ApplyHullDamage(int amount)
    {
        // ...
        HullDamaged?.Invoke(_hull, _maxHull);
        EmitSignal(SignalName.HullWasDamaged, _hull, _maxHull);
    }
}
```
