#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunkGenerator))]
public class ChunkGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChunkGenerator chunkGenerator = target as ChunkGenerator;

        //Draws default inspector here
        DrawDefaultInspector();

        if (GUILayout.Button("Run Benchmark") && Application.isPlaying && chunkGenerator.enabled)
        {
            chunkGenerator.RunBenchmark();
        }

        if (GUILayout.Button("Write triangles to file") && Application.isPlaying && chunkGenerator.enabled)
        {
            chunkGenerator.WriteTrianglesToFile();
        }

        if (GUILayout.Button("Write vertexIndexArray to file") && Application.isPlaying && chunkGenerator.enabled)
        {
            chunkGenerator.WriteVertexIndexArrayToFile();
        }

    }
}
#endif