---
name: dotnet-code-review
description: Code review and design pattern guidance for C#/.NET in a Godot 4.6 game project. Covers SOLID principles, async/await correctness, disposal and cleanup, dependency injection, performance anti-patterns, testability, documentation, and Godot-specific patterns including partial classes, node lifecycle, GetNode caching, and processing discipline.
---

# .NET/C# Code Review for Godot

Use this skill when reviewing C# code in this project. Evaluate each area below and provide specific, actionable feedback.

## Godot-Specific Requirements

These are hard requirements for any Godot node script:

- **All node scripts must be `partial` classes.** Godot's source generator requires this.
- **`GetNode<T>()` must only be called in `_Ready()`.** Calling it in `_Process()`, `_PhysicsProcess()`, or other hot paths allocates and is slow. Cache results in private fields.
- **`SetProcess(false)` / `SetPhysicsProcess(false)` when idle.** Nodes that don't need per-frame updates should disable processing.
- **Cleanup goes in `_ExitTree()`, not `IDisposable.Dispose()`.** Godot manages node lifetime; implement `_ExitTree()` to disconnect signals, stop timers, and release non-Godot resources.
- **Signal connections: prefer C# events (`+=`) over `Connect(string, ...)`.** The typed event syntax is refactor-safe and checked at compile time.
- **No allocations in `_Process()` / `_PhysicsProcess()`.** Avoid LINQ, `new` expressions, string concatenation, and boxing in hot loops.
- **`GD.PushError()` / `GD.PushWarning()` for editor-visible diagnostics.** Reserve C# exceptions for truly exceptional conditions; use `GD.PushError` for errors the developer needs to see in the Godot debugger.

```csharp
// ❌ GetNode in hot path
public override void _Process(double delta)
{
    var bar = GetNode<ProgressBar>("UI/HealthBar"); // allocates every frame
    bar.Value = _health;
}

// ✅ Cached in _Ready
private ProgressBar _healthBar = null!;

public override void _Ready()
{
    _healthBar = GetNode<ProgressBar>("UI/HealthBar");
}

public override void _Process(double delta)
{
    _healthBar.Value = _health;
}
```

```csharp
// ❌ IDisposable on a Node
public partial class AudioManager : Node, IDisposable
{
    public void Dispose() { _stream?.Dispose(); }
}

// ✅ _ExitTree for cleanup
public partial class AudioManager : Node
{
    public override void _ExitTree()
    {
        _stream?.Dispose();
    }
}
```

---

## SOLID Principles

Check each principle:

- **Single Responsibility**: Does the class do one thing? A `Player` class should not also contain inventory logic, save/load logic, and audio management.
- **Open/Closed**: Can behavior be extended without modifying existing code? Prefer composition and interfaces over deep inheritance chains.
- **Liskov Substitution**: Can subtypes replace base types without breaking callers? Verify that overrides do not weaken preconditions or strengthen postconditions.
- **Interface Segregation**: Are interfaces narrow and focused? An `IDamageable` interface should not also include movement and animation methods.
- **Dependency Inversion**: Do high-level modules depend on abstractions, not concrete classes? Game systems that need services should receive them via constructor parameters or exported node references, not `GetNode<ConcreteService>()`.

---

## Design Patterns

Evaluate whether patterns are correctly applied and whether missing patterns would help:

### Recommended Patterns for Game Code

