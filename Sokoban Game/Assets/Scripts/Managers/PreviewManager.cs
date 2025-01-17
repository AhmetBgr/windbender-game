using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewManager : MonoBehaviour{

    public List<PreviewController> previewControllers = new List<PreviewController>();

    public static PreviewManager instance = null;

    void Awake() {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

    }


    private void OnEnable() {
        GameManager.instance.OnSimStarted += SetupSim;
        GameManager.instance.OnSimComleted += CompleteSim;
        GameManager.instance.OnSimEnded += HandleSimEnding;
        

    }

    private void OnDisable() {
        GameManager.instance.OnSimStarted -= SetupSim;
        GameManager.instance.OnSimComleted -= CompleteSim;
        GameManager.instance.OnSimEnded -= HandleSimEnding;

    }


    private  void SetupSim() {
        // todo: generate clone, disable original obj
        // swap pos with clone and color 

        foreach (var item in previewControllers) {
            item.SetupSim();

            item.gameObject.SetActive(false);
        }

    }

    public void HandleSimEnding() {
        foreach (var item in previewControllers) {
            item.HandleSimEnding();
        }
    }

    private void CompleteSim(bool value) {
        foreach (var item in previewControllers) {
            item.gameObject.SetActive(true);

            item.CompleteSim(value);

            //item.clone.gameObject.SetActive(false);

        }
    }
}
