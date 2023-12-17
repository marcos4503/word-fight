using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SyllabeDisplay : MonoBehaviour
{
    //Classes of script
    [System.Serializable]
    public class CharInfo
    {
        public string character;
        public Mesh charMesh;
        public float charWidth;
    }

    //Cache variables
    private bool isInitialized = false;
    private Transform thisTransform;
    private Material materialInstance;

    //Private variables
    private WaitForSeconds rotationInterval = new WaitForSeconds(0.0416f);

    //Public variables
    public CharInfo[] lettersDatabase;
    public Transform syllabePivot;
    public Material materialToUse;

    //Core methods

    void Start()
    {
        //If is already initialized, cancel
        if (isInitialized == true)
            return;

        //Create a instance of the material for the spider, if don't have a instanced material
        if (materialInstance == null)
            materialInstance = Instantiate(materialToUse);
        materialInstance.name = (materialInstance.name + "(Instance)");

        //Get reference for this transform
        thisTransform = this.gameObject.transform;

        //Start the rotation loop
        StartCoroutine(SyllabeRotation());

        //Inform that is initialized
        isInitialized = true;
    }

    private CharInfo GetLetterInfo(string letter)
    {
        //Prepare the letter
        CharInfo charInfo = null;

        //Search the letter in database
        for (int i = 0; i < lettersDatabase.Length; i++)
            if (letter == lettersDatabase[i].character)
            {
                charInfo = lettersDatabase[i];
                break;
            }

        //Return the char info
        return charInfo;
    }

    private IEnumerator SyllabeRotation()
    {
        //Create a rotation loop
        while (true)
        {
            //Wait the rotation interval
            yield return rotationInterval;

            //Rotate the syllabe
            thisTransform.Rotate(0.0f, 5.0f, 0.0f);
        }
    }

    //Public methods

    public void BuildAndShowSyllabe(string syllabe, Color syllabeColor)
    {
        //If is not initialized, initialize it
        if (isInitialized == false)
            Start();

        //Current X position
        float currentX = 0.0f;

        //Create all letters
        for (int i = 0; i < syllabe.Length; i++)
        {
            //Find the current letter in letters database
            CharInfo currentLetterInfo = GetLetterInfo(syllabe[i].ToString());

            //Create the GameObject
            GameObject charGo = new GameObject(syllabe[i].ToString());
            charGo.transform.SetParent(syllabePivot);
            Transform charTransform = charGo.transform;
            MeshFilter meshFilter = charGo.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = charGo.AddComponent<MeshRenderer>();

            //Setup the transform
            charTransform.localPosition = new Vector3(currentX, 0.0f, 0.0f);
            charTransform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            charTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            //Setup the mesh filter
            meshFilter.sharedMesh = currentLetterInfo.charMesh;

            //Setup the mesh renderer
            meshRenderer.sharedMaterials = new Material[] { materialInstance };
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            //Increase the next letter position
            currentX += currentLetterInfo.charWidth;
        }

        //Adjust the size and position of letters pivot
        float desiredScaleForSyllabe = 1.5f;
        syllabePivot.localPosition = new Vector3(((currentX * desiredScaleForSyllabe) / 2.0f) * -1.0f, 0.0f, 0.0f);
        syllabePivot.localScale = new Vector3(desiredScaleForSyllabe, desiredScaleForSyllabe, desiredScaleForSyllabe);

        //Define the material color
        materialInstance.SetColor("_Color", syllabeColor);
    }
}