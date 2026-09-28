shader_type spatial;

uniform vec3 color : source_color = vec3(0.2, 0.6, 0.2); // default green
uniform float roughness = 0.9;

void fragment() {
    ALBEDO = color;
    ROUGHNESS = roughness;
}
