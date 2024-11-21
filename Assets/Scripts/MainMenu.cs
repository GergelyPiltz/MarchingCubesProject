using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    private Settings settings; // Stores and carries over the settings to other scenes
    [SerializeField] private GameObject renderDistanceDisplay;
    private TextMeshProUGUI renderDistanceDisplayTTP;

    // Start is called before the first frame update
    void Start()
    {
        renderDistanceDisplayTTP = renderDistanceDisplay.GetComponent<TextMeshProUGUI>();
        settings = GameObject.Find("Settings").GetComponent<Settings>();
        settings.TerrainProfile = CubicChunk.TerrainProfile.Smooth;
        settings.enableMarchingOnGPU = true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Chooses between TerrainProfile.Smooth and TerrainProfile.Blocky
    public void SetWorldType(int i)
    {
        if (i == 0)
        {
            settings.TerrainProfile = CubicChunk.TerrainProfile.Smooth;
        }
        else if (i == 1)
        {
            settings.TerrainProfile = CubicChunk.TerrainProfile.Blocky;
        }
    }

    public void EnableGPU(int i)
    {
        if (i == 0)
        {
            settings.enableMarchingOnGPU = true;
        }
        else if (i == 1)
        {
            settings.enableMarchingOnGPU = false;
        }
    }

    public void ChangeRenderDistance(float d)
    {
        int dist = Mathf.RoundToInt(d);
        renderDistanceDisplayTTP.text = dist.ToString();
        settings.renderDistance = dist;
    }

    private void OnApplicationQuit()
    {
        CubicChunk.ReleaseBuffers();
    }



}
