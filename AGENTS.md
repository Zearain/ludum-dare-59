# AGENTS.md

## Commands
- Build C# with `dotnet build` from the repo root.
- Run tests with `dotnet test`. No test project is present yet, so this is only useful once tests are added.
- Run the game from the Godot 4.6 editor by opening `project.godot` and pressing `F5`.

## Entry Points
- `project.godot` sets `res://scenes/main.tscn` as the main scene.
- `scenes/main.tscn` is the real composition root: it instantiates `GameManager`, `Camera3D`, `PlayerCruiser`, and three `FighterShip` enemies directly.
- `scripts/systems/GameManager.cs` is a scene node with a static `Instance`, not an autoload configured in `project.godot`.

## Architecture
- Code is organized by role under `scripts/`: `components/`, `entities/`, `systems/`, `core/`, `data/`, `interfaces/`.
- Entities are thin orchestrators that cache child components in `_Ready()` and delegate behavior to them. `PlayerCruiser` is the clearest example.
- State handling is a pure C# helper in `scripts/core/StateMachine.cs`; owner nodes forward both `_Process()` and `_PhysicsProcess()` into it.
- Components and projectiles connect Godot signals in `_Ready()` and disconnect them in `_ExitTree()`; preserve that pattern when editing signal-driven code.

## Repo-Specific Gotchas
- Input actions are created at runtime in `GameManager._Ready()` via `InputMap.AddAction()` / `ActionAddEvent()`. They are not defined in `project.godot`.
- `project.godot` explicitly uses Jolt Physics 3D and GL Compatibility rendering; keep mobile/web-targeted changes compatible with that renderer.
- `Ludum Dare 59.csproj` targets `net8.0`, with a conditional `net9.0` target only for Android builds.
- `opencode.json` currently points the `godot-mcp` server at `/mnt/c/repos/voidline-prospector`, not this repo. Do not trust live Godot MCP/editor actions until that path is fixed.

## Style Sources
- Follow `.editorconfig` for formatting: 4-space C#, file-scoped namespaces, `System` usings first with separated import groups.
- `CLAUDE.md` has the project-specific C# / Godot conventions worth preserving, especially cached `GetNode<T>()`, signal cleanup in `_ExitTree()`, and composition-over-inheritance.
