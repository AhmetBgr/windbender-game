using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class LevelManager : MonoBehaviour
{
    public static string levelDataFolderName = "/LevelData/";

    public SceneLoader SceneLoader;
    public Level[] levels;

    public Level previousLevel;
    public Vector3 previousLevelPos; // pos in level canvas
    public Level curLevel;

    public AreaData are02Data;
    
    public static LevelManager instance = null;

    public void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        if (GameManager.instance)
        {
            GameManager.instance.OnLevelComplete += SetCurLevelComplete;
            GameManager.instance.OnLevelComplete += LoadOverWorld;
        }

        LevelSelectionBox.OnLevelSelect += SetAndLoadCurLevel;
    }

    private void OnDisable()
    {
        if (GameManager.instance)
        {
            GameManager.instance.OnLevelComplete -= SetCurLevelComplete;
            GameManager.instance.OnLevelComplete -= LoadOverWorld;
        }


        LevelSelectionBox.OnLevelSelect -= SetAndLoadCurLevel;
    }

    void Start()
    {
        if (SceneLoader.sceneName == "-OverWorld") {

            Cursor.instance.gameObject.SetActive(false);
        }
    }

    public void SetCurLevelComplete()
    {
        if (curLevel == null) return;

        curLevel.SetComplete();
    }

    private void SetAndLoadCurLevel(Level level, Vector3 pos)
    {
        Debug.LogWarning("Level selected: " + level.name);
        MainUIManager mainUIManager = MainUIManager.instance;
        MainUIManager.TransitionProperty tp = (level.seen | level.state == Level.State.completed) 
            ? mainUIManager.transitionProperty2 : mainUIManager.transitionProperty1;

        //Cursor.instance.ShowCursor();
        Cursor.instance.gameObject.SetActive(true);

        previousLevel = curLevel == null ? level : curLevel;
        previousLevelPos = pos;
        curLevel = level;
        curLevel.seen = true;
        curLevel.SaveLevelData();
        StartCoroutine( SceneLoader.LoadAsyncSceneWithName(level.debugName, tp.durationFH,
            preLoadCallBack : () => mainUIManager.SceneTranstionFH(tp),
            onCompleteCallBack : () => mainUIManager.SceneTranstionSH(tp)) );
    }

    public void LoadOverWorld()
    {
        _LoadOverWorld(MainUIManager.instance.transitionProperty1);
    }

    public void _LoadOverWorld(MainUIManager.TransitionProperty tp)
    {
        MainUIManager mainUIManager = MainUIManager.instance;
        //Cursor.instance.HideCursor();
        StartCoroutine(SceneLoader.LoadAsyncSceneWithName("-OverWorld", tp.durationFH,
            preLoadCallBack: () => mainUIManager.SceneTranstionFH(tp),
            onCompleteCallBack: () => { 
                mainUIManager.SceneTranstionSH(tp);
                //Cursor.instance.HideCursor();
                Cursor.instance.gameObject.SetActive(false);
            }));
    }
}
