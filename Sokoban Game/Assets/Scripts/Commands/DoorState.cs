using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorState : Command
{
    public Door door;
    public bool isOpen;
    private bool shouldClose;
    public DoorState(Door door, bool isOpen,  float executionTime)
    {
        this.door = door;
        this.isOpen = isOpen;
        this.executionTime = executionTime;
        turnID = GameManager.instance.turnID;
        shouldClose = door.shouldClose;
    }

    public override void Execute() {
        door.ToggleState(door.tag);
    }

    public override void Undo()
    {
        if (isOpen)
        {
            door.Open();
        }
        else
        {
            door.Close();
        }
    }
}
