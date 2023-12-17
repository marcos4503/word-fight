using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    //Classes of script

    [Serializable]
    public class LevelInfo
    {
        public bool finished = false;
        public bool star1 = false;
        public bool star2 = false;
        public bool star3 = false;
    }

    [Serializable]
    public class SerializableSaveGame
    {
        public LevelInfo[] gameLevels;
        public int healthPotions = 0;
    }

    //Public static variables

    public static LevelInfo[] gameLevels = new LevelInfo[100];
    public static int healthPotions = 0;

    //Core methods

    public static void LoadData()
    {
        //If the save file don't exists, create one
        if (File.Exists((Application.persistentDataPath + "/wf-2023-v1.wf")) == false)
            SaveData();

        //Load the JSON from the file and convert the loaded content to a object
        string loadedJson = File.ReadAllText((Application.persistentDataPath + "/wf-2023-v1.wf"));
        SerializableSaveGame gameData = JsonUtility.FromJson<SerializableSaveGame>(loadedJson);

        //Unpack the current loaded save game to variables
        gameLevels = gameData.gameLevels;
        healthPotions = gameData.healthPotions;
    }

    public static void SaveData()
    {
        //Packs the current loaded save game to serializable class
        SerializableSaveGame gameData = new SerializableSaveGame();
        gameData.gameLevels = gameLevels;
        gameData.healthPotions = healthPotions;

        //Convert the packed game data for JSON and save into a file
        string resultJson = JsonUtility.ToJson(gameData);
        File.WriteAllText((Application.persistentDataPath + "/wf-2023-v1.wf"), resultJson);
    }
}