using UnityEngine;

public class CubeGridPoints : MonoBehaviour
{
    float[,,] terrainData;
    [SerializeField, Range(4, 20)] int size;
    [SerializeField, Range(0.5f, 9.5f)] float radius;
    [SerializeField] AnimationCurve curveX;
    [SerializeField] AnimationCurve curveZ;
    [SerializeField, Range(0.1f, 1f)] float sphereSize = 0.1f;

    float terrainLevel = 0;

    //[SerializeField] GameObject sphere;
    Material blue;
    Material green;

    GameObject[,,] spheres; 

    bool isStarted = false;
    void Start()
    {

        //terrainData = HelperFunctions.GenerateSphereArray(size + 1, radius);
        //terrainData = HelperFunctions.GenerateCurvedArray(size + 1, curveX, curveZ);

        blue = Resources.Load("Materials/DEV_Blue", typeof(Material)) as Material;
        green = Resources.Load("Materials/DEV_Green", typeof(Material)) as Material;

        spheres = new GameObject[size + 1, size + 1, size + 1];

        GameObject temp;
        for (int i = 0; i < size + 1; i++)
            for (int j = 0; j < size + 1; j++)
                for (int k = 0; k < size + 1; k++)
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
        isStarted = true;
    }

    private void OnValidate()
    {
        size = Mathf.Clamp(size, 4, 20);
        radius = Mathf.Clamp(radius, 0.5f, (float)(size) / 2f - 0.5f);

        if (Application.isPlaying && isStarted)
            for (int i = 0; i < size + 1; i++)
                for (int j = 0; j < size + 1; j++)
                    for (int k = 0; k < size + 1; k++)
                        spheres[i, j, k].transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3((float)size / 2, (float)size / 2, (float)size / 2), new Vector3(size, size, size));
    }

    float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x, point.y, point.z];
    }

}
