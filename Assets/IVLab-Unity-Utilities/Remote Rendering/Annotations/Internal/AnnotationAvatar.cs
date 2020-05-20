using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationAvatar : MonoBehaviour
{
    public Annotation annotation;
    public string ShaderColorName = "_BaseColor";

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public bool stop = false;
    // Update is called once per frame
    void Update()
    {
        if(annotation != null)
        {
            if (!stop)
            {
                Color color = annotation.Color;
                if (annotation.active)
                {
                    GetComponent<MeshRenderer>().enabled = true;
                    color.a = 0.5f;

                }
                else
                {
                    GetComponent<MeshRenderer>().enabled = false;
                    color.a = 0.0f;

                }
                GetComponent<MeshRenderer>().material.SetColor("_BaseColor", color);
                GetComponent<MeshRenderer>().material.SetColor("_Color", color);

                GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
                GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");

                transform.localPosition = annotation.Position;
                transform.localScale = annotation.size * Vector3.one;
            }
            else
            {
                GameObject.Destroy(this.gameObject);
            }
        }    
    }
}
