//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using TMPro;
//using UnityEngine;
//using static Chunk;

//public class ChunkGenerator : MonoBehaviour
//{
//    [Header("Chunk Parameters")]
//    //[SerializeField] int chunksX;
//    //[SerializeField] int chunksZ;
//    Vector3Int chunkDimensions;
//    [SerializeField] ChunkSize chunkSize = ChunkSize._16x16;

//    [SerializeField, Range(0, 8)] int LOD = 0;
//    public void SetLOD(float f)
//    {
//        LOD = (int)f;
//        CheckAndUpdateValues();
//    }

//    [SerializeField] bool generateOnlyOneChunk = false;
//    [SerializeField] bool smoothTerrain = true;
//    public bool SmoothTerrain
//    {
//        get { return smoothTerrain; }
//        set { smoothTerrain = value; CheckAndUpdateValues(); }
//    }
//    [SerializeField] bool meshSharedVertices = true;
//    public bool MeshSharedVertices
//    { 
//        get { return meshSharedVertices; } 
//        set { meshSharedVertices = value; CheckAndUpdateValues(); } 
//    }
//    [SerializeField] bool cubeSharedVertices = true;
//    public bool CubeSharedVertices
//    {
//        get { return cubeSharedVertices; }
//        set { cubeSharedVertices = value; CheckAndUpdateValues(); }
//    }
//    MeshConstructParams meshConstructParams;

//    [SerializeField] Transform player;
//    [SerializeField, Range(2, 16)] int renderDistance = 10; // not really, cause out of range chunks dont disappear (yet)

//    [SerializeField] bool enableBenchmark = false;

//    List<Chunk> chunks;
//    List<Vector2> chunkList;
//    //List<string> dataList;

//    [SerializeField] GameObject debug;
//    TextMeshProUGUI debugTmp;


//    int chunkIndex;

//    MapGenerator mapGenerator;

//    void Start()
//    {
//        if(!TryGetComponent(out mapGenerator)) enabled = false; // disable the script
//        chunks = new List<Chunk> ();
//        chunkList = new List<Vector2> ();
//        //dataList = new List<string> ();

//        Debug.Log(debug.TryGetComponent(out debugTmp));

//        arrayLength = chunkSizeToTest_MAX_Excluded - chunkSizeToTest_MIN_Included;
//        smoothedTerrainProcessTime = new string[arrayLength, 4];
//        roughTerrainProcessTime = new string[arrayLength, 4];
//        terrainTotalVertexCount = new string[arrayLength, 4];
//        memoryUsage = new string[arrayLength, 4];

//        chunkIndex = 0;

//        CheckAndUpdateValues();

//        for (int i = 0; i < 12; i++)
//            for (int j = 0; j < 12; j++)
//            {
//                Chunk temp = new(
//                        new Vector3Int(i * chunkDimensions.x, 0, j * chunkDimensions.z /*its practically .z*/ ),
//                        chunkSize,
//                        transform,
//                        chunkIndex
//                        );

//                chunks.Add(temp);
//                temp.GenerateTerrain(mapGenerator.GetGenerationParameters(), meshConstructParams);

//                chunkIndex++;
//            }
//        string s = "Chunks: " + chunks.Count;
//        debugTmp.SetText(s);


//        if (generateOnlyOneChunk && !enableBenchmark)
//        {
//            Chunk temp = new(
//                    new Vector3Int(0, 0, 0),
//                    chunkSize,
//                    transform,
//                    0
//                );
//            temp.GenerateTerrain(mapGenerator.GetGenerationParameters(), meshConstructParams);
//            chunks.Add(temp);
//        }
//    }

//    void Update()
//    {
//        return;
//        if (generateOnlyOneChunk || enableBenchmark) return;
        

//        int playerInWhichChunkX = Mathf.FloorToInt(player.position.x / chunkDimensions.x);
//        int playerInWhichChunkZ = Mathf.FloorToInt(player.position.z / chunkDimensions.z);

//        Vector2Int playerIsInWhichChunk = new(playerInWhichChunkX, playerInWhichChunkZ);

//        if (renderDistance % 2 != 0) renderDistance--;

        

