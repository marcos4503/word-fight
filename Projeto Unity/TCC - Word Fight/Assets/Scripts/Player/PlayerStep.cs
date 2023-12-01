using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStep : MonoBehaviour
{
    //Private constans
    private const int GROUND_LAYER = 6;

    //Private variables
    private int stepsMaked = 0;

    //Public variables
    public AudioSource[] stepSound;

    //Core methods

    void OnTriggerEnter(Collider collider)
    {
        //If is steping in the ground, play the step sound
        if (collider.gameObject.layer == GROUND_LAYER && stepsMaked > 0)
            stepSound[Random.Range(0, stepSound.Length)].Play();

        //Increase step counter
        stepsMaked += 1;
    }
}
