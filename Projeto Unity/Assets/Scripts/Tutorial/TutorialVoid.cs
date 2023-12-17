using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialVoid : MonoBehaviour
{
    //Public variables
    public Transform teleportTo;

    //Core methods

    void OnCollisionEnter(Collision collision)
    {
        //If is the player, teleport it to the target
        if (collision.gameObject.CompareTag("Player") == false)
            return;

        //Get the player transform and rigidbody
        Transform playerTransform = collision.gameObject.transform;
        Rigidbody playerRigidBody = collision.gameObject.GetComponent<Rigidbody>();

        //Reset the velocity
        playerRigidBody.velocity = Vector3.zero;
        playerTransform.position = teleportTo.position;
    }
}