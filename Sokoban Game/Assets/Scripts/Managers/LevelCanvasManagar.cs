using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCanvasManagar : MonoBehaviour
{
    public GameObject lastSelectedLevelIndicator;

    private void OnEnable()
    {
        ///LevelSelectionBox.OnLevelSelect += ShowIndicator;
        LevelSelectionBox.OnHover += ShowIndicator;
        ///LevelSelectionBox.OnHoverExit += HideIndicator;

        if (LevelManager.instance.previousLevel != null)
            ShowIndicator(LevelManager.instance.previousLevel, LevelManager.instance.previousLevelPos);
    }

    private void OnDisable()
    {
        //LevelSelectionBox.OnLevelSelect -= ShowIndicator;

        LevelSelectionBox.OnHover -= ShowIndicator;
    }

    private void ShowIndicator(Level level, Vector3 pos)
    {
        lastSelectedLevelIndicator.SetActive(true);
        lastSelectedLevelIndicator.transform.position = pos;
    }
}
