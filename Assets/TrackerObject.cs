using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackerObject : MonoBehaviour
{
    public static TrackerObject[] trackerObjects = new TrackerObject[4];
    public Vector3 originOffset;
    public GameObject cylinderOffset;
    public Vector3 modelCenterApprox;
    [SerializeField] public int posIdentifier = 0;
    public bool visible = false;
    // Start is called before the first frame update
    void Start()
    {
        TrackerObject.trackerObjects[posIdentifier] = this;

        // transform offset to scaled size
        originOffset = originOffset / transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        // cylinderOffset.transform.position = gameObject.transform.position;
        if (visible) {
            // modelCenterApprox = gameObject.transform.position + originOffset;
            // cylinderOffset.transform.rotation = Quaternion.LookRotation(originOffset);

            modelCenterApprox = transform.TransformPoint(originOffset);
            cylinderOffset.transform.position = modelCenterApprox;
            cylinderOffset.transform.rotation = Quaternion.LookRotation(transform.up);
        }
    }

    void VuforiaTargetLost() {
        print($"Target {posIdentifier} lost");
        cylinderOffset.SetActive(false);
        visible = false;
    }

    void VuforiaTargetFound() {
        print($"Target {posIdentifier} found");
        cylinderOffset.SetActive(true);
        visible = true;
    }
}
