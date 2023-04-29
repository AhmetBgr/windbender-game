using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMavementManager : ObjectMoveController
{
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

           
            if ( index > 0 || GameManager.instance.isLooping )
            {
                if(index == route.Count)
                    dir = route[index] - route[index-1];
                else
                    dir = route[index] - route[index+1];

                if (index == 0)
                {
                    if (hasSpeed)
                    {
                        intentToMove = true;
                    }
                    else
                    {
                        intentToMove = false;
                    }
                }
            }
        }
        else
        {
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

    }
}
