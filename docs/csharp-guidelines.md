# C# Guidelines

## Formatting

- **Allman braces** — opening brace on its own line, always.
- 4-space indentation, no tabs.
- One class per file; file name must match class name exactly.
- All Godot node scripts must be declared `partial`.
- LF line endings enforced via `.gitattributes`.

```csharp
public partial class Player : CharacterBody3D
{
    public void TakeDamage(int amount)
    {
        // Allman style
    }
}
```

## Naming Conventions

| Element               | Style           | Example                     |
|-----------------------|-----------------|-----------------------------|
| Class / Struct        | PascalCase      | `PlayerController`          |
| Interface             | IPascalCase     | `IDamageable`               |
| Type parameter        | TPascalCase     | `TItem`                     |
| Method                | PascalCase      | `TakeDamage()`              |
| Property              | PascalCase      | `MaxHealth`                 |
| Constant (class)      | PascalCase      | `DefaultGravity`            |
| Enum type & value     | PascalCase      | `ItemType.Weapon`           |
| Private field         | _camelCase      | `_currentHealth`            |
| Private static field  | s_camelCase     | `s_instance`                |
| Local variable        | camelCase       | `spawnPoint`                |
| Parameter             | camelCase       | `damageAmount`              |
| Signal delegate       | PascalCase      | `HealthChangedEventHandler` |

## Namespaces and Imports

- **File-scoped namespaces** (single-line, not block form).
- Namespace must match folder structure under the project root.
- Import ordering: `System` → `Godot` → project namespaces, each group separated by a blank line.
- `dotnet_sort_system_directives_first = true` is enforced in `.editorconfig`.

```csharp
using System;
using System.Collections.Generic;

using Godot;

using VoidlineProspector.Systems;

namespace VoidlineProspector.Characters;
```

## Type Usage

- Use `var` when the type is obvious from the right-hand side; use explicit types otherwise.
- Nullable reference types are **enabled** — annotate with `?` wherever null is possible.
- Prefer pattern matching over casts: `if (node is Player player)`.
- Prefer switch expressions over switch statements for pure value mapping.
- `csharp_style_prefer_primary_constructors = true` (suggestion) — use when it reduces noise.

```csharp
var timer = GetNode<Timer>("CooldownTimer");   // type obvious from RHS
Timer? cachedTimer = null;                      // nullable, explicit type

// Pattern matching preferred
if (target is IDamageable damageable)
    damageable.TakeDamage(info);

// Switch expression preferred for pure mapping
return quadrant switch
{
    Quadrant.Fore => "Front",
    Quadrant.Aft  => "Rear",
    _             => "Unknown",
};
```

## Signals and Exports

```csharp
[Signal]
public delegate void HealthChangedEventHandler(int current, int maximum);

// Emit via SignalName constant (never raw strings)
EmitSignal(SignalName.HealthChanged, _currentHealth, MaxHealth);

// Connect in code with C# events
someNode.HealthChanged += OnHealthChanged;

// Exports — use groups/categories to organise inspector
[ExportGroup("Combat")]
[Export]
private int _baseDamage = 10;

[Export(PropertyHint.Range, "0,100,1")]
private float _acceleration = 20.0f;
```

## Error Handling

- `GD.PushError()` — errors that should surface in the Godot debugger.
- `GD.PushWarning()` — non-fatal warnings.
- `GD.Print()` — temporary debug only; **remove before committing**.
- Guard `FileAccess.Open()` and similar APIs with null checks.
- Use try/catch for I/O and parsing; log failures via `GD.PushError()`.
- Use `System.Diagnostics.Debug.Assert()` for invariants (stripped in release).

```csharp
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
```

## Performance

- Cache `GetNode<T>()` in `_Ready()` — **never** in `_Process()` or `_PhysicsProcess()`.
- Disable processing when idle: `SetProcess(false)` / `SetPhysicsProcess(false)`.
- Use `StringName` for frequently compared strings and signal names.
- Avoid LINQ and heap allocations inside `_Process()` / `_PhysicsProcess()` — reuse collections.
- Prefer Godot value types (`Vector3`, `Color`, `Quaternion`) over custom structs.
