using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GameplayUIManager : MonoBehaviour
{
    public TextMeshProUGUI turnCountText;
    public GameObject pausedPanel;
    public Button undoButton;
    public Button blowButton;
    public Button waitButton;
    public Button returnToLevelSelButton;
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

        undoButton.onClick.AddListener(GameManager.instance.Undo);

        //waitButton.onClick.AddListener(GameManager.instance.WaitATurn);
        EventTrigger trigger = waitButton.GetComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { OnWaitButtonDownDelegate((PointerEventData)data); });
        trigger.triggers.Add(entry);

        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        entry2.callback.AddListener((data) => { OnWaitButtonUpDelegate((PointerEventData)data); });
        trigger.triggers.Add(entry2);


        if (LevelManager.instance != null)
        {
            returnToLevelSelButton.onClick.AddListener(() => LevelManager.instance._LoadOverWorld(MainUIManager.instance.transitionProperty2));
        }

        pausedPanel.SetActive(true);
    }

    private void OnWaitButtonDownDelegate(PointerEventData data)
    {
        GameManager.instance.StartWaiting();
    }

    private void OnWaitButtonUpDelegate(PointerEventData data)
    {
        //waitButton.gameObject.SetActive(false);
        GameManager.instance.StopWaiting();
    }

    private void UpdateTurnCounterText(int turnCount)
    {
        if (turnCount <= 0 | GameManager.instance.isWaiting)
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
        if (to == GameState.DrawingRoute){
            routeDrawingPanel.gameObject.SetActive(true);
        }
        else{
            routeDrawingPanel.gameObject.SetActive(false);
        }
    }

    private void TogglePausedText(GameState from, GameState to)
    {
        if (to == GameState.Running) 
        {
            pausedPanel.SetActive(false);
            //if (GameManager.instance.isWaiting) return;
            //waitButton.gameObject.SetActive(false);
        }
        else
        {
            //if (pausedPanel.activeInHierarchy) return;
            //StartCoroutine(Utility.SetActiveObjWithDelay(pausedPanel, true, GameManager.instance.turnDur));
            pausedPanel.SetActive(true);
            if (waitButton.gameObject.activeInHierarchy) return;
            //StartCoroutine(Utility.SetActiveObjWithDelay(waitButton.gameObject, true, GameManager.instance.turnDur));
            waitButton.gameObject.SetActive(true);
            //pausedPanel.SetActive(true);
            //waitButton.gameObject.SetActive(true);
        }
    }

    public void ToggleBlowButton(bool value)
    {
        blowButton.gameObject.SetActive(value);

        if(value || GameManager.instance.state != GameState.Running)
            waitButton.gameObject.SetActive(!value);
       

        if (blowButtonTween != null)
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
