using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[CreateAssetMenu(menuName = "New Level Data", order = 1)]
public class Level : ScriptableObject
{
    public new string name;
    public string debugName;
    public int sceneIndex;
    public State state;
    public bool seen = false;

    public Level[] connectedLevels;

    [TextArea] public string info;


    public enum State {
        locked, unlocked, completed
    }

    private void Awake()
    {
        bool overrideSaveWithSO = false;

        #if UNITY_EDITOR
            overrideSaveWithSO = true;
            Debug.LogWarning("Unity Editor: " + this);
        #endif
        
        if ( File.Exists(Application.persistentDataPath + LevelManager.levelDataFolderName + name + ".save")  && !overrideSaveWithSO)
            LoadAndSetLevelData();
        else
            SaveAndSetLevelData();
    }

    public void SetComplete()
    {
        state = Level.State.completed;

        foreach(Level level in connectedLevels)
        {
            level.Unlock();
        }

        SaveAndSetLevelData();

        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
        #endif
    }

    public void Unlock()
    {
        if (state != State.locked) return;

        state = Level.State.unlocked;
        SaveAndSetLevelData();

        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
        #endif
    }

    public void SaveAndSetLevelData()
    {
        LevelData levelData = new LevelData();
        levelData.levelName = name;
        levelData.sceneName = debugName;
        levelData.sceneIndex = sceneIndex;
        levelData.state = (int)state;
        levelData.seen = seen;

        Utility.BinarySerialization(LevelManager.levelDataFolderName, levelData.levelName, levelData);
    }

    private LevelData LoadAndSetLevelData()
    {
        LevelData levelData = (LevelData)Utility.BinaryDeserialization(LevelManager.levelDataFolderName, name);
        name = levelData.levelName;
        debugName = levelData.sceneName;
        sceneIndex = levelData.sceneIndex;

        if(levelData.state == 0)
            state = State.locked;
        else if(levelData.state == 1)
            state = State.unlocked;
        else
            state = State.completed;

        return levelData;
        
    }

}
