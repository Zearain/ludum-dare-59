---
name: godot-shader
description: Write Godot 4.x shaders (.gdshader files) for 2D effects, 3D materials, post-processing, and particle systems. Covers shader types, uniforms, built-in variables, and common effect patterns.
---

# Godot Shader Skill

You are an expert at writing Godot 4.x shaders (.gdshader files) for 2D effects, 3D materials, and post-processing. This project uses the Mobile renderer, so keep shader complexity in check for mobile GPU targets.

## Mobile Renderer Considerations

- The Mobile renderer uses a simplified lighting model compared to Forward+
- Avoid excessive texture samples per fragment
- Limit use of `discard` in fragment shaders (breaks early-Z on mobile GPUs)
- Prefer simpler math over texture lookups where possible
- Test on target mobile hardware -- shader performance varies widely
- Screen-space effects are more expensive on mobile

## Shader Types

```glsl
shader_type canvas_item;    // 2D sprites, UI, CanvasItem nodes
shader_type spatial;        // 3D materials
shader_type particles;      // GPU particle systems
shader_type sky;            // Sky/environment
shader_type fog;            // Volumetric fog
```

## Basic Shader Structure

```glsl
shader_type canvas_item;

// Render modes (optional)
render_mode unshaded, blend_mix;

// Uniforms (exposed to inspector)
uniform vec4 tint_color : source_color = vec4(1.0);
uniform float intensity : hint_range(0.0, 1.0) = 0.5;
uniform sampler2D noise_texture : hint_default_white;

// Varyings (pass data vertex -> fragment)
varying vec2 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    COLOR = tex_color * tint_color;
}
```

## Uniform Hints

```glsl
// Color picker in inspector
uniform vec4 color : source_color = vec4(1.0);

// Slider with range
uniform float value : hint_range(0.0, 1.0, 0.01) = 0.5;

// Texture hints
uniform sampler2D albedo_tex : source_color, filter_linear_mipmap;
uniform sampler2D normal_tex : hint_normal;
uniform sampler2D noise_tex : hint_default_white;
uniform sampler2D mask_tex : hint_default_black;

// Instance uniforms (per-instance values)
instance uniform vec4 instance_color : source_color = vec4(1.0);
```

## Built-in Variables

### Canvas Item (2D)
```glsl
// Vertex shader
VERTEX          // vec2 - vertex position
UV              // vec2 - texture coordinates
COLOR           // vec4 - vertex color

// Fragment shader
UV              // vec2 - interpolated UVs
SCREEN_UV       // vec2 - screen space UVs
FRAGCOORD       // vec4 - fragment coordinates
COLOR           // vec4 - output color
TEXTURE         // sampler2D - sprite texture
TEXTURE_PIXEL_SIZE // vec2 - texel size

// Both
TIME            // float - shader time
PI              // float - 3.14159...
TAU             // float - 6.28318...
```

### Spatial (3D)
```glsl
// Vertex shader
VERTEX          // vec3 - vertex position (local)
NORMAL          // vec3 - vertex normal
UV              // vec2 - primary UVs
UV2             // vec2 - secondary UVs
COLOR           // vec4 - vertex color

// Fragment shader
ALBEDO          // vec3 - base color (output)
ALPHA           // float - transparency (output)
METALLIC        // float - metalness (output)
ROUGHNESS       // float - roughness (output)
EMISSION        // vec3 - emissive color (output)
NORMAL_MAP      // vec3 - normal map (output)
NORMAL_MAP_DEPTH // float - normal map strength
AO              // float - ambient occlusion (output)
RIM             // float - rim lighting (output)
RIM_TINT        // float - rim tint amount
```

## Common 2D Shader Patterns

### Flash/Hit Effect
```glsl
shader_type canvas_item;

uniform vec4 flash_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float flash_amount : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    COLOR = mix(tex_color, flash_color, flash_amount);
    COLOR.a = tex_color.a;
}
```

### Outline
```glsl
shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float outline_width : hint_range(0.0, 10.0) = 1.0;

void fragment() {
    vec2 size = TEXTURE_PIXEL_SIZE * outline_width;

    float outline = texture(TEXTURE, UV + vec2(-size.x, 0)).a;
    outline += texture(TEXTURE, UV + vec2(size.x, 0)).a;
    outline += texture(TEXTURE, UV + vec2(0, -size.y)).a;
    outline += texture(TEXTURE, UV + vec2(0, size.y)).a;
    outline = min(outline, 1.0);

    vec4 tex_color = texture(TEXTURE, UV);
    COLOR = mix(outline_color * outline, tex_color, tex_color.a);
}
```

### Dissolve Effect
```glsl
shader_type canvas_item;

uniform sampler2D dissolve_texture : hint_default_white;
uniform float dissolve_amount : hint_range(0.0, 1.0) = 0.0;
uniform float edge_width : hint_range(0.0, 0.2) = 0.05;
uniform vec4 edge_color : source_color = vec4(1.0, 0.5, 0.0, 1.0);

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    float noise = texture(dissolve_texture, UV).r;

    float edge = smoothstep(dissolve_amount, dissolve_amount + edge_width, noise);
    float alpha = step(dissolve_amount, noise);

    COLOR = mix(edge_color, tex_color, edge);
    COLOR.a = tex_color.a * alpha;
}
```

### Pixelate
```glsl
shader_type canvas_item;

uniform float pixel_size : hint_range(1.0, 100.0) = 4.0;

void fragment() {
    vec2 grid_uv = round(UV * pixel_size) / pixel_size;
    COLOR = texture(TEXTURE, grid_uv);
}
```

