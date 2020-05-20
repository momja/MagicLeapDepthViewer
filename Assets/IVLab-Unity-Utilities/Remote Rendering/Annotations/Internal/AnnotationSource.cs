using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationSource : MonoBehaviour
{

    public Color color;
    public Annotation annotation;
    // Start is called before the first frame update
    void Start()
    {
        color = Color.HSVToRGB(UnityEngine.Random.value, 1, 1);

        annotation = new Annotation(this.transform.position, color, transform.lossyScale.x, false);

    }

    // Update is called once per frame
    void Update()
    {
        annotation.Position = this.transform.position;
        annotation.size = transform.lossyScale.x;
        annotation.Color = color;
        
        if (annotation.active) 
            color.a = 0.5f;
        else 
            color.a = 0.1f;

        annotation.Color = color;

        GetComponent<MeshRenderer>().material.SetColor("_BaseColor", color);
        GetComponent<MeshRenderer>().material.SetColor("_Color", color);

        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color*color.a);
        GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
    }
}
     