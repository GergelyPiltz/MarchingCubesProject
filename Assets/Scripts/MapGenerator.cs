
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    [Header("Display Only")]
    [SerializeField, Min(10)] int width;
    [SerializeField, Min(10)] int height;

    [Header("Generation Parameters")]
    [SerializeField] int seed;
    [SerializeField, Min(0.001f)] float startFrequency;
    [SerializeField, Min(1f)] float frequencyModifier;
    [SerializeField, Range(0.001f, 1)] float startAmplitude;
    [SerializeField, Range(0.001f, 1)] float amplitudeModifier;
    [SerializeField, Range(0, 10)] int octaves;

    GenerationParams generationParams;

    [SerializeField] DrawMode drawMode;
    public TerrainType[] terrainTypes;

    public enum DrawMode {NoiseMap, ColorMap}
    MapDisplay mapDisplay;
    [SerializeField] Transform mapDisplayTransform;
    bool isMapDisplay;
    public bool autoUpdate;

    ChunkGenerator chunkGenerator;

    private void OnValidate()
    {
        width = Mathf.Clamp(width, 10, 1000);
        height = Mathf.Clamp(height, 10, 1000);
        startFrequency = Mathf.Clamp(startFrequency, 0.001f, 1000);
        frequencyModifier = Mathf.Clamp(frequencyModifier, 1f, 1000);
        startAmplitude = Mathf.Clamp01(startAmplitude);
        amplitudeModifier = Mathf.Clamp01(amplitudeModifier);
        octaves = Mathf.Clamp(octaves, 1, 10);

        generationParams = new GenerationParams
        {
            seed = seed,
            startFrequency = startFrequency,
            frequencyModifier = frequencyModifier,
            startAmplitude = startAmplitude,
            amplitudeModifier = amplitudeModifier,
            octaves = octaves
        };
    }

    private void Start()
    {
        isMapDisplay = true;
        if (!TryGetComponent(out mapDisplay))
            if (!mapDisplayTransform.TryGetComponent(out mapDisplay))
            {
                Debug.Log("Map Diplay Missing");
                isMapDisplay = false;
            }

        if (!TryGetComponent(out chunkGenerator))
        {
            Debug.Log("Chunk Generator Missing");
            enabled = false; // disable the script
        }
            
        
    }

    public void UpdateMapDisplay()
    {
        if (!isMapDisplay) return;
        float[,] noiseMap = Noise.GenerateNoiseMap(width, height, 0, 0, generationParams);
        if (drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawNoiseMap(noiseMap);
        else
        {
            Color32[] colorMap = new Color32[width * height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    for(int i = 0; i < terrainTypes.Length; i++)
                        if (noiseMap[x, y] < terrainTypes[i].height)
                        {
                            colorMap[y * width + x] = terrainTypes[i].color;
                            break;
                        }
     
            mapDisplay.DrawColorMap(colorMap, width, height);
        }
        
    }

    public void GenerateMap()
    {
        chunkGenerator.UpdateChunks();
    }

    public GenerationParams GetGenerationParameters()
    {
        return generationParams;
    }

}

public struct GenerationParams
{
    public int seed;
    public float startFrequency;
    public float frequencyModifier;
    public float startAmplitude;
    public float amplitudeModifier;
    public int octaves;
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
    
}