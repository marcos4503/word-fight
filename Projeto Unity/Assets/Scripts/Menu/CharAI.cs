using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharAI : MonoBehaviour
{
    //Cache variables
    private WaitForSeconds jumpImpulseTime = new WaitForSeconds(0.75f);
    private bool doingJumpImpulse = false;
    private Coroutine jumpCoroutine = null;
    private Coroutine doubleJumpCoroutine = null;

    //Public variables
    public Animator charAnimator;
    public Rigidbody charRigidbody;
    public ParticleSystem slidingParticles;
    public ParticleSystem swimingParticles;

    //Core methods

    void Start()
    {
        //Play the run animation
        charAnimator.SetBool("run", true);
    }

    void Update()
    {
        //If is not doing any action that uses physics, simulates a real gravity
        if (doingJumpImpulse == false)
            charRigidbody.velocity = new Vector3(charRigidbody.velocity.x, -24.0f, charRigidbody.velocity.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Exit from jumping animation
        charAnimator.SetTrigger("jumpPrimary_exit");
        charAnimator.SetTrigger("falling_exit");
    }

    //Signal receivers

    public void DoStartSwin()
    {
        //Start swin animation
        charAnimator.SetBool("run", false);
        charAnimator.SetBool("swin", true);
    }

    public void DoSwinParticles()
    {
        swimingParticles.Play();
    }

    public void DoJump()
    {
        //Start the jump routine
        jumpCoroutine = StartCoroutine(JumpTimer());

        //Stop any others particles
        slidingParticles.Stop();
        //Go back to run animation
        charAnimator.SetBool("run", true);
        charAnimator.SetBool("swin", false);
    }

    public void DoDoubleJump()
    {
        //Start the jump routine
        doubleJumpCoroutine = StartCoroutine(DoubleJumpTimer());

        //Stop any others particles
        slidingParticles.Stop();
    }

    public void DoParkourForward()
    {
        //Do the movement
        charAnimator.SetTrigger("parkourForward");
    }

    public void DoParkourForwardWithTumble()
    {
        //Do the movement
        charAnimator.SetTrigger("parkourForwardWithTumble");
    }

    public void DoSlidingEnter()
    {
        //Do the movement
        charAnimator.SetTrigger("slidingEnter");
        slidingParticles.Play();
    }

    public void DoSlidingExit()
    {
        //Do the movement
        charAnimator.SetTrigger("slidingExit");
        slidingParticles.Stop();
    }

    //Coroutines

    private IEnumerator JumpTimer()
    {
        //Reset secondary jump if have
        if (doubleJumpCoroutine != null)
            StopCoroutine(doubleJumpCoroutine);

        //Enable the jump
        doingJumpImpulse = true;
        charAnimator.ResetTrigger("falling_exit");
        charAnimator.ResetTrigger("jumpPrimary_exit");
        charAnimator.SetTrigger("jumpPrimary_enter");
        charRigidbody.velocity = new Vector3(0, 0, 0);
        charRigidbody.velocity += new Vector3(0, 16.0f, 0);

        //Wait the impulse time
        yield return jumpImpulseTime;

        //Disable the jump
        doingJumpImpulse = false;
        charAnimator.SetTrigger("falling_enter");
    }

    private IEnumerator DoubleJumpTimer()
    {
        //Reset secondary jump if have
        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);

        //Enable the jump
        doingJumpImpulse = true;
        charAnimator.ResetTrigger("falling_exit");
        charAnimator.ResetTrigger("jumpSecondary_exit");
        charAnimator.SetTrigger("jumpSecondary_enter");
        charRigidbody.velocity = new Vector3(0, 0, 0);
        charRigidbody.velocity += new Vector3(0, 16.0f, 0);

        //Wait the impulse time
        yield return jumpImpulseTime;

        //Disable the jump
        doingJumpImpulse = false;
        charAnimator.SetTrigger("falling_enter");
    }
}
