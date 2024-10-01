using UnityEngine;

public class FlightControl : MonoBehaviour
{


    [SerializeField] Transform head;

    float movementSpeed = 50f;

    void FixedUpdate()
    {
        if (Cursor.lockState == CursorLockMode.None)
            return;

        Vector3 moveDirection = head.forward * Input.GetAxis("Vertical") + head.right * Input.GetAxis("Horizontal");

        transform.position += movementSpeed * Time.deltaTime * moveDirection; 

        #region AircraftLikeControls

        //if (Input.GetKeyDown(KeyCode.W))
        //    velocity += speedSensitivity * Time.deltaTime;
        //else if (Input.GetKeyDown(KeyCode.S))
        //    velocity -= speedSensitivity * Time.deltaTime;

        //if (Input.GetKey(KeyCode.Keypad4))
        //    rotation.x += rotationSensitivity.x * Time.deltaTime;
        //else if (Input.GetKey(KeyCode.Keypad6))
        //    rotation.x -= rotationSensitivity.x * Time.deltaTime;

        //if (Input.GetKey(KeyCode.A))
        //    rotation.y -= rotationSensitivity.y * Time.deltaTime;
        //else if (Input.GetKey(KeyCode.D))
        //    rotation.y += rotationSensitivity.y * Time.deltaTime;

        //if (Input.GetKey(KeyCode.Keypad8))
        //    rotation.z -= rotationSensitivity.z * Time.deltaTime;
        //else if (Input.GetKey(KeyCode.Keypad5))
        //    rotation.z += rotationSensitivity.z * Time.deltaTime;

        //rotation.y -= rotation.x * 0.01f;

        //velocityByAxis.x = velocity * Mathf.Cos(rotation.z * Mathf.Deg2Rad) * Mathf.Cos(rotation.y * Mathf.Deg2Rad);
        //velocityByAxis.y = velocity * Mathf.Sin(rotation.z * Mathf.Deg2Rad);
        //velocityByAxis.z = velocity * -Mathf.Sin(rotation.y * Mathf.Deg2Rad);


        //transform.SetPositionAndRotation(transform.position + velocityByAxis, Quaternion.Euler(rotation));

        #endregion

    }

}
