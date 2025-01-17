using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObstacleParent : MonoBehaviour
{
    public MovingObstacle center;
    public MovingObstacle rightOrBottom;
    public MovingObstacle leftOrTop;

    public TargetTag targetTag;

    public bool ismoving = false;
    public bool isFailedMoving = false;
    public bool shouldFail = false;

    private MovingObstacleParent preview = null;


    private void Start() {

    }

    public void TryMove(MovingObstacle obstacle) {
        if (obstacle != center) return;

        if (shouldFail) {
            isFailedMoving = true;
            FailedMove(obstacle);
        }
        else{
            ismoving = true;
            Move(obstacle);
        }
    }

    public void Move(MovingObstacle obstacle) {
        //if (isFailedMoving) return;
        //if (ismoving) return;


        ismoving = true;

        Vector3 startPos = transform.position;

        Vector3 dest = startPos + obstacle.dir;


            
        obstacle.prevPos = startPos;
        float dur = GameManager.instance.defTurnDur;
        float delay = GameManager.instance.defTurnDur - dur;




        obstacle.hasSpeed = true;

        obstacle.centerWC.OnMoved(obstacle.centerWC.transform.position + obstacle.dir);
        obstacle.leftWC.OnMoved(obstacle.leftWC.transform.position + obstacle.dir);
        obstacle.rightWC.OnMoved(obstacle.rightWC.transform.position + obstacle.dir);


        if (GameManager.instance.curGame.isSimulation) {
            transform.position = dest;
        }
        else {
            obstacle.tween = transform.DOMove(dest, dur)
                .SetEase(Ease.InOutQuad); // Ease.Linear
        }

        obstacle.movementReserve = null;
    }

    public void FailedMove(MovingObstacle obstacle) {
        //if (ismoving) return;

        //if (isFailedMoving) return;

        isFailedMoving = true;

        if(!GameManager.instance.curGame.isSimulation)
            obstacle.tween = transform.DOPunchPosition(obstacle.dir / 10, GameManager.instance.defTurnDur / 1.1f, vibrato: 0).SetEase(Ease.OutCubic);
        
        
        obstacle.hasSpeed = false;
    }

    public void SetPos(MovingObstacle obstacle, Vector3 pos) {
        if (obstacle != center) return;

        transform.position = pos;
    }
}
