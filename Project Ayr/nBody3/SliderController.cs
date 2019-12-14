using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Written by Daniel Lambert
//Basically allows a simple rigidbody/collider slider control without having to use configurable joints.
//Configurable joints are slow and buggy. This is fast and not buggy.
public class SliderController : MonoBehaviour
{
    public GameObject handle = null;
    public float output = 0;

    void FixedUpdate()
    {
        Vector3 handlePos = handle.GetComponent<Rigidbody>().transform.localPosition;
        handlePos.x = 0;
        handlePos.y = 0;
        handlePos.z = Mathf.Clamp(handlePos.z, -25.0f, 25.0f);//operates on the local z axis in a width of 50, could easily be configured.
        output = handlePos.z + 25.0f;
        handle.GetComponent<Rigidbody>().transform.localPosition = handlePos;
    }
}
