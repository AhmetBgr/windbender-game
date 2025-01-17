using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeButtonState : Command
{
    public ButtonEntity button;
    bool isDown;

    public ChangeButtonState(ButtonEntity button, bool isDown, float executionTime)
    {
        this.button = button;
        this.isDown = isDown;
        this.executionTime = Time.time;
        //turnID = GameManager.instance.turnID;
    }

    public override void Execute() {
        button.isDown = !button.isDown;
    }

    public override void Undo()
    {
        button.isDown = this.isDown;
    }
}
