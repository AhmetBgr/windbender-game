using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseRobot : Command
{
    private RobotMoveController obj;

    public CloseRobot(RobotMoveController obj)
    {
        this.obj = obj;
    }

    public override void Execute()
    {

        executionTime = Time.time;
        turnID = GameManager.instance.turnID;
    }

    public override void Undo()
    {


    }
}
