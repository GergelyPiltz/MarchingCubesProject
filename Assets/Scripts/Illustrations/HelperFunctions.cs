using UnityEngine;

public static class HelperFunctions
{
    public static float[,,] GenerateSphereArray(int size, float radius)
    {
        float[,,] array = new float[size, size, size];

        // Calculate the center of the array
        float center = (size - 1) / 2.0f;

        // Loop through each point in the array
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    // Calculate the distance from the center
                    float distance = Mathf.Sqrt(
                        Mathf.Pow(x - center, 2) +
                        Mathf.Pow(y - center, 2) +
                        Mathf.Pow(z - center, 2)
                    );

                    array[x, y, z] = distance - radius;

                }
            }
        }

        return array;
    }

    public static float[,,] GenerateCurvedArray(int size, AnimationCurve animationCurveX, AnimationCurve animationCurveZ)
    {
        float[,,] array = new float[size, size, size];

        // Loop through each point in the array
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float heightModifier = animationCurveX.Evaluate(1 - ((float)x / size)) * animationCurveZ.Evaluate((float)z / size);

                for (int y = 0; y < size; y++)
                {
                    float currentHeight = size * heightModifier;

                    array[x, y, z] = y - currentHeight;

                }
            }
        }

        return array;
    }

}

/*
 void CreateTerrain()
    {

        float xCoord;
        float zCoord;
        float noiseValue;
        float currentHeight;
        terrainData = new float[size + 1, size + 1, size + 1];
        for (int x = 0; x < size + 1; x++)
            for (int z = 0; z < size + 1; z++)
            {
                xCoord = (float)x / size * scale;
                zCoord = (float)z / size * scale;

                noiseValue = Mathf.PerlinNoise(xCoord, zCoord);
                noiseValue = Mathf.Clamp(noiseValue, 0, 1);

                currentHeight = size * noiseValue;

                for (int y = 0; y < size + 1; y++)
                    terrainData[x, y, z] = (float)y - currentHeight;
            }
    }
 */