using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tests : MonoBehaviour
{
    [SerializeField] Transform player;

    void Start()
    {
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
            yield return new WaitForSeconds(0.5f);
            Vector3 current = player.position;
            player.position = new Vector3(current.x + (CubicChunk.cubesPerAxis - 2), current.y, 100);
        }
    }

}
