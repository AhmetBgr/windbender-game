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
    private Material mat;
    public Gradient colorOpen;
    public Gradient colorLoop;
    public Gradient colorCut;


    private GameManager gameManager;

    public float defAlpha;
    public bool isLooping = false;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;

        //route = gameManager.route;
        windMoveRoute = gameManager.windMoveRoute;
        mat = lr.material;
        defAlpha = mat.GetFloat("_alpha");
    }

    public void DrawWind() {
        Debug.Log("should draw wind");
        Debug.Log("wind route count: " + route.Count);
        transform.position = gameManager.curWindSource.transform.position;
        route = new List<Vector3>();
        deformInfo = gameManager.windRouteDeformInfo;
        foreach (var pos in gameManager.route) {
            route.Add(transform.InverseTransformPoint(pos) + 0.2f * Vector3.up);
        }

        isLooping = gameManager.isLooping;
        if (isLooping)
            route.RemoveAt(route.Count - 1);
        else if(deformInfo.cutLenght > 0 && !deformInfo.restore) {
            Vector3 lastPos = route[route.Count - 1];
            //Vector3 dir = (lastPos - route[route.Count - 2]).normalized;
            Vector3 pos = route[route.Count - 1] + deformInfo.restoreDir * 1f;
            route.Add(pos);
        }

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
    public Door door;
    public int cutIndex;
    public int cutLenght;
    public bool restore;
    public Vector3 restoreDir;

    public WindRouteDeformInfo(Door door, int cutIndex, int cutLenght) { //, Vector3 restoreDir
        this.cutIndex = cutIndex;
        this.door = door;
        this.cutLenght = cutLenght;
        this.restore = false;
        this.restoreDir = Vector3.right;
    }
}
