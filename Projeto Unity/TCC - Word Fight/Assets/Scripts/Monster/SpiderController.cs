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

    //Public enums
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
                spiderRigidbody.MovePosition(new Vector3((spiderTransform.position.x + (playerController.attackImpulseSpeed * Time.deltaTime)), 0, 0));
            //If the player is on right...
            if (receivedDamageOrigin.x >= spiderTransform.position.x && spiderTransform.position.x >= spiderAreaStart.position.x)
                spiderRigidbody.MovePosition(new Vector3((spiderTransform.position.x - (playerController.attackImpulseSpeed * Time.deltaTime)), 0, 0));
        }

        //If the distance for the destination is less than 5 meters, OR is on a dash damage OR can't move because a damage dash OR is attacking, don't move
        if (distanceToTargetPosition <= 5 || isOnDamageDash == true || canMoveAfterDamageDash == false || isAttackingNow == true)
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
            spiderRigidbody.MovePosition(new Vector3((spiderTransform.position.x - (movementSpeed * Time.deltaTime)), 0, 0));
        if (facingTo == MonsterFacingTo.Right)
            spiderRigidbody.MovePosition(new Vector3((spiderTransform.position.x + (movementSpeed * Time.deltaTime)), 0, 0));

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

        //Wait time before remove flash effect
        yield return damageFlashDuration;

        //Disable the damage flash
        materialInstanceOfSpider.DisableKeyword("_EMISSION");
        materialInstanceOfSpider.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;

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
        //If is killing, run death animation
        if (killSpider == true)
        {
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
            yield return new WaitForSeconds(5.0f);
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