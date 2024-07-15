using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CubeCircle : MonoBehaviour
{
    [SerializeField] private int radius;
    private int sqrRadius;

    List<Vector2Int> rim;
    List<Vector2Int> circle;
    List<Vector2Int> oldColumns = new();
    Dictionary<Vector2Int, GameObject[]> renderedColumns = new();
    GameObject poolObj;
    GameObject worldObj;
    List<GameObject> pool = new();
    int inPool = 0;
    int inWorld = 0;

    void Start()
    {
        poolObj = new()
        {
            name = "Chunk Pool (0)" 
        };

        worldObj = new()
        {
            name = "World (0)"
        };

        rim = CalculateRim(radius);
        circle = CalculateCircle(radius);
        sqrRadius = (int)(radius * radius * 1.5);

        foreach (var c in circle)
            renderedColumns.Add(c, CreateColumn(c));
    }

    void Update()
    {
        //Debug.Log("Total: " + (inPool + inWorld));

        Vector3 pos = transform.position;
        
        foreach (Vector2Int offset in rim)
        {
            Vector2Int c = Vector2Int.FloorToInt(new Vector2(pos.x, pos.z)) + offset;
            if (!renderedColumns.ContainsKey(c))
                renderedColumns.Add(c, CreateColumn(c));
        }

        foreach (var c in renderedColumns)
        {
            Vector2Int pos2d = Vector2Int.FloorToInt(new Vector2(pos.x, pos.z));

            if ((pos2d - c.Key).sqrMagnitude > sqrRadius)
                oldColumns.Add(c.Key);
        }

        foreach (Vector2Int c in oldColumns)
        {
            if (renderedColumns.TryGetValue(c, out GameObject[] old))
            {
                renderedColumns.Remove(c);
                DisposeOfColumn(old);
            }
        }
        oldColumns.Clear();
    }

    private GameObject[] CreateColumn(Vector2Int pos)
    {
        GameObject[] column = new GameObject[World.verticalChunks];
        for (int y = 0; y < World.verticalChunks; y++)
        {
            column[y] = GetFromPool();
            column[y].transform.position = new Vector3(pos.x, y, pos.y);
        }

        return column;
    } 

    private void DisposeOfColumn(GameObject[] column)
    {
        foreach (var c in column)
            ReturnToPool(c);
    }

    private void InitPool(int count)
    {
        inPool = count;
        poolObj.name = "Chunk Pool (" + inPool + ")";
        for (int i = 0; i < count; i++)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.SetActive(false);
            g.transform.parent = poolObj.transform;
            pool.Add(g);
        }
    }

    private GameObject GetFromPool()
    {
        GameObject g;
        if (inPool > 0)
        {
            inPool--;
            poolObj.name = "Chunk Pool (" + inPool + ")";

            g = pool[0];
            pool.RemoveAt(0);
            g.SetActive(true);
        }
        else
            g = GameObject.CreatePrimitive(PrimitiveType.Cube);

        inWorld++;
        worldObj.name = "World (" + inWorld + ")";
        g.transform.parent = worldObj.transform;
        return g;
    }

    private void ReturnToPool(GameObject g)
    {
        inPool++;
        poolObj.name = "Chunk Pool (" + inPool + ")";
        if (inWorld > 0)
        {
            inWorld--;
            worldObj.name = "World (" + inWorld + ")";
        }

        g.SetActive(false);
        g.transform.parent = poolObj.transform;
        pool.Add(g);
    }

    private List<Vector2Int> CalculateRim(int r)
    {
        List<Vector2Int> rim = new();
        for (int x = -r + 1; x < r; x++)
            for (int z = -r + 1; z < r; z++)
            {
                Vector2Int pos = new(x, z);
                float dist = Vector2Int.Distance(Vector2Int.zero, pos);
                if (dist <= r && dist >= r - 2)
                    rim.Add(pos);
            }

        return rim;
    }

    private List<Vector2Int> CalculateCircle(int r)
    {
        Dictionary<Vector2Int, float> coords = new();
        for (int x = -r + 1; x < r; x++)
            for (int z = -r + 1; z < r; z++)
            {
                Vector2Int pos = new(x, z);
                float dist = Vector2Int.Distance(Vector2Int.zero, pos);
                if (dist <= r)
                    coords.Add(pos, dist);
            }

        var ordered = coords.OrderBy(x => x.Value);
        List<Vector2Int> circle = new();
        foreach (var item in ordered)
            circle.Add(item.Key); 

        return circle;
    }
}
