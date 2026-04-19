shader_type canvas_item;
render_mode unshaded, blend_disabled;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;

uniform vec4 line_color : source_color = vec4(0.95, 0.2, 0.85, 1.0);
uniform float line_density : hint_range(60.0, 600.0, 1.0) = 280.0;
uniform float line_strength : hint_range(0.0, 1.0, 0.01) = 0.13;
uniform float scan_speed : hint_range(0.0, 4.0, 0.01) = 0.45;

uniform float chroma_offset_px : hint_range(0.0, 4.0, 0.01) = 1.25;
uniform float chroma_wobble : hint_range(0.0, 2.0, 0.01) = 0.4;
uniform float chroma_speed : hint_range(0.0, 4.0, 0.01) = 1.1;

uniform float vignette_strength : hint_range(0.0, 1.0, 0.01) = 0.45;
uniform float vignette_inner : hint_range(0.0, 1.5, 0.01) = 0.35;
uniform float vignette_outer : hint_range(0.0, 1.5, 0.01) = 0.75;

uniform float noise_strength : hint_range(0.0, 0.25, 0.001) = 0.03;
uniform float noise_speed : hint_range(0.0, 20.0, 0.01) = 6.0;

float random(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 pixel_size = SCREEN_PIXEL_SIZE;

    float chroma_shift_px = chroma_offset_px + sin(TIME * chroma_speed) * chroma_wobble;
    vec2 chroma_offset_uv = vec2(chroma_shift_px * pixel_size.x, 0.0);

    vec3 base = texture(screen_texture, uv).rgb;
    float r = texture(screen_texture, uv + chroma_offset_uv).r;
    float g = base.g;
    float b = texture(screen_texture, uv - chroma_offset_uv).b;
    vec3 crt = vec3(r, g, b);

    float scan_wave = sin((uv.y + TIME * scan_speed) * line_density * TAU);
    float scan_mask = 1.0 - ((scan_wave * 0.5 + 0.5) * line_strength);
    crt *= scan_mask;
    crt += line_color.rgb * line_strength * 0.15;

    vec2 centered = uv * 2.0 - 1.0;
    float dist = length(centered);
    float vignette = smoothstep(vignette_inner, vignette_outer, dist);
    crt *= mix(1.0, 1.0 - vignette_strength, vignette);

    float noise = random(uv * vec2(960.0, 540.0) + vec2(TIME * noise_speed, TIME * (noise_speed * 0.37)));
    crt += (noise - 0.5) * noise_strength;

    COLOR = vec4(clamp(crt, 0.0, 1.0), 1.0);
}
