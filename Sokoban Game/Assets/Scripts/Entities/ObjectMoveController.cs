using System.Transactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectMoveController : MonoBehaviour
{
    public enum State {
        none,
        standing,
        layingVertical,
        layingHorizantal
    }

    private bool _hasSpeed;
    public bool hasSpeed
    {
        get { return _hasSpeed; }
        set
        {
            _hasSpeed = value;
            if(OnSpeedChange != null)
            {
                OnSpeedChange(value);
            }
        }
    }

    public Vector3 dir;
    public MoveTo movementReserve;
    public State startingState;
    public State curState;
    public Tween tween;

    public delegate void OnSpeedChangeDelegate(bool hasSpeed);
    public event OnSpeedChangeDelegate OnSpeedChange;

    protected virtual void OnEnable(){
        GameManager.instance.OnTurnStart1 += ReserveMovement;
        GameManager.instance.OnTurnStart2 += FindNeighbors;
        GameManager.instance.OnSpeedChanged += UpdateAnimSpeed;
    }

    protected virtual void OnDisable()
    {
        GameManager.instance.OnTurnStart1 -= ReserveMovement;
        GameManager.instance.OnTurnStart2 -= FindNeighbors;
        GameManager.instance.OnSpeedChanged -= UpdateAnimSpeed;
    }

    public virtual void ReserveMovement(List<Vector3> route)
    {
        
        movementReserve = null;
        int index = -1; // index in wind route
        bool intentToMove = true;
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 previousDir = dir;
        if (route.Contains(pos)) // Check if the object is in the wind route
        {
            // Determines the movement direction

            index = route.FindIndex(i => i == pos); // finds index in wind

            // Calculates the direction depand on the index in the route
            if (GameManager.instance.isLooping && (pos == route[0]))
            {
                dir = route[0] - route[route.Count - 2];
            }
            else
            {
                if (index == 0)
                {
                    intentToMove = false;
                }
                else
                {
                    dir = route[index] - route[index - 1];
                }
            }
        }
        else
        {
            hasSpeed = false;
            intentToMove = false;
        }

        //if (!reserveMov) return;

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
        //{
        GameManager.instance.oldCommands.Add(movementReserve);
        //}
    }

    public virtual void FindNeighbors(List<Vector3> route)
    {
        if (movementReserve == null)
        {
            return;
        }

        GameManager gameManager = GameManager.instance;

        Vector3 origin = transform.position;
        int destinationTile = -1;

        List<Vector3> neighborVectors = new List<Vector3> { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
        foreach (Vector3 dir in neighborVectors)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin + dir, Vector2.zero, distance: 1f, LayerMask.GetMask("Wall", "Obstacle", "Pushable"));
            MoveTo neighbor = null;
            if (hit)
            {
                GameObject obj = hit.transform.gameObject;
                
                if (movementReserve.intentToMove && this.dir == dir)
                {
                    //Debug.Log("destination tile layer: " + obj.layer);
                    if (obj.layer == 7)
                    {
                        destinationTile = 1;
                        neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    }
                    else
                    {
                        destinationTile = 2;
                    }
                }
                else
                {
                    if (obj.layer == 7)
                    {
                        neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    }
                }
                movementReserve.neighbors.Add(dir, neighbor);
            }
        }
        movementReserve.destinationTile = destinationTile;

        MoveTo destinationObj;

        if (!movementReserve.intentToMove) return;

        if (movementReserve.neighbors.TryGetValue(movementReserve.dir, out destinationObj))
        {
            if (destinationObj == null) // wall at destination
            {
                gameManager.obstacleAtDestinationMoves.Add(movementReserve);
            }
            else
            {
                if (!destinationObj.intentToMove | (destinationObj.intentToMove && -destinationObj.dir == movementReserve.dir))
                {
                    gameManager.momentumTransferMoves.Add(movementReserve);
                    //Debug.Log("MOMENTUM TRANSFER");
                }
            }
        }
        else
        {
            gameManager.emptyDestinationMoves.Add(movementReserve);
        }
        //Debug.Log("destination tile = " + destinationTile);
    }

    public virtual void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false)
    {
        
        Vector3 startPos = transform.position;
        tween = transform.DOMove(startPos + dir, GameManager.instance.realTurnDur).SetEase(Ease.InOutQuad); //.SetEase(Ease.Linear)
        hasSpeed = true;
        tween.timeScale = 1;
    }

    public virtual void FailedMove()
    {
        
        tween = transform.DOPunchPosition(dir / 5, GameManager.instance.realTurnDur / 1.1f, vibrato: 0).SetEase(Ease.OutCubic);
        hasSpeed = false;
        tween.timeScale = 1;
    }

    public virtual void PlayMoveAnim()
    {

    }

    public virtual void PlayTurnAnim()
    {

    }

    public virtual void PlayFailedMoveAnim()
    {

    }

    public virtual void ChainPush(Vector3 dir)
    {
        MoveTo destinationObj;
        if (movementReserve.neighbors.TryGetValue(dir, out destinationObj))
        {
            if (destinationObj == null) return;
            if (destinationObj.intentToMove && destinationObj.dir != -dir) return;

            destinationObj.pushed = true;
            destinationObj.obj.hasSpeed = true;
            destinationObj.obj.ChainPush(dir);
        }
    }

    public virtual void SetState(State state)
    {
        this.curState = state;
    }

    public virtual void UpdateAnimSpeed(float gameSpeed)
    {
        float previousGameSpeed = GameManager.instance.GetPreviousGameSpeed();

        if (tween == null) return;
        tween.timeScale = gameSpeed / previousGameSpeed;
    }
}
