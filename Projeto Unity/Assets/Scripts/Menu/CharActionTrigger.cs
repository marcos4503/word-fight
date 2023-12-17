using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharActionTrigger : MonoBehaviour
{
    //Public enums

    public enum ActionType
    {
        Jump,
        DoubleJump,
        ParkourForward,
        ParkourForwardWithTumble,
        SlidingEnter,
        SlidingExit,
        SwinStart,
        SwinParticles
    }

    //Public variables

    public ActionType actionType;
    [Space(16)]
    public BoxCollider hitbox;
    public Transform hitboxShower;

    //Core methods

    void Start()
    {
        //Disable the hitbox shower
        if (Application.isPlaying == true)
            hitboxShower.gameObject.SetActive(false);

        //Update the hitbox shower
        UpdateTheHitboxShower();
    }

    void Update()
    {
        //Update the hitbox shower
        UpdateTheHitboxShower();
    }

    private void UpdateTheHitboxShower()
    {
        //If is playing, cancel
        if (Application.isPlaying == true)
            return;

        //Show the hitbox
        hitboxShower.localPosition = new Vector3(hitbox.center.x, hitbox.center.y, 0);
        hitboxShower.localScale = new Vector3(hitbox.size.x, hitbox.size.y, 1.0f);
    }

    void OnTriggerEnter(Collider collision)
    {
        //Get the player script...
        CharAI playerAi = collision.gameObject.GetComponent<CharAI>();

        //Send the correct signal
        if (actionType == ActionType.Jump)
            playerAi.DoJump();
        if (actionType == ActionType.DoubleJump)
            playerAi.DoDoubleJump();
        if (actionType == ActionType.ParkourForward)
            playerAi.DoParkourForward();
        if (actionType == ActionType.ParkourForwardWithTumble)
            playerAi.DoParkourForwardWithTumble();
        if (actionType == ActionType.SlidingEnter)
            playerAi.DoSlidingEnter();
        if (actionType == ActionType.SlidingExit)
            playerAi.DoSlidingExit();
        if (actionType == ActionType.SwinStart)
            playerAi.DoStartSwin();
        if (actionType == ActionType.SwinParticles)
            playerAi.DoSwinParticles();
    }

}