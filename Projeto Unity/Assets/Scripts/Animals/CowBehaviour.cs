using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowBehaviour : MonoBehaviour
{
    //Private variables
    private bool isAlreadyStarted = false;

    //Public variables

    public Animator cowAnimator;
    public ParticleSystem eatParticles;

    //Core methods

    void OnEnable()
    {
        if (isAlreadyStarted == true)
            Start();
    }

    void Start()
    {
        //Start the behaviour loop
        StartCoroutine(CowBehaviourLoop());

        //Inform that is started
        isAlreadyStarted = true;
    }

    private IEnumerator CowBehaviourLoop()
    {
        //Prepare the interval for each situation
        WaitForSeconds standingTime = new WaitForSeconds(1.0f);
        WaitForSeconds preEatingTime = new WaitForSeconds(0.4f);
        WaitForSeconds postEatingTime = new WaitForSeconds(0.5f);

        //Create the loop
        while (true)
        {
            //Calculate a random number
            float randomNumber = Random.Range(0, 100);

            //Run a random animation, based in chance...
            float standChance = 10.0f;
            float eatChance = (standChance) + 25.0f;
            float lookLeftChance = (eatChance) + 15.0f;
            float lookRightChance = (lookLeftChance) + 15.0f;

            //STAND
            if (randomNumber >= 0 && randomNumber < standChance)
            {
                yield return standingTime;
                continue;
            }
            //EAT
            if (randomNumber >= standChance && randomNumber < eatChance)
            {
                cowAnimator.SetTrigger("eat");
                yield return preEatingTime;
                eatParticles.Play();
                yield return postEatingTime;
                eatParticles.Stop();
            }
            //LOOK L
            if (randomNumber >= eatChance && randomNumber < lookLeftChance)
            {
                cowAnimator.SetTrigger("lookL");
            }
            //LOOK R
            if (randomNumber >= lookLeftChance && randomNumber < lookRightChance)
            {
                cowAnimator.SetTrigger("lookR");
            }
            //TAIL
            if (randomNumber >= lookRightChance && randomNumber < 100)
            {
                cowAnimator.SetTrigger("tail");
            }

            //Wait a random time
            yield return new WaitForSeconds(Random.Range(3.0f, 5.0f));
        }
    }
}
