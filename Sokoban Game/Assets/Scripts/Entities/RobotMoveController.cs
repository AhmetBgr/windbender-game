using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RobotMoveController : ObjectMoveController
{
    public Direction moveDir;

    //private Vector3 dirBeforeWind;
    private bool turnAfterMoving = false;
    //private bool pushedByWind = false;
    public bool onOff = true; // true = on, false = off

    public void Start()
    {
        dir = Utility.DirToVectorDir(moveDir);
        hasSpeed = true;
    }

    public override void ReserveMovement(List<Vector3> route)
    {
        movementReserve = null;
        
        //dirBeforeWind = dir;
        int index = -1; // index in wind route
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 previousDir = dir;
        if (route.Contains(pos)) // Check if the object is in the wind route // && !pushedByWind
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
                if (index > 0)
                {
                    dir = route[index] - route[index - 1];
                }
            }
            OnOff(false);
        }
        else
        {
            dir = Utility.DirToVectorDir(moveDir);
            OnOff(true);
        }


        Vector3 from = transform.position;
        Vector3 to = from + dir;

        // Reserves movement
        movementReserve = new MoveTo(this, from, to, previousDir, index, tag);
        movementReserve.executionTime = Time.time;
        movementReserve.intentToMove = true;
        movementReserve.state = curState;
        movementReserve.hasSpeed = hasSpeed;

        //if (GameManager.instance.isFirstTurn)
        //{
        GameManager.instance.oldCommands.Add(movementReserve);
        //}
    }

    public override void FindNeighbors(List<Vector3> route)
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
                //Debug.LogWarning("WALL");
            }
            else
            {
                //gameManager.momentumTransferMoves.Add(movementReserve);

                /*if (!destinationObj.intentToMove || ( destinationObj.dir != -movementReserve.dir | destinationObj.dir != dir ) )
                {
                    gameManager.momentumTransferMoves.Add(movementReserve);
                    
                }
                else
                {
                    gameManager.obstacleAtDestinationMoves.Add(movementReserve);
                }*/

                if (!destinationObj.intentToMove | (destinationObj.intentToMove && -destinationObj.dir == movementReserve.dir))
                {
                    gameManager.momentumTransferMoves.Add(movementReserve);
                }

            }
        }
        else
        {
            gameManager.emptyDestinationMoves.Add(movementReserve);
            //Debug.LogWarning("EMPTY MOVE");
        }

        ChainPush(dir);
    }

    public override void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false)
    {
        base.Move(dir, stopAftermoving, pushed);
        if (turnAfterMoving)
        {
            Turn();
        }
    }

    public override void FailedMove()
    {
        base.FailedMove();
        Turn();
    }


    private void Turn()
    {
        TurnDirection turn = new TurnDirection(this, -dir);
        turn.Execute();
        this.turnAfterMoving = false;
        GameManager.instance.oldCommands.Add(turn);
        /*if (GameManager.instance.isFirstTurn)
        {
            GameManager.instance.oldCommands.Add(turn);
        }*/

    }

    public override void ChainPush(Vector3 dir)
    {
        if (movementReserve.pushed)
            OnOff(false);

        MoveTo destinationObj;
        if (movementReserve.neighbors.TryGetValue(dir, out destinationObj))
        {
            if (destinationObj == null) return;
            if (destinationObj.intentToMove && (destinationObj.dir == -dir | destinationObj.dir == dir)) {
                Debug.LogWarning("should not push");
                return;
            }

            destinationObj.pushed = true;
            destinationObj.obj.hasSpeed = true;
            destinationObj.intentToMove = false;
            destinationObj.obj.ChainPush(dir);

            if (movementReserve.indexInWind < 0 && onOff)
            {
                turnAfterMoving = true;
            }
        }
        
    }

    private void OnOff(bool newState)
    {
        OnOffRobot onOffRobot = new OnOffRobot(this, onOff, newState);
        onOffRobot.Execute();
    }
}
