using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    //Public variables
    public string info = "";
    public TutorialController tutorialController;

    //Core methods

    void OnTriggerEnter(Collider collider)
    {
        //If is not the player, ignore
        if (collider.gameObject.CompareTag("Player") == false)
            return;

        //Send the callback to tutorial controller
        tutorialController.OnReachToTarget(info);
    }
}