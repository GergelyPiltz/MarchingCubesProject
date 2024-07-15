uint valuesPerAxis;

uint indexFromCoord(uint3 coord)
{
    return coord.x * valuesPerAxis * valuesPerAxis + coord.y * valuesPerAxis + coord.z;
}

uint indexFromCoord(uint2 coord)
{
    return coord.x * valuesPerAxis + coord.y;
}