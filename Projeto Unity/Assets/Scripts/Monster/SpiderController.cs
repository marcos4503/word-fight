using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SpiderController : MonoBehaviour
{
    //Cache variables
    private Material materialInstanceOfSpider;
    private WaitForSeconds damageFlashDuration = new WaitForSeconds(0.15f);
    private PlayerController playerController;
    private Transform playerTransform;
    private Coroutine spiderAiBehaviourCoroutine;
    private Coroutine spiderDistanceCalculatorCoroutine;
    private Coroutine damageFlashCoroutine;
    private WaitForSeconds damageDashDuration = new WaitForSeconds(0.15f);
    private WaitForSeconds timeToMoveAfterDamageDash = new WaitForSeconds(0.7f);
    private Coroutine removeDamageDashCoroutine;
    private Vector3 receivedDamageOrigin = Vector3.zero;
    private Coroutine attackIntervalTimer;
    private WaitForSeconds attackInterval = new WaitForSeconds(2.0f);
    private Coroutine disableSpiderCoroutine = null;
    private Color defaultMaterialColorOfSpider;

    //Public enums
    public enum SyllabeType
    {
        None,
        Primary,
        Secondary,
        Tertiary
    }
    public enum MonsterState
    {
        Idle,
        Attack,
        Dead
    }
    public enum MonsterFacingTo
    {
        Left,
        Right
    }

    //Public variables
    [Header("Level Controller")]
    public LevelController levelController;
    [Space(8)]
    [Header("Monster Syllabe")]
    public SyllabeType monsterSyllabeType = SyllabeType.None;
    public string monsterSyllabe = "";
    public Color syllabeColor = Color.white;
    public Color syllabeBrightColor1 = Color.white;
    public Color syllabeBrightColor2 = Color.white;
    public Color syllabeStarColor1 = Color.white;
    public Color syllabeStarColor2 = Color.white;
    public Color syllabeGetColor1 = Color.white;
    public Color syllabeGetColor2 = Color.white;
    public Texture2D spiderPrimaryTexture = null;
    public Texture2D spiderSecondaryTexture = null;
    public Texture2D spiderTertiaryTexture = null;
    public Color spiderDamageColor = Color.white;
    [Space(8)]
    [Header("Monster Status")]
    public MonsterState monsterState = MonsterState.Idle;
    public float maxHealth = 100.0f;
    public float currentHealth = 100.0f;
    public Vector3 targetPosition = Vector3.zero;
    public float distanceToTargetPosition = 0.0f;
    public MonsterFacingTo facingTo = MonsterFacingTo.Right;
    public float movementSpeed = 10.0f;
    public bool isOnDamageDash = false;
    public bool canMoveAfterDamageDash = true;
    public bool isPlayerInsideArea = false;
    public bool canAttackNow = true;
    public bool isAttackingNow = false;
    public float damageToCause = 8.0f;
    [Space(8)]
    [Header("Monster Components")]
    public Transform spiderAreaStart;
    public Transform spiderAreaEnd;
    public Material materialToUse;
    public SkinnedMeshRenderer spiderMeshRenderer;
    public Transform spiderTransform;
    public Transform spiderModelTransform;
    public Rigidbody spiderRigidbody;
    public Animator spiderAnimator;
    public Animator spiderKnockbackAnimator;
    public ParticleSystem damageParticles;
    public GameObject spiderCanvas;
    public Slider spiderHpBar;
    public AudioSource walkSound;
    public GameObject eyeTrails;
    public AudioSource[] attackSound;
    public GameObject attackHitbox;
    public ParticleSystem prepareAttackParticles;
    public CapsuleCollider bodyHitbox0;
    public SphereCollider bodyHitbox1;
    public AudioSource[] deathSound;
    public BoxCollider bodyHitbox2;
    public ParticleSystem deathParticles;
    public SyllabeDisplay syllabeDisplay;
    public ParticleSystem syllabe0Particles;
    public ParticleSystem syllabe1Particles;
    public ParticleSystem syllabeGetParticles;

    //Core methods

    void Start()
    {
        //If is running in Editor
        if (Application.isPlaying == false)
        {
            //Cancel
            return;
        }

        //Create a instance of the material for the spider, if don't have a instanced material
        if (materialInstanceOfSpider == null)
            materialInstanceOfSpider = Instantiate(materialToUse);
        materialInstanceOfSpider.name = (materialInstanceOfSpider.name + "(Instance)");

        //Put it in the mesh
        spiderMeshRenderer.sharedMaterials = new Material[] { materialInstanceOfSpider };

        //Disable the emission, now that have the cache
        materialInstanceOfSpider.DisableKeyword("_EMISSION");
        materialInstanceOfSpider.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

        //Find a player reference
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerGameObject.transform;
        playerController = playerGameObject.GetComponent<PlayerController>();

        //Start the spider behaviour
        spiderAiBehaviourCoroutine = StartCoroutine(SpiderAiBehaviourLoop());
        //Start the spider distance calculator
        spiderDistanceCalculatorCoroutine = StartCoroutine(SpiderAiDistanceToTargetPositionLoop());

        //Hide the demarcations
        spiderAreaStart.GetComponent<SpriteRenderer>().enabled = false;
        spiderAreaEnd.GetComponent<SpriteRenderer>().enabled = false;

        //Setup the syllabe colors
        if (monsterSyllabeType == SyllabeType.Primary)
        {
            syllabeColor = new Color(0.945098f, 0.7972597f, 0.0f, 1.0f);
            syllabeBrightColor1 = new Color(1.0f, 0.631401f, 0.0990566f, 0.8f);
            syllabeBrightColor2 = new Color(0.8962264f, 0.6139151f, 0.0f, 0.6745098f);
            syllabeStarColor1 = new Color(1.0f, 0.9773819f, 0.0f, 0.8705882f);
            syllabeStarColor2 = new Color(0.8396226f, 0.7999347f, 0.0f, 0.8352941f);
            syllabeGetColor1 = new Color(0.945098f, 0.7960784f, 0.0f, 1.0f);
            syllabeGetColor2 = new Color(0.735849f, 0.619823f, 0.0f, 1.0f);
            spiderDamageColor = new Color(1.0f, 0.2509804f, 0.2509804f, 1.0f);
        }
        if (monsterSyllabeType == SyllabeType.Secondary)
        {
            syllabeColor = new Color(0.0f, 0.5372548f, 1.0f, 1.0f);
            syllabeBrightColor1 = new Color(0.5330188f, 0.7846342f, 1.0f, 0.7882353f);
            syllabeBrightColor2 = new Color(0.3545746f, 0.6506481f, 0.9056604f, 0.8078431f);
            syllabeStarColor1 = new Color(0.495283f, 0.7664446f, 1.0f, 0.9098039f);
            syllabeStarColor2 = new Color(0.0f, 0.5372549f, 1.0f, 0.9568627f);
            syllabeGetColor1 = new Color(0.0f, 0.5271179f, 0.9811321f, 1.0f);
            syllabeGetColor2 = new Color(0.0f, 0.3852016f, 0.7169812f, 1.0f);
            spiderDamageColor = new Color(1.0f, 0.4196078f, 0.4196078f, 1.0f);
        }
        if (monsterSyllabeType == SyllabeType.Tertiary)
        {
            syllabeColor = new Color(0.0f, 0.5f, 0.02013421f, 1.0f);
            syllabeBrightColor1 = new Color(0.0f, 0.7264151f, 0.02837559f, 0.8352941f);
            syllabeBrightColor2 = new Color(0.2688679f, 1.0f, 0.2974277f, 0.8588235f);
            syllabeStarColor1 = new Color(0.03671236f, 0.7075472f, 0.06291685f, 0.8078431f);
            syllabeStarColor2 = new Color(0.0f, 0.5f, 0.02046788f, 0.8078431f);
            syllabeGetColor1 = new Color(0.0f, 0.5019608f, 0.01960784f, 1.0f);
            syllabeGetColor2 = new Color(0.0f, 0.3113208f, 0.01216097f, 1.0f);
            spiderDamageColor = new Color(1.0f, 0.4745098f, 0.4745098f, 1.0f);
        }

        //Define the spider texture
        if (monsterSyllabeType == SyllabeType.Primary)
            materialInstanceOfSpider.mainTexture = spiderPrimaryTexture;
        if (monsterSyllabeType == SyllabeType.Secondary)
            materialInstanceOfSpider.mainTexture = spiderSecondaryTexture;
        if (monsterSyllabeType == SyllabeType.Tertiary)
            materialInstanceOfSpider.mainTexture = spiderTertiaryTexture;

        //Make the syllabe show, if have a syllabe
        if (monsterSyllabe != "")
            syllabeDisplay.BuildAndShowSyllabe(monsterSyllabe, syllabeColor);
        //If don't have a syllabe, ignore
        if (monsterSyllabe == "")
        {
            syllabeDisplay.gameObject.SetActive(false);
            syllabe0Particles.gameObject.SetActive(false);
            syllabe1Particles.gameObject.SetActive(false);
        }

        //Setup the syllabe platicles color
        ParticleSystem.MainModule syllabe0 = syllabe0Particles.main;
        syllabe0.startColor = new ParticleSystem.MinMaxGradient(syllabeBrightColor1, syllabeBrightColor2);
        ParticleSystem.MainModule syllabe1 = syllabe1Particles.main;
        syllabe1.startColor = new ParticleSystem.MinMaxGradient(syllabeStarColor1, syllabeStarColor2);
        ParticleSystem.MainModule syllabeGet = syllabeGetParticles.main;
        syllabeGet.startColor = new ParticleSystem.MinMaxGradient(syllabeGetColor1, syllabeGetColor2);
        if (syllabe0Particles.isPlaying == false)
            syllabe0Particles.Stop();
        syllabe0Particles.Play();
        if (syllabe1Particles.isPlaying == false)
            syllabe1Particles.Stop();
        syllabe1Particles.Play();
    }

    void Update()
    {
        //If is running in Editor
        if (Application.isPlaying == false)
        {
            if (spiderAreaStart.localPosition.x > -1.0f)
                spiderAreaStart.localPosition = new Vector3(-1.0f, 0.0f, 0.0f);
            if (spiderAreaEnd.localPosition.x < 1.0f)
                spiderAreaEnd.localPosition = new Vector3(1.0f, 0.0f, 0.0f);

            //Cancel
            return;
        }
        //If is dead, cancel
        if (monsterState == MonsterState.Dead)
            return;

        //Run needed functions
        RotateToTargetPosition();
        PlayerDeathChecker();
    }

    private void RotateToTargetPosition()
    {
        //Rotate the spider to the looking direction
        if (facingTo == MonsterFacingTo.Right)
            spiderModelTransform.localEulerAngles = Vector3.Lerp(spiderModelTransform.localEulerAngles, new Vector3(0, 0, 0), 24 * Time.deltaTime);
        if (facingTo == MonsterFacingTo.Left)
            spiderModelTransform.localEulerAngles = Vector3.Lerp(spiderModelTransform.localEulerAngles, new Vector3(0, 180, 0), 24 * Time.deltaTime);
    }

    void FixedUpdate()
    {
        //If is running in Editor
        if (Application.isPlaying == false)
        {
            //Cancel
            return;
        }
        //If is dead, cancel
        if (monsterState == MonsterState.Dead)
            return;

        //Run needed functions
        MoveToTargetPosition();
        PlayerInsideAreaDetector();
    }

    private void MoveToTargetPosition()
    {
        //If is on a damage dash, move the spider
        if (isOnDamageDash == true)
        {
            //If the player is on left...
            if (receivedDamageOrigin.x < spiderTransform.position.x && spiderTransform.position.x < spiderAreaEnd.position.x)
                spiderRigidbody.MovePosition(spiderTransform.position + (new Vector3(1.0f, 0.0f, 0.0f) * playerController.attackImpulseSpeed * Time.deltaTime));
            //If the player is on right...
            if (receivedDamageOrigin.x >= spiderTransform.position.x && spiderTransform.position.x >= spiderAreaStart.position.x)
                spiderRigidbody.MovePosition(spiderTransform.position + (new Vector3(-1.0f, 0.0f, 0.0f) * playerController.attackImpulseSpeed * Time.deltaTime));
        }

        //If the distance for the destination is less than 5 meters, OR is on a dash damage OR can't move because a damage dash OR is attacking OR is dead, don't move
        if (distanceToTargetPosition <= 5 || isOnDamageDash == true || canMoveAfterDamageDash == false || isAttackingNow == true || monsterState == MonsterState.Dead)
        {
            spiderAnimator.SetBool("walk", false);
            if (walkSound.isPlaying == true)
                walkSound.Stop();
            return;
        }

        //Find the direction for the target position
        if (targetPosition.x < spiderTransform.position.x)
            facingTo = MonsterFacingTo.Left;
        if (targetPosition.x >= spiderTransform.position.x)
            facingTo = MonsterFacingTo.Right;

        //Move the spider
        if (facingTo == MonsterFacingTo.Left)
            spiderRigidbody.MovePosition(spiderTransform.position + (new Vector3(-1.0f, 0.0f, 0.0f) * movementSpeed * Time.deltaTime));
        if (facingTo == MonsterFacingTo.Right)
            spiderRigidbody.MovePosition(spiderTransform.position + (new Vector3(1.0f, 0.0f, 0.0f) * movementSpeed * Time.deltaTime));

        //Enable the walk animation
        spiderAnimator.SetBool("walk", true);
        if (walkSound.isPlaying == false)
            walkSound.Play();
    }

    private void PlayerInsideAreaDetector()
    {
        //Get the current player position
        Vector3 playerCurrenPosition = playerTransform.position;

        //Check if the player is inside this spider area
        if (playerCurrenPosition.x >= spiderAreaStart.position.x && playerCurrenPosition.x < spiderAreaEnd.position.x && playerCurrenPosition.y >= (spiderAreaStart.position.y - 1.0f) && playerCurrenPosition.y < (spiderAreaStart.position.y + 1.0f))
        {
            eyeTrails.SetActive(true);
            isPlayerInsideArea = true;
        }
        else
        {
            eyeTrails.SetActive(false);
            isPlayerInsideArea = false;
        }
    }

    private void PlayerDeathChecker()
    {
        //If the player is dead, disable the spider without kill it
        if (playerController.currentHealth <= 0.0f)
            if (disableSpiderCoroutine == null)
                disableSpiderCoroutine = StartCoroutine(DisableTheSpider(false));
    }

    private IEnumerator SpiderAiDistanceToTargetPositionLoop()
    {
        //Prepare the loop data
        WaitForSeconds countInterval = new WaitForSeconds(0.1f);  //Run at 10 FPS

        //Create the loop of distance counter
        while (true)
        {
            //Calculate the distance between spider and the target position
            distanceToTargetPosition = Vector3.Distance(spiderTransform.position, targetPosition);

            //Wait the interval
            yield return countInterval;
        }
    }

    private IEnumerator SpiderAiBehaviourLoop()
    {
        //Change to correct spider state
        monsterState = MonsterState.Idle;

        //Prepare the AI data
        WaitForSeconds aiLoopInterval = new WaitForSeconds(0.08f);  //Run at 12.5 FPS
        int lastDirectionToWalkWhileIdle = Random.Range(0, 2);
        float preMeleeDodgeWindowIntervalTime = 0.7f;
        WaitForSeconds preMeleeDodgeWindowInterval = new WaitForSeconds(preMeleeDodgeWindowIntervalTime);
        WaitForSeconds preMeleeAttackDelayInterval = new WaitForSeconds(0.1f);
        WaitForSeconds meleeAttackDelayInterval = new WaitForSeconds(0.2f);
        WaitForSeconds postMeleeAttackDelayInterval = new WaitForSeconds(0.7f);

        //Start the AI loop
        while (true)
        {
            //-------------------------------------------- IDLE --------------------------------------------//

            //If is in idle...
            if (monsterState == MonsterState.Idle)
            {
                //If is closer to the destination point, decide the next point
                if (distanceToTargetPosition <= 5)
                {
                    if (lastDirectionToWalkWhileIdle == 1)
                    {
                        lastDirectionToWalkWhileIdle = 0;
                        targetPosition = spiderAreaStart.position;
                        goto AlreadyDecidedDestination;
                    }
                    if (lastDirectionToWalkWhileIdle == 0)
                    {
                        lastDirectionToWalkWhileIdle = 1;
                        targetPosition = spiderAreaEnd.position;
                        goto AlreadyDecidedDestination;
                    }
                }

            //Continue
            AlreadyDecidedDestination:;

                //If the player is inside the monster area, change to attack
                if (isPlayerInsideArea == true)
                    monsterState = MonsterState.Attack;
            }

            //-------------------------------------------- ATTACK --------------------------------------------//

            //If is in attack...
            if (monsterState == MonsterState.Attack)
            {
                //Make spider follow the player
                targetPosition = playerTransform.position;

                //If can attack now, attack
                if (canAttackNow == true && isAttackingNow == false && Vector3.Distance(playerTransform.position, spiderTransform.position) <= 6.5f)
                {
                    //Inform that is attacking
                    isAttackingNow = true;

                    //Inform to rotate the spider to correct direction
                    if (targetPosition.x < spiderTransform.position.x)
                    {
                        facingTo = MonsterFacingTo.Left;
                        spiderModelTransform.localEulerAngles = new Vector3(0, 180, 0);
                    }
                    if (targetPosition.x >= spiderTransform.position.x)
                    {
                        facingTo = MonsterFacingTo.Right;
                        spiderModelTransform.localEulerAngles = new Vector3(0, 0, 0);
                    }

                    //Prepare the attack
                    spiderAnimator.SetTrigger("attackStart");
                    ParticleSystem.MainModule main = prepareAttackParticles.main;
                    main.simulationSpeed = (1.0f / preMeleeDodgeWindowIntervalTime) * 1.0f;
                    prepareAttackParticles.Play();

                    //Wait the dodge window time
                    yield return preMeleeDodgeWindowInterval;

                    //Run the attack animation
                    spiderAnimator.SetTrigger("attack");
                    attackSound[Random.Range(0, attackSound.Length)].Play();

                    //Wait the attack animation time
                    yield return preMeleeAttackDelayInterval;
                    attackHitbox.SetActive(true);
                    yield return meleeAttackDelayInterval;
                    attackHitbox.SetActive(false);
                    yield return postMeleeAttackDelayInterval;

                    //Start the coroutine to restore attack after a time
                    if (attackIntervalTimer != null)
                        StopCoroutine(attackIntervalTimer);
                    attackIntervalTimer = null;
                    attackIntervalTimer = StartCoroutine(AttackIntervalTimer());

                    //Inform that is not attacking more
                    isAttackingNow = false;
                    //Inform that can't attack now
                    canAttackNow = false;
                }

                //If the player is not inside the monster area, change to idle
                if (isPlayerInsideArea == false)
                    monsterState = MonsterState.Idle;
            }

            //Wait the interval
            yield return aiLoopInterval;
        }
    }

    private IEnumerator MakeDamageEffectOfRedFlash()
    {
        //Enable the damage flash
        materialInstanceOfSpider.EnableKeyword("_EMISSION");
        materialInstanceOfSpider.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        defaultMaterialColorOfSpider = materialInstanceOfSpider.color;
        materialInstanceOfSpider.color = spiderDamageColor;

        //Wait time before remove flash effect
        yield return damageFlashDuration;

        //Disable the damage flash
        materialInstanceOfSpider.DisableKeyword("_EMISSION");
        materialInstanceOfSpider.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        materialInstanceOfSpider.color = defaultMaterialColorOfSpider;

        //Clear this coroutine
        damageFlashCoroutine = null;
    }

    private IEnumerator RemoveDamageDashImpulse()
    {
        //Wait time before remove damage dash impulse
        yield return damageDashDuration;

        //Remove the dash impulse
        isOnDamageDash = false;

        //Wait time to move after damage dash
        yield return timeToMoveAfterDamageDash;

        //Inform that can move now
        canMoveAfterDamageDash = true;

        //Clear this coroutine
        removeDamageDashCoroutine = null;
    }

    private IEnumerator AttackIntervalTimer()
    {
        //Wait the time to restore the attack
        yield return attackInterval;

        //Inform that can attack
        canAttackNow = true;

        //Clear this coroutine
        attackIntervalTimer = null;
    }

    private IEnumerator DisableTheSpider(bool killSpider)
    {
        //If is killing...
        if (killSpider == true)
        {
            //Acquire the syllabe
            if (monsterSyllabe != "")
            {
                syllabe0Particles.gameObject.SetActive(false);
                syllabe1Particles.gameObject.SetActive(false);
                syllabeGetParticles.Play();
                syllabeDisplay.gameObject.SetActive(false);
                if (monsterSyllabeType == SyllabeType.Primary)
                    levelController.NotifySyllabeAcquired(LevelController.SyllabeType.Primary, monsterSyllabe, spiderModelTransform);
                if (monsterSyllabeType == SyllabeType.Secondary)
                    levelController.NotifySyllabeAcquired(LevelController.SyllabeType.Secondary, monsterSyllabe, spiderModelTransform);
                if (monsterSyllabeType == SyllabeType.Tertiary)
                    levelController.NotifySyllabeAcquired(LevelController.SyllabeType.Tertiary, monsterSyllabe, spiderModelTransform);
            }

            //Run death animation
            spiderAnimator.SetTrigger("deathStart");
            deathSound[Random.Range(0, deathSound.Length)].Play();
            deathParticles.Play();
        }

        //Cancel all coroutines
        if (spiderAiBehaviourCoroutine != null)
            StopCoroutine(spiderAiBehaviourCoroutine);
        if (spiderDistanceCalculatorCoroutine != null)
            StopCoroutine(spiderDistanceCalculatorCoroutine);
        if (damageFlashCoroutine != null)
            StopCoroutine(damageFlashCoroutine);
        if (removeDamageDashCoroutine != null)
            StopCoroutine(removeDamageDashCoroutine);
        if (attackIntervalTimer != null)
            StopCoroutine(attackIntervalTimer);
        if (attackIntervalTimer != null)
            StopCoroutine(attackIntervalTimer);

        //Inform that is dead
        monsterState = MonsterState.Dead;

        //Stop walking
        spiderAnimator.SetBool("walk", false);

        //Rotate the spider model to a random direction
        spiderModelTransform.localEulerAngles = new Vector3(0, Random.Range(15, 145), 0);

        //Disable hitboxes
        bodyHitbox0.enabled = false;
        bodyHitbox1.enabled = false;
        bodyHitbox2.enabled = false;

        //Reset all stats
        targetPosition = spiderTransform.position;
        isOnDamageDash = false;
        canMoveAfterDamageDash = true;
        isPlayerInsideArea = false;
        canAttackNow = true;
        isAttackingNow = false;

        //Disable the emission, now that have the cache
        materialInstanceOfSpider.DisableKeyword("_EMISSION");
        materialInstanceOfSpider.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        //Change back to default color of the material
        if (defaultMaterialColorOfSpider != Color.clear)
            materialInstanceOfSpider.color = defaultMaterialColorOfSpider;

        //Disable the canvas
        spiderCanvas.SetActive(false);
        //Stop walk sound
        if (walkSound.isPlaying == true)
            walkSound.Stop();
        //Disble eye trails
        eyeTrails.SetActive(false);
        //Disable attack hitbox
        attackHitbox.SetActive(false);

        //If is killed, wait for some seconds and disable this entire spider
        if (killSpider == true)
        {
            yield return new WaitForSeconds(5.0f);
            spiderAnimator.SetTrigger("deathEnd");
            yield return new WaitForSeconds(2.8f);
            this.gameObject.SetActive(false);
        }

        //Return null
        yield return null;
    }

    //Public methods

    public void CauseDamage(float damageToCause, Vector3 damageOriginPosition)
    {
        //Reduce the HP of the spider
        currentHealth -= damageToCause;

        //Show the HP of spider
        spiderCanvas.SetActive(true);
        spiderHpBar.maxValue = maxHealth;
        spiderHpBar.value = currentHealth;

        //Do the knockback animation
        spiderKnockbackAnimator.SetTrigger("doKnockback");

        //Inform the damage origin position
        receivedDamageOrigin = damageOriginPosition;

        //Start coroutine of damage flash
        if (damageFlashCoroutine != null)
            StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = null;
        damageFlashCoroutine = StartCoroutine(MakeDamageEffectOfRedFlash());

        //Inform that is on damage dash
        isOnDamageDash = true;
        //Inform that can't move because the damage dash
        canMoveAfterDamageDash = false;

        //Start coroutine to remove damage dash impulse
        if (removeDamageDashCoroutine != null)
            StopCoroutine(removeDamageDashCoroutine);
        removeDamageDashCoroutine = null;
        removeDamageDashCoroutine = StartCoroutine(RemoveDamageDashImpulse());

        //Play the damage particles
        damageParticles.Play();

        //If the health is equal or minor than zero, run death
        if (currentHealth <= 0.0f)
            if (disableSpiderCoroutine == null)
                disableSpiderCoroutine = StartCoroutine(DisableTheSpider(true));
    }
}