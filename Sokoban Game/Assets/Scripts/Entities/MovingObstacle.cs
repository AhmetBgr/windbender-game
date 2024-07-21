using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingObstacle : ObjectMoveController
{
    public Vector3 moveAxis;

    [HideInInspector] public Vector3 prevPos = Vector3.left * 100000;

    public WindCutter centerWC;
    public WindCutter rightWC;
    public WindCutter leftWC;

    private MovingObstacleParent parent;
    private Collider2D col;
    private TargetTag targetTag;
    public int lenght = 3;

    // Start is called before the first frame update
    void Start()
    {
        parent = GetComponentInParent<MovingObstacleParent>();
        col = GetComponent<Collider2D>();
        targetTag = parent.targetTag;
    }

    protected override void OnEnable() {
        //base.OnEnable();
        Rotator.OnRotates += TryReserveMovement;
        GameManager.instance.OnTurnStart2 += FindNeighbors;

    }

    protected override void OnDisable() {
        //base.OnDisable();
        Rotator.OnRotates -= TryReserveMovement;
        GameManager.instance.OnTurnStart2 -= FindNeighbors;

    }
    private void TryReserveMovement(float rotationDir, TargetTag tag) {
        if (targetTag != tag) return;

        pushedByInfos.Clear();
        movementReserve = null;
        dir = (moveAxis * rotationDir).normalized;

        //Debug.LogWarning("  HERE 5");


        if ((transform.parent.localPosition + dir).magnitude > 1) return;

        
        Debug.LogWarning("  HERE 4: " + transform.parent.name);

        parent.ismoving = false;
        parent.isFailedMoving = false;
        parent.shouldFail = false;
        //col.enabled = transform.position == transform.parent.parent.position;


        int index = -1; // index in wind route
        bool intentToMove = true;
        bool pushed = false;
        transform.position = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);
        Vector3 previousDir = dir;
        this.dir = dir;
        Vector3 from = transform.position;
        Vector3 to = from + dir;

        // Reserves movement
        movementReserve = new MoveTo(this, from, to, previousDir, curState, index, this.tag);
        movementReserve.executionTime = Time.time;
        movementReserve.turnID = GameManager.instance.turnID;
        movementReserve.intentToMove = intentToMove;
        movementReserve.state = curState;
        movementReserve.hasSpeed = hasSpeed;
        movementReserve.pushed = pushed;

        GameManager.instance.oldCommands.Add(movementReserve);
    }

    /*public override void FindNeighbors(List<Vector3> route) {
        //Debug.LogWarning("  HERE 3");


        if (movementReserve == null) return;

        //Debug.LogWarning("  HERE 2");


        GameManager gameManager = GameManager.instance;

        Vector3 origin = transform.position + dir;

        List<Vector3> neighborVectors = new List<Vector3> { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
        if (neighborVectors.Contains(-dir))
            neighborVectors.Remove(-dir);


        if((transform.position - dir - transform.parent.parent.position).magnitude > 1f) {
            //neighborVectors.Remove(dir);
        }

        origin = transform.position;
        foreach (Vector3 dir in neighborVectors) {
            TryToAddToNeighbors(dir, origin);
        }

        foreach (var item in movementReserve.neighbors) {
            Debug.Log("dir: " + item.Key + ", count: " + item.Value.Count);
        }

        if (!movementReserve.intentToMove) return;

        //Debug.LogWarning("  HERE 1");

        // Determines destination tile type and adds movement reserve to the mov. res. list
        List<MoveTo> value;
        if (movementReserve.neighbors.TryGetValue(movementReserve.dir, out value)) {
            //Debug.LogWarning("  HERE 0");
            foreach (var destinationObj in value) {
                //Debug.LogWarning("  HERE -1");
                if (destinationObj == null) {
                    // wall at destination
                    gameManager.obstacleAtDestinationMoves.Add(movementReserve);
                    Debug.LogWarning("added to obstacle at dest:" + transform.parent.name);
                    //pushedByInfos.Remove(movementReserve.dir);
                    break;
                }
                else {
                    // A moveable object at destination tile
                    //gameManager.momentumTransferMoves.Add(movementReserve);
                    if (!destinationObj.intentToMove | (destinationObj.intentToMove && destinationObj.dir != movementReserve.dir)) {
                        Debug.LogWarning("added to momentum transfer:" + transform.parent.name);

                        gameManager.momentumTransferMoves.Add(movementReserve);
                    }
                }

            }

        }
        else {
            // Destination tile is empty
            Debug.LogWarning("added to empty dest2:" + transform.parent.name);

            gameManager.emptyDestinationMoves.Add(movementReserve);
        }
    }*/
    
    /*private void TryToAddToNeighbors(Vector3 dir, Vector3 origin) {
        RaycastHit2D hit = Physics2D.Raycast(origin + dir, Vector2.zero, distance: 1f, LayerMask.GetMask("Wall", "Obstacle", "Pushable"));
        MoveTo neighbor = null;

        if (hit) {
            GameObject obj = hit.transform.gameObject;
            Debug.Log("hit loc: " + hit.point);
            if (movementReserve.intentToMove && movementReserve.dir == dir) {
                Debug.Log("destination tile layer: " + obj.layer);
                if (obj.layer == 7) {
                    neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;

                    Debug.Log("neighbor  name1 : " + hit.transform.parent.name);

                }
                else {
                    Debug.Log("couldnt add because neighrbor not moveable");
                }
            }
            else {
                if (obj.layer == 7) {
                    neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    //neighborsInDir.Add(neighbor);
                    Debug.Log("neighbor  name2 : " + neighbor);

                }
                else {
                    Debug.Log("couldnt add because neighrbor not moveable");
                }
            }

            if (movementReserve.neighbors.ContainsKey(dir)) {
                movementReserve.neighbors[dir].Add(neighbor);
            }
            else {
                List<MoveTo> neighborsInDir = new();
                Debug.Log("neighbor  name : " + neighbor);

                neighborsInDir.Add(neighbor);
                movementReserve.neighbors.Add(dir, neighborsInDir);
            }

            Debug.Log("neighbor  added : " + hit.transform.name + ", dir: " + dir);

        }
    }*/

    public override void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false) {
        
        parent.TryMove(this);
        movementReserve = null;
    }

    public override void FailedMove() {
        parent.shouldFail = true;
        parent.TryMove(this);
        movementReserve = null;

    }

    public override void SetPos(Vector3 pos) {
        parent.SetPos(this, pos);
    }

    public override void Hit(List<MoveTo> emptyDestintionTileMoves) {

        PushInfo pushInfo = new PushInfo();
        pushInfo.pushedBy = null;
        pushInfo.pushOrigin = gameObject.GetInstanceID();
        pushInfo.indexInChainPush = -1;
        pushInfo.pushDir = dir;

        pushInfoThis = pushInfo;
        pushedByInfos.Add(pushInfo.pushDir, pushInfo);

        base.Hit(emptyDestintionTileMoves);
    }
}
