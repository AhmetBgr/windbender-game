using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionBox : MonoBehaviour
{
    //public int levelSceneIndex;
    //public State state;
    public Button button;
    public TextMeshProUGUI text;
    public TextMeshProUGUI debugNameText;
    public Level level;

    // Start is called before the first frame update
    //public LevelSelectionBox[] unlocks;

    public delegate void OnLevelSelectDelegate(Level level);
    public static event OnLevelSelectDelegate OnLevelSelect;

    void Start()
    {
        // get state 

        // set state
        //state = State.unlocked;

        if(level.state == Level.State.locked)
        {
            button.gameObject.SetActive(false);
        }
        else if(level.state == Level.State.completed)
        {
            button.GetComponent<CanvasGroup>().alpha = 0.8f;
        }
        else
        {
            button.gameObject.SetActive(true);
        }

        text.text = level.name;
        debugNameText.text = level.debugName;
    }
    

    public void OnClick()
    {
        Debug.LogWarning("clicked");

        if (level.state == Level.State.locked) return;

        if(OnLevelSelect != null)
        {
            OnLevelSelect(level);
        }

        //SceneLoader.LoadSceneWithIndex(level.sceneIndex);
        SceneLoader.LoadSceneWithName(level.debugName);
    }





}
