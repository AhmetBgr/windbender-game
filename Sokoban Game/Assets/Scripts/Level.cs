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

    public Level[] connectedLevels;

    [TextArea] public string info;

    public enum State {
        locked, unlocked, completed
    }

    private void Awake()
    {
        if( File.Exists(Application.persistentDataPath + LevelManager.levelDataFolderName + name + ".save") ){
            LoadLevelData();
        }
        else
        {
            SaveLevelData();
        }
    }

    public void SetComplete()
    {
        state = Level.State.completed;

        foreach(Level level in connectedLevels)
        {
            level.Unlock();
        }

        SaveLevelData();

        //#if UNITY_EDITOR
        //    EditorUtility.SetDirty(this);
        //#endif
    }

    public void Unlock()
    {
        if (state != State.locked) return;

        state = Level.State.unlocked;

        SaveLevelData();

        //#if UNITY_EDITOR
        //    EditorUtility.SetDirty(this);
        //#endif
    }

    private void SaveLevelData()
    {
        LevelData levelData = new LevelData();
        levelData.levelName = name;
        levelData.sceneName = debugName;
        levelData.sceneIndex = sceneIndex;
        levelData.state = (int)state;

        Utility.BinarySerialization(LevelManager.levelDataFolderName, levelData.levelName, levelData);
    }

    private void LoadLevelData()
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
        
    }

}
