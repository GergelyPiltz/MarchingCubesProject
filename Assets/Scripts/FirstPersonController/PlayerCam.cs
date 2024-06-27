using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPosition;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //mouse inputs
        float mouseX = Input.GetAxisRaw("Mouse X") /** Time.deltaTime*/ * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") /** Time.deltaTime*/ * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate camPosition and orientation
        cameraPosition.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetSensitivity(float value)
    {
        sensX = value;
        sensY = value;
    }
}
