using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutWindRoute : WindDeform{

    public Door door;
    public int cutIndex;
    public int cutPos;

    public CutWindRoute(RouteManager routeManager,  List<Vector3> route, int cutIndex, ParticleSystem cutEffect = null) //, int cutLenght ,Door door,
    {
        this.routeManager = routeManager;
        this.route = route;
        this.cutIndex = cutIndex;
        this.cutEffect = cutEffect;
        routeBeforeDeforming.AddRange(route);
    }

    // Cuts wind route from given index. 
    // This is happens when a wall appears on the wind route. Eg: when door closses
    public override void Execute()
    {
        if (cutIndex < 0) return;
        GameManager gameManager = GameManager.instance;
        Vector3 cutPos = route[cutIndex];

        if (cutEffect){
            SetUpEndPlayCutEffect(gameManager.windRouteDeformInfo.restoreDir, cutPos);
        }

        int count = route.Count;
        int tempCutLenght = count - cutIndex;
        gameManager.windRouteDeformInfo.cutLenght = gameManager.curWindSource.defWindSP - cutIndex;

        if(cutIndex == 0)
        {
            gameManager.windRouteDeformInfo.restoreDir = route[cutIndex + 1] - route[cutIndex];
        }
        else
        {
            gameManager.windRouteDeformInfo.restoreDir = route[cutIndex] - route[cutIndex - 1];
        }
        
        route.RemoveRange(cutIndex, tempCutLenght);
    }

    public override void Undo()
    {
        GameManager gameManager = GameManager.instance;

        gameManager.route.Clear();
        gameManager.route.AddRange(routeBeforeDeforming);
        gameManager.windRouteDeformInfo.cutLenght = 0;
        gameManager.windRouteDeformInfo.cutIndex = -1;

        // Redraws the wind route
        routeManager.DeleteTiles();
        routeManager.DrawWindRoute(route);

    }
}
