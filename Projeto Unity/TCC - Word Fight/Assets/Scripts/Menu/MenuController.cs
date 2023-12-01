using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    //Private variables
    private LevelItem[] levelItemsFound;

    //Public variables
    public Animator menuAnimator;
    public Animator musicAnimator;
    public Animator cameraAnimator;
    public AudioSource clickSound;
    public Button playButton;
    public Button playBackButton;
    public Button creditsButton;
    public Button creditsBackButton;
    public Button exitButton;
    public GameObject levelItemsRoot;
    public Sprite filledStarSprite;

    //Core methods

    public void Start()
    {
        //First, load the game data
        SaveGameManager.LoadData();

        //If don't have the first level unlocked, unlock it
        if (SaveGameManager.gameLevels[1].finished == false)
            SaveGameManager.gameLevels[1].finished = true;

        //Find all levels items and display the levels in the shower
        levelItemsFound = levelItemsRoot.GetComponentsInChildren<LevelItem>(true);
        for (int i = 0; i < levelItemsFound.Length; i++)
        {
            //Get the id of this level item
            int levelId = levelItemsFound[i].thisLevelNumber;

            //Remove the locker, if has done the level
            if (SaveGameManager.gameLevels[levelId].finished == true)
                levelItemsFound[i].levelLocker.gameObject.SetActive(false);

            //Enable the stars
            if (SaveGameManager.gameLevels[levelId].star1 == true)
                levelItemsFound[i].star1.sprite = filledStarSprite;
            if (SaveGameManager.gameLevels[levelId].star2 == true)
                levelItemsFound[i].star2.sprite = filledStarSprite;
            if (SaveGameManager.gameLevels[levelId].star3 == true)
                levelItemsFound[i].star3.sprite = filledStarSprite;
        }

        //Setups the button
        playButton.onClick.AddListener(() =>
        {
            menuAnimator.SetInteger("menuScreen", 2);
            clickSound.Play();
        });
        playBackButton.onClick.AddListener(() =>
        {
            menuAnimator.SetInteger("menuScreen", 1);
            clickSound.Play();
        });
        creditsButton.onClick.AddListener(() =>
        {
            menuAnimator.SetInteger("menuScreen", 3);
            clickSound.Play();
        });
        creditsBackButton.onClick.AddListener(() =>
        {
            menuAnimator.SetInteger("menuScreen", 1);
            clickSound.Play();
        });
        exitButton.onClick.AddListener(() =>
        {
            clickSound.Play();
            Application.Quit();
        });

        //Shows the menu after a time
        StartCoroutine(WaitAndShowTheMenu());
    }

    private IEnumerator WaitAndShowTheMenu()
    {
        //Wait and show the menu
        yield return new WaitForSeconds(3.9f);
        menuAnimator.SetInteger("menuScreen", 0);
        yield return new WaitForSeconds(0.1f);
        menuAnimator.SetInteger("menuScreen", 1);
    }

    private IEnumerator HideMenuMoveCameraAndWaitToStartLoad(string levelName)
    {
        //Play the click button
        clickSound.Play();

        //Hide the menu
        menuAnimator.SetTrigger("hideMenu");

        //Wait before continue
        yield return new WaitForSeconds(0.5f);

        musicAnimator.SetTrigger("doFadeOut");
        cameraAnimator.SetTrigger("doFadeOut");

        //Wait before load..
        yield return new WaitForSeconds(2.5f);

        //Start loading the scene...
        GameObject sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader");
        sceneLoader.GetComponent<SceneLoader>().LoadSceneByName(levelName);
    }

    //Public methods

    public void LoadLevelAsync(string levelName)
    {
        //Start the loading animation
        StartCoroutine(HideMenuMoveCameraAndWaitToStartLoad(levelName));
    }
}