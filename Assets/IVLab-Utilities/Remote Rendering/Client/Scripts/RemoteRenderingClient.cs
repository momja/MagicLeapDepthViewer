using IVLab.Utilities;
using IVLab.Utilities.RemoteRendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;



namespace IVLab.Utilities.RemoteRendering
{
    [System.Serializable]

    public class RemoteRenderingClientView
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public bool enabled;

        [SerializeField]
        public int millisecondDelay;

        [SerializeField]
        public Camera camera;

        [SerializeField]
        public Transform cameraTransform;
        [SerializeField]
        string[] layers;

        RemoteRenderingClient client;

        public void Start(RemoteRenderingClient client)
        {
            this.client = client;
            ConnectToServer();

        }
        public TcpClient TcpClient { get; set; }

        GameObject depthMeshObject;
        public bool Running { get; set; } = true;

        public bool Established { get; private set; } = false;
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> depthUVs = new List<Vector2>();
        bool busy = false;
        CancellationTokenSource source;
        private long timeToRunUnityCode;
        bool waitingForResponse = false;
        //[SerializeField]
        //bool AutoRequestNextFrame = true;
        private long timeToUnpack;
        private long timeToMakeMesh;
        Texture2D image = null;
        [SerializeField]
        UnityEngine.UI.Text timingOutput;
        void ConnectToServer()
        {
            //clientReceiveThread = new Thread(new ThreadStart(Client));
            //clientReceiveThread.IsBackground = true;
            //clientReceiveThread.Start();
            Task.Run(async () =>
            {

                while (Running)
                {
                    TcpClient = null;
                    busy = true;
                    await Client();
                    Debug.Log("Ended client routine");
                    busy = false;

                }
            //CleanUp();
        });

        }

