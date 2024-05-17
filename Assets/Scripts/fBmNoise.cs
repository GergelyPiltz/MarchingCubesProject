using UnityEngine;

public class fBmNoise : MonoBehaviour
{
    [SerializeField] int w = 256;
    [SerializeField] int h = 256;

    [SerializeField] int octaves = 1;
    [SerializeField] float starterFrequency = 1f;
    [SerializeField] float frequencyIncrease = 2f;
    [SerializeField] float starterAmplitude = 1f;
    [SerializeField] float amplitudeDecrease = 2f;
    [SerializeField] int offsetX_1 = 0;
    [SerializeField] int offsetY_1 = 0;

    Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        renderer.material.mainTexture = ApplyTexture();
    }

    private Texture ApplyTexture()
    {
        Texture2D texture = new(w, h);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float total = 0f;
                float frequency = starterFrequency;
                float amplitude = starterAmplitude;
                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = (float)x / w * frequency;
                    float yCoord = (float)y / h * frequency;

                    frequency *= frequencyIncrease;

                    //float sample1 = Mathf.PerlinNoise(xCoord, yCoord);
                    //sample1 = Mathf.Clamp01(sample1);
                    //sample1 -= 0.5f;
                    //sample1 *= 2;
                    //sample1 = Mathf.Abs(sample1);

                    //float sample2 = Mathf.PerlinNoise(xCoord, yCoord);
                    //sample2 = Mathf.Clamp01(sample2);
                    //sample2 -= 0.5f;
                    //sample2 *= 2;
                    //sample2 = Mathf.Abs(sample2);
                    //sample2 *= -1;
                    //sample2 += 1;

                    //sample2 *= amplitude;

                    //float sample3 = Mathf.PerlinNoise(xCoord, yCoord);
                    //sample3 = Mathf.Clamp01(sample3);

                    //total += ((sample1 + sample2) / 2) * amplitude;

                    float sample4 = Mathf.PerlinNoise(xCoord, yCoord);
                    sample4 = Mathf.Clamp01(sample4);

                    float sample5 = Mathf.PerlinNoise(xCoord, yCoord);
                    sample5 = Mathf.Clamp01(sample5);

                    total += amplitude * ((sample4 + sample5) / 2);

                    amplitude /= amplitudeDecrease;

                }

                texture.SetPixel(x, y, new Color(total, total, total));
            }
        }

        texture.Apply();
        return texture;
    }
}
