
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Chunk Parameters")]
    [SerializeField] int chunksX;
    [SerializeField] int chunksZ;
    [SerializeField] int chunkSizeX;
    [SerializeField] int chunkSizeY;
    [SerializeField] int chunkSizeZ;

    [SerializeField] Transform player;
    [SerializeField, Range(2, 16)] int renderDistance; // not really, cause out of range chunks dont disappear (yet)
    
    List<Chunk> chunks;
    List<Vector2> chunkList;

    int chunkIndex;

    MapGenerator mapGenerator;

    void Start()
    {
        if(!TryGetComponent(out mapGenerator)) enabled = false; // disable the script

        chunks = new List<Chunk> ();
        chunkList = new List<Vector2> ();

        chunkIndex = 0;

        //for (int x = 0; x < chunksX; x++)
        //    for (int z = 0; z < chunksZ; z++)
        //        chunks[x * chunksX + z] = new Chunk(
        //            new Vector3Int(x * chunkSizeX, 0, z * chunkSizeZ), 
        //            new Vector3Int(chunkSizeX, chunkSizeY, chunkSizeZ), 
        //            transform, 
        //            x * chunksX + z
        //        );
            

    }

    private void Update()
    {
        int playerInWhickChunkX = Mathf.FloorToInt(player.position.x / chunkSizeX);
        int playerInWhickChunkZ = Mathf.FloorToInt(player.position.z / chunkSizeZ);

        Vector2Int playerIsInWhichChunk = new(playerInWhickChunkX, playerInWhickChunkZ);

        if (renderDistance % 2 != 0)
        {
            renderDistance--;
        }

        for (int i = 0; i <= renderDistance; i++)
        {
            for (int j = 0; j <= renderDistance; j++)
            {
                Vector2Int positionRelativeToPlayer = new(i - renderDistance / 2, j - renderDistance / 2);
                Vector2Int positionInWorld = playerIsInWhichChunk - positionRelativeToPlayer;
                if (!chunkList.Contains(positionInWorld))
                {
                    chunkList.Add(positionInWorld); // for the love of god dont forget this

                    Chunk temp = new (
                        new Vector3Int(positionInWorld.x * chunksX, 0, positionInWorld.y * chunksZ /*its technically .z*/ ),
                        new Vector3Int(chunkSizeX, chunkSizeY, chunkSizeZ),
                        transform,
                        chunkIndex
                        );

                    chunks.Add(temp);
                    temp.GenerateTerrain(mapGenerator.GetGenerationParameters());
                        
                    chunkIndex++;
                }
            }
        }
    }

    private void OnValidate()
    {
        renderDistance = Mathf.Clamp(renderDistance, 2, 16);
    }

    public void UpdateChunks()
    {
        foreach (var chunk in chunks)
        {
            chunk.GenerateTerrain(mapGenerator.GetGenerationParameters());
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(chunkSizeX / 2, chunkSizeY / 2, chunkSizeZ / 2), new Vector3(chunkSizeX, chunkSizeY, chunkSizeZ));
    }

}
