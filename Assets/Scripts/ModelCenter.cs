using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelCenter : MonoBehaviour
{
    Vector3 originalScale;
    // Start is called before the first frame update
    void Start()
    {
        originalScale = gameObject.transform.localScale;        
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
            gameObject.transform.localScale = new Vector3(0,0,0);
            return;
        }
        gameObject.transform.localScale = originalScale;
        avgCenter = avgCenter/enabledTrackerCount;
        avgLookAtRotation = avgLookAtRotation/enabledTrackerCount;
        gameObject.transform.localPosition = avgCenter;
        gameObject.transform.rotation = Quaternion.LookRotation(avgLookUpRotation, avgLookAtRotation);
    }
}
