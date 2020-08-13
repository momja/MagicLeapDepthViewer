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
    private static List<Vector3> previousCoords = new List<Vector3>();
    private static Transform lastUpdatedTransform;

    // The offset from the tracker object center to the center of the model
    private Vector3 originOffset;

    // The settings for the global collection of trackers
    // The rotational offset of the tracker object relative to the model's rotation
    public Quaternion originRotationOffset;
    // Approximate center of model given the tracker objects position, and its offset. In world space
    public Vector3 modelCenterApprox;

    [HideInInspector]
    public int posIdentifier = 0;
    [HideInInspector]
    public bool visible = false;

    public TrackerObject() {
        trackerObjects.Add(this);
    }

    void Start()
    {
        // transform offset to offset in world space
        originOffset = transform.position;
        originOffset /= transform.localScale.x;
        originRotationOffset = transform.rotation;
    }


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

    public static int getVisibleTrackerCount() {
        int cnt = 0;
        foreach (TrackerObject t in trackerObjects) {
            if (t.visible) {
                cnt++;
            }
        }
        return cnt;
    }

    public static Transform getApproxModelCenter() {
        return lastUpdatedTransform;
    }

    public static void setApproxModelCenter() {
        Vector3 avgCenter = Vector3.zero;
        Vector3 avgLookAtRotation = Vector3.zero;
        Vector3 avgLookUpRotation = Vector3.zero;
        int enabledTrackerCount = 0;
        foreach (TrackerObject tracker in trackerObjects) {
            if (tracker.visible) {
                avgCenter += tracker.modelCenterApprox;
                Quaternion upOrientedQuat = tracker.transform.rotation * Quaternion.Inverse(tracker.originRotationOffset);
                avgLookAtRotation += upOrientedQuat * Vector3.up;
                avgLookUpRotation += upOrientedQuat * Vector3.forward;
                enabledTrackerCount += 1;
            }
        }
        avgCenter /= enabledTrackerCount;
        if (TargetSettings.Instance.hideOnMissingTrackers && enabledTrackerCount == 0 || TargetSettings.Instance.model.transform == null) {
            // Hide model if no trackers are found
            TargetSettings.Instance.model.SetActive(false);
            return;
        } else {
            TargetSettings.Instance.model.SetActive(true);
        }
        Vector3 center = Vector3.zero;
        if (TargetSettings.Instance.smoothOverFrames > 0) {
            // ease movement over last n frames to smooth movements
            if (previousCoords.Count >= TargetSettings.Instance.smoothOverFrames) {
                previousCoords.RemoveAt(0);
            }
            previousCoords.Add(avgCenter);

            // Calculate eased movement
            float totalWeights = 0;
            int i = 0;
            foreach (Vector3 coord in previousCoords) {
                float weight = i / previousCoords.Count;
                totalWeights += weight;
                center += coord; 
            }
            center /= previousCoords.Count;
        } else {
            center = avgCenter;
        }
        avgLookAtRotation = avgLookAtRotation/enabledTrackerCount;
        TargetSettings.Instance.modelOffset.transform.rotation = Quaternion.LookRotation(avgLookUpRotation, avgLookAtRotation);
        TargetSettings.Instance.model.transform.position = center;
        lastUpdatedTransform = TargetSettings.Instance.model.transform;
    }

    public static void printModelCenter() {
        Transform t = getApproxModelCenter();
        Vector3 pos = t.position;
        Quaternion rot = t.rotation;
        print("position: " + pos.ToString());
        print("rotation: " + rot.eulerAngles.ToString());
    }
}
