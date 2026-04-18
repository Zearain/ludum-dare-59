---
name: godot-code-gen
description: Godot 4.6 C# best practices, type patterns, signals, state machines, exports, async/await, tweens, input handling, and performance guidelines for .NET game development.
---

# Godot C# Code Generation Skill

You are an expert Godot 4.6 game developer using C# with .NET. When generating C# code for Godot, follow these patterns and best practices. This project targets mobile (iOS/Android) with the Mobile renderer, Jolt Physics (3D), and Direct3D12 on Windows.

## Project Conventions

- **Assembly name**: `Ludum Dare 59`
- **Root namespace**: `LudumDare59`
- **All Godot node scripts must be declared `partial`**
- **File-scoped namespaces** (single-line, not block form)
- **Allman brace style** (opening brace on its own line)
- **Four-space indentation**, no tabs
- **One class per file**, file name matches class name exactly
- **Line endings**: LF enforced

## Naming Conventions

| Element               | Style            | Example                  |
|-----------------------|------------------|--------------------------|
| Class / Struct        | PascalCase       | `PlayerController`       |
| Interface             | IPascalCase      | `IDamageable`            |
| Type parameter        | TPascalCase      | `TItem`                  |
| Method                | PascalCase       | `TakeDamage()`           |
| Property              | PascalCase       | `MaxHealth`              |
| Constant (class)      | PascalCase       | `DefaultGravity`         |
| Enum type             | PascalCase       | `ItemType`               |
| Enum value            | PascalCase       | `ItemType.Weapon`        |
| Private field         | _camelCase       | `_currentHealth`         |
| Private static field  | s_camelCase      | `s_instance`             |
| Local variable        | camelCase        | `spawnPoint`             |
| Local constant        | camelCase        | `maxRetries`             |
| Parameter             | camelCase        | `damageAmount`           |
| Signal delegate       | PascalCase       | `HealthChangedEventHandler` |

## Namespace and Import Ordering

```csharp
using System;
using System.Collections.Generic;

using Godot;

using LudumDare59.Systems;

namespace LudumDare59.Characters;

public partial class Player : CharacterBody3D
{
}
```

Import ordering: `System` first, then `Godot`, then project namespaces, separated by blank lines.

## Type Usage

- Use `var` when the type is obvious from the right-hand side; use explicit types otherwise.
- Use nullable annotations (`?`) for reference types that can be null.
- Prefer pattern matching for null checks: `if (node is Player player)`.

```csharp
var timer = GetNode<Timer>("CooldownTimer");   // type obvious
Timer? cachedTimer = null;                      // nullable, type not obvious
```

## Signals (Godot 4.x C# Style)

```csharp
// Declaration -- delegate name must end with EventHandler
[Signal]
public delegate void HealthChangedEventHandler(int current, int maximum);

[Signal]
public delegate void DiedEventHandler();

[Signal]
public delegate void ItemCollectedEventHandler(string itemName, int count);

// Emitting -- use the generated SignalName constants
EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);
EmitSignal(SignalName.Died);

// Connecting via C# events (preferred in code)
someNode.HealthChanged += OnHealthChanged;

// Connecting via Godot's signal system
someNode.Connect(Node.SignalName.Ready, Callable.From(OnNodeReady));

// One-shot connection
someNode.Connect(SignalName.Died, Callable.From(OnDied), (uint)ConnectFlags.OneShot);

// Disconnecting
someNode.HealthChanged -= OnHealthChanged;
```

## Exports

```csharp
[Export]
private float _speed = 5.0f;

[ExportGroup("Combat")]
[Export]
private int _baseDamage = 10;

[ExportCategory("Movement")]
[Export(PropertyHint.Range, "0,100,1")]
private float _acceleration = 20.0f;

[Export(PropertyHint.File, "*.tscn")]
private string _scenePath = "";

[Export(PropertyHint.Enum, "Easy,Medium,Hard")]
private int _difficulty = 1;

[ExportSubgroup("Advanced")]
[Export]
private bool _enableDebug = false;
```

## Lifecycle Overrides

