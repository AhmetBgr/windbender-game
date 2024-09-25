using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData {
    public int leafCount;
    public float gameSpeed; // 0 to 10, 1 = default, 2 = 2x
}

public class SettingsManager : MonoBehaviour {
    [HideInInspector] public SettingsData settingsData;

    public SettingsDataHolder settingsHolder;

    private GameManager gameManager;

    private string fileName = "settings.txt";

    public delegate void OnSettingsDataLoadedDelegate(SettingsData settingsData);
    public static event OnSettingsDataLoadedDelegate OnSettingsDataLoaded;

    public delegate void OnGameSpeedDataLoadedDelegate(float gamespeed);
    public static event OnGameSpeedDataLoadedDelegate OnGameSpeedDataLoaded;

    public delegate void OnLeafCountChangedDelegate(int leafCount);
    public static event OnLeafCountChangedDelegate OnLeafCountChanged;

    public void Awake()
    {
        gameManager = GameManager.instance;
        LoadSettingsData();
        OnLeafCountChanged?.Invoke(settingsData.leafCount);

    }

    private void OnEnable()
    {
        GameManager.instance.OnPlannedSpeedChanged += SaveGameSpeed;
    }

    private void OnDisable()
    {
        GameManager.instance.OnPlannedSpeedChanged -= SaveGameSpeed;
    }

    public void SaveSettingsData()
    {
        if (settingsData == null) return;

        string jsonData = JsonUtility.ToJson(settingsData, true);
        //JsonUtility.FromJsonOverwrite(jsonData, settingsData);
        
        WriteToFile(fileName, jsonData);
    }

    public void LoadSettingsData()
    {
        string json = ReadFromFIle(fileName);

        if(json == null){
            GetDefSettingsData();
            SaveSettingsData();
            Debug.LogWarning("Default settings loaded");
        }
        else
            JsonUtility.FromJsonOverwrite(json, settingsData);

        Debug.LogWarning("Settings loaded");

        settingsHolder.settings = settingsData;

        if (OnSettingsDataLoaded!= null)
        {
            // Apply loaded data
            OnSettingsDataLoaded(settingsData);
        }

        OnGameSpeedDataLoaded?.Invoke(settingsData.gameSpeed);
    }

    private void WriteToFile(string fileName, string json)
    {
        string path = GetFilePath(fileName);
        FileStream fileStream = new FileStream(path, FileMode.Create);

        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }

        fileStream.Close();
    }

    private string ReadFromFIle(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string json = reader.ReadToEnd();
                return json;
            }
        }
        else
        {
            Debug.LogWarning("File not found");
            return null;
        }

        
    }
    private void SaveGameSpeed(float gameSpeed)
    {
        settingsData.gameSpeed = gameSpeed;
        settingsHolder.settings = settingsData;
        SaveSettingsData();
    }

    public void SetLeaf(int value) {
        settingsData.leafCount = value;
        settingsHolder.settings = settingsData;
        SaveSettingsData();

        OnLeafCountChanged?.Invoke(value);
    }

    private string GetFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }

    // Generates and returns default settings data
    private SettingsData GetDefSettingsData()
    {
        
        settingsData = new SettingsData();
        settingsData.gameSpeed = 1f;
        settingsData.leafCount = 0;

        return settingsData;
    }
}