        async Task Client()
        {
            source = new CancellationTokenSource();
            CancellationToken tkn = source.Token;
            waitingForResponse = false;
            Established = false;
            try
            {
                while (TcpClient == null && !source.IsCancellationRequested)
                {
                    Debug.Log("Trying to connect to server...");
                    try
                    {
                        TcpClient = new TcpClient(client.ipAddress, client.port);
                        TcpClient.Client.ReceiveTimeout = 10000;
                        TcpClient.Client.NoDelay = false;
                    }
                    catch (SocketException socketException)
                    {
                        Debug.Log("Socket exception: " + socketException);
                    }
                    if (TcpClient == null)
                    {
                        Debug.Log("Failed to connect to server.");
                        await Task.Delay(1000);
                    }
                }
                if (source.IsCancellationRequested || TcpClient == null)
                {
                    return;
                }
                Debug.Log("Connected to server: " + TcpClient.Client.RemoteEndPoint.ToString());
                MessageType messageType = (MessageType)await StreamMethods.ReadIntFromStreamAsync(TcpClient.GetStream(), tkn);


                if (messageType != MessageType.ConnectionEstablished)
                {
                    Debug.LogError("Did not receive Connection Established. Aborting.");
                    Running = false;
                }


                Debug.Log("Writing \"StringData\" ");

                await StreamMethods.WriteIntToStreamAsync(TcpClient.GetStream(), (int)MessageType.StringData, tkn);
                Debug.Log("Writing label");

                await StreamMethods.WriteStringToStreamAsync(TcpClient.GetStream(), name, tkn);
                Established = true;
                while (Running && TcpClient.Connected)
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
                        Debug.Log("Lost connection to server");
                        TcpClient.Close();
                        return;
                    }
                    else if (messageType == MessageType.ObjectData)
                    {
                        requestAndResponse.Stop();
                        var unpack = System.Diagnostics.Stopwatch.StartNew();

                        var obj = await StreamMethods.ReadObjectFromStreamAsync(TcpClient.GetStream(), tkn);
                        var cameraInfo = (CameraInfo)obj;

                        // Expect a texture next
                        messageType = (MessageType)await StreamMethods.ReadIntFromStreamAsync(TcpClient.GetStream(), tkn);


                        byte[] data = await StreamMethods.ReadBytesFromStreamAsync(TcpClient.GetStream(), tkn);
                        byte[] decompressedImageData = null;
                        if (cameraInfo.compressImage)
                        {
                            try
                            {
                                decompressedImageData = ByteCompression.Decompress(data);
                            }
                            catch (System.ArgumentException e)
                            {
                                Debug.Log("Failed to unzip data.");
                            }

                        }
                        else
                        {
                            decompressedImageData = data;
                        }

                        unpack.Stop();
                        var makeMesh = System.Diagnostics.Stopwatch.StartNew();

                        DepthMesh.MakeDepthMesh(decompressedImageData, cameraInfo.width, cameraInfo.height, cameraInfo.FarPlane, client.gridDepthThreshold, client.maxDepth, client.gridDimension, client.gridDimension, ref vertices, ref uvs, ref depthUVs, ref indices);
                        makeMesh.Stop();


                        //var verts = ByteCompression.Compress(ByteMethods.ObjectToByteArray(Vector3ToFloats(vertices.ToArray())));
                        //var inds = ByteCompression.Compress(ByteMethods.ObjectToByteArray(indices));
                        //var us = ByteCompression.Compress(ByteMethods.ObjectToByteArray(Vector2ToFloats(uvs.ToArray())));
                        //Debug.Log("Uncompressed mesh: " + vertices.Count * 3 * 4 / 1000.0 + "KB" + " + " + indices.Count * 4 / 1000.0 + "KB" + " + " + uvs.Count * 2 * 4 / 1000.0 + "KB" + " = " + (vertices.Count * 3 * 4 + indices.Count * 4 + uvs.Count * 2 * 4) / 1000.0 + "KB");

                        //Debug.Log("Compressed mesh: " + verts.Length / 1000.0 + "KB" + " + " + inds.Length / 1000.0 + "KB" + " + " + us.Length / 1000.0 + "KB" + " = " + (verts.Length + inds.Length + us.Length) / 1000.0 + "KB");
                        var scheduleUnity = System.Diagnostics.Stopwatch.StartNew();

                        if (decompressedImageData != null)
                            await UnityThreadScheduler.Instance.RunMainThreadWork(() =>
                            {
                                scheduleUnity.Stop();
                                var buildUnityObjects = System.Diagnostics.Stopwatch.StartNew();
                                if (depthMeshObject == null)
                                {
                                    depthMeshObject = new GameObject(name + " depth mesh");

                                    depthMeshObject.transform.SetParent(client.transform, worldPositionStays: true);
                                    depthMeshObject.transform.localPosition = Vector3.zero;
                                    depthMeshObject.transform.localScale = Vector3.one;
                                    depthMeshObject.AddComponent<MeshFilter>();
                                    depthMeshObject.AddComponent<MeshRenderer>().material = client.displacementMaterial;
                                }
                                Mesh mesh = depthMeshObject.GetComponent<MeshFilter>().mesh;
                                if (mesh == null)
                                    mesh = new Mesh();

                                mesh.Clear();
                                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                                Debug.Log(vertices.Count + ", " + uvs.Count + ", " + depthUVs.Count + ", " + indices.Count);

                                mesh.SetVertices(vertices);
                                mesh.SetUVs(0, uvs);
                                mesh.SetUVs(1, depthUVs);
                                mesh.SetIndices(indices, MeshTopology.Quads, 0);
                                mesh.UploadMeshData(false);
                                mesh.RecalculateBounds();
                                Debug.Log(mesh.bounds);
                                depthMeshObject.GetComponent<MeshFilter>().mesh = mesh;

                                depthMeshObject.transform.localRotation = cameraInfo.Rotation;
                                depthMeshObject.transform.localPosition = cameraInfo.Position + cameraInfo.Rotation * (Vector3.forward * cameraInfo.FarPlane);
                                float FOV = cameraInfo.FOV;

                                float Z = cameraInfo.FarPlane;
                                float D = Mathf.Abs(Mathf.Tan(Mathf.Deg2Rad * FOV / 2) * Z);
                                var scale = new Vector3(D / cameraInfo.height * cameraInfo.width * 2, D * 2, 0); ;
                                scale.z = 1;
                                depthMeshObject.transform.localScale = scale;
                                if (image == null || image.width != cameraInfo.width || image.height != cameraInfo.height)
                                {
                                    if (image != null)
                                        Texture.Destroy(image);
                                    image = new Texture2D(cameraInfo.width, cameraInfo.height, TextureFormat.RGBA32, false);

                                }


                                image.LoadRawTextureData(decompressedImageData);
                                image.filterMode = FilterMode.Point;
                                image.Apply();
                            //imageElement?.texture ?= image; 
                            depthMeshObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", image);
                                depthMeshObject.GetComponent<MeshRenderer>().material.SetFloat("_NearPlane", cameraInfo.NearPlane);
                                depthMeshObject.GetComponent<MeshRenderer>().material.SetFloat("_FarPlane", cameraInfo.FarPlane);
                                depthMeshObject.GetComponent<MeshRenderer>().material.SetFloat("_GradThresh", client.gridDepthThreshold / 255.0f);
                                depthMeshObject.GetComponent<MeshRenderer>().material.SetFloat("_MaxDepth", client.maxDepth / 255.0f);

                            //imageGrid.GetComponent<MeshRenderer>().material.SetFloat("_FarPlane", cameraInfo.FarPlane);


                            buildUnityObjects.Stop();
                                stopwatch.Stop();

                                if (timingOutput != null)
                                {
                                    timingOutput.text = "Request & Response: " + requestAndResponse.ElapsedMilliseconds + "\n" +
                                        "Time to unpack message: " + unpack.ElapsedMilliseconds + " ms" + "\n" +
                                        "Time to make mesh: " + makeMesh.ElapsedMilliseconds + " ms" + "\n" +
                                        "Time to schedule on Unity Thread: " + scheduleUnity.ElapsedMilliseconds + " ms" + "\n" +
                                        "Time for Unity thread work: " + buildUnityObjects.ElapsedMilliseconds + " ms" + "\n" +
                                        "Total Time: " + stopwatch.ElapsedMilliseconds + " ms" 
                                        //" or " +
                                        //(requestAndResponse.ElapsedMilliseconds + unpack.ElapsedMilliseconds + makeMesh.ElapsedMilliseconds + scheduleUnity.ElapsedMilliseconds + buildUnityObjects.ElapsedMilliseconds);
                                        ;

                                }

                            });



                        waitingForResponse = false;
                    }
                }



