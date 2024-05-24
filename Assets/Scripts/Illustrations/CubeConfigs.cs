using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CubeConfigs : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f)] float sphereSize;
    ClickableSphere[] spheres;

    List<Vector3> vertices;
    List<int> triangles;

    MeshFilter meshFilter;
    LineRenderer lineRenderer;

    bool isStarted = false;
    void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        meshFilter = GetComponent<MeshFilter>();
        spheres = new ClickableSphere[8];
        GameObject temp;
        for (int i = 0; i < 8; i++)
        {
            temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(temp.GetComponent<MeshCollider>());
            temp.transform.parent = transform;
            temp.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            temp.transform.localPosition = Tables.CornerTable[i];
            spheres[i] = temp.AddComponent<ClickableSphere>();
        }

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = cubeEdges.Length;
        lineRenderer.loop = false;

        DrawCubeLines();

        isStarted = true;
    }

    private void Update()
    {
        UpdateConfig();
    }

    private void OnValidate()
    {
        if(!isStarted) return;
        sphereSize = Mathf.Clamp(sphereSize, 0.1f, 1f);
        UpdateSphereSizes(sphereSize);
    }

    public void UpdateConfig()
    {
        vertices.Clear(); 
        triangles.Clear();

        int configIndex = 0;
        for (int i = 0; i < 8; i++)
            if (spheres[i].GetValue())
                configIndex |= 1 << i;

        if (configIndex == 0 || configIndex == 255)
        {
            meshFilter.mesh = null;
            return;
        }

        for (int TriangleVertexCounter = 0; TriangleVertexCounter < 15/*16*/; TriangleVertexCounter++)
        {
            int edgeIndex = Tables.TriangleTable[configIndex, TriangleVertexCounter];

            if (edgeIndex == -1) break;

            Vector3 vert1 = Tables.CornerTable[Tables.EdgeTable[edgeIndex, 0]];
            Vector3 vert2 = Tables.CornerTable[Tables.EdgeTable[edgeIndex, 1]];

            Vector3 vertPos = (vert1 + vert2) / 2f;

            vertices.Add(vertPos);
            triangles.Add(vertices.Count - 1);
        }

        BuildMesh();
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

    void DrawCubeLines()
    {
        Vector3[] positions = new Vector3[cubeEdges.Length];
        for (int i = 0; i < cubeEdges.Length; i++)
        {
            positions[i] = cubeVertices[cubeEdges[i]];
        }

        lineRenderer.SetPositions(positions);
    }

    // Cube vertices
    private readonly Vector3[] cubeVertices = new Vector3[]
    {
        new (0f, 0f, 0f),
        new (1f, 0f, 0f),
        new (1f, 1f, 0f),
        new (0f, 1f, 0f),
        new (0f, 0f, 1f),
        new (1f, 0f, 1f),
        new (1f, 1f, 1f),
        new (0f, 1f, 1f)
    };

    // Line order to form the cube edges
    private readonly int[] cubeEdges = new int[]
    {
        0, 4, 5, 6, 7, 4, 0, 1, 5, 1, 2, 6, 2, 3, 7, 3, 0
    };
    
    private void UpdateSphereSizes(float size)
    {
        foreach (var sphere in spheres)
        { 
            sphere.SetSphereSize(size);
        }
    }
}
