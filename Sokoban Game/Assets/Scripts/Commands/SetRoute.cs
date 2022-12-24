using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRoute : Command
{
    public WindSourceController windSource;
    public WindSourceController nextWS;
    public RouteManager routeManager;
    public List<Vector3> route = new List<Vector3>();
    public bool isLooping;

    public SetRoute(WindSourceController windSource, RouteManager routeManager, List<Vector3> route, bool isLooping)
    {
        this.windSource = windSource;
        this.routeManager = routeManager;
        this.route.AddRange(route);
        this.isLooping = isLooping;
    }

    public override void Execute()
    {
        base.Execute();
    }

    public void StartWind()
    {
        //GameManager.instance.SetRoute(route);
    }

    public override void Undo()
    {
        // Undo Wind source
        windSource.isUsed = false;
        windSource.MakeInteractable();
        

        // update game manager
        GameManager.instance.curWindSource = windSource;
        GameManager.instance.route = new List<Vector3>();
        GameManager.instance.route.AddRange(route);
        GameManager.instance.state = GameState.DrawingRoute;
        //GameManager.instance.OnDrawingStartedFunc();
        GameManager.instance.isDrawingCompleted = true;
        GameManager.instance.isLooping = isLooping;
        GameManager.instance.cutLenght = 0;
        
        // Undo Route manager
        routeManager.DeleteTiles();
        routeManager.route = new List<Vector3>();
        routeManager.route.AddRange(route);
        routeManager.DrawRoute(route);
        routeManager.UpdateValidPositions(route[route.Count -1]);
        //routeManager.UpdateValidPositions(route[route.Count -1]);

        if(nextWS != null)
        {
            nextWS.windSP = nextWS.defWindSP;
            nextWS.MakeUninteractable();
            ///nextWS.route = new List<Vector3>();
        }
    }
}
