using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

public class TrackerObject : MonoBehaviour
{
    public static List<TrackerObject> trackerObjects = new List<TrackerObject>();

    // The offset from the tracker object center to the center of the model
    private Vector3 originOffset;
    // The rotational offset of the tracker object relative to the model's rotation
    public Quaternion originRotationOffset;
    // Approximate center of model given the tracker objects position, and its offset. In world space
    public Vector3 modelCenterApprox;

    [HideInInspector]
    public int posIdentifier = 0;
    [HideInInspector]
    public bool visible = false;
    // Start is called before the first frame update
    void Start()
    {
        DefaultTrackableEventHandler ev = gameObject.AddComponent<DefaultTrackableEventHandler>();
        trackerObjects.Add(this);
        // transform offset to offset in world space
        originOffset = transform.position;
        originOffset /= transform.localScale.x;
        originRotationOffset = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (visible) {
            modelCenterApprox = transform.TransformPoint(Quaternion.Inverse(originRotationOffset)* -originOffset);
        }
    }

    private void OnDrawGizmos()
    {
        if (visible)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.InverseTransformPoint(modelCenterApprox));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.position - originOffset);
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
