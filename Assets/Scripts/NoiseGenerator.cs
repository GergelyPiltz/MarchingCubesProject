using UnityEngine;

public struct GenerationParams
{
    public string description;
    public int Seed;
    public float startFrequency;
    /// <summary>
    /// Increase the frequency by this factor on every octave. Based on multiplication, use numbers greater than 1. Large numbers recommended.
    /// </summary>
    public float frequencyModifier;
    /// <summary>
    /// Decrease the amplitude by this factor on every octave. Based on multiplication, use numbers less than 1. Small number numbers recommended.
    /// </summary>
    public float amplitudeModifier;
    /// <summary>
    /// How many layers of details are applied. Minimum is 1.
    /// </summary>
    public int octaves;
}
public class NoiseGenerator : MonoBehaviour
{
    #region Singleton
    public static NoiseGenerator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Multiple instances of NoiseGenerator exist. Destroying instance.");
            if (gameObject != null)
                Debug.Log("Extra script was attached to " + gameObject.name);
            else
                Debug.Log("Extra script wasn't attached to a gameobject");
            Destroy(this);
        }
        //DontDestroyOnLoad(this);
    }
    #endregion

    NoiseModifier noiseModifier;

    void Start()
    {
        noiseModifier = NoiseModifier.Instance;
    }

    [SerializeField, Range(1,1000)] float controlSampleFrequency = 200;
    public float[,] GenerateNoiseMap(int width, int height, int offsetX, int offsetY, GenerationParams generationParams)
    {
        Random.InitState(generationParams.Seed);
        Vector2[] octaveOffsets = new Vector2[generationParams.octaves];
        for (int i = 0; i < generationParams.octaves; i++)
            octaveOffsets[i].Set(Random.Range(-100_000, 100_000), Random.Range(-100_000, 100_000));
        
        float[,] noiseMap = new float[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {


                float xCoord = (float)(x + offsetX) / /*1_000_000_000*/ 9999f * controlSampleFrequency;
                float yCoord = (float)(y + offsetY) / /*1_000_000_000*/ 9999f * controlSampleFrequency;
                float controlSample = Mathf.PerlinNoise(xCoord, yCoord);

                float total = 0f;
                float frequency = generationParams.startFrequency;
                float amplitude = 1;
                for (int i = 0; i < generationParams.octaves; i++)
                {

                    xCoord = (float)(x + offsetX) / /*1_000_000_000*/ 9999f * frequency + octaveOffsets[i].x ;
                    yCoord = (float)(y + offsetY) / /*1_000_000_000*/ 9999f * frequency + octaveOffsets[i].y ;

                    

                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    sample = Mathf.Clamp01(sample);
                    
                    sample =controlSample + sample * noiseModifier.Evaluate(controlSample);

                    total += sample * amplitude;

                    frequency *= generationParams.frequencyModifier;
                    amplitude *= generationParams.amplitudeModifier;
                }
                noiseMap[x, y] = total;
            }

        return noiseMap;
    }
}
