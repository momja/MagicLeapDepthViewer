/* Author: Bridger Herman
 * Email: herma582@umn.edu
 * Copyright (c) 2019, University of Minnesota
 *  
 * Moves the camera in screen space. Attach this script to a GameObject which
 * is a parent of the Main Camera.
 *  
 * Useful for counteracting the screen space offset for video and screenshot
 * capture on the Magic Leap.
 */

 using UnityEngine;

 public class ScreenSpaceOffset : MonoBehaviour
{
    public float xOffset = 0.0f;
    public float yOffset = 0.0f;

    public float scaleFactor = 1.0f;

    public bool offsetActive = false;
    public Transform cameraTransform;

    // Update is called once per frame
    void Update()
    {
        if (offsetActive) {
            this.transform.localPosition = this.xOffset * this.cameraTransform.right + this.yOffset * this.cameraTransform.up;
            this.transform.localScale = new Vector3(this.scaleFactor, this.scaleFactor, this.scaleFactor);
        } else {
            this.transform.localPosition = Vector3.zero;
            this.transform.localScale = Vector3.one;
        }
    }
}
