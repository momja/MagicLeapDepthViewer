/* OverrideNearClip.cs
 * Author: Seth Johnson
 * Email: joh08230@umn.edu
 * Copyright (c) 2019, University of Minnesota
 * 
 * Overrides the near clipping plane for the Magic Leap
 *
 * Place this script on your MagicLeap Main Camera Object
 * 
 * NOTE: Introduces one frame of latency.
 */

using UnityEngine;

public class OverrideNearClip : MonoBehaviour
{
    RenderTexture leftRenderTexture;
    RenderTexture rightRenderTexture;

    Camera leftCamera;
    Camera rightCamera;
    Camera masterCamera;

    Texture2DArray srcTextureArray;

    bool firstFrame = true;
    
    public float nearPlaneOverride = 0.001f;

    Matrix4x4 ChangeNearPlane(Matrix4x4 projectionMatrix)
    {
        Matrix4x4 M = projectionMatrix;
        var row3 = M.GetRow(2);
        float a = row3[2];
        float b = row3[3];
        float f = b / (a + 1);
        float n = b / (a - 1);
        n = this.nearPlaneOverride;
        a = (-(f + n) / (f - n));
        b = -2 * f * n / (f - n);
        row3[2] = a;
        row3[3] = b;
        M.SetRow(2, row3);
        return M;
    }

    void OnPreRender()
    {

        this.masterCamera = this.GetComponent<Camera>();
        if (leftCamera == null)
        {
            leftCamera = new GameObject("LeftCamera").AddComponent<Camera>();
            leftCamera.CopyFrom(masterCamera);
            leftCamera.stereoTargetEye = StereoTargetEyeMask.None;
        }

        if (rightCamera == null)
        {
            rightCamera = new GameObject("RightCamera").AddComponent<Camera>();
            rightCamera.CopyFrom(masterCamera);
            rightCamera.stereoTargetEye = StereoTargetEyeMask.None;
        }


        leftCamera.worldToCameraMatrix = masterCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
        rightCamera.worldToCameraMatrix = masterCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

        leftCamera.projectionMatrix = ChangeNearPlane(masterCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
        rightCamera.projectionMatrix = ChangeNearPlane(masterCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));

    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Single Pass Instanced
        if (src.dimension == UnityEngine.Rendering.TextureDimension.Tex2DArray)
        {
            // Initialize on the first frame
            if (firstFrame)
            {
                leftRenderTexture = new RenderTexture(src.width, src.height, 24, src.graphicsFormat);
                rightRenderTexture = new RenderTexture(src.width, src.height, 24, src.graphicsFormat);

                srcTextureArray = new Texture2DArray(src.width, src.height, 2, src.graphicsFormat, 0);

                leftCamera.targetTexture = leftRenderTexture;
                rightCamera.targetTexture = rightRenderTexture;
                firstFrame = false;
            }

            // Put the left and right eyes onto the target texture volume
            Graphics.CopyTexture(leftCamera.activeTexture, 0, srcTextureArray, 0);
            Graphics.CopyTexture(rightCamera.activeTexture, 0, srcTextureArray, 1);

            Graphics.Blit(srcTextureArray, dest);
        }
        // Multi-Pass (called twice, once for each eye)
        else if (src.dimension == UnityEngine.Rendering.TextureDimension.Tex2D)
        {
            // Initialize on the first frame
            if (firstFrame)
            {
                leftRenderTexture = new RenderTexture(src.width, src.height, 24, src.graphicsFormat);
                rightRenderTexture = new RenderTexture(src.width, src.height, 24, src.graphicsFormat);

                leftCamera.targetTexture = leftRenderTexture;
                rightCamera.targetTexture = rightRenderTexture;
                firstFrame = false;
            }
            // Left eye
            if (this.masterCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
            {
                Graphics.Blit(leftCamera.activeTexture, dest);
            }
            else // Right eye
            {
                Graphics.Blit(rightCamera.activeTexture, dest);
            }
        }

    }

}