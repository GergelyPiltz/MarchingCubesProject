using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeGridPoints : MonoBehaviour
{
    public float[,,] terrainData;
    [SerializeField] int xLength = 10;
    [SerializeField] int yLength = 10;
    [SerializeField] int zLength = 10;
    [SerializeField] int scale = 7;
    [SerializeField, Range(0.1f, 1f)] float sphereSize = 0.1f;
    float terrainLevel = 0;

    //[SerializeField] GameObject sphere;
    Material blue;
    Material green;

    GameObject[,,] spheres; 

    bool isScriptLoaded = false;
    void Start()
    {
        CreateTerrain();

        blue = Resources.Load("Materials/DEV_Blue", typeof(Material)) as Material;
        green = Resources.Load("Materials/DEV_Green", typeof(Material)) as Material;

        spheres = new GameObject[xLength + 1, yLength + 1, zLength + 1];

        GameObject temp;
        for (int i = 0; i < xLength + 1; i++)
            for (int j = 0; j < yLength + 1; j++)
                for (int k = 0; k < zLength + 1; k++)
                {
                    temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(temp.GetComponent<MeshCollider>());
                    temp.transform.parent = transform;
                    temp.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    temp.transform.localPosition = new Vector3(i, j, k);
                    if (SampleTerrain(new Vector3Int(i, j, k)) < terrainLevel)
                        temp.GetComponent<Renderer>().material = green;
                    else
                        temp.GetComponent<Renderer>().material = blue;

                    spheres[i, j, k] = temp;
                }
        isScriptLoaded = true;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && isScriptLoaded)
            for (int i = 0; i < xLength + 1; i++)
                for (int j = 0; j < yLength + 1; j++)
                    for (int k = 0; k < zLength + 1; k++)
                        spheres[i, j, k].transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3((float)xLength / 2, (float)yLength / 2, (float)zLength / 2), new Vector3(xLength, yLength, zLength));
    }

    void CreateTerrain()
    {

        float xCoord;
        float zCoord;
        float noiseValue;
        float currentHeight;
        terrainData = new float[xLength + 1, yLength + 1, zLength + 1];
        for (int x = 0; x < xLength + 1; x++)
            for (int z = 0; z < zLength + 1; z++)
            {
                xCoord = (float)x / xLength * scale;
                zCoord = (float)z / zLength * scale;

                noiseValue = Mathf.PerlinNoise(xCoord, zCoord);
                noiseValue = Mathf.Clamp(noiseValue, 0, 1);

                currentHeight = yLength * noiseValue;

                for (int y = 0; y < yLength + 1; y++)
                    terrainData[x, y, z] = (float)y - currentHeight;
            }
    }

    float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x, point.y, point.z];
    }

}
