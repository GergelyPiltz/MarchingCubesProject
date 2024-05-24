using UnityEngine;

public class ClickableSphere : MonoBehaviour
{
    bool value;
    Material mat1;
    Material mat2;
    Renderer renderer;

    private void Start()
    {
        value = true;
        mat1 = Resources.Load("Materials/DEV_Blue", typeof(Material)) as Material;
        mat2 = Resources.Load("Materials/DEV_Green", typeof(Material)) as Material;
        renderer = GetComponent<Renderer>();
        renderer.material = mat1;
        
    }
    void OnMouseDown()
    {
        value = !value;
        if (value)
            renderer.material = mat1;
        else 
            renderer.material = mat2;
    }

    public bool GetValue()
    {
        return value;
    }

    public void SetSphereSize(float size)
    {
        transform.localScale = new Vector3(size, size, size);
    }
}