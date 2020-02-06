using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.XR.MagicLeap;

public class getCenterpoint : MonoBehaviour
{
    private Vector3 previousPosition;
    private GameObject _vuforiaTracker;
    // private MLInputController _controller;

    private GameObject bathymmetry;
    private GameObject isosurface_data;
    
    void Start()
    {
        // Initialize the anchor point
        _vuforiaTracker = GameObject.Find("ImageTarget");

        bathymmetry = transform.GetChild(1).gameObject;
        isosurface_data = transform.GetChild(0).gameObject;

        // Setup Magic Leap controller
        // MLInput.Start();
        // MLInput.OnControllerButtonDown += OnButtonDown;
        // MLInput.OnControllerButtonUp += OnButtonUp;
        // _controller = MLInput.GetController(MLInput.Hand.Left);
    }

    void OnDestroy()
    {
        // MLInput.OnControllerButtonDown -= OnButtonDown;
        // MLInput.OnControllerButtonUp -= OnButtonUp;
        // MLInput.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(previousPosition, transform.position) > 0.05) {
            print("Jittering");
        }

        previousPosition = transform.position;
    }

    void VuforiaTargetLost() {
        print("Vuforia target lost.");
        print("Enabling Magic Leap Tracking");
        // bathymmetry.SetActive(false);
        isosurface_data.SetActive(false);
    }

    void VuforiaTargetFound() {
        print("Vuforia target found.");
        print("Disabling Magic Leap Tracking");
        // bathymmetry.SetActive(true);
        isosurface_data.SetActive(true);
    }
}
