using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Contains the noisemap as a float[ , ] and its size. * operator overloaded to allow multiplication of layers with the same type. Can be cast to Texture2D. 
/// </summary>
public class NoiseMap2D
{
    public readonly int size;
    public readonly float[,] noise;

    public NoiseMap2D(float[,] noise)
    {
        if (noise.GetLength(0) != noise.GetLength(1))
            throw new ArgumentException("Dimensions of the noisemap must equal.");
        size = noise.GetLength(0);
        this.noise = noise;
    }

    #region operator overloading

    public static NoiseMap2D operator *(NoiseMap2D a, NoiseMap2D b)
    {
        if (a.size != b.size)
            throw new ArgumentException("Noisemaps not of the same size.");
        int size = a.size;

        float[,] noise = new float[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                noise[x, y] = a.noise[x, y] * b.noise[x, y];

        return new NoiseMap2D(noise);
    }

    public static NoiseMap2D operator *(NoiseMap2D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] *= b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator *(NoiseMap2D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] *= b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator +(NoiseMap2D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] += b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator /(NoiseMap2D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] /= b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator /(NoiseMap2D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] /= b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator +(NoiseMap2D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] += b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator -(NoiseMap2D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] -= b;

        return new NoiseMap2D(a.noise);
    }

    public static NoiseMap2D operator -(NoiseMap2D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                a.noise[x, y] -= b;

        return new NoiseMap2D(a.noise);
    }

    #endregion

    public static explicit operator Texture2D(NoiseMap2D a)
    {
        return a.ConvertToTexture2D();   
    }

    public Texture2D ConvertToTexture2D()
    {
        Texture2D noiseMapTexture = new(size, size)
        {
            filterMode = FilterMode.Point
        };

        Color32[] Colors = new Color32[size * size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                Colors[i * size + j] = Color.Lerp(Color.black, Color.white, noise[i, j]);

        noiseMapTexture.SetPixels32(Colors);
        noiseMapTexture.Apply();

        return noiseMapTexture;
    }

}

/// <summary>
/// Contains the noisemap as a float[ , , ] and its size. * operator overloaded to allow multiplication of layers with the same type.
/// </summary>
public class NoiseMap3D
{
    public readonly int size;
    public readonly float[,,] noise;

    public NoiseMap3D(float[,,] noise)
    {
        if (noise.GetLength(0) != noise.GetLength(1) || noise.GetLength(1) != noise.GetLength(2))
            throw new ArgumentException("Dimensions of the noisemap must equal.");
        size = noise.GetLength(0);
        this.noise = noise;
    }

    #region operator overloading

    public static NoiseMap3D operator *(NoiseMap3D a, NoiseMap3D b)
    {
        if (a.size != b.size)
            throw new ArgumentException("Noisemaps not of the same size.");
        int size = a.size;

        float[,,] noise = new float[size, size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    noise[x, y, z] = a.noise[x, y, z] * b.noise[x, y, z];

        return new NoiseMap3D(noise);
    }

    public static NoiseMap3D operator *(NoiseMap3D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] *= b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator *(NoiseMap3D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] *= b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator +(NoiseMap3D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] += b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator /(NoiseMap3D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] /= b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator /(NoiseMap3D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] /= b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator +(NoiseMap3D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] += b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator -(NoiseMap3D a, int b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] -= b;

        return new NoiseMap3D(a.noise);
    }

    public static NoiseMap3D operator -(NoiseMap3D a, float b)
    {
        int size = a.size;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    a.noise[x, y, z] -= b;

        return new NoiseMap3D(a.noise);
    }

    #endregion

}

public enum NoiseType { _2D,  _3D };

/// <summary>
/// Contains information used for procedural generation of a noiselayer.
/// </summary>
public struct LayerParameters
{
    /// <summary> Type of the noise. </summary>
    public string type;
    /// <summary> Describes the function of the layer. </summary>
    public string description;
    /// <summary> Initial input value used to initialize the pseudo-random number generator. </summary>
    public float startFrequency;
    /// <summary> Frequency multiplied by this after each octave. </summary>
    public float frequencyModifier;
    /// <summary> Amplitude multiplied by this after each octave. </summary>
    public float amplitudeModifier;
    /// <summary> Each octave is a layer of smaller details of the same noise. </summary>
    public int octaves;
    /// <summary> Modifies the noise using an AnimationCurve (spline). X component of a keyframe. times.Length must equal to values.Length. </summary>
    public float[] times;
    /// <summary> Modifies the noise using an AnimationCurve (spline). X component of a keyframe. times.Length must equal to values.Length. </summary>
    public float[] values;
    public string seed;

    public readonly bool ValidateStruct()
    {
        return true;
        if (startFrequency <= 0) return false;
        if (frequencyModifier < 1) return false;
        if (amplitudeModifier <= 0 || amplitudeModifier > 1) return false;
        if (octaves <= 0) return false;
        if ((times == null && values != null) || (times != null && values == null)) return false;
        if (times != null || values != null)
        {
            if (times.Length != values.Length) return false;
        }
        for (int i = 0; i < times.Length; i++)
        {
            if (times[i] < 0 || times[i] > 1) return false;
            if (i != 0 && times[i] > times[i + 1]) return false;
        }
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] < 0 || values[i] > 1) return false;
        }
        return true;
    }
}

