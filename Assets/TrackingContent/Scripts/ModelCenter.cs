using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelCenter : MonoBehaviour
{
    public bool hideOnMissingTrackers = false;
    Vector3 originalScale;
    private static List<Vector3> previousCoords = new List<Vector3>();
    private GameObject _MODELOFFSET;
    // Start is called before the first frame update
    void Start()
    {
        _MODELOFFSET = GameObject.Find("MODELOFFSET");
        originalScale = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 avgCenter = Vector3.zero;
        Vector3 avgLookAtRotation = Vector3.zero;
        Vector3 avgLookUpRotation = Vector3.zero;
        int enabledTrackerCount = 0;
        for (int i = 0; i < TrackerObject.trackerObjects.Count; i++) {
            TrackerObject trackerObj = TrackerObject.trackerObjects[i];
            if (trackerObj.visible) {
                avgCenter += trackerObj.modelCenterApprox;
                Quaternion upOrientedQuat = 
                    trackerObj.transform.rotation * Quaternion.Inverse(trackerObj.originRotationOffset);
                avgLookAtRotation += upOrientedQuat * Vector3.up;
                avgLookUpRotation += upOrientedQuat * Vector3.forward;
                print(trackerObj.modelCenterApprox);
                print(avgCenter);
                enabledTrackerCount += 1;
            }
        }
        avgCenter = avgCenter/enabledTrackerCount;
        if (hideOnMissingTrackers && enabledTrackerCount == 0) {
            // Hide model if no trackers are found
            gameObject.transform.localScale = new Vector3(0,0,0);
            return;
        }
        // Blur movement over last 15 frames to ease moving slightly
        if (previousCoords.Count > 15) {
            previousCoords.RemoveAt(0);
        }
        previousCoords.Add(avgCenter);
        
        gameObject.transform.localScale = originalScale;
        avgLookAtRotation = avgLookAtRotation/enabledTrackerCount;
        _MODELOFFSET.transform.rotation = Quaternion.LookRotation(avgLookUpRotation, avgLookAtRotation);
        gameObject.transform.position = avgCenter;
    }
}
