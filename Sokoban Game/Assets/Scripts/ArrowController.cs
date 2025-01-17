using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowController : MonoBehaviour
{
    public LineRenderer lr;
    public Transform head;
    public Transform origin;

    private GameManager gameManager;
    private IEnumerator coroutine;

    private void OnEnable() {
        gameManager = GameManager.instance;
        //gameManager.OnPlay += AnimateArrow;
        gameManager.OnUndo += KillAnimation;
        //gameManager.OnSpeedChanged += UpdateAnimSpeed;
    }

    private void OnDisable() {
        //gameManager.OnPlay -= AnimateArrow;
        gameManager.OnUndo -= KillAnimation;
        //gameManager.OnSpeedChanged -= UpdateAnimSpeed;
    }

    public void SetPositions(List<Vector3> windMoveroute){
        this.gameObject.SetActive(true);
        lr.positionCount = windMoveroute.Count;
        lr.SetPositions(windMoveroute.ToArray());
        head.gameObject.SetActive(true);
        head.position = lr.GetPosition(lr.positionCount - 1);
        origin.gameObject.SetActive(true);
        origin.position = lr.GetPosition(0);
    }

    public void AddPos(Vector3 pos){
        head.gameObject.SetActive(true);
        origin.gameObject.SetActive(true);
        lr.positionCount += 1;
        lr.SetPosition(lr.positionCount -1, pos);
        head.position = pos;
        origin.position = lr.GetPosition(0);
    }

    public void RemoveLastPos(){
        lr.positionCount -= 1;
        head.position = lr.GetPosition(lr.positionCount - 1);
        if(lr.positionCount == 0)   
            head.gameObject.SetActive(false);
    }

    public void RemovePosAt(int index){
        int lrCount = lr.positionCount - 1;
        Vector3[] newPositions = new Vector3[lrCount - index];
 
        for (int i = index; i < lrCount; i++){
            newPositions[i] = lr.GetPosition(i + 1);
        }
        
        if(lrCount - index == 0)
            RemoveLastPos();
        else{
            lr.SetPositions(newPositions);
            head.position = lr.GetPosition(lr.positionCount - 1);;
        }
            
    }

    public void Clear(){
        lr.positionCount = 0;
        head.gameObject.SetActive(false);
        origin.gameObject.SetActive(false);
    }

    private void AnimateArrow(){
        if(gameManager.turnCount == 0 || lr.positionCount <= 1)    return;

        //Get old Position Length
        Vector3[] positions = new Vector3[lr.positionCount];
        //Get old Positions
        lr.GetPositions(positions);

        origin.DOPath(positions, lr.positionCount * gameManager.defTurnDur).SetEase(Ease.Linear); 
        //.SetDelay(gameManager.defTurnDur);

        /*if(coroutine != null){
            //Debug.LogWarning("coroutine stopped");
            StopCoroutine(coroutine);
        }
        coroutine = DisappearAnim();
        StartCoroutine(coroutine);*/
    }

    private IEnumerator DisappearAnim(){
        //int pointsCount = lr.positionCount;

        float startTime = Time.time;
        float time = 0;
        Vector3 startPosition = lr.GetPosition(0); //gameManager.turnCount - gameManager.curWindSource.defWindSP
        Vector3 endPosition = lr.GetPosition(1);
        Vector3 pos = startPosition;
        float dur = gameManager.defTurnDur - 0.08f;
        while (time <= dur) {
            // / (gameManager.defTurnDur - 0.04f);//(Time.time - startTime) / gameManager.defTurnDur;
            //float t =  Time.time - startTime / dur; 
            float t =  time / (dur); 
            pos = Vector3.Lerp (startPosition, endPosition, t) ;
            lr.SetPosition (0, pos);
            origin.position = pos;
            time += (Time.deltaTime * gameManager.gameSpeed);; // 
            
            yield return null ;
        }
        
        // Removes first point from the line renderer
        //List<Vector3> points = new List<Vector3>();
        Vector3[] newPositions = new Vector3[lr.positionCount];
        for (int i = 0; i < lr.positionCount - 1; i++){
            newPositions[i] = lr.GetPosition(i + 1);            
        }

        lr.SetPositions(newPositions);
        lr.positionCount--;    

        if(lr.positionCount == 1){
            head.DOScale(Vector3.zero, gameManager.defTurnDur).SetEase(Ease.InBounce);
            origin.DOScale(Vector3.zero, gameManager.defTurnDur).SetEase(Ease.InBounce)
            .OnComplete(() => {
                head.localScale = new Vector3(0.35f, 0.35f, 1f);
                origin.localScale = new Vector3(0.2f, 0.2f, 1f);
                this.gameObject.SetActive(false);
            });
        }
    }

    private void KillAnimation(){
        //StopCoroutine(coroutine);

        SetPositions(gameManager.curGame.windMoveRoute);
    }

    private void UpdateAnimSpeed(float gameSpeed){

    }
}
