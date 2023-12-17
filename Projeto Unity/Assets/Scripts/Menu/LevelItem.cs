using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    //Public variables
    public int thisLevelNumber = -1;
    public string levelSceneName = "";
    public Image star1;
    public Image star2;
    public Image star3;
    public Text title;
    public GameObject levelLocker;
    public Button levelButton;
    public MenuController menuController;

    //Core methods

    void Start()
    {
        //Load the level
        levelButton.onClick.AddListener(() => { menuController.LoadLevelAsync(levelSceneName); });
    }
}