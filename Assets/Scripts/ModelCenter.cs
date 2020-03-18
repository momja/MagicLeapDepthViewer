using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelCenter : MonoBehaviour
{
    Vector3 originalScale;

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
                    trackerObj.transform.rotation * Quaternion.Inverse(trackerObj.imageRotation);
                print(Quaternion.Inverse(trackerObj.imageRotation).eulerAngles);
                avgLookAtRotation += upOrientedQuat * Vector3.up;
                avgLookUpRotation += upOrientedQuat * Vector3.forward;
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
        _MODELOFFSET.transform.rotation = Quaternion.LookRotation(avgLookUpRotation, avgLookAtRotation);
        gameObject.transform.position = avgCenter;
    }
}
