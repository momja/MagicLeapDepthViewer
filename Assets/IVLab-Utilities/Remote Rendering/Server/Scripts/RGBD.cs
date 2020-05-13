﻿using UnityEngine;

namespace IVLab.Utilities.RemoteRendering
{
	//behaviour which should lie on the same gameobject as the main camera
	public class RGBD : MonoBehaviour
	{
		//material that's applied when doing postprocessing
		[SerializeField]
		private Material postprocessMaterial;

		private void Start()
		{
			//get the camera and tell it to render a depth texture
			Camera cam = GetComponent<Camera>();
			cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
		}


		//method which is automatically called by unity after the camera is done rendering
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			//draws the pixels from the source texture to the destination texture
			Graphics.Blit(source, destination, postprocessMaterial);
		}
	}
}
