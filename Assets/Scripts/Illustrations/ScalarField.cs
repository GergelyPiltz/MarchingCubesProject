using TMPro;
using UnityEngine;

public class ScalarField : MonoBehaviour
{

    public float[,,] terrainData;
    [SerializeField] int xLength = 10;
    [SerializeField] int yLength = 10;
    [SerializeField] int zLength = 10;
    [SerializeField] int scale = 7;

    [SerializeField] float valueDisplayFontSize = 1f;
    GameObject DebugParent;
    GameObject[,,] valueDisplay;
    TextMeshPro[,,] valueDisplayText;

    bool scriptLoaded = false;
    void Start()
    {
        CreateTerrain();

        DebugParent = gameObject;
        valueDisplay = new GameObject[xLength + 1, yLength + 1, zLength + 1];
        valueDisplayText = new TextMeshPro[xLength + 1, yLength + 1, zLength + 1];
        for (int i = 0; i < xLength + 1; i++)
            for (int j = 0; j < yLength + 1; j++)
                for (int k = 0; k < zLength + 1; k++)
                {

                    valueDisplay[i, j, k] = new GameObject();
                    valueDisplay[i, j, k].transform.parent = DebugParent.transform;
                    valueDisplay[i, j, k].AddComponent<TextMeshPro>();
                    valueDisplay[i, j, k].transform.position = new Vector3Int(i, j, k);

                    valueDisplayText[i, j, k] = valueDisplay[i, j, k].GetComponent<TextMeshPro>();
                    valueDisplayText[i, j, k].fontSize = valueDisplayFontSize;
                    valueDisplayText[i, j, k].color = Color.black;
                    valueDisplayText[i, j, k].horizontalAlignment = HorizontalAlignmentOptions.Center;
                    valueDisplayText[i, j, k].verticalAlignment = VerticalAlignmentOptions.Middle;
                    valueDisplayText[i, j, k].text = SampleTerrain(new Vector3Int(i, j, k)).ToString("0.00");

                }
        scriptLoaded = true;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && scriptLoaded)
            for (int i = 0; i < xLength + 1; i++)
                for (int j = 0; j < yLength + 1; j++)
                    for (int k = 0; k < zLength + 1; k++)
                        valueDisplayText[i, j, k].fontSize = valueDisplayFontSize;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R))
        //    for (int i = 0; i < xLength + 1; i++)
        //        for (int j = 0; j < yLength + 1; j++)
        //            for (int k = 0; k < zLength + 1; k++)
        //                valueDisplayText[i, j, k].fontSize = valueDisplayFontSize;

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
