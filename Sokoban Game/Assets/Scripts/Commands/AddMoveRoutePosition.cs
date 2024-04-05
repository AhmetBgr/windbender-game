using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMoveRoutePosition : Command{
    private ArrowController arrowController;
    private GameManager gameManager;

    public List<Vector3> windMoveRoute = new List<Vector3>();
    private Vector3[] positions;
    private Vector3 pos;

    private int index;

    public AddMoveRoutePosition(ArrowController arrowController, Vector3 pos){
        this.arrowController = arrowController;
        this.gameManager = GameManager.instance;
        this.pos = pos;
        this.index = gameManager.windMoveRoute.Count;
        this.windMoveRoute.AddRange(gameManager.windMoveRoute);
        arrowController.lr.GetPositions(this.positions);
    }

    public override void Execute(){
        /*arrowController.transform.gameObject.SetActive(true);
        arrowController.AddPos(pos);
        gameManager.windMoveRoute.Add(pos);
        gameManager.UpdateValidPositions(gameManager.windMoveRoute[gameManager.windMoveRoute.Count-1], setAllValid : true);*/
    }

    public override void Undo(){
        // Undo added pos
        /*arrowController.lr.SetPositions(positions);
        gameManager.windMoveRoute.Clear();
        gameManager.windMoveRoute.AddRange(windMoveRoute);
        gameManager.UpdateValidPositions(gameManager.windMoveRoute[gameManager.windMoveRoute.Count-1], deleting : true);

        if(index == 0){
            // Returns to drawing route state if first position removed
            gameManager.isDrawingMoveRoute = false;
            arrowController.transform.gameObject.SetActive(false);
            return;
        }*/
    }
}
