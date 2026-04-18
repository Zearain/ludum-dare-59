---
name: godot-interactive
description: Persistent godot-mcp and AI Bridge workflows for Godot 4.x C# projects. Use this skill for .tscn, .cs, .gdshader, .tres, and live editor tasks that need scene inspection, iterative node edits, refresh/run/test loops, runtime diagnostics, or hybrid file-plus-editor validation.
---

# Godot Interactive

Use a persistent `godot-mcp` session to work on this Godot 4.6 C# project without rediscovering the editor state every turn. This skill keeps one live editor connection alive when possible, combines it with file-based scene/script/shader tools, and requires explicit validation evidence before signoff.

This project uses **C# (.NET)**, not GDScript. Scripts are `.cs` files, validated with `dotnet build`, and follow the conventions in `AGENTS.md`. GDScript knowledge is still relevant for editor-side diagnostics via `godot_editor_execute_gdscript`.

Prefer the smallest evidence-producing loop. For live-editor tasks, stay on the live `godot_editor_*` and `godot_runtime_*` surface unless a file-only detail is truly required.

## Preconditions

- `godot-mcp` must be configured in `opencode.json` for this project.
- For live editing, Godot 4.x must be running with the project open and the AI Bridge plugin enabled (installed at `addons/godot_ai_bridge/`).
- The bridge target should come from the active `godot-mcp` configuration. `127.0.0.1:6550` is the default.
- If tool choice is unclear, call `godot_help` first.
- If the bridge is unavailable, continue with file-based tools and manual Godot review instead of inventing live-editor results.
- Treat a dropped bridge connection as a session problem. Reconnect before doing more live-editor work.

## Workflow Decision Tree

- Use the **live-editor workflow** for node hierarchy changes, layout tuning, quick property edits, opening/running scenes, and immediate runtime inspection.
- Use the **file-based workflow** for larger structural edits, generated scenes/scripts/shaders/resources, or any work done before Godot is open.
- Use the **runtime-automation workflow** whenever dedicated `godot_runtime_*` tools are available. These target the running game process, not the editor.
- Use the **hybrid workflow** for most real tasks: edit files, refresh the editor, inspect the scene tree, run the scene, collect output/errors, and iterate.

## Core Workflow

1. **Build a QA inventory** before changing anything.
   - Combine the user's requirements, the behaviors you implement, and the claims you expect to make in the final response.
   - For each claim, decide what evidence will prove it: scene tree state, `dotnet build` output, runtime output/errors, manual in-engine verification, or a saved screenshot.
   - Add at least 2 off-happy-path checks for fragile gameplay, scene state, or editor synchronization.

2. **Confirm the active project and current scene state.**
   - Prefer `godot_connection_status`, `godot_editor_get_project_info`, and `godot_editor_get_scene_tree`.
   - If no live session exists yet, decide whether to start with file-based edits or connect immediately.

3. **Inspect before editing.**
   - For files: use `godot_read_scene`, `godot_list_scene_nodes`, `godot_read_script`, `godot_read_shader`, or `godot_read_resource`.
   - For live scenes: use `godot_editor_get_scene_tree` and optionally `godot_editor_select_node`.
   - Prefer `godot_list_scene_nodes` before `godot_read_scene` for file-based checks.
   - Do not read the full `.tscn` during a live task unless the live tree cannot answer the question.

4. **Make the smallest meaningful change.**
   - Prefer incremental edits over large speculative rewrites.
   - Keep node names and paths stable when possible.

5. **Refresh or save based on how the change was made.**
   - External file edits (including `dotnet build` output) while Godot is open: `godot_editor_refresh_filesystem`.
   - Live scene edits you intend to keep: `godot_editor_save_scene`.

6. **Validate before running.**
   - Use `godot_validate_scene` on touched scenes.
   - For C# scripts: run `dotnet build` instead of `godot_validate_script` (which validates GDScript).
   - Use `godot_help` or docs tools if a class/property choice is still uncertain.

7. **Run and inspect.**
   - Use `godot_editor_run_scene`.
   - Inspect `godot_editor_get_errors`, `godot_editor_get_output`, and when needed `godot_editor_get_log_file`.
   - Use `godot_runtime_*` tools for gameplay input, runtime waits, and screenshot capture.
   - After a live node add, prefer `godot_editor_modify_node` or structured `properties` on `godot_editor_add_node` before reaching for `godot_editor_execute_gdscript`.

8. **Update the QA inventory** if exploration reveals new states, controls, failure modes, or visible claims.

9. **Sign off only from explicit evidence**, and note any remaining manual verification gaps.

## Start Or Reuse Live Session

Keep the same bridge connection alive across iterations when possible.

Typical attach sequence:
```
godot_connect {}
godot_connection_status {}
godot_editor_get_project_info {}
godot_editor_get_scene_tree {}
```

