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
        this.windMoveRoute.AddRange(GameManager.instance.curGame.windMoveRoute);
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
        GameManager.instance.curGame.curWindSource = windSource;
        GameManager.instance.curGame.route = new List<Vector3>();
        GameManager.instance.curGame.route.AddRange(route);
        gameManager.drawingController.route = gameManager.curGame.route;
        GameManager.instance.state = GameState.DrawingRoute;
        //GameManager.instance.OnDrawingStartedFunc();
        GameManager.instance.drawingController.isDrawingCompleted = true;
        GameManager.instance.curGame.isLooping = isLooping;
        GameManager.instance.curGame.curWindDeformInfo.cutLenght = 0;
        gameManager.isWaiting = false;
        //gameManager.state = GameState.DrawingRoute;
        if(isLooping){
            gameManager.curGame.windMoveRoute = new List<Vector3>();
            gameManager.curGame.windMoveRoute.AddRange(windMoveRoute);
            gameManager.arrowController.SetPositions(this.windMoveRoute);
            gameManager.drawingController.isDrawingMoveRoute = true;
        }
        else {
            gameManager.drawingController.isDrawingMoveRoute = false;
            gameManager.curGame.windMoveRoute.Clear();
            gameManager.arrowController.Clear();
        }
        
        Debug.Log("undno set route");

        gameManager.wind.StopAllCoroutines();
        gameManager.wind.EndWind(0f, isLooping);
        
        // Undo Route manager
        routeManager.DeleteTiles();
        routeManager.route = new List<Vector3>();
        routeManager.route.AddRange(route);
        routeManager.DrawRoute(route);
        routeManager.tilemap.color = new Color(1f, 1f, 1f, 0.75f);
        //GameManager.instance.UpdateValidPositions(route[route.Count -1]);
        //routeManager.UpdateValidPositions(route[route.Count -1]);

        if (nextWS != null)
        {
            nextWS.windSP = nextWS.defWindSP;
            //nextWS.isUsed = false;
            nextWS.MakeUninteractable();
            ///nextWS.route = new List<Vector3>();
        }
    }
}
