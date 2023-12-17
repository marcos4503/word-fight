using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //Constant variables
    private const int MONSTER_LAYER = 9;

    //Public variables
    public AudioSource[] slashSound;
    public PlayerController playerController;

    //Core methods

    void OnTriggerEnter(Collider collider)
    {
        //If is not a monster, ignore
        if (collider.gameObject.layer != MONSTER_LAYER)
            return;

        //Try to get the Monster controller
        SpiderController spiderController = collider.gameObject.GetComponent<SpiderControllerLinker>().spiderController;

        //Cause the damage
        spiderController.CauseDamage(playerController.damageToCause, playerController.transform.position);

        //Play the slash sound
        slashSound[Random.Range(0, slashSound.Length)].Play();

        //Disable this hitbox
        this.gameObject.SetActive(false);
    }
}