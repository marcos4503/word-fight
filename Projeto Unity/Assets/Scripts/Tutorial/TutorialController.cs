using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    //Public variables
    public LevelController levelController;
    public HudController hudController;
    public PlayerController playerController;
    public GameObject introMessage1;
    public GameObject introMessage2;
    public GameObject walk_targetWalkTo;
    public GameObject introMessage3;
    public GameObject introMessage31;
    public GameObject dash_targetWalkTo0;
    public GameObject dash_targetWalkTo1;
    public GameObject introMessage4;
    public GameObject jump_targetWalkTo0;
    public GameObject introMessage41;
    public GameObject jump_targetWalkTo1;
    public GameObject introMessage42;
    public GameObject jump_targetWalkTo2;
    public GameObject introMessage43;
    public GameObject jump_targetWalkTo3;
    public GameObject introMessage5;
    public SpiderController tutorialSpider;
    public GameObject introMessage6;
    public GameObject introMessage7;
    public GameObject introMessagePlatform7;
    public GameObject general_targetWalkTo0;
    public GameObject introMessage8;
    public GameObject introMessage81;
    public GameObject introMessage9;
    public GameObject introMessage91;
    public GameObject introMessagePlatform9;
    public GameObject general_targetWalkTo1;
    public GameObject introMessage10;
    public GameObject introMessage11;
    public GameObject introMessage111;
    public GameObject introMessagePlatform11;
    public GameObject general_targetWalkTo2;
    public GameObject introMessage12;

    //Core methods

    void Start()
    {
        //Hide all control elements
        hudController.joystickAxis.gameObject.SetActive(false);
        hudController.attackButton.gameObject.SetActive(false);
        hudController.jumpButton.gameObject.SetActive(false);
        hudController.dashButton.gameObject.SetActive(false);
        hudController.itemButton.gameObject.SetActive(false);

        //Start the tutorial coroutine
        StartCoroutine(ShowInitialMessages());
    }

    private IEnumerator ShowInitialMessages()
    {
        //Wait the level controller initialize
        while (levelController.levelControllerInitialized == false)
            yield return 0;

        //Wait the time of the fase title exits
        yield return new WaitForSeconds(3.0f);

        //Enble the initial message
        introMessage1.SetActive(true);

        //Wait some time
        yield return new WaitForSeconds(8.0f);

        //Change the message and allow walk
        introMessage1.SetActive(false);
        introMessage2.SetActive(true);
        walk_targetWalkTo.SetActive(true);

        //Enable the joystick
        hudController.joystickAxis.gameObject.SetActive(true);
    }

    private IEnumerator CombatTutorial()
    {
        //Get player data
        Vector3 startPosition = playerController.transform.position;

        //Start the combat loop
        while (tutorialSpider.currentHealth > 0.0f)
        {
            //If the player was received damage
            if (playerController.currentHealth < playerController.maxHealth)
            {
                playerController.playerRigidbody.position = startPosition;
                tutorialSpider.currentHealth = tutorialSpider.maxHealth;
                tutorialSpider.spiderHpBar.value = tutorialSpider.spiderHpBar.maxValue;
                tutorialSpider.spiderCanvas.gameObject.SetActive(false);
                playerController.currentHealth = playerController.maxHealth;
                playerController.isOnDamageThrow = PlayerController.DamageThrowTo.None;
                playerController.ShowEmote(PlayerController.EmoteType.Sad2);
            }

            //Wait for the next frame
            yield return 0;
        }

        //Disable the items and continue
        introMessage5.SetActive(false);

        //Play the comemoration
        levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
        levelController.syllabeAcquieredSound.Play();
        playerController.reachParticles.Play();
        playerController.ShowEmote(PlayerController.EmoteType.Smile3);

        //Reduce the player hp
        playerController.currentHealth = 80.0f;

        //Wait some time
        yield return new WaitForSeconds(3.0f);

        //Change to next step
        introMessage6.SetActive(true);
        int lastQuantityOfPotions = SaveGameManager.healthPotions;
        SaveGameManager.healthPotions += 1;
        hudController.itemButton.gameObject.SetActive(true);

        //Wait the potion usage
        while (SaveGameManager.healthPotions == (lastQuantityOfPotions + 1))
            yield return 0;

        //Hide the text
        introMessage6.SetActive(false);

        //Wait some time
        yield return new WaitForSeconds(2.0f);

        //Play the comemoration
        levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
        levelController.syllabeAcquieredSound.Play();
        playerController.reachParticles.Play();
        playerController.ShowEmote(PlayerController.EmoteType.Smile2);

        //Wait some time
        yield return new WaitForSeconds(3.0f);

        //Show the next instruction to continue
        introMessage7.SetActive(true);
        introMessagePlatform7.SetActive(true);
        general_targetWalkTo0.SetActive(true);
        introMessage8.SetActive(true);
    }

    private IEnumerator SecondaryWordMonitor()
    {
        //Get player data
        Vector3 startPosition = playerController.transform.position;

        //Wait acquiere the secondary word
        while (levelController.secondaryStarReceived == false)
            yield return 0;

        //Wait some time
        yield return new WaitForSeconds(2.0f);

        //Move the player to the start
        playerController.playerRigidbody.position = startPosition;

        //Play the comemoration
        levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
        levelController.syllabeAcquieredSound.Play();
        playerController.reachParticles.Play();
        playerController.ShowEmote(PlayerController.EmoteType.Smile3);

        //Hide the messages
        introMessage8.SetActive(false);
        introMessage81.SetActive(false);

        //Show the new message
        introMessage9.SetActive(true);
        introMessage91.SetActive(true);
        introMessagePlatform9.SetActive(true);
        general_targetWalkTo1.SetActive(true);
    }

    private IEnumerator TertiaryWordMonitor()
    {
        //Get player data
        Vector3 startPosition = playerController.transform.position;

        //Wait acquiere the secondary word
        while (levelController.tertiaryStarReceived == false)
            yield return 0;

        //Wait some time
        yield return new WaitForSeconds(2.0f);

        //Move the player to the start
        playerController.playerRigidbody.position = startPosition;

        //Play the comemoration
        levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
        levelController.syllabeAcquieredSound.Play();
        playerController.reachParticles.Play();
        playerController.ShowEmote(PlayerController.EmoteType.Smile3);

        //Hide the messages
        introMessage10.SetActive(false);

        //Show the new message
        introMessage11.SetActive(true);
        introMessage111.SetActive(true);
        introMessagePlatform11.SetActive(true);
        general_targetWalkTo2.SetActive(true);
    }

    //Public methods

    public void OnReachToTarget(string targetInfo)
    {
        //
        if (targetInfo == "walk_target0")
        {
            introMessage2.SetActive(false);
            walk_targetWalkTo.SetActive(false);
            introMessage3.SetActive(true);
            dash_targetWalkTo0.SetActive(true);

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "dash_target0")
        {
            introMessage3.SetActive(false);
            introMessage31.SetActive(true);
            dash_targetWalkTo1.SetActive(true);

            if (hudController.dashButton.gameObject.activeSelf == false)
            {
                levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
                levelController.syllabeAcquieredSound.Play();
                playerController.reachParticles.Play();
                playerController.ShowEmote(PlayerController.EmoteType.Smile2);
            }

            hudController.dashButton.gameObject.SetActive(true);
        }

        //
        if (targetInfo == "dash_target1")
        {
            introMessage31.SetActive(false);
            dash_targetWalkTo0.SetActive(false);
            dash_targetWalkTo1.SetActive(false);
            introMessage4.SetActive(true);
            jump_targetWalkTo0.SetActive(true);

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "jump_target0")
        {
            introMessage4.SetActive(false);
            jump_targetWalkTo0.SetActive(false);
            introMessage41.SetActive(true);
            jump_targetWalkTo1.SetActive(true);
            hudController.jumpButton.gameObject.SetActive(true);

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile3);
        }

        //
        if (targetInfo == "jump_target1")
        {
            jump_targetWalkTo1.SetActive(false);
            jump_targetWalkTo2.SetActive(true);
            introMessage42.SetActive(true);

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "jump_target2")
        {
            jump_targetWalkTo2.SetActive(false);
            introMessage42.SetActive(false);
            introMessage43.SetActive(true);
            jump_targetWalkTo3.SetActive(true);

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "jump_target3")
        {
            introMessage43.SetActive(false);
            jump_targetWalkTo3.SetActive(false);
            introMessage5.SetActive(true);
            tutorialSpider.gameObject.SetActive(true);
            hudController.attackButton.gameObject.SetActive(true);
            StartCoroutine(CombatTutorial());

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "general_target0")
        {
            introMessage7.SetActive(false);
            general_targetWalkTo0.SetActive(false);
            introMessage81.SetActive(true);
            StartCoroutine(SecondaryWordMonitor());

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "general_target1")
        {
            introMessage9.SetActive(false);
            introMessage91.SetActive(false);
            general_targetWalkTo1.SetActive(false);
            introMessage10.SetActive(true);
            StartCoroutine(TertiaryWordMonitor());

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile1);
        }

        //
        if (targetInfo == "general_target2")
        {
            introMessage11.SetActive(false);
            introMessage111.SetActive(false);
            general_targetWalkTo2.SetActive(false);
            introMessage12.SetActive(true);

            levelController.syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
            levelController.syllabeAcquieredSound.Play();
            playerController.reachParticles.Play();
            playerController.ShowEmote(PlayerController.EmoteType.Smile2);
        }
    }
}