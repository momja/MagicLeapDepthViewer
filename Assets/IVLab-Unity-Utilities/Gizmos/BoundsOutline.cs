using IVLab.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsOutline : MonoBehaviour
{
    public static void Init(GameObject go)
    {
        var outline = go.AddComponent<BoundsOutline>();
    }
    public IBoundable BoundedObject { get;set;}
    [SerializeField] Bounds _bounds;
    private void OnGUI()
    {
        _bounds = BoundedObject?.Bounds ?? default;
    }

    // Start is called before the first frame update
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        if(BoundedObject != null)
        Gizmos.DrawWireCube(BoundedObject.Bounds.center, BoundedObject.Bounds.size);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
