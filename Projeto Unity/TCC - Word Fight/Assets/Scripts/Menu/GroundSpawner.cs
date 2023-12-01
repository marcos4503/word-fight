using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    //Cache variables
    private Transform lastGroundEndSpawned;

    //Public variables
    public GameObject groundPoolRoot;
    public GroundObject[] groundPool;

    //Core methods
    public Transform groundsSpawnPoint;

    void Start()
    {
        //Find all ground objects
        groundPool = groundPoolRoot.GetComponentsInChildren<GroundObject>(true);
        //Disable all ground objects
        foreach (GroundObject obj in groundPool)
            obj.gameObject.SetActive(false);
        //Enable the first ground object
        groundPool[0].thisObjectTransform.position = new Vector3(-14, 0, 0);
        groundPool[0].gameObject.SetActive(true);

        //Get a random ground part
        GroundObject groundPart = GetRandomDisabledGroundPart();

        //Spawn first ground part
        groundPart.thisObjectTransform.position = new Vector3(36.0f, groundsSpawnPoint.position.y, groundsSpawnPoint.position.z);
        lastGroundEndSpawned = groundPart.thisObjectEnd;
        groundPart.gameObject.SetActive(true);
    }

    void Update()
    {
        DoTerrainGenerationProccess();
    }

    private GroundObject GetRandomDisabledGroundPart()
    {
        //Prepare the ground object to return
        GroundObject targetGroundObj = null;

        //Prepare a loop to get a random ground part
        while (true)
        {
            //Get a random ground part
            targetGroundObj = groundPool[Random.Range(0, groundPool.Length)];

            //If the target ground part is disabled, can continue
            if (targetGroundObj.gameObject.activeSelf == false)
                break;
        }

        //Return the object
        return targetGroundObj;
    }

    private void DoTerrainGenerationProccess()
    {
        //If the end of the last spawned ground part has passed from spawn point, spawn more one ground part
        if (lastGroundEndSpawned.position.x > (groundsSpawnPoint.position.x + 1.5f))
            return;

        //Get a random ground part
        GroundObject groundPart = GetRandomDisabledGroundPart();

        //Spawn the new ground part
        groundPart.thisObjectTransform.position = new Vector3(groundsSpawnPoint.position.x, groundsSpawnPoint.position.y, groundsSpawnPoint.position.z);
        lastGroundEndSpawned = groundPart.thisObjectEnd;
        groundPart.gameObject.SetActive(true);
    }
}
