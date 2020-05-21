using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class MLControllerAnnotationTracker : MonoBehaviour {
  #region Private Variables
  private MLInput.Controller _controller;
  private AnnotationSource _annotation;
  #endregion
  
  #region Unity Methods
  void Start () {
    //Start receiving input by the Control
    MLInput.Start();
    MLInput.OnControllerButtonDown += OnButtonDown;
    MLInput.OnControllerButtonUp += OnButtonUp;
    _controller = MLInput.GetController(MLInput.Hand.Left);
  }
  void OnDestroy () {
    //Stop receiving input by the Control
    MLInput.OnControllerButtonDown -= OnButtonDown;
    MLInput.OnControllerButtonUp -= OnButtonUp;
    MLInput.Stop();
  }
  void OnButtonDown(byte controllerId, MLInput.Controller.Button button) {
      if (button == MLInput.Controller.Button.Bumper) {
          _annotation.annotation.active = true;
      }
  }
  void OnButtonUp(byte controllerId, MLInput.Controller.Button button) {
    if (button == MLInput.Controller.Button.Bumper) {
          _annotation.annotation.active = false;      
    }
  }
  void Update () {
    //Attach the Beam GameObject to the Control
    transform.position = _controller.Position;
    transform.rotation = _controller.Orientation;
  }
  #endregion
}