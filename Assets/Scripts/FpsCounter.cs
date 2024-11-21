using UnityEngine;
using System.Collections;
using TMPro;

public class FpsCounter : MonoBehaviour
{
    private float count;
    [SerializeField] private GameObject FPSobj;
    private TextMeshProUGUI FPStext;

    private IEnumerator Start()
    {
        FPStext = FPSobj.GetComponent<TextMeshProUGUI>();

        WaitForSeconds wait = new (0.1f);
        //GUI.depth = 2;
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            FPStext.text = "FPS: " + Mathf.Round(count);
            yield return wait;
        }
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(5, 40, 400, 100), "FPS: " + Mathf.Round(count));
    //    GUI.skin.label.fontSize = 32;
    //}
}