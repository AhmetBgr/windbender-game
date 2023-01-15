using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LevelSelectionBox : MonoBehaviour
{
    //public int levelSceneIndex;
    //public State state;
    //public Button button;
    public TextMeshProUGUI text;
    public LevelData level;

    // Start is called before the first frame update
    public LevelSelectionBox[] unlocks;

    void Start()
    {
        // get state 

        // set state
        //state = State.unlocked;

        /*if(level.state == LevelData.State.locked)
        {
            button.interactable = false;
        }*/

        text.text = level.name;
    }
    

    public void OnMouseDown()
    {
        if (level.state == LevelData.State.locked) return;

        // load Level
        //SceneLoader.LoadSceneWithName(level.name);
        SceneLoader.LoadSceneWithIndex(level.sceneIndex);
    }

    public void OnClick()
    {
        //if (level.state == LevelData.State.locked) return;

        // load Level
        //SceneLoader.LoadSceneWithName(level.name);
    }

    public void OnComplete()
    {
        // unlock next levels

    }

    public void Unlock()
    {

    }

    /*public enum State {
        locked,
        unlocked,
        completed
    }*/


}
