using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirObject : MonoBehaviour
{
    //Cache variables
    private Transform thisTransform;

    //Public variables
    public Transform thisObjectTransform;
    public Transform thisObjectEnd;
    public float objectSpeed;

    //Core methods

    void Start()
    {
        //Get reference for this transform
        thisTransform = this.gameObject.transform;
    }

    void Update()
    {
        Movement();
        RemoveItFromSceneIfIsOutOfView();
    }

    private void Movement()
    {
        //Move this object
        thisTransform.Translate((objectSpeed * -1.0f) * Time.deltaTime, 0, 0);
    }

    private void RemoveItFromSceneIfIsOutOfView()
    {
        //If is out of view, disable it automatically
        if (thisObjectEnd.position.x <= -14.0)
            this.gameObject.SetActive(false);
    }
}
