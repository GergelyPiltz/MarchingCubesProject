using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CubeCircle : MonoBehaviour
{
    [SerializeField] private int spawnDistance;
    private int despawnDistance;
    private int innerDiameter;
    private int outerDiameter;

    List<Vector2Int> innerRing;
    List<Vector2Int> outerRing;
    Vector2Int[,,] renderedChunks;
    TextMeshPro[,,] display;
    GameObject poolObj;
    GameObject worldObj;
    GameObject displayObj;
    List<GameObject> pool = new();
    int inPool = 0;
    int inWorld = 0;

    Vector2Int nullValue = new Vector2Int(-9999, -9999);
    Vector2Int center;

    void Start()
    {
        despawnDistance = spawnDistance + 6;
        innerDiameter = spawnDistance * 2 + 1;
        outerDiameter = despawnDistance * 2 + 1;
        center = new(despawnDistance, despawnDistance);

        Debug.Log("SpawnDist: " + spawnDistance);
        Debug.Log("DespawnDist: " + despawnDistance);

        renderedChunks = new Vector2Int[outerDiameter, World.verticalChunks, outerDiameter];
        display = new TextMeshPro[outerDiameter, World.verticalChunks, outerDiameter];
        Debug.Log("total: " + renderedChunks.Length);
        Debug.Log("x: " + renderedChunks.GetLength(0) + " y: " + renderedChunks.GetLength(1) + " z: " + renderedChunks.GetLength(2));

        poolObj = new("Chunk Pool (0)");
        worldObj = new("World (0)");
        displayObj = new("Display");

        innerRing = CalculateRing(spawnDistance, center, outerDiameter);
        Debug.Log(innerRing.Count);
        outerRing = CalculateRing(despawnDistance, center, outerDiameter);
        Debug.Log(outerRing.Count);

        for (int x = 0; x < outerDiameter; x++)
            for (int z = 0; z < outerDiameter; z++)
            {
                GameObject g = new ("D");
                g.transform.parent = displayObj.transform;
                g.transform.position = new Vector3(x, 1, z);
                g.transform.rotation = Quaternion.Euler(90, 0, 0);
                TextMeshPro t = g.AddComponent<TextMeshPro>();
                display[x, 0, z] = t;
                t.text = new Vector2Int(x, z).ToString();
                t.alignment = TextAlignmentOptions.Center;
                t.fontSize = 3;

                //if (x == dimensionX / 2 && z == dimensionZ / 2 && x == center.x && z == center.y)
                //{
                //    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                //    g.transform.localScale = new(0.5f, 10, 0.5f);
                //    g.transform.position = new Vector3(x, 10, z);
                //}
                //else
                //{
                //    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //    g.transform.localScale = new(0.25f, 0.25f, 0.25f);
                //    g.transform.position = new Vector3(x, 0, z);
                //}
            }

        for (int x = 0; x < outerDiameter; x++)
            for (int z = 0; z < outerDiameter; z++)
                renderedChunks[x, 0, z] = nullValue;


        //foreach (var c in innerRing)        
        //    if (renderedChunks[c.x, 0, c.y] == nullValue)
        //        renderedChunks[c.x, 0, c.y] = new Vector2Int(c.x, c.y);//GetFromPool(new(c.x, 0, c.y));

        //foreach (var c in outerRing)
        //{
        //    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //    g.transform.position = new(c.x, 0, c.y);
        //    g.transform.localScale = new(0.5f, 0.5f, 0.5f);
        //}
    }

    Vector2Int oldPos = Vector2Int.zero;
    void Update()
    {
        Vector2Int newPos = Vector2Int.FloorToInt(new (transform.position.x, transform.position.z));
        if (oldPos == newPos) return;

        if (newPos.x > oldPos.x)
            for (int x = 0; x < outerDiameter - 1; x++)
                for (int z = 0; z < outerDiameter; z++)
                {
                    renderedChunks[x, 0, z] = renderedChunks[x + 1, 0, z];
                }
        else if (newPos.x < oldPos.x)
            for (int x = outerDiameter - 2; x >= 0; x--)
                for (int z = 0; z < outerDiameter; z++)
                {
                    renderedChunks[x + 1, 0, z] = renderedChunks[x, 0, z];
                }

        if (newPos.y > oldPos.y)
            for (int x = 0; x < outerDiameter; x++)
                for (int z = 0; z < outerDiameter - 1; z++)
                {
                    renderedChunks[x, 0, z] = renderedChunks[x, 0, z + 1];
                }
        else if (newPos.y < oldPos.y)
            for (int x = 0; x < outerDiameter; x++)
                for (int z = outerDiameter - 2; z >= 0; z--)
                {
                    renderedChunks[x, 0, z + 1] = renderedChunks[x, 0, z];
                }

        oldPos = newPos;

        foreach (Vector2Int v in innerRing)
            if (renderedChunks[v.x, 0, v.y] == nullValue)
                renderedChunks[v.x, 0, v.y] = newPos - center + v;

        foreach (Vector2Int v in outerRing)
            if (renderedChunks[v.x, 0, v.y] != nullValue)
                renderedChunks[v.x, 0, v.y] = nullValue;;

        for (int x = 0; x < outerDiameter; x++)
            for (int z = 0; z < outerDiameter; z++)
                if (renderedChunks[x, 0, z] == nullValue)
                    display[x, 0, z].text = "NULL";
                else
                    display[x, 0, z].text = renderedChunks[x, 0, z].ToString();





    }






    private GameObject GetFromPool(Vector3Int newPosition)
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
        g.transform.position = newPosition;
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
