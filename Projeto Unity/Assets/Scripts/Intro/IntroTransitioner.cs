using System.Collections;
using System.Collections.Generic;
using MTAssets.NativeAndroidToolkit;
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
        yield return new WaitForSeconds(1);

        //Check if have microphone permission
        bool havePermission = false;
        if (Application.isEditor == false)
            havePermission = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.RecordAudio);
        if (Application.isEditor == true)
            havePermission = true;

        //Wait some seconds..
        yield return new WaitForSeconds(15);

        //Load the new scene
        if (havePermission == false)
            SceneManager.LoadScene("PreMenu");
        if (havePermission == true)
            SceneManager.LoadScene("Menu");
    }
}