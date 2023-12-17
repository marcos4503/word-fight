using System.Collections;
using System.Collections.Generic;
using MTAssets.NativeAndroidToolkit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PermissionController : MonoBehaviour
{
    //Public variables
    public Button givePermissionButton;
    public Image statusIcon;
    public AudioSource wrongSound;
    public AudioSource successSound;
    public Animator canvasAnimator;
    public Sprite successSprite;

    //Core methods

    void Start()
    {
        //Start the coroutine to show the give permission button
        StartCoroutine(ShowGiveButton());
    }

    private IEnumerator ShowGiveButton()
    {
        //Wait some time
        yield return new WaitForSeconds(3.0f);

        //Setup the button
        givePermissionButton.onClick.AddListener(() =>
        {
            StartCoroutine(RequestPermissionAndWait());
        });

        //Show the button
        givePermissionButton.gameObject.SetActive(true);
    }

    private IEnumerator RequestPermissionAndWait()
    {
        //Hide the button and status
        givePermissionButton.gameObject.SetActive(false);
        statusIcon.gameObject.SetActive(false);

        //Request the permission
        new NAT.Permissions.PermissionRequester()
            .addPermissionToRequest(NAT.Permissions.AndroidPermission.RecordAudio)
            .RequestThisPermissions();

        //Wait time for check permission
        yield return new WaitForSeconds(2.5f);

        //Check if have permission
        bool permissionGuaranteed = NAT.Permissions.isPermissionGuaranteed(NAT.Permissions.AndroidPermission.RecordAudio);

        //If don't have the permission
        if (permissionGuaranteed == false)
        {
            //Show the button and status
            givePermissionButton.gameObject.SetActive(true);
            statusIcon.gameObject.SetActive(true);

            //Play the error sound
            wrongSound.Play();
            canvasAnimator.SetTrigger("status");
        }
        //If have the permission
        if (permissionGuaranteed == true)
        {
            //Show the status
            statusIcon.sprite = successSprite;
            statusIcon.gameObject.SetActive(true);

            //Play the error sound
            successSound.Play();
            canvasAnimator.SetTrigger("status");

            //Wait some time
            yield return new WaitForSeconds(3.0f);

            //Play the out animation
            canvasAnimator.SetTrigger("out");

            //Load the menu scene
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("Menu");
        }
    }
}