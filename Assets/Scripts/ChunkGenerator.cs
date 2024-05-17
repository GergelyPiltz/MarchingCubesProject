
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
    
    Chunk[] chunks;
    void Start()
    {
        chunks = new Chunk[chunksX * chunksZ];
        for (int x = 0; x < chunksX; x++)
            for (int z = 0; z < chunksZ; z++)
                chunks[x * chunksX + z] = new Chunk(
                    new Vector3Int(x * chunkSizeX, 0, z * chunkSizeZ), 
                    new Vector3Int(chunkSizeX, chunkSizeY, chunkSizeZ), 
                    transform, 
                    x * chunksX + z
                );
            

    }

    public void UpdateChunks(int seed, float startFrequency, float frequencyModifier, float startAmplitude, float amplitudeModifier, int octaves)
    {
        for (int x = 0; x < chunksX; x++)
            for (int z = 0; z < chunksZ; z++)
                chunks[x * chunksX + z].GenerateTerrain(seed, startFrequency, frequencyModifier, startAmplitude, amplitudeModifier, octaves);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(chunkSizeX / 2, chunkSizeY / 2, chunkSizeZ / 2), new Vector3(chunkSizeX, chunkSizeY, chunkSizeZ));
    }

}
