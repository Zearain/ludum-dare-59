# Live Editor Tool Map

Use this file only when you need the exact current `godot-mcp` live-editor surface. The main skill stays workflow-focused; this file is the low-level reference.

## Connection

```text
godot_connect {}
godot_connection_status {}
godot_disconnect {}
```

Notes:

- Prefer `godot_connect {}` so the active `godot_mcp` configuration chooses the correct host and port.
- `host` defaults to `127.0.0.1` when provided explicitly.
- `port` often defaults to `6550`, but local MCP config may override it.
- Reconnect after Godot restarts or the bridge closes.

## Scene And Editor State

```text
godot_editor_get_project_info {}
godot_editor_get_scene_tree {}
godot_editor_open_scene { "scenePath": "res://Scenes/Game.tscn" }
godot_editor_select_node { "nodePath": "Player/CollisionShape3D" }
godot_editor_save_scene {}
godot_editor_run_scene {}
godot_editor_run_scene { "scenePath": "res://Scenes/TestRoom.tscn" }
godot_editor_stop_scene {}
godot_editor_refresh_filesystem {}
```

## Live Node Editing

```text
godot_editor_add_node {
  "parentPath": ".",
  "name": "HUD",
  "type": "CanvasLayer"
}

godot_editor_modify_node {
  "nodePath": "HUD",
  "properties": {
    "visible": true
  }
}

godot_editor_remove_node {
  "nodePath": "HUD/DebugLabel"
}
```

Important argument names:

- `parentPath` for add-node parent selection
- `nodePath` for select/modify/remove
- `scenePath` for open-scene and optional run-scene targeting

## Typed Property Payloads

The AI Bridge converts structured dictionaries automatically.

### Vector2

```json
{ "_type": "Vector2", "x": 100.0, "y": 200.0 }
```

### Vector3

```json
{ "_type": "Vector3", "x": 1.0, "y": 2.0, "z": 3.0 }
```

### Color

```json
{ "_type": "Color", "r": 1.0, "g": 0.5, "b": 0.0, "a": 1.0 }
```

Example:

```text
godot_editor_modify_node {
  "nodePath": "Player",
  "properties": {
    "position": { "_type": "Vector3", "x": 10, "y": 0, "z": 5 },
    "scale": { "_type": "Vector3", "x": 2, "y": 2, "z": 2 }
  }
}
```

## Runtime Diagnostics

```text
godot_editor_get_errors {
  "includeRuntime": true,
  "includeScript": true,
  "includeLogFile": true,
  "severity": "all",
  "clear": false
}

godot_editor_get_output {
  "lines": 100,
  "level": "all",
  "source": "all",
  "includeMetadata": true,
  "clear": false
}

godot_editor_get_log_file {
  "lines": 200,
  "filter": "all",
  "sinceLine": 0,
  "includeMetadata": true
}

godot_editor_execute_gdscript {
  "code": "return editor_interface.get_edited_scene_root().name"
}
```

Guidance:

- Use `clear: false` unless you are deliberately starting a fresh capture window.
- Prefer short, diagnostic `godot_editor_execute_gdscript` snippets that return data.
- Note: `godot_editor_execute_gdscript` runs GDScript in the editor context. This is valid even in a C# project -- it is useful for editor-side diagnostics and inspection.
- Use `godot_editor_get_log_file` when you need incremental polling or broader context.

## Runtime Automation

```text
godot_runtime_status {}

godot_runtime_wait { "frames": 6 }

godot_runtime_press_action { "action": "move_right", "strength": 1.0 }

godot_runtime_release_action { "action": "move_right" }

godot_runtime_tap_action { "action": "jump", "frames": 1 }

godot_runtime_mouse_move { "x": 120, "y": 180 }

godot_runtime_click { "x": 120, "y": 180, "button": 1, "holdFrames": 1 }

godot_runtime_type_text { "text": "Player One" }

godot_runtime_capture_screenshot { "path": "tmp/runtime-smoke.png" }
```

Guidance:

- `godot_runtime_*` targets the running game process, not the editor tree.
- Call `godot_editor_run_scene` before runtime automation.
- Prefer `godot_runtime_mouse_move` + `godot_runtime_click` for UI interactions.
- Prefer `godot_runtime_tap_action` or press/release pairs for InputMap-driven gameplay.
- `godot_runtime_capture_screenshot` accepts absolute paths plus `res://`, `user://`, and project-relative paths.

## File-Based Tools To Pair With Live Editing

- `godot_read_scene`, `godot_write_scene`
- `godot_add_node`, `godot_modify_node`, `godot_remove_node`
- `godot_list_scene_nodes`, `godot_validate_scene`, `godot_list_scenes`
- `godot_read_shader`, `godot_write_shader`, `godot_generate_shader`
- `godot_read_resource`, `godot_write_resource`
- `godot_ui_*` (theme, menu, HUD, dialog, panel, layout, container tools)
- `godot_animation_*` (clips, keyframes, state machines, blend spaces)
- `godot_input_*` (InputMap actions, presets)
- `godot_audio_*` (bus layouts, players, effects)
- `godot_navigation_*` (regions, agents, links)

For C# scripts, use standard file editing tools and `dotnet build` for validation. The `godot_write_script` and `godot_generate_script` tools generate GDScript.

When the exact tool choice is unclear, call `godot_help`.
