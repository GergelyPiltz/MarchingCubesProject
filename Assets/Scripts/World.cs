using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private int radius;
    private int sqrMaxRenderRadius;

    private GameObject player;
    private Transform playerTransform;

    public const int verticalChunks = 16;
    public const int worldHeight = CubicChunk.cubesPerAxis * verticalChunks;


    [SerializeField] Transform generateIndicator;
    [SerializeField] Transform removeIndicator;

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

        poolObj = new()
        {
            name = "Pool (0)"
        };
        poolTransform = poolObj.transform;

        activeObj = new()
        {
            name = "Active (0)"
        };
        activeTransform = activeObj.transform;

        ring = CalculateRing(radius, CubicChunk.cubesPerAxis);
        circle = CalculateCircle(radius, CubicChunk.cubesPerAxis);
        sqrMaxRenderRadius = (int) Mathf.Pow((radius + 2) * CubicChunk.cubesPerAxis, 2);

        CubicChunk[] column;
        foreach (Vector2Int offset in circle)
        {
            Vector2Int c = playerChunkPos + offset;

            column = new CubicChunk[verticalChunks];
            for (int y = 0; y < verticalChunks; y++)
            {
                CubicChunk cubicChunk = GetFromPool();
                column[y] = cubicChunk;
                Vector3Int position = new(c.x, y * CubicChunk.cubesPerAxis, c.y);
                cubicChunk.Move(position);
                chunksToBuild.Add(cubicChunk);
            }
            renderedColumns.Add(c, column);
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

    private List<Vector2Int> ring;
    private List<Vector2Int> circle;
    private List<Vector2Int> oldColumns = new();
    private Dictionary<Vector2Int, CubicChunk[]> renderedColumns = new();
    private GameObject poolObj;
    private Transform poolTransform;
    private GameObject activeObj;
    private Transform activeTransform;
    private List<CubicChunk> pool = new();
    private int inPool = 0;
    private int inWorld = 0;
    private List<CubicChunk> chunksToBuild = new();
    private List<CubicChunk> finishedBuild = new();
    private readonly int buildsPerFrame = 10;
    private Vector2Int playerChunkPos = new (0, 0);
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
        Vector2Int newPos = Vector2FloorToNearestMultipleOf(ToVector2FromXZ(playerTransform.position), CubicChunk.cubesPerAxis);
        if (newPos == playerChunkPos)
            return;
        else
            playerChunkPos = newPos;

        // Check all chunks for ones outside render distance 
        foreach (var c in renderedColumns)
            if ((playerChunkPos - c.Key).sqrMagnitude > sqrMaxRenderRadius)
                oldColumns.Add(c.Key);

        // Dispose of chunks outside render distance
        foreach (Vector2Int c in oldColumns)
            if (renderedColumns.TryGetValue(c, out CubicChunk[] column))
            {
                renderedColumns.Remove(c);
                DisposeOfColumn(column);
            }
        oldColumns.Clear();

        // Check outer ring in rendered chunks for missing ones
        foreach (Vector2Int offset in ring)
        {
            Vector2Int c = playerChunkPos + offset;
            if (!renderedColumns.ContainsKey(c))
                renderedColumns.Add(c, CreateColumn(c));
        }

    }

    private void OnApplicationQuit()
    {
        CubicChunk.ReleaseBuffers();
    }

    private CubicChunk[] CreateColumn(Vector2Int pos)
    {
        CubicChunk[] column = new CubicChunk[verticalChunks];
        for (int y = 0; y < verticalChunks; y++)
        {
            CubicChunk cubicChunk = GetFromPool();
            column[y] = cubicChunk;
            cubicChunk.Move(new(pos.x, y * CubicChunk.cubesPerAxis, pos.y));
            chunksToBuild.Add(cubicChunk);
        }
        return column;
    }

    private void DisposeOfColumn(CubicChunk[] column)
    {
        foreach (var c in column)
            ReturnToPool(c);
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
            c = new(activeTransform);

        inWorld++;
        activeObj.name = "Active (" + inWorld + ")";
        c.chunkTransform.parent = activeTransform;
        return c;
    }

    private void ReturnToPool(CubicChunk c)
    {
        inPool++;
        poolObj.name = "Pool (" + inPool + ")";
        if (inWorld > 0)
        {
            inWorld--;
            activeObj.name = "Active (" + inWorld + ")";
        }

        c.SetActive(false);
        c.chunkTransform.parent = poolTransform;
        pool.Add(c);
    }

    private List<Vector2Int> CalculateRing(int radius, int multiplier)
    {
        List<Vector2Int> ring = new();
        for (int x = -radius + 1; x < radius; x++)
            for (int z = -radius + 1; z < radius; z++)
            {
                Vector2Int pos = new(x, z);
                float dist = Vector2Int.Distance(Vector2Int.zero, pos);
                if (dist <= radius && dist >= radius - 2)
                    ring.Add(pos * multiplier);
            }

        return ring;
    }

    private List<Vector2Int> CalculateCircle(int radius, int multiplier)
    {
        Dictionary<Vector2Int, float> coords = new();
        for (int x = -radius + 1; x < radius; x++)
            for (int z = -radius + 1; z < radius; z++)
            {
                Vector2Int pos = new(x, z);
                float dist = Vector2Int.Distance(Vector2Int.zero, pos);
                if (dist <= radius)
                    coords.Add(pos, dist);
            }

        var ordered = coords.OrderBy(x => x.Value);
        List<Vector2Int> circle = new();
        foreach (var item in ordered)
            circle.Add(item.Key * multiplier);

        return circle;
    }

    private List<Vector3Int> CreateVariations(Vector3Int pos)
    {
        List<Vector3Int> variations = new(){ pos };

        for (int i = 0; i < 3; i++)
        {
            if (pos[i] == 0)
            {
                int count = variations.Count;
                for (int j = 0; j < count; j++)
                {
                    Vector3Int v = variations[j];
                    v[i] = 16;
                    variations.Add(v);
                }
            }
            else if (pos[i] == 16)
            {
                int count = variations.Count;
                for (int j = 0; j < count; j++)
                {
                    Vector3Int v = variations[j];
                    v[i] = 0;
                    variations.Add(v);
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

    //public Vector3Int[] GetChunkKeys(Vector3 pos)
    //{
    //    Vector3Int posInt = Vector3Int.FloorToInt(pos);

    //    Vector3Int[] corners = new Vector3Int[8];
    //    for (int i = 0; i < 8; i++)
    //        corners[i] = posInt + Tables.CornerTable[i];

    //    Vector3Int[] overlaps = new Vector3Int[8];
    //    for (int i = 0; i < 8; i++)
    //        for (int j = 0; j < 3; j++)
    //            if (corners[i][j] == 16)
    //                corners[i][j] = 0;





    //    int modX = posInt.x % CubicChunk.cubesPerAxis;
    //    int modY = posInt.y % CubicChunk.cubesPerAxis;
    //    int modZ = posInt.z % CubicChunk.cubesPerAxis;

    //    bool negBorderOnX = modX == 0;
    //    bool negBorderOnY = modY == 0;
    //    bool negBorderOnZ = modZ == 0;

    //    bool posBorderOnX = modX == 0;
    //    bool posBorderOnY = modY == 0;
    //    bool posBorderOnZ = modZ == 0;

    //    int keyX = posInt.x - modX;
    //    int keyY = posInt.y - modY;
    //    int keyZ = posInt.z - modZ;

    //    List<Vector3Int> keys = new()
    //    {
    //        new(keyX, keyY, keyZ)
    //    };

    //    if (negBorderOnX)
    //        keys.Add(new(keyX - CubicChunk.cubesPerAxis, keyY, keyZ));
    //    if (negBorderOnY)
    //        keys.Add(new(keyX, keyY - CubicChunk.cubesPerAxis, keyZ));
    //    if (negBorderOnZ)
    //        keys.Add(new(keyX, keyY, keyZ - CubicChunk.cubesPerAxis));

    //    if (posBorderOnX)
    //        keys.Add(new(keyX + CubicChunk.cubesPerAxis, keyY, keyZ));
    //    if (posBorderOnY)
    //        keys.Add(new(keyX, keyY + CubicChunk.cubesPerAxis, keyZ));
    //    if (posBorderOnZ)
    //        keys.Add(new(keyX, keyY, keyZ + CubicChunk.cubesPerAxis));

    //    //public static readonly Vector3Int[] CornerTable = new Vector3Int[] {
    //    //    new (0, 0, 0),
    //    //    new (1, 0, 0),
    //    //    new (1, 1, 0),
    //    //    new (0, 1, 0),
    //    //    new (0, 0, 1),
    //    //    new (1, 0, 1),
    //    //    new (1, 1, 1),
    //    //    new (0, 1, 1)
    //    //};


    //}

    public CubicChunk GetChunk(Vector3 pos)
    {
        Vector3Int chunkPos = Vector3FloorToNearestMultipleOf(pos, CubicChunk.cubesPerAxis);
        if(renderedColumns.TryGetValue(ToVector2FromXZ(chunkPos), out CubicChunk[] column))
        {
            Debug.Log(chunkPos.y / CubicChunk.cubesPerAxis);
            return column[chunkPos.y / CubicChunk.cubesPerAxis];
        }
        else
        {
            Debug.Log("Something went wrong");
            return null;
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
    private Vector3Int Vector3FloorToNearestMultipleOf(Vector3Int vector, int multipleOf)
    {
        int x = vector.x - vector.x % multipleOf;
        int y = vector.y - vector.y % multipleOf;
        int z = vector.z - vector.z % multipleOf;
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
        int x = vector.x - vector.x % multipleOf;
        int y = vector.y - vector.y % multipleOf;
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