Rules:
- Reuse the existing connection instead of reconnecting on every turn.
- If Godot was restarted, the project changed, or the bridge timed out, reconnect and reacquire the scene tree.
- If a node path changed, rerun `godot_editor_get_scene_tree` instead of guessing.

## File-Based Workflow

Use file-based tools when the bridge is unavailable or when broad edits are easier outside the editor.

Common loop:
1. Read the current artifact.
2. Edit with the appropriate `godot_*` file tool.
3. Validate the changed file (scene: `godot_validate_scene`; C# script: `dotnet build`).
4. If Godot is open, call `godot_editor_refresh_filesystem`.
5. Open or rerun the scene in the editor to verify behavior.

Typical pairings:
- Scene structure: `godot_read_scene`, `godot_add_node`, `godot_modify_node`, `godot_validate_scene`
- C# Scripts: read/write `.cs` files with standard file tools, validate with `dotnet build`
- Shaders: `godot_read_shader`, `godot_write_shader`
- Resources: `godot_read_resource`, `godot_write_resource`
- UI generation: `godot_ui_*` tools followed by validation and a live run pass

## C# Script Workflow

Since this project uses C# instead of GDScript:

1. **Write or edit `.cs` files** using standard file editing tools (not `godot_write_script` which generates GDScript).
2. **Build with `dotnet build`** to validate compilation.
3. **Refresh the editor**: `godot_editor_refresh_filesystem` so Godot picks up the new/changed scripts.
4. **Attach scripts to nodes** either by editing the `.tscn` file (adding `script = ExtResource(...)`) or through the live editor.
5. **Test by running**: `godot_editor_run_scene` and inspect output/errors.

The `godot_generate_script` and `godot_write_script` tools generate GDScript -- do not use them for this project unless you specifically need a GDScript utility or editor tool script.

## Refresh And Rerun Decision

- External `.tscn`, `.cs`, `.gdshader`, `.tres`, or `project.godot` changes while the editor is open: run `godot_editor_refresh_filesystem`.
- C# code changes: run `dotnet build` first, then refresh.
- Live scene changes you want persisted: run `godot_editor_save_scene`.
- Runtime behavior changes: rerun the scene.
- Bridge disconnect or editor restart: reconnect and reacquire state.
- Stale node paths: reread the scene tree.

## Runtime Automation Workflow

Use `godot_runtime_*` tools when the scene is running:

- `godot_runtime_status` -- Check if the runtime harness is ready
- `godot_runtime_wait` -- Wait for frames in the running game
- `godot_runtime_press_action` / `godot_runtime_release_action` -- Hold/release InputMap actions
- `godot_runtime_tap_action` -- Tap an action for a few frames
- `godot_runtime_mouse_move` / `godot_runtime_click` -- UI/pointer interactions
- `godot_runtime_type_text` -- Type into focused controls
- `godot_runtime_capture_screenshot` -- Capture the viewport to PNG

Rules:
- Call `godot_editor_run_scene` before using runtime tools.
- `godot_runtime_*` targets the running game process, not the editor tree.
- Pair screenshots with logs or state assertions for behavioral claims.

## Checklists

### Session Loop
- Keep one live connection alive when the editor is available.
- Reinspect current state before a new edit burst.
- Make one coherent change set.
- Refresh or save based on where the change happened.
- Validate touched files (scenes with `godot_validate_scene`, C# with `dotnet build`).
- Run the relevant scene.
- Inspect runtime output and errors.

### Signoff
- Coverage is explicit against the QA inventory.
- Touched scenes and scripts were validated.
- The final scene tree or file structure was rechecked after edits.
- A runtime pass was completed for behavior-affecting changes.
- The response clearly distinguishes between validated behavior and assumptions.
- Any missing evidence is called out explicitly.

## Common Failure Modes

- `godot_connect` times out: Godot is not running, the AI Bridge plugin is disabled, or the port is wrong.
- `Not connected to Godot editor`: reconnect before using `godot_editor_*` tools.
- `Node not found`: reacquire the scene tree and verify the path.
- External edits do not appear in the editor: run `godot_editor_refresh_filesystem`.
- C# compilation errors: run `dotnet build` to get detailed error output, fix, then refresh.
- Runtime automation tools time out: make sure the scene is actually running and the runtime harness autoload is loaded.

## References

Read [references/live-editor-tool-map.md](references/live-editor-tool-map.md) for current tool names and common payload shapes.
Read [references/fast-probe-presets.md](references/fast-probe-presets.md) for fast temporary UI targets for runtime verification.
Read [references/runtime-automation-extension-points.md](references/runtime-automation-extension-points.md) for current runtime tool limits and recommended harness patterns.