| Pattern | Use Case |
|---------|----------|
| **Composition** | Entity assembly from reusable component nodes via `[Export]` typed references. Components emit signals; the orchestrator wires them together. |
| **State Machine** | Character/entity state (idle/moving/boosting/warping). Use interface-based `StateMachine<TEnum>` (pure C# class, not a Node). Each state is its own class with injected owner reference. |
| **Observer (Signals)** | Decoupled event notification. Use Godot signals via C# events for parent-child; use static events for game-wide cross-cutting notifications. |
| **Factory** | Creating enemy/item instances with configuration. |
| **Strategy** | Swappable AI behaviors, damage formulas, movement modes. |
| **Command** | Input buffering, undo/redo, replay systems. |
| **Object Pool** | Frequently spawned/despawned objects (projectiles, particles). |
| **Resource** (Godot) | Data-only configuration (ore definitions, ship stats, wave tables) via `[GlobalClass] Resource`. |

### Common Misapplications

- **Singleton overuse**: Autoloads/singletons should be limited to truly global concerns (GameManager, AudioManager, SceneLoader). Avoid making systems singletons just for convenience. Prefer passing dependencies via `[Export]` references.
- **Inheritance over composition**: Prefer component nodes and interfaces over deep class hierarchies. In Godot, composition means assembling entities from small, reusable component nodes (each with its own scene and script). Components are wired together via `[Export]` typed references and communicate through signals. An entity's root script should be a thin orchestrator, not a monolithic class containing all behavior.
- **Enum+switch state machines**: A growing switch statement in a single class is an open/closed violation. Use interface-based states where each state is its own class. This keeps state-specific logic isolated and testable.
- **`GetNode` paths for component wiring**: Hard-coded node paths break when scenes are reorganized. Use `[Export]` typed references for component dependencies.
- **God class**: A class with 500+ lines doing everything is a design smell. Extract component nodes.

---

## Async/Await Patterns

- Async methods must return `Task` or `Task<T>` -- **not `void`**, except for Godot signal handlers and lifecycle overrides.
- All awaitable methods must be awaited. Ignoring a returned `Task` silently swallows exceptions.
- Do not use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()` -- they block the thread.
- Use `await ToSignal()` for Godot signal-based async, not `Task.Delay`.
- Do not add `ConfigureAwait(false)` in game code -- Godot's synchronization context handles thread affinity.
- Pass `CancellationToken` through async call chains for long-running operations.

---

## Performance

- **No allocations in hot paths**: `_Process`, `_PhysicsProcess`, and `_Input` run every frame. Avoid LINQ, `new`, `string.Format`, and boxing.
- **Use value types for Godot math**: `Vector3`, `Color`, `Quaternion` are structs -- pass by value, not by reference, unless mutation is needed.
- **Prefer `StringName` over `string`** for signal names, input actions, and animation names compared frequently.
- **`Mathf` over `System.Math`**: Avoids float/double conversions and is consistent with Godot's type system.
- **Reuse collections**: Declare `List<T>`, `Dictionary<K,V>` etc. as fields and `Clear()` them rather than allocating new ones each frame.

```csharp
// ❌ Allocation in _Process
public override void _Process(double delta)
{
    var enemies = GetTree().GetNodesInGroup("enemies").OfType<Enemy>().ToList(); // alloc + LINQ
}

// ✅ Pre-cached or reused
private readonly List<Enemy> _nearbyEnemies = new();

public override void _PhysicsProcess(double delta)
{
    _nearbyEnemies.Clear();
    // populate from detection area
}
```

---

## Dependency Injection and Testability

- Dependencies should be injected (constructor parameters, `[Export]` node references) rather than fetched with `GetNode<ConcreteType>()` inside logic.
- Use interfaces to enable mocking in tests (see `gdunit4` skill for Moq patterns).
- Avoid static state and global mutable singletons inside logic classes.
- Game systems that own non-Godot resources (file handles, network connections) should implement cleanup in `_ExitTree()`.

```csharp
// ❌ Hard dependency via GetNode
public partial class QuestSystem : Node
{
    public void CompleteQuest(string id)
    {
        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        saveManager.Save();
    }
}

// ✅ Injected via [Export] reference
public partial class QuestSystem : Node
{
    [Export]
    private SaveManager _saveManager = null!;

    public void CompleteQuest(string id)
    {
        _saveManager.Save();
    }
}
```

```csharp
// ❌ Component reaching sideways to a sibling via GetParent
public partial class BoostComponent : Node
{
    public void Activate()
    {
        // Tight coupling to sibling -- breaks if scene is reorganized
        var defense = GetParent().GetNode<DefenseComponent>("DefenseComponent");
        defense.SetInvulnerable(true);
    }
}

// ✅ Component emits a signal; the orchestrator mediates between siblings
public partial class BoostComponent : Node
{
    [Signal]
    public delegate void BoostActivatedEventHandler();

    public void Activate()
    {
        // Do boost logic...
        EmitSignal(SignalName.BoostActivated);
    }
}

// Orchestrator (PlayerShip) wires siblings together
public partial class PlayerShip : CharacterBody3D
{
    [Export] private BoostComponent _boost = null!;
    [Export] private QuadrantDefenseComponent _defense = null!;

    public override void _Ready()
    {
        _boost.BoostActivated += OnBoostActivated;
    }

    public override void _ExitTree()
    {
        _boost.BoostActivated -= OnBoostActivated;
    }

    private void OnBoostActivated()
    {
        // Orchestrator decides how to react across components
        // e.g., trigger VFX, notify defense, etc.
    }
}
```

---

## Error Handling

- Use `GD.PushError()` for errors visible in the Godot debugger -- not silent `catch {}` blocks.
- Use `GD.PushWarning()` for non-fatal unexpected states.
- Guard Godot APIs with null checks: `GetNodeOrNull<T>()` returns null if the node does not exist.
- Use `System.Diagnostics.Debug.Assert()` for invariants that should never be false (stripped in release builds).
- Use `GD.Print()` only for temporary debug output -- remove before committing.

```csharp
// ❌ Silent failure
var node = GetNode<Player>("Player");
node?.TakeDamage(10); // silently does nothing if missing

// ✅ Explicit error
var node = GetNodeOrNull<Player>("Player");
if (node is null)
{
    GD.PushError("Player node not found -- check scene structure");
    return;
}
node.TakeDamage(10);
```

---

## Naming and Code Quality

Check against project naming conventions (AGENTS.md):

| Element | Expected Style |
|---------|---------------|
| Class/Struct | PascalCase |
| Interface | IPascalCase |
| Type parameter | TPascalCase |
| Method / Property | PascalCase |
| Class-level constant | PascalCase |
| Enum type and values | PascalCase |
| Private field | `_camelCase` |
| Private static field | `s_camelCase` |
| Local variable / parameter | `camelCase` |
| Local constant | `camelCase` |

Additional checks:

- File name must exactly match the class name.
- One class per file.
- Namespace must match folder structure under the project root.
- File-scoped namespaces (not block form).
- Allman brace style (opening brace on its own line).
- Four-space indentation, no tabs.

---

## Documentation

- All public types and members should have XML documentation.
- `[Export]` properties and `[Signal]` delegates should have `<summary>` tags (see `csharp-docs` skill).
- Avoid `GD.Print()` left in production code as documentation substitutes.

---

## Review Checklist Summary

When reviewing a file, check:

**Godot Node Requirements**
1. `partial` class for all node scripts
2. `GetNode<T>()` only in `_Ready()`, cached as field
3. No allocations in `_Process()` / `_PhysicsProcess()`
4. `_ExitTree()` for cleanup, not `IDisposable`
5. Signal connections unsubscribed in `_ExitTree()`
6. C# event syntax for signal connections
7. `SetProcess(false)` when idle

**Composition & Architecture**
8. Components are self-contained -- no assumptions about parent node type
9. `[Export]` typed references used for component wiring (not `GetNode` paths)
10. Components communicate via signals, not direct calls to siblings
11. Orchestrator/root node is the only one that knows about all sibling components
12. `_GetConfigurationWarnings()` implemented for required `[Export]` references
13. State machine uses interface-based states (not enum+switch monolith)

**Correctness & Safety**
14. Async methods return `Task`/`Task<T>` (not `void`, except signal handlers)
15. No `.Wait()` / `.Result` / `.GetAwaiter().GetResult()`
16. `GD.PushError()` for errors, not silent swallowing
17. Dependencies injected, not fetched with `GetNode<Concrete>()`

**Quality**
18. Naming conventions match AGENTS.md table
19. Public API has XML documentation
