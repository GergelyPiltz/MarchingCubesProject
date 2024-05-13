
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

    [Header("Generation Parameters")]
    [SerializeField] int octaves;
    [SerializeField] float starterFrequency;
    [SerializeField] float frequencyIncrease;
    [SerializeField] float starterAmplitude;
    [SerializeField] float amplitudeDecrease;

    [SerializeField] GameObject quad;
    Renderer renderer;
    
    SerializeField playerPosition;

    Chunk[] chunks;
    void Start()
    {
        renderer = quad.GetComponent<Renderer>();

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

    private void Update()
    {
        renderer.material.mainTexture = ApplyTexture();

        if (Input.GetKeyDown(KeyCode.R))
            for (int x = 0; x < chunksX; x++)
                for (int z = 0; z < chunksZ; z++)
                    chunks[x * chunksX + z].GenerateTerrain(octaves, starterFrequency, frequencyIncrease, starterAmplitude, amplitudeDecrease);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(chunkSizeX / 2, chunkSizeY / 2, chunkSizeZ / 2), new Vector3(chunkSizeX, chunkSizeY, chunkSizeZ));
    }

    private Texture ApplyTexture()
    {
        Texture2D texture = new(chunkSizeX * chunksX, chunkSizeZ * chunksZ);
        for (int x = 0; x < chunkSizeX * chunksX; x++)
            for (int z = 0; z < chunkSizeZ * chunksZ; z++)
            {
                float total = 0f;
                float frequency = starterFrequency;
                float amplitude = starterAmplitude;
                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = (float)x / 1000f * frequency;
                    float zCoord = (float)z / 1000f * frequency;

                    //float sample1 = Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord)); //height
                    //float sample2 = Mathf.Abs((Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord)) - 0.5f) * 2); //valleys
                    //float noise = amplitude * ((sample1 + sample2) / 2);

                    //float noise = amplitude * Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord));

                    float sample3 = Mathf.Abs((Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord)) - 0.5f) * 2) * -1 + 1; //ridges

                    total += sample3;

                    frequency *= frequencyIncrease;
                    amplitude /= amplitudeDecrease;
                }
                
                texture.SetPixel(x, z, new Color(total, total, total));
            }

        texture.Apply();
        return texture;
    }
}
