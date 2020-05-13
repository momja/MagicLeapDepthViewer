using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtentions 
{
    public static float MaxComponent(this Vector3 vec)
    {
        return Mathf.Max(vec.x, vec.y, vec.z);
    }
    public static float MinComponent(this Vector3 vec)
    {
        return Mathf.Min(vec.x, vec.y, vec.z);
    }
    public static Vector3 Reciprocal(this Vector3 vec)
    {
        return new Vector3(1 / vec.x, 1 / vec.y, 1 / vec.z);
    }
}
