using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{

    [Header("Display Only")]
    [SerializeField, Min(10)] int width;
    [SerializeField, Min(10)] int height;

    [Header("Generation Parameters")]
    [SerializeField] int seed = 101010;
    public void Seed(string seedString)
    {
        if (int.TryParse(seedString, out int seedInt))
        {
            seed = seedInt;
            CheckAndUpdateValues();

        }
    }
    [SerializeField, Min(0.001f)] float startFrequency = 200;
    public float StartFrequency
    {
        get { return startFrequency; }
        set { startFrequency = value; CheckAndUpdateValues(); }
    }
    [SerializeField, Min(1f)] float frequencyModifier = 2;
    public float FrequencyModifier
    {
        get { return frequencyModifier; }
        set { frequencyModifier = value; CheckAndUpdateValues(); }
    }
    [SerializeField, Range(0.001f, 1)] float startAmplitude = 0.5f;
    public float StartAmplitude
    {
        get { return startAmplitude; }
        set { startAmplitude = value; CheckAndUpdateValues(); }
    }
    [SerializeField, Range(0.001f, 1)] float amplitudeModifier = 0.5f;
    public float AmlitudeModifier
    {
        get { return amplitudeModifier; }
        set { amplitudeModifier = value; CheckAndUpdateValues(); }
    }
    [SerializeField, Range(0, 10)] int octaves = 5;
    public void Octaves(float f)
    {
        octaves = (int)f;
        CheckAndUpdateValues();
    }

    GenerationParams generationParams;

    [SerializeField] DrawMode drawMode;
    public TerrainType[] terrainTypes;

    public enum DrawMode {NoiseMap, ColorMap}

    public void SetDrawMode(int mode)
    {
        if (mode == 0)
            drawMode = DrawMode.NoiseMap;
        else
            drawMode = DrawMode.ColorMap;
        CheckAndUpdateValues();
    }

    [SerializeField] RawImage mapImage;
    bool isMapDisplay;
    public bool autoUpdate;

    ChunkGenerator chunkGenerator;

    private void CheckAndUpdateValues()
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

        if (Application.isPlaying && autoUpdate)
        {
            UpdateMapDisplay();
        }
    }

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

        if (Application.isPlaying && autoUpdate)
        {
            UpdateMapDisplay();
        }
    }

    private void Start()
    {
        isMapDisplay = true;
        if (!mapImage)
        {
            Debug.Log("Map Diplay Missing");
            isMapDisplay = false;
        }

        if (!TryGetComponent(out chunkGenerator))
        {
            Debug.Log("Chunk Generator Missing");
            enabled = false; // disable the script
        }

        CheckAndUpdateValues();
        UpdateMapDisplay();
        GenerateMap();

    }

    public void UpdateMapDisplay()
    {
        if (!isMapDisplay) return;
        float[,] noiseMap = Noise.GenerateNoiseMap(width, height, 0, 0, generationParams);
        if (drawMode == DrawMode.NoiseMap)
            DrawNoiseMap(noiseMap);
        else
        {
            for (int i = 0; i < terrainTypes.Length; i++)
                terrainTypes[i].color.a = 1;
                
            Color32[] colorMap = new Color32[width * height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    for(int i = 0; i < terrainTypes.Length; i++)
                        if (noiseMap[x, y] < terrainTypes[i].height)
                        {
                            colorMap[y * width + x] = terrainTypes[i].color;
                            break;
                        }
     
            DrawColorMap(colorMap, width, height);
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

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        Color32[] colorMap = new Color32[width * height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                colorMap[x + width * y] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);

        texture.SetPixels32(colorMap);
        texture.Apply();

        mapImage.texture = texture;
    }

    public void DrawColorMap(Color32[] colorMap, int width, int height)
    {
        Texture2D texture = new(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,

        };

        texture.SetPixels32(colorMap);
        texture.Apply();

        mapImage.texture = texture;
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