using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRoute : Command
{
    public WindSourceController windSource;
    public WindSourceController nextWS;
    public RouteManager routeManager;

    private GameManager gameManager;

    public List<Vector3> route = new List<Vector3>();
    public List<Vector3> windMoveRoute = new List<Vector3>();
    public bool isLooping;

    public SetRoute(GameManager gameManager, WindSourceController windSource, RouteManager routeManager, List<Vector3> route, bool isLooping)
    {
        this.windSource = windSource;
        this.routeManager = routeManager;
        this.route.AddRange(route);
        this.windMoveRoute.AddRange(GameManager.instance.windMoveRoute);
        this.isLooping = isLooping;
        this.gameManager = gameManager;
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
        GameManager.instance.windRouteDeformInfo.cutLenght = 0;
        gameManager.isWaiting = false;
        if(isLooping){
            gameManager.windMoveRoute = new List<Vector3>();
            gameManager.windMoveRoute.AddRange(windMoveRoute);
            gameManager.arrowController.SetPositions(this.windMoveRoute);
            gameManager.isDrawingMoveRoute = true;
        }

        
        // Undo Route manager
        routeManager.DeleteTiles();
        routeManager.route = new List<Vector3>();
        routeManager.route.AddRange(route);
        routeManager.DrawRoute(route);
        //GameManager.instance.UpdateValidPositions(route[route.Count -1]);
        //routeManager.UpdateValidPositions(route[route.Count -1]);

        if(nextWS != null)
        {
            nextWS.windSP = nextWS.defWindSP;
            //nextWS.isUsed = false;
            nextWS.MakeUninteractable();
            ///nextWS.route = new List<Vector3>();
        }
    }
}
