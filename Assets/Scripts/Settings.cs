using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores and carries over the settings to other scenes. Singleton like.
public class Settings : MonoBehaviour
{

    public CubicChunk.TerrainProfile TerrainProfile;
    public bool enableMarchingOnGPU;
    public int renderDistance = 10;

    // Start is called before the first frame update
    void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Settings").Length != 1)
        {
            Destroy(gameObject);
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }
}
