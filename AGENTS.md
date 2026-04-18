# Ghost Frequency: Storm Relay Project Rules

## External File Loading

CRITICAL: When you encounter a file reference (for example `@docs/godot-patterns.md`), load it with the Read tool on a need-to-know basis for the task at hand.

Instructions:

- Do not preemptively load all references.
- When loaded, treat content as mandatory instructions that override defaults.
- Follow references recursively when needed.

## Project Overview

- **Engine**: Godot 4.6 with .NET/C# (primary language is C#)
- **Physics**: Jolt Physics (3D)
- **Renderer**: GL Compatibility (desktop and mobile)
- **Assembly / project name**: `Ludum Dare 59`
- **Root namespace**: `LudumDare59`
- **Target frameworks**: `net8.0` (`net9.0` only when `GodotTargetPlatform == android`)
- **Main scene**: `res://scenes/main.tscn`

## Build / Format / Test Commands

```bash
# Build
dotnet restore
dotnet build
dotnet build -c Release
dotnet clean

# Format
dotnet format "Ludum Dare 59.csproj"                      # apply fixes
dotnet format "Ludum Dare 59.csproj" --verify-no-changes  # CI check

# Code analysis via build-enforced style rules
dotnet build /p:EnforceCodeStyleInBuild=true

# Tests
dotnet test
```

Note: no test project is present yet, so `dotnet test` is mainly useful once tests are added.

## Guidelines

Load these files when working on related tasks:

- C# code style and formatting: `@docs/csharp-guidelines.md`
- Godot node lifecycle, node reference strategy, scene naming: `@docs/godot-patterns.md`
- Entity composition and signal wiring: `@docs/composition-architecture.md`
- Test structure and GdUnit usage: `@docs/testing-guidelines.md`

Deep references (load only when directly relevant):

- High-level architecture and runtime ownership: `@ARCHITECTURE_OVERVIEW.md`
- Gameplay goals and scope: `@GHOST_FREQUENCY_STORM_RELAY_MINI_GDD.md`
- Dependency-ordered implementation plan: `@IMPLEMENTATION_TASK_LIST.md`

## Runtime Entry Points

- `project.godot` sets `res://scenes/main.tscn` as the main scene.
- `scenes/main.tscn` is the composition root and currently instantiates `GameManager`, `RunController`, `SectorRoot`, `PlayerShip`, `Camera2D`, and `HUD`.
- `scripts/systems/GameManager.cs` is a scene node singleton (`static Instance`) and is not configured as an autoload in `project.godot`.

## Architecture Notes

- Code is organized by role under `scripts/`: `components/`, `entities/`, `systems/`, `core/`, `data/`, `interfaces/`.
- Root entity scripts are thin orchestrators: cache child nodes in `_Ready()`, coordinate flow, and delegate behavior to components.
- Components should be single-responsibility and communicate via signals/events rather than direct sibling calls.
- Connect signals in `_Ready()` and disconnect in `_ExitTree()` to avoid dangling handlers.
- Keep hot paths allocation-light and avoid `GetNode<T>()` lookups inside `_Process()` / `_PhysicsProcess()`.

## Godot MCP Tools

`godot-mcp` is configured in `opencode.json`, but it currently points to `/mnt/c/repos/voidline-prospector` instead of this repository.

Until that path is corrected, do not trust live-editor MCP actions against this project.

| Task | Approach |
| --- | --- |
| Create / modify `.tscn` files | MCP scene tools (`godot_write_scene`, `godot_add_node`) |
| Create / modify `.cs` files | Standard file editing (MCP script tools generate GDScript) |
| Validate C# | `dotnet build` |
| After external file changes with editor open | `godot_editor_refresh_filesystem` |

## Repo-Specific Gotchas

- Input actions are created at runtime in `GameManager._Ready()` via `InputMap.AddAction()` and `InputMap.ActionAddEvent()`. They are not persisted in `project.godot`.
- Rendering is set to GL Compatibility (`renderer/rendering_method` and `renderer/rendering_method.mobile`). Keep compatibility in mind for visual features.
- Physics is explicitly configured to Jolt in `project.godot`.

## Git Conventions

- Use focused, atomic commits.
- Keep commit messages descriptive and intent-first (for example `feat: add relay activation hold timer`).
- Do not commit temporary debug logs (`GD.Print`) unless intentionally needed.

## Applied Learning

When something fails repeatedly, when a workaround is discovered, or when an important repo quirk is learned, add a one-line bullet here.

Rules:

- Keep each bullet under 15 words.
- No explanations, only the actionable takeaway.
- Only add items that save time in future sessions.

- Never assume Godot resource UIDs; prefer explicit `path="..."` when uncertain.
