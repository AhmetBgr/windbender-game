using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        movementReserve.turnID = GameManager.instance.turnID;
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


}
