using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class LevelManager : MonoBehaviour
{
    public SceneLoader SceneLoader;
    public LevelData[] levels;

    public LevelData curlevel;

    private void OnEnable()
    {
        GameManager.instance.OnLevelComplete += LoadOverWorld;
    }

    private void OnDisable()
    {
        GameManager.instance.OnLevelComplete -= LoadOverWorld;
    }

    void Start()
    {
        
    }


    public void SetComplete(LevelData level)
    {
        level.state = LevelData.State.completed;
        EditorUtility.SetDirty(level);
    }

    public void LoadOverWorld()
    {
        SceneLoader.LoadSceneWithName("OverWorld");
    }
}
