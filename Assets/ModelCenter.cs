using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelCenter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 avgCenter = Vector3.zero;
        Vector3 avgLookAtRotation = Vector3.zero;
        Vector3 avgLookUpRotation = Vector3.zero;
        int enabledTrackerCount = 0;
        for (int i = 0; i < TrackerObject.trackerObjects.Length; i++) {
            TrackerObject trackerObj = TrackerObject.trackerObjects[i];
            if (trackerObj.visible) {
                print($"Tracking {trackerObj.posIdentifier}");
                avgCenter += trackerObj.modelCenterApprox;
                avgLookAtRotation += trackerObj.transform.up;
                avgLookUpRotation += trackerObj.transform.forward;
                enabledTrackerCount += 1;
            }
        }
        if (enabledTrackerCount == 0) {
            return;
        }
        avgCenter = avgCenter/enabledTrackerCount;
        avgLookAtRotation = avgLookAtRotation/enabledTrackerCount;
        gameObject.transform.position = avgCenter;
        gameObject.transform.rotation = Quaternion.LookRotation(avgLookAtRotation, avgLookUpRotation);
    }
}
