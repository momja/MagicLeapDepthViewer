using IVLab.Utilities;
using IVLab.Utilities.GenericObjectPool;
using UnityEngine;

public class ContainBoundables : MonoBehaviour, IBoundable
{

    public static void Init(GameObject go)
    {
        ContainBoundables containBoundables =  go.AddComponent<ContainBoundables>();
        var innerScaleObject = new GameObject("Inner Scale");
        innerScaleObject.transform.SetParent(go.transform, false);
        containBoundables.ScaleObject = innerScaleObject.transform;
        containBoundables.Bounds = new Bounds(Vector3.zero, Vector3.one);
    }

    [SerializeField] Bounds _bounds;
    public Bounds Bounds { get { return _bounds; } set { _bounds = value; } }
    public Transform ScaleObject { get; protected set; }

    public bool autoUpdate = true;

    public void AddGameObject(GameObject gameObject, bool worldPositionStays = false)
    {
        gameObject.transform.SetParent(ScaleObject, worldPositionStays);
    }

    // Start is called before the first frame update
    void Start() {     
        ScaleObject = transform.GetChild(0);
        GenericObjectPool OP = GenericObjectPool.Instance;
        OP.GetObjectFromPool<BoundsOutline>(transform).BoundedObject = this;

    }

    // Update is called once per frame
    public void UpdateScale()
    {
        var boundables = ScaleObject.GetComponentsInChildren<IBoundable>();
        if (boundables.Length == 0) return;
        Bounds innerBounds = Boundable.EncapsulateAll(boundables);
        if (innerBounds.size == Vector3.zero) return;
        float scaleFactor = Vector3.Scale(_bounds.size, innerBounds.size.Reciprocal()).MinComponent();
        ScaleObject.transform.localScale = Vector3.one * scaleFactor;
        ScaleObject.transform.localPosition = _bounds.center-innerBounds.center * scaleFactor;
        
    }

    void Update()
    {
        if(autoUpdate)UpdateScale();
    }
}
