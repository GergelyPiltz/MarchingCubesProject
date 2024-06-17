#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraPosition))]
public class CameraPositionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraPosition cameraPosition = target as CameraPosition;

        if (GUILayout.Button("Move Camera Up"))
            cameraPosition.IncrementPositionForward();

        if (GUILayout.Button("Move Camera Down"))
            cameraPosition.IncrementPositionBackward();

    }
}
#endif