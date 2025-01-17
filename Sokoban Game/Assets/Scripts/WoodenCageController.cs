using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodenCageController : ObjectMoveController
{
    public override void ReserveMovement(List<Vector3> route) {
        pushedByInfos.Clear();
        movementReserve = null;

        int index = -1; // index in wind route
        bool intentToMove = false;
        bool pushed = true;
        Vector3 pos = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);
        Vector3 previousDir = dir;

        index = -1;
        /*PushInfo pushInfo = new PushInfo();
        pushInfo.pushedBy = null;
        pushInfo.pushOrigin = 0;
        pushInfo.indexInChainPush = 0;
        pushInfo.pushDir = dir;
        pushInfo.isValidated = 2;
        pushedByInfos.Add(dir, pushInfo);
        pushInfoThis = pushInfo;*/

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
        //if (GameManager.instance.isFirstTurn)
        //{
        GameManager.instance.oldCommands.Add(movementReserve);
        //}
    }

}
