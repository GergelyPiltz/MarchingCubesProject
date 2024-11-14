using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Tests : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float delay = 2f;
    [SerializeField] Transform world;
    World worldScript;

    void Start()
    {
        worldScript = world.GetComponent<World>();
        StartCoroutine(MovePlayer());
    }

    void Update()
    {
        
    }

    IEnumerator MovePlayer()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            yield return new WaitForSeconds(delay);
            Vector3 current = player.position;
            player.position = new Vector3(current.x + (CubicChunk.cubesPerAxis - 2), current.y, 100);
            if (worldScript.WorldTest())
                Debug.Log("World: OK");
            else
                Debug.Log("World: X");
        }
    }

}
