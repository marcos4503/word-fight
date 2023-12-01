using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaterMakerGame : MonoBehaviour
{
    //Private variables
    private Material instancedMaterial;
    private float lastWidthOfWater;
    private float lastHeightOfWater;
    private Transform thisTransform;

    //Public variables
    public Material originalMaterial;
    public MeshRenderer waterMeshRenderer;
    public float widthDivider = 2.0f;
    public float heightDivider = 1.0f;
    public float waterSpeed = 2.0f;
    public Transform groundTransform;
    public float groundDistance = 3.0f;
    public Transform waterTransform;
    public float waterDistance = 0.35f;
    public ParticleSystem dropsParticle;

    void Start()
    {
        //If don't have a instanced material, create one
        if (instancedMaterial == null)
            instancedMaterial = Instantiate(originalMaterial);
        instancedMaterial.name = (instancedMaterial.name + "(Instance)");

        //Put it in the mesh
        waterMeshRenderer.sharedMaterials = new Material[] { instancedMaterial };

        //Get reference for this transform
        thisTransform = this.gameObject.transform;
    }

    void Update()
    {
        //If not runned start, run it
        if (instancedMaterial == null)
        {
            Start();
            return;
        }

        //If the width was changed...
        if (lastWidthOfWater != thisTransform.localScale.x)
        {
            //Change the tiling of the material
            instancedMaterial.mainTextureScale = new Vector2((thisTransform.localScale.x / widthDivider), instancedMaterial.mainTextureScale.y);

            //Inform the new width
            lastWidthOfWater = thisTransform.localScale.x;
        }

        //If the height was changed...
        if (lastHeightOfWater != thisTransform.localScale.y)
        {
            //Change the tiling of the material
            instancedMaterial.mainTextureScale = new Vector2(instancedMaterial.mainTextureScale.x, (thisTransform.localScale.y / heightDivider));

            //Mantain the ground distance at same desired
            groundTransform.position = new Vector3(groundTransform.position.x, (thisTransform.position.y - groundDistance), groundTransform.position.z);
            //Mantain the water distance at same desired
            waterTransform.position = new Vector3(waterTransform.position.x, (thisTransform.position.y - waterDistance), waterTransform.position.z);
            //Mantain the drops particles area ate same desired
            ParticleSystem.ShapeModule shapeModule = dropsParticle.shape;
            shapeModule.scale = new Vector3(thisTransform.localScale.x, shapeModule.scale.y, shapeModule.scale.z);

            //Inform the new width
            lastHeightOfWater = thisTransform.localScale.y;
        }

        //Move the water
        instancedMaterial.mainTextureOffset += new Vector2(0, 2.0f * Time.deltaTime * -1.0f);
    }

    void OnEnable()
    {
        //Reset component
        instancedMaterial = null;
        lastHeightOfWater = float.MaxValue;
        lastWidthOfWater = float.MaxValue;
        thisTransform = null;

        //Recall the start
        Start();
    }
}