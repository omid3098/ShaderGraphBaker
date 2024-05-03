// Created basing on: https://www.shadertoy.com/view/Md3SzB

float2 hash(float2 p)
{
    p.xy = mul(p.xy, float2x2(127.1, 311.7, 269.5, 183.3));
    p = -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
    return sin(p * 6.283 + 1);
}

void PerlinNoise_float(float2 p, float2 offset, float scale, float smooth, out float result)
{
    p.xy = p.xy + offset;
    p.xy = p.xy * scale;
    
    float2 pi = floor(p);
    float2 pf = p - pi;
    
    float2 w = pf * pf * (3 - 2 * pf);
    
    float f00 = dot(hash(pi + float2(0, 0)), pf - float2(0, 0));
    float f01 = dot(hash(pi + float2(0, 1)), pf - float2(0, 1));
    float f10 = dot(hash(pi + float2(1.0, 0)), pf - float2(1.0, 0));
    float f11 = dot(hash(pi + float2(1.0, 1)), pf - float2(1.0, 1));
    
    float xm1 = lerp(f00, f10, w.x);
    float xm2 = lerp(f01, f11, w.x);
    
    float hasSmooth = smooth > 0;
    float m = hasSmooth * 0.5 + (hasSmooth == 0);
    
    result = lerp(xm1, xm2, w.y) * m + smooth;
}