```csharp
public override void _Ready()
{
    // Cache node references here -- NEVER in _Process or _PhysicsProcess
    _healthBar = GetNode<ProgressBar>("UI/HealthBar");
    _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
}

public override void _EnterTree()
{
    // Called when node enters the scene tree
}

public override void _ExitTree()
{
    // Cleanup, disconnect signals
}

public override void _Process(double delta)
{
    // Per-frame logic (rendering frame rate)
}

public override void _PhysicsProcess(double delta)
{
    // Fixed timestep physics logic (Jolt Physics)
}

public override void _Input(InputEvent @event)
{
    // Handle input events
}

public override void _UnhandledInput(InputEvent @event)
{
    // Input not consumed by UI or _Input
}
```

## Node References

### Preferred: `[Export]` Typed References

Use `[Export]` typed references to wire components and dependencies together in the Godot inspector. This decouples nodes from scene tree paths, enables drag-and-drop assembly, and is the primary composition mechanism.

```csharp
// Component references -- assigned in the inspector by wiring child nodes
[Export]
private QuadrantDefenseComponent _defense = null!;

[Export]
private BoostComponent _boost = null!;

[Export]
private CargoComponent _cargo = null!;

public override void _Ready()
{
    // Wire component signals
    _defense.ShieldChanged += OnShieldChanged;
    _cargo.CargoFull += OnCargoFull;
}

public override void _ExitTree()
{
    // Always unsubscribe in _ExitTree to avoid memory leaks
    _defense.ShieldChanged -= OnShieldChanged;
    _cargo.CargoFull -= OnCargoFull;
}
```

### Fallback: `GetNode<T>()` Caching

For child nodes whose identity is fixed and always present within the same scene (e.g., a `CollisionShape3D` built into the root scene). Cache in `_Ready()`, never call in `_Process()` or `_PhysicsProcess()`.

```csharp
private CollisionShape3D _collisionShape = null!;
private MeshInstance3D _mesh = null!;

public override void _Ready()
{
    _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
    _mesh = GetNode<MeshInstance3D>("MeshInstance3D");
}
```

## Common Node Patterns

### CharacterBody3D Movement (3D)

```csharp
using Godot;

namespace LudumDare59.Characters;

public partial class Player : CharacterBody3D
{
    [Export]
    private float _speed = 5.0f;

    [Export]
    private float _jumpVelocity = 4.5f;

    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = _jumpVelocity;
        }

        var inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * _speed;
            velocity.Z = direction.Z * _speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, _speed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, _speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
```

### CharacterBody2D Movement (2D)

```csharp
using Godot;

namespace LudumDare59.Characters;

public partial class Player2D : CharacterBody2D
{
    [Export]
    private float _speed = 200.0f;

    [Export]
    private float _jumpForce = 400.0f;

    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y += _gravity * (float)delta;
        }

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = -_jumpForce;
        }

        float direction = Input.GetAxis("move_left", "move_right");
        velocity.X = direction * _speed;

        Velocity = velocity;
        MoveAndSlide();
    }
}
```

### State Machine (Interface-Based)

Use an interface-based state machine with a **pure C# host class** (not a Node). Each state is its own class with a single responsibility. The owning node drives the state machine by calling `Process()` and `PhysicsProcess()` from its Godot overrides.

This pattern is preferred over enum+switch because:
- Each state is testable in isolation
- Adding new states does not require modifying existing state logic
- State-specific data lives in the state class, not as fields on the owner

```csharp
// Core/StateMachine/IState.cs
namespace LudumDare59.Core.StateMachine;

public interface IState
{
    void Enter();
    void Exit();
    void Process(double delta);
    void PhysicsProcess(double delta);
}
```

