using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IVLab.Utilities.RemoteRendering
{



    public class ServerCamera : MonoBehaviour
    {



        //private TcpClient _tcpClient;

        public TcpClient TcpClient { get; set; }

        private System.Threading.Thread tcpListenerThread;

        private NetworkStream stream;

        [SerializeField]
        string viewLabel;

        public void StartListening()
        {
            //tcpListenerThread = new Thread(new ThreadStart(Server));
            //tcpListenerThread.IsBackground = true;
            //tcpListenerThread.Start();
            Task.Run(async () => {
                await Server();
            });
        }

        Camera _camera = null;
        Camera Camera { get { if (_camera == null) _camera = GetComponent<Camera>(); return _camera; } }
        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
        }
        RenderTexture rt = null;
        Texture2D image = null;

        CancellationTokenSource source = new CancellationTokenSource();
        public bool Running { get; set; } = true;
        async Task Server()
        {
            CancellationToken tkn = source.Token;

            try
            {
                MessageType messageType;

                Debug.Log("Sending a connection established message");

                await StreamMethods.WriteIntToStreamAsync(TcpClient.GetStream(), (int)MessageType.ConnectionEstablished, tkn);
                Debug.Log("Now waiting for label");

                MessageType stringMessageType = (MessageType)await StreamMethods.ReadIntFromStreamAsync(TcpClient.GetStream(), tkn);
                if (stringMessageType != MessageType.StringData)
                {
                    Debug.LogError("Expected a " + MessageType.StringData + " message, got a " + stringMessageType + ", aborting.");
                    return;
                }

                viewLabel = await StreamMethods.ReadStringFromStreamAsync(TcpClient.GetStream(), tkn);

                Debug.Log("Connected with view named \"" + viewLabel + "\" at address " + TcpClient.Client.LocalEndPoint.ToString());

                while(Running)
                {
                    if (tkn.IsCancellationRequested) return;

                    // If no Data's avaialble, idle and then restart the loop
                    if (TcpClient.GetStream().DataAvailable == false)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    messageType = (MessageType)await StreamMethods.ReadIntFromStreamAsync(TcpClient.GetStream(), tkn);

                    if (messageType == MessageType.ConnectionClosed)
                    {
                        Debug.Log("Lost connection to  view \"" + viewLabel + "\" at address " + TcpClient.Client.RemoteEndPoint.ToString());
                        Running = false;
                        source.Cancel();
                        return;
                    } 
                    else if(messageType == MessageType.ObjectData)
                    {
                        Debug.Log("Receiving an object, probably a camera info object");

                        var obj = await StreamMethods.ReadObjectFromStreamAsync(TcpClient.GetStream(), tkn);
                        var cameraInfo = (CameraInfo)obj;
                        byte[] imageData = null;
                        await IVLab.Utilities.UnityThreadScheduler.Instance.RunMainThreadWork(() =>
                        {
                            transform.localPosition = new Vector3(cameraInfo.PosX, cameraInfo.PosY, cameraInfo.PosZ);
                            transform.localRotation = new Quaternion(cameraInfo.RotX, cameraInfo.RotY, cameraInfo.RotZ, cameraInfo.RotW);
                            Camera.fieldOfView = cameraInfo.FOV;
                            int width = cameraInfo.width;
                            int height = cameraInfo.height;
                            Camera.farClipPlane = cameraInfo.FarPlane;
                            Camera.nearClipPlane = cameraInfo.NearPlane;


                            if (rt == null || rt.width != width || rt.height != height)
                            {
                                if (rt != null) Destroy(rt);
                                rt = null;
                                rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

                            }
                            if (image == null || image.width != width || image.height != height)
                            {
                                if (image != null) Destroy(image);
                                image = null;
                                image = new Texture2D(width, height, TextureFormat.RGBA32, false);
                            }
                            Camera.targetTexture = rt;
                            if(cameraInfo.layers.Length == 0)
                            {
                                Camera.cullingMask = ~0;
                            } else
                            {
                                Camera.cullingMask = 0;
                                foreach(string layer in cameraInfo.layers)
                                {
                                    Camera.cullingMask |= 1 << LayerMask.NameToLayer(layer);
                                }
                            }
                            Camera.Render();
                            RenderTexture.active = rt;
                            image.ReadPixels(new Rect(0, 0, Camera.pixelWidth, Camera.pixelHeight), 0, 0);
                            image.Apply();
                            Camera.targetTexture = null;
                            RenderTexture.active = null;

                            Destroy(rt);


                            //cameraInfo.CaptureTime = DateTime.Now;

                            imageData = image.GetRawTextureData();


                        });


                        Debug.Log("Sending back the camera info");
                        await StreamMethods.WriteIntToStreamAsync(TcpClient.GetStream(), (int)MessageType.ObjectData, tkn);
                        await StreamMethods.WriteObjectToStreamAsync(TcpClient.GetStream(), cameraInfo,tkn);

                        Debug.Log("Sending the texture");
                        Debug.Log("Image size: " + cameraInfo.width + " x " + cameraInfo.height);

                        Debug.Log("image data (uncompressed): " + imageData.Length/1000.0 + "KB");



                        await StreamMethods.WriteIntToStreamAsync(TcpClient.GetStream(), (int)MessageType.TextureData, tkn);
                        if (cameraInfo.compressImage)
                        {
                            var compressed = ByteCompression.Compress(imageData);
                            Debug.Log("image data (compressed): " + compressed.Length / 1000.0 + "KB");
                            await StreamMethods.WriteBytestoStreamAsync(TcpClient.GetStream(), compressed, tkn);

                        } else
                        {
                            await StreamMethods.WriteBytestoStreamAsync(TcpClient.GetStream(), imageData, tkn);

                        }

                    }
                }


                //messageType = (MessageType)await StreamMethods.ReadIntFromStreamAsync(TcpClient.GetStream());
                //if (messageType != MessageType.StringData) Debug.LogError("Expected String, got " + messageType);
                //viewLabel = await StreamMethods.ReadStringFromStreamAsync(TcpClient.GetStream());

                //Debug.Log("Connected with view named \"" + viewLabel + "\" at address " + TcpClient.Client.RemoteEndPoint.ToString());

                //messageType = (MessageType)await StreamMethods.ReadIntFromStreamAsync(TcpClient.GetStream());

                //Task.Run(async () =>
                //{
                //    if (messageType == MessageType.ConnectionClosed)
                //    {
                //        Debug.Log("Disconnecting from view named \"" + viewLabel + "\" at address " + _tcpClient.Client.RemoteEndPoint.ToString());

                //    }
                //}).Wait();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }



            Debug.Log("Leaving server");
            //if (messageType == MessageType.ConnectionClosed)
            //{
            //    Debug.Log("Disconnected from view named \"" + viewLabel + "\" at address " + _tcpClient.Client.RemoteEndPoint.ToString());
            //    _tcpClient.Close();
            //    _tcpClient = null;
            //}

            
        }


        void StopServer()
        {

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(Running == false)
            {
                Debug.Log("Destroying ServerCamera for " + viewLabel);
                Destroy(gameObject);

            }
        }

        private void OnDestroy()
        {
            //if(_tcpClient != null)
            //{
            //    StreamMethods.WriteIntToStream(stream, (int)MessageType.ConnectionClosed);
            //    _tcpClient.Close();
            //}
            Debug.Log("Setting Running to false");
            Running = false;

            source.Cancel();
            if (TcpClient != null)
            {
                StreamMethods.WriteIntToStream(TcpClient.GetStream(), (int)MessageType.ConnectionClosed);
                TcpClient.Close();
                TcpClient = null;
            }
        }
    }

}
