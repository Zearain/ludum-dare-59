# Runtime Automation Extension Points

Use this file when a task asks for Playwright-like interaction, screenshot evidence, or repeatable gameplay checks.

## Hard Boundary

`godot-interactive` is a skill layered on top of transports.

- A skill can coordinate existing MCP tools and project workflows.
- A skill cannot by itself create new runtime transport primitives that the MCP or project does not already expose.
- `godot_editor_execute_gdscript` runs in the editor/plugin process, not as a general command channel into the running game process.

## What Exists Today

Current `godot-mcp` (alexmeckes fork) is strong at:

- Scene inspection and live editor edits
- Open/save/run/stop loops
- Runtime output and error capture
- Editor-side GDScript diagnostics
- Runtime input automation and screenshot capture through `godot_runtime_*`

Current gaps:

- Project-specific state snapshots
- Reset hooks for deterministic test setup
- Drag-and-drop and richer pointer semantics
- Multi-window focus and advanced OS-level interactions

## What The Skill Should Prefer

When runtime automation is needed, prefer these sources in order:

1. Dedicated `godot_runtime_*` MCP tools when they fit the task.
2. A project-provided runtime automation harness or deterministic test runner for project-specific state/setup.
3. Manual in-engine verification.

Do not present editor-only evidence as if it were runtime automation evidence.

## Current MCP Runtime Tool Set

- `godot_runtime_status`
- `godot_runtime_wait`
- `godot_runtime_press_action`
- `godot_runtime_release_action`
- `godot_runtime_tap_action`
- `godot_runtime_mouse_move`
- `godot_runtime_click`
- `godot_runtime_type_text`
- `godot_runtime_capture_screenshot`

Future useful additions would be:

- `godot_runtime_get_state`
- `godot_runtime_reset_test_state`
- Richer pointer gestures (drag-and-drop)

## C# Testing Integration

For this project, GdUnit4 is available for automated testing:

```bash
# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~MyTestClass"
```

Consider using GdUnit4 tests as a complement to runtime automation for verifying game logic without needing the editor bridge.

## Evidence Rules

For automation-backed claims:

- Pair input commands with explicit postconditions.
- Pair screenshots with logs, scene state, or structured runtime state.
- Prefer deterministic test scenes and seeded content when possible.
- If the project lacks a runtime harness, say so plainly and fall back to manual verification.
