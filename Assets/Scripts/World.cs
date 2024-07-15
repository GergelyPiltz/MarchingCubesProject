using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private int radius;
    private int maxRenderRadius;

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
    private int active = 0;

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
        maxRenderRadius = (int) Mathf.Pow((radius + 2) * CubicChunk.cubesPerAxis, 2);

        for (int i = 0; i < 12; i++)
            StartCoroutine(CreateColumn(circle[i], verticalChunks, CubicChunk.cubesPerAxis, 0));

        int count = circle.Count;
        for (int i = 12; i < count; i++)
            StartCoroutine(CreateColumn(circle[i], verticalChunks, CubicChunk.cubesPerAxis, i * 0.1f));

        StartCoroutine(PlacePlayer());
    }

    private IEnumerator PlacePlayer()
    {
        yield return new WaitForSeconds(1);
        if (Physics.Raycast(new Vector3(0, worldHeight, 0), Vector3.down, out RaycastHit hit))
            playerTransform.position = hit.point + new Vector3(0, 2, 0);
    }

    Vector2Int playerChunkPos = new (0, 0);
    void Update()
    {
        if (!dynamicRender) return;

        Vector2Int newPos = Vector2FloorToNearestMultipleOf(ToVector2FromXZ(playerTransform.position), CubicChunk.cubesPerAxis);

        if (newPos == playerChunkPos)
            return;
        else
            playerChunkPos = newPos;

        float delay = 0;
        foreach (Vector2Int offset in ring)
        {
            delay += 0.01f;
            Vector2Int c = playerChunkPos + offset;
            if (!renderedColumns.ContainsKey(c))
                StartCoroutine(CreateColumn(c, verticalChunks, CubicChunk.cubesPerAxis, delay));
        }

        foreach (var c in renderedColumns)
        {
            if ((playerChunkPos - c.Key).sqrMagnitude > maxRenderRadius)
                oldColumns.Add(c.Key);
        }

        foreach (Vector2Int c in oldColumns)
        {
            if (renderedColumns.TryGetValue(c, out CubicChunk[] old))
            {
                renderedColumns.Remove(c);
                DisposeOfColumn(old);
            }
        }
        oldColumns.Clear();
    }

    private IEnumerator CreateColumn(Vector2Int pos, int height, int step, float delay)
    {
        yield return new WaitForSeconds(delay);
        CubicChunk[] column = new CubicChunk[height];
        for (int y = 0; y < height; y++)
        {
            column[y] = GetFromPool();
            column[y].MoveAndBuild(new Vector3Int(pos.x, y * step, pos.y));
        }
        renderedColumns.Add(pos, column);
        //return column;
        yield return null;
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

        active++;
        activeObj.name = "Active (" + active + ")";
        c.chunkTransform.parent = activeTransform;
        return c;
    }

    private void ReturnToPool(CubicChunk c)
    {
        inPool++;
        poolObj.name = "Pool (" + inPool + ")";
        if (active > 0)
        {
            active--;
            activeObj.name = "Active (" + active + ")";
        }

        c.SetActive(false);
        c.chunkTransform.parent = poolTransform;
        pool.Add(c);
    }

    private List<Vector2Int> CalculateRing(int radius, int multiplier)
    {
        List<Vector2Int> rim = new();
        for (int x = -radius + 1; x < radius; x++)
            for (int z = -radius + 1; z < radius; z++)
            {
                Vector2Int pos = new(x, z);
                float dist = Vector2Int.Distance(Vector2Int.zero, pos);
                if (dist <= radius && dist >= radius - 2)
                    rim.Add(pos * multiplier);
            }

        return rim;
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

    Vector2 ToVector2FromXZ(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    Vector2Int ToVector2FromXZ(Vector3Int v)
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