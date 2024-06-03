using UnityEngine;

public class ClickableSphere : MonoBehaviour
{
    bool value;
    Material mat1;
    Material mat2;
    Renderer _renderer;

    private void Start()
    {
        value = true;
        mat1 = Resources.Load("Materials/DEV_Blue", typeof(Material)) as Material;
        mat2 = Resources.Load("Materials/DEV_Green", typeof(Material)) as Material;
        _renderer = GetComponent<Renderer>();
        _renderer.material = mat1;
        
    }
    void OnMouseDown()
    {
        value = !value;
        if (value)
            _renderer.material = mat1;
        else 
            _renderer.material = mat2;
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