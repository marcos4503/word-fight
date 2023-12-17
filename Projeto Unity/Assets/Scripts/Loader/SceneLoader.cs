using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SceneLoader : MonoBehaviour
{
    //Classes of script
    public class Delegates
    {
        public delegate void OnDismissLoadingScreen();
    }

    //Cache variables
    public int lastDefinedTip = -1;

    //Public variables
    public GameObject loadCanvas;
    public Animator canvasAnimator;
    public Text tipsDisplay;
    public string[] tipsList;
    public ContentSizeFitter contentSizeFitter0;
    public ContentSizeFitter contentSizeFitter1;
    public GameObject loadingText;
    public GameObject doneText;
    [Space(8)]
    [Header("Events")]
    public Delegates.OnDismissLoadingScreen onDismissLoadingScreen;

    //Core methods

    void Start()
    {
        //Defines as not destructible
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {

    }

    private void ShowRandomTip()
    {
        //Get random number for tip
        int random = Random.Range(0, tipsList.Length);
        if (random == lastDefinedTip)
            while (random == lastDefinedTip)
                random = Random.Range(0, tipsList.Length);

        //Show a random tip
        tipsDisplay.text = tipsList[random];

        //Inform the defined tip
        lastDefinedTip = random;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        //Prepare the interval between loaded checks
        WaitForSeconds checkInterval = new WaitForSeconds(1.5f);

        //Enable the canvas
        loadCanvas.SetActive(true);

        //Show the correct text
        loadingText.SetActive(true);
        doneText.SetActive(false);

        //Show a random tip
        ShowRandomTip();
        Canvas.ForceUpdateCanvases();
        yield return 0;
        contentSizeFitter0.enabled = false;
        contentSizeFitter1.enabled = false;
        yield return 0;
        contentSizeFitter0.enabled = true;
        contentSizeFitter1.enabled = true;

        //Wait a time
        yield return new WaitForEndOfFrame();

        //Call the start loading animation
        canvasAnimator.SetTrigger("startLoad");

        //Wait a time
        yield return new WaitForSeconds(1.0f);

        //Start loading the scene
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        //Wait the loading finishes, before continue...
        while (asyncLoad.isDone == false)
        {
            //If is not loaded yet..
            if (asyncLoad.progress < 0.9f)
                yield return checkInterval;

            //Wait more time
            yield return checkInterval;

            //If is loaded, allow the scene activation
            asyncLoad.allowSceneActivation = true;
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
        }

        //Wait a time
        yield return new WaitForEndOfFrame();

        //Show the correct text
        loadingText.SetActive(false);
        doneText.SetActive(true);

        //Wait the touch to remove the loading screen
        while (true)
        {
            //If touch the screen, continue
            if (Input.GetMouseButtonDown(0) == true)
            {
                loadingText.SetActive(false);
                doneText.SetActive(false);
                break;
            }

            //Wait the next frame
            yield return 0;
        }

        //Call the start loading animation
        canvasAnimator.SetTrigger("finishLoad");

        //Wait a time
        yield return new WaitForSeconds(1.0f);

        //Send callback for the dismiss of the loading screen
        if (onDismissLoadingScreen != null)
            onDismissLoadingScreen();
        onDismissLoadingScreen = null;

        //Disable the canvas
        loadCanvas.SetActive(false);
    }

    private IEnumerator ReLoadThisSceneAsync()
    {
        //Prepare the interval between loaded checks
        WaitForSeconds checkInterval = new WaitForSeconds(1.0f);
        //Get current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        //Enable the canvas
        loadCanvas.SetActive(true);

        //Show the correct text
        loadingText.SetActive(true);
        doneText.SetActive(false);

        //Show a random tip
        ShowRandomTip();
        Canvas.ForceUpdateCanvases();
        yield return 0;
        contentSizeFitter0.enabled = false;
        contentSizeFitter1.enabled = false;
        yield return 0;
        contentSizeFitter0.enabled = true;
        contentSizeFitter1.enabled = true;

        //Wait a time
        yield return new WaitForEndOfFrame();

        //Call the start loading animation
        canvasAnimator.SetTrigger("startLoad");

        //Wait a time
        yield return new WaitForSeconds(1.0f);

        //Start loading a Temp Scene
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncLoad0 = SceneManager.LoadSceneAsync("TempScene");
        asyncLoad0.allowSceneActivation = false;

        //Wait the loading finishes, before continue...
        while (asyncLoad0.isDone == false)
        {
            //If is not loaded yet..
            if (asyncLoad0.progress < 0.9f)
                yield return checkInterval;

            //Wait more time
            yield return checkInterval;

            //If is loaded, allow the scene activation
            asyncLoad0.allowSceneActivation = true;
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
        }

        //Wait a time
        yield return new WaitForEndOfFrame();

        //Wait more time
        yield return checkInterval;

        //Show a random tip
        ShowRandomTip();
        Canvas.ForceUpdateCanvases();
        yield return 0;
        contentSizeFitter0.enabled = false;
        contentSizeFitter1.enabled = false;
        yield return 0;
        contentSizeFitter0.enabled = true;
        contentSizeFitter1.enabled = true;

        //Start loading the same scene
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncLoad1 = SceneManager.LoadSceneAsync(currentSceneName);
        asyncLoad1.allowSceneActivation = false;

        //Wait the loading finishes, before continue...
        while (asyncLoad1.isDone == false)
        {
            //If is not loaded yet..
            if (asyncLoad1.progress < 0.9f)
                yield return checkInterval;

            //Wait more time
            yield return checkInterval;

            //If is loaded, allow the scene activation
            asyncLoad1.allowSceneActivation = true;
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
        }

        //Wait a time
        yield return new WaitForEndOfFrame();

        //Show the correct text
        loadingText.SetActive(false);
        doneText.SetActive(true);

        //Wait the touch to remove the loading screen
        while (true)
        {
            //If touch the screen, continue
            if (Input.GetMouseButtonDown(0) == true)
            {
                loadingText.SetActive(false);
                doneText.SetActive(false);
                break;
            }

            //Wait the next frame
            yield return 0;
        }

        //Call the start loading animation
        canvasAnimator.SetTrigger("finishLoad");

        //Wait a time
        yield return new WaitForSeconds(1.0f);

        //Send callback for the dismiss of the loading screen
        if (onDismissLoadingScreen != null)
            onDismissLoadingScreen();
        onDismissLoadingScreen = null;

        //Disable the canvas
        loadCanvas.SetActive(false);
    }

    //Public methods

    public void LoadSceneByName(string sceneNameToLoad)
    {
        //Call the load async
        StartCoroutine(LoadSceneAsync(sceneNameToLoad));
    }

    public void ReLoadThisScene()
    {
        //Call the load async
        StartCoroutine(ReLoadThisSceneAsync());
    }
}