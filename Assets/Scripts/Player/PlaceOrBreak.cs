using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOrBreak : MonoBehaviour
{
    private World world;
    private Camera mainCamera;
    float maxDistance = 20f;

    void Start()
    {
        GameObject.Find("World").TryGetComponent(out world);
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, maxDistance))
            {
                CubicChunk chunk = world.GetChunk(hit.point);
                chunk.ModifyTerrain(hit.point - chunk.Position, false);
                Debug.Log("Destroy");
            }

        if (Input.GetKeyDown(KeyCode.Mouse1))
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, maxDistance))
            {
                world.GetChunk(hit.point).ModifyTerrain(hit.point, true);
                Debug.Log("Place");
            }
    }
}
