---
name: godot-live-edit
description: Lightweight live-editor guidance via the Godot AI Bridge plugin. Covers connection workflow, common live edit operations, property type formatting, and iterative editing patterns for Godot 4.x with C#.
---

# Godot Live Edit Skill

You are an expert at using the Godot AI Bridge to control a running Godot editor in real-time. This skill guides you through live editing workflows. For more advanced persistent inspect/edit/run/debug loops, prefer the `godot-interactive` skill.

This project uses C# (.NET), so scripts are `.cs` files and exported properties follow C# naming conventions.

## Connection Workflow

### Step 1: Connect to Godot
```
Ensure:
1. Godot 4.x is running with the project open
2. The godot_ai_bridge plugin is installed in addons/ and enabled
3. The plugin shows "AI Bridge listening on port 6550" in the Output panel

Then use: godot_connect
```

### Step 2: Verify Connection
```
Use: godot_connection_status

Expected response includes connected: true, port, and project path.
```

### Step 3: Explore the Scene
```
Use: godot_editor_get_scene_tree

This shows all nodes in the current scene with their types and hierarchy.
```

## Common Live Edit Operations

### Adding Nodes
```
godot_editor_add_node
  parentPath: "."
  name: "HealthBar"
  type: "ProgressBar"
  properties: { "value": 100, "max_value": 100 }
```

### Modifying Node Properties
```
godot_editor_modify_node
  nodePath: "Player"
  properties: {
    "position": { "_type": "Vector3", "x": 10, "y": 0, "z": 5 },
    "scale": { "_type": "Vector3", "x": 2, "y": 2, "z": 2 }
  }
```

### Running and Testing
```
1. godot_editor_save_scene    -- Save current work
2. godot_editor_run_scene     -- Run and test
3. godot_editor_get_output    -- Check console output
4. godot_editor_get_errors    -- Check for issues
5. godot_editor_stop_scene    -- Stop when done
```

## Property Type Formatting

When modifying properties via live editing, use these JSON formats:

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

### Simple Values
```json
{ "visible": true, "rotation": 1.57, "_speed": 200.0, "text": "Hello World" }
```

Note: C# exported fields use `_camelCase` naming (e.g., `_speed`, `_baseDamage`).

## Workflow Patterns

### Iterative UI Design
```
1. Connect to editor
2. Get current scene tree
3. Add container nodes (VBoxContainer, HBoxContainer)
4. Add UI elements (Label, Button, ProgressBar)
5. Adjust positions/sizes with modify_node
6. Run scene to preview
7. Iterate until satisfied
8. Save scene
```

### Level Building (3D)
```
1. Connect to editor
2. Add Node3D containers for organization
3. Add StaticBody3D with CollisionShape3D and MeshInstance3D
4. Position with modify_node
5. Run to test gameplay
6. Adjust and iterate
```

### Debugging
```
1. Connect to editor
2. Run scene
3. Use godot_editor_get_errors to check for problems
4. Use godot_editor_get_output for console output
5. Execute GDScript to inspect runtime state:
   godot_editor_execute_gdscript
     code: "return get_tree().current_scene.get_node('Player').position"
6. Stop scene
7. Fix issues in C# code, rebuild with dotnet build
8. Refresh filesystem: godot_editor_refresh_filesystem
9. Rerun scene
```

## Best Practices

1. **Always save before running** -- `godot_editor_save_scene` first
2. **Work incrementally** -- Add one node, verify, then continue
3. **Use descriptive names** -- "PlayerHealthBar" not "ProgressBar2"
4. **Check the scene tree** -- Use `godot_editor_get_scene_tree` to verify structure
5. **Refresh after external edits** -- After C# code changes or file edits, use `godot_editor_refresh_filesystem`
6. **Rebuild C# after code changes** -- Run `dotnet build` before refreshing the editor if you changed `.cs` files
7. **Use undo in the editor** -- All live operations support Ctrl+Z in Godot

## Error Handling

| Error | Cause | Solution |
|-------|-------|----------|
| "Not connected" | Plugin not running | Start Godot, enable plugin |
| "Node not found" | Wrong path | Use `godot_editor_get_scene_tree` to find correct path |
| "No scene open" | No scene loaded | Open a scene in Godot first |
| "Parent not found" | Invalid parent path | Check parent exists in tree |
| C# build errors | Script compilation failed | Run `dotnet build` and fix errors before refreshing editor |

## Scene Management

```
# Open a different scene
godot_editor_open_scene
  scenePath: "res://Scenes/MainMenu.tscn"

# Save current scene
godot_editor_save_scene

# Refresh filesystem after external changes
godot_editor_refresh_filesystem
```
