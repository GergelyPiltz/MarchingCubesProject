using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;


// Leave integral type on default int. causes less confusion.
// Mathf.Pow(2, (int)chunkSize);
// Always specify all Enum integers explicitly !!!
public enum ChunkSize
{
    _8x8 = 3,
    _16x16 = 4,
    _32x32 = 5,
    _64x64 = 6,
    _128x128 = 7,
    _256x256 = 8,
}
// It is both possible to have 2^x cubes or values. If needed refactor to whatever fits better. 
// If 2^x values then  + 1 + (LOD * 2)

public class Chunk
{
    readonly GameObject chunkObject;
    readonly Vector3Int chunkPosition;

    readonly MeshFilter meshFilter;
    readonly MeshCollider meshCollider;
    readonly MeshRenderer meshRenderer;

    List<Vector3> vertices;
    List<int> triangles;

    readonly ChunkSize chunkSize;

    readonly int cubesX = 16;
    readonly int cubesY = 256;
    readonly int cubesZ = 16;

    readonly int valuesX = 17;
    readonly int valuesY = 257;
    readonly int valuesZ = 17;

    readonly float terrainHeight = 0f;

    bool smoothTerrain = true;
    bool meshSharedVertices = true;
    bool cubeSharedVertices = true;
    int LOD = 1;
    readonly bool enableLOD = true;

    bool dataAvailable = false;

    float[,] noiseMap;
    float[,,] terrainData;
    int[,,,] vertexIndexArray;

    double time = 0f;
    int totalVertexCount = 0;
    int uniqueVertexCount = 0;
    long meshMemoryUsage = 0;

    readonly int chunkIndex = 0;

    public Chunk(Vector3Int _position, ChunkSize _chunkSize, Transform _parent, int _index)
    {
        chunkObject = new GameObject();
        chunkObject.transform.parent = _parent;
        chunkIndex = _index;
        chunkObject.name = "Chunk(" + chunkIndex + ")";
        chunkPosition = _position;
        chunkObject.transform.position = chunkPosition;

        chunkSize = _chunkSize;
        if (chunkSize == 0) chunkSize = ChunkSize._16x16;
        Vector3Int dimensions = new(
            (int)Mathf.Pow(2, (int)chunkSize),
            256,
            (int)Mathf.Pow(2, (int)chunkSize)
            );

        //Vector3Int dimensions = chunkSize switch
        //{
        //    ChunkSize._7x7     => new(7, chunkHeight, 7),
        //    ChunkSize._15x15   => new(15, chunkHeight, 15),
        //    ChunkSize._31x31   => new(31, chunkHeight, 31),
        //    ChunkSize._63x63   => new(63, chunkHeight, 63),
        //    ChunkSize._127x127 => new(127, chunkHeight, 127),
        //    ChunkSize._255x255 => new(255, chunkHeight, 255),
        //    _                  => new(15, chunkHeight, 15),
        //};

        cubesX = dimensions.x;
        cubesY = dimensions.y;
        cubesZ = dimensions.z;

        valuesX = cubesX + 1;
        valuesY = cubesY + 1;
        valuesZ = cubesZ + 1;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");

        noiseMap = new float[valuesX, valuesZ];
        terrainData = new float[valuesX, valuesY, valuesZ];
        vertexIndexArray = new int[cubesX, cubesY, cubesZ, 12];
        ResetVertexIndexArray();

        vertices = new();
        triangles = new();

    }

    // This constructor is for debugging and benchmarking only
    public Chunk(Vector3Int _position, Vector3Int _dimensions, Transform _parent, int _index)
    {
        chunkObject = new GameObject();
        chunkObject.transform.parent = _parent;
        chunkObject.name = "Chunk(" + _index + ")";
        chunkPosition = _position;
        chunkObject.transform.position = chunkPosition;


        cubesX = _dimensions.x;
        cubesY = _dimensions.y;
        cubesZ = _dimensions.z;

        valuesX = cubesX + 1;
        valuesY = cubesY + 1;
        valuesZ = cubesZ + 1;

        enableLOD = false;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");

        noiseMap = new float[valuesX, valuesZ];
        terrainData = new float[valuesX, valuesY, valuesZ];
        vertexIndexArray = new int[cubesX, cubesY, cubesZ, 12];
        ResetVertexIndexArray();

        vertices = new();
        triangles = new();

    }

    ~Chunk()
    {
        Debug.Log("chunk destructor called");
        GameObject.Destroy(chunkObject);
    }

    public void GenerateTerrain(GenerationParams generationParams, MeshConstructParams meshConstructParams)
    {
        smoothTerrain = meshConstructParams.smoothTerrain;
        meshSharedVertices = meshConstructParams.meshSharedVertices;
        cubeSharedVertices = meshConstructParams.cubeSharedVertices;
        LOD = meshConstructParams.LOD;

        noiseMap = Noise.GenerateNoiseMap(
            valuesX,
            valuesZ,
            chunkPosition.x,
            chunkPosition.z,
            generationParams
            );

        CreateTerrainData(noiseMap);
        CreateMeshData();
        
        BuildMesh();
    }
    
