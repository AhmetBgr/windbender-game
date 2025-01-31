using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCutter : MonoBehaviour
{
    public Vector3 immediateRestoreDirs = Vector3.zero;
    public bool isDoor;

    private GameManager gameManager;
    [HideInInspector] public Vector3 prevPos;
    [HideInInspector] public bool isCutting = false;

    [HideInInspector] public bool isMoved = false;
    [HideInInspector] public bool canCut = true;
    [HideInInspector] public bool canRestore = false;

    void Start(){
        Vector3 pos = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);
        prevPos = pos;
    }

    private void OnEnable() {
        gameManager = GameManager.instance;
        //gameManager.OnWindRouteGenerated += CheckForDeformRequest;
    }

    private void OnDisable() {
        //gameManager.OnWindRouteGenerated -= CheckForDeformRequest;
    }

    public void OnMoved(Vector3 dest) {
        gameManager = GameManager.instance;
        List<Vector3> route = gameManager.curGame.route;
        Vector3 pos = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);

        //Debug.Log("should try request, is in route: " + route.Contains(pos) + ", iscutting: " + isCutting);
        Debug.Log("should try request for: " + name + ", pos: " + dest);
        bool isInWind = route.Contains(dest);

        Vector3 windTileDir = Vector3.zero;
        int windTileIndex = 0;
        bool isWindTilePerpendicular = false;
        if (isInWind && !isCutting) {
            Debug.Log("should check if wind tile perpendicular");
            windTileIndex = route.IndexOf(dest);
            if(windTileIndex == route.Count - 1) {
                isWindTilePerpendicular = false;
            }
            else {
                isWindTilePerpendicular = Vector3.Dot(route[windTileIndex - 1] - route[windTileIndex], route[windTileIndex] - route[windTileIndex + 1]) == 0;

            }
        }
        Debug.Log("wind tile perpendicular?: " + isWindTilePerpendicular);
        if (isInWind) { // && !isWindTilePerpendicular
            // generate cut request
            int index = route.FindIndex(i => i == dest);

            Vector3 restoreDir = index == 0 ? route[index + 1] - route[index] : route[index] - route[index - 1];

            bool isPerpendicular = Vector3.Dot(immediateRestoreDirs + restoreDir, restoreDir) == 0;

            Debug.Log("immediate restore dir: " + (immediateRestoreDirs + restoreDir));

            if (isPerpendicular) {
                restoreDir = immediateRestoreDirs + restoreDir;
            }

            Debug.Log("cut index: " + index);

            gameManager.curGame.windCutRequests.Add(GenerateNewCutRequest(gameManager, route, dest, index));

            if (isPerpendicular) {

                Debug.Log("adding immidiate restore request");
                gameManager.curGame.windRestoreRequests.Add(new WindRestoreRequest(this, restoreDir, index));

            }


        }
        else if (isCutting && route.Contains(pos)) {
            // generate restore request

            //int index = gameManager.curWindCutRequest.cutIndex; //route.FindIndex(i => i == pos);
            int index = route.FindIndex(i => i == pos);

            Debug.Log("should add restore req, index: " + index);
            Debug.Log("should add restore req, windcutter: " + gameObject.name);

            Vector3 restoreDir = index == 0 ? route[index + 1] - route[index] : route[index] - route[index - 1];
            Debug.Log("should add restore req, restore dir 0: " + restoreDir);

            gameManager.curGame.windRestoreRequests.Add(new WindRestoreRequest(this, restoreDir.normalized, index));
            gameManager.curGame.curWindCutRequest = null;
        }
    }

    private void CheckForDeformRequest(List<Vector3> route) {
        GameManager gameManager = GameManager.instance;


        Vector3 pos = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);

        if (!isDoor) { //  && gameManager.curWindCutRequest != null && gameManager.curWindCutRequest.windCutter == this
            isMoved = pos != prevPos;
            canCut = isMoved ? isMoved : canCut;
        }
        else if (!isDoor)
            isMoved = false;

        prevPos = pos;

        //if (gameManager.curWindCutRequest?.windCutter != null && !isPosChanged) return;

        // Wind route cutting
        if (gameManager.turnCount > 0) {
            /*Debug.Log("obj name: " + gameObject.name);
            Debug.Log("is moved: " + isMoved);
            Debug.Log("is route contains: " + route.Contains(pos));
            Debug.Log("can cut : " + canCut);
            Debug.Log("can restore : " + canRestore);

            */
            if (route.Contains(pos) && canCut) {
                //if (!canCut) return;

                int index = route.FindIndex(i => i == pos);

                Vector3 restoreDir = index == 0 ? route[index + 1] - route[index] : route[index] - route[index - 1];

                bool isPerpendicular = Vector3.Dot(immediateRestoreDirs + restoreDir, restoreDir) == 0;

                //Debug.Log("immediate restore dir: " + (immediateRestoreDirs + restoreDir));

                if (isPerpendicular) {
                    restoreDir = immediateRestoreDirs + restoreDir;
                }

                //Debug.Log("cut index: " + index);

                gameManager.curGame.windCutRequests.Add(GenerateNewCutRequest(gameManager, route, pos, index));
                canRestore = true;
                canCut = false;
                if (isPerpendicular)
                    gameManager.curGame.windRestoreRequests.Add(new WindRestoreRequest(this, restoreDir, index));

            }
            else if (isMoved && canRestore && gameManager.curGame.curWindCutRequest != null) {
                int index = gameManager.curGame.curWindCutRequest.cutIndex; //route.FindIndex(i => i == pos);
                //Debug.Log("should restore wind route, index: " + index);
                //Debug.Log("should restore wind route, windcutter: " + gameObject.name);

                Vector3 restoreDir = index == 0 ? route[index + 1] - route[index] : route[index] - route[index - 1];
                //Debug.Log("should restore wind route, restore dir 0: " + restoreDir);

                gameManager.curGame.windRestoreRequests.Add(new WindRestoreRequest(this, restoreDir.normalized, index));
                gameManager.curGame.curWindCutRequest = null;
                isMoved = false;
                canRestore = false;
                canCut = false;
            }
        }
    }

    public WindCutRequest GenerateNewCutRequest(GameManager gameManager, List<Vector3> route, Vector3 pos, int cutIndex) {
        WindCutRequest windCutRequest = new WindCutRequest(this, pos, cutIndex);
        return windCutRequest;
    }


    public Vector3 CalculateRestoreDir(List<Vector3> route, int indexInWind) {
        //int indexInWind = route.FindIndex(i => i == pos);
        //Debug.Log("cal res dir index: " + indexInWind);

        Vector3 restoreDir = indexInWind == 0 ? route[indexInWind + 1] - route[indexInWind] : route[indexInWind] - route[indexInWind - 1];

        bool isPerpendicular = Vector3.Dot(immediateRestoreDirs + restoreDir, restoreDir) == 0;
        if (isPerpendicular) {
            restoreDir = immediateRestoreDirs + restoreDir;
        }

        return restoreDir;
    }


    public void CutWind(List<Vector3> route, int cutIndex) {
        if (cutIndex < 0 | cutIndex >= route.Count - 1) return;

        Debug.Log("before cut- route count: " + route.Count);


        isCutting = true;
        GameManager gameManager = GameManager.instance;
        Vector3 cutPos = route[cutIndex];

        int count = route.Count;
        int tempCutLenght = count - cutIndex;
        gameManager.curGame.curWindDeformInfo.cutLenght = gameManager.curGame.curWindSource.defWindSP - cutIndex;

        if (cutIndex == 0) {
            gameManager.curGame.curWindDeformInfo.restoreDir = route[cutIndex + 1] - route[cutIndex];
        }
        else {
            gameManager.curGame.curWindDeformInfo.restoreDir = route[cutIndex] - route[cutIndex - 1];
        }

        route.RemoveRange(cutIndex + 1, tempCutLenght - 1);

        Debug.Log("after cut- route count: " + route.Count);

    }

    public void Reflect(List<Vector3> route, Vector3 reflectionDir, int cutLenght, int cutIndex, bool isLooping) {
        if (cutLenght == 0) return;

        Debug.Log("before- should reflect, cut lenght: " + cutLenght + ", cut index: " + cutIndex + ", dir: " + reflectionDir);
        Debug.Log("before- route count: " + route.Count);
        isCutting = true;
        cutLenght = isLooping ? cutLenght - 1 : cutLenght;

        if (cutIndex < route.Count - 1) {
            cutLenght = route.Count - cutIndex -1;
            route.RemoveRange(cutIndex + 1, cutLenght);
        }
        Debug.Log("route count: " + route.Count);
        Debug.Log("should reflect, cut lenght: " + cutLenght + ", cut index: " + cutIndex + ", dir: " + reflectionDir);
        //RaycastHit2D hit = Physics2D.Raycast(route[route.Count - 1] + reflectionDir * 0.5f, reflectionDir, cutLenght - 0.5f, LayerMask.GetMask("Wall", "WindCutter"));
        WindCutter windCutter = null;

        reflectionDir = reflectionDir.normalized;
        List<Vector3> restors = new List<Vector3>();
        for (int i = 1; i < cutLenght + 1; i++)
        {
            Vector3 pos = route[route.Count - 1] + (reflectionDir * i);
            GameObject obj = GridManager.Instance.GetCell(pos).obj;

            if(obj != null && (obj.layer == 8 || obj.layer == 11)){
                if(obj.layer == 11)
                    windCutter = obj.GetComponent<WindCutter>();

                cutLenght = i;
                break;
            }
            else
            {
                restors.Add(pos);
            }
        }

        route.AddRange(restors);

        /*if (hit) {
            if (hit.transform.gameObject.layer == 11) {
                windCutter = hit.transform.gameObject.GetComponent<WindCutter>();
            }

            cutLenght = Mathf.FloorToInt(hit.distance) + 1;
        }*/

        /*for (int i = 0; i < cutLenght; i++) {
            Vector3 restorePos = route[route.Count - 1];
            restorePos += reflectionDir;

            route.Add(restorePos);
        }*/
        Debug.Log("route count after restore: " + route.Count);

        if (windCutter != null) {
            int index = route.Count - 1; //route.FindIndex(i => i == pos);

            Vector3 dir = route[route.Count - 1] - route[route.Count - 2];
            Vector3 restoreDir = windCutter.CalculateRestoreDir(route, index);
            if (Vector3.Dot(dir, restoreDir) == 0) {
                windCutter.Reflect(route, restoreDir, gameManager.curGame.curWindSource.defWindSP - route.Count, index, false);
                windCutter.isCutting = true;
            }

            if (route.Count == 0) return;

            GameManager.instance.wind.DrawWind();
        }
    }
}
