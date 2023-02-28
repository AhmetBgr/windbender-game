using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelSelectionBox))]
public class LevelSelBoxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelSelectionBox t = target as LevelSelectionBox;
        t.text.text = t.level.name;
        t.debugNameText.text = t.level.debugName;
    }
}
