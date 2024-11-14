using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private int spawnDistance;

    //-------------------
    //private int spawnDistance;
    private int despawnDistance;
    private int outerDiameter;
    private Vector2Int center2;
    private Vector3Int center3;

    private List<Vector2Int> innerRing;
    private List<Vector2Int> outerRing;
    private List<Vector2Int> circle;

    private CubicChunk[,,] renderedChunks;

    private List<CubicChunk> chunksToBuild = new();
    private List<CubicChunk> finishedBuild = new();
    private readonly int buildsPerFrame = 10;
    private Vector2Int oldPos = new(0, 0);
    private Vector2Int playerChunk;

    private ChunkPool chunkPool;
    private Transform world;
    private Transform player;
    //-------------------

    public const int verticalChunks = 16;
    public const int worldHeight = CubicChunk.cubesPerAxis * verticalChunks;

    [Header("Terrain Profile")]
    [SerializeField] CubicChunk.TerrainProfile terrainProfile;

    private bool dynamicRender = true;

    void Start()
    {
        CubicChunk.SetTerrainProfile(terrainProfile);

        try
        {
            player = GameObject.Find("Player").transform;
        }
        catch (NullReferenceException)
        {
            dynamicRender = false;
        }

        //-------------------------
        world = transform;
        despawnDistance = spawnDistance + 6;
        outerDiameter = despawnDistance * 2 + 1;
        center2 = new(despawnDistance, despawnDistance);
        center3 = new(despawnDistance, World.verticalChunks / 2, despawnDistance);

        renderedChunks = new CubicChunk[outerDiameter, World.verticalChunks, outerDiameter];

        innerRing = CalculateRing(spawnDistance, center2, outerDiameter);
        outerRing = CalculateRing(despawnDistance, center2, outerDiameter);
        circle = CalculateCircle(spawnDistance, center2, outerDiameter);

        chunkPool = new ChunkPool();
        //-------------------------


        //-------------------------
        foreach (Vector2Int offset in circle)
            for (int y = 0; y < World.verticalChunks; y++)
            {
                CubicChunk cubicChunk = chunkPool.GetFromPool(world);
                renderedChunks[offset.x, y, offset.y] = cubicChunk;
                Vector2Int horizontalPos = /*playerChunk + */(offset - center2) * CubicChunk.cubesPerAxis; // playerchunk is (0, 0)
                Vector3Int position = new(horizontalPos.x, y * CubicChunk.cubesPerAxis, horizontalPos.y);
                cubicChunk.Move(position);
                chunksToBuild.Add(cubicChunk);
            }
        //-------------------------

        StartCoroutine(PlacePlayer());
    }

    private IEnumerator PlacePlayer()
    {
        yield return new WaitForSeconds(1);
        RaycastHit hit;
        while (!Physics.Raycast(new Vector3(0, worldHeight, 0), Vector3.down, out hit))
            yield return new WaitForSeconds(1);
        player.position = hit.point + new Vector3(0, 2, 0);
    }

    private float chunkManagementTimeTotal = 0f;
    private int avgCounter = 0;
    private int cycles = 0;

    void Update()
    {
        transform.name = "World (" + transform.childCount + ")";

        //Debug.Log("Runtimes:" +
        //    " Shader Max: " + CubicChunk.maxComputeTime.ToString("0.000000") + 
        //    " Shader Avg: " + CubicChunk.avgComputeTime.ToString("0.000000") +
        //    " Build Max: " + CubicChunk.maxBuildTime.ToString("0.000000") +
        //    " Build Avg: " + CubicChunk.avgBuildTime.ToString("0.000000")
        //    );

        if (!dynamicRender) return;

        // Build the chunks
        int counter = 0;
        foreach (var c in chunksToBuild)
        {
            c.Build();
            finishedBuild.Add(c);
            counter++;
            if (counter == buildsPerFrame) break;
        }

        foreach (var c in finishedBuild)
            chunksToBuild.Remove(c);
        finishedBuild.Clear();

        // Check if the player moved chunks
        playerChunk = HelperFunctions.Vector2FloorToNearestMultipleOf
            (
            HelperFunctions.ToVector2FromXZ(player.position),
            CubicChunk.cubesPerAxis
            );

        if (playerChunk == oldPos) return;

        float chunkManagementTime = Time.realtimeSinceStartup;

        if (playerChunk.x > oldPos.x)
            for (int x = 0; x < outerDiameter - 1; x++)
                for (int z = 0; z < outerDiameter; z++)
                    for (int y = 0; y < World.verticalChunks; y++)
                        renderedChunks[x, y, z] = renderedChunks[x + 1, y, z];
        else if (playerChunk.x < oldPos.x)
            for (int x = outerDiameter - 2; x >= 0; x--)
                for (int z = 0; z < outerDiameter; z++)
                    for (int y = 0; y < World.verticalChunks; y++)
                        renderedChunks[x + 1, y, z] = renderedChunks[x, y, z];

        if (playerChunk.y > oldPos.y)
            for (int x = 0; x < outerDiameter; x++)
                for (int z = 0; z < outerDiameter - 1; z++)
                    for (int y = 0; y < World.verticalChunks; y++)
                        renderedChunks[x, y, z] = renderedChunks[x, y, z + 1];
        else if (playerChunk.y < oldPos.y)
            for (int x = 0; x < outerDiameter; x++)
                for (int z = outerDiameter - 2; z >= 0; z--)
                    for (int y = 0; y < World.verticalChunks; y++)
                        renderedChunks[x, y, z + 1] = renderedChunks[x, y, z];

        oldPos = playerChunk;

        foreach (Vector2Int offset in innerRing)
        {
            if (renderedChunks[offset.x, 0, offset.y] == null)
            {
                Vector2Int horizontalPos = playerChunk + (offset - center2) * CubicChunk.cubesPerAxis;
                for (int y = 0; y < World.verticalChunks; y++)
                {
                    CubicChunk c = chunkPool.GetFromPool(world);
                    renderedChunks[offset.x, y, offset.y] = c;
                    Vector3Int pos = new(horizontalPos.x, y * CubicChunk.cubesPerAxis, horizontalPos.y);
                    c.Move(pos);
                    chunksToBuild.Add(c);
                }
            }
        }

        foreach (Vector2Int offset in outerRing)
        {
            if (renderedChunks[offset.x, 0, offset.y] != null)
            {
                for (int y = 0; y < World.verticalChunks; y++)
                {
                    chunkPool.ReturnToPool(renderedChunks[offset.x, y, offset.y]);
                    renderedChunks[offset.x, y, offset.y] = null;
                }
            }
        }

        chunkManagementTime = Time.realtimeSinceStartup - chunkManagementTime;

        cycles++;
        if (cycles > despawnDistance + 2)
        {
            avgCounter++;
            chunkManagementTimeTotal += chunkManagementTime;
            Debug.Log("Chunk Management Time: " + chunkManagementTimeTotal / avgCounter);
        }


    }

    void OnApplicationQuit()
    {
        CubicChunk.ReleaseBuffers();
    }

    private List<Vector3Int[]> CreateVariationsWithOffset(Vector3Int pos)
    {
        List<Vector3Int[]> variations = new (){ new []{ pos, new(0, 0, 0) } };

        for (int i = 0; i < 3; i++)
        {
            if (pos[i] == 0)
            {
                int count = variations.Count;
                for (int j = 0; j < count; j++)
                {
                    Vector3Int v = variations[j][0];
                    v[i] = 16;
                    Vector3Int o = variations[j][1];
                    o[i] = -1;
                    variations.Add(new[] { v, o });
                }
            }
            else if (pos[i] == 16)
            {
                int count = variations.Count;
                for (int j = 0; j < count; j++)
                {
                    Vector3Int v = variations[j][0];
                    v[i] = 0;
                    Vector3Int o = variations[j][1];
                    o[i] = +1;
                    variations.Add(new[] { v, o });
                }
            }
        }

        return variations;

        //Vector3Int[] variations = new Vector3Int[8];

        //for (int i = 0; i < 3; i++)
        //    if (pos[i] == 0 || pos[i] == 16)
        //        for (int j = 0; j < 8; j++)
        //            variations[j][i] = 16 * Tables.CornerTable[j][i]; // CornerTable is technically all variations of a 3 component binary vector
        //    else
        //        for (int j = 0; j < 8; j++)
        //            variations[j][i] = pos[i];

        //return variations.Distinct();
    }

    public void ModifyBlock(Vector3 pos, bool place)
    {
        Vector3Int posInt = Vector3Int.FloorToInt(pos);
        Vector3Int chunkPosition = HelperFunctions.Vector3FloorToNearestMultipleOf(posInt, CubicChunk.cubesPerAxis);
        Vector3Int indexOfChunk = GetIndexOfChunkByChunkPosition(chunkPosition);
        Vector3Int coordInChunk = posInt - chunkPosition;

        for (int i = 0; i < 3; i++)
            if (coordInChunk[i] < 0)
                coordInChunk[i] = CubicChunk.cubesPerAxis - Mathf.Abs(coordInChunk[i]);

        Debug.Log("Pos: " + pos + " ChunkPos: " + chunkPosition + " Position in chunk: " + coordInChunk);

        Vector3Int[] corners = new Vector3Int[8];
        for (int i = 0; i < 8; i++)
            corners[i] = coordInChunk + Tables.CornerTable[i];

        
        for (int i = 0; i < 8; i++)
        {
            List<Vector3Int[]> variations = CreateVariationsWithOffset(corners[i]);

            CubicChunk chunk = null;
            foreach (Vector3Int[] v in variations)
            {
                Vector3Int indexWithOffset = indexOfChunk + v[1];

                chunk = GetChunkByIndex(indexWithOffset);
                if (chunk != null)
                {
                    Debug.Log(v[0] + " - " + v[1]);
                    if (place)
                    {
                        chunk.OverwriteTerrainValue(v[0], -1);
                        chunk.RecalculateMesh();
                    }
                    else
                    {
                        chunk.OverwriteTerrainValue(v[0], 1);
                        chunk.RecalculateMesh();
                    }
                }
                else
                    Debug.Log("No chunk found at " + chunkPosition);
            }
        }
    }

    private List<Vector2Int> CalculateRing(int radius, Vector2Int center, int gridSize)
    {
        List<Vector2Int> ring = new();
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int pos = new(x, y);
                float dist = Vector2Int.Distance(center, pos);
                if (dist < radius + 1 && dist >= radius + 1 - 2)
                    ring.Add(pos);
            }

        return ring;
    }

    private List<Vector2Int> CalculateCircle(int radius, Vector2Int center, int gridSize)
    {
        Dictionary<Vector2Int, float> coords = new();
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int pos = new(x, y);
                float dist = Vector2Int.Distance(center, pos);
                if (dist <= radius)
                    coords.Add(pos, dist);
            }

        var ordered = coords.OrderBy(x => x.Value);
        List<Vector2Int> circle = new();
        foreach (var item in ordered)
            circle.Add(item.Key);

        return circle;
    }

    private Vector3Int GetIndexOfChunkByChunkPosition(Vector3Int chunkPosition)
    {
        return center3 - ((renderedChunks[center3.x, center3.y, center3.z].Position - chunkPosition) / CubicChunk.cubesPerAxis);
    }

    private Vector3Int GetIndexOfChunkAtPosition(Vector3Int positionInWorld)
    {
        Vector3Int chunkPosition = HelperFunctions.Vector3FloorToNearestMultipleOf(positionInWorld, CubicChunk.cubesPerAxis);
        return GetIndexOfChunkByChunkPosition(chunkPosition);
    }

    private CubicChunk GetChunkAtPosition(Vector3Int positionInWorld)
    {
        Vector3Int indexOfChunk = GetIndexOfChunkAtPosition(positionInWorld);
        return renderedChunks[indexOfChunk.x, indexOfChunk.y, indexOfChunk.z];
    }

    private CubicChunk GetChunkByIndex(Vector3Int index)
    {
        return renderedChunks[index.x, index.y, index.z];
    }

    int minChunks = 0;
    int maxChunks = 0;
    public bool WorldTest()
    {
        if (minChunks == 0)
        {
            for (int x = -spawnDistance; x < spawnDistance + 1; x++)
                for (int y = -spawnDistance; y < spawnDistance + 1; y++)
                {
                    Vector2Int v = new (x, y);
                    if (Vector2.Distance(Vector2.zero, v) < spawnDistance)
                    {
                        minChunks++;
                    }
                }
            minChunks *= verticalChunks;
        }

        if (maxChunks == 0)
        {
            for (int x = -despawnDistance; x < despawnDistance + 1; x++)
                for (int y = -despawnDistance; y < despawnDistance + 1; y++)
                {
                    Vector2Int v = new (x, y);
                    if (Vector2.Distance(Vector2.zero, v) < despawnDistance)
                    {
                        maxChunks++;
                    }
                }
            maxChunks *= verticalChunks;
        }

        
        

        Debug.Log("min " + minChunks + " max " + maxChunks);

        int count = 0;
        foreach (var chunk in renderedChunks)
            if (chunk != null)
                count++;


        if (count < minChunks)
        {
            Debug.Log("Less than min chunks");
            return false;
        }

        if (count > maxChunks)
        {
            Debug.Log("More than max chunks");
            return false;
        }

        foreach (var chunk in renderedChunks)
            if (chunk != null)
                if (Vector2.Distance(HelperFunctions.ToVector2FromXZ(chunk.Position), playerChunk) > despawnDistance * CubicChunk.cubesPerAxis)
                {
                    Debug.Log("Distance error");
                    return false;
                }

        return true;
    }



}