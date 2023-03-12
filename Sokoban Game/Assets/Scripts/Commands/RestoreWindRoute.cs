using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreWindRoute : Command
{
    public RouteManager routeManager;
    public List<Vector3> route;
    public List<Vector3> routeBeforeRestoring = new List<Vector3>();
    public Vector3 cutPos;
    public int cutIndex;
    public int cutLenght;
    public bool isLooping;

    public RestoreWindRoute(RouteManager routeManager, List<Vector3> route, int cutIndex, int cutLenght)
    {
        this.routeManager = routeManager;
        this.route = route;
        this.cutIndex = cutIndex;
        this.cutLenght = cutLenght;
        routeBeforeRestoring.AddRange(route);
    }

    // Restores wind route linearly after cut position.
    // This is happens when a wall dissappears from the cut possition. Eg: when door opens
    public override void Execute(){
        if ( cutLenght == 0 ) return;
        if ( route.Count < 3 ) return;

        cutLenght = isLooping ? cutLenght - 1 : cutLenght;
        Vector3 restoreDir = GameManager.instance.windRouteDeformInfo.restoreDir;

        RaycastHit2D hit = Physics2D.Raycast(route[route.Count - 1], restoreDir, cutLenght, LayerMask.GetMask("Wall"));

        if (hit){
            cutLenght = Mathf.FloorToInt(hit.distance);
        }

        for (int i = 0; i < cutLenght; i++){
            Vector3 restorePos = route[route.Count - 1];
            restorePos += restoreDir;
            
            route.Add(restorePos);
        }

        // Redraws the wind route
        routeManager.DeleteTiles();
        routeManager.DrawWindRoute(route);

        cutLenght = 0;
    }

    public override void Undo()
    {
        GameManager gameManager = GameManager.instance;

        gameManager.route.Clear();
        gameManager.route.AddRange(routeBeforeRestoring);
        gameManager.windRouteDeformInfo.cutLenght = cutLenght;
        gameManager.windRouteDeformInfo.cutIndex = cutIndex;

        // Redraws the wind route
        routeManager.DeleteTiles();
        routeManager.DrawWindRoute(route);
    }
}
