shader_type particles;

// These will both appear in the Inspector
uniform float particle_diameter = 5.0; 
uniform float emission_height = 5.0; // Controls the vertical spawn box

float rand(vec2 co){ 
    return fract(sin(dot(co.xy ,vec2(23.21,101.83))) * 34759.214);
}

void start() {
    CUSTOM.y = 0.0; 

    float x_modifier = (rand(vec2(float(INDEX), 0.0)) * particle_diameter);
    float z_modifier = (rand(vec2(float(INDEX) + 1.0, 0.0)) * particle_diameter);
    
    // Use the new uniform for the Y modifier
    float y_modifier = (rand(vec2(float(INDEX) + 2.0, 0.0)) * emission_height);
    
    TRANSFORM[3][0] = x_modifier - 0.5 * particle_diameter;
    // Apply the Y modifier (centered around the emitter's origin)
    TRANSFORM[3][1] = y_modifier - 0.5 * emission_height; 
    TRANSFORM[3][2] = z_modifier - 0.5 * particle_diameter;
    
    VELOCITY.y = -1.0;
}