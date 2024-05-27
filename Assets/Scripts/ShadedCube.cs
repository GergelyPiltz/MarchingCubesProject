using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class ShadedCube : MonoBehaviour
{

    [SerializeField] bool smoothShaded = false;

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load("Materials/DEV_Default") as Material;

        if (smoothShaded)
        {
            // Define vertices for a smooth shaded cube (8 vertices total)
            Vector3[] vertices = new Vector3[]
            {
            new (0, 0, 0), // 0
            new (1, 0, 0), // 1
            new (1, 1, 0), // 2
            new (0, 1, 0), // 3
            new (0, 0, 1), // 4
            new (1, 0, 1), // 5
            new (1, 1, 1), // 6
            new (0, 1, 1), // 7
            };

            // Define triangles (each face has 2 triangles, 12 triangles total)
            int[] triangles = new int[]
            {
                // Front face
                4, 5, 6, 4, 6, 7,
                // Back face
                0, 2, 1, 0, 3, 2,
                // Left face
                0, 7, 3, 0, 4, 7,
                // Right face
                1, 2, 6, 1, 6, 5,
                // Top face
                3, 7, 6, 3, 6, 2,
                // Bottom face
                0, 1, 5, 0, 5, 4,
            };

            // Define normals for each vertex
            Vector3[] normals = new Vector3[]
            {
                new Vector3(-1, -1, -1).normalized, // 0
                new Vector3( 1, -1, -1).normalized, // 1
                new Vector3( 1,  1, -1).normalized, // 2
                new Vector3(-1,  1, -1).normalized, // 3
                new Vector3(-1, -1,  1).normalized, // 4
                new Vector3( 1, -1,  1).normalized, // 5
                new Vector3( 1,  1,  1).normalized, // 6
                new Vector3(-1,  1,  1).normalized, // 7
            };

            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles,
                normals = normals
            };

            //mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }
        else
        {

            // Define vertices for each face (24 vertices total, 4 per face)
            Vector3[] vertices = new Vector3[]
            {
                // Front face
                new (0, 0, 1),
                new (1, 0, 1),
                new (1, 1, 1),
                new (0, 1, 1),

                // Back face
                new (1, 0, 0),
                new (0, 0, 0),
                new (0, 1, 0),
                new (1, 1, 0),

                // Left face
                new (0, 0, 0),
                new (0, 0, 1),
                new (0, 1, 1),
                new (0, 1, 0),

                // Right face
                new (1, 0, 1),
                new (1, 0, 0),
                new (1, 1, 0),
                new (1, 1, 1),

                // Top face
                new (0, 1, 1),
                new (1, 1, 1),
                new (1, 1, 0),
                new (0, 1, 0),

                // Bottom face
                new (0, 0, 0),
                new (1, 0, 0),
                new (1, 0, 1),
                new (0, 0, 1),
            };

            // Define triangles with reversed winding order (each face has 2 triangles, 12 triangles total)
            int[] triangles = new int[]
            {
                // Front face
                0, 1, 2, 0, 2, 3,
                // Back face
                4, 5, 6, 4, 6, 7,
                // Left face
                8, 9, 10, 8, 10, 11,
                // Right face
                12, 13, 14, 12, 14, 15,
                // Top face
                16, 17, 18, 16, 18, 19,
                // Bottom face
                20, 21, 22, 20, 22, 23,
            };

            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };

            // Optionally, calculate normals for flat shading
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
        }
    }

}
