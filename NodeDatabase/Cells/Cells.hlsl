// Created basing on: https://www.shadertoy.com/view/ltjyRm

#define B 3.0  // blobs per cell
#define R  0.5  // jittering blob location. .5 = anywhere in the tile. could be < or >
#define dr 0.5  // blob radius fluctuates in [1-r,1+r]
#define N  3   // tested neighborhood. Make it odd > 3 if R is high 

void Cells_float(float2 v, float cellsCount, out float result)
{    
    float S = cellsCount; // make number of cells integer for tilability
    float bps = B;

    v = v * S;
    float m = 1e9, w, r, r0 = 1e2;
    
    for (int k = 0; k < N * N; k++) // neihborhood
        for (float i = 0; i < bps; i++)
        {
            float2 iU = floor(v);
            float2 g = fmod(iU + float2(k % N, k / N) - 1.0, S); // cell location within neighborhood
            float2 p = g + 0.5 + R * (2.0 * frac(4567.89 * sin(mul(4567.8 * (g + i * 0.1), float2x2(1, -13.17, 377.1, -78.7)))) - 1.0); // blob location
            
            p = fmod(p - v + S / 2.0, S) - S / 2.0; // distance to blob center
            r = 1. + dr * (2.0 * frac(43758.5453 * sin(dot(g + i * 0.1 + 0.1, float2(12.9898, 78.233)))) - 1.0) * 0.35; // blob radius
            
            w = length(p) * 1.15 / r; // distance in blob referential   
            
            if (w < m)
                m = w; // nearest win
        }
    
    result = 1.0 - m * m * 1.5;
}