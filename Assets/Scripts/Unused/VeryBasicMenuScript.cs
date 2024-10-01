using UnityEngine;
using UnityEngine.SceneManagement;

public class VeryBasicMenuScript : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void PlayBlocky()
    {
        CubicChunk.SetTerrainProfile(CubicChunk.TerrainProfile.Blocky);
        SceneManager.LoadScene(1);
    }

    public void PlaySmooth()
    {
        CubicChunk.SetTerrainProfile(CubicChunk.TerrainProfile.Smooth);
        SceneManager.LoadScene(1);
    }
}
