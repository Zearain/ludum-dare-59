---
name: godot-scene-design
description: Design and create Godot 4.x scenes (.tscn files), node hierarchies, and level layouts. Covers scene file format, common 2D/3D patterns, collision layers, groups, instancing, and animation setup.
---

# Godot Scene Design Skill

You are an expert at designing and creating Godot 4.x scenes (.tscn files), node hierarchies, and level layouts. This project uses Godot 4.6 with C# (.NET), Jolt Physics (3D), and targets mobile platforms.

## Scene File Format (TSCN)

Godot uses a text-based scene format that is highly readable and AI-friendly.

### Basic Structure
```ini
[gd_scene load_steps=3 format=3 uid="uid://abc123"]

[ext_resource type="Script" path="res://Characters/Player.cs" id="1_abc"]
[ext_resource type="Texture2D" path="res://Sprites/player.png" id="2_def"]

[sub_resource type="CapsuleShape3D" id="CapsuleShape3D_xyz"]
radius = 0.5
height = 1.8

[node name="Player" type="CharacterBody3D"]
script = ExtResource("1_abc")
_speed = 5.0

[node name="CollisionShape3D" type="CollisionShape3D" parent="."]
shape = SubResource("CapsuleShape3D_xyz")

[node name="MeshInstance3D" type="MeshInstance3D" parent="."]

[connection signal="HealthChanged" from="." to="." method="OnHealthChanged"]
```

### Key Sections
1. **Header**: `[gd_scene load_steps=N format=3]` -- N is total resources + 1
2. **External Resources**: References to other files (scripts, textures, scenes)
3. **Sub-resources**: Inline resources (shapes, materials, curves)
4. **Nodes**: Scene tree structure with parent references
5. **Connections**: Signal connections between nodes

### C# Script References in TSCN
```ini
[ext_resource type="Script" path="res://Characters/Player.cs" id="1_abc"]

[node name="Player" type="CharacterBody3D"]
script = ExtResource("1_abc")
```

Note: C# scripts use `.cs` extension. Exported properties from C# appear with their `_camelCase` field names in the TSCN file (matching the `[Export]` field name).

## Common Scene Patterns

### Player Character (3D -- Primary for this project)
```
Player (CharacterBody3D)
├── CollisionShape3D
├── MeshInstance3D (or imported model)
├── AnimationPlayer
├── AnimationTree
├── Camera3D
│   └── SpringArm3D (for collision avoidance)
├── Marker3D (weapon attachment points)
├── Hitbox (Area3D)
│   └── CollisionShape3D
└── RayCast3D (ground/interaction detection)
```

### Player Character (2D Platformer)
```
Player (CharacterBody2D)
├── Sprite2D
├── CollisionShape2D
├── AnimationPlayer
├── Camera2D
│   └── (smooth follow settings)
├── Hitbox (Area2D)
│   └── CollisionShape2D
└── RayCast2D (ground detection)
```

### Enemy (3D)
```
Enemy (CharacterBody3D)
├── CollisionShape3D
├── MeshInstance3D
├── AnimationPlayer
├── NavigationAgent3D
├── DetectionArea (Area3D)
│   └── CollisionShape3D
├── Hitbox (Area3D)
│   └── CollisionShape3D
└── Timer (attack cooldown)
```

### UI Screen
```
UIScreen (Control)
├── Background (TextureRect/ColorRect)
├── VBoxContainer
│   ├── Title (Label)
│   ├── HSeparator
│   └── ButtonContainer (VBoxContainer)
│       ├── StartButton (Button)
│       ├── OptionsButton (Button)
│       └── QuitButton (Button)
└── AnimationPlayer (transitions)
```

### HUD
```
HUD (CanvasLayer)
└── Control
    ├── MarginContainer
    │   └── HBoxContainer
    │       ├── HealthBar (ProgressBar/TextureProgressBar)
    │       └── ScoreLabel (Label)
    ├── Minimap (SubViewportContainer)
    │   └── SubViewport
    │       └── Camera3D
    └── DialogBox (PanelContainer)
        └── VBoxContainer
            ├── SpeakerLabel
            └── DialogText (RichTextLabel)
```

### Level/World (3D)
```
Level (Node3D)
├── Environment (WorldEnvironment)
├── DirectionalLight3D
├── Geometry (Node3D)
│   ├── Ground (StaticBody3D)
│   │   ├── MeshInstance3D
│   │   └── CollisionShape3D
│   └── Obstacles (Node3D)
├── Entities (Node3D)
│   ├── Player
│   ├── Enemies (Node3D)
│   └── Items (Node3D)
├── NavigationRegion3D
│   └── (navigation mesh)
└── Camera3D (level bounds / cutscene camera)
```

### Inventory Item (3D)
```
Item (Area3D)
├── MeshInstance3D
├── CollisionShape3D (pickup area)
├── AnimationPlayer (bob/glow)
├── OmniLight3D (subtle glow)
└── AudioStreamPlayer3D (pickup sound)
```

## Node Selection Guide

### Physics Bodies
| Node | Use Case |
|------|----------|
| `CharacterBody2D/3D` | Player, enemies, NPCs with custom movement |
| `RigidBody2D/3D` | Physics-driven objects (crates, balls) |
| `StaticBody2D/3D` | Immovable collision (walls, floors) |
| `Area2D/3D` | Triggers, detection zones, pickups |

