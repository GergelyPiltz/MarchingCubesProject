using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoughTerrain : MonoBehaviour
{
    float[,,] terrainData;
    [SerializeField, Range(4, 20)] int size;
    [SerializeField, Range(0.5f, 9.5f)] float radius;
    [SerializeField] AnimationCurve curveX;
    [SerializeField] AnimationCurve curveZ;
    readonly float terrainLevel = 0;

    MeshFilter meshFilter;

    List<Vector3> vertices;
    List<int> triangles;

    [SerializeField] bool smoothTerrain = true;

    // Start is called before the first frame update
    void Start()
    {

        meshFilter = GetComponent<MeshFilter>();

        terrainData = new float[size + 1, size + 1, size + 1];

        vertices = new();
        triangles = new();

    }

    // Update is called once per frame
    void Update()
    {
        GenerateTerrain();
    }

    private void OnValidate()
    {

        size = Mathf.Clamp(size, 4, 20);
        radius = Mathf.Clamp(radius, 0.5f, (float)(size) / 2f - 0.5f);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3((float)size / 2, (float)size / 2, (float)size / 2), new Vector3(size, size, size));
    }

    public void GenerateTerrain()
    {
        //CreateTerrainData();
        //terrainData = HelperFunctions.GenerateSphereArray(size + 1, radius);
        terrainData = HelperFunctions.GenerateCurvedArray(size + 1, curveX, curveZ);
        CreateMeshData();
        BuildMesh();
    }

    void CreateMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    Vector3Int position = new(x, y, z);

                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                        cube[i] = SampleTerrain(position + Tables.CornerTable[i]);

                    int configIndex = GetCubeCongif(cube);

                    if (configIndex == 0 || configIndex == 255) continue;

                    for (int TriangleVertexCounter = 0; TriangleVertexCounter < 15/*16*/; TriangleVertexCounter++)
                    {
                        int edgeIndex = Tables.TriangleTable[configIndex, TriangleVertexCounter];

                        if (edgeIndex == -1) break;

                        CalculateVertex(position, edgeIndex, cube);
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

            difference = (terrainLevel - vert1Sample) / difference;

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
            if (cube[i] > terrainLevel)
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
    }
}
