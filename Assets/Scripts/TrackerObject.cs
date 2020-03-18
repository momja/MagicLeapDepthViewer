using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TrackerObject : MonoBehaviour
{
    public static List<TrackerObject> trackerObjects = new List<TrackerObject>();

    // The offset from the tracker object center to the center of the model
    public Vector3 originOffset;
    // The rotational offset of the object from the rotation of the model
    public Vector3 rotationEuler;
    // Approximate center of model given the tracker objects position, and its offset. In world space
    public Vector3 modelCenterApprox;

    [HideInInspector]
    public Quaternion imageRotation
    {
        get { return Quaternion.Euler(rotationEuler); }
    }
    [HideInInspector]
    public int posIdentifier = 0;
    [HideInInspector]
    public bool visible = false;
    // Start is called before the first frame update
    void Start()
    {
        posIdentifier = trackerObjects.Count;
        trackerObjects.Add(this);
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
