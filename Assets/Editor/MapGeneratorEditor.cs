using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = target as MapGenerator;

        //Draws default inspector here
        if (DrawDefaultInspector() && mapGenerator.autoUpdate && Application.isPlaying && mapGenerator.enabled)
        {
            mapGenerator.UpdateMapDisplay();
        }

        if (GUILayout.Button("Update Display") && Application.isPlaying && mapGenerator.enabled)
        {
            mapGenerator.UpdateMapDisplay();
        }

        if (GUILayout.Button("Generate Chunks") && Application.isPlaying && mapGenerator.enabled)
        {
            mapGenerator.GenerateMap();
        }

    }
}
