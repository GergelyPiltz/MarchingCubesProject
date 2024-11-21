using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Random = UnityEngine.Random;

public class CubicChunk
{
    public static float maxComputeTime = 0f;
    public static float avgComputeTime = 0f;
    public static float totalComputeTime = 0f;
    public static int totalComputes = 0;

    public static float maxBuildTime = 0f;
    public static float avgBuildTime = 0f;
    public static float totalBuildTime = 0f;
    public static int totalBuilds = 0;

    public Vector3Int Position { get; private set; } //! Position of the chunk
    public readonly GameObject chunkObject; //! GameObject of the chunk
    public readonly Transform chunkTransform; //! Transform of the GameObject of the chunk

    private readonly MeshFilter meshFilter;
    private readonly MeshCollider meshCollider;
    private readonly MeshRenderer meshRenderer;

    private List<Vector3> vertices = new();
    private List<int> triangles = new();

    private static bool isMarchingOnGPU = true; // Enables the use of compute shaders

    //GPU
    public const int cubesPerAxis = 16; //! The amount of cubes on an axis. Don't change!
    public const int valuesPerAxis = cubesPerAxis + 1; //! The amount of values on an axis
    public const int totalValues = valuesPerAxis * valuesPerAxis * valuesPerAxis; //! The total values present
    // There are hardcoded (only way) values in compute shaders. 
    private const int marchingNumThreads = cubesPerAxis / 4; // The amount of threads the compute shader will use per dimension for the Marching Cubes algorithm. Calculated by dividing cubesPerAxis with the hardcoded value in the computeshader.
    private const int noiseNumThreads = valuesPerAxis / 1; // The amount of threads the compute shader will use per dimension for the Noise generation algorithm. (valuesPerAxis is 17 which is a prime number)

    //CPU
    private const float terrainHeight = 0f; // Values higher then this are above the terrain, lower than this are below

    private static bool smoothTerrain = false; // Enables interpolation between whole coordinates based on values
    private static bool meshSharedVertices = false; // Enables optimization by sharing vertices between cubes
    private static bool cubeSharedVertices = false; // Enables optimization by sharint vertices within a single cube
    
    private float[] terrainData = new float[totalValues]; // The values representing the shape of the terrain
    private ushort[,,,] vertexIndexArray; // By this being ushort we save a whopping 96 KiBs per chunk.

    //private static readonly ScriptResources scriptResources;

    private static readonly Settings settings;

    // Static constructor. Sets the compute shader scripts and compute buffers for all instances.
    static CubicChunk()
    {
        //if (!GameObject.Find("Assets").TryGetComponent(out assets))
        //    Debug.LogError("Chunk can't find assets. Gameobject with assets must be named \"Assets\"");

        if (GameObject.Find("ScriptResources").TryGetComponent(out ScriptResources scriptResources))
        {

            marchingCompute = scriptResources.MarchingCompute;
            noiseCompute = scriptResources.NoiseCompute;

            // General values for Noise
            noiseCompute.SetInt("valuesPerAxis", valuesPerAxis); // Constant
            noiseCompute.SetInt("worldHeight", World.worldHeight); // Constant
            // Noise parameters
            noiseCompute.SetFloat("startFrequency", 0.0005f);
            noiseCompute.SetFloat("frequencyModifier", 10f);
            noiseCompute.SetFloat("amplitudeModifier", 0.1f);
            noiseCompute.SetInt("octaves", 3);
            // Noise buffers
            noiseCompute.SetBuffer(0, "noise2DBuffer", noise2DBuffer);
            noiseCompute.SetBuffer(1, "noise2DBuffer", noise2DBuffer);
            noiseCompute.SetBuffer(1, "terrainDataBuffer", terrainDataBuffer);

            // General values for Marching Cubes
            marchingCompute.SetBool("smoothTerrain", smoothTerrain); // Can change
            marchingCompute.SetInt("valuesPerAxis", valuesPerAxis); // Constant
            // Marching buffers
            marchingCompute.SetBuffer(0, "terrainDataBuffer", terrainDataBuffer);
            marchingCompute.SetBuffer(0, "triangleBuffer", triangleBuffer);
        }
        else
            Debug.LogError("Chunk can't find scripts. Gameobject with scripts must be named \"ScriptResources\"");
    }

