using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroTransitioner : MonoBehaviour
{
    //Core methods

    public void Start()
    {
        //Start the timer to go to next scene
        StartCoroutine(MoveToNextScene());
    }

    private IEnumerator MoveToNextScene()
    {
        //Wait some seconds..
        yield return new WaitForSeconds(16);

        //Load the new scene
        SceneManager.LoadScene("Menu");
    }
}