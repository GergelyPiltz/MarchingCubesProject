using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    
    public const int verticalChunks = 16;
    public const int worldHeight = CubicChunk.cubesPerAxis * verticalChunks;

    [SerializeField] RawImage oneChunkNoise;
    [SerializeField] RawImage mapNoise;
    [SerializeField] GameObject chunkCounterObj;
    TextMeshProUGUI chunkCounterTMP;

    [SerializeField] Transform generateIndicator;
    [SerializeField] Transform removeIndicator;

    [SerializeField] bool use2DNoise;
    [SerializeField] bool use3DNoise;

    [Header("2D")]
    [SerializeField] float startFrequency2D;
    [SerializeField] float frequencyModifier2D;
    [SerializeField] float amplitudeModifier2D;
    [SerializeField] int octaves2D;

    [Header("3D")]
    [SerializeField] float startFrequency3D;
    [SerializeField] float frequencyModifier3D;
    [SerializeField] float amplitudeModifier3D;
    [SerializeField] int octaves3D;

    LayerParameters layerParameters2D;
    LayerParameters layerParameters3D;

    NoiseLayer2D noise2Dgenerator;
    NoiseLayer3D noise3Dgenerator;

    [Header("Terrain Profile")]
    [SerializeField] CubicChunk.TerrainProfile terrainProfile;
    private void OnValidate()
    {
        CubicChunk.SetTerrainProfile(terrainProfile);

        layerParameters2D = new LayerParameters()
        {
            startFrequency = startFrequency2D,
            frequencyModifier = frequencyModifier2D,
            amplitudeModifier = amplitudeModifier2D,
            octaves = octaves2D,
        };

        layerParameters3D = new LayerParameters()
        {
            startFrequency = startFrequency3D,
            frequencyModifier = frequencyModifier3D,
            amplitudeModifier = amplitudeModifier3D,
            octaves = octaves3D,
        };
        if (noise2Dgenerator != null || noise3Dgenerator != null)
        {
            noise2Dgenerator.LayerParams = layerParameters2D;
            noise3Dgenerator.LayerParams = layerParameters3D;
        }
    }

    void Start()
    {
        if (generateIndicator)
            generateIndicator.localScale = new Vector3 (CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis);
        if (removeIndicator)
            removeIndicator.localScale = new Vector3(CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis);

        if (chunkCounterObj)
            chunkCounterObj.TryGetComponent(out chunkCounterTMP);

        player = GameObject.Find("Player");
        if (player == null)
            dynamicRender = false;
        else
            playerTransform = player.transform;

        currentlyRenderedChunks = new();

        #region OnValidate in build i should stop using onvalidate

        layerParameters2D = new LayerParameters()
        {
            startFrequency = startFrequency2D,
            frequencyModifier = frequencyModifier2D,
            amplitudeModifier = amplitudeModifier2D,
            octaves = octaves2D,
        };

        layerParameters3D = new LayerParameters()
        {
            startFrequency = startFrequency3D,
            frequencyModifier = frequencyModifier3D,
            amplitudeModifier = amplitudeModifier3D,
            octaves = octaves3D,
        };

        #endregion

        noise2Dgenerator = new(layerParameters2D);
        noise3Dgenerator = new(layerParameters3D);

        if (dynamicRender)
        {
            for (int y = 0; y < verticalChunks; y++)
                for (int x = -2; x < 2; x++)
                    for (int z = -2; z < 2; z++)
                    {
                        Vector3Int chunkPosition = new Vector3Int(x, y, z) * CubicChunk.cubesPerAxis;

                        CubicChunk chunk = new(chunkPosition, transform);

                        currentlyRenderedChunks.Add(chunkPosition, chunk);

                        NoiseMap3D noise3d = noise3Dgenerator.GetNoiseMap(chunkPosition);
                        NoiseMap2D noise2d = noise2Dgenerator.GetNoiseMap(chunkPosition.x, chunkPosition.z);

                        if (use2DNoise && !use3DNoise) // this mess 
                        {
                            chunk.Build(noise2d);
                        }
                        else if (!use2DNoise && use3DNoise)
                        {
                            chunk.Build(noise3d);
                        }
                        else if (use2DNoise && use3DNoise)
                        {
                            chunk.Build(noise2d, noise3d);
                        }

                    }
            if (Physics.Raycast(new (0, worldHeight, 0), Vector3.down, out RaycastHit hit))
                playerTransform.position = hit.point + new Vector3(0, 2, 0);
        }
        else
        {
            for (int x = -5; x < 5; x++)
                for (int z = -5; z < 5; z++)
                {
                    int horizontalOffsetX = x * CubicChunk.cubesPerAxis;
                    int horizontalOffsetZ = z * CubicChunk.cubesPerAxis;
                    NoiseMap2D noise2d = noise2Dgenerator.GetNoiseMap(horizontalOffsetX, horizontalOffsetZ);

                    for (int y = 0; y < verticalChunks; y++)
                    {
                        Vector3Int chunkPosition = new(horizontalOffsetX, y * CubicChunk.cubesPerAxis, horizontalOffsetZ);

                        CubicChunk chunk = new(chunkPosition, transform);
                        currentlyRenderedChunks.Add(chunkPosition, chunk);
                        NoiseMap3D noise3d = noise3Dgenerator.GetNoiseMap(chunkPosition);

                        if (use2DNoise && !use3DNoise) // this mess
                        {
                            chunk.Build(noise2d);
                        }
                        else if (!use2DNoise && use3DNoise)
                        {
                            chunk.Build(noise3d);
                        }
                        else if (use2DNoise && use3DNoise)
                        {
                            chunk.Build(noise2d, noise3d);
                        }
                    }
                }
        }
    }

    [Header("Rendering Options")]
    private GameObject player;
    private Transform playerTransform;
    [SerializeField] private bool dynamicRender = true;
    [SerializeField] private int renderdistance = 5;
    Dictionary<Vector3Int, CubicChunk> currentlyRenderedChunks;

    void Update()
    {
        if (chunkCounterTMP)
            chunkCounterTMP.SetText("Number of Chunks: " + currentlyRenderedChunks.Count);

        if (player == null || !dynamicRender) return;

        //double time = Time.realtimeSinceStartupAsDouble;

        foreach (var chunk in currentlyRenderedChunks)
        {
            if (Vector3.Distance(chunk.Key, playerTransform.position) > (renderdistance * 2 + 1) * CubicChunk.cubesPerAxis)
            {
                removeIndicator.position = chunk.Key;
                // do some logic of leaving behind chunks. disable? destroy? save?
                Destroy(chunk.Value.ChunkObject);
                currentlyRenderedChunks.Remove(chunk.Key);
                break; // only one chunk removed per frame. no big deal.
            }
            else
            {
                //chunk.Value.Update();
            }
        }


        Vector3Int playerPos = Vector3Int.FloorToInt(playerTransform.position);
        Vector3Int playerChunk = Vector3IntFloorToNearestMultipleOf(playerPos, CubicChunk.cubesPerAxis);
        
        bool breakAll = false;
        for (int y = -renderdistance; y < renderdistance; y++)
        {
            for (int x = -renderdistance; x < renderdistance; x++)
            {
                for (int z = -renderdistance; z < renderdistance; z++)
                {
                    Vector3Int currentLoopPosition = new Vector3Int(x, y, z) * CubicChunk.cubesPerAxis;
                    Vector3Int chunkPosition = playerChunk + currentLoopPosition;

                    if (chunkPosition.y >= worldHeight || chunkPosition.y < 0) continue;

                    if (!currentlyRenderedChunks.ContainsKey(chunkPosition))
                    {
                        if (generateIndicator)
                            generateIndicator.position = chunkPosition;

                        CubicChunk chunk = new(chunkPosition, transform);

                        currentlyRenderedChunks.Add(chunkPosition, chunk);

                        NoiseMap3D noise3d = noise3Dgenerator.GetNoiseMap(chunkPosition);
                        NoiseMap2D noise2d = noise2Dgenerator.GetNoiseMap(chunkPosition.x, chunkPosition.z);

                        if (use2DNoise && !use3DNoise) // this mess 
                        {
                            chunk.Build(noise2d);
                        }
                        else if (!use2DNoise && use3DNoise)
                        {
                            chunk.Build(noise3d);
                        }
                        else if (use2DNoise && use3DNoise)
                        {
                            chunk.Build(noise2d, noise3d);
                        }

                        //time = Time.realtimeSinceStartupAsDouble - time;
                        //if (time >= (double)1 / 60) breakAll = true;
                        breakAll = true; // only one chunk is generated per frame
                    }

                    

                    if (breakAll) break;
                }
                if (breakAll) break;
            }
            if (breakAll) break;
        }
                
    }

    /// <summary>
    /// Floors each component of a Vector3Int to the nearest multiple of the specified value.
    /// Example: (-4,15,17) with 5 results in (-5,15,15)
    /// </summary>
    /// <param name="vector"> Vector3Int to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    Vector3Int Vector3IntFloorToNearestMultipleOf(Vector3Int vector, int multipleOf)
    {
        int x = vector.x - vector.x % multipleOf;
        int y = vector.y - vector.y % multipleOf;
        int z = vector.z - vector.z % multipleOf;
        return new Vector3Int(x, y, z);
    }

    public void BuildChunks()
    {
        noise3Dgenerator = new(layerParameters3D);
        noise2Dgenerator = new(layerParameters2D);
        foreach (var chunk in currentlyRenderedChunks)
        {
            Vector3Int position = chunk.Key;
            if (use2DNoise && !use3DNoise) // search "this mess" to find instances of this mess later
            {
                NoiseMap2D noise2d = noise2Dgenerator.GetNoiseMap(position.x, position.z);
                chunk.Value.Build(noise2d);
            }
            else if (!use2DNoise && use3DNoise)
            {
                NoiseMap3D noise3d = noise3Dgenerator.GetNoiseMap(position);
                chunk.Value.Build(noise3d);
            }
            else if (use2DNoise && use3DNoise)
            {
                NoiseMap2D noise2d = noise2Dgenerator.GetNoiseMap(position.x, position.z);
                NoiseMap3D noise3d = noise3Dgenerator.GetNoiseMap(position);
                chunk.Value.Build(noise2d, noise3d);
            }
        }
    }

    public void PainNoiseToScreen()
    {
        NoiseLayer2D localNoiseGen = new NoiseLayer2D(layerParameters2D);

        NoiseMap2D noise = localNoiseGen.GetNoiseMap(-5, -5, 10 * CubicChunk.valuesPerAxis);
        mapNoise.texture = (Texture2D)noise;


        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Noise_Map.jpg");
        byte[] bytes = ImageConversion.EncodeToJPG((Texture2D)noise);
        File.WriteAllBytes(path, bytes);

        noise = localNoiseGen.GetNoiseMap(0, 0);
        oneChunkNoise.texture = (Texture2D)noise;

        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Noise_Chunk.jpg");
        bytes = ImageConversion.EncodeToJPG((Texture2D)noise);
        File.WriteAllBytes(path, bytes);
        
    }

    public void ReadParamsFromFile()
    {
        List<NoiseLayer> layers = new List<NoiseLayer>();
        foreach (string file in Directory.GetFiles("NoiseLayers", "*.json"))
        {
            string contents = File.ReadAllText(file);

        }

        string fileName = "NoiseLayers2D.json";
        string jsonString;

        using (StreamReader inputFile = new(fileName))
        {
            jsonString = inputFile.ReadToEnd();
        }

        NoiseLayer2D[] noiseLayers2D = JsonUtility.FromJson<NoiseLayer2D[]>(jsonString);

    }

}

namespace Xml2CSharp
{
    [XmlRoot(ElementName = "key")]
    public class Key
    {
        [XmlElement(ElementName = "time")]
        public string Time { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "Modifier")]
    public class Modifier
    {
        [XmlElement(ElementName = "key")]
        public List<Key> Key { get; set; }
    }

    [XmlRoot(ElementName = "Layer")]
    public class Layer
    {
        [XmlElement(ElementName = "Type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "StartFrequency")]
        public string StartFrequency { get; set; }
        [XmlElement(ElementName = "FrequencyModifier")]
        public string FrequencyModifier { get; set; }
        [XmlElement(ElementName = "AmplitudeModifier")]
        public string AmplitudeModifier { get; set; }
        [XmlElement(ElementName = "Octaves")]
        public string Octaves { get; set; }
        [XmlElement(ElementName = "Seed")]
        public string Seed { get; set; }
        [XmlElement(ElementName = "Modifier")]
        public Modifier Modifier { get; set; }
    }

    [XmlRoot(ElementName = "XML")]
    public class XML
    {
        [XmlElement(ElementName = "Layer")]
        public Layer Layer { get; set; }
    }

}

