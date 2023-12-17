using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThanksController : MonoBehaviour
{
    //Public variables
    public LevelController levelController;
    public Transform thanksTransform;
    public Transform playerTransform;

    //Core methods
    void Start()
    {
        //Start the coroutine of thanks
        StartCoroutine(WaitFinishAndGiveThanks());
    }

    private IEnumerator WaitFinishAndGiveThanks()
    {
        //Prepare the loop interval
        WaitForSeconds interval = new WaitForSeconds(0.5f);

        //Create the loop to wait the finish
        while (levelController.primaryWordFormed == false)
            yield return interval;

        //Move the thanks to the player
        thanksTransform.position = playerTransform.position;
        //Enable the thanks
        thanksTransform.gameObject.SetActive(true);
    }
}
