using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    //Public variables
    public SpiderController spiderController;

    //Core methods

    void OnTriggerEnter(Collider collider)
    {
        //Try to get the Player Controller
        PlayerController playerController = collider.gameObject.GetComponent<PlayerController>();

        //If don't have a player controller, cancel
        if (playerController == null)
            return;

        //Throw the player
        playerController.ThrowPlayer(this.gameObject.transform.position);
        //Cause the damage
        playerController.CauseDamage(spiderController.damageToCause, PlayerController.DamageType.MonsterAttack);

        //Disable this hitbox
        this.gameObject.SetActive(false);
    }
}