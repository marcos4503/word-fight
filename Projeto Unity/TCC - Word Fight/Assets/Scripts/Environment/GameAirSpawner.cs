using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAirSpawner : MonoBehaviour
{
    //Public variables
    public GameAirObject[] cloudsPool;
    public Transform cloudsSpawnPoint;

    void Start()
    {
        //Start the clouds spawner loop
        StartCoroutine(CloudsSpawnerLoop());
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
            GameAirObject targetCloud = cloudsPool[Random.Range(0, cloudsPool.Length)];

            //If is already enabled, continues
            if (targetCloud.gameObject.activeSelf == true)
                continue;

            //Move it
            targetCloud.thisObjectTransform.position = new Vector3(0, (cloudsSpawnPoint.position.y + (Random.Range(-2.5f, 2.5f))), cloudsSpawnPoint.position.z);
            targetCloud.thisObjectTransform.localPosition = new Vector3(Mathf.Lerp(-40, 40, Random.Range(0.0f, 1.0f)), targetCloud.thisObjectTransform.localPosition.y, targetCloud.thisObjectTransform.localPosition.z);

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
            GameAirObject targetCloud = null;
            foreach (GameAirObject cloud in cloudsPool)
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
}