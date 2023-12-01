using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Cache variables
    private Transform thisTransform;
    private PlayerController playerController;

    //Public variables
    public Transform playerTransform;
    public float movementSmooth = 2.0f;
    public float maxHeightToFollow = -8.0f;

    //Core methods

    void Start()
    {
        //Get reference for this transform
        thisTransform = this.gameObject.transform;

        //Find the player controller
        playerController = playerTransform.gameObject.GetComponent<PlayerController>();
    }

    void LateUpdate()
    {
        //Get player current position with Y clampped
        Vector3 playerPositionClampped = new Vector3(playerTransform.position.x, Mathf.Clamp(playerTransform.position.y, maxHeightToFollow, 1000.0f), playerTransform.position.z);

        //If the player is falling into void, just move the camera instantly to follow, and cancel
        if (playerController.cameraFollowInstantly == true)
        {
            thisTransform.position = playerPositionClampped;
            return;
        }

        //Move this camera to the player with smooth
        thisTransform.position = Vector3.Lerp(thisTransform.position, playerPositionClampped, movementSmooth * Time.deltaTime);
    }
}