#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class AssetProvider
{
    //-sealed-----------------------SINGLETON-------------------------------
    private static readonly Lazy<AssetProvider> lazy =
        new Lazy<AssetProvider>(() => new AssetProvider());
    public static AssetProvider Instance { get { return lazy.Value; } }
    //----------------------------------------------------------------------

    private readonly string[] pathsToResources = new string[] {
            Path.Combine("Assets","LowpolyStreetPack","Prefabs","Foliage","Bushes"),
            Path.Combine("Assets","LowpolyStreetPack","Prefabs","Foliage","Trees"),
    };
    
    public readonly GameObject[] Vegetation;

    private AssetProvider()
    {

        string[] guids = AssetDatabase.FindAssets("t:prefab", pathsToResources);
        int numberOfPrefabs = guids.Length;
        Vegetation = new GameObject[numberOfPrefabs];
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Vegetation[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }

}
#endif