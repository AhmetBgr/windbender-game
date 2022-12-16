using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletePosition : Command
{
    public WindSourceController windSource;
    public RouteManager routeManager;

    public Vector3 pos;
    public DeletePosition(WindSourceController windSource, RouteManager routeManager, Vector3 pos)
    {
        this.windSource = windSource;
        this.routeManager = routeManager;
        this.pos = pos;
    }


    public override void Execute()
    {
        base.Execute();
    }


    public override void Undo()
    {
        GameManager.instance.route.Add(pos);
        routeManager.AddPosition(pos);
        ////windSource.AddPosition(pos);
        windSource.UpdateWindSP(GameManager.instance.route.Count);
    }
}
