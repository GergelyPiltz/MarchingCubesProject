using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    readonly GameObject chunkObject;
    readonly Vector3Int chunkPosition;

    readonly MeshFilter meshFilter;
    readonly MeshCollider meshCollider;
    readonly MeshRenderer meshRenderer;

    List<Vector3> vertices;
    List<int> triangles;

    readonly int xLength = 10;
    readonly int yLength = 100;
    readonly int zLength = 10;
    readonly float terrainHeight = 0f;

    readonly bool smoothTerrain = true;

    float[,] noiseMap;
    float[,,] terrainData;
    int[,,,] vertexIndexArray;

    public Chunk(Vector3Int _position, Vector3Int _dimensions, Transform parent, int index)
    {
        chunkObject = new GameObject();
        chunkObject.transform.parent = parent;
        chunkObject.name = "Chunk(" + index + ")";
        chunkPosition = _position;
        chunkObject.transform.position = chunkPosition;

        xLength = _dimensions.x;
        yLength = _dimensions.y;
        zLength = _dimensions.z;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");

        noiseMap = new float[xLength + 1, zLength + 1];
        terrainData = new float[xLength + 1, yLength + 1, zLength + 1];
        vertexIndexArray = new int[xLength, yLength, zLength, 12];

        vertices = new();
        triangles = new();

    }

    public void GenerateTerrain(GenerationParams generationParams)
    {

        noiseMap = Noise.GenerateNoiseMap(
            xLength + 1,
            zLength + 1,
            chunkPosition.x,
            chunkPosition.z,
            generationParams.seed,
            generationParams.startFrequency,
            generationParams.frequencyModifier,
            generationParams.startAmplitude,
            generationParams.amplitudeModifier,
            generationParams.octaves
            );

        CreateTerrainData(noiseMap);
        CreateMeshData();
        BuildMesh();
    }
    
    void CreateTerrainData(float[,] noiseMap)
    {

        for (int x = 0; x < xLength + 1; x++)
            for (int z = 0; z < zLength + 1; z++)
            {
                float currentHeight = yLength * noiseMap[x, z];

                for (int y = 0; y < yLength + 1; y++)
                    terrainData[x, y, z] = (float)y - currentHeight;
            }
    }

    void CreateMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        for (int x = 0; x < xLength; x++)
            for (int y = 0; y < yLength; y++)
                for (int z = 0; z < zLength; z++)
                {
                    Vector3Int position = new(x, y, z);

                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                        cube[i] = SampleTerrain(position + Tables.CornerTable[i]);

                    int configIndex = GetCubeCongif(cube);

                    if (configIndex == 0 || configIndex == 255) continue;

                    for (int vertexCounter = 0; vertexCounter < 15/*16*/; vertexCounter++)
                    {
                        int edgeIndex = Tables.TriangleTable[configIndex, vertexCounter];

                        if (edgeIndex == -1) break;

                        switch (edgeIndex)
                        {
                            case 0:
                                if (y > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 2];
                                    triangles.Add(vertexIndexArray[x, y - 1, z, 2]);
                                }
                                else if (z > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 4];
                                    triangles.Add(vertexIndexArray[x, y, z - 1, 4]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 1:
                                if (z > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 5];
                                    triangles.Add(vertexIndexArray[x, y, z - 1, 5]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 2:
                                if (z > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 6];
                                    triangles.Add(vertexIndexArray[x, y, z - 1, 6]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 3:
                                if (x > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 1];
                                    triangles.Add(vertexIndexArray[x - 1, y, z, 1]);
                                }
                                else if (z > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y, z - 1, 7];
                                    triangles.Add(vertexIndexArray[x, y, z - 1, 7]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 4:
                                if (y > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 6];
                                    triangles.Add(vertexIndexArray[x, y - 1, z, 6]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 7:
                                if (x > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 5];
                                    triangles.Add(vertexIndexArray[x - 1, y, z, 5]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 8:
                                if (x > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 9];
                                    triangles.Add(vertexIndexArray[x - 1, y, z, 9]);
                                }
                                else if (y > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 11];
                                    triangles.Add(vertexIndexArray[x, y - 1, z, 11]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 9:
                                if (y > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x, y - 1, z, 10];
                                    triangles.Add(vertexIndexArray[x, y - 1, z, 10]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            case 11:
                                if (x > 0)
                                {
                                    vertexIndexArray[x, y, z, edgeIndex] = vertexIndexArray[x - 1, y, z, 10];
                                    triangles.Add(vertexIndexArray[x - 1, y, z, 10]);
                                }
                                else
                                    vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                            default:
                                vertexIndexArray[x, y, z, edgeIndex] = CalculateVertex(position, edgeIndex, cube);
                                break;
                        }
                    }
                }
    }

    int CalculateVertex(Vector3Int position, int index, float[] cube)
    {

        Vector3 vert1 = position + Tables.CornerTable[Tables.EdgeTable[index, 0]];
        Vector3 vert2 = position + Tables.CornerTable[Tables.EdgeTable[index, 1]];

        Vector3 vertPos;
        if (smoothTerrain)
        {
            float vert1Sample = cube[Tables.EdgeTable[index, 0]];
            float vert2Sample = cube[Tables.EdgeTable[index, 1]];

            float difference = vert2Sample - vert1Sample;

            if (difference == 0)
                Debug.Log("DIFFERENCE IS 0");

            difference = (terrainHeight - vert1Sample) / difference;

            vertPos = vert1 + (vert2 - vert1) * difference;
        }
        else
            vertPos = (vert1 + vert2) / 2f;

        vertices.Add(vertPos);
        int vertexCount = vertices.Count;
        triangles.Add(vertexCount - 1);

        return (vertexCount - 1);
    }

    int GetCubeCongif(float[] cube)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
            if (cube[i] > terrainHeight)
                configIndex |= 1 << i;
        return configIndex;
    }

    float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x, point.y, point.z];
    }

    void BuildMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    
}
