



//for unpacking colours etc
uint4 Unpack4x8(uint v)
{
    return uint4(v >> 24, v >> 16, v >> 8, v) & 0xFF;
}
float4 Unpack4x8UNF(uint v)
{
    float4 u = (float4)Unpack4x8(v);
    return u * 0.0039215686274509803921568627451f;// u * 1/255
}




