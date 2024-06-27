using UnityEngine;

public class NoiseModifier : MonoBehaviour
{
    #region Singleton
    public static NoiseModifier Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Multiple instances of NoiseModifier exist. Destroying instance.");
            if (gameObject != null)
                Debug.Log("Extra script was attached to " + gameObject.name);
            else
                Debug.Log("Extra script wasn't attached to a gameobject");
            Destroy(this);
        }
        //DontDestroyOnLoad(this);
    }
    #endregion

    [SerializeField] AnimationCurve curve;
    private void Start()
    {
        curve = new()
        {
            keys = new Keyframe[]
            {
                new(0,0),
                new(0.6f,0.0f),
                new(0.8f,1f),
                new(1,1),
            }
        };
    }

    public float Evaluate(float time)
    {
        return curve.Evaluate(time);
    }
}
