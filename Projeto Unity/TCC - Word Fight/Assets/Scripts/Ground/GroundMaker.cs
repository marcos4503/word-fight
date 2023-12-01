using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GroundMaker : MonoBehaviour
{
    //Private variables
    private Material instancedMaterial;
    private float lastWidthOfGround;
    private float lastHeightOfGround;
    private Transform thisTransform;

    //Public variables
    public Material originalMaterial;
    public MeshRenderer groundMeshRenderer;
    public float widthDivider = 2.0f;
    public float heightDivider = 1.0f;

    void Start()
    {
        //If don't have a instanced material, create one
        if (instancedMaterial == null)
            instancedMaterial = Instantiate(originalMaterial);
        instancedMaterial.name = (instancedMaterial.name + "(Instance)");

        //Put it in the mesh
        groundMeshRenderer.sharedMaterials = new Material[] { instancedMaterial };

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
        if (lastWidthOfGround != thisTransform.localScale.x)
        {
            //Change the tiling of the material
            instancedMaterial.mainTextureScale = new Vector2((thisTransform.localScale.x / widthDivider), instancedMaterial.mainTextureScale.y);

            //Inform the new width
            lastWidthOfGround = thisTransform.localScale.x;
        }

        //If the height was changed...
        if (lastHeightOfGround != thisTransform.localScale.y)
        {
            //Change the tiling of the material
            instancedMaterial.mainTextureScale = new Vector2(instancedMaterial.mainTextureScale.x, (thisTransform.localScale.y / heightDivider));

            //Inform the new width
            lastHeightOfGround = thisTransform.localScale.y;
        }
    }

    void OnEnable()
    {
        //Reset component
        instancedMaterial = null;
        lastHeightOfGround = float.MaxValue;
        lastWidthOfGround = float.MaxValue;
        thisTransform = null;

        //Recall the start
        Start();
    }

}