using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackerObject : MonoBehaviour
{
    public static TrackerObject[] trackerObjects = new TrackerObject[6];
    public Vector3 originOffset;
    public Vector3 modelCenterApprox;
    [SerializeField] public int posIdentifier = 0;
    public bool visible = false;
    // Start is called before the first frame update
    void Start()
    {
        TrackerObject.trackerObjects[posIdentifier] = this;
        // transform offset to offset in world space
        originOffset /= transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (visible) {
            modelCenterApprox = transform.TransformPoint(originOffset);
        }
    }

    void VuforiaTargetLost() {
        print($"Target {posIdentifier} lost");
        // cylinderOffset.SetActive(false);
        visible = false;
    }

    void VuforiaTargetFound() {
        print($"Target {posIdentifier} found");
        // cylinderOffset.SetActive(true);
        visible = true;
    }
}
