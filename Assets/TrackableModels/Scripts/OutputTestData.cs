using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.MagicLeap;
using Vuforia;
using TMPro;
using System.Linq;

public class OutputTestData : MonoBehaviour
{

    public TextMeshPro sceneModeTextBox;
    public TextMeshPro enabledTrackerCount;
    public int activeTrackerCount;

    #region Private Variables
    private MLInput.Controller _controller;
    private bool _enabled = false;
    private bool _bumper = false;
    private bool _trigger = false;
    private int trackerCount;
    TrackerObject[] trackers;
    #endregion

    void Start()
    {
        //Start receiving input by the Control
        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;
        _controller = MLInput.GetController(MLInput.Hand.Left);
        trackers = GameObject.Find("[TARGETS]").GetComponentsInChildren<TrackerObject>(true);
        trackers = trackers.OrderBy(component => component.gameObject.name).ToArray();
        trackerCount = trackers.Length;
        TrackerObject.trackerObjects = new List<TrackerObject>();
        for (int i = 0; i < activeTrackerCount; i++)
        {
            trackers[i].gameObject.SetActive(true);
            TrackerObject.trackerObjects.Add(trackers[i]);
        }
        for (int i = activeTrackerCount; i < trackerCount; i++)
        {
            // trackers[i].gameObject.SetActive(false);
        }
        sceneModeTextBox.text = activeTrackerCount.ToString();
        VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, activeTrackerCount);
    }

    void OnDestroy()
    {
        //Stop receiving input by the Control
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
    }

    void Update()
    {
        if (_bumper && _enabled)
        {
            print("Bumper pressed");
            _enabled = false;
            if (activeTrackerCount < trackerCount)
            {
                activeTrackerCount++;
            }
            else
            {
                // reset the active trackers
                activeTrackerCount = 1;
            }
            TrackerObject.trackerObjects = new List<TrackerObject>();
            for (int i = 0; i < activeTrackerCount; i++)
            {
                trackers[i].gameObject.SetActive(true);
                TrackerObject.trackerObjects.Add(trackers[i]);
            }
            for (int i = activeTrackerCount; i < trackerCount; i++)
            {
                // trackers[i].gameObject.SetActive(false);
            }
            sceneModeTextBox.text = activeTrackerCount.ToString();
            VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, activeTrackerCount);
            print($"MAX_TARGETS: {activeTrackerCount}");
        }

        int visibleTargets = TrackerObject.getVisibleTrackerCount();
        enabledTrackerCount.text = visibleTargets.ToString();

        if (_trigger && _enabled)
        {
            print("Trigger pressed");
            _enabled = false;
            // if (activeTrackerCount == visibleTargets) {
            Vector3 pos = TargetSettings.Instance.model.transform.position;
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
                print($"POSITION: {pos.ToString("F10")}");
            }
        }

    }

    void OnControllerButtonDown()
    {

    }

    void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
    {
        if ((button == MLInput.Controller.Button.Bumper))
        {
            print("Bumper Down");
            _bumper = true;
        }
        if ((button == MLInput.Controller.Button.HomeTap))
        {
            print("Home down");
            _trigger = true;
        }
        _enabled = true;
    }

    void OnButtonUp(byte controllerId, MLInput.Controller.Button button)
    {
        // print("Button up");
        // _bumper = false;
        // _trigger = false;
        // _enabled = true;
    }
}
