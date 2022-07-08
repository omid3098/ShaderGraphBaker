inline float2 voronoi_noise_randomVector (float2 UV, float offset){
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}
 
void Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells) {
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float2 res = float2(8.0, 8.0);
    float2 ml = float2(0,0);
    float2 mv = float2(0,0);
 
    for(int y=-1; y<=1; y++){
        for(int x=-1; x<=1; x++){
            float2 lattice = float2(x, y);
            float2 offset = voronoi_noise_randomVector(g + lattice, AngleOffset);
            float2 v = lattice + offset - f;
            float d = dot(v, v);
 
            if(d < res.x){
                res.x = d;
                res.y = offset.x;
                mv = v;
                ml = lattice;
            }
        }
    }
 
    Cells = res.y;
 
    res = float2(8.0, 8.0);
    for(int y=-2; y<=2; y++){
        for(int x=-2; x<=2; x++){
            float2 lattice = ml + float2(x, y);
            float2 offset = voronoi_noise_randomVector(g + lattice, AngleOffset);
            float2 v = lattice + offset - f;
 
            float2 cellDifference = abs(ml - lattice);
            if (cellDifference.x + cellDifference.y > 0.1){
                float d = dot(0.5*(mv+v), normalize(v-mv));
                res.x = min(res.x, d);
            }
        }
    }
 
    Out = res.x;
}