                //Debug.Log("Writing \"StringData\" ");

                //StreamMethods.WriteIntToStream(TcpClient.GetStream(), (int)MessageType.StringData);
                //Debug.Log("Writing label");

                //StreamMethods.WriteStringToStream(TcpClient.GetStream(), viewLabel);

                //await Task.Delay(1000);
                //Debug.LogError("Writing Connection Closed");

                //StreamMethods.WriteIntToStream(TcpClient.GetStream(), (int)MessageType.ConnectionClosed);



            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }


        }
        public System.Diagnostics.Stopwatch requestAndResponse;
        public System.Diagnostics.Stopwatch stopwatch;

        long timeTilResponse;

        [FunctionDebugger]
        public void SendCameraInfo()
        {

            if (TcpClient?.Connected ?? false)
            {
                stopwatch = System.Diagnostics.Stopwatch.StartNew();
                requestAndResponse = System.Diagnostics.Stopwatch.StartNew();
                waitingForResponse = true;

                StreamMethods.WriteIntToStream(TcpClient.GetStream(), (int)MessageType.ObjectData);
                Transform t = cameraTransform;


                CameraInfo cameraInfo = new CameraInfo();
                Vector3 relativePosition = client.transform.InverseTransformPoint(t.position);
                Quaternion relativeRotation = Quaternion.Inverse(client.transform.rotation) * t.rotation;
                cameraInfo.PosX = relativePosition.x;
                cameraInfo.PosY = relativePosition.y;
                cameraInfo.PosZ = relativePosition.z;
                cameraInfo.RotX = relativeRotation.x;
                cameraInfo.RotY = relativeRotation.y;
                cameraInfo.RotZ = relativeRotation.z;
                cameraInfo.RotW = relativeRotation.w;
                cameraInfo.FOV = (client.FOVOverride > 0 ? client.FOVOverride : camera.fieldOfView) * client.FOVMultiplier;
                cameraInfo.NearPlane = client.nearPlaneOverride > 0 ? client.nearPlaneOverride : camera.nearClipPlane;// camera.nearClipPlane;
                cameraInfo.FarPlane = client.farPlaneOverride > 0 ? client.farPlaneOverride : camera.farClipPlane;// camera.farClipPlane;
                cameraInfo.height = (int)((client.heightOverride > 0 ? client.heightOverride : camera.pixelHeight) * client.heightMultiplier);
                cameraInfo.width = (int)((client.widthOverride > 0 ? client.widthOverride : camera.pixelWidth) * client.widthMultiplier);
                cameraInfo.compressImage = client.compressImage;
                cameraInfo.layers = layers;
                try
                {
                    StreamMethods.WriteObjectToStream(TcpClient.GetStream(), cameraInfo);
                }
                catch (SocketException e)
                {
                    Debug.Log("Server aborted the connection.");
                    source.Cancel();
                }



            }
        }
        
        System.Diagnostics.Stopwatch delayTimer;

        // Update is called once per frame
        public void Update()
        {
            if( delayTimer == null)
            {
                delayTimer = System.Diagnostics.Stopwatch.StartNew();
            }

            if(delayTimer.IsRunning && delayTimer.ElapsedMilliseconds < millisecondDelay)
            {
                return;
            } else
            {
                delayTimer.Stop();
            }

            if (client.AutoRequestNextFrame && !waitingForResponse && (TcpClient?.Connected ?? false) && Established )
            {
                SendCameraInfo();
            }
        }

        public void OnDestroy()
        {
            //if(stream != null)
            //    StreamMethods.WriteIntToStream(stream, (int)MessageType.ConnectionClosed);
            //_tcpClient.Close();
            //clientReceiveThread.Abort();
            Debug.Log("Setting Running to false");
            Running = false;

            source?.Cancel();
            //Task.Run(() => { 
            //    while (busy) Task.Delay(100); 
            //}).Wait();
            if (TcpClient != null)
            {
                StreamMethods.WriteIntToStream(TcpClient.GetStream(), (int)MessageType.ConnectionClosed);

                TcpClient = null;
                //TcpClient.Close();
            }



        }

    }
    public class RemoteRenderingClient : MonoBehaviour
    {

        [SerializeField]
        public int port;
        [SerializeField]
        public string ipAddress;

        [SerializeField]
        public int widthOverride = -1;

        [SerializeField]
        public float widthMultiplier = 1;
        [SerializeField]
        public int heightOverride = -1;
        [SerializeField]
        public float heightMultiplier = 1;
        [SerializeField]
        public float nearPlaneOverride = -1;

        [SerializeField]
        public float farPlaneOverride = -1;

        [SerializeField]
        public float FOVOverride = -1;

        [SerializeField]
        public float FOVMultiplier = 1;

        [SerializeField]
        public int gridDimension = 64;

        [SerializeField]
        public int gridDepthThreshold = 20;

        [SerializeField]
        public int maxDepth = 254;

        [SerializeField]
        public bool compressImage = true;

        [SerializeField]
        public bool AutoRequestNextFrame = true;
        [SerializeField]
        public Material displacementMaterial;
        [SerializeField]
        List<RemoteRenderingClientView> views;

        // Start is called before the first frame update
        void Start()
        {
            UnityThreadScheduler.GetInstance();
            foreach (var view in views)
            {
                view.Start(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var view in views)
            {
                view.Update();
            }
        }

        private void OnDestroy()
        {
            foreach (var view in views)
            {
                view.OnDestroy();
            }
        }
    }

}