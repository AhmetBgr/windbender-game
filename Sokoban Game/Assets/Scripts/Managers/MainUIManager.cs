using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainUIManager : MonoBehaviour
{
    [System.Serializable]
    public struct TransitionProperty {
        public Color startColor;
        public Color endColor;

        public Ease ease;

        public float durationFH;    // duration first half
        public float durationSH;    // duration second half
    }

    public SettingsManager settings;

    public Image sceneTransitionPanel;

    public TransitionProperty transitionProperty1;
    public TransitionProperty transitionProperty2;

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
        //instance.SceneTranstionSH(transitionProperty1);
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        //GameManager.instance.OnLevelComplete += SceneTranstion;
        //SceneLoader.OnSceneLoad += SceneTranstionFH;
        //SceneLoader.OnSceneLoadComplete += SceneTranstionSH;

    }
    
    private void OnDisable()
    {
        //GameManager.instance.OnLevelComplete -= SceneTranstion;
        //SceneLoader.OnSceneLoad -= SceneTranstionFH;
        //SceneLoader.OnSceneLoadComplete -= SceneTranstionSH;
        
        
    }

    /*private void SceneTranstionFH()
    {
        _SceneTranstionFH(transitionProperty1);
    }

    private void SceneTranstionSH()
    {
        _SceneTranstionSH(transitionProperty1);
    }
    */

    public void SceneTranstionFH(TransitionProperty tp)
    {
        sceneTransitionPanel.gameObject.SetActive(true);
        sceneTransitionPanel.color = tp.startColor;

        sceneTransitionPanel.DOColor(tp.endColor, tp.durationFH).SetEase(tp.ease);
    }

    public void SceneTranstionSH(TransitionProperty tp)
    {
        sceneTransitionPanel.gameObject.SetActive(true);
        sceneTransitionPanel.color = tp.endColor;

        sceneTransitionPanel.DOColor(tp.startColor, tp.durationSH).SetEase(tp.ease)
            .OnComplete(() => { sceneTransitionPanel.gameObject.SetActive(false); });
    }
}
