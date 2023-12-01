using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSpawner : MonoBehaviour
{
    //Public variables
    public AirObject[] cloudsPool;
    public AirObject[] ballonsPool;
    public AirObject[] birdsPool;
    public Transform cloudsSpawnPoint;
    public Transform ballonsSpawnPoint;
    public Transform birdsSpawnPoint;

    //Core methods

    void Start()
    {
        //Start the clouds spawner loop
        StartCoroutine(CloudsSpawnerLoop());
        StartCoroutine(BallonsSpawnerLoop());
        StartCoroutine(BirdsSpawnerLoop());
    }

    void Update()
    {

    }

    private IEnumerator CloudsSpawnerLoop()
    {
        //Put the initial clouds
        int initialCloudsCount = Random.Range(2, 5);
        int cloudsPlaced = 0;

        //Put all initial clouds
        while (cloudsPlaced < initialCloudsCount)
        {
            //Find a random cloud to place
            AirObject targetCloud = cloudsPool[Random.Range(0, cloudsPool.Length)];

            //If is already enabled, continues
            if (targetCloud.gameObject.activeSelf == true)
                continue;

            //Move it
            targetCloud.thisObjectTransform.position = new Vector3(Mathf.Lerp(-10, 35, Random.Range(0.0f, 1.0f)), (cloudsSpawnPoint.position.y + (Random.Range(-2.5f, 3.5f))), cloudsSpawnPoint.position.z);

            //Set the scale
            float targetScale = (Random.Range(0.8f, 1.5f));
            targetCloud.thisObjectTransform.localScale = new Vector3(targetScale, targetScale, targetScale);

            //Enable it
            targetCloud.gameObject.SetActive(true);

            //Increase the clouds placed counter
            cloudsPlaced += 1;
        }

        //Create the loop
        while (true)
        {
            //Try to find a disabled cloud
            AirObject targetCloud = null;
            foreach (AirObject cloud in cloudsPool)
                if (cloud.gameObject.activeSelf == false)
                    targetCloud = cloud;

            //If found, spawn it
            if (targetCloud != null)
            {
                //Move it
                targetCloud.thisObjectTransform.position = new Vector3(cloudsSpawnPoint.position.x, (cloudsSpawnPoint.position.y + (Random.Range(-2.5f, 3.5f))), cloudsSpawnPoint.position.z);

                //Set the scale
                float targetScale = (Random.Range(0.8f, 1.5f));
                targetCloud.thisObjectTransform.localScale = new Vector3(targetScale, targetScale, targetScale);

                //Enable it
                targetCloud.gameObject.SetActive(true);
            }

            //Wait a random interval
            yield return new WaitForSeconds(Random.Range(30.0f, 80.0f));
        }
    }

    private IEnumerator BallonsSpawnerLoop()
    {
        //Put the initial ballons
        int initialBallonsCount = Random.Range(1, 4);
        int ballonsPlaced = 0;

        //Put all initial ballons
        while (ballonsPlaced < initialBallonsCount)
        {
            //Find a random ballon to place
            AirObject targetBallon = ballonsPool[Random.Range(0, ballonsPool.Length)];

            //If is already enabled, continues
            if (targetBallon.gameObject.activeSelf == true)
                continue;

            //Move it
            targetBallon.thisObjectTransform.position = new Vector3(Mathf.Lerp(-10, 35, Random.Range(0.0f, 1.0f)), (ballonsSpawnPoint.position.y + (Random.Range(-2.5f, 2.5f))), ballonsSpawnPoint.position.z - (Random.Range(0.0f, 20.0f)));

            //Set the scale
            float targetScale = (Random.Range(0.25f, 1.0f));
            targetBallon.thisObjectTransform.localScale = new Vector3(targetScale, targetScale, targetScale);

            //Enable it
            targetBallon.gameObject.SetActive(true);

            //Increase the ballons placed counter
            ballonsPlaced += 1;
        }

        //Create the loop
        while (true)
        {
            //Wait a random interval
            yield return new WaitForSeconds(Random.Range(45.0f, 120.0f));

            //Try to find a disabled ballon
            AirObject targetBallon = null;
            foreach (AirObject ballon in ballonsPool)
                if (ballon.gameObject.activeSelf == false)
                    targetBallon = ballon;

            //If found, spawn it
            if (targetBallon != null)
            {
                //Move it
                targetBallon.thisObjectTransform.position = new Vector3(ballonsSpawnPoint.position.x, (ballonsSpawnPoint.position.y + (Random.Range(-2.5f, 2.5f))), ballonsSpawnPoint.position.z - (Random.Range(0.0f, 20.0f)));

                //Set the scale
                float targetScale = (Random.Range(0.25f, 1.0f));
                targetBallon.thisObjectTransform.localScale = new Vector3(targetScale, targetScale, targetScale);

                //Enable it
                targetBallon.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator BirdsSpawnerLoop()
    {
        //Create the loop
        while (true)
        {
            //Try to find a disabled bird
            AirObject targetBird = null;
            foreach (AirObject bird in birdsPool)
                if (bird.gameObject.activeSelf == false)
                    targetBird = bird;

            //If found, spawn it
            if (targetBird != null)
            {
                //Move it
                targetBird.thisObjectTransform.position = new Vector3(birdsSpawnPoint.position.x, (birdsSpawnPoint.position.y + (Random.Range(-2.5f, 2.5f))), birdsSpawnPoint.position.z);

                //Set the scale
                float targetScale = (Random.Range(0.5f, 1.0f));
                targetBird.thisObjectTransform.localScale = new Vector3(targetScale, targetScale, targetScale);

                //Enable it
                targetBird.gameObject.SetActive(true);
            }

            //Wait a random interval
            yield return new WaitForSeconds(Random.Range(1.0f, 20.0f));
        }
    }

}