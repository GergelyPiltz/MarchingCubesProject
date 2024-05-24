using TMPro;
using UnityEngine;

public class ScalarField : MonoBehaviour
{

    float[,,] terrainData;
    [SerializeField, Range(4, 20)] int size;
    [SerializeField, Range(0.5f, 9.5f)] float radius;
    [SerializeField] AnimationCurve curveX;
    [SerializeField] AnimationCurve curveZ;

    [SerializeField, Range(0.1f, 5f)] float valueDisplayFontSize = 1f;
    GameObject DebugParent;
    GameObject[,,] valueDisplay;
    TextMeshPro[,,] valueDisplayText;

    bool isStarted = false;
    void Start()
    {
        GenerateField();
        isStarted = true;
    }

    private void OnValidate()
    {
        size = Mathf.Clamp(size, 4, 20);
        radius = Mathf.Clamp(radius, 0.5f, (float)(size) / 2f - 0.5f);

        if (!Application.isPlaying || !isStarted) return;

        GenerateField();
        for (int i = 0; i < size + 1; i++)
            for (int j = 0; j < size + 1; j++)
                for (int k = 0; k < size + 1; k++)
                    valueDisplayText[i, j, k].fontSize = valueDisplayFontSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3((float)size / 2, (float)size / 2, (float)size / 2), new Vector3(size, size, size));
    }

    private void GenerateField()
    {
        if (isStarted)
            for (int i = 0; i < size + 1; i++)
                for (int j = 0; j < size + 1; j++)
                    for (int k = 0; k < size + 1; k++)
                        Destroy(valueDisplay[i, j, k]);


        //terrainData = HelperFunctions.GenerateSphereArray(size + 1, radius);
        terrainData = HelperFunctions.GenerateCurvedArray(size + 1, curveX, curveZ);

        DebugParent = gameObject;
        valueDisplay = new GameObject[size + 1, size + 1, size + 1];
        valueDisplayText = new TextMeshPro[size + 1, size + 1, size + 1];
        for (int i = 0; i < size + 1; i++)
            for (int j = 0; j < size + 1; j++)
                for (int k = 0; k < size + 1; k++)
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
    }

    float SampleTerrain(Vector3Int point)
    {
        return terrainData[point.x, point.y, point.z];
    }

}
