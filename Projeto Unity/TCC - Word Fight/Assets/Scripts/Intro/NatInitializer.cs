using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTAssets.NativeAndroidToolkit;
using System.Net;

public class NatInitializer : MonoBehaviour
{
    //Core methods

    void Start()
    {
        //Initialize the Native Android Toolkit
        if (NativeAndroidToolkit.isInitialized == false)
            NativeAndroidToolkit.Initialize();

        //Get the device GPU name
        string gpuName = SystemInfo.graphicsDeviceName;
        //Split the GPU name parts
        string[] gpuNameParts = gpuName.Split(" ");
        //If is Adreno 630 or newer, enable MSAA...
        if (gpuNameParts[0].ToLower().Contains("adreno") == true)
            if (gpuNameParts.Length >= 3)
            {
                if (int.Parse(gpuNameParts[2]) >= 630)
                {
                    QualitySettings.antiAliasing = 4;
                    Debug.LogWarning("Adreno 630 or newer detected: MSAA 4x enabled!");
                    goto ContinueToFrameRate;
                }
                if (int.Parse(gpuNameParts[2]) < 630)
                {
                    Debug.LogWarning("Adreno older than 630 detected: MSAA disabled!");
                    goto ContinueToFrameRate;
                }
            }

        ContinueToFrameRate:

        //Define the target frame rate of application
        Application.targetFrameRate = 75;
    }
}