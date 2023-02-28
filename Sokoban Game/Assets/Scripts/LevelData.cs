using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[CreateAssetMenu(menuName = "New Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    public new string name;
    public string debugName;
    public int sceneIndex;
    public State state;

    public LevelData[] connectedLevels;

    [TextArea] public string info;

    public enum State {
        locked, unlocked, completed
    }

    public void SetComplete()
    {
        state = LevelData.State.completed;

        foreach(LevelData level in connectedLevels)
        {
            level.Unlock();
        }

        EditorUtility.SetDirty(this);
    }

    public void Unlock()
    {
        if (state != State.locked) return;

        state = LevelData.State.unlocked;
        EditorUtility.SetDirty(this);
    }
}
