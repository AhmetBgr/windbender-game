using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BarrelMovement : ObjectMoveController
{
    public Animator animator;
    private bool stop;
    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.instance.OnUndo += UpdateAnimState;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameManager.instance.OnUndo -= UpdateAnimState;
    }


    private void Start()
    {
        if(startingState == State.standing)
        {
            animator.Play("Barrel_stand");
            curState = State.standing;
        }
        else if (startingState == State.layingHorizantal)
        {
            animator.Play("Barrel_lay_horizantal");
            curState = State.layingHorizantal;
        }
        else
        {
            animator.Play("Barrel_lay_vertical");
            curState = State.layingVertical;
        }

        hasSpeed = dir.magnitude > 0 ? true : false;
    }

    public override void ReserveMovement(List<Vector3> route)
    {
        Vector3 previousDir = dir;
        int index = -1; // index in wind route
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
        bool intentToMove = false;
        bool reserveMove = true;
        if (route.Contains(pos)) // Checks if the object is in the wind route
        {
            intentToMove = true;

            index = route.FindIndex(i => i == pos); // finds index in wind

            // Calculates the direction depand on the index in the route
            if (GameManager.instance.isLooping && (pos == route[0]) )
            {
                dir = route[0] - route[route.Count - 2];
            }
            else
            {
                if (index == 0)
                {
                    /*if (hasSpeed && stop)
                    {
                        hasSpeed = false;
                        intentToMove = false;
                    }
                    */
                    if (hasSpeed)
                    {
                        /*if (stop)
                        {
                            hasSpeed = false;
                            intentToMove = false;

                        }
                        else
                        {
                            intentToMove = true;
                        }*/
                        intentToMove = true;
                    }
                    else
                    {
                        intentToMove = false;
                    }
                }
                else
                {
                    dir = route[index] - route[index - 1];
                }
            }
        }
        else
        {
            //if (hasSpeed && stop)
                //hasSpeed = false;

            intentToMove = hasSpeed;
        }

        if (!reserveMove) return;

        
        Vector3 from = transform.position;
        Vector3 to = from + dir;

        // Reserves movement
        movementReserve = new MoveTo(this, from, to, previousDir, curState, index, tag);
        movementReserve.executionTime = Time.time;
        movementReserve.turnID = GameManager.instance.turnID;
        movementReserve.intentToMove = intentToMove;
        movementReserve.state = curState;
        movementReserve.hasSpeed = hasSpeed;

        //if (GameManager.instance.isFirstTurn)
            //GameManager.instance.oldCommands.Add(movementReserve);
    }

    public override void FindNeighbors(List<Vector3> route)
    {
        base.FindNeighbors(route);
    }

    public override void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false){

        //if(GameManager.instance.isFirstTurn || GameManager.instance.state == GameState.Waiting)
        GameManager.instance.oldCommands.Add(movementReserve);


        this.dir = dir;
        Ease ease = Ease.InOutQuad;
        

        if ( (curState == State.layingHorizantal && (dir == Vector3.up | dir == Vector3.down))
            | (curState == State.layingVertical && (dir == Vector3.right | dir == Vector3.left)) ) // Checks if rolling
        {
            if(!pushed)
                ease = Ease.Linear;
            
            //stopAftermoving = false;
            //Debug.Log("BARREL ROLLING");
        }
        else
        {
            stopAftermoving = true;
        }
        
        Vector3 startPos = transform.position;
        float gameSpeed = GameManager.instance.gameSpeed;
        tween = transform.DOMove(startPos + dir, GameManager.instance.realTurnDur).SetEase(ease)
            .OnComplete(() => {
                if (GameManager.instance.turnCount == 0)
                    SetState(curState);

                if (stopAftermoving)
                    SetState(curState);
            });

        //hasSpeed = true;
        hasSpeed = !stopAftermoving;
        /*if (!GameManager.instance.route.Contains(startPos + dir))
        {
            if (stopAftermoving)
                hasSpeed = false;
        }*/

        tween.timeScale = 1;
        movementReserve = null;
        stop = stopAftermoving;
        
        if (pushed) return;
        PlayMoveAnim();

    }
    public override void FailedMove()
    {
        //if (GameManager.instance.isFirstTurn || GameManager.instance.state == GameState.Waiting)
        GameManager.instance.oldCommands.Add(movementReserve);

        base.FailedMove();
        SetState(curState);
        movementReserve = null;
    }

    public override void PlayMoveAnim()
    {

        float gameSpeed = GameManager.instance.gameSpeed;
        animator.speed = gameSpeed ; //1 / GameManager.instance.turnDur
        if (dir == Vector3.right && curState == State.standing)
        {
            animator.Play("Barrel_fall_right");
            curState = State.layingHorizantal;
            //stop = true;
        }
        else if (dir == Vector3.left && curState == State.standing)
        {
            animator.Play("Barrel_fall_left");
            curState = State.layingHorizantal;
            //stop = true;
        }
        else if (dir == Vector3.down && curState == State.standing)
        {
            animator.Play("Barrel_fall_down");
            curState = State.layingVertical;
            //stop = true;
        }
        else if (dir == Vector3.up && curState == State.standing)
        {
            animator.Play("Barrel_fall_up");
            curState = State.layingVertical;
            //stop = true;
        }
        else if (dir == Vector3.right && curState == State.layingHorizantal)
        {
            animator.Play("Barrel_rise_right");
            curState = State.standing;
            //stop = true;
        }
        else if (dir == Vector3.left && curState == State.layingHorizantal)
        {
            animator.Play("Barrel_rise_left");
            curState = State.standing;
            //stop = true;
        }
        else if (dir == Vector3.up && curState == State.layingVertical)
        {
            animator.Play("Barrel_rise_up");
            curState = State.standing;
            //stop = true;
        }
        else if (dir == Vector3.down && curState == State.layingVertical)
        {
            animator.Play("Barrel_rise_down");
            curState = State.standing;
            //stop = true;
        }
        else
        {
            if (dir == Vector3.up)
                animator.Play("Barrel_roll_up");
            else if (dir == Vector3.right)
                animator.Play("Barrel_roll_right");
            else if (dir == Vector3.down)
                animator.Play("Barrel_roll_down");
            else if (dir == Vector3.left)
                animator.Play("Barrel_roll_left");
            //ease = Ease.Linear;
        }
    }

    public override void SetState(State state)
    {
        //Debug.LogWarning("state changed to: " + state.ToString());
        curState = state;
        UpdateAnimState();

    }

    private void UpdateAnimState()
    {
        if (curState == State.standing)
        {
            animator.Play("Barrel_stand");
        }
        else if (curState == State.layingHorizantal)
        {
            animator.Play("Barrel_lay_horizantal");
        }
        else
        {
            animator.Play("Barrel_lay_vertical");
        }
    }


    public override void UpdateAnimSpeed(float gameSpeed)
    {
        base.UpdateAnimSpeed(gameSpeed);

        animator.speed = gameSpeed;
    }


}
