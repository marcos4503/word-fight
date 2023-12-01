using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlatformArea : MonoBehaviour
{
    //Constant variables
    private const int PLAYER_LAYER = 10;

    //Cache variables
    private Coroutine movementCoroutine;
    private int movingTo = -1;

    //Public variables
    public Transform platformStart;
    public Transform platformStartDegree;
    public Transform platformEnd;
    public Transform platformEndDegree;
    public SpriteRenderer traceRenderer;
    public Transform traceRendererTransform;
    public float movementSpeed = 8.0f;
    public Transform mobilePlatformTransform;
    public Rigidbody mobilePlatformRigidbody;
    [Space(8)]
    [Header("Components Status")]
    public Transform playerTransformInHere;
    public Vector3 onEnterLocalPosition;
    public PlayerController playerControllerInHere;

    //Core methods

    void Start()
    {
        //If is not playing, ignore
        if (Application.isPlaying == false)
        {

            //Cancel
            return;
        }

        //Hide the gizmos of start and end area
        platformStart.GetComponent<SpriteRenderer>().enabled = false;
        platformEnd.GetComponent<SpriteRenderer>().enabled = false;

        //Disable the platform for optimization
        mobilePlatformTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        //If is not playing, ignore
        if (Application.isPlaying == false)
        {
            //Fix the edge position
            if (platformStart.localPosition.x > -1.0f)
                platformStart.localPosition = new Vector3(-1.0f, platformStart.localPosition.y, platformStart.localPosition.z);
            if (platformEnd.localPosition.x < 1.0f)
                platformEnd.localPosition = new Vector3(1.0f, platformEnd.localPosition.y, platformEnd.localPosition.z);

            //Resize the trace renderer
            traceRendererTransform.transform.position = new Vector3(platformStart.position.x, platformStart.position.y, traceRendererTransform.gameObject.transform.position.z);
            traceRenderer.size = new Vector2(Vector3.Distance(platformStart.position, platformEnd.position), 0.5f);

            //Rotate the trace, and arrows
            platformStartDegree.LookAt(new Vector3(platformEnd.position.x, platformEnd.position.y, platformStartDegree.position.z));
            platformEndDegree.LookAt(new Vector3(platformStart.position.x, platformStart.position.y, platformEndDegree.position.z));
            traceRendererTransform.gameObject.transform.eulerAngles = platformStartDegree.eulerAngles;

            //Cancel
            return;
        }

        //If have a player above the platform...
        if (playerTransformInHere != null)
        {
            //Make the player stays in the same position that he is entered on the platform, (if is not running AND not jumping AND not dashing AND not attacking)
            if (playerControllerInHere.isRunning == false && playerControllerInHere.isOnJump1Impulse == false && playerControllerInHere.isOnJump2Impulse == false &&
                playerControllerInHere.isOnDashImpulse == false && playerControllerInHere.isOnAttackImpulse == false)
                playerTransformInHere.localPosition = onEnterLocalPosition;

            //If is moving above the platform, update to know the new position of the player
            if (playerControllerInHere.isRunning == true || playerControllerInHere.isOnDashImpulse == true || playerControllerInHere.isOnAttackImpulse == true)
                onEnterLocalPosition = playerTransformInHere.localPosition;

            //If the player is below the ground height of the platform, or is not in a ground, release the player from the platform
            if (playerTransformInHere.position.y < (mobilePlatformTransform.position.y + 0.7f) || playerControllerInHere.isOnGround == false)
            {
                playerTransformInHere.SetParent(null);
                playerTransformInHere = null;
                playerControllerInHere = null;
                onEnterLocalPosition = Vector3.zero;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        //If is not playing, ignore
        if (Application.isPlaying == false)
            return;

        //If is not the player, ignore
        if (collider.gameObject.layer != PLAYER_LAYER)
            return;

        //Start the movement coroutine
        if (movementCoroutine == null)
            movementCoroutine = StartCoroutine(MovementLoop());

        //Enable the platform
        mobilePlatformTransform.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider collider)
    {
        //If is not playing, ignore
        if (Application.isPlaying == false)
            return;

        //If is not the player, ignore
        if (collider.gameObject.layer != PLAYER_LAYER)
            return;

        //Stop the movement coroutine
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementCoroutine = null;

        //Disable the platform
        mobilePlatformTransform.gameObject.SetActive(false);
    }

    private IEnumerator MovementLoop()
    {
        //If not decided side to move, decide now
        if (movingTo == -1)
            movingTo = Random.Range(0, 2);

        //Start the movement loop
        while (true)
        {
            //If is needed to move to end..
            if (movingTo == 0)
            {
                //Move the platform in the direction of the target
                mobilePlatformRigidbody.MovePosition(mobilePlatformTransform.position + ((platformEnd.position - mobilePlatformTransform.position).normalized * movementSpeed * Time.deltaTime));

                //If the distance to destination, is closer, change to move to other side
                if (Vector3.Distance(mobilePlatformTransform.position, platformEnd.position) <= 1.0f)
                    movingTo = 1;
            }
            //If is needed to move to start...
            if (movingTo == 1)
            {
                //Move the platform in the direction of the target
                mobilePlatformRigidbody.MovePosition(mobilePlatformTransform.position + ((platformStart.position - mobilePlatformTransform.position).normalized * movementSpeed * Time.deltaTime));

                //If the distance to destination, is closer, change to move to other side
                if (Vector3.Distance(mobilePlatformTransform.position, platformStart.position) <= 1.0f)
                    movingTo = 0;
            }

            //Wait for the next frame (FixedUpdate)
            yield return new WaitForFixedUpdate();
        }

        //...
    }
}