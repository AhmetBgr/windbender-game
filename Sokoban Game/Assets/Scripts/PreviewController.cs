using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject previewPrefab;
    public Collider2D[] cols;
    //public Sprite previewSprite;

    [HideInInspector] public PreviewController clone;
    [HideInInspector] public SpriteRenderer preview;
    private Color initPreviewColor;
    private Vector3 initPos;
    private Vector3 previewPos;

    public bool isClone = false;
    private bool isPreviewActive = false;

    /*private void OnEnable() {
        GameManager.instance.OnSimStarted += SetupSim;
        GameManager.instance.OnSimComleted += CompleteSim;
        GameManager.instance.OnSimEnded += HandleSimEnding;

    }

    private void OnDisable() {
        GameManager.instance.OnSimStarted -= SetupSim;
        GameManager.instance.OnSimComleted -= CompleteSim;
        GameManager.instance.OnSimEnded -= HandleSimEnding;

    }*/

    private void Start() {
        if (isClone) return;

        PreviewManager.instance.previewControllers.Add(this);
    }

    public void SetupSim() {
        // todo: generate clone, disable original obj
        // swap pos with clone and color 



        Debug.LogWarning("should setup sim");

        if (previewPrefab == null) return;

        if(clone == null) {
            clone = Instantiate(gameObject).GetComponent<PreviewController>();
            clone.isClone = true;
            clone.transform.SetParent(PreviewManager.instance.transform);
            clone.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder;
        }

        foreach (var item in clone.cols) {
            item.enabled = true;

        }

        if (preview == null) {
            preview = Instantiate(previewPrefab, clone.spriteRenderer.transform.position, Quaternion.identity).GetComponent<SpriteRenderer>();
            initPreviewColor = preview.color;
            preview.sprite = clone.spriteRenderer.sprite;
            preview.gameObject.SetActive(false);
            preview.gameObject.SetActive(true);
            preview.sortingOrder = clone.spriteRenderer.sortingOrder;
            preview.flipX = clone.spriteRenderer.flipX;
            preview.flipY = clone.spriteRenderer.flipY;

            preview.transform.SetParent(PreviewManager.instance.transform);
        }

        initPos = transform.position;

        if (!isPreviewActive) {
            preview.enabled = true;

            preview.transform.position = clone.spriteRenderer.transform.position;

            Color temp = clone.spriteRenderer.color;

            clone.spriteRenderer.color = preview.color;

            preview.color = temp;
            isPreviewActive = true;
        }

        previewPos = clone.spriteRenderer.transform.position;
    }

    public void HandleSimEnding() {
        if (clone == null) return;

        foreach (var item in clone.cols) {
            item.enabled = false;

        }
    }

    public virtual void CompleteSim(bool value) {
        if (!value) return;

        if (!isPreviewActive) return;

        transform.position = initPos;

        preview.transform.position = previewPos;

        clone.spriteRenderer.color = Color.white;
        
        preview.color = initPreviewColor;
        preview.enabled = false;
        isPreviewActive = false;

        Destroy(clone.gameObject);
        clone = null;
    }



}
