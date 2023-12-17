using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentVoid : MonoBehaviour
{
    //Cache variables
    private Transform thisTransform;

    //Public variables
    public float heightOfVoid = -20.0f;

    //Core methods

    void Start()
    {
        //Get reference to this transform
        thisTransform = this.gameObject.transform;

        //Run the Update method
        Update();
    }

    void Update()
    {
        //If the application is running, disable it
        if (Application.isPlaying == true)
        {
            this.enabled = false;
            return;
        }

        //Move this transform to the void
        thisTransform.position = new Vector3(0.0f, heightOfVoid, 0.0f);
    }
}