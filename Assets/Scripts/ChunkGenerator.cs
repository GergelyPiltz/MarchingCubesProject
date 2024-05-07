using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkGenerator : MonoBehaviour
{
    //int chunkIndex;
    [SerializeField] int chunksX;
    [SerializeField] int chunksZ;
    [SerializeField] int chunkSizeX;
    [SerializeField] int chunkSizeY;
    [SerializeField] int chunkSizeZ;


    Chunk[] chunk;
    void Start()
    {
        chunk = new Chunk[chunksX * chunksZ];
        for (int x = 0; x < chunksX; x++)
        {
            for (int z = 0; z < chunksZ; z++)
            {
                chunk[x * chunksX + z] = new Chunk(new Vector3Int(x * chunkSizeX, 0, z * chunkSizeZ), new Vector3Int(chunkSizeX, chunkSizeY, chunkSizeZ), transform, x * chunksX + z);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(chunkSizeX / 2, chunkSizeY / 2, chunkSizeZ / 2), new Vector3(chunkSizeX, chunkSizeY, chunkSizeZ));
    }

}
