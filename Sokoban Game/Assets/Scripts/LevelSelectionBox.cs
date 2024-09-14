using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LevelSelectionBox : MonoBehaviour
{
    //public int levelSceneIndex;
    //public State state;
    public Button button;
    public TextMeshProUGUI text;
    public TextMeshProUGUI debugNameText;
    public Level level;
    public GameObject mark;
    public Sprite markedFrame;

    static float delay = 0f;
    // Start is called before the first frame update
    //public LevelSelectionBox[] unlocks;

    public delegate void OnLevelSelectDelegate(Level level, Vector3 pos);
    public static event OnLevelSelectDelegate OnLevelSelect;

    public delegate void OnHoverDelegate(Level level, Vector3 pos);
    public static event OnHoverDelegate OnHover;

    public delegate void OnHoverExitDelegate(Level level, Vector3 pos);
    public static event OnHoverExitDelegate OnHoverExit;

    private void Awake()
    {
        delay = 0f;
    }

    void Start()
    {
        // get state 

        // set state
        //state = State.unlocked;

        if (GameDebug.allLevelsUnlocked) {
            level.Unlock();
        }


        if(level.state == Level.State.locked)
        {
            button.gameObject.SetActive(false);
        }
        else if(level.state == Level.State.completed)
        {

            //button.GetComponent<CanvasGroup>().alpha = 0.7f;
            //mark.SetActive(true);
            button.image.sprite = markedFrame;
        }
        else
        {
            button.gameObject.SetActive(true);

            if (level.justUnlocked)
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(new Vector3(0.85f, 0.85f, 1f), 1f)
                    .SetDelay(delay);

                level.justUnlocked = false;
                delay += 0.05f;
            }


        }

        //text.text = level.name;
        debugNameText.text = level.debugName;
    }
    
    public void UpdateTextUI()
    {
        debugNameText.text = level.debugName;
    }
    public void OnClick()
    {
        Debug.LogWarning("clicked");

        if (level.state == Level.State.locked) return;

        if(OnLevelSelect != null)
        {
            OnLevelSelect(level, transform.position);
        }

        //SceneLoader.LoadSceneWithIndex(level.sceneIndex);
        
    }

    public void OnMouseEnter()
    {
        if(OnHover != null)
        {
            OnHover(level, transform.position);
        }
    }

    public void OnMouseExit()
    {
        if (OnHoverExit != null)
        {
            OnHoverExit(level, transform.position);
        }
    }

}
