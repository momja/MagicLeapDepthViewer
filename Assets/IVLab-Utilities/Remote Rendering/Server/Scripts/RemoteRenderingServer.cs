using IVLab.Utilities;
using IVLab.Utilities.RemoteRendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IVLab.Utilities.RemoteRendering
{
	public class RemoteRenderingServer : MonoBehaviour
	{
		private Thread tcpListenerThread;
		private TcpListener tcpListener;

		[SerializeField]
		private int port;

		[SerializeField]
		private GameObject serverCameraPrefab;

		// Start is called before the first frame update
		void Start()
		{
			var UTS = IVLab.Utilities.UnityThreadScheduler.Instance;
			// Start TcpServer background thread 		
			tcpListenerThread = new Thread(new ThreadStart(ListenForNewClients));
			tcpListenerThread.IsBackground = true;
			tcpListenerThread.Start();
		}

		// Update is called once per frame
		void Update()
		{

		}

		private void OnDestroy()
		{
			tcpListener.Stop();
			tcpListener = null;
			Running = false;
			tcpListenerThread.Abort();
		}
		bool Running { get; set; } = true;
		private void ListenForNewClients()
		{
			while (true)
			{
				try
				{
					// Create listener on localhost port 8052. 			
					tcpListener = new TcpListener(IPAddress.Any, port);
					tcpListener.Start();
					Debug.Log("Server is listening");
					Byte[] bytes = new Byte[1024];
					bool stop = false;
					while (Running)
					{
						Debug.Log("Waiting for a new client");
						var connectedTcpClient = tcpListener.AcceptTcpClient();
						{

							Debug.Log("Connected a new client");
							UnityThreadScheduler.Instance.RunMainThreadWork(() =>
							{
								var newServerCamera = Instantiate(serverCameraPrefab, transform, false);

								ServerCamera serverCamera = newServerCamera.GetComponent<ServerCamera>();
								serverCamera.TcpClient = connectedTcpClient;
								serverCamera.StartListening();

							}).Wait();


						}
					}
				}
				catch (SocketException socketException)
				{
					Debug.Log("SocketException " + socketException.ToString());
				}
				Debug.Log("Client disconnected, waiting for next client");
			}

		}
	}
}