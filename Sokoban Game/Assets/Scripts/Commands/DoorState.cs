using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorState : Command
{
    public Door door;
    public bool isOpen;

    public DoorState(Door door, bool isOpen,  float executionTime)
    {
        this.door = door;
        this.isOpen = isOpen;
        this.executionTime = executionTime;
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
