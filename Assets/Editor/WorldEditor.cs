//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(World))]
//public class WorldEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        World world = target as World;

//        //Draws default inspector here
//        DrawDefaultInspector();

//        if (GUILayout.Button("Generate") && Application.isPlaying && world.enabled)
//        {
//            world.BuildChunks();
//        }
        
//        if (GUILayout.Button("Paint Noise")  && world.enabled)
//        {
//            world.PainNoiseToScreen();
//        }

//    }
//}
