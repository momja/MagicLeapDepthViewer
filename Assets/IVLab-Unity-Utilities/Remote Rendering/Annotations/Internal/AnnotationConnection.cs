using IVLab.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AnnotationConnection : MonoBehaviour
{
    public TcpClient tcpClient;
    public Annotation annotation;
    Task connectionTask;
    bool stop = false;
    void Handler()
    {
        while (true)
        {
            if (tcpClient.Connected)
                try
                {
                    AnnotationMessageTypes type = (AnnotationMessageTypes)StreamMethods.ReadIntFromStream(tcpClient.GetStream());
                    if (type == AnnotationMessageTypes.CLOSE)
                    {
                        stop = true;
                        return;
                    }
                    else
                    {
                        annotation = (Annotation)StreamMethods.ReadObjectFromStream(tcpClient.GetStream());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    stop = true;
                    return;
                }

        }
    }
    Thread thread;

    // Start is called before the first frame update
    public void Init()
    {
        annotation = new Annotation(Vector3.zero, Color.white, 1, false);
        thread = new Thread(new ThreadStart(Handler));
        thread.Start();

    
    }
    private void OnDisable()
    {
        thread.Abort();
    }
    // Update is called once per frame
    void Update()
    {
        if (!stop)
        {
            Color color = annotation.Color;
            if (annotation.active)
                color.a = 0.5f;
            else
                color.a = 0.0f;
            GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            transform.localPosition = annotation.Position;
            transform.localScale = annotation.size * Vector3.one;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
      
    }
}
