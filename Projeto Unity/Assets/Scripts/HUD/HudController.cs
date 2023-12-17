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
    public GameObject syllabesScreenObj;
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
    public SyllabeItem[] primarySillabesItems;
    public SyllabeItem[] secondarySillabesItems;
    public SyllabeItem[] tertiarySillabesItems;
    public RectTransform syllabeAnimRootObj;
    public RectTransform syllabeAnimText;
    public CanvasScaler canvasScaler;
    public GameObject starsContainerObj;
    public RectTransform star1Transform;
    public RectTransform star1PlaceTransform;
    public RectTransform star2Transform;
    public RectTransform star2PlaceTransform;
    public RectTransform star3Transform;
    public RectTransform star3PlaceTransform;
    public GameObject speakStepsObj;
    public GameObject wordValidationBeforeObj;
    public Text wordValidationWordToSay;
    public Button wordValidationSpeakButton;
    public GameObject wordValidationArrow;
    public GameObject wordValidationSpeakingObj;
    public GameObject wordValidationErrorObj;
    public Button wordValidationErrorButton;
    public GameObject wordValidationSuccessObj;
    public Text wordValidationSuccessMessage;
    public Text wordValidationErrorMessage;
    public GameObject finishScreenObj;
    public Animator finishScreenAnimator;
    public Text primaryWordDisplay;
    public Text secondaryWordDisplay;
    public Text tertiaryWordDisplay;
    public GameObject primaryStar;
    public GameObject secondaryStar;
    public GameObject tertiaryStar;
    public Text levelElapsedTime;
    public Text levelFinishElapsedTime;
    public Button finishGoToMenuButton;
    public GameObject levelScreenObj;
    public Text levelText;

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
            //coinsObj.SetActive(false);
            healthBarObj.SetActive(false);
            syllabesScreenObj.SetActive(false);
            starsContainerObj.SetActive(false);

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
            //coinsObj.SetActive(true);
            healthBarObj.SetActive(true);
            syllabesScreenObj.SetActive(true);
            starsContainerObj.SetActive(true);

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

            //Re-load the save
            SaveGameManager.LoadData();

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