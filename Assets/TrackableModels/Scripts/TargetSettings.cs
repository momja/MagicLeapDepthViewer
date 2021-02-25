using UnityEngine;

public class TargetSettings : Singleton<TargetSettings> {
    public bool hideOnMissingTrackers = false;
    public int smoothOverFrames = 0;
    public GameObject model;

    [HideInInspector]
    public GameObject modelOffset;

    private void Start() {
        // place model inside model offset
        modelOffset = new GameObject();
        model.transform.parent = modelOffset.transform;
    }

    protected TargetSettings() {
    }
}
