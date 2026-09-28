shader_type spatial;
render_mode cull_disabled;

// --- Inspector Controls ---
group_uniforms AlbedoColors;
uniform vec4 sand_albedo : source_color = vec4(0.82, 0.73, 0.52, 1.0);
uniform vec4 grass_albedo : source_color = vec4(0.32, 0.58, 0.22, 1.0);
uniform vec4 patch_albedo : source_color = vec4(0.72, 0.35, 0.15, 1.0); // Default orange dirt

group_uniforms SurfaceRoughness;
uniform float sand_roughness : hint_range(0.0, 1.0) = 0.9;
uniform float grass_roughness : hint_range(0.0, 1.0) = 0.75;
uniform float patch_roughness : hint_range(0.0, 1.0) = 0.85;

group_uniforms SandGrassBlending;
uniform float sand_height_max : hint_range(-50.0, 50.0) = 2.0;
uniform float height_blend_margin : hint_range(0.01, 10.0) = 1.0;
uniform float min_grass_slope : hint_range(0.0, 1.0) = 0.6;
uniform float slope_blend_margin : hint_range(0.01, 0.5) = 0.1;

group_uniforms OrganicPatches;
uniform float patch_scale : hint_range(0.001, 1.0) = 0.08;
uniform float patch_threshold : hint_range(0.0, 1.0) = 0.55;
uniform float patch_blend_margin : hint_range(0.01, 0.5) = 0.1;

varying vec3 world_position;
varying vec3 world_normal;

// --- 3D Value Noise Generator ---
float hash(vec3 p) {
    p = fract(p * vec3(443.897, 441.423, 437.195));
    p += dot(p, p.yxz + 19.19);
    return fract((p.x + p.y) * p.z);
}

float value_noise(vec3 p) {
    vec3 i = floor(p);
    vec3 f = fract(p);

    // Smoothstep curve for seamless interpolation
    f = f * f * (3.0 - 2.0 * f);

    float n000 = hash(i + vec3(0.0, 0.0, 0.0));
    float n100 = hash(i + vec3(1.0, 0.0, 0.0));
    float n010 = hash(i + vec3(0.0, 1.0, 0.0));
    float n110 = hash(i + vec3(1.0, 1.0, 0.0));
    float n001 = hash(i + vec3(0.0, 0.0, 1.0));
    float n101 = hash(i + vec3(1.0, 0.0, 1.0));
    float n011 = hash(i + vec3(0.0, 1.0, 1.0));
    float n111 = hash(i + vec3(1.0, 1.0, 1.0));

    return mix(
        mix(mix(n000, n100, f.x), mix(n010, n110, f.x), f.y),
        mix(mix(n001, n101, f.x), mix(n011, n111, f.x), f.y),
        f.z
    );
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);
}

void fragment() {
    // 1. Terrain Height & Slope Base Blend (Sand vs Grass)
    float slope = dot(world_normal, vec3(0.0, 1.0, 0.0));
    float height_factor = smoothstep(sand_height_max - height_blend_margin, sand_height_max + height_blend_margin, world_position.y);
    float slope_factor = smoothstep(min_grass_slope - slope_blend_margin, min_grass_slope + slope_blend_margin, slope);

    // Calculate grass weight (0 = Sand, 1 = Grass)
    float grass_weight = height_factor * slope_factor;

    vec3 base_albedo = mix(sand_albedo.rgb, grass_albedo.rgb, grass_weight);
    float base_roughness = mix(sand_roughness, grass_roughness, grass_weight);

    // 2. Organic Patch Calculation with Noise
    float noise = value_noise(world_position * patch_scale);
    float raw_patch = smoothstep(patch_threshold - patch_blend_margin, patch_threshold + patch_blend_margin, noise);

    // 3. Mask patches so they primarily appear on grass, not sand
    float patch_factor = raw_patch * grass_weight;

    // 4. Final Color & Surface Property Interpolation
    vec3 final_albedo = mix(base_albedo, patch_albedo.rgb, patch_factor);
    float final_roughness = mix(base_roughness, patch_roughness, patch_factor);

    // 5. Output Assignment
    ALBEDO = final_albedo;
    ROUGHNESS = final_roughness;
    SPECULAR = 0.0;
}