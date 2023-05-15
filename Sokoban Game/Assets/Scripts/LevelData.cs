using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData 
{
    public string levelName;
    public string sceneName;
    public int sceneIndex;
    public int state;  // 0 = locked, 1 = unlocked, 2 = completed
    public bool seen;
}
