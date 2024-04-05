using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "New Area Data")]
public class AreaData : ScriptableObject
{
    public GameObject areaObj;
    public Level unlocker;

    public bool isUnlocked = false;

    public void SetUnlocked() {
        isUnlocked = true;
        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
        #endif
    }
}
