using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffRobot : Command
{
    private RobotMoveController obj;
    private bool oldState;
    private bool newState;
    public OnOffRobot(RobotMoveController obj, bool oldState, bool newState)
    {
        this.obj = obj;
        this.oldState = oldState;
        this.newState = newState;
    }

    public override void Execute()
    {
        obj.onOff = newState;
        executionTime = Time.time;
        //turnID = GameManager.instance.turnID;

        GameManager.instance.oldCommands.Add(this);
    }

    public override void Undo()
    {

        obj.onOff = oldState;
    }
}
