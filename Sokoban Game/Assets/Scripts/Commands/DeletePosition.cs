using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeletePosition : Command
{
    public WindSourceController windSource;
    public RouteManager routeManager;
    private GameManager gameManager;
    public Vector3 pos;
    public DeletePosition(WindSourceController windSource, RouteManager routeManager, Vector3 pos)
    {
        this.windSource = windSource;
        this.routeManager = routeManager;
        this.pos = pos;
        turnID = GameManager.instance.turnID;
        gameManager = GameManager.instance;
    }


    public override void Execute()
    {
        gameManager.route.RemoveAt(gameManager.route.Count - 1);
        gameManager.curWindSource.UpdateWindSP(gameManager.route.Count);

        List<Vector2> points = new List<Vector2>();
        foreach (var item in gameManager.route) {
            points.Add(item);
        }
        gameManager.wind.col.SetPoints(points);
    }


    public override void Undo()
    {
        GameManager.instance.route.Add(pos);
        List<Vector2> points = new List<Vector2>();
        foreach (var item in gameManager.route) {
            points.Add(item);
        }
        gameManager.wind.col.SetPoints(points);
        routeManager.AddPosition(pos);
        ////windSource.AddPosition(pos);
        windSource.UpdateWindSP(GameManager.instance.route.Count);
    }
}
