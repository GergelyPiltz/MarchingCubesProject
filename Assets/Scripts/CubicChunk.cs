using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubicChunk
{
    public Vector3Int Position { get; private set; }
    public readonly GameObject chunkObject;
    public readonly Transform chunkTransform;

    private readonly MeshFilter meshFilter;
    private readonly MeshCollider meshCollider;
    private readonly MeshRenderer meshRenderer;

    private List<Vector3> vertices = new();
    private List<int> triangles = new();

    public const int cubesPerAxis = 24; // Must be a multiple of 8. Don't change. See below.
    public const int valuesPerAxis = cubesPerAxis + 1;
    public const int totalValues = valuesPerAxis * valuesPerAxis * valuesPerAxis;
    //There are hardcoded (only way) values in compute shader. CAREFUL!!!
    private const int noiseNumThreads = valuesPerAxis / 5; // If cubesPerAxis is changed from 24 it will break. 
    private const int chunkNumThreads = valuesPerAxis / 8; // If cubesPerAxis is not divisible by 8 it will break. 

    //private const float terrainHeight = 0f;

    private static bool smoothTerrain = false;
    //private static bool meshSharedVertices = false;
    //private static bool cubeSharedVertices = false;
    //int LOD = 1;

    //private float[] terrainData;
    //private ushort[,,,] vertexIndexArray; // By this being ushort we save a whopping 96 KiBs per chunk. 

    private static readonly Assets assets;
    //private static readonly ScriptResources scriptResources;

    private static readonly ComputeShader marchingCompute;
    private static readonly ComputeShader noiseCompute;

    static CubicChunk()
    {
        if (!GameObject.Find("Assets").TryGetComponent(out assets))
            Debug.LogError("Chunk can't find assets. Gameobject with assets must be named \"Assets\"");

        if (GameObject.Find("ScriptResources").TryGetComponent(out ScriptResources scriptResources))
        {
            marchingCompute = scriptResources.MarchingCompute;
            noiseCompute = scriptResources.NoiseCompute;
        }
        else
            Debug.LogError("Chunk can't find scripts. Gameobject with scripts must be named \"ScriptResources\"");
    }

    public CubicChunk(Transform parent)
    {
        Position = new(0, 0, 0);
        chunkObject = new("Chunk" + Position.ToString());
        chunkTransform = chunkObject.transform;
        chunkTransform.parent = parent;
        chunkTransform.position = Position;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
    }

    #region Compute

    struct Triangle
    {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
    }

    private void Compute()
    {
        noiseCompute.SetInt("valuesPerAxis", valuesPerAxis);
        noiseCompute.SetInt("worldHeight", World.worldHeight);
        noiseCompute.SetInt("posX", Position.x);
        noiseCompute.SetInt("posY", Position.y);
        noiseCompute.SetInt("posZ", Position.z);

        noiseCompute.SetFloat("startFrequency", 0.0005f);
        noiseCompute.SetFloat("frequencyModifier", 10f);
        noiseCompute.SetFloat("amplitudeModifier", 0.1f);
        noiseCompute.SetInt("octaves", 3);

        ComputeBuffer noise2DBuffer = new(valuesPerAxis * valuesPerAxis, sizeof(float));
        noiseCompute.SetBuffer(0, "noise2DBuffer", noise2DBuffer);
        noiseCompute.SetBuffer(1, "noise2DBuffer", noise2DBuffer);

        ComputeBuffer terrainDataBuffer = new(totalValues, sizeof(float));
        noiseCompute.SetBuffer(1, "terrainDataBuffer", terrainDataBuffer);

        noiseCompute.Dispatch(0, noiseNumThreads, 1, noiseNumThreads);

        noiseCompute.Dispatch(1, noiseNumThreads, noiseNumThreads, noiseNumThreads);

        #region ToFile

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //float[] terrainDataBufferArray = new float[totalValues];
        //terrainDataBuffer.GetData(terrainDataBufferArray);
        //string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //string path = Path.Combine(myDocs, "terrainDataBuffer.txt");
        //using (StreamWriter outputFile = new StreamWriter(path))
        //{
        //    for (int x = 0; x < valuesPerAxis; x++)
        //    {
        //        for (int y = 0; y < valuesPerAxis; y++)
        //        {
        //            for (int z = 0; z < valuesPerAxis; z++)
        //            {
        //                outputFile.Write(terrainDataBufferArray[x * valuesPerAxis * valuesPerAxis + y * valuesPerAxis + z] + " ");
        //            }
        //            outputFile.WriteLine();
        //        }
        //        outputFile.WriteLine();
        //    }
        //}

        //float[] noiseBufferArray = new float[valuesPerAxis * valuesPerAxis];
        //noise2DBuffer.GetData(noiseBufferArray);
        //path = Path.Combine(myDocs, "noiseBuffer.txt");
        //using (StreamWriter outputFile = new StreamWriter(path))
        //{
        //    for (int x = 0; x < valuesPerAxis; x++)
        //    {
        //        for (int z = 0; z < valuesPerAxis; z++)
        //        {
        //            outputFile.Write(noiseBufferArray[x * valuesPerAxis + z] + " ");
        //        }
        //        outputFile.WriteLine();
        //    }
        //}
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        marchingCompute.SetBool("smoothTerrain", smoothTerrain);
        marchingCompute.SetInt("valuesPerAxis", valuesPerAxis);

        marchingCompute.SetBuffer(0, "terrainDataBuffer", terrainDataBuffer);

        ComputeBuffer triangleBuffer = new(totalValues * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        marchingCompute.SetBuffer(0, "triangleBuffer", triangleBuffer);

        marchingCompute.Dispatch(0, chunkNumThreads, chunkNumThreads, chunkNumThreads);

        ComputeBuffer triangleCountBuffer = new(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = new int[1];
        triangleCountBuffer.GetData(triangleCountArray);

        Triangle[] triangleArray = new Triangle[triangleCountArray[0]];
        triangleBuffer.GetData(triangleArray);

        noise2DBuffer.Release();
        triangleBuffer.Release();
        terrainDataBuffer.Release();
        triangleCountBuffer.Release();

        triangles.Clear();
        vertices.Clear();

        for (int i = 0; i < triangleCountArray[0]; i++)
        {
            vertices.Add(triangleArray[i].vertexA);
            triangles.Add(i * 3);
            vertices.Add(triangleArray[i].vertexB);
            triangles.Add(i * 3 + 1);
            vertices.Add(triangleArray[i].vertexC);
            triangles.Add(i * 3 + 2);
        }

    }

    #endregion

    public void MoveAndBuild(Vector3Int position)
    {
        Position = position;
        chunkObject.name = "Chunk" + Position.ToString();
        chunkTransform.position = Position;

        Compute();
        ApplyMesh();
        DestroyAllChildren();
        PlaceVegetation();
    }

    public void RecalculateMesh()
    {
        Compute();
        ApplyMesh();
    }

    /*

    #region Creating terrain data from noise UNUSED

    private void CreateTerrainData(NoiseMap2D noise2D)
    {
        if (noise2D.size != valuesPerAxis)
            throw new ArgumentException("Noise layer does not fit chunk");

        terrainData = new float[totalValues];
        for (int x = 0; x < valuesPerAxis; x++)
            for (int z = 0; z < valuesPerAxis; z++)
                for (int y = 0; y < valuesPerAxis; y++)
                    terrainData[x * valuesPerAxis * valuesPerAxis + y * valuesPerAxis + z] = ((float)1 / World.worldHeight * (y + position.y)) - noise2D.noise[x, z];
                
    }

    private void CreateTerrainData(NoiseMap3D noise3D)
    {
        throw new NotImplementedException();

        //if (noise3D.size != valuesPerAxis)
        //    throw new ArgumentException("Noise layer does not fit chunk");

        //terrainData = (noise3D * 2 - 1).noise;
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
                    if (terrainData[x * valuesPerAxis * valuesPerAxis + y * valuesPerAxis + z] < terrainHeight) 
                        terrainData[x * valuesPerAxis * valuesPerAxis + y * valuesPerAxis + z] *= noise3D.noise[x, y, z];
        
    }

    #endregion

    #region Marching Cubes UNUSED

    private void CreateMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        ResetVertexIndexArray(); // all values to 56535

        //float startTime = Time.realtimeSinceStartup;

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

                    for (int vertexCounter = 0; vertexCounter < 15; vertexCounter++)
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
        return terrainData[point.x * valuesPerAxis * valuesPerAxis + point.y * valuesPerAxis + point.z];
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

    */

    private void ApplyMesh()
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
        foreach (Transform child in chunkTransform)
            GameObject.Destroy(child.gameObject);
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
            Vector3 origin = new(Random.value * cubesPerAxis + Position.x, World.worldHeight, Random.value * cubesPerAxis + Position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit))
                if (Vector3.Angle(Vector3.up, hit.normal) < 50f)
                {
                    int x = Random.Range(0, assets.vegetation.Length - 1);
                    Quaternion rotation = Quaternion.Euler( new (0, Random.Range(0, 360), 0));
                    Vector3 pos = hit.point - new Vector3(0, 0.5f, 0);

                    GameObject veg = GameObject.Instantiate(assets.vegetation[x], pos, rotation, chunkTransform);

                    float scale = Random.Range(scaleMultiplier - scaleMultiplier * scaleRange, scaleMultiplier + scaleMultiplier * scaleRange);
                    Vector3 scaleVect = new(scale, scale, scale);

                    veg.transform.localScale = scaleVect;
                }
        }
    }

    public void SetActive(bool b)
    {
        chunkObject.SetActive(b);
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
        //CubicChunk.meshSharedVertices = meshSharedVertices;
        //CubicChunk.cubeSharedVertices = cubeSharedVertices;
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