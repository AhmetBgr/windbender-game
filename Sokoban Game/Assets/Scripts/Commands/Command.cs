using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Command
{
    public float executionTime;

    public delegate void OnExecuteDelegate(Command command);
    public static event OnExecuteDelegate OnExecute;

    public virtual void Execute()
    {
        if(OnExecute != null)
        {
            OnExecute(this);
        }
    }

    public abstract void Undo();

}
