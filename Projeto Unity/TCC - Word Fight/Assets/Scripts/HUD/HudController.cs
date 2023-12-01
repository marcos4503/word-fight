using System.Collections;
using System.Collections.Generic;
using MTAssets.MobileInputControls;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    //Public variables
    public Button pauseButton;
    public Button resumeButton;
    public Button goToMenuButton;
    public Button tryAgainButton;
    public Button deathGoToMenuButton;
    public AudioSource clickSound;
    public Slider hpBar;
    public GameObject pauseScreenObj;
    public GameObject controlsObj;
    public GameObject pauseButtonObj;
    public GameObject coinsObj;
    public GameObject healthBarObj;
    public GameObject fallTransitionObj;
    public GameObject deathObj;
    [Space(16)]
    [Header("UI Components Shortcuts")]
    public JoystickAxis joystickAxis;
    public JoystickButton jumpButton;
    public JoystickButton dashButton;
    public JoystickButton attackButton;
    public JoystickButton releaseButton;
    public JoystickButton actionButton;
    public JoystickButton itemButton;
    public Text itemCounterButtonText;
    public GameObject itemsButtonsHolder;
    public GameObject actionsButtonsHolder;

    //Core methods

    void Start()
    {
        //Setup the buttons
        pauseButton.onClick.AddListener(() =>
        {
            //Open the pause screen
            clickSound.Play();
            pauseScreenObj.SetActive(true);
            controlsObj.SetActive(false);
            pauseButtonObj.SetActive(false);
            coinsObj.SetActive(false);
            healthBarObj.SetActive(false);

            //Pause the time
            Time.timeScale = 0.0f;
        });
        resumeButton.onClick.AddListener(() =>
        {
            //Close the pause screen
            clickSound.Play();
            pauseScreenObj.SetActive(false);
            controlsObj.SetActive(true);
            pauseButtonObj.SetActive(true);
            coinsObj.SetActive(true);
            healthBarObj.SetActive(true);

            //Resume the time
            Time.timeScale = 1.0f;
        });
        goToMenuButton.onClick.AddListener(() =>
        {
            //Close the pause screen
            clickSound.Play();
            pauseScreenObj.SetActive(false);

            //Resume the time
            Time.timeScale = 1.0f;

            //Start loading the menu
            GameObject sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader");
            sceneLoader.GetComponent<SceneLoader>().LoadSceneByName("Menu");
        });
        tryAgainButton.onClick.AddListener(() =>
        {
            //Close the pause screen
            clickSound.Play();
            deathObj.SetActive(false);

            //Re-load this scene
            GameObject sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader");
            sceneLoader.GetComponent<SceneLoader>().ReLoadThisScene();
        });
        deathGoToMenuButton.onClick.AddListener(() =>
        {
            //Close the pause screen
            clickSound.Play();
            deathObj.SetActive(false);

            //Start loading the menu
            GameObject sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader");
            sceneLoader.GetComponent<SceneLoader>().LoadSceneByName("Menu");
        });
    }
}