using UnityEngine;
using System.Collections;

public class FpsCounter : MonoBehaviour
{
    private float count;

    private IEnumerator Start()
    {
        WaitForSeconds wait = new (0.1f);
        GUI.depth = 2;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            yield return wait;
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(5, 40, 400, 100), "FPS: " + Mathf.Round(count));
        GUI.skin.label.fontSize = 32;
    }
}