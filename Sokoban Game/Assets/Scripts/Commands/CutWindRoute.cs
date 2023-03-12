using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutWindRoute : Command
{
    public RouteManager routeManager;
    public Door door;
    public List<Vector3> route;
    public List<Vector3> routeBeforeCutting = new List<Vector3>();
    public int cutIndex;
    //public int cutLenght;
    public int cutPos;

    public CutWindRoute(RouteManager routeManager,  List<Vector3> route, int cutIndex) //, int cutLenght ,Door door,
    {
        this.routeManager = routeManager;
        //this.door = door;
        this.route = route;
        this.cutIndex = cutIndex;
        //this.cutLenght = cutLenght;
        routeBeforeCutting.AddRange(route);
    }

    // Cuts wind route from given index. 
    // This is happens when a wall appears on the wind route. Eg: when door closses
    public override void Execute()
    {
        if (cutIndex < 0) return; 

        int count = route.Count;
        int tempCutLenght = count - cutIndex;
        GameManager.instance.windRouteDeformInfo.cutLenght = GameManager.instance.curWindSource.defWindSP - cutIndex;

        if(cutIndex == 0)
        {
            GameManager.instance.windRouteDeformInfo.restoreDir = route[cutIndex + 1] - route[cutIndex];
        }
        else
        {
            GameManager.instance.windRouteDeformInfo.restoreDir = route[cutIndex] - route[cutIndex - 1];
        }
        
        route.RemoveRange(cutIndex, tempCutLenght);
    }

    public override void Undo()
    {
        GameManager gameManager = GameManager.instance;

        gameManager.route.Clear();
        gameManager.route.AddRange(routeBeforeCutting);
        gameManager.windRouteDeformInfo.cutLenght = 0;
        gameManager.windRouteDeformInfo.cutIndex = -1;

        // Redraws the wind route
        routeManager.DeleteTiles();
        routeManager.DrawWindRoute(route);

    }
}
