using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndWind : Command{
    GameManager gameManager;
    ArrowController arrowController;
    Wind wind;
    WindSourceController curWindSource;
    List<Vector3> route = new List<Vector3>();
    List<Vector3> windMoveRoute = new List<Vector3>();
    Vector3 windPos;

    public EndWind(GameManager gameManager, Wind wind, ArrowController arrowController) {
        this.arrowController = arrowController;
        this.gameManager = gameManager;
        this.wind = wind;
        this.route.AddRange(gameManager.route);
        this.windMoveRoute.AddRange(gameManager.windMoveRoute);
        curWindSource = gameManager.curWindSource;
    }

    public override void Execute() {
        gameManager.route.Clear();
        windMoveRoute.Clear();
        arrowController.Clear();
        wind.EndWind(gameManager.defTurnDur);
        //routeManager.transform.position = Vector3.zero;
    }

    public override void Undo() {
        gameManager.route.AddRange(route);
        gameManager.windMoveRoute.AddRange(windMoveRoute);
        gameManager.curWindSource = curWindSource;
        if(windMoveRoute.Count > 0) {
            arrowController.SetPositions(windMoveRoute);

        }
        wind.mat.SetFloat("_alpha", wind.defAlpha);

        wind.DrawWind();
    }


}
