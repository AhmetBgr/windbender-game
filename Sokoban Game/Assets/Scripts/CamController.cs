using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CamController : MonoBehaviour
{
    //public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        Camera cam = Camera.main;
        float zoomAmount = cam.orthographicSize / 10;
        cam.orthographicSize -= zoomAmount;
        cam.DOOrthoSize(cam.orthographicSize + zoomAmount, 2.5f).SetEase(Ease.OutSine);
    }

    private void OnEnable()
    {
       
    }

    private void OnDisable()
    {
        
    }

}
