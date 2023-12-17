using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAreaFixer : MonoBehaviour
{
    //Constant variables
    private const int PLAYER_LAYER = 10;

    //Public variables
    public PlatformArea parentPlatformArea;

    //Core methods

    void OnTriggerEnter(Collider collider)
    {
        //If is not the player, ignore
        if (collider.gameObject.layer != PLAYER_LAYER)
            return;

        //Get the player data
        Transform playerTransform = collider.gameObject.transform;
        PlayerController playerController = playerTransform.gameObject.GetComponent<PlayerController>();
        Rigidbody playerRigidBody = playerTransform.gameObject.GetComponent<Rigidbody>();

        //Set parent of this platform
        playerTransform.SetParent(this.gameObject.transform.parent.transform);

        //Inform that have a player
        parentPlatformArea.playerTransformInHere = playerTransform;
        parentPlatformArea.playerControllerInHere = playerController;
        parentPlatformArea.playerRigidbodyInHere = playerRigidBody;
        parentPlatformArea.onEnterLocalPosition = playerTransform.localPosition;
        parentPlatformArea.onEnterLocalPosition.y = 1.0f;
        //Inform that player is above the platform
        parentPlatformArea.playerControllerInHere.isAbovePlatform = true;
    }

    void OnTriggerExit(Collider collider)
    {
        //If is not the player, ignore
        if (collider.gameObject.layer != PLAYER_LAYER)
            return;

        //Get the player data
        Transform playerTransform = collider.gameObject.transform;

        //Remove the parent
        playerTransform.SetParent(null);

        //Inform that player is not above the platform
        if (parentPlatformArea.playerControllerInHere != null)
            parentPlatformArea.playerControllerInHere.isAbovePlatform = false;
        //Inform that don't have a player
        parentPlatformArea.playerTransformInHere = null;
        parentPlatformArea.playerControllerInHere = null;
        parentPlatformArea.playerRigidbodyInHere = null;
        parentPlatformArea.onEnterLocalPosition = Vector3.zero;
    }
}