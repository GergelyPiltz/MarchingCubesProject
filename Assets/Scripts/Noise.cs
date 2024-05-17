using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int width, int height, int offsetX, int offsetY, int seed, float startFrequency, float frequencyModifier, float startAmplitude, float amplitudeModifier, int octaves)
    {
        Random.InitState(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i].Set(Random.Range(-100_000, 100_000), Random.Range(-100_000, 100_000));    
        }

        float[,] noiseMap = new float[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float total = 0f;
                float frequency = startFrequency;
                float amplitude = startAmplitude;
                float totalAmplitudes = 0;
                for (int i = 0; i < octaves; i++)
                {

                    float xCoord = (x + offsetX) / 9999f * frequency + octaveOffsets[i].x;
                    float yCoord = (y + offsetY) / 9999f * frequency + octaveOffsets[i].y;

                    //float sample1 = Mathf.PerlinNoise(xCoord, yCoord);
                    //sample1 = Mathf.Clamp01(sample1);
                    //sample1 -= 0.5f;
                    //sample1 *= 2;
                    //sample1 = Mathf.Abs(sample1);

                    float sample2 = Mathf.PerlinNoise(xCoord, yCoord);
                    sample2 = Mathf.Clamp01(sample2);
                    sample2 *= 2;
                    sample2 -= 1;
                    sample2 = Mathf.Abs(sample2);
                    sample2 *= -1;
                    sample2 += 1;


                    float sample3 = Mathf.PerlinNoise(xCoord, yCoord);
                    sample3 = Mathf.Clamp01(sample3);

                    total += ((sample2 + sample3) / 2) * amplitude;

                    //total += ((sample1 + sample2) / 2) * amplitude;

                    //float sample4 = Mathf.PerlinNoise(xCoord, yCoord);
                    //sample4 = Mathf.Clamp01(sample4);
                    //sample4 = sample4 * 2 - 1;

                    //total += amplitude * sample4;
                    //totalAmplitudes += amplitude;

                    frequency *= frequencyModifier;
                    amplitude *= amplitudeModifier;

                }
                //total /= totalAmplitudes;
                //total /= 2;
                //total += 0.5f;

                //total = Mathf.Clamp01(total);
                noiseMap[x, y] = total;
            }

        return noiseMap;
    }
}
