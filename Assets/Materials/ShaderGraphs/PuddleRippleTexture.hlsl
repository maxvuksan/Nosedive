// Returns a random 2D vector
float2 voronoi_randomVector (float2 UV, float offset) {
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

// GLSL modulus function, as hlsl's "fmod()" uses truncate instead of floor which treats negative values differently.
float2 mod(float2 x, float y){
    return x - y * floor(x/y);
}

// Custom Function for Shader Graph
void Ripples_float(float2 UV, float AngleOffset, float CellDensity, float Time, float Strength, out float Out, out float3 Normal) {
    float2 g = floor(UV * CellDensity); // cells
    float2 f = frac(UV * CellDensity);  // 0-1 UV per cell

    // Initalise outputs
    Out = 0;
    Normal = float3(0, 0, 1);

    // Loop over cell and it's neighbours
    for(int y=-1; y<=1; y++) {
        for(int x=-1; x<=1; x++) {
            // Get distance to random point in cell
            float2 lattice = float2(x,y);
            float2 offset = voronoi_randomVector(mod(lattice + g, CellDensity), AngleOffset);
            float d = distance(lattice + offset, f);
            // Random time offset
            float t = frac(Time + (offset.x * 5));
            // Ripple (greyscale)
            d = (1-d) * (1-d) * Strength * pow(saturate(1 - abs(d - t)), 8) * (sin((d-t) * 30));
            Out = max(Out, -d);
            // Ripple normal vector
            Normal += d * (normalize(float3(normalize((lattice + offset).xy - f), 3))).xyz;
        }
    }
    Normal = normalize(Normal);
}