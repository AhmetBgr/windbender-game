using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWindRoute : Command{
    GameManager gameManager;
    List<Vector3> oldRoute = new List<Vector3>();
    List<WindCutter> windCutters = new List<WindCutter>();

    List<WindCutRequest> cutRequests = new List<WindCutRequest>();
    List<WindRestoreRequest> restoreRequests = new List<WindRestoreRequest>();


    public ChangeWindRoute(GameManager gameManager, List<WindCutRequest> cutRequests, List<WindRestoreRequest> restoreRequests) {
        this.gameManager = gameManager;
        this.oldRoute.AddRange(gameManager.route);
        this.cutRequests.AddRange(cutRequests);
        this.restoreRequests.AddRange(restoreRequests);

    }

    public override void Execute() {
        WindCutRequest windCutRequest = new WindCutRequest(null, Vector3.zero, 99999999);
        windCutters.Clear();

        foreach (var item in cutRequests) {
            if (item.cutIndex < windCutRequest.cutIndex) {
                windCutRequest = item;
            }
            else {
                item.windCutter.isCutting = false;
            }

            if (!windCutters.Contains(item.windCutter))
                windCutters.Add(item.windCutter);
        }

        WindRestoreRequest windRestoreRequest = new WindRestoreRequest(null, Vector3.zero, 9999999);

        foreach (var item in restoreRequests) {
            if (item.index < windRestoreRequest.index)
                windRestoreRequest = item;
            else
                item.windCutter.isCutting = false;

            if (!windCutters.Contains(item.windCutter))
                windCutters.Add(item.windCutter);
        }

        if (windCutRequest.windCutter != null && windCutRequest.cutIndex < windRestoreRequest.index) {
            //windCutRequest.windCutter.isCutting = true;

            windCutRequest.windCutter.CutWind(gameManager.route, windCutRequest.cutIndex);

            //gameManager.windCutRequests.Clear();
            gameManager.curWindCutRequest = windCutRequest;
            Debug.Log("cur wint cutter name: " + gameManager.curWindCutRequest.windCutter.name);
        }

        // check for restore request
        if (windRestoreRequest.windCutter != null) {
            Debug.Log("should restore wind route, restore dir: " + windRestoreRequest.restoreDir);
            int cutLenght = gameManager.curWindSource.defWindSP - windRestoreRequest.index - 1;
            windRestoreRequest.windCutter.Reflect(gameManager.route, windRestoreRequest.restoreDir, cutLenght, windRestoreRequest.index, false);

            //gameManager.windRestoreRequests.Clear();
        }

        // draw wind route
        if (gameManager.route.Count == 0) return;

        //gameManager.wind.DrawWind();
    }

    public override void Undo() {
        gameManager.route.Clear();
        gameManager.route.AddRange(oldRoute);
        gameManager.wind.DrawWind();

        foreach (var item in windCutters) {
            item.isCutting = oldRoute.Contains(item.transform.position);
        }
    }
}
