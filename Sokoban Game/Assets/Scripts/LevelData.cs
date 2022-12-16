using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "New Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    public string name;
    public int sceneIndex;
    public State state;

    public enum State {
        locked, unlocked, completed
    }
}
