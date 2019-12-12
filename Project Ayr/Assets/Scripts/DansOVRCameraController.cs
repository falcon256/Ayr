using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DansOVRCameraController : MonoBehaviour
{

    public GameObject leftHand = null;
    public GameObject rightHand = null;
    public SystemTestManager systemController = null;
    private float handDistance = 0;
    private float totalScale = 200;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += this.transform.forward*Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical") * Time.deltaTime * totalScale*0.2f;
        this.transform.position += this.transform.right * Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal") * Time.deltaTime * totalScale*0.2f;
        this.transform.Rotate(new Vector3(0, Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickHorizontal") * Time.deltaTime * 100.0f, 0));
        
        //Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical") * Time.deltaTime * 100.0f
        //now we do the calcs for scaling.
        float newHandDistance = (leftHand.transform.position - rightHand.transform.position).magnitude;

        if(Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger")>0.5f && Input.GetAxis("Oculus_CrossPlatform_SecondaryHandTrigger")>0.5f)
        {
            float scaleDelta = handDistance - newHandDistance;
            Vector3 deltaV = this.transform.position * scaleDelta;
            deltaV.y += scaleDelta;
            Debug.Log(scaleDelta);
            this.transform.position += deltaV;
            totalScale = this.transform.position.magnitude;
        }

        handDistance = newHandDistance;
    }

    private void FixedUpdate()
    {
        if(Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger")>0.1f)
        {
            systemController.spawnEntity(leftHand.transform, -1.0f * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));
        }

        if (Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.1f)
        {
            systemController.spawnEntity(rightHand.transform, 2000.0f * Input.GetAxis("Oculus_CrossPlatform_SecondaryIndexTrigger"));
        }

    }
}
