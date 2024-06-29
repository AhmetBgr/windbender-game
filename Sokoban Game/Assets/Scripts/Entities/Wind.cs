using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class Wind : MonoBehaviour{

    public List<Vector3> route = new List<Vector3>();
    public List<Vector3> windMoveRoute = new List<Vector3>();
    public WindRouteDeformInfo deformInfo = new WindRouteDeformInfo(null, -1, 0);

    public LineRenderer lr;
    public Material mat;
    public Gradient colorOpen;
    public Gradient colorLoop;
    public Gradient colorCut;


    private GameManager gameManager;

    public float defAlpha;
    public bool isLooping = false;
    private float defSpeed;

    // Start is called before the first frame update
    void Start()
    {

        //route = gameManager.route;
        windMoveRoute = gameManager.windMoveRoute;
        mat = lr.material;
        defAlpha = mat.GetFloat("_alpha");
        defSpeed = mat.GetFloat("_speed");

    }

    private void OnEnable() {
        gameManager = GameManager.instance;
        gameManager.OnStateChange += UpdateWindMatSpeed;
    }

    private void OnDisable() {
        gameManager.OnStateChange += UpdateWindMatSpeed;

    }

    private void UpdateWindMatSpeed(GameState from, GameState to) {
        if(to == GameState.Paused | to == GameState.DrawingRoute) {
            mat.SetFloat("_speed", 0);
        }
        else {
            mat.SetFloat("_speed", defSpeed);

        }
    }

    public void DrawWind() {
        Debug.Log("should draw wind");
        Debug.Log("wind route count: " + route.Count);
        transform.position = gameManager.curWindSource.transform.position;
        route = new List<Vector3>();
        deformInfo = gameManager.curWindDeformInfo;
        foreach (var pos in gameManager.route) {
            route.Add(transform.InverseTransformPoint(pos) + 0.2f * Vector3.up);
        }

        isLooping = gameManager.isLooping;
        if (isLooping)
            route.RemoveAt(route.Count - 1);
        /*else if(deformInfo.cutLenght > 0 && !deformInfo.restore) {
            Vector3 lastPos = route[route.Count - 1];
            //Vector3 dir = (lastPos - route[route.Count - 2]).normalized;
            Vector3 pos = route[route.Count - 1] + deformInfo.restoreDir * 1f;
            route.Add(pos);
        }*/

        lr.positionCount = route.Count;
        lr.SetPositions(route.ToArray());
        lr.loop = isLooping;
        lr.colorGradient = isLooping ? colorLoop : colorOpen; // (windRouteDeformInfo.cutLenght > 0 && !windRouteDeformInfo.restore) ? colorCut : 
    }

    public void StartWind(float dur, float delay = 0) {
        DrawWind();

        mat.SetFloat("_alpha", 0f);
        StartCoroutine(_LerpAlpha(defAlpha, dur, delay));
    }

    public void EndWind(float dur, float delay = 0) {

        StartCoroutine(_LerpAlpha(0f, dur, delay, () => {
            lr.positionCount = 0;
        }));
    }

    public IEnumerator _LerpAlpha(float alpha, float dur, float delay, UnityAction onComplete = null) {
        yield return new WaitForSeconds(delay);
        float initAlpha = mat.GetFloat("_alpha");
        float time = 0;
        while(time < dur) {
            float currentAlpha = Mathf.Lerp(initAlpha, alpha, time / dur);
            mat.SetFloat("_alpha", currentAlpha);
            time += Time.deltaTime;
            yield return null;
        }
        mat.SetFloat("_alpha", alpha);
    }


}

public struct WindRouteDeformInfo {
    public WindCutter windCutter;
    public int cutIndex;
    public int cutLenght;
    public bool restore;
    public Vector3 restoreDir;
    public Vector3 immediateRestoreDir;
    public Vector3 cutPos;

    public WindRouteDeformInfo(WindCutter windCutter, int cutIndex, int cutLenght) { //, Vector3 restoreDir
        this.cutIndex = cutIndex;
        this.windCutter = windCutter;
        this.cutLenght = cutLenght;
        this.restore = false;
        this.restoreDir = Vector3.right;
        this.immediateRestoreDir = Vector3.zero;
        this.cutPos = Vector3.zero;
    }
}
[System.Serializable]
public class WindCutRequest {
    public WindCutter windCutter;
    public int cutIndex;
    public Vector3 cutPos;
    public bool isExeCuted;

    public WindCutRequest(WindCutter windCutter, Vector3 cutPos, int cutIndex) {
        this.cutIndex = cutIndex;
        this.windCutter = windCutter;
        this.cutPos = cutPos;
        this.isExeCuted = false;
    }
}
[System.Serializable]

public class WindRestoreRequest {
    public WindCutter windCutter;
    //public int cutLenght;
    public int index;
    public Vector3 restoreDir;
    public bool isExeCuted;

    public WindRestoreRequest(WindCutter windCutter, Vector3 restoreDir, int index) {
        //this.cutLenght = cutLenght;
        this.index = index;
        this.windCutter = windCutter;
        this.restoreDir = restoreDir;
        this.isExeCuted = false;
    }
}
