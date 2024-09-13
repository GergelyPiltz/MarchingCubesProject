using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private int spawnDistance;
    private int despawnDistance;
    //private int innerDiameter;
    private int outerDiameter;
    private Vector2Int center2;
    private Vector3Int center3;

    private GameObject player;
    private Transform playerTransform;

    public const int verticalChunks = 16;
    public const int worldHeight = CubicChunk.cubesPerAxis * verticalChunks;

    [Header("2D")]
    [SerializeField] float startFrequency2D;
    [SerializeField] float frequencyModifier2D;
    [SerializeField] float amplitudeModifier2D;
    [SerializeField] int octaves2D;

    [Header("3D")]
    [SerializeField] float startFrequency3D;
    [SerializeField] float frequencyModifier3D;
    [SerializeField] float amplitudeModifier3D;
    [SerializeField] int octaves3D;

    [Header("Terrain Profile")]
    [SerializeField] CubicChunk.TerrainProfile terrainProfile;

    private bool dynamicRender = true;

    void Start()
    {
        despawnDistance = spawnDistance + 6;
        //innerDiameter = spawnDistance * 2 + 1;
        outerDiameter = despawnDistance * 2 + 1;
        center2 = new(despawnDistance, despawnDistance);
        center3 = new(despawnDistance, verticalChunks / 2, despawnDistance);

        renderedChunks = new CubicChunk[outerDiameter, verticalChunks, outerDiameter];

        CubicChunk.SetTerrainProfile(terrainProfile);

        //if (generateIndicator)
        //    generateIndicator.localScale = new Vector3 (CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis);
        //if (removeIndicator)
        //    removeIndicator.localScale = new Vector3(CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis);

        player = GameObject.Find("Player");
        if (player == null)
            dynamicRender = false;
        else
            playerTransform = player.transform;

        poolObj = new("Pool (0)");
        poolTransform = poolObj.transform;

        worldObj = new("Active (0)");
        worldTransform = worldObj.transform;

        innerRing = CalculateRing(spawnDistance, center2, outerDiameter);
        outerRing = CalculateRing(despawnDistance, center2, outerDiameter);
        circle = CalculateCircle(spawnDistance, center2, outerDiameter);

        foreach (Vector2Int offset in circle)
            for (int y = 0; y < verticalChunks; y++)
            {
                CubicChunk cubicChunk = GetFromPool();
                renderedChunks[offset.x, y, offset.y] = cubicChunk;
                Vector2Int horizontalPos = /*playerChunk + */(offset - center2) * CubicChunk.cubesPerAxis; // playerchunk is (0, 0)
                Vector3Int position = new(horizontalPos.x, y * CubicChunk.cubesPerAxis, horizontalPos.y);
                cubicChunk.Move(position);
                chunksToBuild.Add(cubicChunk);
            }
        
        
        StartCoroutine(PlacePlayer());
    }

    private IEnumerator PlacePlayer()
    {
        yield return new WaitForSeconds(1);
        RaycastHit hit;
        while (!Physics.Raycast(new Vector3(0, worldHeight, 0), Vector3.down, out hit))
            yield return new WaitForSeconds(1);
        playerTransform.position = hit.point + new Vector3(0, 2, 0);
    }

    private List<Vector2Int> innerRing;
    private List<Vector2Int> outerRing;
    private List<Vector2Int> circle;
    private CubicChunk[,,] renderedChunks;
    private GameObject poolObj;
    private Transform poolTransform;
    private GameObject worldObj;
    private Transform worldTransform;
    private List<CubicChunk> pool = new();
    private int inPool = 0;
    private int inWorld = 0;
    private List<CubicChunk> chunksToBuild = new();
    private List<CubicChunk> finishedBuild = new();
    private readonly int buildsPerFrame = 10;
    private Vector2Int oldPos = new (0, 0);
    private Vector2Int playerChunk;
    void Update()
    {
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
        playerChunk = Vector2FloorToNearestMultipleOf(ToVector2FromXZ(playerTransform.position), CubicChunk.cubesPerAxis);
        if (playerChunk == oldPos) return;

        if (playerChunk.x > oldPos.x)
            for (int x = 0; x < outerDiameter - 1; x++)
                for (int z = 0; z < outerDiameter; z++)
                    for (int y = 0; y < verticalChunks; y++)
                        renderedChunks[x, y, z] = renderedChunks[x + 1, y, z];
        else if (playerChunk.x < oldPos.x)
            for (int x = outerDiameter - 2; x >= 0; x--)
                for (int z = 0; z < outerDiameter; z++)
                    for (int y = 0; y < verticalChunks; y++)
                        renderedChunks[x + 1, y, z] = renderedChunks[x, y, z];
                
        if (playerChunk.y > oldPos.y)
            for (int x = 0; x < outerDiameter; x++)
                for (int z = 0; z < outerDiameter - 1; z++)
                    for (int y = 0; y < verticalChunks; y++)
                        renderedChunks[x, y, z] = renderedChunks[x, y, z + 1];
        else if (playerChunk.y < oldPos.y)
            for (int x = 0; x < outerDiameter; x++)
                for (int z = outerDiameter - 2; z >= 0; z--)
                    for (int y = 0; y < verticalChunks; y++)
                        renderedChunks[x, y, z + 1] = renderedChunks[x, y, z];
                
        oldPos = playerChunk;

        foreach (Vector2Int offset in innerRing)
        {
            if (renderedChunks[offset.x, 0, offset.y] == null)
            {
                Vector2Int horizontalPos = playerChunk + (offset - center2) * CubicChunk.cubesPerAxis;
                for (int y = 0; y < verticalChunks; y++)
                {
                    CubicChunk c = GetFromPool();
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
                for (int y = 0; y < verticalChunks; y++)
                {
                    ReturnToPool(renderedChunks[offset.x, y, offset.y]);
                    renderedChunks[offset.x, y, offset.y] = null;
                }
            }
        }
            

    }

    private void OnApplicationQuit()
    {
        CubicChunk.ReleaseBuffers();
    }

    private CubicChunk GetFromPool()
    {
        CubicChunk c;
        if (inPool > 0)
        {
            inPool--;
            poolObj.name = "Pool (" + inPool + ")";

            c = pool[0];
            pool.RemoveAt(0);
            c.SetActive(true);
        }
        else
            c = new(worldTransform);

        inWorld++;
        worldObj.name = "Active (" + inWorld + ")";
        c.chunkTransform.parent = worldTransform;
        return c;
    }

    private void ReturnToPool(CubicChunk c)
    {
        inPool++;
        poolObj.name = "Pool (" + inPool + ")";
        if (inWorld > 0)
        {
            inWorld--;
            worldObj.name = "Active (" + inWorld + ")";
        }

        c.SetActive(false);
        c.chunkTransform.parent = poolTransform;
        pool.Add(c);
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

    private List<Vector3Int[]> CreateVariationsWithOffset(Vector3Int pos)
    {
        List<Vector3Int[]> variations = new (){ new []{ pos, new(0, 0, 0) } };
        //List<Vector3Int> offsets = new(){ new(0, 0, 0) };

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

    private Vector3Int GetIndexOfChunk(Vector3Int chunkPos)
    {
        return center3 - ((renderedChunks[center3.x, center3.y, center3.z].Position - chunkPos) / CubicChunk.cubesPerAxis);
    }

    public void ModifyBlock(Vector3 pos, bool place)
    {
        
        Vector3Int chunkPos = Vector3FloorToNearestMultipleOf(pos, CubicChunk.cubesPerAxis);
        Debug.Log("NearestMultipleOf result: " + chunkPos);
        Vector3Int chunkIndex = GetIndexOfChunk(chunkPos);

        Vector3Int coordInChunk = Vector3Int.FloorToInt(pos) - chunkPos;

        for (int i = 0; i < 3; i++)
            if (coordInChunk[i] < 0)
                coordInChunk[i] = CubicChunk.cubesPerAxis - Mathf.Abs(coordInChunk[i]);

        Debug.Log("Pos: " + pos + " chunkPos: " + chunkPos + " position in chunk: " + coordInChunk);

        Vector3Int[] corners = new Vector3Int[8];
        for (int i = 0; i < 8; i++)
            corners[i] = coordInChunk + Tables.CornerTable[i];

        
        for (int i = 0; i < 8; i++)
        {
            List<Vector3Int[]> variations = CreateVariationsWithOffset(corners[i]);

            CubicChunk c = null;
            foreach (Vector3Int[] v in variations)
            {
                Vector3Int indexWithOffset = chunkIndex + v[1];

                c = renderedChunks[indexWithOffset.x, indexWithOffset.y, indexWithOffset.z];
                if (c != null)
                {
                    Debug.Log(v[0] + " - " + v[1]);
                    if (place)
                    {
                        c.OverwriteTerrainValue(v[0], -1);
                        c.RecalculateMesh();
                    }
                    else
                    {
                        c.OverwriteTerrainValue(v[0], 1);
                        c.RecalculateMesh();
                    }
                }
                else
                    Debug.Log("No chunk found at " + chunkPos + "with index " + chunkIndex);
            }
        }

        

    }

    public Vector2 ToVector2FromXZ(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public Vector2Int ToVector2FromXZ(Vector3Int v)
    {
        return new Vector2Int(v.x, v.z);
    }

    /// <summary>
    /// Floors each component of a Vector3Int to the nearest multiple of the specified value.
    /// Example: (-4,15,17) with 5 results in (-5,15,15)
    /// </summary>
    /// <param name="vector"> Vector3Int to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    Vector3Int Vector3FloorToNearestMultipleOf(Vector3Int vector, int multipleOf)
    {
        int x, y, z;

        if (vector.x > 0)
            x = vector.x - vector.x % multipleOf;
        else if (vector.x < 0)
            x = vector.x - (multipleOf + vector.x % multipleOf);
        else
            x = 0;

        if (vector.y > 0)
            y = vector.y - vector.y % multipleOf;
        else if (vector.y < 0)
            y = vector.y - (multipleOf + vector.y % multipleOf);
        else
            y = 0;

        if (vector.z > 0)
            z = vector.z - vector.z % multipleOf;
        else if (vector.z < 0)
            z = vector.z - (multipleOf + vector.z % multipleOf);
        else
            z = 0;

        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Floors each component of a Vector3 to the nearest multiple of the specified value.
    /// Example: (-4,15,17) with 5 results in (-5,15,15)
    /// </summary>
    /// <param name="vector"> Vector3 to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    private Vector3Int Vector3FloorToNearestMultipleOf(Vector3 vector, int multipleOf)
    {
        return Vector3FloorToNearestMultipleOf(Vector3Int.FloorToInt(vector), multipleOf);
    }

    /// <summary>
    /// Floors each component of a Vector2Int to the nearest multiple of the specified value.
    /// Example: (-4,17) with 5 results in (-5,15)
    /// </summary>
    /// <param name="vector"> Vector2Int to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    private Vector2Int Vector2FloorToNearestMultipleOf(Vector2Int vector, int multipleOf)
    {
        int x, y;

        if (vector.x > 0)
            x = vector.x - vector.x % multipleOf;
        else if (vector.x < 0)
            x = vector.x - (multipleOf + vector.x % multipleOf);
        else
            x = 0;

        if (vector.y > 0)
            y = vector.y - vector.y % multipleOf;
        else if (vector.y < 0)
            y = vector.y - (multipleOf + vector.y % multipleOf);
        else
            y = 0;

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Floors each component of a Vector2 to the nearest multiple of the specified value.
    /// Example: (-4,17) with 5 results in (-5,15)
    /// </summary>
    /// <param name="vector"> Vector2 to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    private Vector2Int Vector2FloorToNearestMultipleOf(Vector2 vector, int multipleOf)
    {
        return Vector2FloorToNearestMultipleOf(Vector2Int.FloorToInt(vector), multipleOf);
    }


}