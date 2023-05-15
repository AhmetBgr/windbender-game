using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VelocityIndicator : MonoBehaviour
{
    public ObjectMoveController moveController;
    public Vector3 dir;
    private SpriteRenderer spriteRenderer;
    private Tween moveTween;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        ShowWithDelay(GameManager.instance.state, GameManager.instance.state);
        HideWithDelay(GameManager.instance.state, GameManager.instance.state);
    }

    private void OnEnable()
    {
        GameManager.instance.OnStateChange += ShowWithDelay;
        GameManager.instance.OnStateChange += HideWithDelay;
        GameManager.instance.OnUndo += Show;
        GameManager.instance.OnUndo += Hide;
    }
    private void OnDisable()
    {
        GameManager.instance.OnStateChange -= ShowWithDelay;
        GameManager.instance.OnStateChange -= HideWithDelay;
        GameManager.instance.OnUndo -= Show;
        GameManager.instance.OnUndo -= Hide;
    }

    private void ShowWithDelay(GameState from, GameState to) //
    {
        if (to == GameState.Running) return; //

        Invoke("Show",  0.2f); //GameManager.instance.turnDur +
    }

    private void Show()
    {
        if (moveController == null) return;

        if (!moveController.hasSpeed) return;

        if (moveTween != null)
        {
            moveTween.Kill();
        }

        spriteRenderer.enabled = true;

        dir = moveController.dir;
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        moveTween = transform.DOLocalMove(dir * 0.4f, 0.8f).SetLoops(-1);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, dir);
        transform.rotation = rotation;
    }

    private void HideWithDelay(GameState from, GameState to) // 
    {
        if (to != GameState.Running) return; // 

        Invoke("Hide", GameManager.instance.realTurnDur - 0.04f);
    }

    private void Hide()
    {
        spriteRenderer.enabled = false;
        if (moveTween != null)
        {
            moveTween.Kill();
        }
    }

}
