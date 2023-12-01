using System.Collections;
using System.Collections.Generic;
using MTAssets.MobileInputControls;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Private constants
    private const int VOID_LAYER = 7;
    private const int WATER_LAYER = 8;

    //Private variables
    private WaitForSeconds attackImpulseDuration = new WaitForSeconds(0.15f);
    private WaitForSeconds attackAnimationResetTimer = new WaitForSeconds(1.0f);
    private WaitForSeconds attackDuration;
    private Coroutine attackCoroutine;
    private Coroutine attackAnimationResetCoroutine;
    private WaitForSeconds damageImpulseDuration = new WaitForSeconds(0.35f);
    private Coroutine damageThrowCoroutine;
    private Coroutine jump1ImpulseCoroutine;
    private Coroutine jump2ImpulseCoroutine;
    private Coroutine dashImpulseCoroutine;
    private Coroutine climbCoroutine;
    private Coroutine releaseButtonCoroutine;
    private Coroutine swimingEffectsCoroutine;

    //Public enums
    public enum CurrentlyFacingTo
    {
        Left,
        Right
    }
    public enum DamageType
    {
        VoidFall,
        VoidFallWithDeath,
        MonsterAttack
    }
    public enum DamageThrowTo
    {
        None,
        Right,
        Left
    }
    public enum AnimateThrowTo
    {
        None,
        Front,
        Back
    }

    //Cache variables
    private bool fallEnded = true;
    private int currentAttackAnimation = 0;
    private Vector3 startingPosition;
    RigidbodyInterpolation defaultRbInterpolation = RigidbodyInterpolation.None;
    CollisionDetectionMode defaultRbCollision = CollisionDetectionMode.Discrete;
    private float lastHeathPoints = -1.0f;
    private int lastHealthPotionsCount = -1;

    //Public variables
    [Header("Status")]
    public CurrentlyFacingTo currentlyFacingTo;
    public float maxHealth = 100.0f;
    public float currentHealth = 100.0f;
    public float movementSpeed = 16.0f;
    public bool isOnGround = false;
    public bool isOnWater = false;
    public bool isRunning = false;
    public bool isOnJump1Impulse = false;
    public bool isOnJump2Impulse = false;
    public bool primeJumpReady = true;
    public bool doubleJumpReady = true;
    public bool isOnDashImpulse = false;
    public float dashCooldown = 8.0f;
    public float dashTimer = 0.0f;
    public float dashImpulseSpeed = 24.0f;
    public float dashImpulseTime = 0.35f;
    public bool isAttacking = false;
    public float attackCooldown = 0.75f;
    public float attackTimer = 0.75f;
    public bool isOnAttackImpulse = false;
    public float attackImpulseSpeed = 8.0f;
    public bool isFallingToVoid = false;
    public float hpLossOnFallIntoVoid = 10.0f;
    public bool cameraFollowInstantly = false;
    public float damageThrowImpulseSpeed = 8.0f;
    public DamageThrowTo isOnDamageThrow = DamageThrowTo.None;
    public bool isDead = false;
    public bool isClimbingGroundEdge = false;
    public bool isClimbingReady = true;
    public bool canReceiveDamageFromUpMonster = true;
    public float damageToCause = 5.0f;
    public bool canUseHealthPotion = false;
    public float healthPotionHeal = 20.0f;
    public float healthPotionMaxCooldown = 10.0f;
    public float healthPotionCurrentTimer = 10.0f;
    [Space(8)]
    [Header("Player Components")]
    public Transform thisTransform;
    public Rigidbody playerRigidbody;
    public CapsuleCollider playerHitbox;
    public Animator playerAnimator;
    public Transform playerModelTransform;
    public LayerMask groundCheckLayer;
    public Transform groundCheckRaycast;
    public AudioSource[] fallSound;
    public ParticleSystem fallParticles;
    public AudioSource[] jumpSound;
    public ParticleSystem jumpParticles;
    public AudioSource[] effortSound;
    public AudioSource[] dashSound;
    public ParticleSystem dashParticles;
    public AudioSource[] swordMoveSound;
    public AudioSource[] swordHitSound;
    public GameObject swordOnBack;
    public GameObject swordOnHand;
    public Transform swordOnHandTransform;
    public ParticleSystem slashParticles;
    public AudioSource[] hurtSound;
    public AudioSource[] deathSound;
    public AudioSource blowSound;
    public ParticleSystem blowParticles;
    public Transform playerDetectors;
    public Transform groundEdgeDetectorOrigin;
    public Transform groundEdgeDistanceOrigin;
    public ParticleSystem edgeDirtParticles;
    public Transform hitHeadDetectorOrigin;
    public Transform hitDashDetectorOrigin;
    public AudioSource[] waterSplashSound;
    public ParticleSystem waterSplashParticles;
    public AudioSource[] swimSound;
    public ParticleSystem waterExitParticles;
    public GameObject attackHitbox;
    public LayerMask monsterCheckLayers;
    public AudioSource healSound;
    public ParticleSystem healParticles;
    [Space(8)]
    [Header("UI Components")]
    public HudController hudController;

    //Core methods

    void Start()
    {
        //Prepare UI control buttons
        hudController.jumpButton.onButtonDown.AddListener(() =>
        {
            //Repass the command of jump to the method
            Jumping(true);
        });
        hudController.dashButton.onButtonDown.AddListener(() =>
        {
            //Repass the command of dash to the method
            Dashing(true);
        });
        hudController.attackButton.onButtonDown.AddListener(() =>
        {
            //Repass the command of attack to the method
            Attacking(hudController.joystickAxis.currentInput.x, true);
        });
        hudController.releaseButton.onButtonDown.AddListener(() =>
        {
            //Repass the command of release to the method
            ForceClimbToFinishOnPressReleaseButton(true);
        });
        hudController.itemButton.onButtonDown.AddListener(() =>
        {
            //If can't use health potion, ignore
            if (canUseHealthPotion == false)
                return;
            //If have zero potions, ignore
            if (SaveGameManager.healthPotions <= 0)
                return;

            //Heal the player
            currentHealth += healthPotionHeal;

            //Play the effects
            healParticles.Play();
            healSound.Play();

            //Reset the potion
            healthPotionCurrentTimer = 0.0f;

            //Remove one potion and save the game
            SaveGameManager.healthPotions -= 1;
            SaveGameManager.SaveData();
        });

        //Prepare the attack cooldown
        attackDuration = new WaitForSeconds(attackCooldown);

        //Get the starting position of the player
        startingPosition = this.gameObject.transform.position;

        //Get the default Rigidbody parameters
        defaultRbCollision = playerRigidbody.collisionDetectionMode;
        defaultRbInterpolation = playerRigidbody.interpolation;
    }

    void Update()
    {
        //Run needed tasks
        CheckIfInUpOfAMonster();
        CheckIfIsOnGround();
        ReadPlayerInputs();
        ForceJumpFinishIfHitHeadInSomething();
        DashCooldownDisplay();
        ForceDashFinishIfHitBodyInGround();
        AttackCooldownUpdater();
        HealthManager();
        SyncDetectorsWithTheCurrentLookingDirectionOfPlayer();
        RunGroundEdgeClimbingDetector();
        HealthPotionManager();
    }

    private void CheckIfInUpOfAMonster()
    {
        //If is on ground, cancel
        if (isOnGround == true)
            return;
        //If can't receive damage from up of a monster, cancel
        if (canReceiveDamageFromUpMonster == false)
            return;

        //Prepare the raycast result storage
        RaycastHit raycastHitResult;

        //Launch a raycast to check if have a monster below the player
        bool hasHit = Physics.Raycast(groundCheckRaycast.position, groundCheckRaycast.TransformDirection(Vector3.down), out raycastHitResult, 0.5f, monsterCheckLayers);

        //If don't have a hit, cancel
        if (hasHit == false)
            return;

        //Cause damage to player
        ThrowPlayer(raycastHitResult.collider.gameObject.transform.position);
        CauseDamage(5.0f, DamageType.MonsterAttack);

        //Informs that can't receive a new damage of up of a monster
        canReceiveDamageFromUpMonster = false;
    }

    private void CheckIfIsOnGround()
    {
        //Prepare the raycast result storage
        RaycastHit raycastHitResult;

        //Launch a raycast to check if have a ground below the player
        bool hasHit = Physics.Raycast(groundCheckRaycast.position, groundCheckRaycast.TransformDirection(Vector3.down), out raycastHitResult, 0.5f, groundCheckLayer);

        //Defines to run the falling animation
        if (hasHit == true)
        {
            //Detect the end of fall
            if (fallEnded == false)
            {
                OnFallOnGround();
                fallEnded = true;
            }

            //Inform to animator...
            playerAnimator.SetBool("falling", false);
        }
        if (hasHit == false)
        {
            //Detect the start of fall
            if (fallEnded == true)
            {
                OnExitFromGround();
                fallEnded = false;
            }

            //Inform to animator...
            playerAnimator.SetBool("falling", true);
        }

        //Inform that is on ground
        isOnGround = hasHit;
    }

    private void OnExitFromGround()
    {
        //...
    }

    private void OnFallOnGround()
    {
        //Play the particles and sound of fall (if is climbing or in water, don't run FX)
        if (isClimbingGroundEdge == false && isOnWater == false)
        {
            fallSound[Random.Range(0, fallSound.Length)].Play();
            fallParticles.Play();
        }

        //Restore double jumping
        primeJumpReady = true;
        doubleJumpReady = true;

        //Inform that now can receive a new damage of a up of any monster
        canReceiveDamageFromUpMonster = true;
    }

    private void ReadPlayerInputs()
    {
        //Get the inputs
        int currentMoveValue = 0;

        //Read keyboard input and joystick axis to get the movement command
        if (hudController.joystickAxis.isHoldingAxis == true)
        {
            if (hudController.joystickAxis.currentInput.x < 0)
                currentMoveValue = -1;
            if (hudController.joystickAxis.currentInput.x > 0)
                currentMoveValue = 1;
            if (hudController.joystickAxis.currentInput.x == 0)
                currentMoveValue = 0;
        }
        if (hudController.joystickAxis.isHoldingAxis == false)
        {
            if (Input.GetKey(KeyCode.A) == true)
                currentMoveValue = -1;
            if (Input.GetKey(KeyCode.D) == true)
                currentMoveValue = 1;
            if (Input.GetKey(KeyCode.A) == false && Input.GetKey(KeyCode.D) == false)
                currentMoveValue = 0;
        }

        //Read keyboard input to get the jumping command
        if (Input.GetKeyDown(KeyCode.Space) == true && isClimbingGroundEdge == false)
            Jumping(true);
        //Read keyboard input to get the ground edge release command
        if (Input.GetKeyDown(KeyCode.Space) == true && isClimbingGroundEdge == true && isClimbingReady == true)
            ForceClimbToFinishOnPressReleaseButton(true);

        //Read keyboard input to get the dashing command
        if (Input.GetKeyDown(KeyCode.Q) == true)
            Dashing(true);

        //Read mouse input to get the attacking command
        if (Input.GetMouseButtonDown(0) == true && Application.isEditor == true)
            Attacking(currentMoveValue, true);

        //Execute the movement routine
        Movement(currentMoveValue);
    }

    private void Movement(float currentInput)
    {
        //Calculate the artifical gravity
        float artificialGravity = 0.0f;
        if (isOnGround == false)
            artificialGravity = -24.0f;
        if (isOnGround == true)
            artificialGravity = 0.0f;

        //If is running... AND is not in a dash impulse AND not attacking AND is not falling into void AND is not death AND is not in a damage throw AND is not climbing...
        if (currentInput != 0 && isOnDashImpulse == false && isAttacking == false && isFallingToVoid == false && isDead == false && isOnDamageThrow == DamageThrowTo.None && isClimbingGroundEdge == false)
        {
            //Play the animation
            if (isOnWater == false)
            {
                playerAnimator.SetBool("run", true);
                playerAnimator.SetBool("runWater", false);
            }
            if (isOnWater == true)
            {
                playerAnimator.SetBool("run", false);
                playerAnimator.SetBool("runWater", true);
            }
            //Make the movement
            if (isOnJump1Impulse == false && isOnJump2Impulse == false)
                playerRigidbody.velocity = transform.TransformVector(new Vector3(movementSpeed * currentInput, artificialGravity, 0));
            if (isOnJump1Impulse == true || isOnJump2Impulse == true)
                playerRigidbody.velocity = transform.TransformVector(new Vector3(movementSpeed * currentInput, playerRigidbody.velocity.y, 0));

            //Rotate the player
            if (currentInput == -1)
            {
                playerModelTransform.localEulerAngles = Vector3.Lerp(playerModelTransform.localEulerAngles, new Vector3(0, 180, 0), 24 * Time.deltaTime);
                currentlyFacingTo = CurrentlyFacingTo.Left;
            }
            //Rotate the player
            if (currentInput == 1)
            {
                playerModelTransform.localEulerAngles = Vector3.Lerp(playerModelTransform.localEulerAngles, new Vector3(0, 0, 0), 24 * Time.deltaTime);
                currentlyFacingTo = CurrentlyFacingTo.Right;
            }

            //If is on water, start the coroutine of sound effects
            if (isOnWater == true)
                if (swimingEffectsCoroutine == null)
                    swimingEffectsCoroutine = StartCoroutine(SwimingSoundAndVisualEffects());

            //Inform that is running
            isRunning = true;
        }

        //If is not running... OR is in dash impulse OR is attacking OR is falling into void OR is dead OR is in a damage throw OR is climbing...
        if (currentInput == 0 || isOnDashImpulse == true || isAttacking == true || isFallingToVoid == true || isDead == true || isOnDamageThrow != DamageThrowTo.None || isClimbingGroundEdge == true)
        {
            //If is not in a dash impulse AND not in a attack impulse AND not in a damage throw impulse AND not climbing a edge...
            if (isOnDashImpulse == false && isOnAttackImpulse == false && isOnDamageThrow == DamageThrowTo.None && isClimbingGroundEdge == false)
            {
                //Stop the movement
                if (isOnJump1Impulse == false && isOnJump2Impulse == false)
                    playerRigidbody.velocity = new Vector3(0, artificialGravity, 0);
                if (isOnJump1Impulse == true || isOnJump2Impulse == true)
                    playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, 0);
            }

            //Stop the coroutine of swiming sound/visual effects, if have one
            if (swimingEffectsCoroutine != null)
            {
                StopCoroutine(swimingEffectsCoroutine);
                swimingEffectsCoroutine = null;
            }

            //Stop the animation and inform that is not running
            playerAnimator.SetBool("run", false);
            playerAnimator.SetBool("runWater", false);
            isRunning = false;
        }

        //SKILLS, AND OTHER TIPE OF MOVEMENTS THAT IS NOT ORIGINED BY THE PLAYER

        //If is in a dash, move the player...
        if (isOnDashImpulse == true && currentlyFacingTo == CurrentlyFacingTo.Left)
            playerRigidbody.velocity = new Vector3((dashImpulseSpeed * -1.0f), 0, 0);
        if (isOnDashImpulse == true && currentlyFacingTo == CurrentlyFacingTo.Right)
            playerRigidbody.velocity = new Vector3(dashImpulseSpeed, 0, 0);

        //If is in a attack impulse, move the player...
        if (isOnAttackImpulse == true && currentlyFacingTo == CurrentlyFacingTo.Left)
            playerRigidbody.velocity = new Vector3((attackImpulseSpeed * -1.0f), 0, 0);
        if (isOnAttackImpulse == true && currentlyFacingTo == CurrentlyFacingTo.Right)
            playerRigidbody.velocity = new Vector3(attackImpulseSpeed, 0, 0);

        //If is in a damage throw impulse, move the player...
        if (isOnDamageThrow == DamageThrowTo.Right)
            playerRigidbody.velocity = new Vector3(damageThrowImpulseSpeed, 0, 0);
        if (isOnDamageThrow == DamageThrowTo.Left)
            playerRigidbody.velocity = new Vector3((damageThrowImpulseSpeed * -1.0f), 0, 0);
    }

    private void ForceJumpFinishIfHitHeadInSomething()
    {
        //If is not in some jump impulse, ignore
        if (isOnJump1Impulse == false && isOnJump2Impulse == false)
            return;

        //Prepare the raycast result storage
        RaycastHit raycastHitResult;

        //Launch a raycast to check if have a ground above the player
        bool hasHit = Physics.Raycast(hitHeadDetectorOrigin.position, hitHeadDetectorOrigin.TransformDirection(Vector3.up), out raycastHitResult, 0.5f, groundCheckLayer);

        //If don't have a hit, cancel
        if (hasHit == false)
            return;

        //If have a hit, cancel impulse of any jump
        if (jump1ImpulseCoroutine != null)
            StopCoroutine(jump1ImpulseCoroutine);
        jump1ImpulseCoroutine = null;
        if (jump2ImpulseCoroutine != null)
            StopCoroutine(jump2ImpulseCoroutine);
        jump2ImpulseCoroutine = null;
        isOnJump1Impulse = false;
        isOnJump2Impulse = false;

        //Force the end of any jump animation
        playerAnimator.SetTrigger("jumpCancel");
    }

    private void Jumping(bool requestingJump)
    {
        //If is not requesting jump, ignore
        if (requestingJump == false)
            return;
        //If is dashing, ignore
        if (isOnDashImpulse == true)
            return;
        //If is attacking, ignore
        if (isAttacking == true)
            return;
        //If is falling into void, ignore
        if (isFallingToVoid == true)
            return;
        //If is dead, ignore
        if (isDead == true)
            return;
        //If is in a damage throw, ignore
        if (isOnDamageThrow != DamageThrowTo.None)
            return;
        //If is climbing, ignore
        if (isClimbingGroundEdge == true)
            return;

        //If is not in the first jump, and is in the ground, do the first jump
        if (primeJumpReady == true && isOnGround == true)
        {
            //Play the jump animation and FX
            playerAnimator.ResetTrigger("jumpCancel");
            playerAnimator.SetTrigger("jump1");

            //Do the impulse
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            playerRigidbody.velocity += new Vector3(0, 16.0f, 0);

            //Start the timer to remove jumping effect
            jump1ImpulseCoroutine = StartCoroutine(RemoveJumpingEffect1());

            //Inform that is on jump
            isOnJump1Impulse = true;
            //Block the jumping
            primeJumpReady = false;

            //Stop the execution of the method
            return;
        }

        //If already make the prime jump, and is not in the ground, and double jump ready, do the second jump
        if (primeJumpReady == false && isOnGround == false && doubleJumpReady == true)
        {
            //Play the jump animation and FX
            playerAnimator.ResetTrigger("jumpCancel");
            playerAnimator.SetTrigger("jump2");
            jumpSound[Random.Range(0, jumpSound.Length)].Play();
            jumpParticles.Play();

            //Do the impulse
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            playerRigidbody.velocity += new Vector3(0, 16.0f, 0);

            //Start the timer to remove jumping effect
            jump2ImpulseCoroutine = StartCoroutine(RemoveJumpingEffect2());

            //Inform that is on jump
            isOnJump2Impulse = true;
            //Block the jumping
            doubleJumpReady = false;

            //Stop the execution of the method
            return;
        }
    }

    private void DashCooldownDisplay()
    {
        //Reduce the dash cooldown
        if (dashTimer > 0.0f)
            dashTimer -= Time.deltaTime;
        if (dashTimer < 0.0f)
            dashTimer = 0.0f;

        //Shows the dash timer in the UI
        hudController.dashButton.progressFillValue = ((dashTimer / dashCooldown) * 100.0f);
    }

    private void ForceDashFinishIfHitBodyInGround()
    {
        //If is not in a dash, cancel
        if (isOnDashImpulse == false)
            return;

        //Prepare the raycast result storage
        RaycastHit raycastHitResult;

        //Launch a raycast to check if have a ground above the player
        bool hasHit = Physics.Raycast(hitDashDetectorOrigin.position, hitDashDetectorOrigin.TransformDirection(Vector3.right), out raycastHitResult, 1.0f, groundCheckLayer);

        //If don't have a hit, cancel
        if (hasHit == false)
            return;

        //If have a hit, cancel impulse of dash
        if (dashImpulseCoroutine != null)
            StopCoroutine(dashImpulseCoroutine);
        dashImpulseCoroutine = null;

        //Stop effects
        if (dashParticles.isPlaying == true)
            dashParticles.Stop();

        //Enable the hitbox
        playerHitbox.enabled = true;

        //Remove dash effect
        isOnDashImpulse = false;

        //Force the end of any dash animation
        playerAnimator.SetTrigger("dashCancel");
    }

    private void Dashing(bool requestingDash)
    {
        //If is not requesting the dash, ignore
        if (requestingDash == false)
            return;
        //If the cooldown is not ready, ignore
        if (dashTimer > 0.0f)
            return;
        //If is not on the ground, ignore
        if (isOnGround == false)
            return;
        //If is attacking, ignore
        if (isAttacking == true)
            return;
        //If is falling into void, ignore
        if (isFallingToVoid == true)
            return;
        //If is dead, ignore
        if (isDead == true)
            return;
        //If is in a damage throw, ignore
        if (isOnDamageThrow != DamageThrowTo.None)
            return;
        //If is climbing, ignore
        if (isClimbingGroundEdge == true)
            return;
        //If is on water, ignore
        if (isOnWater == true)
            return;

        //Disable the hitbox
        playerHitbox.enabled = false;

        //Rotate the player to correct direction, base in the looking at
        if (currentlyFacingTo == CurrentlyFacingTo.Left)
            playerModelTransform.localEulerAngles = new Vector3(0, 180, 0);
        if (currentlyFacingTo == CurrentlyFacingTo.Right)
            playerModelTransform.localEulerAngles = new Vector3(0, 0, 0);

        //Play the jump animation and FX
        playerAnimator.ResetTrigger("dashCancel");
        playerAnimator.SetTrigger("dash");
        dashSound[Random.Range(0, dashSound.Length)].Play();
        if ((Random.Range(0.0f, 100.0f)) <= 35.0f)
            effortSound[Random.Range(0, effortSound.Length)].Play();
        dashParticles.Play();

        //Start the timer to remove jumping effect
        dashImpulseCoroutine = StartCoroutine(RemoveDashingEffect());

        //Reset the cooldown and inform that is on dash
        dashTimer = dashCooldown;
        isOnDashImpulse = true;
    }

    private void AttackCooldownUpdater()
    {
        //Increase the attack cooldown timer
        if (attackTimer < 3.0f)
            attackTimer += Time.deltaTime;
    }

    private void Attacking(float currentMovementImput, bool requestingAttack)
    {
        //If is not requesting attack, ignore
        if (requestingAttack == false)
            return;
        //If is not in the ground, ignore
        if (isOnGround == false)
            return;
        //If is dashing, ignore
        if (isOnDashImpulse == true)
            return;
        //If the cooldown of attack is not ready, ignore
        if (attackTimer < 0.5f == true)
            return;
        //If is falling into void, ignore
        if (isFallingToVoid == true)
            return;
        //If is dead, ignore
        if (isDead == true)
            return;
        //If is in a damage throw, ignore
        if (isOnDamageThrow != DamageThrowTo.None)
            return;
        //If is climbing, ignore
        if (isClimbingGroundEdge == true)
            return;
        //If is on water, ignore
        if (isOnWater == true)
            return;

        //Inform the CORRECTLY current side that the player is facing, based on current movement input
        if (currentMovementImput < 0)
            currentlyFacingTo = CurrentlyFacingTo.Left;
        if (currentMovementImput > 0)
            currentlyFacingTo = CurrentlyFacingTo.Right;

        //Change to sword on hand
        swordOnBack.SetActive(false);
        swordOnHand.SetActive(true);

        //Rotate the player to correct direction, base in the looking at
        if (currentlyFacingTo == CurrentlyFacingTo.Left)
            playerModelTransform.localEulerAngles = new Vector3(0, 180, 0);
        if (currentlyFacingTo == CurrentlyFacingTo.Right)
            playerModelTransform.localEulerAngles = new Vector3(0, 0, 0);

        //Play the correct animation
        if (currentAttackAnimation == 0)
            playerAnimator.SetTrigger("attack0");
        if (currentAttackAnimation == 1)
            playerAnimator.SetTrigger("attack1");
        if (currentAttackAnimation == 2)
            playerAnimator.SetTrigger("attack2");
        if (currentAttackAnimation == 3)
            playerAnimator.SetTrigger("attack3");
        //Play the attack animation and FX
        swordMoveSound[Random.Range(0, swordMoveSound.Length)].PlayDelayed(0.1f);
        if ((Random.Range(0.0f, 100.0f)) <= 10.0f)
            effortSound[Random.Range(0, effortSound.Length)].PlayDelayed(0.1f);
        slashParticles.Play();

        //Increase the attack animation counter
        currentAttackAnimation += 1;
        if (currentAttackAnimation > 3)
            currentAttackAnimation = 0;

        //Enable the damage hitbox
        attackHitbox.SetActive(true);

        //If already have a attack coroutine, clear it
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        //Start the timer to remove attack effect
        attackCoroutine = StartCoroutine(RemoveAttackEffect());
        //If already have a attack animation reset coroutine, clear it
        if (attackAnimationResetCoroutine != null)
            StopCoroutine(attackAnimationResetCoroutine);
        //Start the timer to reset the attack animation counter
        attackAnimationResetCoroutine = StartCoroutine(ResetAttackAnimationCounter());
        //Start the timer to remove attack impulse
        StartCoroutine(RemoveAttackEffectImpulse());

        //Reset the attack timer and inform that is on attack, and on attack impulse
        attackTimer = 0.0f;
        isAttacking = true;
        isOnAttackImpulse = true;
    }

    private void HealthManager()
    {
        //Fix the HP
        if (currentHealth < 0.0f)
            currentHealth = 0.0f;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        //Show the HP on HUD
        hudController.hpBar.maxValue = maxHealth;
        hudController.hpBar.value = currentHealth;
    }

    private void ForceClimbToFinishOnPressReleaseButton(bool requestingRelease)
    {
        //If is not requesting release, ignore
        if (requestingRelease == false)
            return;
        //If is not climbing, ignore
        if (isClimbingGroundEdge == false)
            return;
        //If the climbing is already not ready, ignore
        if (isClimbingReady == false)
            return;

        //Clear climb coroutines, if have
        if (climbCoroutine != null)
            StopCoroutine(climbCoroutine);
        climbCoroutine = null;
        if (releaseButtonCoroutine != null)
            StopCoroutine(releaseButtonCoroutine);
        releaseButtonCoroutine = null;

        //Hide the button
        hudController.releaseButton.gameObject.SetActive(false);

        //Force go to idle animation
        playerAnimator.ResetTrigger("edgeClimb0");
        playerAnimator.ResetTrigger("edgeClimb1");
        playerAnimator.ResetTrigger("edgeClimb2");
        playerAnimator.ResetTrigger("edgeClimbExit");
        playerAnimator.SetTrigger("forceGoToIdle");

        //Re-enable the physics
        playerHitbox.enabled = true;
        playerRigidbody.interpolation = defaultRbInterpolation;
        playerRigidbody.collisionDetectionMode = defaultRbCollision;
        playerRigidbody.useGravity = true;
        playerRigidbody.isKinematic = false;

        //Inform that the climbing is not available
        isClimbingReady = false;

        //Start the coroutine to monitor falling, to make the climbing ready again after fall X meters down
        StartCoroutine(ClimbReleaseAvailabilityMonitor());

        //Inform that is not climbing
        isClimbingGroundEdge = false;
    }

    private void SyncDetectorsWithTheCurrentLookingDirectionOfPlayer()
    {
        //If is looking to right...
        if (currentlyFacingTo == CurrentlyFacingTo.Right)
            playerDetectors.localEulerAngles = new Vector3(0, 0, 0);
        //If is looking to left...
        if (currentlyFacingTo == CurrentlyFacingTo.Left)
            playerDetectors.localEulerAngles = new Vector3(0, 180, 0);
    }

    private void RunGroundEdgeClimbingDetector()
    {
        //If is on ground, cancel
        if (isOnGround == true)
            return;
        //If is already climbing, cancel
        if (isClimbingGroundEdge == true)
            return;
        //If is on jump impulse, cancel
        if (isOnJump1Impulse == true || isOnJump2Impulse == true)
            return;
        //If is falling into void, cancel
        if (isFallingToVoid == true)
            return;
        //If the climbing is not ready, ignore
        if (isClimbingReady == false)
            return;
        //If is on water, ignore
        if (isOnWater == true)
            return;

        //Prepare the raycast result storage
        RaycastHit raycastHitResult;

        //Launch a raycast to check if have a ground edge that the player can climb
        bool hasHit = Physics.Raycast(groundEdgeDetectorOrigin.position, groundEdgeDetectorOrigin.TransformDirection(Vector3.down), out raycastHitResult, 1.25f, groundCheckLayer);

        //If don't have a hit, cancel
        if (hasHit == false)
            return;
        //If is a ground of a platform, cancel
        if (raycastHitResult.collider.gameObject.GetComponent<PlatformGround>() != null)
            return;

        //Start a coroutine of climb
        climbCoroutine = StartCoroutine(ClimbTheGroundEdgeAhead(raycastHitResult.point));

        //Start the coroutine that controls the climb release button
        releaseButtonCoroutine = StartCoroutine(ControlReleaseButtonDisplayWhileClimbing());
    }

    private void HealthPotionManager()
    {
        //Increase healthpotion timer
        if (healthPotionCurrentTimer < healthPotionMaxCooldown)
            healthPotionCurrentTimer += Time.deltaTime;
        if (healthPotionCurrentTimer > healthPotionMaxCooldown)
            healthPotionCurrentTimer = healthPotionMaxCooldown;

        //If the health bar was changed, check if the potion can be used
        if (currentHealth != lastHeathPoints)
        {
            //If have less health than the potion heal, enable the potion use
            if (currentHealth <= (maxHealth - healthPotionHeal))
            {
                hudController.itemButton.iconColor = Color.white;
                canUseHealthPotion = true;
            }
            //If have more health than the potion heal, disable the potion use
            if (currentHealth > (maxHealth - healthPotionHeal))
            {
                hudController.itemButton.iconColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
                canUseHealthPotion = false;
            }

            //Inform the new health points
            lastHeathPoints = currentHealth;
        }

        //If the potions quantity was changed, show it
        if (SaveGameManager.healthPotions != lastHealthPotionsCount)
        {
            //Show the count
            hudController.itemCounterButtonText.text = SaveGameManager.healthPotions.ToString();

            //Inform the new health potions
            lastHealthPotionsCount = SaveGameManager.healthPotions;
        }

        //Show the cooldown at the button
        hudController.itemButton.progressFillValue = ((healthPotionCurrentTimer / healthPotionMaxCooldown) * 100.0f);
    }

    //Public methods

    public void CauseDamage(float damageToCause, DamageType damageType)
    {
        //If is dead, ignore
        if (isDead == true)
            return;

        //If the player have more HP than damage...
        if (currentHealth > damageToCause)
        {
            //Reduces the player HP
            currentHealth -= damageToCause;
            //Play the hurt sound
            hurtSound[Random.Range(0, hurtSound.Length)].Play();
            if (damageType == DamageType.MonsterAttack)
            {
                blowSound.pitch = Random.Range(0.9f, 1.15f);
                blowSound.Play();
                blowParticles.Play();
            }

            //Cancel execution of the method
            return;
        }

        //If the player don't have more HP than damage...
        if (currentHealth <= damageToCause)
        {
            //Reduces the player HP
            currentHealth -= damageToCause;
            //Play the death sound
            deathSound[Random.Range(0, deathSound.Length)].Play();
            if (damageType == DamageType.MonsterAttack)
            {
                blowSound.pitch = Random.Range(0.9f, 1.15f);
                blowSound.Play();
                blowParticles.Play();
            }

            //If is dying because of void, show the death screen
            if (damageType == DamageType.VoidFallWithDeath)
                StartCoroutine(DieBecauseOfVoid());
            //If is dying because of anything that is not the void, run the death animation
            if (damageType != DamageType.VoidFallWithDeath)
                StartCoroutine(DieBecauseOfAnythingThatIsNotVoid());

            //Inform that is dead
            isDead = true;

            //Cancel execution of the method
            return;
        }
    }

    public void ThrowPlayer(Vector3 throwOriginGlobal)
    {
        //If is dead, ignore
        if (isDead == true)
            return;
        //If is climbing, ignore
        if (isClimbingGroundEdge == true)
            return;
        //If is on water, ignore
        if (isOnWater == true)
            return;

        //If sword slash particle is playing, stop it
        if (slashParticles.isPlaying == true)
            slashParticles.Stop();

        //Calculate the damage impulse direction...
        if (throwOriginGlobal.x <= thisTransform.position.x)
            isOnDamageThrow = DamageThrowTo.Right;
        if (throwOriginGlobal.x > thisTransform.position.x)
            isOnDamageThrow = DamageThrowTo.Left;

        //Prepare the result for the animation
        AnimateThrowTo animateThrowFor = AnimateThrowTo.None;
        //Calculate the damage impulse animation...
        if (isOnDamageThrow == DamageThrowTo.Left)
        {
            //If is looking for right...
            if (currentlyFacingTo == CurrentlyFacingTo.Right)
                animateThrowFor = AnimateThrowTo.Front;
            //If is looking for left...
            if (currentlyFacingTo == CurrentlyFacingTo.Left)
                animateThrowFor = AnimateThrowTo.Back;
        }
        if (isOnDamageThrow == DamageThrowTo.Right)
        {
            //If is looking for right...
            if (currentlyFacingTo == CurrentlyFacingTo.Right)
                animateThrowFor = AnimateThrowTo.Back;
            //If is looking for left...
            if (currentlyFacingTo == CurrentlyFacingTo.Left)
                animateThrowFor = AnimateThrowTo.Front;
        }

        //Fix the rotation of the player
        if (currentlyFacingTo == CurrentlyFacingTo.Right)
            playerModelTransform.localEulerAngles = new Vector3(0, 0, 0);
        if (currentlyFacingTo == CurrentlyFacingTo.Left)
            playerModelTransform.localEulerAngles = new Vector3(0, 180, 0);

        //If is not on attack impulse, run the damage thrown animation
        if (isOnAttackImpulse == false)
        {
            //If is desired to animate throwing from back...
            if (animateThrowFor == AnimateThrowTo.Back)
            {
                //Get random animation
                int random = Random.Range(0, 2);

                //Call the animation
                if (random == 0)
                    playerAnimator.SetTrigger("backThrow0");
                if (random == 1)
                    playerAnimator.SetTrigger("backThrow1");
            }
            //If is desired to animate throwing from front...
            if (animateThrowFor == AnimateThrowTo.Front)
            {
                //Get random animation
                int random = Random.Range(0, 2);

                //Call the animation
                if (random == 0)
                    playerAnimator.SetTrigger("frontThrow0");
                if (random == 1)
                    playerAnimator.SetTrigger("frontThrow1");
            }
        }

        //If already have a damage throw coroutine, clear it
        if (damageThrowCoroutine != null)
            StopCoroutine(damageThrowCoroutine);
        //Start a damage throw coroutine to remove the impulse after a time
        damageThrowCoroutine = StartCoroutine(RemoveDamageThrowEffect());
    }

    //Collision detectors

    void OnCollisionEnter(Collision collision)
    {
        //If is dead, ignore
        if (isDead == true)
            return;

        //If is a collision with void...
        if (collision.gameObject.layer == VOID_LAYER)
            StartCoroutine(FallingIntoVoid());
    }

    void OnTriggerEnter(Collider collider)
    {
        //If is dead, ignore
        if (isDead == true)
            return;

        //If is triggering with water...
        if (collider.gameObject.layer == WATER_LAYER)
        {
            //Play the effects
            waterSplashSound[Random.Range(0, waterSplashSound.Length)].Play();
            waterSplashParticles.Play();

            //Inform to animator that is in the water
            playerAnimator.SetBool("onWater", true);

            //Inform that is on water
            isOnWater = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        //If is dead, ignore
        if (isDead == true)
            return;

        //If is triggering with water...
        if (collider.gameObject.layer == WATER_LAYER)
        {
            //Play the effects
            if (waterExitParticles.isPlaying == true)
                waterExitParticles.Stop();
            waterExitParticles.Play();

            //Stop the coroutine of swiming sound/visual effects, if have one
            if (swimingEffectsCoroutine != null)
            {
                StopCoroutine(swimingEffectsCoroutine);
                swimingEffectsCoroutine = null;
            }

            //Inform to animator that is not in the water more
            playerAnimator.SetBool("onWater", false);

            //Inform that is not on water
            isOnWater = false;
        }
    }

    //Coroutines and timers

    private IEnumerator RemoveJumpingEffect1()
    {
        //Wait time before remove jumping impulse
        yield return new WaitForSeconds(0.5f);

        //Remove jumping effect
        isOnJump1Impulse = false;

        //Clear this coroutine
        jump1ImpulseCoroutine = null;
    }

    private IEnumerator RemoveJumpingEffect2()
    {
        //Wait time before remove jumping impulse
        yield return new WaitForSeconds(0.5f);

        //Remove jumping effect
        isOnJump2Impulse = false;

        //Clear this coroutine
        jump2ImpulseCoroutine = null;
    }

    private IEnumerator RemoveDashingEffect()
    {
        //Wait time before remove dash impulse
        yield return new WaitForSeconds(dashImpulseTime);

        //Enable the hitbox
        playerHitbox.enabled = true;

        //Remove dash effect
        isOnDashImpulse = false;

        //Clear this coroutine
        dashImpulseCoroutine = null;
    }

    private IEnumerator RemoveAttackEffect()
    {
        //Wait attack duration time, and remove attack effect
        yield return attackDuration;

        //Change to sword on back
        swordOnBack.SetActive(true);
        swordOnHand.SetActive(false);

        //Remove attack effect
        isAttacking = false;

        //Clear this attack coroutine
        attackCoroutine = null;
    }

    private IEnumerator RemoveAttackEffectImpulse()
    {
        //Wait time before remove attack impulse
        yield return attackImpulseDuration;

        //Disable the damage hitbox
        attackHitbox.SetActive(false);

        //Remove the attack impulse
        isOnAttackImpulse = false;
    }

    private IEnumerator ResetAttackAnimationCounter()
    {
        //Wait the time window for attack commands before reset the animation counter
        yield return attackAnimationResetTimer;

        //Reset the attack animation counter
        currentAttackAnimation = 0;
    }

    private IEnumerator FallingIntoVoid()
    {
        //Change the UI
        hudController.pauseButtonObj.SetActive(false);
        hudController.controlsObj.SetActive(false);

        //Inform that is falling into void
        isFallingToVoid = true;

        //If the player don't have more HP than the fall damage...
        if (currentHealth <= hpLossOnFallIntoVoid)
        {
            //Defines the player HP to zero
            CauseDamage(hpLossOnFallIntoVoid, DamageType.VoidFallWithDeath);
        }
        //If the player have more HP than the fall damage...
        if (currentHealth > hpLossOnFallIntoVoid)
        {
            //Show the UI fall transition
            hudController.fallTransitionObj.SetActive(true);

            //Reduces the player HP
            CauseDamage(hpLossOnFallIntoVoid, DamageType.VoidFall);

            //Wait the fade in finishes...
            yield return new WaitForSeconds(1.0f);

            //Inform to camera follow instantly
            cameraFollowInstantly = true;

            //Disable the rigidbody of player
            playerRigidbody.isKinematic = true;

            //Wait some time before teleport the player
            yield return new WaitForSeconds(0.5f);

            //Move the player to start of the level
            this.gameObject.transform.position = startingPosition;

            //Make the player look at correct direction
            if (currentlyFacingTo == CurrentlyFacingTo.Left)
                playerModelTransform.localEulerAngles = new Vector3(0, 180, 0);
            if (currentlyFacingTo == CurrentlyFacingTo.Right)
                playerModelTransform.localEulerAngles = new Vector3(0, 0, 0);

            //Wait the fade out finishes...
            yield return new WaitForSeconds(2.0f);

            //Inform to camera follow normally
            cameraFollowInstantly = false;

            //Re-enable the rigidbody of player
            playerRigidbody.isKinematic = false;

            //Change the UI
            hudController.pauseButtonObj.SetActive(true);
            hudController.controlsObj.SetActive(true);

            //Disable the UI fall transition
            hudController.fallTransitionObj.SetActive(false);

            //Inform that is not falling into void
            isFallingToVoid = false;
        }
    }

    private IEnumerator DieBecauseOfVoid()
    {
        //Change the UI
        hudController.pauseButtonObj.SetActive(false);
        hudController.controlsObj.SetActive(false);
        hudController.healthBarObj.SetActive(false);
        hudController.coinsObj.SetActive(false);

        //Wait time before show death screen
        yield return new WaitForSeconds(1.5f);

        //Show the death UI
        hudController.deathObj.SetActive(true);
    }

    private IEnumerator RemoveDamageThrowEffect()
    {
        //Wait time before remvoe the damage impulse
        yield return damageImpulseDuration;

        //Remove the damage throw impulse
        isOnDamageThrow = DamageThrowTo.None;

        //Clear this damage throw coroutine
        damageThrowCoroutine = null;
    }

    private IEnumerator DieBecauseOfAnythingThatIsNotVoid()
    {
        //Stop any important coroutine
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        //Change the UI
        hudController.pauseButtonObj.SetActive(false);
        hudController.controlsObj.SetActive(false);
        hudController.healthBarObj.SetActive(false);
        hudController.coinsObj.SetActive(false);

        //Change the sword to back
        swordOnBack.SetActive(true);
        swordOnHand.SetActive(false);

        //Calculate the death animation
        int deathAnim = Random.Range(0, 2);
        //Run the death animation
        if (deathAnim == 0)
            playerAnimator.SetTrigger("death0");
        if (deathAnim == 1)
            playerAnimator.SetTrigger("death1");

        //Wait time before show death screen
        yield return new WaitForSeconds(2.0f);

        //Show the death UI
        hudController.deathObj.SetActive(true);
    }

    private IEnumerator ClimbTheGroundEdgeAhead(Vector3 groundEdgeDetectorHitPosition)
    {
        //Inform that is climbing
        isClimbingGroundEdge = true;

        //Rotate the player to correct direction, according to currently facing
        if (currentlyFacingTo == CurrentlyFacingTo.Right)
            playerModelTransform.localEulerAngles = new Vector3(0, 0, 0);
        if (currentlyFacingTo == CurrentlyFacingTo.Left)
            playerModelTransform.localEulerAngles = new Vector3(0, 180, 0);

        //Disable the physics
        playerRigidbody.useGravity = false;
        playerRigidbody.isKinematic = true;
        playerRigidbody.interpolation = RigidbodyInterpolation.None;
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        playerHitbox.enabled = false;

        //Try to launch a raycast to position the player body at the best distance from the ground edge
        RaycastHit raycastHitResult;
        bool hasHit = Physics.Raycast(groundEdgeDistanceOrigin.position, groundEdgeDistanceOrigin.TransformDirection(Vector3.right), out raycastHitResult, 3.0f, groundCheckLayer);
        //If has a hit, try to position the player most closer possible of the ground edge
        if (hasHit == true)
        {
            //If the player is looking to right...
            if (currentlyFacingTo == CurrentlyFacingTo.Right)
                thisTransform.position = new Vector3((raycastHitResult.point.x - 1.1f), raycastHitResult.point.y, raycastHitResult.point.z);
            //If the player is looking to left...
            if (currentlyFacingTo == CurrentlyFacingTo.Left)
                thisTransform.position = new Vector3((raycastHitResult.point.x + 1.1f), raycastHitResult.point.y, raycastHitResult.point.z);
        }
        //Fix the player body global position height to fit the animation perfectly
        thisTransform.position = new Vector3(thisTransform.position.x, (groundEdgeDetectorHitPosition.y - 5.1f), thisTransform.position.z);

        //Run the climb hold animation
        playerAnimator.SetTrigger("edgeClimb0");
        //Play the edge dirt particles
        edgeDirtParticles.Play();

        //Wait a time handling
        yield return new WaitForSeconds(1.0f);

        //Run the climb up animation
        playerAnimator.SetTrigger("edgeClimb1");
        //Play the effort sound
        effortSound[Random.Range(0, effortSound.Length)].PlayDelayed(0.07f);

        //Wait a little time
        yield return new WaitForSeconds(0.25f);

        //Prepare to move the player to 3 meters up - Step 1
        Vector3 step1_playerInitialPosition = thisTransform.position;
        Vector3 step1_targetPlayerPosition = new Vector3(step1_playerInitialPosition.x, (step1_playerInitialPosition.y + 3.05f), step1_playerInitialPosition.z);
        float step1_animationLasTime = Time.time;
        float step1_animationDuration = 0.75f;
        float step1_animationCurrentTime = 0.0f;
        //Run the climb animation - Step 1
        while (step1_animationCurrentTime < step1_animationDuration)
        {
            //Update the elapsed time since animation start
            step1_animationCurrentTime += (Time.time - step1_animationLasTime);
            step1_animationLasTime = Time.time;

            //Fix the animation current time
            if (step1_animationCurrentTime > step1_animationDuration)
                step1_animationCurrentTime = step1_animationDuration;

            //Move the player to the desired point
            thisTransform.position = Vector3.Lerp(step1_playerInitialPosition, step1_targetPlayerPosition, (step1_animationCurrentTime / step1_animationDuration));

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Run the climb final animation
        playerAnimator.SetTrigger("edgeClimb2");

        //Prepare to move the player to 2 meters forward and 2 meters up - Step 2
        Vector3 step2_playerInitialPosition = thisTransform.position;
        Vector3 step2_targetPlayerPosition = Vector3.zero;
        if (currentlyFacingTo == CurrentlyFacingTo.Right)
            step2_targetPlayerPosition = new Vector3((step2_playerInitialPosition.x + 2.0f), (step2_playerInitialPosition.y + 2.05f), step2_playerInitialPosition.z);
        if (currentlyFacingTo == CurrentlyFacingTo.Left)
            step2_targetPlayerPosition = new Vector3((step2_playerInitialPosition.x - 2.0f), (step2_playerInitialPosition.y + 2.05f), step2_playerInitialPosition.z);
        float step2_animationLasTime = Time.time;
        float step2_animationDuration = 0.8f;
        float step2_animationCurrentTime = 0.0f;
        //Run the climb animation - Step 1
        while (step2_animationCurrentTime < step2_animationDuration)
        {
            //Update the elapsed time since animation start
            step2_animationCurrentTime += (Time.time - step2_animationLasTime);
            step2_animationLasTime = Time.time;

            //Fix the animation current time
            if (step2_animationCurrentTime > step2_animationDuration)
                step2_animationCurrentTime = step2_animationDuration;

            //Move the player to the desired point
            thisTransform.position = Vector3.Lerp(step2_playerInitialPosition, step2_targetPlayerPosition, (step2_animationCurrentTime / step2_animationDuration));

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Wait the end of animation and finish the climb animation
        yield return new WaitForSeconds(0.5f);

        //Re-enable the physics
        playerHitbox.enabled = true;
        playerRigidbody.interpolation = defaultRbInterpolation;
        playerRigidbody.collisionDetectionMode = defaultRbCollision;
        playerRigidbody.useGravity = true;
        playerRigidbody.isKinematic = false;

        //Inform that is not climbing anymore
        isClimbingGroundEdge = false;

        //Finish the climbing animation
        playerAnimator.SetTrigger("edgeClimbExit");

        //Clear this coroutine
        climbCoroutine = null;
    }

    private IEnumerator ControlReleaseButtonDisplayWhileClimbing()
    {
        //Show the release hand of ground edge button
        hudController.releaseButton.gameObject.SetActive(true);

        //Wait the input window finishes to hide the button
        yield return new WaitForSeconds(2.0f);

        //Hide the button
        hudController.releaseButton.gameObject.SetActive(false);

        //Clear this coroutine
        releaseButtonCoroutine = null;
    }

    private IEnumerator ClimbReleaseAvailabilityMonitor()
    {
        //Get the current player height
        float startingHeight = thisTransform.position.y;

        //Create a loop to monitor the player height and make the climb ready after fall X meters down
        while (true)
        {
            //Get the current height
            float currentHeight = thisTransform.position.y;

            //Get the height difference between starting and current height
            float difference = startingHeight - currentHeight;

            //If is falled more than 3 meters, make the climbing ready again...
            if (difference >= 5.5f)
                break;

            //Wait the interval
            yield return 0;   //<- Wait for the next game frame
        }

        //Inform that the climbing is ready
        isClimbingReady = true;
    }

    private IEnumerator SwimingSoundAndVisualEffects()
    {
        //Prepare the effects interval
        WaitForSeconds effectsInterval = new WaitForSeconds(0.4f);

        //Wait some interval
        yield return new WaitForSeconds(0.2f);

        //Create the loop of effects
        while (true)
        {
            //Run the effects
            swimSound[Random.Range(0, swimSound.Length)].Play();

            //Wait the interval time
            yield return effectsInterval;
        }
    }
}