//        for (int i = 0; i <= renderDistance; i++)
//        {
//            for (int j = 0; j <= renderDistance; j++)
//            {
//                Vector2Int positionRelativeToPlayer = new(i - renderDistance / 2, j - renderDistance / 2);
//                Vector2Int positionInWorld = playerIsInWhichChunk - positionRelativeToPlayer;
//                if (!chunkList.Contains(positionInWorld))
//                {
//                    chunkList.Add(positionInWorld); // for the love of god dont forget this

//                    Chunk temp = new (
//                        new Vector3Int(positionInWorld.x * chunkDimensions.x, 0, positionInWorld.y * chunkDimensions.z /*its practically .z*/ ),
//                        chunkSize,
//                        transform,
//                        chunkIndex
//                        );

//                    chunks.Add(temp);
//                    temp.GenerateTerrain(mapGenerator.GetGenerationParameters(), meshConstructParams);
                        
//                    chunkIndex++;
//                }
//            }
//        }
//    }

//    public void RunBenchmark()
//    {
//        if (enableBenchmark && !generateOnlyOneChunk)
//        {
//            StartCoroutine(Benchmark());
//        }
//    }

//    IEnumerator Benchmark()
//    {
//        //List<Task> tasks = new();

//        //for (int i = 0; i < arrayLength; i++)
//        //{
//        //    Task t = new Task(GenerateMeshForChunk(i));
//        //    tasks.Add(t);
//        //    //t.Start();

//        //    tasks.Add(t);

//        //}

//        //yield return new WaitForSeconds(10);

//        //while (true)
//        //{
//        //    bool tasksFinished = true;
//        //    foreach (var task in tasks)
//        //        if (task.Running)
//        //        {
//        //            tasksFinished = false;
//        //            yield return new WaitForSeconds(0.1f);
//        //        }
//        //    if (tasksFinished)
//        //        break;
//        //}

//        for (int i = 0; i < arrayLength; i++)
//        {
//            GenerateMeshForChunk(i + chunkSizeToTest_MIN_Included);
//        }

//        //----------------------------------------------------------------------------------
//        string fileName = "SmoothProcessingTimes.txt";

//        string docPath =
//          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName))) {
//            outputFile.WriteLine(descSmoothProcessTime);
//            outputFile.WriteLine(HEADER);
//            for (int i = 0; i < arrayLength; i++)
//            {
//                outputFile.Write((i + chunkSizeToTest_MIN_Included).ToString() + DELIMITER);
//                for (int j = 0; j < 4; j++)
//                    outputFile.Write(smoothedTerrainProcessTime[i,j] + DELIMITER);
//                outputFile.WriteLine();
//            }
//        }
//        //----------------------------------------------------------------------------------
//        fileName = "RoughProcessingTimes.txt";

//        docPath =
//          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName)))
//        {
//            outputFile.WriteLine(descRoughProcessTime);
//            outputFile.WriteLine(HEADER);
//            for (int i = 0; i < arrayLength; i++)
//            {
//                outputFile.Write((i + chunkSizeToTest_MIN_Included).ToString() + DELIMITER);
//                for (int j = 0; j < 4; j++)
//                    outputFile.Write(roughTerrainProcessTime[i, j] + DELIMITER);
//                outputFile.WriteLine();
//            }
//        }
//        //----------------------------------------------------------------------------------
//        fileName = "TotalVertexCounts.txt";

//        docPath =
//          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName)))
//        {
//            outputFile.WriteLine(descTotalVertexCount);
//            outputFile.WriteLine(HEADER);
//            for (int i = 0; i < arrayLength; i++)
//            {
//                outputFile.Write((i + chunkSizeToTest_MIN_Included).ToString() + DELIMITER);
//                for (int j = 0; j < 4; j++)
//                    outputFile.Write(terrainTotalVertexCount[i, j] + DELIMITER);
//                outputFile.WriteLine();
//            }
//        }
//        //----------------------------------------------------------------------------------
//        fileName = "MeshMemoryUsage.txt";

//        docPath =
//          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName)))
//        {
//            outputFile.WriteLine(descMemoryUsage);
//            outputFile.WriteLine(HEADER);
//            for (int i = 0; i < arrayLength; i++)
//            {
//                outputFile.Write((i + chunkSizeToTest_MIN_Included).ToString() + DELIMITER);
//                for (int j = 0; j < 4; j++)
//                    outputFile.Write(memoryUsage[i, j] + DELIMITER);
//                outputFile.WriteLine();
//            }
//        }
//        //----------------------------------------------------------------------------------