```csharp
// Core/StateMachine/StateMachine.cs
namespace LudumDare59.Core.StateMachine;

/// <summary>
/// Generic state machine. Pure C# -- not a Node. Owned and driven by an entity node.
/// </summary>
public sealed class StateMachine<TEnum> where TEnum : notnull
{
    private readonly Dictionary<TEnum, IState> _states = new();

    public TEnum? CurrentStateKey { get; private set; }
    public IState? CurrentState { get; private set; }

    public event Action<TEnum, TEnum>? StateChanged;

    public void RegisterState(TEnum key, IState state)
    {
        _states[key] = state;
    }

    public void ChangeState(TEnum newKey)
    {
        if (!_states.TryGetValue(newKey, out var newState))
        {
            return;
        }

        var oldKey = CurrentStateKey;
        CurrentState?.Exit();
        CurrentStateKey = newKey;
        CurrentState = newState;
        CurrentState.Enter();

        if (oldKey is not null)
        {
            StateChanged?.Invoke(oldKey, newKey);
        }
    }

    public void Process(double delta) => CurrentState?.Process(delta);
    public void PhysicsProcess(double delta) => CurrentState?.PhysicsProcess(delta);
}
```

```csharp
// Entities/Player/States/PlayerMovingState.cs -- state class with injected owner
namespace LudumDare59.Entities.Player.States;

public sealed class PlayerMovingState : IState
{
    private readonly PlayerShip _owner;

    public PlayerMovingState(PlayerShip owner)
    {
        _owner = owner;
    }

    public void Enter() { }
    public void Exit() { }
    public void Process(double delta) { }

    public void PhysicsProcess(double delta)
    {
        // Read input, call movement methods on _owner
    }
}
```

```csharp
// Entities/Player/PlayerShip.cs -- owning node drives the state machine
public partial class PlayerShip : CharacterBody3D
{
    private readonly StateMachine<PlayerState> _stateMachine = new();

    public override void _Ready()
    {
        _stateMachine.RegisterState(PlayerState.Moving, new PlayerMovingState(this));
        _stateMachine.RegisterState(PlayerState.Boosting, new PlayerBoostingState(this));
        _stateMachine.RegisterState(PlayerState.Warping, new PlayerWarpingState(this));
        _stateMachine.RegisterState(PlayerState.Destroyed, new PlayerDestroyedState(this));
        _stateMachine.ChangeState(PlayerState.Moving);
    }

    public override void _Process(double delta) => _stateMachine.Process(delta);
    public override void _PhysicsProcess(double delta) => _stateMachine.PhysicsProcess(delta);
}

public enum PlayerState { Moving, Mining, Boosting, Warping, Destroyed }
```

### Composition Pattern

Build entities by composing small, reusable **component nodes**. Each component does one job and communicates via signals. The entity root script is a thin **orchestrator** that wires components together via `[Export]` typed references.

Components are self-contained scenes that work without knowing what entity they belong to. The same component can be attached to a player ship, an enemy, or a drone.

```csharp
// Components/Defense/QuadrantDefenseComponent.cs
namespace LudumDare59.Components.Defense;

/// <summary>
/// 4-quadrant directional shield/armor/hull system.
/// Self-contained -- no assumptions about the parent entity.
/// </summary>
public partial class QuadrantDefenseComponent : Node
{
    [Signal]
    public delegate void ShieldChangedEventHandler(int quadrant, int current, int max);

    [Signal]
    public delegate void HullChangedEventHandler(int current, int max);

    [Signal]
    public delegate void ShipDestroyedEventHandler();

    /// <summary>Data resource that defines base stats for this entity.</summary>
    [Export]
    private ShipDefinition _shipDefinition = null!;

    public override string[] _GetConfigurationWarnings()
    {
        if (_shipDefinition is null)
        {
            return ["ShipDefinition is required. Assign a ship data resource in the inspector."];
        }
        return [];
    }

    public void TakeDamage(int rawDamage, Vector3 attackOrigin, Vector3 shipPosition, Vector3 shipForward)
    {
        var quadrant = QuadrantResolver.Resolve(attackOrigin, shipPosition, shipForward);
        // Apply damage to that quadrant's shield -> armor -> hull
        // Emit ShieldChanged, HullChanged, or ShipDestroyed as appropriate
    }
}
```

