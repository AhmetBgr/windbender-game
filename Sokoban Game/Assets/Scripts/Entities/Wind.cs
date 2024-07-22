using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class Wind : MonoBehaviour{

    private static readonly Dictionary<(Vector3, Vector3), int> RotDirMap = new Dictionary<(Vector3, Vector3), int>
    {
        { (Vector3.right, Vector3.up), 1 },
        { (Vector3.right, Vector3.down), -1 },
        { (Vector3.left, Vector3.up), -1 },
        { (Vector3.left, Vector3.down), 1 },
        { (Vector3.down, Vector3.right), 1 },
        { (Vector3.down, Vector3.left), -1 },
        { (Vector3.up, Vector3.right), 1 },
        { (Vector3.up, Vector3.left), -1 }
    };

    public List<Vector3> route = new List<Vector3>();
    public List<Vector3> windMoveRoute = new List<Vector3>();
    public WindRouteDeformInfo deformInfo = new WindRouteDeformInfo(null, -1, 0);

    public LineRenderer lr;
    public SpriteRenderer tornado;
    public Material mat;
    public Material tornadoMat;

    public Gradient colorOpen;
    public Gradient colorLoop;
    public Gradient colorCut;
    public Gradient color2L;



    private GameManager gameManager;

    public float defAlpha;
    public float defAlpha2;

    public bool isLooping = false;
    private float defSpeed;

    // Start is called before the first frame update
    void Start()
    {

        //route = gameManager.route;
        windMoveRoute = gameManager.windMoveRoute;
        mat = lr.material;
        tornadoMat = tornado.material;
        defAlpha = mat.GetFloat("_alpha");
        defAlpha2 = tornadoMat.GetFloat("_alpha");

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


        if (isLooping) {

            route.RemoveAt(route.Count - 1);

            Vector3 dir1 = (route[1] - route[0]).normalized;
            Vector3 dir2 = (route[2] - route[1]).normalized;

            int rotDir = GetRotationDirection(route);

            tornadoMat.SetFloat("_speed", 7 * rotDir);

            Vector3 sum = Vector3.zero;
            foreach (var item in route) {
                sum += item;
            }
            Vector3 center = sum / route.Count;
            tornado.transform.localPosition = (center - route[0])+ 0.1f * Vector3.up;
            tornado.gameObject.SetActive(true);
            lr.enabled = false;
        }
        else {
            lr.enabled = true;
            tornado.gameObject.SetActive(false);

            Vector3 lastPos = route[route.Count - 1];
            //Vector3 dir = (lastPos - route[route.Count - 2]).normalized;
            Vector3 pos = lastPos + (lastPos - route[route.Count - 2]).normalized * 0.5f;
            route[route.Count - 1] = pos; 
            Vector3 firstPos = route[0];
            pos = firstPos + (firstPos-route[1]).normalized * 0.5f;
            route[0] = pos;

        }
        /*else if(deformInfo.cutLenght > 0 && !deformInfo.restore) {
            Vector3 lastPos = route[route.Count - 1];
            //Vector3 dir = (lastPos - route[route.Count - 2]).normalized;
            Vector3 pos = route[route.Count - 1] + deformInfo.restoreDir * 1f;
            route.Add(pos);
        }*/

        lr.positionCount = route.Count;
        lr.SetPositions(route.ToArray());
        lr.loop = isLooping;

        if(gameManager.route.Count == 2) {
            lr.colorGradient = color2L;
        }
        else if (isLooping) {
            lr.colorGradient = colorLoop;
        }
        else {
            lr.colorGradient = colorOpen;
        }

    }

    public void StartWind(float dur, bool isLooping, float delay = 0) {
        DrawWind();

        if (!isLooping) {
            mat.SetFloat("_alpha", 0f);
            StartCoroutine(_LerpAlpha(mat, defAlpha, dur, delay));
        }
        else {
            //tornado.color = Color.clear;
            //StartCoroutine(_LerpSpriteColor(tornado, Color.white, dur, delay));
            tornadoMat.SetFloat("_alpha", 0f);

            StartCoroutine(_LerpAlpha(tornadoMat, defAlpha2, dur, delay));

        }

    }

    public void EndWind(float dur, bool isLooping, float delay = 0) {
        if (!isLooping) {
            StartCoroutine(_LerpAlpha(mat, 0f, dur, delay, () => {
                lr.positionCount = 0;
            }));
            tornado.gameObject.SetActive(false);
        }
        else {
            Debug.Log("shoud end tornado");
            lr.enabled = false;

            StartCoroutine(_LerpAlpha(tornadoMat, 0f, dur, delay));
        }
    }

    public IEnumerator _LerpAlpha(Material mat, float alpha, float dur, float delay, UnityAction onComplete = null) {
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

    public IEnumerator _LerpSpriteColor(SpriteRenderer sprite, Color color, float dur, float delay, UnityAction onComplete = null) {
        yield return new WaitForSeconds(delay);

        Color initColor = sprite.color;
        float time = 0;
        while (time < dur) {
            Color currentColor = Color.Lerp(initColor, color, time / dur);

            sprite.color = currentColor;

            time += Time.deltaTime;


            yield return null;
        }

        sprite.color = color;

    }

    public int GetRotationDirection(List<Vector3> route) {
        if (route == null || route.Count < 3) {
            Debug.LogError("route cannot be null or less than 3");
            return 0;
        } 

        Vector3 dir1 = (route[1] - route[0]).normalized;
        Vector3 dir2 = (route[2] - route[1]).normalized;

        RotDirMap.TryGetValue((dir1, dir2), out int rotDir);
        return rotDir;
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


