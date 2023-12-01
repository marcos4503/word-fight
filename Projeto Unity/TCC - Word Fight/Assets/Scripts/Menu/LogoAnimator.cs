using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoAnimator : MonoBehaviour
{
    //Private variables
    private bool isInitialized = false;

    //Public variables

    public Animator logoAnimator;

    //Core methods

    void OnEnable()
    {
        //If is already initialized, start the coroutine animation
        if (isInitialized == true)
            StartCoroutine(LogoAnimatorTimer());
    }

    void Start()
    {
        //Start the animator
        StartCoroutine(LogoAnimatorTimer());

        //Inform that is initialized
        isInitialized = true;
    }

    private IEnumerator LogoAnimatorTimer()
    {
        //Prepare the timer
        WaitForSecondsRealtime animationInterval = new WaitForSecondsRealtime(3.0f);

        //Start the loop
        while (true)
        {
            //Wait the interval
            yield return animationInterval;

            //Do the animation
            logoAnimator.SetTrigger("highlight");
        }
    }
}