//        yield return null;
//    }

//    private void GenerateMeshForChunk(int i)
//    {
        
//        Vector3Int position = Vector3Int.zero;

//        Vector3Int dimensions = new(i, i, i);

//        Chunk chunk = new(
//            position,
//            dimensions,
//            transform,
//            i
//            );

//        i -= chunkSizeToTest_MIN_Included;

//        //-----------------------------------0----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), smoothNoShared);

//        smoothedTerrainProcessTime[i, 0] = chunk.GetTimeToGenerate().ToString();
//        terrainTotalVertexCount[i, 0] = chunk.GetTotalVertexCount().ToString();
//        memoryUsage[i, 0] = ((double)chunk.GetMeshMemoryUsage() / 1024).ToString();
//        //-----------------------------------1----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), smoothCubeShared);

//        smoothedTerrainProcessTime[i, 1] = chunk.GetTimeToGenerate().ToString();
//        terrainTotalVertexCount[i, 1] = chunk.GetTotalVertexCount().ToString();
//        memoryUsage[i, 1] = ((double)chunk.GetMeshMemoryUsage() / 1024).ToString();
//        //-----------------------------------2----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), smoothMeshShared);

//        smoothedTerrainProcessTime[i, 2] = chunk.GetTimeToGenerate().ToString();
//        terrainTotalVertexCount[i, 2] = chunk.GetTotalVertexCount().ToString();
//        memoryUsage[i, 2] = ((double)chunk.GetMeshMemoryUsage() / 1024).ToString();
//        //-----------------------------------3----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), smoothBothShared);

//        smoothedTerrainProcessTime[i, 3] = chunk.GetTimeToGenerate().ToString();
//        terrainTotalVertexCount[i, 3] = chunk.GetTotalVertexCount().ToString();
//        memoryUsage[i, 3] = ((double)chunk.GetMeshMemoryUsage() / 1024).ToString();
//        //----------------------------------------------------------------------------
//        //-----------------------------------4----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), roughNoShared);

//        roughTerrainProcessTime[i, 0] = chunk.GetTimeToGenerate().ToString();
//        //-----------------------------------5----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), roughCubeShared);

//        roughTerrainProcessTime[i, 1] = chunk.GetTimeToGenerate().ToString();
//        //-----------------------------------6----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), roughMeshShared);

//        roughTerrainProcessTime[i, 2] = chunk.GetTimeToGenerate().ToString();
//        //-----------------------------------7----------------------------------------
//        chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), roughBothShared);

//        roughTerrainProcessTime[i, 3] = chunk.GetTimeToGenerate().ToString();
//        //----------------------------------------------------------------------------

//        //yield return null;

//    }

//    public void WriteTrianglesToFile()
//    {
//        string fileName = "Triangles.txt";

//        string docPath =
//          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        List<int> triangles = new();
//        foreach (var chunk in chunks)
//        {
//            triangles = chunk.GetTriangles();
//            break;
//        }

//        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName)))
//        {
//            int i = 1;
//            foreach (var tri in triangles)
//            {
//                if (i == 1 || i == 2)
//                    outputFile.Write(tri.ToString() + "; ");
//                else
//                    outputFile.WriteLine(tri.ToString());
//                i++;
//                if (i == 4)
//                    i = 1;
//            }
//        }
//    }

//    public void WriteVertexIndexArrayToFile()
//    {
//        string fileName = "VertexIndexArray.txt";

//        string docPath =
//          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

//        int[,,,] vertexIndexArray = { };
//        foreach (var chunk in chunks)
//        {
//            vertexIndexArray = chunk.GetVertexIndexArray();
//            break;
//        }

//        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName)))
//            for (int x = 0; x < vertexIndexArray.GetLength(0); x++)
//                for (int y = 0; y < vertexIndexArray.GetLength(1); y++)
//                    for (int z = 0; z < vertexIndexArray.GetLength(2); z++)
//                    {
//                        for (int i = 0; i < 12; i++) 
//                        { 
//                            outputFile.Write(vertexIndexArray[x, y, z, i] + "; " );
//                        }
//                        outputFile.WriteLine();
//                    }
//    }

//    public void UpdateChunks()
//    {
//        foreach (var chunk in chunks)
//        {
//            chunk.GenerateTerrain(mapGenerator.GetGenerationParameters(), meshConstructParams);
//        }
//    }

