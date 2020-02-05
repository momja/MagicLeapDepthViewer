using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class getCenterpoint : MonoBehaviour
{
    GameObject ul;
    GameObject ll;
    GameObject ur;
    GameObject lr;
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialize the anchor points
        ul = GameObject.Find("upperleft");
        ll = GameObject.Find("lowerleft");
        ur = GameObject.Find("upperright");
        lr = GameObject.Find("lowerright");
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 ul_pos = ul.transform.position;
        Vector3 ll_pos = ll.transform.position;
        Vector3 ur_pos = ur.transform.position;
        Vector3 lr_pos = lr.transform.position;

        // Counterclockwise list of points making up the corners of our object.
        Vector3[] corner_coordinates = { ul_pos, ll_pos, lr_pos, ur_pos };

        Vector3 centerpoint = Vector3.zero;

        for (int i = 0; i < corner_coordinates.Length; i++) {
            centerpoint += corner_coordinates[i];
        }
        centerpoint = centerpoint / corner_coordinates.Length;

        print($"Center point: {centerpoint}");
        gameObject.transform.position = centerpoint;
    }
}