    public CubicChunk()
    {
        
        Position = new(0, 0, 0);
        chunkObject = new("Chunk (Uninitialized)");
        chunkTransform = chunkObject.transform;
        chunkTransform.position = Position;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
    }

    #region Compute

    /// <summary>
    /// The compute shader returns the mesh by individual triangles.
    /// </summary>
    private struct Triangle
    {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
    }

    private static readonly ComputeShader marchingCompute; // The script for the Marching Cubes compute shader
    private static readonly ComputeShader noiseCompute; // The script for the Perlin Noise compute shader 

    private static readonly ComputeBuffer noise2DBuffer = new(valuesPerAxis * valuesPerAxis, sizeof(float));
    private static readonly ComputeBuffer terrainDataBuffer = new(totalValues, sizeof(float));
    private static readonly ComputeBuffer triangleBuffer = new(totalValues * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
    private static readonly ComputeBuffer triangleCountBuffer = new(1, sizeof(int), ComputeBufferType.Raw);
    private int[] triangleCountArray = new int[1];

    /// <summary>
    /// Computes the noise using a compute shader
    /// </summary>
    private void ComputeNoise()
    {
        noiseCompute.SetInt("posX", Position.x);
        noiseCompute.SetInt("posY", Position.y);
        noiseCompute.SetInt("posZ", Position.z);

        noiseCompute.Dispatch(0, noiseNumThreads, 1, noiseNumThreads);
        noiseCompute.Dispatch(1, noiseNumThreads, noiseNumThreads, noiseNumThreads);

        terrainDataBuffer.GetData(terrainData);
    }

    /// <summary>
    /// Runs the Marching Cubes algorithm using a compute shader
    /// </summary>
    private void ComputeMesh()
    {
        triangleBuffer.SetCounterValue(0);
        marchingCompute.Dispatch(0, marchingNumThreads, marchingNumThreads, marchingNumThreads);

        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triangleCountArray);

        Triangle[] triangleArray = new Triangle[triangleCountArray[0]];
        triangleBuffer.GetData(triangleArray);

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

    /// <summary>
    /// Releases the buffers to prevent memory leaks. Buffers are static. 
    /// </summary>
    public static void ReleaseBuffers()
    {
        noise2DBuffer.Release();
        triangleBuffer.Release();
        terrainDataBuffer.Release();
        triangleCountBuffer.Release();
    }

    #endregion

    /// <summary>
    /// Manipulates the terrain in a block
    /// </summary>
    /// <param name="value">Sets all corners to this value</param>
    public void OverwriteBlock(Vector3Int block, float value)
    {
        for (int i = 0; i < 8; i++)
        {
            int index = IndexFromCoord(block + Tables.CornerTable[i]);
            if (index < terrainData.GetLength(0))
                terrainData[index] = value;
            else
                Debug.Log("index out of range");            
        }
    }

    /// <summary>
    /// Manipulates the terrain at a single point
    /// </summary>
    public void OverwriteTerrainValue(Vector3Int valueCoord, float value)
    {

        int index = IndexFromCoord(valueCoord);
        try
        {
            if (index >= terrainData.GetLength(0))
            {
                Debug.Log("Invalid index: " + index);
                return;
            }
            terrainData[index] = value;
        }
        catch (IndexOutOfRangeException)
        {
            Debug.Log("Length: " + terrainData.GetLength(0) + " Index: " + index + " Coord: " + valueCoord);
        }
        
    }

    public void AddToTerrainValue(Vector3Int valueCoord, float value)
    {
        terrainData[IndexFromCoord(valueCoord)] = value;
    }

    /// <summary>
    /// Converts a 3 dimensional coordinate to a 1 dimensional index
    /// </summary>
    private int IndexFromCoord(Vector3Int coord)
    {
        return coord.x * valuesPerAxis * valuesPerAxis + coord.y * valuesPerAxis + coord.z;
    }

    /// <summary>
    /// Moves the chunk to a new position and disables it
    /// </summary>
    public void Move(Vector3Int position)
    {
        chunkObject.SetActive(false);
        Position = position;
        chunkObject.name = "Chunk" + Position.ToString();
        chunkTransform.position = Position; 
    }

    /// <summary>
    /// Builds the chunk from start to finish 
    /// </summary>
    public void Build()
    {
        float time = Time.realtimeSinceStartup;

        ComputeNoise();
        if (isMarchingOnGPU)
        {
            ComputeMesh();
        }
        else
            CreateMeshData();
        ApplyMesh();

        chunkObject.SetActive(true);

        time = Time.realtimeSinceStartup - time;
        if (time > maxBuildTime) maxBuildTime = time;
        totalBuilds++;
        totalBuildTime += time;
        avgBuildTime = totalBuildTime / totalBuilds;
    }

    /// <summary>
    /// Recalculates the mesh from the data in terrainData
    /// </summary>
    public void RecalculateMesh()
    {
        terrainDataBuffer.SetData(terrainData);
        ComputeMesh();
        ApplyMesh();
    }

    /*

    #region Creating terrain data from noise, NOT USED, IMPLEMENTED IN COMPUTE SHADER  

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

    */

    #region Marching Cubes

    /// <summary>
    /// Creates the mesh data using the Marching Cubes algorith on the CPU
    /// </summary>
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

    /// <summary>
    /// Calculates a single vertex
    /// </summary>
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

    /// <summary>
    /// Calculates the configuration of a single cube
    /// </summary>
    /// <param name="cube">Float[] of the corners of the cube.</param>
    private int GetCubeCongif(float[] cube)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
            if (cube[i] > terrainHeight)
                configIndex |= 1 << i;
        return configIndex;
    }

