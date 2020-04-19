using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MagicLeap;
using System;

public class CaptureScreenshot : MonoBehaviour
{
    #region Private Variables
    private MLInputController _controller;
    private bool bumperDown = false;
    #endregion

    #region Unity Methods
    void Start()
    {
        //Start receiving input by the Control
        MLInput.Start();
        _controller = MLInput.GetController(MLInput.Hand.Left);
    }
    void OnDestroy()
    {
        //Stop receiving input by the Control
        MLInput.Stop();
    }

    private void OnPostRender()
    {
        if (_controller.IsBumperDown)
        {
            if (!bumperDown)
            {
                bumperDown = true;
                print("Capturing Screenshot");
                ScreenCapture.CaptureScreenshot("documents/C2/vuforia_demo.jpg");
            }
        }
        else
        {
            bumperDown = false;
        }
    }
    #endregion
}