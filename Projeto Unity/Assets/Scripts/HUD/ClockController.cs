using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ClockController : MonoBehaviour
{
    //Cache variables
    private Text clockText = null;
    private bool isInitialized = false;
    private int seconds;
    private int minutes;

    //Core methods
    void Start()
    {
        //Get the references
        clockText = this.gameObject.GetComponent<Text>();

        //Start the clock coroutine
        StartCoroutine(DoClockRoutine());

        //Inform that is initialized
        isInitialized = true;
    }

    void OnEnable()
    {
        //If is not initialized, ignore
        if (isInitialized == false)
            return;

        //Start the clock coroutine
        StartCoroutine(DoClockRoutine());
    }

    private IEnumerator DoClockRoutine()
    {
        //Prepare the clock interval
        WaitForSeconds interval = new WaitForSeconds(1.0f);

        //Create the clock loop
        while (true)
        {
            //Wait the interval
            yield return interval;

            //Increase the clock
            seconds += 1;
            if (seconds > 59)
            {
                seconds = 0;
                minutes += 1;
            }

            //Prepare the time to text
            StringBuilder stringBuilder = new StringBuilder();
            if (minutes < 10)
            {
                stringBuilder.Append("0");
                stringBuilder.Append(minutes);
            }
            if (minutes >= 10)
                stringBuilder.Append(minutes);
            if (seconds < 10)
            {
                stringBuilder.Append(":0");
                stringBuilder.Append(seconds);
            }
            if (seconds >= 10)
            {
                stringBuilder.Append(":");
                stringBuilder.Append(seconds);
            }

            //Update the clock
            clockText.text = stringBuilder.ToString();
        }
    }
}