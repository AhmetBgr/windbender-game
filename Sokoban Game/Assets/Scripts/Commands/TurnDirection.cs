using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnDirection : Command
{
    private RobotMoveController obj;
    private Vector3 newDir;
    private Vector3 oldDir;

    public TurnDirection(RobotMoveController obj, Vector3 newDir)
    {
        this.obj = obj;
        this.newDir = newDir;
        this.oldDir = obj.dir;
        this.executionTime = Time.time;

    }

    public override void Execute()
    {

        obj.dir = newDir;
        obj.moveDir = Utility.VectorDirToDir(newDir);
        obj.PlayTurnAnim();


    }


    public override void Undo()
    {
        obj.dir = oldDir;
        obj.moveDir = Utility.VectorDirToDir(oldDir);
        Debug.LogWarning("UNDO ROBOT TURN");
    }
}
