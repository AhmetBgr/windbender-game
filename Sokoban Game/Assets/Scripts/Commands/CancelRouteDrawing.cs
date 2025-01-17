using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelRouteDrawing : Command
{
    public WindSourceController windSource;
    public RouteManager routeManager;
    private GameManager gameManager;
    private DrawingController drawingController;
    private GameState oldState;
    public List<Vector3> route = new List<Vector3>();
    public CancelRouteDrawing(WindSourceController windSource, RouteManager routeManager, DrawingController drawingController, List<Vector3> route)
    {
        this.windSource = windSource;
        this.routeManager = routeManager;
        this.route = new List<Vector3>();
        this.route.AddRange(route);
        this.gameManager = GameManager.instance;
        this.oldState = gameManager.state;
        this.drawingController = drawingController;
    }
    public override void Execute()
    {
        routeManager.DeleteTiles();
        gameManager.curGame.route.Clear();
        routeManager.validPos.Clear();
        windSource.UpdateWindSP(gameManager.game.route.Count);
        //gameManager.curWindSource.windSP = GameManager.instance.curWindSource.defWindSP;
        ///gameManager.curWindSource.route.Clear();
        drawingController.UpdateValidPositions(Vector3.zero, none: true);
        gameManager.curGame.curWindSource = null;
        drawingController.isDrawingCompleted = false;
        gameManager.state = GameState.Paused;
        executionTime = Time.time;
        //turnID = GameManager.instance.turnID;
    }

    public override void Undo()
    {
        Debug.Log("cancel route drawing undo.");
        // Undo wind source power
        windSource.windSP = windSource.defWindSP - route.Count;

        // Undo game manager stuff
        GameManager.instance.curGame.curWindSource = windSource;
        GameManager.instance.curGame.route = new List<Vector3>();
        GameManager.instance.curGame.route.AddRange(route);
        GameManager.instance.state = oldState;

        // Undo route tile manager stuff
        routeManager.route = new List<Vector3>();
        routeManager.route.AddRange(route);
        drawingController.UpdateValidPositions(route[route.Count - 1]);
        windSource.UpdateWindSP(gameManager.game.route.Count);
        routeManager.DrawRoute(route);
    }
}
