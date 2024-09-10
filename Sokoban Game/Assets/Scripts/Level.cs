using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CreateAssetMenu(menuName = "New Level Data", order = 1)]
public class Level : ScriptableObject
{
    //public string sceneName;
    public string debugName;
    //public int sceneIndex;
    public State state;
    public bool seen = false;
    public bool justUnlocked = true;
    public bool isFirstLevel;
    public Level[] connectedLevels;

    [TextArea] public string info;


    public enum State {
        locked, unlocked, completed
    }

    private void Awake()
    {
        /*bool overrideSaveWithSO = false;

        #if UNITY_EDITOR
            overrideSaveWithSO = true;
            Debug.LogWarning("Unity Editor: " + this);
        #endif
        */
        if (File.Exists(Application.persistentDataPath + LevelManager.levelDataFolderName + name + ".save")) { //&& !overrideSaveWithSO
            LoadAndSetLevelData();


        }
        else {
            // Generates default level data
            state = isFirstLevel ? State.unlocked : State.locked;
            seen = false;
            SaveLevelData();
        }
            
    }

    private void OnDisable()
    {
        justUnlocked = true;
    }

    public void SetComplete()
    {
        state = Level.State.completed;

        foreach(Level level in connectedLevels)
        {
            level.Unlock();
        }

        SaveLevelData();

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void Unlock()
    {
        if (state != State.locked) return;

        state = Level.State.unlocked;
        justUnlocked = true;
        SaveLevelData();

        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
        #endif
    }

    public void SaveLevelData(bool isFirstLevel = false)
    {
        LevelData levelData = GenerateLevelData(state, seen, isFirstLevel);    

        Utility.BinarySerialization(LevelManager.levelDataFolderName, levelData.levelName, levelData);
    }

    private LevelData GenerateLevelData(State state, bool seen, bool isFirstLevel = false)
    {
        LevelData levelData = new LevelData();
        levelData.levelName = debugName;
        levelData.sceneName = name;
        //levelData.sceneIndex = sceneIndex;
        levelData.state = isFirstLevel ? (int)State.unlocked : (int)state;
        levelData.seen = seen;

        return levelData;
    }

    private LevelData LoadAndSetLevelData()
    {
        LevelData levelData = (LevelData)Utility.BinaryDeserialization(LevelManager.levelDataFolderName, name);
        //sceneName = levelData.sceneName;
        debugName = levelData.levelName;
        //sceneIndex = levelData.sceneIndex;

        if(levelData.state == 0)
            state = State.locked;
        else if(levelData.state == 1)
            state = State.unlocked;
        else
            state = State.completed;

        return levelData;
        
    }

}
