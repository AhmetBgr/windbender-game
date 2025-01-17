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
    int defTurnCount;

    public EndWind(GameManager gameManager, Wind wind, ArrowController arrowController) {
        this.arrowController = arrowController;
        this.gameManager = gameManager;
        this.wind = wind;
        this.route.AddRange(gameManager.curGame.route);
        this.windMoveRoute.AddRange(gameManager.curGame.windMoveRoute);
        curWindSource = gameManager.curGame.curWindSource;
        isLooping = gameManager.curGame.isLooping;
        windPos = wind.transform.position;
        defTurnCount = gameManager.defTurnCount;
    }

    public override void Execute() {
        gameManager.curGame.route.Clear();
        arrowController.Clear();
        wind.EndWind(gameManager.defTurnDur*2, isLooping);
        gameManager.curGame.windMoveRoute.Clear();
        //routeManager.transform.position = Vector3.zero;
    }

    public override void Undo() {
        Debug.Log("should undo end wind");

        gameManager.curGame.route.AddRange(route);
        gameManager.curGame.windMoveRoute.AddRange(windMoveRoute);
        gameManager.curGame.curWindSource = curWindSource;
        gameManager.curGame.isLooping = isLooping;
        gameManager.defTurnCount = defTurnCount;
        if(windMoveRoute.Count > 0) {
            arrowController.SetPositions(windMoveRoute);
        }

        wind.StopAllCoroutines();
        wind.DrawWind();
        wind.transform.position = windPos;
        wind.mat.SetFloat("_alpha", wind.defAlpha);

    }


}
