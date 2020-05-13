using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
public class WorldHeadLockCanvas : MonoBehaviour
{

    #region Public Variables
    public GameObject Camera;
    #endregion
        static float DISTANCE = 2.5f;
        static float SPEED = 5.0f;
        void Update()
        {
            var step = SPEED * Time.deltaTime;
            var position = Camera.transform.position + Camera.transform.forward * DISTANCE;
            Quaternion rotation = Quaternion.LookRotation(transform.position - Camera.transform.position);
            transform.position = Vector3.SlerpUnclamped(transform.position, position, step);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, step);
        }
}