```csharp
// Entities/Player/PlayerShip.cs -- thin orchestrator
namespace LudumDare59.Entities.Player;

public partial class PlayerShip : CharacterBody3D
{
    // All components assigned in the inspector (not looked up via GetNode paths)
    [Export] private QuadrantDefenseComponent _defense = null!;
    [Export] private BoostComponent _boost = null!;
    [Export] private CargoComponent _cargo = null!;
    [Export] private WarpDriveComponent _warp = null!;
    [Export] private MiningLaserComponent _laser = null!;

    public override void _Ready()
    {
        // Orchestrator wires component signals together
        _defense.ShipDestroyed += OnShipDestroyed;
        _cargo.CargoFull += OnCargoFull;
        _warp.WarpCompleted += OnWarpCompleted;
        _boost.BoostActivated += OnBoostActivated;
    }

    public override void _ExitTree()
    {
        _defense.ShipDestroyed -= OnShipDestroyed;
        _cargo.CargoFull -= OnCargoFull;
        _warp.WarpCompleted -= OnWarpCompleted;
        _boost.BoostActivated -= OnBoostActivated;
    }

    private void OnShipDestroyed() => _stateMachine.ChangeState(PlayerState.Destroyed);
    private void OnCargoFull() { /* notify HUD, optionally trigger auto-warp */ }
    private void OnWarpCompleted() { /* transition to hub scene */ }
    private void OnBoostActivated() { /* trigger VFX */ }
}
```

**Scene tree** -- components are direct children, wired via `[Export]`:
```text
PlayerShip (CharacterBody3D)          <- orchestrator script
+-- CollisionShape3D
+-- MeshInstance3D
+-- QuadrantDefenseComponent (Node)   <- [Export] _defense
+-- BoostComponent (Node)             <- [Export] _boost
+-- CargoComponent (Node)             <- [Export] _cargo
+-- WarpDriveComponent (Node)         <- [Export] _warp
+-- MiningLaserComponent (Node3D)     <- [Export] _laser
```

The **Scout enemy** reuses the same `BoostComponent` without any changes:
```text
Scout (CharacterBody3D)               <- orchestrator script
+-- CollisionShape3D
+-- MeshInstance3D
+-- SimpleDefenseComponent (Node)     <- [Export] _defense
+-- BoostComponent (Node)             <- [Export] _boost (same component!)
+-- WeaponComponent (Node)            <- [Export] _weapon
+-- AIMovementComponent (Node)        <- [Export] _movement
```

### Resource Pattern

```csharp
using Godot;

namespace LudumDare59.Data;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export]
    public string Name { get; set; } = "";

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    [Export]
    public Texture2D? Icon { get; set; }

    [Export]
    public int Value { get; set; }

    [Export]
    public bool Stackable { get; set; } = true;

    [Export(PropertyHint.Range, "1,99,1")]
    public int MaxStack { get; set; } = 99;
}
```

Using resources:

```csharp
// Preload at class level
private static readonly PackedScene EnemyScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");
private static readonly ItemData SwordData = GD.Load<ItemData>("res://Data/Items/Sword.tres");

// Use in code
GD.Print(SwordData.Name);
```

### Autoload / Singleton

```csharp
using Godot;

namespace LudumDare59.Systems;

/// <summary>
/// Add to Project Settings > Autoload as "GameManager".
/// </summary>
public partial class GameManager : Node
{
    [Signal]
    public delegate void GamePausedEventHandler();

    [Signal]
    public delegate void GameResumedEventHandler();

    public int Score { get; private set; }
    public bool IsPaused { get; private set; }

    public void Pause()
    {
        IsPaused = true;
        GetTree().Paused = true;
        EmitSignal(SignalName.GamePaused);
    }

    public void Resume()
    {
        IsPaused = false;
        GetTree().Paused = false;
        EmitSignal(SignalName.GameResumed);
    }

    public void AddScore(int points)
    {
        Score += points;
    }
}
```

## Async / Await

```csharp
// Wait for a timer
await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

// Wait for an animation to finish
await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished);

// Wait for a custom signal
await ToSignal(someNode, Node.SignalName.Ready);

// Custom async method
private async void LoadLevelAsync(string path)
{
    ResourceLoader.LoadThreadedRequest(path);

    while (ResourceLoader.LoadThreadedGetStatus(path) == ResourceLoader.ThreadLoadStatus.InProgress)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    var scene = ResourceLoader.LoadThreadedGet(path) as PackedScene;
    if (scene is not null)
    {
        GetTree().ChangeSceneToPacked(scene);
    }
}
```