### Visual Nodes (3D)
| Node | Use Case |
|------|----------|
| `MeshInstance3D` | 3D models |
| `CSGBox3D/Sphere3D/etc` | Prototyping geometry (avoid in production) |
| `MultiMeshInstance3D` | Many identical objects (grass, debris) |
| `GPUParticles3D` | 3D particle effects |
| `CPUParticles3D` | Simpler particle effects (mobile-friendly) |
| `Decal` | Projected textures (scorch marks, shadows) |
| `Sprite3D` | 2D sprites in 3D space |

### Visual Nodes (2D)
| Node | Use Case |
|------|----------|
| `Sprite2D` | Static images |
| `AnimatedSprite2D` | Frame-based animations |
| `TileMapLayer` | Level geometry, backgrounds |
| `Line2D` | Trails, lasers, drawing |
| `Polygon2D` | Custom shapes |
| `GPUParticles2D` | Particle effects |

### UI Nodes
| Node | Use Case |
|------|----------|
| `Control` | Base container |
| `Label/RichTextLabel` | Text display |
| `Button/TextureButton` | Clickable elements |
| `ProgressBar/TextureProgressBar` | Health bars, loading |
| `HBoxContainer/VBoxContainer` | Auto-layout |
| `MarginContainer` | Padding |
| `PanelContainer` | Styled backgrounds |
| `ScrollContainer` | Scrollable content |
| `TabContainer` | Tabbed interfaces |
| `TouchScreenButton` | Mobile on-screen controls |

### Audio Nodes
| Node | Use Case |
|------|----------|
| `AudioStreamPlayer` | Global audio (music, UI) |
| `AudioStreamPlayer2D` | Positional 2D audio |
| `AudioStreamPlayer3D` | Positional 3D audio |

### Utility Nodes
| Node | Use Case |
|------|----------|
| `Timer` | Delayed actions, cooldowns |
| `AnimationPlayer` | Property animations |
| `AnimationTree` | Complex animation state machines |
| `RayCast2D/3D` | Line-of-sight, ground detection |
| `ShapeCast2D/3D` | Swept collision detection |
| `NavigationAgent2D/3D` | Pathfinding |
| `NavigationRegion2D/3D` | Navigation mesh regions |

## Collision Layers Best Practice

```
Layer 1: World (static geometry)
Layer 2: Player
Layer 3: Enemies
Layer 4: Player projectiles
Layer 5: Enemy projectiles
Layer 6: Pickups
Layer 7: Triggers/areas
Layer 8: Interactables
```

Set in scene:
```ini
[node name="Player" type="CharacterBody3D"]
collision_layer = 2
collision_mask = 13
```

Note: Collision layer/mask values in TSCN are bitmask integers. Layer 1 = 1, Layer 2 = 2, Layer 3 = 4, etc. Combine with OR: layers 1+3+4 = 1|4|8 = 13.

## Groups for Organization

Use groups to organize and find nodes:
```ini
[node name="Enemy" type="CharacterBody3D" groups=["enemies", "damageable"]]
```

Common groups:
- `enemies` -- All enemy nodes
- `damageable` -- Anything that can take damage
- `interactable` -- Objects player can interact with
- `persistent` -- Nodes that persist between scenes
- `pausable` -- Nodes affected by pause

In C#:
```csharp
var enemies = GetTree().GetNodesInGroup("enemies");
```

## Scene Instancing

Reference external scenes:
```ini
[ext_resource type="PackedScene" path="res://Scenes/Enemy.tscn" id="enemy_scene"]

[node name="Enemy1" parent="Enemies" instance=ExtResource("enemy_scene")]
position = Vector3(10, 0, 5)

[node name="Enemy2" parent="Enemies" instance=ExtResource("enemy_scene")]
position = Vector3(30, 0, 10)
```

## Property Overrides

Override instanced scene properties:
```ini
[node name="Enemy1" parent="Enemies" instance=ExtResource("enemy_scene")]
position = Vector3(10, 0, 5)
_speed = 3.0
```

## Tips for AI Scene Generation

1. **Start with the root node type** -- Choose based on the scene's purpose (CharacterBody3D for characters, Control for UI, Node3D for levels)
2. **Build hierarchy logically** -- Visual nodes as children of physics bodies, UI elements in containers
3. **Use appropriate collision shapes** -- CapsuleShape3D for characters, BoxShape3D for crates, SphereShape3D for projectiles
4. **Set up layers correctly** -- Avoid "everything collides with everything"
5. **Name nodes descriptively** -- "PlayerSprite" not "Sprite2D", "HealthBar" not "ProgressBar"
6. **Use groups for runtime queries** -- `GetTree().GetNodesInGroup("enemies")`
7. **Instance reusable scenes** -- Don't duplicate complex node trees
8. **Consider processing order** -- Parent nodes process before children
9. **Use CanvasLayer for HUD** -- Keeps UI separate from game camera
10. **For mobile**: Keep node count reasonable, prefer instancing over deep hierarchies
11. **Use `godot_write_scene` or `godot_add_node` MCP tools** for programmatic scene creation
12. **Validate with `godot_validate_scene`** after structural changes
