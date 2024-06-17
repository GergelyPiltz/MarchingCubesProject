using UnityEngine;

public class CameraControl : MonoBehaviour
{
    float xRotation;
    float yRotation;

    [SerializeField] float sensX = 100f;
    [SerializeField] float sensY = 100f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }

        if (Cursor.lockState == CursorLockMode.None)
            return;

        //mouse inputs
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void SetSensitivity(float value)
    {
        sensX = value;
        sensY = value;
    }
}
