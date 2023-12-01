using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInMaker : MonoBehaviour
{
    //Public variables

    public GameObject fadeInScreen;

    //Core methods

    void Start()
    {
        //Enable the fade in
        fadeInScreen.SetActive(true);
    }
}