## Tweens (Godot 4.x)

```csharp
// Simple tween
var tween = CreateTween();
tween.TweenProperty(_sprite, "modulate:a", 0.0f, 1.0f);

// Chained tweens (sequential)
var tween = CreateTween();
tween.TweenProperty(node, "position", new Vector2(100, 100), 0.5);
tween.TweenProperty(node, "scale", new Vector2(2, 2), 0.3);
tween.TweenCallback(Callable.From(() => GD.Print("Done!")));

// Parallel tweens
var tween = CreateTween();
tween.SetParallel(true);
tween.TweenProperty(node, "position:x", 100.0f, 0.5);
tween.TweenProperty(node, "rotation", Mathf.Pi, 0.5);

// Easing and transitions
tween.SetTrans(Tween.TransitionType.Bounce);
tween.SetEase(Tween.EaseType.Out);

// Kill previous tween before creating new one
_activeTween?.Kill();
_activeTween = CreateTween();
```

## Scene Instantiation

```csharp
// Preload (compile-time, preferred for frequently used scenes)
private static readonly PackedScene EnemyScene = GD.Load<PackedScene>("res://Scenes/Enemy.tscn");

public void SpawnEnemy()
{
    var enemy = EnemyScene.Instantiate<Enemy>();
    AddChild(enemy);
    enemy.GlobalPosition = _spawnPoint.GlobalPosition;
}

// Runtime load (for dynamic loading)
public void LoadScene(string path)
{
    var scene = GD.Load<PackedScene>(path);
    var instance = scene.Instantiate();
    AddChild(instance);
}

// Safe cast with pattern matching
var node = EnemyScene.Instantiate();
if (node is Enemy enemy)
{
    enemy.Initialize(_difficulty);
    AddChild(enemy);
}
```

## Input Handling

```csharp
public override void _Input(InputEvent @event)
{
    if (@event.IsActionPressed("attack"))
    {
        Attack();
    }

    if (@event is InputEventMouseButton mouseButton)
    {
        if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
        {
            Shoot();
        }
    }

    // Touch input (mobile)
    if (@event is InputEventScreenTouch touch)
    {
        if (touch.Pressed)
        {
            OnTouchStart(touch.Position);
        }
        else
        {
            OnTouchEnd(touch.Position);
        }
    }

    if (@event is InputEventScreenDrag drag)
    {
        OnTouchDrag(drag.Position, drag.Relative);
    }
}

public override void _UnhandledInput(InputEvent @event)
{
    // Only receives events not consumed by UI
    if (@event.IsActionPressed("pause"))
    {
        TogglePause();
    }
}

// Polling in _Process / _PhysicsProcess
public override void _PhysicsProcess(double delta)
{
    var direction = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
    bool isJumping = Input.IsActionJustPressed("jump");
    bool isAttacking = Input.IsActionPressed("attack");
}
```

## Error Handling

```csharp
// Use GD.PushError for errors visible in the Godot debugger
public string? LoadFileText(string path)
{
    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
    if (file is null)
    {
        GD.PushError($"Failed to open file: {path}");
        return null;
    }
    return file.GetAsText();
}

// Use GD.PushWarning for non-fatal warnings
if (_healthBar is null)
{
    GD.PushWarning("HealthBar node not found, UI updates will be skipped");
}

// Use Debug.Assert for invariants (stripped in release builds)
System.Diagnostics.Debug.Assert(_currentHealth >= 0, "Health should never be negative");

// Guard Godot APIs with null checks and pattern matching
var node = GetNodeOrNull<Player>("Player");
if (node is Player player)
{
    player.TakeDamage(10);
}
```

## Collections and Data

```csharp
// Godot arrays (for interop with signals and editor)
var godotArray = new Godot.Collections.Array<Node>();
var godotDict = new Godot.Collections.Dictionary<string, Variant>();

// C# collections (for internal logic)
private readonly List<Enemy> _activeEnemies = new();
private readonly Dictionary<StringName, int> _stats = new();
private readonly HashSet<Vector3I> _occupiedCells = new();

// StringName for frequently compared strings
private static readonly StringName MoveLeft = "move_left";
private static readonly StringName MoveRight = "move_right";
```

