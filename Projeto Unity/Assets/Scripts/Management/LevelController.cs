using System.Collections;
using System.Collections.Generic;
using MTAssets.NativeAndroidToolkit;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    //Enums of script
    public enum SyllabeType
    {
        Primary,
        Secondary,
        Tertiary
    }
    public enum WordFormed
    {
        Primary,
        Secondary,
        Tertiary
    }
    public enum EditorTestingMode
    {
        EmitErrorResponse,
        EmitErrorWord,
        EmitSuccessWord
    }

    //Classes of script
    [System.Serializable]
    public class WordSyllabe
    {
        public string syllabe = "";
        public bool acquired = false;
        public RectTransform syllabeSlot = null;
    }

    //Cache variables
    private SpiderController[] spidersControllers;
    private string wordSpokenByTheUser = "";
    private Coroutine errorReasonCoroutine = null;

    //Public variables
    [Header("Level Info")]
    public int levelId = -1;
    public EditorTestingMode editorTestingMode = EditorTestingMode.EmitSuccessWord;
    public float damageOnWordError = 10.0f;
    public WordSyllabe[] primaryWordSillabes;
    public bool primaryWordFormed = false;
    public bool primaryStarReceived = false;
    public WordSyllabe[] secondaryWordSillabes;
    public bool secondaryWordFormed = false;
    public bool secondaryStarReceived = false;
    public WordSyllabe[] tertiaryWordSillabes;
    public bool tertiaryWordFormed = false;
    public bool tertiaryStarReceived = false;
    public bool levelControllerInitialized = false;
    [Space(8)]
    [Header("Controller Components")]
    public AudioSource syllabeAcquieredSound;
    public Transform microphoneRootTransform;
    public Transform microphoneWireEndTransform;
    public LineRenderer microphoneWireRenderer;
    public Animator microphoneAnimator;
    public AudioSource starAcquieredSound;
    public AudioSource microphoneEntrySound;
    public AudioSource questioningSound;
    public AudioSource worldResponseSuccessSound;
    public AudioSource worldResponseFailSound;
    public AudioSource[] finishSounds;
    public AudioSource finishSoundLoop;
    [Space(8)]
    [Header("Level Components")]
    public HudController hudController;
    public PlayerController playerController;
    public CameraController cameraController;
    public Transform monstersRootGameObject;
    public AudioSource levelBackgroundMusic;

    //Core methods

    void Awake()
    {
        //If have a scene loader in the game...
        GameObject sceneLoaderObj = GameObject.FindGameObjectWithTag("SceneLoader");
        if (sceneLoaderObj != null)
        {
            //Get the scene loader script
            SceneLoader sceneLoader = sceneLoaderObj.GetComponent<SceneLoader>();

            //Bock the player to play
            playerController.isFreezedByLevelController = true;
            //Hide the controls
            hudController.controlsObj.SetActive(false);

            //Install a callback to know when the player dismiss the loading screen
            sceneLoader.onDismissLoadingScreen += () => { StartCoroutine(LevelIntro(false)); };
        }
        if (sceneLoaderObj == null)
            StartCoroutine(LevelIntro(true));

        //Enable the slots for sillabes of primary word
        foreach (SyllabeItem item in hudController.primarySillabesItems)
            item.gameObject.SetActive(false);
        for (int i = 0; i < primaryWordSillabes.Length; i++)
        {
            hudController.primarySillabesItems[i].syllabeText.text = "";
            hudController.primarySillabesItems[i].gameObject.SetActive(true);

            //Get reference of slot of this syllabe
            primaryWordSillabes[i].syllabeSlot = hudController.primarySillabesItems[i].syllabeText.GetComponent<RectTransform>();
        }

        //Enable the slots for sillabes of secondary word
        foreach (SyllabeItem item in hudController.secondarySillabesItems)
            item.gameObject.SetActive(false);
        for (int i = 0; i < secondaryWordSillabes.Length; i++)
        {
            hudController.secondarySillabesItems[i].syllabeText.text = "";
            hudController.secondarySillabesItems[i].gameObject.SetActive(true);

            //Get reference of slot of this syllabe
            secondaryWordSillabes[i].syllabeSlot = hudController.secondarySillabesItems[i].syllabeText.GetComponent<RectTransform>();
        }

        //Enable the slots for sillabes of tertiary word
        foreach (SyllabeItem item in hudController.tertiarySillabesItems)
            item.gameObject.SetActive(false);
        for (int i = 0; i < tertiaryWordSillabes.Length; i++)
        {
            hudController.tertiarySillabesItems[i].syllabeText.text = "";
            hudController.tertiarySillabesItems[i].gameObject.SetActive(true);

            //Get reference of slot of this syllabe
            tertiaryWordSillabes[i].syllabeSlot = hudController.tertiarySillabesItems[i].syllabeText.GetComponent<RectTransform>();
        }

        //Apply this level controller to all spiders found
        spidersControllers = monstersRootGameObject.GetComponentsInChildren<SpiderController>(true);
        foreach (SpiderController spider in spidersControllers)
            spider.levelController = this;
    }

    private IEnumerator LevelIntro(bool skipIntro)
    {
        //If is not desired to skip the intro
        if (skipIntro == false)
        {
            //Disable the pause button
            hudController.pauseButtonObj.SetActive(false);

            //Show the intro screen
            if (levelId == 1)
                hudController.levelText.text = "Fase 1 - Tutorial";
            if (levelId != 1)
                hudController.levelText.text = ("Fase " + levelId);
            hudController.levelScreenObj.SetActive(true);

            //Wait some seconds
            yield return new WaitForSeconds(3.0f);

            //Hide the screen
            hudController.levelScreenObj.SetActive(false);

            //Unblock the player to play
            playerController.isFreezedByLevelController = false;
            //Show the controls
            hudController.controlsObj.SetActive(true);

            //Enable the pause button
            hudController.pauseButtonObj.SetActive(true);
        }

        //Inform that is initialized
        levelControllerInitialized = true;

        //Wait some time
        yield return new WaitForSeconds(1.0f);

        //Start playing the level music
        if (levelBackgroundMusic != null)
            levelBackgroundMusic.Play();
    }

    private IEnumerator SyllabePlacing(SyllabeType syllabeType, string acquiredSyllabe, RectTransform syllabeUiSlotTransform, Transform spiderTransform)
    {
        //Enable the syllabe object animation
        hudController.syllabeAnimRootObj.gameObject.SetActive(true);

        //Format the syllabe text
        Text syllabeTextComponent = hudController.syllabeAnimText.GetComponent<Text>();
        if (syllabeType == SyllabeType.Primary)
        {
            syllabeTextComponent.fontSize = 14;
            syllabeTextComponent.lineSpacing = 1;
            syllabeTextComponent.alignment = TextAnchor.MiddleCenter;
            syllabeTextComponent.resizeTextForBestFit = true;
            syllabeTextComponent.resizeTextMinSize = 10;
            syllabeTextComponent.resizeTextMaxSize = 20;
            syllabeTextComponent.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            syllabeTextComponent.raycastTarget = false;
        }
        if (syllabeType == SyllabeType.Secondary || syllabeType == SyllabeType.Tertiary)
        {
            syllabeTextComponent.fontSize = 14;
            syllabeTextComponent.lineSpacing = 1;
            syllabeTextComponent.alignment = TextAnchor.MiddleCenter;
            syllabeTextComponent.resizeTextForBestFit = true;
            syllabeTextComponent.resizeTextMinSize = 10;
            syllabeTextComponent.resizeTextMaxSize = 16;
            syllabeTextComponent.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            syllabeTextComponent.raycastTarget = false;
        }
        if (syllabeType == SyllabeType.Primary)
            syllabeTextComponent.color = new Color(0.8980393f, 0.7960785f, 0.09019608f, 1.0f);
        if (syllabeType == SyllabeType.Secondary)
            syllabeTextComponent.color = new Color(0.07058824f, 0.5176471f, 0.9294118f, 1.0f);
        if (syllabeType == SyllabeType.Tertiary)
            syllabeTextComponent.color = new Color(0.07058824f, 0.5254902f, 0.1098039f, 1.0f);

        //Show the acquired syllabe
        syllabeTextComponent.text = acquiredSyllabe;

        //Get screen size information
        Vector2 maxCanvasSize = hudController.canvasScaler.referenceResolution;

        //Get the default size of font and set the initial size of the font in the start of animation
        float initialSizeOfFont = 40.0f;
        int defaultSizeOfFont = syllabeTextComponent.resizeTextMaxSize;
        syllabeTextComponent.resizeTextMaxSize = (int)initialSizeOfFont;
        //Get the default color of font and set the initial color of the font in the start of animation
        Color initialFontColor = syllabeTextComponent.color;
        Color defaultFontColor = Color.white;
        if (syllabeType == SyllabeType.Primary)
            defaultFontColor = Color.black;
        if (syllabeType == SyllabeType.Secondary || syllabeType == SyllabeType.Tertiary)
            defaultFontColor = Color.white;

        //Play the syllabe acquiered sound effect
        syllabeAcquieredSound.pitch = Random.Range(1.0f, 1.2f);
        syllabeAcquieredSound.PlayDelayed(1.35f);

        //Start the movement of the syllabe to te respective slot
        float general_animationLastTime = Time.time;
        float general_animationDuration = 1.5f;
        float general_animationCurrentTime = 0.0f;
        float fontReductionStartTime = 0.8f;
        float fontReductionDurationTime = 0.5f;
        float fontRecolorStartTime = 1.0f;
        float fontRecolorDurationTime = 0.5f;
        //Prepare to move the syllabe to the slot
        while (general_animationCurrentTime < general_animationDuration)
        {
            //Update the elapsed time since animation start
            general_animationCurrentTime += (Time.time - general_animationLastTime);
            general_animationLastTime = Time.time;

            //Fix the animation current time
            if (general_animationCurrentTime > general_animationDuration)
                general_animationCurrentTime = general_animationDuration;

            //Move the root of syllabe to follow the spider
            Vector3 spiderPositionInCameraViewport = cameraController.gameCamera.WorldToViewportPoint(new Vector3(spiderTransform.position.x, (spiderTransform.position.y + 4.5f), spiderTransform.position.z));
            hudController.syllabeAnimRootObj.anchoredPosition = new Vector2(spiderPositionInCameraViewport.x * maxCanvasSize.x, spiderPositionInCameraViewport.y * maxCanvasSize.y);

            //Move the syllabe to target slot
            hudController.syllabeAnimText.position = Vector3.Lerp(hudController.syllabeAnimRootObj.position, syllabeUiSlotTransform.position, (general_animationCurrentTime / general_animationDuration));

            //Animate the font size
            if (general_animationCurrentTime > fontReductionStartTime)
            {
                float timeFixed = general_animationCurrentTime - fontReductionStartTime;
                syllabeTextComponent.resizeTextMaxSize = (int)(Mathf.Lerp(initialSizeOfFont, (float)defaultSizeOfFont, timeFixed / fontReductionDurationTime));
            }

            //Animat font color
            if (general_animationCurrentTime > fontRecolorStartTime)
            {
                float timeFixed = general_animationCurrentTime - fontRecolorStartTime;
                syllabeTextComponent.color = Color.Lerp(initialFontColor, defaultFontColor, timeFixed / fontRecolorDurationTime);
            }

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Start the increase of the syllabe
        float step1_animationLastTime = Time.time;
        float step1_animationDuration = 0.2f;
        float step1_animationCurrentTime = 0.0f;
        float targetScale = 1.75f;
        //Start the animation loop
        while (step1_animationCurrentTime < step1_animationDuration)
        {
            //Update the elapsed time since animation start
            step1_animationCurrentTime += (Time.time - step1_animationLastTime);
            step1_animationLastTime = Time.time;

            //Fix the animation current time
            if (step1_animationCurrentTime > step1_animationDuration)
                step1_animationCurrentTime = step1_animationDuration;

            //Increase the word syllabe
            hudController.syllabeAnimText.localScale = Vector3.Lerp(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(targetScale, targetScale, targetScale), (step1_animationCurrentTime / step1_animationDuration));

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Start the decrease of the syllabe
        float step2_animationLastTime = Time.time;
        float step2_animationDuration = 0.1f;
        float step2_animationCurrentTime = 0.0f;
        //Start the animation loop
        while (step2_animationCurrentTime < step2_animationDuration)
        {
            //Update the elapsed time since animation start
            step2_animationCurrentTime += (Time.time - step2_animationLastTime);
            step2_animationLastTime = Time.time;

            //Fix the animation current time
            if (step2_animationCurrentTime > step2_animationDuration)
                step2_animationCurrentTime = step2_animationDuration;

            //Increase the word syllabe
            hudController.syllabeAnimText.localScale = Vector3.Lerp(new Vector3(targetScale, targetScale, targetScale), new Vector3(1.0f, 1.0f, 1.0f), (step2_animationCurrentTime / step2_animationDuration));

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Disable the syllabe object animation
        hudController.syllabeAnimRootObj.gameObject.SetActive(false);
        syllabeUiSlotTransform.GetComponent<Text>().text = acquiredSyllabe;
    }

    private IEnumerator WaitTimeAndSendFakeUserResponse(float waitTime, string toSay)
    {
        //Wait time
        yield return new WaitForSeconds(waitTime);

        //Send fake response
        wordSpokenByTheUser = toSay;
    }

    private IEnumerator ShowSpeakErrorReason(string speakErrorReason)
    {
        //Show the error reason
        hudController.wordValidationErrorMessage.text = speakErrorReason;
        hudController.wordValidationErrorMessage.gameObject.SetActive(true);

        //Wait time
        yield return new WaitForSeconds(10.0f);

        //Hide the error reason
        hudController.wordValidationErrorMessage.gameObject.SetActive(false);
    }

    private IEnumerator CauseDamageAfterSomeTime()
    {
        //Wait some time
        yield return new WaitForSeconds(0.2f);

        //Cause the damage
        playerController.CauseDamage(damageOnWordError, PlayerController.DamageType.WordFail);
    }

    private IEnumerator MicrophoneInputReceiver(WordFormed wordFormed)
    {
        //Lock the player from do anything
        playerController.isFreezedByLevelController = true;
        hudController.joystickAxis.gameObject.SetActive(false);
        hudController.attackButton.gameObject.SetActive(false);
        hudController.jumpButton.gameObject.SetActive(false);
        hudController.dashButton.gameObject.SetActive(false);
        hudController.pauseButtonObj.gameObject.SetActive(false);

        //Wait the player fall to the ground, stop running and stop attacking, before continue
        while (playerController.isOnGround == false && playerController.isRunning == false && playerController.isAttacking == false && playerController.isOnDashImpulse == false)
            yield return 0;

        //Wait time before pull microphone
        yield return new WaitForSeconds(1.0f);

        //Get the default volume of the background music
        float defaultBgMusicVolume = levelBackgroundMusic.volume;

        //Pull the Microphone
        microphoneRootTransform.gameObject.SetActive(true);
        microphoneWireRenderer.gameObject.SetActive(false);
        yield return 0;
        microphoneWireRenderer.gameObject.SetActive(true);
        microphoneRootTransform.position = playerController.transform.position;
        if (playerController.currentlyFacingTo == PlayerController.CurrentlyFacingTo.Right)
            microphoneRootTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        if (playerController.currentlyFacingTo == PlayerController.CurrentlyFacingTo.Left)
            microphoneRootTransform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);

        //Play the pull sound effect
        microphoneEntrySound.pitch = Random.Range(1.0f, 1.1f);
        microphoneEntrySound.PlayDelayed(0.2f);

        //Animate the wire of the microphone during the animation
        float amim1_animationLastTime = Time.time;
        float amim1_animationDuration = 1.0f;
        float amim1_animationCurrentTime = 0.0f;
        //Start the animation loop
        while (amim1_animationCurrentTime < amim1_animationDuration)
        {
            //Update the elapsed time since animation start
            amim1_animationCurrentTime += (Time.time - amim1_animationLastTime);
            amim1_animationLastTime = Time.time;

            //Fix the animation current time
            if (amim1_animationCurrentTime > amim1_animationDuration)
                amim1_animationCurrentTime = amim1_animationDuration;

            //Calculate the positions
            Vector3 startPosition = new Vector3(microphoneWireEndTransform.position.x, (microphoneWireEndTransform.position.y + 30.0f), microphoneWireEndTransform.position.z);
            Vector3 endPosition = microphoneWireEndTransform.position;
            //Update the wire positions
            microphoneWireRenderer.SetPosition(0, startPosition);
            microphoneWireRenderer.SetPosition(1, endPosition);

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Clear the last workd spoken
        wordSpokenByTheUser = "";

        //Get the word to be speaked
        string wordToBeSpeaked = "";
        if (wordFormed == WordFormed.Primary)
            foreach (WordSyllabe syllabe in primaryWordSillabes)
                wordToBeSpeaked += syllabe.syllabe;
        if (wordFormed == WordFormed.Secondary)
            foreach (WordSyllabe syllabe in secondaryWordSillabes)
                wordToBeSpeaked += syllabe.syllabe;
        if (wordFormed == WordFormed.Tertiary)
            foreach (WordSyllabe syllabe in tertiaryWordSillabes)
                wordToBeSpeaked += syllabe.syllabe;

        //Prepare the speak button
        hudController.wordValidationSpeakButton.onClick.RemoveAllListeners();
        hudController.wordValidationSpeakButton.onClick.AddListener(() =>
        {
            //Change the UI
            hudController.wordValidationBeforeObj.SetActive(false);
            hudController.wordValidationSpeakingObj.SetActive(true);
            hudController.wordValidationErrorObj.SetActive(false);
            hudController.wordValidationSuccessObj.SetActive(false);

            //Show the speech ballon
            playerController.speechBallonObj.SetActive(true);

            //Reduce the level background music to 20%
            levelBackgroundMusic.volume = (defaultBgMusicVolume * 0.2f);

            //If is Android
            if (Application.platform == RuntimePlatform.Android)
            {
                //Prepare a callback
                NATEvents.onMicrophoneSpeechToTextFinished += (NAT.Microphone.SpeechToTextResult result, string textResult) =>
                {
                    if (result != NAT.Microphone.SpeechToTextResult.NoError)
                        wordSpokenByTheUser = "!E4500E!";
                    if (result == NAT.Microphone.SpeechToTextResult.NoError)
                        wordSpokenByTheUser = textResult;
                };

                //Start listening
                NAT.Microphone.StartListeningSpeechToText();
            }
            //If is not Android, wait a time and send a user response
            if (Application.platform != RuntimePlatform.Android)
            {
                if (editorTestingMode == EditorTestingMode.EmitErrorResponse)
                    StartCoroutine(WaitTimeAndSendFakeUserResponse(2.0f, "!E4500E!"));
                if (editorTestingMode == EditorTestingMode.EmitErrorWord)
                    StartCoroutine(WaitTimeAndSendFakeUserResponse(2.0f, "teste-errado"));
                if (editorTestingMode == EditorTestingMode.EmitSuccessWord)
                    StartCoroutine(WaitTimeAndSendFakeUserResponse(2.0f, wordToBeSpeaked));
            }
        });

        //Show thinking emote
        playerController.ShowEmote(PlayerController.EmoteType.Thinking);
        //Play the thinking sound
        questioningSound.PlayDelayed(0.45f);

    //Prepare a return point
    GoBackToStartOfResponseTask:

        //Prepare the speak interface
        hudController.wordValidationArrow.SetActive(false);
        if (wordFormed == WordFormed.Primary)
            hudController.wordValidationWordToSay.color = new Color(1.0f, 0.7568628f, 0.0f, 1.0f);
        if (wordFormed == WordFormed.Secondary)
            hudController.wordValidationWordToSay.color = new Color(0.2122642f, 0.635479f, 1.0f, 1.0f);
        if (wordFormed == WordFormed.Tertiary)
            hudController.wordValidationWordToSay.color = new Color(0.0f, 1.0f, 0.16855f, 1.0f);
        hudController.wordValidationWordToSay.text = wordToBeSpeaked.ToUpper();
        //Enable the speak interface
        hudController.wordValidationBeforeObj.SetActive(true);
        hudController.wordValidationSpeakingObj.SetActive(false);
        hudController.wordValidationErrorObj.SetActive(false);
        hudController.wordValidationSuccessObj.SetActive(false);
        //Wait time and show the arrow
        yield return new WaitForSeconds(2.5f);
        hudController.wordValidationArrow.SetActive(true);

        //Prepare the wait time
        WaitForSeconds responseWaitTime = new WaitForSeconds(0.5f);
        //Wait the user response
        while (string.IsNullOrEmpty(wordSpokenByTheUser) == true)
            yield return responseWaitTime;

        //Wait time before continue
        yield return new WaitForSeconds(1.0f);

        //Hide the speech ballon
        playerController.speechBallonObj.SetActive(false);

        //Changeo to default volume of background music
        levelBackgroundMusic.volume = defaultBgMusicVolume;

        //Check if is correct
        if (wordSpokenByTheUser.ToLower().Contains(wordToBeSpeaked.ToLower()) == false)
        {
            //Show sad emote
            int randomEmote = Random.Range(0, 2);
            if (randomEmote == 0)
                playerController.ShowEmote(PlayerController.EmoteType.Sad1);
            if (randomEmote == 1)
                playerController.ShowEmote(PlayerController.EmoteType.Sad2);
            //Play the sad sound
            worldResponseFailSound.PlayDelayed(0.05f);

            //Show the error reason
            string reasonOfError = "";
            if (wordSpokenByTheUser == "!E4500E!")
                reasonOfError = "Houve um problema com seu Microfone.\nVerifique se não há outro app o usando!";
            if (wordSpokenByTheUser != "!E4500E!")
            {
                reasonOfError = "Você disse \"" + wordSpokenByTheUser.ToUpper() + "\", mas o correto é \"" + wordToBeSpeaked.ToUpper() + "\"!";

                //Cause the damage
                StartCoroutine(CauseDamageAfterSomeTime());
            }
            if (errorReasonCoroutine != null)
                StopCoroutine(errorReasonCoroutine);
            errorReasonCoroutine = null;
            if (errorReasonCoroutine == null)
                errorReasonCoroutine = StartCoroutine(ShowSpeakErrorReason(reasonOfError));

            //Clear the last workd spoken
            wordSpokenByTheUser = "";

            //Enable the error interface
            hudController.wordValidationBeforeObj.SetActive(false);
            hudController.wordValidationSpeakingObj.SetActive(false);
            hudController.wordValidationErrorObj.SetActive(true);
            hudController.wordValidationSuccessObj.SetActive(false);

            //Wait time before go back to speaking screen
            yield return new WaitForSeconds(3.0f);

            //Go back to start of process
            goto GoBackToStartOfResponseTask;
        }
        if (wordSpokenByTheUser.ToLower().Contains(wordToBeSpeaked.ToLower()) == true)
        {
            //Show happy emote
            int randomEmote = Random.Range(0, 3);
            if (randomEmote == 0)
                playerController.ShowEmote(PlayerController.EmoteType.Smile1);
            if (randomEmote == 1)
                playerController.ShowEmote(PlayerController.EmoteType.Smile2);
            if (randomEmote == 2)
                playerController.ShowEmote(PlayerController.EmoteType.Smile3);
            //Play the happy sound
            worldResponseSuccessSound.PlayDelayed(0.02f);

            //Increase potions by one, if have less than 2 potions
            if (SaveGameManager.healthPotions < 2 && wordFormed != WordFormed.Primary)
                SaveGameManager.healthPotions += 1;

            //Decide the success mesage
            string successMessage = "";
            int randomMessage = Random.Range(0, 9);
            if (randomMessage == 0)
                successMessage = "Parabéns! Você acertou!";
            if (randomMessage == 1)
                successMessage = "É isso aí! Você acertou!";
            if (randomMessage == 2)
                successMessage = "Boa! Você acertou!";
            if (randomMessage == 3)
                successMessage = "Arrebentou! Você acertou!";
            if (randomMessage == 4)
                successMessage = "Matemááático! Você acertou!";
            if (randomMessage == 5)
                successMessage = "Algébrico! Você acertou!";
            if (randomMessage == 6)
                successMessage = "Alfazemaaa! Você acertou!";
            if (randomMessage == 7)
                successMessage = "Ótimo! Você acertou!";
            if (randomMessage == 8)
                successMessage = "ôôôôô! Você acertou!";

            //Prepare the success interface
            hudController.wordValidationSuccessMessage.text = successMessage.ToUpper();
            //Enable the success interface
            hudController.wordValidationBeforeObj.SetActive(false);
            hudController.wordValidationSpeakingObj.SetActive(false);
            hudController.wordValidationErrorObj.SetActive(false);
            hudController.wordValidationSuccessObj.SetActive(true);

            //If is forming the primary word, disable the potion button
            if (wordFormed == WordFormed.Primary)
                hudController.itemButton.gameObject.SetActive(false);

            //Wait time before put star
            yield return new WaitForSeconds(1.0f);

            //Put the star
            if (wordFormed == WordFormed.Primary)
            {
                hudController.star1Transform.position = hudController.star1PlaceTransform.position;
                hudController.star1Transform.gameObject.SetActive(true);
                primaryWordFormed = true;
                primaryStarReceived = true;
            }
            if (wordFormed == WordFormed.Secondary)
            {
                hudController.star2Transform.position = hudController.star2PlaceTransform.position;
                hudController.star2Transform.gameObject.SetActive(true);
                secondaryWordFormed = true;
                secondaryStarReceived = true;
            }
            if (wordFormed == WordFormed.Tertiary)
            {
                hudController.star3Transform.position = hudController.star3PlaceTransform.position;
                hudController.star3Transform.gameObject.SetActive(true);
                tertiaryWordFormed = true;
                tertiaryStarReceived = true;
            }
            starAcquieredSound.Play();

            //Wait time before continue
            yield return new WaitForSeconds(2.0f);

            //Disable the interfaces
            hudController.wordValidationBeforeObj.SetActive(false);
            hudController.wordValidationSpeakingObj.SetActive(false);
            hudController.wordValidationErrorObj.SetActive(false);
            hudController.wordValidationSuccessObj.SetActive(false);
        }

        //If is acquired the primary star and formed the primary star, skip to end of this coroutine
        if (primaryWordFormed == true && primaryStarReceived == true)
        {
            StartCoroutine(DoTransitionToEndOfLevel());
            goto EndThisCoroutine;
        }

        //Run push microphone animation
        microphoneAnimator.SetTrigger("exit");

        //Play the pull sound effect
        microphoneEntrySound.pitch = Random.Range(1.0f, 1.1f);
        microphoneEntrySound.PlayDelayed(0.12f);

        //Animate the wire of the microphone during the animation
        float amim2_animationLastTime = Time.time;
        float amim2_animationDuration = 1.0f;
        float amim2_animationCurrentTime = 0.0f;
        //Start the animation loop
        while (amim2_animationCurrentTime < amim2_animationDuration)
        {
            //Update the elapsed time since animation start
            amim2_animationCurrentTime += (Time.time - amim2_animationLastTime);
            amim2_animationLastTime = Time.time;

            //Fix the animation current time
            if (amim2_animationCurrentTime > amim2_animationDuration)
                amim2_animationCurrentTime = amim2_animationDuration;

            //Calculate the positions
            Vector3 startPosition = new Vector3(microphoneWireEndTransform.position.x, (microphoneWireEndTransform.position.y + 30.0f), microphoneWireEndTransform.position.z);
            Vector3 endPosition = microphoneWireEndTransform.position;
            //Update the wire positions
            microphoneWireRenderer.SetPosition(0, startPosition);
            microphoneWireRenderer.SetPosition(1, endPosition);

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Disable the microphone
        microphoneRootTransform.gameObject.SetActive(false);

        //Unlock the player from do anything
        playerController.isFreezedByLevelController = false;
        hudController.joystickAxis.gameObject.SetActive(true);
        hudController.attackButton.gameObject.SetActive(true);
        hudController.jumpButton.gameObject.SetActive(true);
        hudController.dashButton.gameObject.SetActive(true);
        hudController.pauseButtonObj.gameObject.SetActive(true);

    //Prepare a end point
    EndThisCoroutine:

        //Wait one frame before finishes
        yield return 0;
    }

    private IEnumerator DoTransitionToEndOfLevel()
    {
        //Save the game
        if (SaveGameManager.gameLevels != null && SaveGameManager.gameLevels.Length > 0 && SaveGameManager.gameLevels[0] != null)
        {
            SaveGameManager.gameLevels[levelId].star1 = true;
            if (SaveGameManager.gameLevels[levelId].star2 == false)
                SaveGameManager.gameLevels[levelId].star2 = secondaryStarReceived;
            if (SaveGameManager.gameLevels[levelId].star3 == false)
                SaveGameManager.gameLevels[levelId].star3 = tertiaryStarReceived;
            SaveGameManager.gameLevels[levelId].finished = true;
            SaveGameManager.SaveData();
        }

        //Wait a time before do the transition
        yield return new WaitForSeconds(2.0f);

        //Disable the level background music
        if (levelBackgroundMusic != null)
            if (levelBackgroundMusic.isPlaying == true)
                levelBackgroundMusic.Stop();
        //Play the sound
        finishSounds[Random.Range(0, finishSounds.Length)].Play();
        finishSoundLoop.PlayDelayed(8.0f);

        //Disable all the UI elements
        hudController.pauseScreenObj.SetActive(true);
        hudController.controlsObj.SetActive(false);
        hudController.pauseButtonObj.SetActive(false);
        hudController.coinsObj.SetActive(false);
        hudController.healthBarObj.SetActive(false);
        hudController.syllabesScreenObj.SetActive(false);
        hudController.starsContainerObj.SetActive(false);

        //Show the words
        string word1 = "";
        foreach (WordSyllabe syllabe in primaryWordSillabes)
            word1 += syllabe.syllabe;
        hudController.primaryWordDisplay.text = word1.ToUpper();
        string word2 = "";
        foreach (WordSyllabe syllabe in secondaryWordSillabes)
            word2 += syllabe.syllabe;
        hudController.secondaryWordDisplay.text = word2.ToUpper();
        string word3 = "";
        foreach (WordSyllabe syllabe in tertiaryWordSillabes)
            word3 += syllabe.syllabe;
        hudController.tertiaryWordDisplay.text = word3.ToUpper();

        //Show the time
        hudController.levelFinishElapsedTime.text = hudController.levelElapsedTime.text;

        //Show the finish screen
        hudController.finishScreenObj.SetActive(true);

        //Wait time before show the acquired stas
        yield return new WaitForSeconds(1.5f);

        //Fill the stars
        if (primaryStarReceived == true)
        {
            hudController.primaryStar.SetActive(true);
            syllabeAcquieredSound.pitch = 1.0f;
            syllabeAcquieredSound.Play();
            yield return new WaitForSeconds(1.0f);
        }
        if (secondaryStarReceived == true)
        {
            hudController.secondaryStar.SetActive(true);
            syllabeAcquieredSound.pitch = 1.1f;
            syllabeAcquieredSound.Play();
            yield return new WaitForSeconds(1.0f);
        }
        if (tertiaryStarReceived == true)
        {
            hudController.tertiaryStar.SetActive(true);
            syllabeAcquieredSound.pitch = 1.2f;
            syllabeAcquieredSound.Play();
            yield return new WaitForSeconds(1.0f);
        }

        //Wait more time
        yield return new WaitForSeconds(1.0f);

        //Enable the menu button and allow the stars jumping
        hudController.finishScreenAnimator.SetBool("doStarsEffect", true);
        hudController.finishGoToMenuButton.onClick.AddListener(() =>
        {
            //Go to menu
            hudController.clickSound.Play();
            hudController.finishGoToMenuButton.gameObject.SetActive(false);

            //Start loading the menu
            GameObject sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader");
            sceneLoader.GetComponent<SceneLoader>().LoadSceneByName("Menu");
        });
        hudController.finishGoToMenuButton.gameObject.SetActive(true);
    }

    //Public methods

    public void NotifySyllabeAcquired(SyllabeType syllabeType, string acquiredSyllabe, Transform spiderTransform)
    {
        //Prepare the storage for the syllabe text slot
        RectTransform syllabeUiTextSlotTransform = null;

        //Inform that is acquired a syllabe
        if (syllabeType == SyllabeType.Primary)
            foreach (WordSyllabe syllabe in primaryWordSillabes)
                if (syllabe.syllabe == acquiredSyllabe)
                {
                    syllabeUiTextSlotTransform = syllabe.syllabeSlot;
                    syllabe.acquired = true;
                }
        if (syllabeType == SyllabeType.Secondary)
            foreach (WordSyllabe syllabe in secondaryWordSillabes)
                if (syllabe.syllabe == acquiredSyllabe)
                {
                    syllabeUiTextSlotTransform = syllabe.syllabeSlot;
                    syllabe.acquired = true;
                }
        if (syllabeType == SyllabeType.Tertiary)
            foreach (WordSyllabe syllabe in tertiaryWordSillabes)
                if (syllabe.syllabe == acquiredSyllabe)
                {
                    syllabeUiTextSlotTransform = syllabe.syllabeSlot;
                    syllabe.acquired = true;
                }

        //Start the animation of syllabe placing
        StartCoroutine(SyllabePlacing(syllabeType, acquiredSyllabe, syllabeUiTextSlotTransform, spiderTransform));

        //Check if any word was formed
        //Primary
        int syllabesOfPrimaryWord = 0;
        foreach (WordSyllabe syllabe in primaryWordSillabes)
            if (syllabe.acquired == true)
                syllabesOfPrimaryWord += 1;
        if (syllabesOfPrimaryWord >= primaryWordSillabes.Length)
            primaryWordFormed = true;
        //Secondary
        int syllabesOfSecondaryWord = 0;
        foreach (WordSyllabe syllabe in secondaryWordSillabes)
            if (syllabe.acquired == true)
                syllabesOfSecondaryWord += 1;
        if (syllabesOfSecondaryWord >= secondaryWordSillabes.Length)
            secondaryWordFormed = true;
        //Tertiary
        int syllabesOfTertiaryWord = 0;
        foreach (WordSyllabe syllabe in tertiaryWordSillabes)
            if (syllabe.acquired == true)
                syllabesOfTertiaryWord += 1;
        if (syllabesOfTertiaryWord >= tertiaryWordSillabes.Length)
            tertiaryWordFormed = true;

        //If is formed a word, that not received star, start the input receiving task to validate the word
        if (primaryWordFormed == true && primaryStarReceived == false)
            StartCoroutine(MicrophoneInputReceiver(WordFormed.Primary));
        if (secondaryWordFormed == true && secondaryStarReceived == false)
            StartCoroutine(MicrophoneInputReceiver(WordFormed.Secondary));
        if (tertiaryWordFormed == true && tertiaryStarReceived == false)
            StartCoroutine(MicrophoneInputReceiver(WordFormed.Tertiary));
    }
}