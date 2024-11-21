using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    private int spawnDistance;
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

    public ChunkLoader(Transform world, Transform player, int spawnDistance)
    {
        this.spawnDistance = spawnDistance;
        despawnDistance = spawnDistance + 6;
        outerDiameter = despawnDistance * 2 + 1;
        center2 = new(despawnDistance, despawnDistance);
        center3 = new(despawnDistance, World.verticalChunks / 2, despawnDistance);

        renderedChunks = new CubicChunk[outerDiameter, World.verticalChunks, outerDiameter];

        innerRing = CalculateRing(spawnDistance, center2, outerDiameter);
        outerRing = CalculateRing(despawnDistance, center2, outerDiameter);
        circle = CalculateCircle(spawnDistance, center2, outerDiameter);

        chunkPool = new ChunkPool();

        this.player = player;
        this.world = world;
    }

    void Start()
    {

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
    }

    void Update()
    {
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

    public Vector3Int GetIndexOfChunkByChunkPosition(Vector3Int chunkPosition)
    {
        return center3 - ((renderedChunks[center3.x, center3.y, center3.z].Position - chunkPosition) / CubicChunk.cubesPerAxis);
    }

    public Vector3Int GetIndexOfChunkAtPosition(Vector3Int positionInWorld)
    {
        Vector3Int chunkPosition = HelperFunctions.Vector3FloorToNearestMultipleOf(positionInWorld, CubicChunk.cubesPerAxis);
        return GetIndexOfChunkByChunkPosition(chunkPosition);
    }

    public CubicChunk GetChunkAtPosition(Vector3Int positionInWorld)
    {
        Vector3Int indexOfChunk = GetIndexOfChunkAtPosition(positionInWorld);
        return renderedChunks[indexOfChunk.x, indexOfChunk.y, indexOfChunk.z];
    }

    public CubicChunk GetChunkByIndex(Vector3Int index)
    {
        return renderedChunks[index.x, index.y, index.z];
    }
}
