using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsExtentions 
{

    public static float MaxDimension(this Bounds bounds)
    {
        return bounds.size.MaxComponent();
    }
}
