using UnityEngine;

public class ChunkBorder : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 objectSize = new(CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis, CubicChunk.cubesPerAxis);
        Vector3 objectCenter = transform.position + objectSize / 2;
        Gizmos.DrawWireCube(objectCenter, objectSize);
    }
}
