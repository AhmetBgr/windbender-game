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
    public TextMeshProUGUI gameSpeedText;
    public GameObject pausedPanel;
    public Button undoButton;
    public Button restartButton;
    public Button blowButton;
    public Button waitButton;
    public Button gameSpeedButton;
    public Button returnToLevelSelButton;
    public Button playButton;
    public Button pauseButton;
    public Button singleUndoButton;
    public Button multiUndoButton;



    public Transform routeDrawingPanel;

    Tween blowButtonTween;
    private void OnEnable()
    {
        GameManager.instance.OnTurnCountChange += UpdateTurnCounterText;
        GameManager.instance.OnDrawingCompleted += ToggleBlowButton;
        GameManager.instance.OnStateChange += TogglePausedText;
        GameManager.instance.OnStateChange += UpdatePlayButton;

        GameManager.instance.OnPlannedSpeedChanged += UpdateGameSpeedText;
        //GameManager.instance.OnStateChange += TryToggleGameSpeedButton;

        //GameManager.instance.OnStateChange += ToggleRouteDrawingPanel;
    }
    private void OnDisable()
    {
        GameManager.instance.OnTurnCountChange -= UpdateTurnCounterText;
        GameManager.instance.OnDrawingCompleted -= ToggleBlowButton;
        GameManager.instance.OnStateChange -= TogglePausedText;
        GameManager.instance.OnPlannedSpeedChanged -= UpdateGameSpeedText;
        GameManager.instance.OnStateChange -= UpdatePlayButton;

        //GameManager.instance.OnStateChange -= TryToggleGameSpeedButton;
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
        restartButton.onClick.AddListener(GameManager.instance.Restart);
        playButton.onClick.AddListener(GameManager.instance.Play);
        pauseButton.onClick.AddListener(GameManager.instance.PauseWhenTurnEnd);
        singleUndoButton.onClick.AddListener(GameManager.instance.UndoSingleStep);
        multiUndoButton.onClick.AddListener(GameManager.instance.UndoMultiStep);

        gameSpeedButton.onClick.AddListener(() => GameManager.instance.SetNextGameSpeed());
        //gameSpeedButton.onClick.AddListener(UpdateGameSpeedText);
        //GameManager.instance.UpdatePlannedGameSpeed();
        UpdateGameSpeedText(GameManager.instance.plannedGameSpeed);
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

    private void TryToggleGameSpeedButton(GameState from, GameState to) {
        bool active = to == GameState.Running;

        gameSpeedButton.gameObject.SetActive(active);
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

    private void UpdateGameSpeedText(float value)
    {
        gameSpeedText.text = value.ToString() + "x";
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
        }
        else
        {
            pausedPanel.SetActive(true);
            //if (waitButton.gameObject.activeInHierarchy) return;
            //waitButton.gameObject.SetActive(true);
        }
    }

    private void UpdatePlayButton(GameState from, GameState to) {
        bool isplaying = (to == GameState.Running);

        playButton.gameObject.SetActive(!isplaying);
        pauseButton.gameObject.SetActive(isplaying);

        playButton.interactable = !(to == GameState.DrawingRoute);
    }

    public void ToggleBlowButton(bool value)
    {
        blowButton.gameObject.SetActive(value);

        //if(value || GameManager.instance.state != GameState.Running)
            //waitButton.gameObject.SetActive(!value);
       

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
