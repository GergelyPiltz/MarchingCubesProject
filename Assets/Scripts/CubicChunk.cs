using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubicChunk
{
    private readonly GameObject chunkObject;
    public GameObject ChunkObject { get { return chunkObject; } }

    private readonly Vector3Int position;
    public Vector3Int Position { get { return position; } }

    private readonly MeshFilter meshFilter;
    private readonly MeshCollider meshCollider;
    private readonly MeshRenderer meshRenderer;

    private List<Vector3> vertices;
    private List<int> triangles;

    public const int cubesPerAxis = 32;
    public const int valuesPerAxis = cubesPerAxis + 1;

    private const float terrainHeight = 0f;

    private static bool smoothTerrain      = false;
    private static bool meshSharedVertices = false;
    private static bool cubeSharedVertices = false;
    //int LOD = 1;

    private float[,,] terrainData;
    private ushort[,,,] vertexIndexArray; // By this being ushort we save a whopping 96 KiBs per chunk. 

    //private AssetProvider assetProvider;
    private static Assets assetProvider;

    public CubicChunk(Vector3Int position, Transform parent)
    {
        if (!assetProvider)
            assetProvider = GameObject.Find("Assets").GetComponent<Assets>();

        chunkObject = new GameObject();
        chunkObject.transform.parent = parent;
        chunkObject.name = "Chunk" + position.ToString();
        this.position = position;
        chunkObject.transform.position = this.position;

        //chunkObject.AddComponent<ChunkBorder>();

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");

        vertexIndexArray = new ushort[cubesPerAxis, cubesPerAxis, cubesPerAxis, 12];

        vertices = new();
        triangles = new();

        //assetProvider = AssetProvider.Instance;
    }

    #region Build

    public void Build(NoiseMap2D noise2D)
    {
        CreateTerrainData(noise2D);
        BuildRest();
    }
    
    public void Build(NoiseMap3D noise3D)
    {
        CreateTerrainData(noise3D);
        BuildRest();
    }

    public void Build(NoiseMap2D noise2D, NoiseMap3D noise3D)
    {
        CreateTerrainData(noise2D, noise3D);
        BuildRest();
    }

    private void BuildRest() // this keeps expending and i dont want to type everything 3 times
    {
        CreateMeshData();
        BuildMesh();
        DestroyAllChildren();
        PlaceVegetation();
    }

    #endregion

    #region Creating terrain data from noise

    private void CreateTerrainData(NoiseMap2D noise2D)
    {
        if (noise2D.size != valuesPerAxis)
            throw new ArgumentException("Noise layer does not fit chunk");

        terrainData = new float[valuesPerAxis, valuesPerAxis, valuesPerAxis];
        for (int x = 0; x < valuesPerAxis; x++)
            for (int z = 0; z < valuesPerAxis; z++)
            {
                //float height = World.worldHeight * noise2D.noise[x, z];
                for (int y = 0; y < valuesPerAxis; y++)
                {
                    //terrainData[x, y, z] = y + position.y - height;
                    terrainData[x, y, z] = ((float)1 / World.worldHeight * (y + position.y)) - noise2D.noise[x, z];
                }
            }
                
    }

    private void CreateTerrainData(NoiseMap3D noise3D)
    {
        if (noise3D.size != valuesPerAxis)
            throw new ArgumentException("Noise layer does not fit chunk");

        terrainData = (noise3D * 2 - 1).noise;
    }

    private void CreateTerrainData(NoiseMap2D noise2D, NoiseMap3D noise3D)
    {
        CreateTerrainData(noise2D); // Declares terrainData

        if (noise3D.size != valuesPerAxis)
            throw new ArgumentException("Noise layer does not fit chunk");

        noise3D = noise3D * 2 - 1; // DO NOT DO THIS INSIDE ANY LOOP !!!!!!!!!!!!!

        for (int x = 0; x < valuesPerAxis; x++)
            for (int y = 0; y < valuesPerAxis; y++)
                for (int z = 0; z < valuesPerAxis; z++)
                    if (terrainData[x, y, z] < terrainHeight)
                    {
                        terrainData[x, y, z] *= noise3D.noise[x, y, z];
                    }
        
    }

    #endregion

    #region Marching Cubes

    private void CreateMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        ResetVertexIndexArray(); // all values to 56535

        float startTime = Time.realtimeSinceStartup;

        for (int x = 0; x < cubesPerAxis; x++)
            for (int y = 0; y < cubesPerAxis; y++)
                for (int z = 0; z < cubesPerAxis; z++)
                {
                    Vector3Int position = new(x, y, z);

                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                        cube[i] = SampleTerrain(position + Tables.CornerTable[i]);

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
                            if (cubeSharedVertices && (index = vertexIndexArray[x, y, z, edgeIndex]) != ushort.MaxValue) // if a value already exists no need to copy
                            {
                                triangles.Add(index);
                            }
                            else if (x > 0 && (redirect = Tables.redirect[edgeIndex].x) != -1) // try to copy a value from the given axis, if cant, try an other axis
                            {
                                index = vertexIndexArray[x - 1, y, z, redirect];
                                vertexIndexArray[x, y, z, edgeIndex] = (ushort)index;
                                triangles.Add(index);
                            }
                            else if (y > 0 && (redirect = Tables.redirect[edgeIndex].y) != -1) // try to copy a value from the given axis, if cant, try an other axis
                            {
                                index = vertexIndexArray[x, y - 1, z, redirect];
                                vertexIndexArray[x, y, z, edgeIndex] = (ushort)index;
                                triangles.Add(index);
                            }
                            else if (z > 0 && (redirect = Tables.redirect[edgeIndex].z) != -1) // try to copy a value from the given axis, if cant, try an other axis
                            {
                                index = vertexIndexArray[x, y, z - 1, redirect];
                                vertexIndexArray[x, y, z, edgeIndex] = (ushort)index;
                                triangles.Add(index);
                            }
                            else // if no preexisting values could be used, calculate the vertex and retrieve its index
                            {
                                vertexIndexArray[x, y, z, edgeIndex] = (ushort)CalculateVertex(position, edgeIndex, cube);
                            }

                        }
                        else if (cubeSharedVertices) // sharing vertices in cubes only
                        {
                            if (vertexIndexArray[x, y, z, edgeIndex] != ushort.MaxValue)
                                triangles.Add(vertexIndexArray[x, y, z, edgeIndex]);
                            else
                                vertexIndexArray[x, y, z, edgeIndex] = (ushort)CalculateVertex(position, edgeIndex, cube);
                        }
                        else // not sharing vertices
                        {
                            CalculateVertex(position, edgeIndex, cube);
                        }
                    }
                }
    }

    private int CalculateVertex(Vector3Int position, int edgeIndex, float[] cube)
    {
        Vector3 vert1 = Tables.CornerTable[Tables.EdgeTable[edgeIndex, 0]];
        Vector3 vert2 = Tables.CornerTable[Tables.EdgeTable[edgeIndex, 1]];

        Vector3 vertPos;
        if (smoothTerrain)
        {
            float vert1Sample = cube[Tables.EdgeTable[edgeIndex, 0]];
            float vert2Sample = cube[Tables.EdgeTable[edgeIndex, 1]];

            float difference = vert2Sample - vert1Sample;

            if (difference == 0)
                throw new Exception("While calculating a vertex the difference between values were 0 within cube at position " + position.ToString());

            difference = (terrainHeight - vert1Sample) / difference;

            vertPos = vert1 + (vert2 - vert1) * difference;
        }
        else
            vertPos = (vert1 + vert2) / 2f;

        vertPos += position;

        vertices.Add(vertPos);
        int vertexCount = vertices.Count;
        triangles.Add(vertexCount - 1);

        return (vertexCount - 1);
    }

    private int GetCubeCongif(float[] cube)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
            if (cube[i] > terrainHeight)
                configIndex |= 1 << i;
        return configIndex;
    }

    private float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x, point.y, point.z];
    }

    private void ResetVertexIndexArray()
    {
        for (int i = 0; i < cubesPerAxis; i++)
            for (int j = 0; j < cubesPerAxis; j++)
                for (int k = 0; k < cubesPerAxis; k++)
                    for (int l = 0; l < 12; l++)
                        vertexIndexArray[i, j, k, l] = ushort.MaxValue;
    }

    #endregion

    private void BuildMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void DestroyAllChildren()
    {
        foreach (Transform child in chunkObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private readonly int numberOfObjectsToTryMax = 5;
    private readonly float scaleMultiplier = 5f;
    private readonly float scaleRange = 0.5f; // 0.2f = +/- 20%
    private void PlaceVegetation()
    {
        if (vertices.Count == 0) return;

        int tries = Random.Range(0, numberOfObjectsToTryMax);

        for (byte i = 0; i < tries; i++)
        {
            Vector3 origin = new(Random.value * cubesPerAxis + position.x, World.worldHeight, Random.value * cubesPerAxis + position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit/*, World.worldHeight, vegetationLayer*/))
                if (Vector3.Angle(Vector3.up, hit.normal) < 50f)
                {
                    int x = Random.Range(0, assetProvider.vegetation.Length - 1);
                    Quaternion rotation = Quaternion.Euler( new (0, Random.Range(0, 360), 0));
                    Vector3 pos = hit.point - new Vector3(0, 0.5f, 0);

                    GameObject veg = GameObject.Instantiate(assetProvider.vegetation[x], pos, rotation, chunkObject.transform);
                    //veg.layer = vegetationLayer;

                    float scale = Random.Range(scaleMultiplier - scaleMultiplier * scaleRange, scaleMultiplier + scaleMultiplier * scaleRange);
                    Vector3 scaleVect = new(scale, scale, scale);

                    veg.transform.localScale = scaleVect;
                }
        }
    }

    /// <summary>
    /// Sets how the terrain looks using 3 parameters. All false: flat shading, blocky and slightly more memory usage. All true: smooth and less memory usage.
    /// </summary>
    /// <param name="smoothTerrain"> True: snooth surface. False: blocky terrain. </param>
    /// <param name="meshSharedVertices">For best effect use together with cubeSharedVertices</param>
    /// <param name="cubeSharedVertices">For best effect use together with meshSharedVertices</param>
    public static void SetTerainParameters(bool smoothTerrain, bool meshSharedVertices, bool cubeSharedVertices)
    {
        CubicChunk.smoothTerrain = smoothTerrain;
        CubicChunk.meshSharedVertices = meshSharedVertices;
        CubicChunk.cubeSharedVertices = cubeSharedVertices;
    }

    [Serializable] public enum TerrainProfile{ Smooth, Blocky }
    /// <summary>
    /// Choose a terrainprofile between a blocky look and a smooth, more realistic look.
    /// </summary>
    /// <param name="profile"></param>
    public static void SetTerrainProfile(TerrainProfile profile)
    {
        if (profile == TerrainProfile.Smooth)
            SetTerainParameters(true, true, true);
        else
            SetTerainParameters(false, false, false);
    }
}