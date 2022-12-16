using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI turnCountText;
    public GameObject pausedPanel;
    public Button blowButton;
    public Transform routeDrawingPanel;
    Tween blowButtonTween;
    private void OnEnable()
    {
        GameManager.instance.OnTurnCountChange += UpdateTurnCounterText;
        GameManager.instance.OnDrawingCompleted += ToggleBlowButton;
        GameManager.instance.OnStateChange += TogglePausedText;
        //GameManager.instance.OnStateChange += ToggleRouteDrawingPanel;
    }
    private void OnDisable()
    {
        GameManager.instance.OnTurnCountChange -= UpdateTurnCounterText;
        GameManager.instance.OnDrawingCompleted -= ToggleBlowButton;
        GameManager.instance.OnStateChange -= TogglePausedText;
        //GameManager.instance.OnStateChange += ToggleRouteDrawingPanel;
    }

    void Start()
    {
        turnCountText.gameObject.SetActive(false);
        blowButton.onClick.AddListener(() =>
        {
            GameManager.instance.StartWindBlow();
        });

        pausedPanel.SetActive(true);
    }

    private void UpdateTurnCounterText(int turnCount)
    {
        if (turnCount <= 0)
        {
            turnCountText.gameObject.SetActive(false);
        }
        else
        {
            turnCountText.gameObject.SetActive(true);
            turnCountText.text = turnCount.ToString();
        }
    }

    public void OnUndoButtonDown()
    {
        GameManager.instance.Undo();
    }

    private void ToggleRouteDrawingPanel(GameState from, GameState to)
    {
        if (to == GameState.DrawingRoute)
        {
            routeDrawingPanel.gameObject.SetActive(true);
        }
        else
        {
            routeDrawingPanel.gameObject.SetActive(false);
        }
    }

    private void TogglePausedText(GameState from, GameState to)
    {
        if (to == GameState.Running)
            pausedPanel.SetActive(false);
        else
            StartCoroutine(Utility.SetActiveObjWithDelay(pausedPanel, true, GameManager.instance.turnDur));
    }

    public void ToggleBlowButton(bool value)
    {
        blowButton.gameObject.SetActive(value);

        if(blowButtonTween != null)
        {
            blowButtonTween.Kill();
            blowButton.transform.rotation = Quaternion.identity;
        }
        if (value)
        {
            blowButtonTween = blowButton.transform.DOShakeRotation(1f, strength: 10, fadeOut: false).SetLoops(-1);
        }
    }

}
