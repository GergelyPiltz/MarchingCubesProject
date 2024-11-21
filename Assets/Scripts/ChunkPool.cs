using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkPool
{
    private readonly GameObject poolObj; // The object that holds the script
    private readonly Transform poolTransform; // The transform of the object that holds the script
    private List<CubicChunk> pool; // The pool itself
    private int inPool; // Counter for how many is in the pool

    // Constructor. Creates the object for the pool, sets the transform and initializes the list.
    public ChunkPool()
    {
        pool = new List<CubicChunk>();
        poolObj = new GameObject();
        poolTransform = poolObj.transform;
        inPool = 0;
    }

    // Returns an object from the pool and sets its parent to the parameter. If the pool is empty returns a new object.
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

    // Returns an object to the pool and sets its parent to the poolObject. 
    public void ReturnToPool(CubicChunk chunk)
    {
        inPool++;
        poolObj.name = "Pool (" + inPool + ")";

        chunk.SetActive(false);
        chunk.chunkTransform.parent = poolTransform;
        pool.Add(chunk);
    }
}
