using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCrateMoveController : CrateMoveController
{
    public override void ReserveMovement(List<Vector3> route) {
        base.ReserveMovement(route);
    }
}
