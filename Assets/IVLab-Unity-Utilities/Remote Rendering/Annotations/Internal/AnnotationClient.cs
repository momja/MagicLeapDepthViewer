using IVLab.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;


public class AnnotationClient : MonoBehaviour
{

    public string address;
    public int port;
    private TcpClient tcpClient;

	public Guid guid;
	public Color color;
	public AnnotationSource annotationSource;

	public GameObject annotationAvatarPrefab;
    // Start is called before the first frame update
    void OnEnable()
    {
		guid = Guid.NewGuid() ;
        tcpClient = new TcpClient();

		if (address == "localhost") address = "127.0.0.1";


		tcpClient.BeginConnect(address, port, new AsyncCallback(DoConnectTcpClientCallback),  this);


    }

	Dictionary<string, Annotation> annotations = new Dictionary<string, Annotation>();

	public static void DoConnectTcpClientCallback(IAsyncResult ar)
	{
		Debug.Log("ON CONNECT");

		AnnotationClient annotationClient;
		 annotationClient = ( AnnotationClient ) ar.AsyncState ;

		bool failed = false;
		try
		{

			annotationClient.tcpClient.EndConnect(ar);
		}catch(Exception e)
		{
			Debug.LogError(e);
			failed = true;
		}

		if(failed == false)
		{
			NetworkStream stream = annotationClient.tcpClient.GetStream();
			Debug.Log("Connected to Annotation Server");
			if (stream.CanWrite)
			{
				Debug.Log("Establishing...");
				try
				{
					StreamMethods.WriteIntToStream(stream, (int)AnnotationMessageTypes.ESTABLISH);
					StreamMethods.WriteStringToStream(stream, annotationClient.guid.ToString());
					StreamMethods.WriteObjectToStream(stream, new Annotation(Vector3.one, annotationClient.color, 1, false) );


					Task.Run(() =>
					{
						while(true)
						{
							AnnotationMessageTypes type = (AnnotationMessageTypes) StreamMethods.ReadIntFromStream(stream);

							if(type == AnnotationMessageTypes.CLOSE)
							{
								break;
							} else
							{
								int count = StreamMethods.ReadIntFromStream(stream);

								lock (annotationClient.annotations)
								{
									annotationClient.annotations.Clear();

									for (int i = 0; i < count; i++)
									{
										string guid = StreamMethods.ReadStringFromStream(stream);
										Annotation a = (Annotation)StreamMethods.ReadObjectFromStream(stream);
										annotationClient.annotations[guid] = a;
									}
								}


							}
						}
					});
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}

				annotationClient.established = true;


			}  
			Debug.Log("Client connected");
		} else
		{
			annotationClient.tcpClient.BeginConnect(annotationClient.address, annotationClient. port, new AsyncCallback(DoConnectTcpClientCallback), annotationClient.tcpClient);

		}

	}
	public bool annotationActive = true;
	public bool established = false;
	// Update is called once per frame
	void Update()
    {
		if (established == false) return; ;
		if(tcpClient.Connected)
		{
			StreamMethods.WriteIntToStream(tcpClient.GetStream(), (int)AnnotationMessageTypes.UPDATE);
			if(annotationSource != null)
			StreamMethods.WriteObjectToStream(tcpClient.GetStream(), new Annotation(transform.InverseTransformPoint(annotationSource.annotation.Position), annotationSource.annotation.Color, annotationSource.annotation.size / transform.lossyScale.x, annotationSource.annotation.active));
			else StreamMethods.WriteObjectToStream(tcpClient.GetStream(), new Annotation(Vector3.down, Color.black, 0, false));


		}

		var avatars = GetComponentsInChildren<AnnotationAvatar>();

		lock(annotations)
		{
			foreach(var avatar in avatars)
			{
				if(!annotations.ContainsKey(avatar.name))
				{
					avatar.stop = true;
				}
			}

			foreach(var guid in annotations.Keys)
			{
				var trans = transform.Find(guid);
				if(trans == null)
				{
					var go = GameObject.Instantiate(annotationAvatarPrefab);
					go.name = guid;
					go.transform.SetParent(transform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
					trans = go.transform;
				}

				if(trans)
				trans.GetComponent<AnnotationAvatar>().annotation = annotations[guid];
			}
		}





	}

	private void OnDisable()
	{
		StreamMethods.WriteIntToStream(tcpClient.GetStream(), (int)AnnotationMessageTypes.CLOSE);
	}

}
