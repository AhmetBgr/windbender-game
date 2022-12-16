using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveTo : Command
{
    public ObjectMoveController obj;
    public Vector3 from;
    public Vector3 to;
    public Vector3 dir;
    public Vector3 previousDir;

    public int indexInWind; // -1 is outside of wind
    public int destinationTile; // 0 is empty, 1 is moveable object, 2 is obstacle

    public bool intentToMove = false;
    public bool isMovementChecked = false;
    public bool isMomentumTransferred = false;

    public string tag;
    public ObjectMoveController.State state;
    public bool hasSpeed;


    public IDictionary<Vector3, MoveTo> neighbors = new Dictionary<Vector3, MoveTo>();


    public MoveTo(ObjectMoveController obj, Vector3 from, Vector3 to, Vector3 previousDir, int indexInWind, string tag)
    {
        this.obj = obj;
        this.from = from;
        this.to = to;
        //this.destinationTile = destinationTile;
        this.indexInWind = indexInWind;
        this.tag = tag;
        this.dir = to - from;
        this.previousDir = previousDir;
    }


    public override void Execute()
    {
        base.Execute();
    }

    public override void Undo()
    {
        obj.transform.position = from;
        obj.SetState(state);
        obj.hasSpeed = hasSpeed;
        obj.dir = previousDir;
        if(obj.tween != null)
        {
            obj.tween.Kill();
        }
    }

    public void Pass(bool stopAftermoving = true)
    {
        stopAftermoving = isMomentumTransferred;
    }

    public void Move(bool stopAftermoving = true)
    {
        stopAftermoving = isMomentumTransferred;
        obj.Move(dir, stopAftermoving);
        Debug.Log(obj.name + " is moving");
        isMovementChecked = true;

        if(obj.name == "Barrel")
        {
            Debug.Log("stop after moving : " + stopAftermoving);
        }

    }

    public void ChainMove()
    {
        List<MoveTo> chainNeighbors = new List<MoveTo>();

        // Looks for the neighbors who wants to move to this object's position
        // and extends chain move for that neighbor if priority is higher.
        foreach (KeyValuePair<Vector3, MoveTo> kvp in neighbors)
        {
            if (kvp.Value != null && kvp.Value.intentToMove && !kvp.Value.isMovementChecked)
            {
                if (kvp.Value.to == from)
                {
                    chainNeighbors.Add(kvp.Value);
                }
            }
            /**if(kvp.Value.to == from && kvp.Value.dir == dir)
            {
                kvp.Value.ChainMove();
            }
            else if (kvp.Value.to == from && kvp.Value.dir != dir)
            {
                kvp.Value.ChainFailedMove();
            }*/        
        }
        if(chainNeighbors.Count > 0)
        {
            // Determine which object will move since at least one object wants to move same position
            chainNeighbors = GameManager.instance.GetMoveWithHighestPriority(chainNeighbors, dir);
            for (int i = 0; i < chainNeighbors.Count; i++)
            {
                if (i == 0)
                {
                    chainNeighbors[i].ChainMove();
                }
                else
                {
                    chainNeighbors[i].ChainFailedMove();
                }
            }
        }

        bool stopAftermoving = isMomentumTransferred;
        Move(stopAftermoving);
        isMovementChecked = true;
    }

    public void FailedMove()
    {
        obj.FailedMove();
        isMovementChecked = true;
        intentToMove = false;
    }

    public void ChainFailedMove(float moveAmount = 0.5f)
    {
        isMovementChecked = true;

        // Looks for the neighrbors who wants to move to this object's position
        // and extends chain failed move for that neighbor.
        foreach (KeyValuePair<Vector3, MoveTo> kvp in neighbors)
        {
            if (kvp.Value == null) continue;

            if (!kvp.Value.intentToMove && !kvp.Value.isMovementChecked) continue;

            if (kvp.Value.to == from && kvp.Value.dir != - dir)
            {
                kvp.Value.ChainFailedMove(moveAmount);
            }
        }
        FailedMove();
    }

    public void ChainMomentumTransfer(List<MoveTo> emptyDestintionTileMoves)
    {
        MoveTo destinationObjA;
        if (neighbors.TryGetValue(dir, out destinationObjA)) // Checks if there is something in the way for current movement. if so add that object to the destinationObjA
        {
            if (destinationObjA.intentToMove)
            {
                if(-dir == destinationObjA.dir) // Checks if destination object wants to move towards current object's tile loc
                {
                    //  Extends chain failed move to the neighbor object
                    destinationObjA.destinationTile = 2;
                    destinationObjA.ChainFailedMove();
                    isMomentumTransferred = true;
                }
                return;
            }
            

            if ( (destinationObjA.obj.curState == ObjectMoveController.State.standing) |
                 (destinationObjA.obj.curState == ObjectMoveController.State.layingHorizantal && dir.y != 0) |
                 (destinationObjA.obj.curState == ObjectMoveController.State.layingVertical && dir.x != 0))
            {
                destinationObjA.dir = dir;
                destinationObjA.to = destinationObjA.from + dir;
                destinationObjA.intentToMove = true;

                MoveTo destinationObjB;
                if (destinationObjA.neighbors.TryGetValue(dir, out destinationObjB)) // Checks if there is something in the way for objA
                {
                    if (destinationObjB == null) // Checks if obstacle in the way for objA
                    {
                        // Extends chain failed move to the neighbor object
                        destinationObjA.destinationTile = 2;
                        destinationObjA.ChainFailedMove();
                    }
                    else // Checks if moveable object in the way for objA
                    {
                        // Transfers this movements momentum to bumbped object
                        destinationObjA.destinationTile = 1;
                        destinationObjA.ChainMomentumTransfer(emptyDestintionTileMoves);
                    }
                }
                else // destination tile is empty
                {
                    // objA movement added to emptyDestinationTileMoves from GameManager which will starts to movement.
                    destinationObjA.destinationTile = 0;
                    emptyDestintionTileMoves.Add(destinationObjA);
                    destinationObjA.isMomentumTransferred = false;
                }
                isMomentumTransferred = true;
            }
            else
            {
                
                ChainFailedMove();
                isMomentumTransferred = true;
            }
        }

    }

    /*public void _ChainMomentumTransfer(List<MoveTo> emptyDestintionTileMoves)
    {
        MoveTo destinationObjA;
        if (neighbors.TryGetValue(dir, out destinationObjA))
        {

            if (destinationObjA.intentToMove)
            {

                if (-dir == destinationObjA.dir)
                {
                    // obstacle in the way
                    destinationObjA.destinationTile = 2;
                    destinationObjA.ChainFailedMove();
                    isMomentumTransferred = true;
                }

                return;
            }


            if ((destinationObjA.obj.curState == ObjectMoveController.State.standing) |
                 (destinationObjA.obj.curState == ObjectMoveController.State.layingHorizantal && dir.y != 0) |
                 (destinationObjA.obj.curState == ObjectMoveController.State.layingVertical && dir.x != 0))
            {
                Debug.Log("destination obj name: " + destinationObjA.obj.name);
                destinationObjA.dir = dir;
                destinationObjA.to = destinationObjA.from + dir;
                //destinationObjA.intentToMove = true;

                destinationObjA.obj.hasSpeed = true;
                destinationObjA.obj.dir = dir;

                ChainFailedMove();
                isMomentumTransferred = true;
            }
            else
            {
                ChainFailedMove();
                isMomentumTransferred = true;
            }
        }
        
    }*/




}