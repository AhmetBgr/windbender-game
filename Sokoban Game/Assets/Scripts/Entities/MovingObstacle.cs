using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObstacle : ObjectMoveController
{
    public Vector3 moveAxis;

    private Vector3 prevPos = Vector3.left * 100000;

    public WindCutter centerWC;
    public WindCutter rightWC;
    public WindCutter leftWC;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected override void OnEnable() {
        //base.OnEnable();
        Rotator.OnRotates += TryReserveMovement;
        GameManager.instance.OnTurnStart2 += FindNeighbors;

    }

    protected override void OnDisable() {
        //base.OnDisable();
        Rotator.OnRotates -= TryReserveMovement;
        GameManager.instance.OnTurnStart2 += FindNeighbors;

    }

    private void TryReserveMovement(float rotationDir) {
        movementReserve = null;
        dir = (moveAxis * rotationDir).normalized;

        if ((transform.position + dir - transform.parent.position).magnitude > 1) return;

        int index = -1; // index in wind route
        bool intentToMove = true;
        bool pushed = false;
        transform.position = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);
        Vector3 previousDir = dir;
        Vector3 from = transform.position;
        Vector3 to = from + dir;

        // Reserves movement
        movementReserve = new MoveTo(this, from, to, previousDir, curState, index, tag);
        movementReserve.executionTime = Time.time;
        movementReserve.turnID = GameManager.instance.turnID;
        movementReserve.intentToMove = intentToMove;
        movementReserve.state = curState;
        movementReserve.hasSpeed = hasSpeed;
        movementReserve.pushed = pushed;

        GameManager.instance.oldCommands.Add(movementReserve);

    }

    public override void FindNeighbors(List<Vector3> route) {
        if (movementReserve == null) return;

        GameManager gameManager = GameManager.instance;

        Vector3 origin = transform.position + dir;
        int destinationTile = -1;

        List<Vector3> neighborVectors = new List<Vector3> { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
        if (neighborVectors.Contains(-dir))
            neighborVectors.Remove(-dir);

        foreach (Vector3 dir in neighborVectors) {
            RaycastHit2D hit = Physics2D.Raycast(origin + dir, Vector2.zero, distance: 1f, LayerMask.GetMask("Wall", "Obstacle", "Pushable"));
            MoveTo neighbor = null;
            if (hit) {
                GameObject obj = hit.transform.gameObject;

                if (movementReserve.intentToMove && this.dir == dir) {
                    //Debug.Log("destination tile layer: " + obj.layer);
                    if (obj.layer == 7) {
                        destinationTile = 1;
                        neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    }
                    else {
                        destinationTile = 2;
                    }
                }
                else {
                    if (obj.layer == 7) {
                        neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    }
                }
                movementReserve.neighbors.Add(dir, neighbor);
            }
        }
        movementReserve.destinationTile = destinationTile;

        MoveTo destinationObj;

        if (!movementReserve.intentToMove) return;

        // Determines destination tile type and adds movement reserve to the mov. res. list

        if (movementReserve.neighbors.TryGetValue(movementReserve.dir, out destinationObj)) {

            if (destinationObj == null) {
                // wall at destination
                gameManager.obstacleAtDestinationMoves.Add(movementReserve);
                //Debug.LogWarning("here");
                //pushedByInfos.Remove(movementReserve.dir);
            }
            else {
                // A moveable object at destination tile
                if (!destinationObj.intentToMove | (destinationObj.intentToMove && -destinationObj.dir == movementReserve.dir)) {
                    gameManager.momentumTransferMoves.Add(movementReserve);
                }
            }
        }
        else {
            // Destination tile is empty
            gameManager.emptyDestinationMoves.Add(movementReserve);
        }
        movementReserve = null;
    }

    public override void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false) {
        Vector3 startPos = transform.position;
        
        Vector3 dest = startPos + dir;



        /*bool isMoved = !(dest == prevPos);
        centerWC.isMoved = isMoved;
        rightWC.isMoved = isMoved;
        leftWC.isMoved = isMoved;
        */
        prevPos = startPos;
        float dur = GameManager.instance.defTurnDur;
        float delay = GameManager.instance.defTurnDur - dur;

        tween = transform.DOMove(dest, dur)
            //.SetDelay(delay)
            .SetEase(Ease.InQuad); // Ease.Linear
        hasSpeed = true;

        centerWC.OnMoved(centerWC.transform.position + dir);
        leftWC.OnMoved(leftWC.transform.position + dir);
        rightWC.OnMoved(rightWC.transform.position + dir);
    }
}
