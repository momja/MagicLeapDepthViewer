using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities.RemoteRendering
{
    [System.Serializable]
    public class CameraInfo
    {
        //public DateTime RequestTime;
        //public DateTime CaptureTime;
        public Vector3 Position { get { return new Vector3(PosX, PosY, PosZ); } }
        public Quaternion Rotation { get { return new Quaternion(RotX, RotY, RotZ, RotW); } }
        public float PosX;
        public float PosY;
        public float PosZ;
        public float RotX;
        public float RotY;
        public float RotZ;
        public float RotW;
        public float FOV;
        public float FarPlane;
        public float NearPlane;
        public int width;
        public int height;
        public bool compressImage;
        public string[] layers;
    }
}
