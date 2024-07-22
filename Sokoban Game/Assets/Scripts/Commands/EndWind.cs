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
    bool isLooping;

    public EndWind(GameManager gameManager, Wind wind, ArrowController arrowController) {
        this.arrowController = arrowController;
        this.gameManager = gameManager;
        this.wind = wind;
        this.route.AddRange(gameManager.route);
        this.windMoveRoute.AddRange(gameManager.windMoveRoute);
        curWindSource = gameManager.curWindSource;
        isLooping = gameManager.isLooping;
    }

    public override void Execute() {
        gameManager.route.Clear();
        windMoveRoute.Clear();
        arrowController.Clear();
        wind.EndWind(gameManager.defTurnDur*2, isLooping);
        gameManager.windMoveRoute.Clear();
        //routeManager.transform.position = Vector3.zero;
    }

    public override void Undo() {
        Debug.Log("should undo end wind");

        gameManager.route.AddRange(route);
        gameManager.windMoveRoute.AddRange(windMoveRoute);
        gameManager.curWindSource = curWindSource;
        if(windMoveRoute.Count > 0) {
            arrowController.SetPositions(windMoveRoute);
        }

        wind.DrawWind();
        wind.mat.SetFloat("_alpha", wind.defAlpha);

    }


}