public class NoiseLayer
{
    public LayerParameters LayerParams;
    protected AnimationCurve Modifier { get; }

    public NoiseLayer(LayerParameters layerParams)
    {
        if (!layerParams.ValidateStruct()) throw new ArgumentException("Invalid parameters!");
        LayerParams = layerParams;

        if (LayerParams.seed != null)
            Random.InitState(LayerParams.seed.GetHashCode());
        else
            Random.InitState(0);


        int keys;
        if (LayerParams.times != null && (keys = layerParams.times.Length) != 0)
        {
            Keyframe[] keyframes = new Keyframe[keys];
            for (int i = 0; i < keys; i++)
            {
                keyframes[i] = new Keyframe(layerParams.times[i], layerParams.values[i]);
            }
            Modifier = new AnimationCurve()
            {
                keys = keyframes
            };
        }
    }
}

public class NoiseLayer2D : NoiseLayer
{
    private readonly Vector2[] octaveOffsets;

    public NoiseLayer2D(LayerParameters layerParams) : base(layerParams)
    {
        octaveOffsets = new Vector2[LayerParams.octaves];
        for (int i = 0; i < LayerParams.octaves; i++)
            octaveOffsets[i] = 1_000f * new Vector2(Random.value, Random.value);
    }

    public NoiseMap2D GetNoiseMap(int offsetX, int offsetY, int size = CubicChunk.valuesPerAxis)
    {
        float[,] noiseMap = new float[size, size];

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float frequency = LayerParams.startFrequency;
                float amplitude = 1;

                
                float total = 0;
                float totalAmplitudes = 0;

                for (int i = 0; i < LayerParams.octaves; i++)
                {
                    //xCoord += octaveOffsets[i].x;
                    //yCoord += octaveOffsets[i].y;

                    float xCoord = (x + offsetX) /*/ 1f*/ * frequency;
                    float yCoord = (y + offsetY) /*/ 1f*/ * frequency;

                    total += Mathf.Clamp01(Mathf.PerlinNoise(xCoord, yCoord)) * amplitude;
                    totalAmplitudes += amplitude;

                    amplitude *= LayerParams.amplitudeModifier;
                    frequency *= LayerParams.frequencyModifier;
                }

                total /= totalAmplitudes;

                if (Modifier == null)
                    noiseMap[x, y] = total;
                else
                    noiseMap[x, y] = Modifier.Evaluate(total);
            }

        return new NoiseMap2D(noiseMap);
    }
}

public class NoiseLayer3D : NoiseLayer
{
    private readonly Vector3[] octaveOffsets;

    public NoiseLayer3D(LayerParameters layerParams) : base(layerParams)
    {
        
        octaveOffsets = new Vector3[LayerParams.octaves];
        for (int i = 0; i < LayerParams.octaves; i++)
            octaveOffsets[i] = 1_000f * new Vector3(Random.value, Random.value, Random.value);
    }

    public NoiseMap3D GetNoiseMap(Vector3Int offset, int size = CubicChunk.valuesPerAxis)
    {
        float[,,] noiseMap = new float[size, size, size];

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    float frequency = LayerParams.startFrequency;
                    float amplitude = 1;

                    float total = 0;
                    float totalAmplitudes = 0;

                    for (int i = 0; i < LayerParams.octaves; i++)
                    {
                        //xCoord += octaveOffsets[i].x;
                        //yCoord += octaveOffsets[i].y;
                        //zCoord += octaveOffsets[i].z;
                        float xCoord = (x + offset.x) /*/ 1_000f*/ * frequency;
                        float yCoord = (y + offset.y) /*/ 1_000f*/ * frequency;
                        float zCoord = (z + offset.z) /*/ 1_000f*/ * frequency;

                        // Get all permutations of noise for each individual axis
                        float noiseXY = Mathf.Clamp01(Mathf.PerlinNoise(xCoord, yCoord)) * amplitude;
                        float noiseXZ = Mathf.Clamp01(Mathf.PerlinNoise(xCoord, zCoord)) * amplitude;
                        float noiseYZ = Mathf.Clamp01(Mathf.PerlinNoise(yCoord, zCoord)) * amplitude;

                        // Reverse of the permutations of noise for each individual axis
                        float noiseYX = Mathf.Clamp01(Mathf.PerlinNoise(yCoord, xCoord)) * amplitude;
                        float noiseZX = Mathf.Clamp01(Mathf.PerlinNoise(zCoord, xCoord)) * amplitude;
                        float noiseZY = Mathf.Clamp01(Mathf.PerlinNoise(zCoord, yCoord)) * amplitude;

                        // Use the average of the noise functions
                        total += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;
                        totalAmplitudes += amplitude;

                        amplitude *= LayerParams.amplitudeModifier;
                        frequency *= LayerParams.frequencyModifier;
                    }

                    total /= totalAmplitudes;

                    if (Modifier == null)
                        noiseMap[x, y, z] = total;
                    else
                        noiseMap[x, y, z] = Modifier.Evaluate(total);
                }

        return new NoiseMap3D(noiseMap);
    }
}