    void CreateTerrainData(float[,] noiseMap)
    {

        for (int x = 0; x < valuesX; x++)
            for (int z = 0; z < valuesZ; z++)
            {
                float currentHeight = cubesY * noiseMap[x, z];

                for (int y = 0; y < valuesY; y++)
                    terrainData[x, y, z] = (float)y - currentHeight;
            }
    }

    void CreateMeshData()
    {
        dataAvailable = false;

        vertices.Clear();
        triangles.Clear();
        ResetVertexIndexArray();

        if (!enableLOD || LOD <= 0 || LOD >= (int)chunkSize)
            LOD = 1;
        else
            LOD = (int)Mathf.Pow(2, LOD);


        float startTime = Time.realtimeSinceStartup;

        for (int x = 0; x < cubesX; x += LOD)
            for (int y = 0; y < cubesY; y += LOD)
                for (int z = 0; z < cubesZ; z += LOD)
                {
                    Vector3Int position = new(x, y, z);

                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                        cube[i] = SampleTerrain(position + Tables.CornerTable[i] * LOD);

                    int configIndex = GetCubeCongif(cube);

                    if (configIndex == 0 || configIndex == 255) continue;

                    for (int vertexCounter = 0; vertexCounter < 15/*16*/; vertexCounter++)
                    {
                        int edgeIndex = Tables.TriangleTable[configIndex, vertexCounter];

                        if (edgeIndex == -1) break;

                        // On a given edge, a value must exist on the correlating edge on the previous cube, 
                        // because the mesh doesnt have holes in it. No need to check for an actual value on that edge,
                        // just copy it as there is a value in all cases. 
                        // No unnecessary copy operations are done as if the previous value is -1 its correlating edge is not even being computed.
                        // Only check for a value at the same position as the given edge to share vertices inside the cube.

                        if (meshSharedVertices) // sharing vertices in mesh and maybe in cubes
                        {
                            int redirect, index;
                            if (cubeSharedVertices && (index = vertexIndexArray[x, y, z, edgeIndex]) != -1) // if a value already exists no need to copy
                            {
                                triangles.Add(index);
                            }
                            else if (x > 0 && (redirect = Tables.redirect[edgeIndex].x) != -1) // try to copy a value from the given axis, if cant, try an other axis
                            {
                                index = vertexIndexArray[x - 1 * LOD, y, z, redirect];
                                vertexIndexArray[x, y, z, edgeIndex] = index;
                                triangles.Add(index);
                            } 
                            else if (y > 0 && (redirect = Tables.redirect[edgeIndex].y) != -1) // try to copy a value from the given axis, if cant, try an other axis
                            {
                                index = vertexIndexArray[x, y - 1 * LOD, z, redirect];
                                vertexIndexArray[x, y, z, edgeIndex] = index;
                                triangles.Add(index);
                            }
                            else if (z > 0 && (redirect = Tables.redirect[edgeIndex].z) != -1) // try to copy a value from the given axis, if cant, try an other axis
                            {
                                index = vertexIndexArray[x, y, z - 1 * LOD, redirect];
                                vertexIndexArray[x, y, z, edgeIndex] = index;
                                triangles.Add(index);
                            }
                            else // if no preexisting values could be used, calculate the vertex and retrieve its index
                            {
                                vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                            }

                            /*
                            switch (edgeIndex)
                            {
                                case 0:
                                    if (y > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 2];
                                        triangles.Add(vertexIndexArray[x, y - 1, z, 2]);
                                    }
                                    else if (z > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 4];
                                        triangles.Add(vertexIndexArray[x, y, z - 1, 4]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 1:
                                    if (z > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 5];
                                        triangles.Add(vertexIndexArray[x, y, z - 1, 5]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 2:
                                    if (z > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 6];
                                        triangles.Add(vertexIndexArray[x, y, z - 1, 6]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 3:
                                    if (x > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 1];
                                        triangles.Add(vertexIndexArray[x - 1, y, z, 1]);
                                    }
                                    else if (z > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 7];
                                        triangles.Add(vertexIndexArray[x, y, z - 1, 7]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 4:
                                    if (y > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 6];
                                        triangles.Add(vertexIndexArray[x, y - 1, z, 6]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 7:
                                    if (x > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 5];
                                        triangles.Add(vertexIndexArray[x - 1, y, z, 5]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 8:
                                    if (x > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 9];
                                        triangles.Add(vertexIndexArray[x - 1, y, z, 9]);
                                    }
                                    else if (y > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 11];
                                        triangles.Add(vertexIndexArray[x, y - 1, z, 11]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 9:
                                    if (y > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 10];
                                        triangles.Add(vertexIndexArray[x, y - 1, z, 10]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                case 11:
                                    if (x > 0)
                                    {
                                        vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 10];
                                        triangles.Add(vertexIndexArray[x - 1, y, z, 10]);
                                    }
                                    else
                                        vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                                default:
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                    break;
                            }
                            */
                        }
                        else if (cubeSharedVertices) // sharing vertices in cubes only
                        {
                            if (vertexIndexArray[x, y, z, edgeIndex] != -1)
                                triangles.Add(vertexIndexArray[x, y, z, edgeIndex]);
                            else
                                vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                        }
                        else // not sharing vertices
                        {
                            CalculateVertex(position, edgeIndex, cube);
                        }
                    }
                }

        float endTime = Time.realtimeSinceStartup;

        time = endTime - startTime;

        totalVertexCount = vertices.Count();

        uniqueVertexCount = vertices.Distinct().Count();

#if _DEBUG
        Debug.Log("Time to calculate mesh: " + time);

        if (vertexCount != uniqueVertexCount)
            Debug.Log("Duplicate vertices exist");
        else
            Debug.Log("No duplicate vertices exist");

        Debug.Log("Vertex count with dupes:    " + vertexCount);
        Debug.Log("Vertex count without dupes: " + uniqueVertexCount);  
#endif

        dataAvailable = true;
    }