## Common Interfaces

In a composition-based architecture, interfaces are implemented by **component nodes**, not by entities. An entity is damageable because it has a `DefenseComponent` that implements `IDamageable` -- not because `PlayerShip` inherits from a damageable base class.

Use pattern matching on an entity's children to find the component that implements the interface:

```csharp
// Check if an entity has a defense component
if (hitBody.GetNodeOrNull<IDamageable>("DefenseComponent") is IDamageable damageable)
{
    damageable.TakeDamage(damage, attackOrigin, hitBody.GlobalPosition, hitBody.GlobalTransform.Basis.Z);
}
```

```csharp
namespace LudumDare59.Interfaces;

/// <summary>
/// Implemented by defense component nodes (QuadrantDefenseComponent, SimpleDefenseComponent).
/// Entities are damageable because they HAVE a defense component, not because they ARE damageable.
/// </summary>
public interface IDamageable
{
    int CurrentHull { get; }
    int MaxHull { get; }
    bool IsDestroyed { get; }

    /// <param name="rawDamage">Incoming damage before shield/armor absorption.</param>
    /// <param name="attackOrigin">World position the attack came from (used for quadrant resolution).</param>
    /// <param name="targetPosition">World position of the defending entity's center.</param>
    /// <param name="targetForward">Forward direction of the defending entity (used for quadrant resolution).</param>
    void TakeDamage(int rawDamage, Vector3 attackOrigin, Vector3 targetPosition, Vector3 targetForward);
}

public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(Node3D interactor);
}

public interface ISaveable
{
    Godot.Collections.Dictionary<string, Variant> Save();
    void Load(Godot.Collections.Dictionary<string, Variant> data);
}
```

## Performance Guidelines

1. **Prefer `[Export]` typed references** over `GetNode<T>()` for component wiring -- no runtime lookup cost
2. **Cache `GetNode<T>()` results in `_Ready()`** -- never in hot loops
3. **Disable processing when idle**: `SetProcess(false)` / `SetPhysicsProcess(false)`
4. **Prefer Godot value types**: `Vector3`, `Color`, `Quaternion` over custom structs
5. **Use `StringName`** for frequently compared strings and signal names
6. **Avoid allocations in `_Process()` / `_PhysicsProcess()`**: reuse collections, avoid LINQ
7. **Avoid boxing**: use `Variant` carefully, prefer typed APIs
8. **Prefer `Mathf`** over `System.Math` for Godot math operations (avoids float/double conversion)
9. **Use `float`** for most Godot APIs (positions, rotations) -- Godot C# uses `float` for Vector3, etc.
10. **`delta` parameter is `double`**: cast to `float` when multiplying with Godot vector types: `(float)delta`

## Mobile-Specific Considerations

- Handle touch input via `InputEventScreenTouch` and `InputEventScreenDrag`
- Keep draw calls minimal -- Mobile renderer has stricter limits
- Prefer `GPUParticles3D` over `CPUParticles3D` only if the effect warrants it; simple effects can use CPU
- Test with lower-end target hardware in mind
- Use `OS.GetName()` to detect platform: `"Android"`, `"iOS"`
- Consider battery and thermal throttling in performance-sensitive loops

## Godot C# Gotchas

- All Godot node scripts **must** be `partial` classes
- `@` prefix required for C# keywords used as Godot names: `@event`, `@object`, `@class`
- `_Ready()`, `_Process()`, etc. use `override` not `new`
- Godot signals use `delegate` declarations, not C# `event` keyword directly
- `Callable.From()` wraps C# lambdas/methods for Godot signal system
- `ToSignal()` returns `SignalAwaiter` for async/await patterns
- Export properties on Resources need `[GlobalClass]` on the class to appear in the editor
- `PackedScene.Instantiate()` returns `Node`, use `Instantiate<T>()` for typed instantiation
- Node paths use `/` separators, not `\`
- String properties from Godot are never null, they default to `""` (empty string)
- `Vector3` and other value types are **structs** in C#, they are copied on assignment
- Use `GodotObject.IsInstanceValid(node)` to check if a node has been freed
