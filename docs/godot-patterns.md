# Godot Patterns

## Lifecycle Overrides

Use the correct lifecycle method for each concern. Never call `GetNode` outside `_Ready`.

```csharp
public override void _Ready()
{
    // Cache node refs, subscribe to signals
    _healthBar = GetNode<ProgressBar>("UI/HealthBar");
    _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    _defense.ShieldChanged += OnShieldChanged;
}

public override void _ExitTree()
{
    // Always unsubscribe — prevents dangling handlers on free
    _defense.ShieldChanged -= OnShieldChanged;
}

public override void _Process(double delta) { }          // per-frame logic
public override void _PhysicsProcess(double delta) { }  // physics-rate logic
```

Rule: Never change `position`, `rotation`, or `scale` in `_Process()` for nodes involved in physics, collision, or area detection. Perform those changes in `_PhysicsProcess()`.

## Node Reference Strategy

Two clear rules:

1. **Orchestrators use `GetNode<T>()`** — Root node scripts acquire their direct children via `GetNode` in `_Ready()`. This is appropriate because the orchestrator *owns* its scene tree and its children's identities are fixed.

2. **Components use `[Export]`** — Any node that is not the root should receive references to its parent or siblings via `[Export]` fields. This keeps components reusable and decoupled from scene-tree paths.

```csharp
// Orchestrator (root) — gets children via GetNode
public partial class PlayerShip : CharacterBody3D
{
    private BoostComponent _boost = null!;
    private CargoComponent _cargo = null!;

    public override void _Ready()
    {
        _boost = GetNode<BoostComponent>("BoostComponent");
        _cargo = GetNode<CargoComponent>("CargoComponent");
    }
}
```

```csharp
// Component — receives injected reference via [Export]
public partial class MiningLaserComponent : Node
{
    [Export]
    private CargoComponent _cargo = null!;

    public override string[] _GetConfigurationWarnings()
    {
        if (_cargo is null)
            return ["CargoComponent is required. Assign in the inspector."];
        return [];
    }
}
```

**Never** call `GetNode` in `_Process`, `_PhysicsProcess`, or any hot-path method.

## Configuration Warnings

Override `_GetConfigurationWarnings()` on any node that has required `[Export]` fields. The Scene dock displays a warning icon when they are unset — this is the primary self-documentation mechanism for components.

See the component example in **Node Reference Strategy** above for a complete implementation.

## Autoload Singleton Pattern

Singletons are Godot autoloads registered in `project.godot`. Expose a static `Instance` property and guard against double-registration.

```csharp
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
    }
}
```

Expose internal collections as read-only in the public API:

```csharp
private readonly Dictionary<OreDefinition, int> _cargo = new();
public IReadOnlyDictionary<OreDefinition, int> Cargo => _cargo;
```

## Scene and File Naming

- Scene files: PascalCase `.tscn` — `Player.tscn`, `MainMenu.tscn`.
- C# files: match class name exactly — `Player.cs` for `class Player`.
- Node names in scenes: PascalCase.
- Component scenes must be self-contained — no assumptions about parent scene structure.

## MCP Tools vs File Editing

| Task | Use |
|------|-----|
| Create or modify `.tscn` files | MCP scene tools (`godot_write_scene`, `godot_add_node`) |
| Create or modify `.cs` files | Standard file editing (MCP script tools generate GDScript, not C#) |
| Validate C# code | `dotnet build` |
| After external file changes with editor open | `godot_editor_refresh_filesystem` |
