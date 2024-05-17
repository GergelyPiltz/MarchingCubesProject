
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField, Range(1,100)] int incrementX = 15;
    public void IncrementPositionForward()
    {
        transform.position = new Vector3(transform.position.x + incrementX, transform.position.y, transform.position.z);
    }

    public void IncrementPositionBackward()
    {
        transform.position = new Vector3(transform.position.x - incrementX, transform.position.y, transform.position.z);
    }
}
