shader_type particles;

// Note: You must include the random utility file provided in the video description
// Alternatively, you can paste standard godot random/hash functions here.
#include "res://Assets/windshader/random_utils.gdshaderinc" 

uniform vec2 emission_plane_size = vec2(10.0, 10.0);
uniform float min_velocity = 10.0;
uniform float max_velocity = 15.0;

uniform sampler2D noise_texture;
uniform float domain_scale = 0.01;
uniform float min_intensity = 1.0;
uniform float max_intensity = 3.0;

uniform float min_alpha : hint_range(0.0, 1.0) = 0.3;
uniform float max_alpha : hint_range(0.0, 1.0) = 0.8;

void start() {
    // Generate a unique seed for the start function[cite: 1]
    uint seed = hash(NUMBER + RANDOM_SEED);

    // Calculate a random starting position on the emission plane[cite: 1]
    float random_x = (random(seed) - 0.5) * emission_plane_size.x;
    float random_y = (random(seed) - 0.5) * emission_plane_size.y;
    TRANSFORM[3].xyz = vec3(random_x, random_y, 0.0);

    // Save the spawn position to USERDATA1 so it persists across frames[cite: 1]
    USERDATA1 = TRANSFORM[3];

    // Compute a random angle and construct a rotation matrix around the Z-axis[cite: 1]
    float random_angle = random_range(seed, -PI, PI);
    mat3 random_rotation_z = mat3(
        vec3(cos(random_angle), sin(random_angle), 0.0),
        vec3(-sin(random_angle), cos(random_angle), 0.0),
        vec3(0.0, 0.0, 1.0)
    );
    
    // Apply the random rotation[cite: 1]
    TRANSFORM = mat4(random_rotation_z) * TRANSFORM;

    // Multiply by EMISSION_TRANSFORM so particles spawn relative to the emitter[cite: 1]
    TRANSFORM = EMISSION_TRANSFORM * TRANSFORM;
}

void process() {
    // Generate a different seed for the process function to avoid correlation issues[cite: 1]
    uint seed = hash(NUMBER + RANDOM_SEED + 1u);
    
    float velocity = random_range(seed, min_velocity, max_velocity);
    float amplitude = random_range(seed, min_intensity, max_intensity);

    vec3 spawn_position = USERDATA1.xyz;
    vec3 world_position = TRANSFORM[3].xyz;

    // --- LOCAL SPACE CONVERSION ---
    // Compute the position relative to the spawn point[cite: 1]
    vec3 spawn_relative_position = world_position - spawn_position;
    
    // Extract the rotation matrix from the current transform[cite: 1]
    mat3 transform_rotation = mat3(TRANSFORM);
    
    // Use transpose as a performant alternative to inverse() for rotation matrices[cite: 1]
    vec3 local_position = transpose(transform_rotation) * spawn_relative_position;

    vec3 new_local_position = vec3(0.0);

    // Apply the wavy noise texture offsets in local space[cite: 1]
    new_local_position.x = (texture(noise_texture, local_position.yz * domain_scale).r - 0.5) * amplitude;
    new_local_position.y = (texture(noise_texture, local_position.xz * domain_scale).r - 0.5) * amplitude;
    
    // Move the particle forward along the Z axis[cite: 1]
    new_local_position.z = local_position.z + DELTA * velocity;

    // Pack the updated local position back into world space and add the initial spawn position[cite: 1]
    TRANSFORM[3].xyz = (transform_rotation * new_local_position) + spawn_position;

    // Apply random transparency[cite: 1]
    COLOR.a = random_range(seed, min_alpha, max_alpha);
}