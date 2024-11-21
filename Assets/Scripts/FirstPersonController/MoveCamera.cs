using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;

    void Update()
    {
        //transform.position = cameraPosition.position;
        //transform.rotation = cameraPosition.rotation;
        transform.SetPositionAndRotation(cameraPosition.position, cameraPosition.rotation);
    }
}