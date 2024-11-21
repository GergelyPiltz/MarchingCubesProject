using UnityEngine;

public static class HelperFunctions
{
    // Converts a Vector3 to Vector2 from the X and Z components. Vector2.y = Vector3.z
    public static Vector2 ToVector2FromXZ(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    // Converts a Vector3Int to Vector2Int from the X and Z components. Vector2Int.y = Vector3Int.z
    public static Vector2Int ToVector2FromXZ(Vector3Int v)
    {
        return new Vector2Int(v.x, v.z);
    }

    /// <summary>
    /// Floors each component of a Vector3Int to the nearest multiple of the specified value.
    /// Example: (-4,15,17) with 5 results in (-5,15,15)
    /// </summary>
    /// <param name="vector"> Vector3Int to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    public static Vector3Int  Vector3FloorToNearestMultipleOf(Vector3Int vector, int multipleOf)
    {
        int x, y, z;

        if (vector.x > 0)
            x = vector.x - vector.x % multipleOf;
        else if (vector.x < 0)
            x = vector.x - (multipleOf + vector.x % multipleOf);
        else
            x = 0;

        if (vector.y > 0)
            y = vector.y - vector.y % multipleOf;
        else if (vector.y < 0)
            y = vector.y - (multipleOf + vector.y % multipleOf);
        else
            y = 0;

        if (vector.z > 0)
            z = vector.z - vector.z % multipleOf;
        else if (vector.z < 0)
            z = vector.z - (multipleOf + vector.z % multipleOf);
        else
            z = 0;

        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Floors each component of a Vector3 to the nearest multiple of the specified value.
    /// Example: (-4,15,17) with 5 results in (-5,15,15)
    /// </summary>
    /// <param name="vector"> Vector3 to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    public static Vector3Int Vector3FloorToNearestMultipleOf(Vector3 vector, int multipleOf)
    {
        return Vector3FloorToNearestMultipleOf(Vector3Int.FloorToInt(vector), multipleOf);
    }

    /// <summary>
    /// Floors each component of a Vector2Int to the nearest multiple of the specified value.
    /// Example: (-4,17) with 5 results in (-5,15)
    /// </summary>
    /// <param name="vector"> Vector2Int to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    public static Vector2Int Vector2FloorToNearestMultipleOf(Vector2Int vector, int multipleOf)
    {
        int x, y;

        if (vector.x > 0)
            x = vector.x - vector.x % multipleOf;
        else if (vector.x < 0)
            x = vector.x - (multipleOf + vector.x % multipleOf);
        else
            x = 0;

        if (vector.y > 0)
            y = vector.y - vector.y % multipleOf;
        else if (vector.y < 0)
            y = vector.y - (multipleOf + vector.y % multipleOf);
        else
            y = 0;

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Floors each component of a Vector2 to the nearest multiple of the specified value.
    /// Example: (-4,17) with 5 results in (-5,15)
    /// </summary>
    /// <param name="vector"> Vector2 to be floored </param>
    /// <param name="multipleOf"> [component] - [component] % [multipleOf]</param>
    /// <returns></returns>
    public static Vector2Int Vector2FloorToNearestMultipleOf(Vector2 vector, int multipleOf)
    {
        return Vector2FloorToNearestMultipleOf(Vector2Int.FloorToInt(vector), multipleOf);
    }
}