    int CalculateVertex(Vector3Int position, int edgeIndex, float[] cube)
    {
        Vector3 vert1 = Tables.CornerTable[Tables.EdgeTable[edgeIndex, 0]];
        Vector3 vert2 = Tables.CornerTable[Tables.EdgeTable[edgeIndex, 1]];

        Vector3 vertPos;
        if (smoothTerrain)
        {
            float vert1Sample = cube[Tables.EdgeTable[edgeIndex, 0]];
            float vert2Sample = cube[Tables.EdgeTable[edgeIndex, 1]];

            float difference = vert2Sample - vert1Sample;
#if _DEBUG
            if (difference == 0)
                Debug.Log("DIFFERENCE IS 0");
#endif

            difference = (terrainHeight - vert1Sample) / difference;

            vertPos = vert1 + (vert2 - vert1) * difference;
        }
        else
            vertPos = (vert1 + vert2) / 2f;

        vertPos *= LOD;
        vertPos += position;

        vertices.Add(vertPos);
        int vertexCount = vertices.Count;
        triangles.Add(vertexCount - 1);

        return (vertexCount - 1);
    }

    int GetCubeCongif(float[] cube)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
            if (cube[i] > terrainHeight)
                configIndex |= 1 << i;
        return configIndex;
    }

    float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x, point.y, point.z];
    }

    void BuildMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();

        meshMemoryUsage = Profiler.GetRuntimeMemorySizeLong(mesh);

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void ResetVertexIndexArray()
    {
        for (int i = 0; i < cubesX; i++)
            for (int j = 0; j < cubesY; j++)
                for (int k = 0; k < cubesZ; k++)
                    for (int i1 = 0; i1 < 12; i1++)
                        vertexIndexArray[i, j, k, i1] = -1;
    }
    
    public bool IsDataAvailable()
    {
        return dataAvailable;
    }

    public string[] GetData()
    {
        string chunkIndexStr = chunkIndex.ToString();
        string dimensionsStr = cubesX.ToString() + "," + cubesY.ToString() + "," + cubesZ.ToString();
        string timeStr = time.ToString();
        string vertexCountStr = totalVertexCount.ToString();
        string uniqueVertexCountStr = uniqueVertexCount.ToString();
        string smoothTerrainStr = smoothTerrain.ToString();
        string meshSharedVerticesStr = meshSharedVertices.ToString();
        string cubeSharedVerticesStr = cubeSharedVertices.ToString();
        return new string[8] { chunkIndexStr, dimensionsStr, timeStr, vertexCountStr, uniqueVertexCountStr, smoothTerrainStr, meshSharedVerticesStr, cubeSharedVerticesStr };
        // chunkIndexStr, dimensionsStr, timeStr, vertexCountStr, uniqueVertexCountStr, smoothTerrainStr, meshSharedVerticesStr, cubeSharedVerticesStr
    }

    public List<int> GetTriangles() { return triangles; }

    public int[,,,] GetVertexIndexArray() { return vertexIndexArray; }

    public double GetTimeToGenerate() { return time; }

    public int GetTotalVertexCount() { return totalVertexCount; }

    public int GetUniqueVertexCount() { return uniqueVertexCount; }

    public long GetMeshMemoryUsage() { return meshMemoryUsage; }

    public struct MeshConstructParams
    {
        public bool smoothTerrain;
        public bool meshSharedVertices;
        public bool cubeSharedVertices;
        public int LOD;
    }

}
