using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDrawingMoveRoute : Command
{
    private ArrowController arrowController;
    private GameManager gameManager;

    private Vector3 pos;

    private int index;

    public StartDrawingMoveRoute(ArrowController arrowController, Vector3 pos){
        this.arrowController = arrowController;
        this.gameManager = GameManager.instance;
        this.pos = pos;
        this.index = gameManager.windMoveRoute.Count;
    }

    public override void Execute(){
        gameManager.isDrawingMoveRoute = true;
        arrowController.transform.gameObject.SetActive(true);
        arrowController.AddPos(pos);
        gameManager.windMoveRoute.Add(pos);
    }

    public override void Undo(){
        // Returns to drawing route state from drawing move route state
        /*arrowController.lr.positionCount = 0;
        arrowController.transform.gameObject.SetActive(false);
        gameManager.windMoveRoute.Clear();
        //gameManager.UpdateValidPositions(pos, none : true);
        gameManager.isDrawingMoveRoute = false;*/
    }
}
