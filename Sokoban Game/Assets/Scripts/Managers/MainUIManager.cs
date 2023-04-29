using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainUIManager : MonoBehaviour
{
    public Image sceneTransitionPanel;

    public static MainUIManager instance = null;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        instance.SceneTranstion2();
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        //GameManager.instance.OnLevelComplete += SceneTranstion;
        SceneLoader.OnSceneLoad += SceneTranstion;
        SceneLoader.OnSceneLoadComplete += SceneTranstion2;
    }
    private void OnDisable()
    {
        //GameManager.instance.OnLevelComplete -= SceneTranstion;
        SceneLoader.OnSceneLoad -= SceneTranstion;
        SceneLoader.OnSceneLoadComplete -= SceneTranstion2;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SceneTranstion()
    {
        sceneTransitionPanel.gameObject.SetActive(true);
        sceneTransitionPanel.color = new Color(1f, 1f, 1f, 0f);


        Color endColor = new Color(1f, 1f, 1f, 1f);

        sceneTransitionPanel.DOColor(endColor, SceneLoader.loadDelay).SetEase(Ease.InSine);
            //.OnComplete( () => { instance.SceneTranstion2(); } );
    }

    private void SceneTranstion2()
    {
        sceneTransitionPanel.gameObject.SetActive(true);
        sceneTransitionPanel.color = new Color(1f, 1f, 1f, 1f);


        Color endColor = new Color(1f, 1f, 1f, 0f);

        sceneTransitionPanel.DOColor(endColor, SceneLoader.loadDelay).SetEase(Ease.InSine)
            .OnComplete(() => { sceneTransitionPanel.gameObject.SetActive(false); });
    }
}
