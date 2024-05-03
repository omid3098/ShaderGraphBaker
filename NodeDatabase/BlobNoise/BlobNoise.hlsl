// Created basing on: https://www.shadertoy.com/view/4ljSW3

#define PI 3.1415926

// Hash function from: https://www.shadertoy.com/view/Md3SzB
float2 hash(float2 p)
{
    p.xy = mul(p.xy, float2x2(127.1, 311.7, 269.5, 183.3));
    p = -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
    return sin(p * 6.283 + 1);
}

float simplegridnoise(float2 v)
{
    float s = 1 / 256;
    float2 fl = floor(v), fr = frac(v);
    float mindist = 1e9;
    
    for (int y = -1; y <= 1; y++)
        for (int x = -1; x <= 1; x++)
        {
            float2 offset = float2(x, y);
            float2 pos = 0.5 + 0.5 * cos(2. * PI * (6.1 * 0.1 + hash(fl + offset)) + float2(0, 1.6));
            mindist = min(mindist, length(pos + offset - fr));
        }
    
    return mindist;
}

void BlobNoise_float(float2 v, float scale, out float result)
{
    result = pow(0.5 + 0.5 * cos(PI * clamp(simplegridnoise(v * scale) * 2, 0, 1)), 5);
}