//    private void CheckAndUpdateValues()
//    {
//        renderDistance = Mathf.Clamp(renderDistance, 2, 16);
//        meshConstructParams = new MeshConstructParams()
//        {
//            smoothTerrain = smoothTerrain,
//            meshSharedVertices = meshSharedVertices,
//            cubeSharedVertices = cubeSharedVertices,
//            LOD = LOD
//        };

//        if (LOD < 0 || LOD >= (int)chunkSize) LOD = 0;

//        chunkDimensions = new(
//            (int)Mathf.Pow(2, (int)chunkSize) - 1,
//            255,
//            (int)Mathf.Pow(2, (int)chunkSize) - 1
//            );
//    }

//    private void OnValidate()
//    {
//        renderDistance = Mathf.Clamp(renderDistance, 2, 16);
//        meshConstructParams = new MeshConstructParams()
//        {
//            smoothTerrain = smoothTerrain,
//            meshSharedVertices = meshSharedVertices,
//            cubeSharedVertices = cubeSharedVertices,
//            LOD = LOD
//        };

//        if (LOD < 0 || LOD >= (int)chunkSize) LOD = 0;

//        chunkDimensions = new(
//            (int)Mathf.Pow(2, (int)chunkSize) - 1,
//            255,
//            (int)Mathf.Pow(2, (int)chunkSize) - 1
//            );
//    }

//    void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireCube(new Vector3((float)chunkDimensions.x / 2, (float)chunkDimensions.y / 2, (float)chunkDimensions.z / 2), new Vector3(chunkDimensions.x, chunkDimensions.y, chunkDimensions.z));
//    }

//    readonly int chunkSizeToTest_MIN_Included = 8;
//    readonly int chunkSizeToTest_MAX_Excluded = 65;
//    int arrayLength;

//    const string descSmoothProcessTime= "This table shows the processing times for creating the mesh with surface smoothing enabled";
//    string[,] smoothedTerrainProcessTime;

//    const string descRoughProcessTime = "This table shows the processing times for creating the mesh with surface smoothing disabled";
//    string[,] roughTerrainProcessTime;

//    const string descTotalVertexCount = "This table shows the total vertex count for the mesh";
//    string[,] terrainTotalVertexCount;

//    const string descMemoryUsage = "This table shows the memory used the mesh";
//    string[,] memoryUsage;

//    const char DELIMITER = ';';

//    readonly string HEADER = 
//        "Dimensions (cube)" + DELIMITER + 
//        "No vertices shared" + DELIMITER + 
//        "Cube verties shared" + DELIMITER + 
//        "Mesh vertices shared" + DELIMITER + 
//        "All vertices shared" + DELIMITER;

//    MeshConstructParams smoothNoShared = new()
//    {
//        smoothTerrain = true,
//        cubeSharedVertices = false,
//        meshSharedVertices = false,
//        LOD = 0,
//    };

//    MeshConstructParams smoothCubeShared = new()
//    {
//        smoothTerrain = true,
//        cubeSharedVertices = true,
//        meshSharedVertices = false,
//        LOD = 0,
//    };

//    MeshConstructParams smoothMeshShared = new()
//    {
//        smoothTerrain = true,
//        cubeSharedVertices = false,
//        meshSharedVertices = true,
//        LOD = 0,
//    };

//    MeshConstructParams smoothBothShared = new()
//    {
//        smoothTerrain = true,
//        cubeSharedVertices = true,
//        meshSharedVertices = true,
//        LOD = 0,
//    };

//    MeshConstructParams roughNoShared = new()
//    {
//        smoothTerrain = false,
//        cubeSharedVertices = false,
//        meshSharedVertices = false,
//        LOD = 0,
//    };

//    MeshConstructParams roughCubeShared = new()
//    {
//        smoothTerrain = false,
//        cubeSharedVertices = true,
//        meshSharedVertices = false,
//        LOD = 0,
//    };

//    MeshConstructParams roughMeshShared = new()
//    {
//        smoothTerrain = false,
//        cubeSharedVertices = false,
//        meshSharedVertices = true,
//        LOD = 0,
//    };

//    MeshConstructParams roughBothShared = new()
//    {
//        smoothTerrain = false,
//        cubeSharedVertices = true,
//        meshSharedVertices = true,
//        LOD = 0,
//    };

//}
