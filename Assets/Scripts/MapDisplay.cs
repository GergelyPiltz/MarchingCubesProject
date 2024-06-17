using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    Renderer _renderer;
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        Color32[] colorMap = new Color32[width * height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                colorMap[x + width * y] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);

        texture.SetPixels32(colorMap);
        texture.Apply();

        _renderer.material.mainTexture = texture;
    }

    public void DrawColorMap(Color32[] colorMap, int width, int height)
    {
        Texture2D texture = new(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            
        };

        texture.SetPixels32(colorMap);
        texture.Apply();

        _renderer.material.mainTexture = texture;
    }

}