### Wave/Distortion
```glsl
shader_type canvas_item;

uniform float wave_amplitude : hint_range(0.0, 0.1) = 0.02;
uniform float wave_frequency : hint_range(0.0, 50.0) = 10.0;
uniform float wave_speed : hint_range(0.0, 10.0) = 2.0;

void fragment() {
    vec2 uv = UV;
    uv.x += sin(uv.y * wave_frequency + TIME * wave_speed) * wave_amplitude;
    COLOR = texture(TEXTURE, uv);
}
```

## Common 3D Shader Patterns

### Basic PBR Override
```glsl
shader_type spatial;

uniform vec4 albedo_color : source_color = vec4(1.0);
uniform sampler2D albedo_texture : source_color, hint_default_white;
uniform float metallic : hint_range(0.0, 1.0) = 0.0;
uniform float roughness : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    vec4 tex = texture(albedo_texture, UV);
    ALBEDO = tex.rgb * albedo_color.rgb;
    METALLIC = metallic;
    ROUGHNESS = roughness;
    ALPHA = tex.a * albedo_color.a;
}
```

### Fresnel/Rim Lighting
```glsl
shader_type spatial;

uniform vec4 rim_color : source_color = vec4(1.0);
uniform float rim_power : hint_range(0.0, 10.0) = 3.0;

void fragment() {
    float fresnel = pow(1.0 - dot(NORMAL, VIEW), rim_power);
    EMISSION = rim_color.rgb * fresnel * rim_color.a;
}
```

### Triplanar Mapping
```glsl
shader_type spatial;

uniform sampler2D texture_albedo : source_color;
uniform float blend_sharpness : hint_range(1.0, 10.0) = 3.0;

varying vec3 world_position;
varying vec3 world_normal;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = (MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz;
}

void fragment() {
    vec3 blending = pow(abs(world_normal), vec3(blend_sharpness));
    blending /= blending.x + blending.y + blending.z;

    vec3 x_proj = texture(texture_albedo, world_position.yz).rgb;
    vec3 y_proj = texture(texture_albedo, world_position.xz).rgb;
    vec3 z_proj = texture(texture_albedo, world_position.xy).rgb;

    ALBEDO = x_proj * blending.x + y_proj * blending.y + z_proj * blending.z;
}
```

### Hologram Effect
```glsl
shader_type spatial;
render_mode cull_disabled, depth_draw_opaque;

uniform vec4 hologram_color : source_color = vec4(0.0, 1.0, 1.0, 1.0);
uniform float scan_speed : hint_range(0.0, 10.0) = 2.0;
uniform float scan_line_count : hint_range(10.0, 200.0) = 50.0;
uniform float flicker_speed : hint_range(0.0, 20.0) = 5.0;

void fragment() {
    float scan = sin((UV.y + TIME * scan_speed) * scan_line_count) * 0.5 + 0.5;
    float flicker = sin(TIME * flicker_speed) * 0.1 + 0.9;
    float fresnel = pow(1.0 - dot(NORMAL, VIEW), 2.0);

    ALBEDO = hologram_color.rgb;
    EMISSION = hologram_color.rgb * (scan * 0.5 + 0.5) * flicker;
    ALPHA = (fresnel + 0.3) * scan * flicker * hologram_color.a;
}
```

## Post-Processing (Screen Shaders)

Apply to ColorRect covering viewport:

### Vignette
```glsl
shader_type canvas_item;

uniform float vignette_intensity : hint_range(0.0, 1.0) = 0.4;
uniform float vignette_opacity : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    vec4 color = texture(TEXTURE, UV);

    float vignette = UV.x * UV.y * (1.0 - UV.x) * (1.0 - UV.y);
    vignette = clamp(pow(16.0 * vignette, vignette_intensity), 0.0, 1.0);

    color.rgb = mix(color.rgb, color.rgb * vignette, vignette_opacity);
    COLOR = color;
}
```

## Particle Shaders

```glsl
shader_type particles;

uniform float spread : hint_range(0.0, 180.0) = 45.0;
uniform float initial_speed : hint_range(0.0, 100.0) = 10.0;

void start() {
    float angle = radians(spread) * (RANDOM.x - 0.5);
    vec3 direction = vec3(sin(angle), cos(angle), 0.0);
    VELOCITY = direction * initial_speed;
}

void process() {
    VELOCITY.y -= 9.8 * DELTA;
    COLOR.a = 1.0 - LIFETIME;
    TRANSFORM[0].x = 1.0 - LIFETIME * 0.5;
    TRANSFORM[1].y = 1.0 - LIFETIME * 0.5;
}
```

## MCP Tool Usage

Use these `godot-mcp` tools for shader workflows:

- `godot_write_shader` -- Create or update a `.gdshader` file
- `godot_read_shader` -- Read and inspect an existing shader with parsed uniforms
- `godot_generate_shader` -- Generate a shader from a preset name or description (supports 11 built-in presets: flash, outline, dissolve, pixelate, wave, gradient_map, chromatic_aberration, vignette, crt, hologram, fresnel)
- `godot_list_shaders` -- List all `.gdshader` files in the project

## Tips

1. **Start simple** -- Begin with basic structure, add complexity iteratively
2. **Use hints** -- `hint_range`, `source_color` make shaders usable in the inspector
3. **Provide defaults** -- Always set sensible default values for uniforms
4. **Consider mobile performance** -- Avoid expensive operations in fragment shaders; fewer texture samples is better
5. **Match shader type to use case** -- `canvas_item` for 2D, `spatial` for 3D
6. **Use built-ins** -- `TIME`, `UV`, `TEXTURE` are optimized
7. **Validate with `godot_validate_scene`** -- After attaching shaders to materials in scenes
8. **The `godot_generate_shader` preset list** covers the most common effects; use it before writing from scratch
