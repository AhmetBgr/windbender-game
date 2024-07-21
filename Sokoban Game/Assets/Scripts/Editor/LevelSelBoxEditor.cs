using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelSelectionBox))]
[CanEditMultipleObjects]
public class LevelSelBoxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //do this first to make sure you have the latest version
        //serializedObject.Update();

        DrawDefaultInspector();

        LevelSelectionBox t = target as LevelSelectionBox;
        

        /*if (t.text != null && t.level != null)
        {
            t.text.text = t.level.name;
            EditorUtility.SetDirty(t.text);
        }*/
            
        if (t.debugNameText != null && t.level != null)
        {
            t.debugNameText.text = EditorGUILayout.TextField("name: ", t.level.debugName);
            EditorUtility.SetDirty(t.debugNameText);
        }

        //do this last!  it will loop over the properties on your object and apply any it needs to, no if necessary!
        //serializedObject.ApplyModifiedProperties();
    }

}
