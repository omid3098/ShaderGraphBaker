void Stripes_float(float2 v, float stripesCount, out float result)
{
    float step = 1.0 / stripesCount;
    float stripe = floor(v.y / step);
    result = fmod(stripe, 2.0) < 1.0 ? 1.0 : 0.0;
}