using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenBehaviour : MonoBehaviour
{
    //Private variables
    private bool isAlreadyStarted = false;

    //Public variables

    public Animator chickenAnimator;
    public ParticleSystem peckParticles;

    //Core methods

    void OnEnable()
    {
        if (isAlreadyStarted == true)
            Start();
    }

    void Start()
    {
        //Start the behaviour loop
        StartCoroutine(ChickenBehaviourLoop());

        //Inform that is started
        isAlreadyStarted = true;
    }

    private IEnumerator ChickenBehaviourLoop()
    {
        //Prepare the interval for each situation
        WaitForSeconds standingTime = new WaitForSeconds(1.0f);
        WaitForSeconds prePeckingTime = new WaitForSeconds(1.0f);
        WaitForSeconds postPeckingTime = new WaitForSeconds(0.3f);

        //Create the loop
        while (true)
        {
            //Calculate a random number
            float randomNumber = Random.Range(0, 100);

            //Run a random animation, based in chance...
            float standChance = 10.0f;
            float lookLeftChance = (standChance) + 20.0f;
            float lookRightChance = (lookLeftChance) + 20.0f;
            float peckChance = (lookRightChance) + 25.0f;
            float scratchChance = (peckChance) + 20.0f;
            //float wingsChance = (scratchChance) + 5.0f;

            //STAND
            if (randomNumber >= 0 && randomNumber < standChance)
            {
                yield return standingTime;
                continue;
            }
            //LOOK L
            if (randomNumber >= standChance && randomNumber < lookLeftChance)
            {
                chickenAnimator.SetTrigger("lookL");
            }
            //LOOK R
            if (randomNumber >= lookLeftChance && randomNumber < lookRightChance)
            {
                chickenAnimator.SetTrigger("lookR");
            }
            //PECK
            if (randomNumber >= lookRightChance && randomNumber < peckChance)
            {
                chickenAnimator.SetTrigger("peck");
                yield return prePeckingTime;
                peckParticles.Play();
                yield return postPeckingTime;
                peckParticles.Stop();
            }
            //SCRATCH
            if (randomNumber >= peckChance && randomNumber < scratchChance)
            {
                chickenAnimator.SetTrigger("scratch");
            }
            //WINGS
            if (randomNumber >= scratchChance && randomNumber < 100)
            {
                chickenAnimator.SetTrigger("wings");
            }

            //Wait a random time
            yield return new WaitForSeconds(Random.Range(1.75f, 4.5f));
        }
    }
}
