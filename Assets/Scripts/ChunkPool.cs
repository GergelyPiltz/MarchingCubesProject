using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkPool
{
    private readonly GameObject poolObj;
    private readonly Transform poolTransform;
    private List<CubicChunk> pool;
    private int inPool;

    public ChunkPool()
    {
        pool = new List<CubicChunk>();
        poolObj = new GameObject();
        poolTransform = poolObj.transform;
        inPool = 0;
    }

    public CubicChunk GetFromPool(Transform parent)
    {
        CubicChunk chunk;
        if (inPool > 0)
        {
            inPool--;
            poolObj.name = "Pool (" + inPool + ")";

            chunk = pool[0];
            pool.RemoveAt(0);
            chunk.SetActive(true);
        }
        else
            chunk = new CubicChunk();

        chunk.chunkTransform.parent = parent;
        return chunk;
    }

    public void ReturnToPool(CubicChunk chunk)
    {
        inPool++;
        poolObj.name = "Pool (" + inPool + ")";

        chunk.SetActive(false);
        chunk.chunkTransform.parent = poolTransform;
        pool.Add(chunk);
    }
}