    /// <summary>
    /// Samples the terrain at the given position.
    /// </summary>
    /// <param name="point">Point where it samples.</param>
    private float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x * valuesPerAxis * valuesPerAxis + point.y * valuesPerAxis + point.z];
    }

    /// <summary>
    /// (Re)Initializes the vertexIndexArray and sets all values to ushort.MaxValue.
    /// </summary>
    private void ResetVertexIndexArray()
    {
        vertexIndexArray = new ushort[cubesPerAxis, cubesPerAxis, cubesPerAxis, 12];
        for (int i = 0; i < cubesPerAxis; i++)
            for (int j = 0; j < cubesPerAxis; j++)
                for (int k = 0; k < cubesPerAxis; k++)
                    for (int l = 0; l < 12; l++)
                        vertexIndexArray[i, j, k, l] = ushort.MaxValue;
    }

    #endregion

    /// <summary>
    /// Applies the calculated mesh to the gameObjects meshFilter and meshCollider.
    /// </summary>
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

    /// <summary>
    /// Activates or disables the gameobject of the chunk
    /// </summary>
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
        CubicChunk.meshSharedVertices = meshSharedVertices;
        CubicChunk.cubeSharedVertices = cubeSharedVertices;

        marchingCompute.SetBool("smoothTerrain", smoothTerrain);
    }

    [Serializable] public enum TerrainProfile{ Smooth, Blocky }
    /// <summary>
    /// Choose a terrainprofile between a blocky look and a smooth, more realistic look.
    /// </summary>
    /// <param name="profile">Smooth/Blocky</param>
    public static void SetTerrainProfile(TerrainProfile profile)
    {
        if (profile == TerrainProfile.Smooth)
            SetTerainParameters(true, true, true);
        else
            SetTerainParameters(false, false, false);
        Debug.Log(profile.ToString());
    }

    /// <summary>
    /// Enables the Marching Cubes algorithm to run on the GPU using compute shaders
    /// </summary>
    public static void EnableMarchingOnGPU(bool b)
    {
        isMarchingOnGPU = b;
        Debug.Log(isMarchingOnGPU);
    }
}



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