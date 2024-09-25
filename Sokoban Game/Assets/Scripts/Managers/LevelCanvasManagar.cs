using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelCanvasManagar : MonoBehaviour
{
    public GameObject lastSelectedLevelIndicator;
    public TextMeshProUGUI leafCounterText;
    private void OnEnable()
    {
        ///LevelSelectionBox.OnLevelSelect += ShowIndicator;
        LevelSelectionBox.OnHover += ShowIndicator;
        SettingsManager.OnLeafCountChanged += UpdateLeafCounterText;
        ///LevelSelectionBox.OnHoverExit += HideIndicator;

        UpdateLeafCounterText(MainUIManager.instance.settings.settingsHolder.settings.leafCount);

        if (LevelManager.instance.previousLevel != null)
            ShowIndicator(LevelManager.instance.previousLevel, LevelManager.instance.previousLevelPos);
    }

    private void OnDisable()
    {
        //LevelSelectionBox.OnLevelSelect -= ShowIndicator;
        SettingsManager.OnLeafCountChanged -= UpdateLeafCounterText;

        LevelSelectionBox.OnHover -= ShowIndicator;
    }

    private void ShowIndicator(Level level, Vector3 pos)
    {
        lastSelectedLevelIndicator.SetActive(true);
        lastSelectedLevelIndicator.transform.position = pos;
    }

    private void UpdateLeafCounterText(int leafCount) {
        leafCounterText.text = leafCount.ToString();
    }
}
