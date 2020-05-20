using IVLab.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class AnnotationServer : MonoBehaviour
{

    public int port;

    public TcpListener tcpListener;

	public GameObject AnnotationPrefab;

    // Start is called before the first frame update
    void Start()
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
		Debug.Log("Waiting for new annotation Client");
		tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), this);
    }


	public static void DoAcceptTcpClientCallback(IAsyncResult ar)
	{


		// Get the listener that handles the client request.
		var server = (AnnotationServer)ar.AsyncState;
		TcpListener listener = server.tcpListener;
		// End the operation and display the received data on  
		// the console.
		var client = listener.EndAcceptTcpClient(ar);
		NetworkStream stream = client.GetStream();

		if (stream.CanRead)
		{
			AnnotationMessageTypes type = (AnnotationMessageTypes)StreamMethods.ReadIntFromStream(stream);
			if (type != AnnotationMessageTypes.ESTABLISH) Debug.LogError("WRONG TYPE");

			var str = StreamMethods.ReadStringFromStream(stream);
			Debug.Log("Received new annotation client: " + str);
			try { 
				Annotation annotation = (Annotation)StreamMethods.ReadObjectFromStream(stream);

				Task.Run(() => UnityThreadScheduler.Instance.RunMainThreadWork(() =>
				{
					GameObject go = GameObject.Instantiate(server.AnnotationPrefab);
					go.name = str;
					var color = annotation.Color;
					color.a = 0.5f ;
					go.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
					go.GetComponent<AnnotationConnection>().tcpClient = client;
					go.GetComponent<AnnotationConnection>().Init();
					go.transform.SetParent(server.transform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localScale = Vector3.one;

				}).Wait());
			}
			catch (Exception e)
			{
				
				Debug.LogError(e);
			}

		}
		Debug.Log("Client connected");

		// Signal the calling thread to continue.

		listener.BeginAcceptTcpClient(
			new AsyncCallback(DoAcceptTcpClientCallback), server);
	}

	// Update is called once per frame
	void Update()
    {
		var annotationConnections = GetComponentsInChildren<AnnotationConnection>();
		foreach(var ac in annotationConnections)
		{
			if(ac?.tcpClient?.Connected ?? false)
			{
				StreamMethods.WriteIntToStream(ac.tcpClient.GetStream(), (int)AnnotationMessageTypes.UPDATE);
				StreamMethods.WriteIntToStream(ac.tcpClient.GetStream(), annotationConnections.Length -1);
				foreach(var c in annotationConnections)
				{
					if (c != ac)
					{
						StreamMethods.WriteStringToStream(ac.tcpClient.GetStream(), c.gameObject.name);
						StreamMethods.WriteObjectToStream(ac.tcpClient.GetStream(), c.annotation);

					}
				}

			}
		}
    }

	private void OnDestroy()
	{
		var annotationConnections = GetComponentsInChildren<AnnotationConnection>();

		foreach (var ac in annotationConnections)
		{
			StreamMethods.WriteIntToStream(ac.tcpClient.GetStream(), (int)AnnotationMessageTypes.CLOSE);
		}
		tcpListener.Stop();
	}
}
