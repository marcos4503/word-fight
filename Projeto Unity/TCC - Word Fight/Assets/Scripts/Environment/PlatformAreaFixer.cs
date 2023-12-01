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

        //Set parent of this platform
        playerTransform.SetParent(this.gameObject.transform.parent.transform);

        //Inform that have a player
        parentPlatformArea.playerTransformInHere = playerTransform;
        parentPlatformArea.playerControllerInHere = playerController;
        parentPlatformArea.onEnterLocalPosition = playerTransform.localPosition;
        parentPlatformArea.onEnterLocalPosition.y = 1.0f;
    }

    void OnTriggerExit(Collider collider)
    {
        //If is not the player, ignore
        if (collider.gameObject.layer != PLAYER_LAYER)
            return;

        //Get the player data
        Transform playerTransform = collider.gameObject.transform;
        PlayerController playerController = playerTransform.gameObject.GetComponent<PlayerController>();

        //Remove the parent
        playerTransform.SetParent(null);

        //Inform that don't have a player
        parentPlatformArea.playerTransformInHere = null;
        parentPlatformArea.playerControllerInHere = null;
        parentPlatformArea.onEnterLocalPosition = Vector3.zero;
    }
}