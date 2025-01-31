using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingController : MonoBehaviour
{
    public List<Vector3> route = new List<Vector3>();
    //public List<Vector3> windMoveRoute = new List<Vector3>();
    public List<Vector3> windPath = new List<Vector3>();
    public List<Vector3> validPos = new List<Vector3>();

    public List<Command> simSingleStepCommands = new List<Command>();
    public List<MultipleCommand> simMultiStepCommands = new List<MultipleCommand>();

    public Cursor cursor;
    //public RouteManager routeManager;

    public GameManager gameManager;
    public ArrowController arrowController;
    public GameObject[] validPositionSprites;
    public GameObject[] validRemovePositionSprites;

    private Vector3[] neighborVectors = new Vector3[4] { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
    private Vector3 cursorPos;
    public Color validPosAlternateColor;

    [HideInInspector] public Turn curSimTurn;
    [HideInInspector] public MultipleCommand windSimTurnsCommand;


    //public bool isHoveringUI = false;
    public bool isDrawingMoveRoute = false;
    private bool _isDrawingCompleted = false;
    public bool isDrawingCompleted {
        get { return _isDrawingCompleted; }
        set {
            if (_isDrawingCompleted != value && OnDrawingCompleted != null) {
                OnDrawingCompleted(value);
            }

            _isDrawingCompleted = value;
        }
    }

    public delegate void OnDrawingCompletedDelegate(bool value);
    public static event OnDrawingCompletedDelegate OnDrawingCompleted; // On drawing completed but not wind blowing started

    public delegate void OnRouteChangedDelegate();
    public static event OnRouteChangedDelegate OnRouteChanged;



    private void Start() {
        gameManager = GameManager.instance;
        route = gameManager.game.route;
        //windMoveRoute = gameManager.windMoveRoute;
        windPath = gameManager.game.windPath;
        cursor = Cursor.instance;
        //routeManager = gameManager.routeManager;
    }
    private void LateUpdate() {
        if (cursor == null) return;

        cursorPos = cursor.pos;
    }  

    void Update()
    {
        if (gameManager.state == GameState.DrawingRoute) {

            // drawing route for wind

            if (gameManager.game.curWindSource.isAlternative) {
                // Drawing wind route

                if (Input.GetMouseButtonDown(0))
                    UpdateValidPositions(route[route.Count - 1]);
                else if (Input.GetMouseButtonDown(1) && route.Count >= 2)
                    UpdateValidPositions(route[route.Count - 1], deleting: true);
                else if (Input.GetMouseButtonUp(2)) {
                    // Deletes all route tiles and cancels drawing route
                    CancelDrawing();
                    return;
                }

                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    UpdateValidPositions(cursorPos, none: true);

                if (cursorPos == route[route.Count - 1]) return;

                // Update path pos
                if (Input.GetMouseButton(0)) {

                    if (windPath.Count < gameManager.game.curWindSource.defWindSP && validPos.Contains(cursorPos)) {
                        // Adds new position
                        arrowController.transform.gameObject.SetActive(true);
                        arrowController.AddPos(cursorPos);
                        windPath.Add(cursorPos);
                    }

                    if (windPath.Count >= gameManager.game.curWindSource.defWindSP) {
                        isDrawingCompleted = true;
                    }

                    Vector3 newPos = cursorPos;

                    float distance = 99999999f;
                    if (!validPos.Contains(cursorPos) && (cursorPos - route[route.Count - 1]).magnitude > 1) {
                        foreach (var item in validPos) {
                            float distance2 = (cursorPos - item).magnitude;
                            Vector3 validPosDir = (item - route[route.Count - 1]).normalized;
                            Vector3 cursorDir = (cursorPos - route[route.Count - 1]).normalized;
                            float angle = Vector3.Angle(cursorDir, validPosDir);
                            if (distance2 < distance && !isDrawingCompleted && item != route[route.Count - 1] && angle < 90) {
                                distance = distance2;
                                newPos = item;
                            }
                        }
                    }
                    if ((route.Count < 4) && validPos.Contains(newPos)) {

                        // Adds new position to the route
                        AddRoutePosition addNewPos = new AddRoutePosition(gameManager.game.curWindSource, gameManager.routeManager, this, newPos, route.Count, false);
                        addNewPos.Execute();
                        gameManager.game.curWindSource.oldCommands.Add(addNewPos);

                        if (route.Count >= 4) //Checks if the route is completed
                        {
                            // Set the drawing as completed but player still can draw if looping is possible
                            //isDrawingCompleted = true;
                            gameManager.turnCount = gameManager.game.curWindSource.defWindSP;

                            // Starts drawing movement route for the looped wind
                            if (gameManager.game.isLooping) {
                                isDrawingCompleted = false;
                                StartDrawingMoveRoute startDrawingMoveRoute = new StartDrawingMoveRoute(arrowController, cursorPos);
                                startDrawingMoveRoute.Execute();
                                //startDrawingMoveRoute.turnID = gameManager.turnID + 1;
                            }

                            UpdateValidPositions(route[route.Count - 1]);
                        }
                        else {
                            isDrawingMoveRoute = false;
                            ///(route[route.Count - 1]);
                            isDrawingCompleted = false;
                            arrowController.transform.gameObject.SetActive(false);
                        }
                    }



                    UpdateValidPositions(windPath[windPath.Count - 1]);
                    return;


                }
                else if (Input.GetMouseButton(1)) {
                    if (route.Count >= 2 && cursorPos == route[route.Count - 2]) {
                        // Deletes the last position of the route
                        if (route.Count == 2) {
                            new CancelRouteDrawing(gameManager.game.curWindSource, gameManager.routeManager, this, route).Execute();
                        }
                        else {
                            gameManager.game.isLooping = false;
                            isDrawingCompleted = route.Count - 1 >= gameManager.game.curWindSource.defWindSP ? true : false;
                            gameManager.routeManager.DeletePos(route.Count - 1);
                            DeletePosition deletePosition = new DeletePosition(gameManager.game.curWindSource, gameManager.routeManager, route[route.Count - 1]);
                            deletePosition.Execute();
                            gameManager.game.curWindSource.oldCommands.Add(deletePosition);
                            ////curWindSource.RemovePosition(route.Count - 1);



                            //if (route.Count >= 2)
                            UpdateValidPositions(route[route.Count - 1], deleting: true); //route[route.Count - 2]
                        }
                    }


                    // Remove last position
                    arrowController.RemoveLastPos();
                    windPath.RemoveAt(gameManager.game.windMoveRoute.Count - 1);
                    if (gameManager.game.windMoveRoute.Count >= 2)
                        UpdateValidPositions(windPath[windPath.Count - 2], deleting: true);
                    else if (gameManager.game.windMoveRoute.Count == 1) {
                        windPath.Clear();
                        UpdateValidPositions(route[route.Count - 1], deleting: true);
                        //isDrawingMoveRoute = true;
                    }
                    else if (gameManager.game.windMoveRoute.Count == 0) {

                    }
                }


            }
            else if (isDrawingMoveRoute) {
                // Drawing movement route for the looped winds
                //Vector3 cursorPos2 = cursor.pos;
                //Debug.Log("here1");
                if (Input.GetMouseButtonDown(0))
                    UpdateValidPositions(gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 1]);
                else if (Input.GetMouseButtonDown(1)) {
                    if (gameManager.game.windMoveRoute.Count >= 2)
                        UpdateValidPositions(gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 1], deleting: true);
                    else if (gameManager.game.windMoveRoute.Count == 1) {
                        //isDrawingMoveRoute = false;
                        UpdateValidPositions(route[route.Count - 1], deleting: true);
                        //isDrawingMoveRoute = true;
                    }
                }
                else if (Input.GetMouseButtonUp(2)) {
                    CancelDrawing();
                    /*
                    // Deletes all route tiles and cancels drawing route
                    //Undo();
                    arrowController.lr.positionCount = 1;
                    arrowController.transform.gameObject.SetActive(false);
                    windMoveRoute.RemoveRange(0, windMoveRoute.Count);
                    //gameManager.UpdateValidPositions(pos, none : true);
                    isDrawingMoveRoute = false;
                    //isDrawingCompleted = true;

                    Debug.LogWarning("should return drawing route");

                    // Deletes the last position of the route
                    isLooping = false;
                    //isDrawingCompleted = route.Count - 1 >= curWindSource.defWindSP ? true : false;
                    //routeManager.DeletePos(route.Count - 1);
                    //DeletePosition deletePosition = new DeletePosition(curWindSource, routeManager, route[route.Count - 1]);

                    //curWindSource.oldCommands.Add(deletePosition);
                    //curWindSource.UpdateWindSP(route.Count);

                    //route.RemoveAt(route.Count - 1);
                    UpdateValidPositions(route[route.Count - 1]);


                    CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
                    cancelDrawing.Execute();
                    //Debug.LogWarning("should cancel drawing");
                    //isDrawingCompleted = false;
                    turnCount = route.Count;

                    //Debug.Log("here2");
                    */
                    return;
                }

                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    UpdateValidPositions(cursorPos, none: true);

                //Debug.Log("here3");

                if (!validPos.Contains(cursorPos)) return; 
                //gameManager.game.windMoveRoute.Count == 0 ||
                //cursorPos == gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 1] |


                //Debug.Log("here4");

                if (Input.GetMouseButton(0)) {
                    // Adds new position
                    arrowController.transform.gameObject.SetActive(true);
                    arrowController.AddPos(cursorPos);
                    gameManager.game.windMoveRoute.Add(cursorPos);
                    UpdateValidPositions(gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 1]);

                    gameManager.StartSim();

                }
                else if (Input.GetMouseButton(1)) {
                    // Remove last position
                    arrowController.RemoveLastPos();
                    gameManager.game.windMoveRoute.RemoveAt(gameManager.game.windMoveRoute.Count - 1);
                    if (gameManager.game.windMoveRoute.Count >= 2)
                        UpdateValidPositions(gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 2], deleting: true);
                    else if (gameManager.game.windMoveRoute.Count == 1) {
                        isDrawingMoveRoute = false;
                        gameManager.game.windMoveRoute.Clear();
                        UpdateValidPositions(route[route.Count - 1], deleting: true);
                        //isDrawingMoveRoute = true;
                    }
                    else if (gameManager.game.windMoveRoute.Count == 0) {
                        isDrawingMoveRoute = false;
                    }

                    gameManager.StartSim();

                }

                isDrawingCompleted = gameManager.game.windMoveRoute.Count == route.Count;

                /*if(isDrawingCompleted)
                    StartSim(skipOnSimStarted: true);

                */

                return;
            }
            else if (!isDrawingMoveRoute) { // Input.GetMouseButton(0)
                // Drawing wind route
                //Vector3 cursorPos = cursor.pos;



                if (Input.GetMouseButtonDown(0))
                    UpdateValidPositions(route[route.Count - 1]);
                else if (Input.GetMouseButtonDown(1) && route.Count >= 2)
                    UpdateValidPositions(route[route.Count - 1], deleting: true);
                else if (Input.GetMouseButtonUp(2)) {
                    // Deletes all route tiles and cancels drawing route
                    CancelDrawing();
                    /*CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
                    cancelDrawing.Execute();
                    Debug.LogWarning("should cancel drawing");
                    isDrawingCompleted = false;
                    turnCount = route.Count;*/
                    return;
                }

                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    UpdateValidPositions(cursorPos, none: true);

                if (cursorPos == route[route.Count - 1]) return;



                if (Input.GetMouseButton(0)) { // validPos.Contains(cursorPos)

                    Vector3 newPos = cursorPos;
                    float distance = 99999999f;
                    if (!validPos.Contains(cursorPos) && (cursorPos - route[route.Count - 1]).magnitude > 1) {
                        foreach (var item in validPos) {
                            float distance2 = (cursorPos - item).magnitude;
                            Vector3 validPosDir = (item - route[route.Count - 1]).normalized;
                            Vector3 cursorDir = (cursorPos - route[route.Count - 1]).normalized;
                            float angle = Vector3.Angle(cursorDir, validPosDir);

                            if (distance2 < distance && !isDrawingCompleted && item != route[route.Count - 1] && angle < 90) {
                                distance = distance2;
                                newPos = item;
                            }
                        }
                    }

                    bool canLoop = route.Count == 4 && route[0] == newPos;

                    /*if (route.Count >=2 && newPos == route[route.Count - 2])
                    {
                        // Deletes the last position of the route
                        isLooping = false;
                        isDrawingCompleted = route.Count - 1 >= curWindSource.defWindSP ? true : false;
                        routeManager.DeletePos(route.Count - 1);
                        DeletePosition deletePosition = new DeletePosition(curWindSource, routeManager, route[route.Count - 1]);

                        curWindSource.oldCommands.Add(deletePosition);
                        ////curWindSource.RemovePosition(route.Count - 1);
                        curWindSource.UpdateWindSP(route.Count);

                        route.RemoveAt(route.Count - 1);
                        //if (route.Count >= 2)
                        UpdateValidPositions(route[route.Count - 1]); //route[route.Count - 2]
                    }
                    else */
                    if ((route.Count < gameManager.game.curWindSource.defWindSP | canLoop) && validPos.Contains(newPos)) {

                        gameManager.game.isLooping = canLoop;

                        // Adds new position to the route
                        AddRoutePosition addNewPos = new AddRoutePosition(gameManager.game.curWindSource, gameManager.routeManager, this, newPos, route.Count, gameManager.game.isLooping);
                        addNewPos.Execute();
                        gameManager.game.curWindSource.oldCommands.Add(addNewPos);


                        //gameManager.state = GameState.Running;
                        //gameManager.isSimulating = true;
                        gameManager.StartSim(onTurnComplete: () => UpdateValidPositions(cursorPos));



                        if (route.Count >= gameManager.game.curWindSource.defWindSP) //Checks if the route is completed
                        {


                            // Set the drawing as completed but player still can draw if looping is possible
                            isDrawingCompleted = true;
                            gameManager.turnCount = gameManager.game.curWindSource.defWindSP;

                            // Starts drawing movement route for the looped wind
                            if (gameManager.game.isLooping) {
                                isDrawingCompleted = false;
                                StartDrawingMoveRoute startDrawingMoveRoute = new StartDrawingMoveRoute(arrowController, cursorPos);
                                startDrawingMoveRoute.Execute();
                                //oldCommands.Add(startDrawingMoveRoute);
                                //startDrawingMoveRoute.turnID = gameManager.turnID + 1;
                                //undoTimes.Add(turnID + 1);
                                /*isDrawingMoveRoute = true;
                                arrowController.transform.gameObject.SetActive(true);
                                arrowController.AddPos(cursorPos);
                                windMoveRoute.Add(cursorPos);*/
                            }

                            UpdateValidPositions(route[route.Count - 1]);
                            return;
                        }
                        else {

                            isDrawingMoveRoute = false;
                            UpdateValidPositions(route[route.Count - 1]);
                            isDrawingCompleted = false;
                            arrowController.transform.gameObject.SetActive(false);
                        }
                    }
                }
                else if (Input.GetMouseButton(1)) { //route.Count >= 2 && cursorPos == route[route.Count - 2]
                    if (route.Count >= 2 && validPos.Contains(cursorPos)) { //cursorPos == route[route.Count - 2]
                        // Deletes the last position of the route
                        if (route.Count == 2) {
                            new CancelRouteDrawing(gameManager.game.curWindSource, gameManager.routeManager, this, route).Execute();

                        }
                        else {
                            gameManager.game.isLooping = false;
                            isDrawingCompleted = route.Count - 1 >= gameManager.game.curWindSource.defWindSP ? true : false;
                            //gameManager.routeManager.DeletePos(gameManager.routeManager.route.Count - 1); //route.Count - 1

                            DeletePosition deletePosition = new DeletePosition(gameManager.game.curWindSource, gameManager.routeManager, route[route.Count - 1]);
                            deletePosition.Execute();
                            gameManager.routeManager.DrawRoute(gameManager.game.route);


                            gameManager.game.curWindSource.oldCommands.Add(deletePosition);
                            ////curWindSource.RemovePosition(route.Count - 1);

                            //if (route.Count >= 2)
                            UpdateValidPositions(gameManager.game.route[gameManager.game.route.Count - 1], deleting: true); //route[route.Count - 2]


                            gameManager.StartSim();


                            //gameManager.state = GameState.Running;
                            //gameManager.isSimulating = true;
                            // 
                            /*gameManager.UndoMultiStep();

                            if (simCor != null) {
                                StopCoroutine(simCor);
                            }

                            // Simulate turns
                            simCor = StartCoroutine(gameManager.Simulate2(route.Count));
                            */
                            // Simulate turns
                            //gameManager.SimulateTurns(route.Count);

                            //gameManager.state = GameState.DrawingRoute;
                            //gameManager.isSimulating = false;

                        }
                    }
                }



            }


            if (isDrawingCompleted) {
                //gameManager.UndoMultiStep();
                return;
            } 

            //gameManager.turnCount = route.Count;


        }
    }




    public void UpdateValidPositions(Vector3 centerPos, bool setAllValid = false, bool deleting = false, bool none = false) {

        validPos.Clear();
        foreach (GameObject sprite in validPositionSprites) {
            sprite.SetActive(false);
        }
        foreach (GameObject sprite in validRemovePositionSprites) {
            sprite.SetActive(false);
        }

        bool mayLoop = (!deleting && isDrawingCompleted && !gameManager.game.isLooping && route.Count == 4 &&
            (this.route[0] - centerPos).magnitude == 1) ? true : false;

        if (!deleting && ( //(!mayLoop && isDrawingCompleted && !isDrawingMoveRoute) ||
             gameManager.game.windMoveRoute.Count >= gameManager.game.curWindSource.defWindSP + 1)) return;


        /*if(!isDrawingMoveRoute && route.Count >= 2)
        {
            validPos.Add(route[route.Count - 2]);
        }*/

        if (none) {
            validPos.Clear();
        }
        else if (mayLoop) {
            validPos.Add(route[0]);
        }

        else if (deleting) {
            if (!isDrawingMoveRoute && route.Count >= 2) {
                validPos.Add(route[route.Count - 2]);
            }
            if (isDrawingMoveRoute && gameManager.game.windMoveRoute.Count >= 2) {
                validPos.Remove(route[route.Count - 2]);
                validPos.Add(gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 2]);
            }
        }
        else if (!mayLoop && isDrawingCompleted && !isDrawingMoveRoute) {

        }
        else if (setAllValid) {
            validPos.Remove(route[route.Count - 2]);
            foreach (Vector3 neighborVector in neighborVectors) {
                Vector3 origin = centerPos + neighborVector;
                validPos.Add(origin);
            }
        }
        else if (gameManager.game.isLooping) {
            /*Debug.Log("should try adding calid pos for looped");
            foreach (Vector3 neighborVector in neighborVectors) {
                Physics2D.simulationMode = SimulationMode2D.Script;

                Vector3 offsetAmount = Vector3.zero;

                if(windMoveRoute.Count >= 2) {
                    offsetAmount = windMoveRoute[windMoveRoute.Count - 1] + windMoveRoute[0];
                }

                Vector3 origin = offsetAmount + neighborVector;
                wind.col.offset = origin;
                Physics2D.Simulate(Time.fixedDeltaTime);

                if (!Physics2D.IsTouchingLayers(wind.col, LayerMask.GetMask("Wall"))) {
                    Debug.Log("should add valid pos for looped, pos:" + origin);

                    validPos.Add(centerPos + neighborVector);
                }
            }*/

            foreach (Vector3 neighborVector in neighborVectors) {
                bool isTouchingWall = false;
                Vector3 offset = Vector3.zero;
                if (gameManager.game.windMoveRoute.Count >= 2) {
                    offset = gameManager.game.windMoveRoute[gameManager.game.windMoveRoute.Count - 1] - gameManager.game.windMoveRoute[0];
                }

                foreach (var item in route) {
                    //Debug.Log("offset: " + offset);
                    Vector3 origin = item + neighborVector + offset;
                    //RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, distance: 1f, layerMask: LayerMask.GetMask("Wall", "WindCutter"));
                    Vector2Int index = GridManager.Instance.PosToGridIndex(origin);
                    GameObject obj = GridManager.grid[index.x, index.y].obj;

                    if (obj != null && obj.layer == 8) {
                        isTouchingWall = true;
                        break;
                    }
                }

                if (!isTouchingWall) {
                    validPos.Add(centerPos + neighborVector);

                }

            }
        }
        else {
            foreach (Vector3 neighborVector in neighborVectors) {
                Vector3 origin = centerPos + neighborVector;
                /*RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, distance: 1f, layerMask: LayerMask.GetMask("Wall", "WindCutter"));

                if (!hit && !route.Contains(origin)) //
                    validPos.Add(origin);*/
                
                if (route.Contains(origin))
                    continue;

                Vector2Int index = GridManager.Instance.PosToGridIndex(origin);
                GameObject obj = GridManager.grid[index.x, index.y].obj;
                Debug.Log("index: " + index + ", obj: " + obj);



                if(obj == null || (obj.layer != 8))
                {
                    validPos.Add(origin);
                }

            }
        }

        for (int i = 0; i < validPos.Count; i++) {
            if (deleting || (!isDrawingMoveRoute && route.Count >= 2 && validPos[i] == route[route.Count - 2])) {
                validRemovePositionSprites[0].SetActive(true);
                validRemovePositionSprites[0].transform.position = validPos[i];
                continue;
            }

            validPositionSprites[i].SetActive(true);
            validPositionSprites[i].transform.position = validPos[i];
            if (isDrawingMoveRoute || mayLoop)
                validPositionSprites[i].GetComponent<SpriteRenderer>().color = validPosAlternateColor;
            else
                validPositionSprites[i].GetComponent<SpriteRenderer>().color = Color.white;

        }
    }

    public void CancelDrawing() {
        if (isDrawingMoveRoute) {
            arrowController.lr.positionCount = 1;
            arrowController.transform.gameObject.SetActive(false);
            gameManager.game.windMoveRoute.RemoveRange(0, gameManager.game.windMoveRoute.Count);
            //gameManager.UpdateValidPositions(pos, none : true);
            isDrawingMoveRoute = false;
            //isDrawingCompleted = true;

            Debug.LogWarning("should return drawing route");

            // Deletes the last position of the route
            gameManager.game.isLooping = false;
        }

        CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(gameManager.game.curWindSource, gameManager.routeManager, this, route);
        cancelDrawing.Execute();
    }


    public void AddActionToCurSimTurn(Command action) {
        curSimTurn.actions.Add(action);
    }


}
