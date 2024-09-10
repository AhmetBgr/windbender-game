using UnityEngine;
using System.Collections.Generic;

public class AddRoutePosition : Command
{
    private WindSourceController windSource;
    private RouteManager routeManager;
    private GameManager gameManager;
    private Vector3 pos;
    private int index;
    private bool isLooping;

    public AddRoutePosition(WindSourceController windSource, RouteManager routeManager, Vector3 pos, int index, bool isLooping = false)
    {
        this.windSource = windSource;
        this.routeManager = routeManager;
        this.pos = pos;
        this.index = index;
        this.isLooping = isLooping;
        gameManager = GameManager.instance;
    }

    public override void Execute()
    {
        gameManager.route.Add(pos);

        List<Vector2> points = new List<Vector2>();
        foreach (var item in gameManager.route) {
            points.Add(item);
        }
        gameManager.wind.col.SetPoints(points);

        routeManager.AddPosition(pos, isLooping: isLooping);
        ///windSource.AddPosition(pos);
        windSource.UpdateWindSP(gameManager.route.Count);
        executionTime = Time.time;
        turnID = GameManager.instance.turnID;
    }

    public override void Undo()
    {
        List<Vector2> points = new List<Vector2>();
        foreach (var item in gameManager.route) {
            points.Add(item);
        }
        gameManager.wind.col.SetPoints(points);

        gameManager.isLooping = false;
        gameManager.isDrawingCompleted = gameManager.route.Count - 1 >= windSource.defWindSP ? true : false;
        if (index == 0) // Undoing initial route position
        {
            // We'are canceling route drawing instead of undoing the added position 
            // becuase this allows other wind sources to be interactable, 
            // since we make them uninteractable at the start of the drawing
            CancelRouteDrawing cancelRouteDrawing = new CancelRouteDrawing(windSource, routeManager, GameManager.instance.route);
            cancelRouteDrawing.Execute();
        }
        else // Undoes the added position
        {
            routeManager.DeletePos(index);

            ///windSource.RemovePosition(index);
            windSource.UpdateWindSP(gameManager.route.Count);

            gameManager.route.RemoveAt(index);
        }

    }
}
