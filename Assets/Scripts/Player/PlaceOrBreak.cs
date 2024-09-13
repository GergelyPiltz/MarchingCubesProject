using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOrBreak : MonoBehaviour
{
    private World world;
    private Camera mainCamera;
    private readonly float maxDistance = 20f;

    [SerializeField] private Transform indicator;
    private Vector3 half = new (0.5f, 0.5f, 0.5f);

    void Start()
    {
        GameObject.Find("World").TryGetComponent(out world);
        mainCamera = Camera.main;

    }

    void Update()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, maxDistance))
        {
            //Debug.Log(hit.point);
            indicator.gameObject.SetActive(true);
            indicator.position = Vector3Int.FloorToInt(hit.point) + half;
            if (Input.GetKeyDown(KeyCode.Mouse0)) world.ModifyBlock(hit.point, false);
            if (Input.GetKeyDown(KeyCode.Mouse1)) world.ModifyBlock(hit.point, true);
        }
        else
        {
            indicator.gameObject.SetActive(false);
        }
    }
}
