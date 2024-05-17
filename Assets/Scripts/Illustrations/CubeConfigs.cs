using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeConfigs : MonoBehaviour
{
    ClickableSphere[] spheres;

    List<Vector3> vertices;
    List<int> triangles;

    MeshFilter meshFilter;


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
    }

    private void Update()
    {
        UpdateConfig();
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

            Debug.Log(vertPos);

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


}
