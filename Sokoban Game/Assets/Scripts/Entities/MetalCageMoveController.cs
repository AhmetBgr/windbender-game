using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MetalCageMoveController : ObjectMoveController
{
    public override void ReserveMovement(List<Vector3> route)
    {
        pushedByInfos.Clear();
        movementReserve = null;
        int index = -1; // index in wind route
        bool intentToMove = true;
        bool pushed = false;
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 previousDir = dir;
        hasSpeed = false;
        intentToMove = false;

        //if (!reserveMov) return;

        Vector3 from = transform.position;
        Vector3 to = from + dir;

        // Reserves movement
        movementReserve = new MoveTo(this, from, to, previousDir, curState, index, tag);
        movementReserve.executionTime = Time.time;
        //movementReserve.turnID = GameManager.instance.turnID;
        movementReserve.intentToMove = intentToMove;
        movementReserve.state = curState;
        movementReserve.hasSpeed = hasSpeed;
        movementReserve.pushed = pushed;

        /*if (OnMovRes != null)
        {
            OnMovRes();
        }*/
        GameManager.instance.oldCommands.Add(movementReserve);
    }


    public override void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false) {
        //if (!pushInfoThis.pushedBy.obj.CompareTag("MovingObstacle")) return;

        base.Move(dir, stopAftermoving, pushed);
    }
    /*public override void TryToPush(PushInfo pushedByInfo) {
        MoveTo moveRes = movementReserve;
        MoveTo destinationObjA;
        //Vector3 dir = moveRes.dir;

        //PushInfo pushInfoThis = pushedByInfos[pushedByInfo.pushDir];

        // Checks if there is something in the way for current movement. if so add that object to the destinationObjA
        if (movementReserve.neighbors.TryGetValue(pushedByInfo.pushDir, out destinationObjA)) {

            if (destinationObjA == null | !pushedByInfo.initiator.obj.CompareTag("MovingObstacle")) {
                // wall at destination
                pushedByInfo.destinationTile = 2;
                //Debug.LogWarning("wall at push dest.");
            }
            else {
                // A moveable object at destination tile
                //Debug.LogWarning("A moveable object at push dest. " + gameObject.name);
                pushedByInfo.destinationTile = 1;

                PushInfo pushInfoOther = new PushInfo();
                pushInfoOther.pushedBy = moveRes;
                pushInfoOther.pushDir = pushedByInfo.pushDir;
                pushInfoOther.indexInChainPush = pushedByInfo.indexInChainPush + 1;
                pushInfoOther.pushOrigin = pushedByInfo.pushOrigin;
                pushInfoOther.isValidated = 2;
                //MoveTo destinationObjB;
                //destinationObjA.neighbors.TryGetValue(dir, out destinationObjB)

                //pushInfo.destinationTile =
                //Debug.LogWarning(gameObject.name + " :" + pushInfoOther);
                destinationObjA.obj.pushedByInfos.Add(pushedByInfo.pushDir, pushInfoOther);
                destinationObjA.obj.TryToPush(pushInfoOther);
            }
        }
        else {
            // destination tile is empty
            pushedByInfo.destinationTile = 0;
        }


    }*/

    public override void ValidatePush(List<MoveTo> emptyDestintionTileMoves) {

        //Debug.LogWarning("push  res count: " + pushedByInfos.Count + " :" + gameObject.name);

        if (pushedByInfos.Count == 0 | (pushedByInfos.Count == 1 && movementReserve.destinationTile == 2)) return;
        // Determines destination tile type and adds movement reserve to the mov. res. list
        Vector3 dirSum = Vector3.zero;
        for (int i = 0; i < pushedByInfos.Count; i++) {
            PushInfo info = pushedByInfos.ElementAt(i).Value;
            if (info == null) {
                pushedByInfos.Remove(pushedByInfos.ElementAt(i).Key);
                continue;
            }

            if (info.destinationTile == 2) {
                info.isValidated = 0;
                //pushedByInfos.Remove(pushedByInfos.ElementAt(i).Key);
                continue;
            }

            dirSum += pushedByInfos.ElementAt(i).Key;
        }
        //Debug.LogWarning("push  res count: " + pushedByInfos.Count + " :" + gameObject.name);
        //Debug.LogWarning("push  res dir sum: " + dirSum + " :" + gameObject.name);
        //Debug.LogWarning("push  res right dir : " + pushedByInfos[Vector3.right].pushDir + " :" + gameObject.name);
        //Debug.LogWarning("push  res left  dir : " + pushedByInfos[Vector3.left].pushDir + " :" + gameObject.name);
        if (dirSum == Vector3.zero) {
            // Failed move
            //Debug.LogWarning("push failed: " + gameObject.name);

            Vector3 failedMoveDir = Vector3.zero;
            int lowestIndex = 200;
            //int previoulowestIndex = lowestIndex;
            bool equalLowestIndexExists = false;
            foreach (var item in pushedByInfos) {
                if (item.Value.indexInChainPush < lowestIndex) {
                    //previoulowestIndex = lowestIndex;
                    lowestIndex = item.Value.indexInChainPush;
                    failedMoveDir = item.Key;
                    equalLowestIndexExists = false;
                }
                else if (item.Value.indexInChainPush == lowestIndex) {
                    equalLowestIndexExists = true;
                    //break;
                }
            }

            if (failedMoveDir != Vector3.zero && !equalLowestIndexExists) {
                movementReserve.dir = failedMoveDir;
                movementReserve.intentToMove = true;
                gameManager.curGame.obstacleAtDestinationMoves.Add(movementReserve);
            }

        }
        else {
            PushInfo pushInfo;
            if (pushedByInfos.ContainsKey(dirSum)) {
                pushInfo = pushedByInfos[dirSum];
            }
            else {
                if (pushedByInfos.Count != 2) {
                    Debug.Log("Push info count is incorrect");
                }

                // Determine priority
                Vector3 moveDir = Vector3.zero;
                int lowestIndexInChain = 200;
                foreach (var item in pushedByInfos) {
                    if (item.Value.indexInChainPush < lowestIndexInChain) {
                        lowestIndexInChain = item.Value.indexInChainPush;
                        moveDir = item.Key;
                    }
                    else if (item.Value.indexInChainPush == lowestIndexInChain) {
                        moveDir = Vector3.zero;
                        break;
                    }
                }

                if (moveDir != Vector3.zero) {
                    pushInfo = pushedByInfos[moveDir];
                }
                else {
                    PushInfo pushWithWindOrigin = pushedByInfos.Values.ToList().Find(item => item.pushOrigin == 0);
                    if (pushWithWindOrigin != null) {
                        pushInfo = pushWithWindOrigin;
                    }
                    else {
                        //Debug.LogWarning("checking for instance id to determine priority");
                        PushInfo first = pushedByInfos.Values.First();
                        PushInfo last = pushedByInfos.Values.Last();
                        pushInfo = first.pushOrigin > last.pushOrigin ? first : last;
                    }
                }
            }

            if(pushInfo.initiator != null && !pushInfo.initiator.obj.CompareTag("MovingObstacle")) {
                Debug.Log("here15");
                gameManager.curGame.obstacleAtDestinationMoves.Add(movementReserve);
                return;
            }

            movementReserve.destinationTile = pushInfo.destinationTile;
            movementReserve.intentToMove = true;
            movementReserve.pushed = true;
            movementReserve.dir = pushInfo.pushDir;
            movementReserve.to = movementReserve.from + pushInfo.pushDir;
            //Debug.LogWarning("push reserve: " + gameObject.name + " : " + movementReserve.from);
            if (pushInfo.destinationTile == 0) {
                emptyDestintionTileMoves.Add(movementReserve);
                //Debug.LogWarning("here2");
                //Debug.LogWarning("here2: " + gameObject.name);

                //movementReserve.isMomentumTransferred = false;
            }
            //movementReserve.isMomentumTransferred = true;

        }


    }
}
