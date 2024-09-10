using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;


public class GameManager : MonoBehaviour{
    #region Variables
    public Dictionary<Vector3, Vector3> routeWithDir = new Dictionary<Vector3, Vector3>();
    public List<Vector3> route = new List<Vector3>();
    public List<Vector3> windMoveRoute = new List<Vector3>();

    public List<WindCutRequest> windCutRequests = new List<WindCutRequest>();
    public WindCutRequest curWindCutRequest = null;
    public List<WindRestoreRequest> windRestoreRequests = new List<WindRestoreRequest>();


    public WindRestoreRequest curWindRestoreRequest = null;

    public WindRouteDeformInfo curWindDeformInfo = new WindRouteDeformInfo(null, -1, 0);

    public List<MoveTo> emptyDestinationMoves = new List<MoveTo>();
    public List<MoveTo> momentumTransferMoves = new List<MoveTo>();             // object at the  not moving or moving opposite direction
    public List<MoveTo> obstacleAtDestinationMoves = new List<MoveTo>();        // wall or obstacle

    public List<Command> oldCommands = new List<Command>();
    public List<Command> singleStepCommands = new List<Command>();
    public List<MultipleCommand> multiStepCommands = new List<MultipleCommand>();


    private List<float> undoTimes = new List<float>();// Stores gameplay related commands to undo them 

    public List<ObjectDestination> destinations = new List<ObjectDestination>();// Stores all destinations in the level to check for level completion
    public List<WindSourceController> windSources = new List<WindSourceController>();

    [HideInInspector] public RouteManager routeManager;
    [HideInInspector] public Cursor cursor;

    public Tilemap dustTileMap;
    public ArrowController arrowController;
    public GameObject[] validPositionSprites;
    public GameObject[] validRemovePositionSprites;
    public Color validPosAlternateColor;
    public WindSourceController curWindSource;
    //public ParticleSystem cutEffect;
    public SettingsDataHolder settingsHolder;
    public Wind wind;
    [HideInInspector] public Turn curTurn;
    [HideInInspector] public MultipleCommand windTurnsCommand;

    private SetRoute previousRoute;
    private GameState _state;
    [HideInInspector] public GameState state{
        get { return _state; }
        set{
            if (OnStateChange != null && value != _state) { //
                OnStateChange(_state, value);
            }
            _state = value; 
        }
    }

    public List<Vector3> validPos = new List<Vector3>();
    private Vector3[] neighborVectors = new Vector3[4] {Vector3.up, Vector3.down, Vector3.right, Vector3.left};

    private List<float> gameSpeeds = new List<float> { 0.5f, 1f, 2f };
    private Vector3 cursorPos;
    public Vector3 windMoveDir;
    public float gameSpeed = 1;

    private float _plannedGameSpeed;
    public float plannedGameSpeed {
        get { return _plannedGameSpeed; }
        set {
            _plannedGameSpeed = value;
            OnPlannedSpeedChanged?.Invoke(value);
        }
    }
    private int curGameSpeedIndex = 1;
    public float defTurnDur = 0.5f;
    private float t = 0;

    public int turnID = 0;
    
    private int _turnCount;
    public int turnCount{
        get { return _turnCount; }
        set{
            _turnCount = value;

            if (OnTurnCountChange != null)
                OnTurnCountChange(_turnCount);
        }
    }
    
    public bool isLooping = false;
    public bool isWindRouteMoving = false;
    public bool isFirstTurn = true;
    public bool isWaiting = false;
    public bool isHoveringUI = false;
    public bool isDrawingMoveRoute = false;
    private bool _isDrawingCompleted;
    public bool isDrawingCompleted { get { return _isDrawingCompleted; } 
        set {
            _isDrawingCompleted = value;
            if(OnDrawingCompleted != null){
                OnDrawingCompleted(value);
            }
        }
    }
    public int defTurnCount = 0;
    private bool pauseOnTurnEnd = false;
    private bool fastForward = false;
    #endregion Variables

    #region Events
    public delegate void OnTurnStartDelegate(List<Vector3> route);
    public event OnTurnStartDelegate OnTurnStart1;
    public event OnTurnStartDelegate OnTurnStart2;

    public delegate void OnTurnEndDelegate();
    public event OnTurnEndDelegate OnTurnEnd;

    public delegate void OnPlayDelegate();
    public event OnPlayDelegate OnPlay;

    public delegate void OnLevelCompleteDelegate();
    public event OnLevelCompleteDelegate OnLevelComplete;

    public delegate void OnTurnCountChangeDelegate(int turnCount);
    public event OnTurnCountChangeDelegate OnTurnCountChange;

    public delegate void OnStateChangeDelegate(GameState from, GameState to);
    public event OnStateChangeDelegate OnStateChange;

    public delegate void OnUndoDelegate();
    public event OnUndoDelegate OnUndo;

    public delegate void OnDrawingCompletedDelegate(bool value);
    public event OnDrawingCompletedDelegate OnDrawingCompleted; // On drawing completed but not wind blowing started

    public delegate void OnWindRouteGeneratedDelegate(List<Vector3> route);
    public event OnWindRouteGeneratedDelegate OnWindRouteGenerated;

    public delegate void OnSpeedChangedDelegate(float gameSpeed);
    public event OnSpeedChangedDelegate OnSpeedChanged;

    public delegate void OnPlannedSpeedChangedDelegate(float gameSpeed);
    public event OnPlannedSpeedChangedDelegate OnPlannedSpeedChanged;

    public delegate void OnHitsCheckedDelegate(List<MoveTo> emptyDestintionMoves);
    public event OnHitsCheckedDelegate OnHitsChecked;
    #endregion Events

    public static GameManager instance = null;

    void Awake(){
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

        instance.routeManager = FindObjectOfType<RouteManager>();
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable(){
        SceneLoader.OnSceneLoad += ResetVariables;
        SettingsManager.OnGameSpeedDataLoaded += UpdateGameSpeed;
    }

    private void OnDisable(){
        SceneLoader.OnSceneLoad -= ResetVariables;
        SettingsManager.OnGameSpeedDataLoaded -= UpdateGameSpeed;
    }

    private void Start(){
        //cursor = FindObjectOfType<Cursor>();
        cursor = Cursor.instance;
        turnCount = 0;
        state = GameState.Paused;
        //realTurnDur = defTurnDur / gameSpeed;
    }
    
    private void LateUpdate() {
        if (cursor == null) return;

        cursorPos = cursor.pos;
    }

    void Update(){
        if (state == GameState.Running){
            if (plannedGameSpeed != gameSpeed && !fastForward) {
                fastForward = false;
                SetGameSpeed(plannedGameSpeed);
            }

            // Start new turn
            t += Time.deltaTime;

            if (t >= defTurnDur){
                curTurn = new Turn(turnCount, turnID);

                t = 0;
                turnID++;
                //realTurnDur = defTurnDur / gameSpeed;

                emptyDestinationMoves.Clear();
                momentumTransferMoves.Clear();
                obstacleAtDestinationMoves.Clear();
                windCutRequests.Clear();
                windRestoreRequests.Clear();

                isFirstTurn = defTurnCount - turnCount == 0;

                isWindRouteMoving = false;
                //Debug.Log("turn count: " + turnCount + ", def turn count: " + defTurnCount);
                if(turnCount > 1 && windMoveRoute.Count > 0){

                    int index = defTurnCount - turnCount;
                    //index = index == windMoveRoute.Count ? index - 1 : index;
                    windMoveDir = windMoveRoute[index +1] - windMoveRoute[index];
                    
                    isWindRouteMoving = true;
                }

                // Gets all movement reservs
                if (OnTurnStart1 != null){
                    OnTurnStart1(route);
                }
                // Object which reserved movement will find their negihboring objects
                if (OnTurnStart2 != null){
                    OnTurnStart2(route);
                }

                for (int i = 0; i < momentumTransferMoves.Count; i++){
                    // Hit
                    //Debug.Log("should transfer momentum");
                    momentumTransferMoves[i].Hit(emptyDestinationMoves);
                }
                if(OnHitsChecked != null){
                    OnHitsChecked(emptyDestinationMoves);
                }

                for (int i = 0; i < obstacleAtDestinationMoves.Count; i++){
                    //Debug.Log("should failed chain move 1");
                    obstacleAtDestinationMoves[i].ChainFailedMove();
                }

                for (int i = 0; i < emptyDestinationMoves.Count; i++){
                    List<MoveTo> sameDestinationMoves = new List<MoveTo>();
                    for(int j = 0; j < emptyDestinationMoves.Count; j++){
                        if(emptyDestinationMoves[i].to == emptyDestinationMoves[j].to){
                            //Debug.Log("SAME DEST: " + emptyDestinationMoves[i].obj.transform.name + ", " + emptyDestinationMoves[j].obj.transform.name);
                            sameDestinationMoves.Add(emptyDestinationMoves[j]);
                        }
                    }

                    sameDestinationMoves = GetMoveWithHighestPriority(sameDestinationMoves, Vector3.zero);
                    if (emptyDestinationMoves[i] == sameDestinationMoves[0]){
                        //Debug.Log("should chain move");
                        emptyDestinationMoves[i].ChainMove();
                    }
                    else{
                        //Debug.Log("should failed chain move: " + emptyDestinationMoves[i].obj.transform.name);

                        emptyDestinationMoves[i].ChainFailedMove();
                    }
                }

                /*if (isWaiting && (emptyDestinationMoves.Count > 0 || momentumTransferMoves.Count > 0 || obstacleAtDestinationMoves.Count > 0))
                {
                    undoTimes.Add(turnID);
                }*/

                Invoke("OnTurnEndEvent", defTurnDur - Time.deltaTime);
            
                if(isWindRouteMoving){
                    // TODO: kill this tween on undo
                    MoveWindRoute moveWindRoute = new MoveWindRoute(wind, windMoveDir, defTurnDur);
                    moveWindRoute.Execute();
                    AddActionToCurTurn(moveWindRoute);
                }

                if (turnCount == 1) {
                    routeManager.ClearTiles();
                    //cutEffect.gameObject.SetActive(false);
                    //wind.EndWind(defTurnDur);
                    if (route.Count > 0) {
                        Debug.Log("should END TURN  ");
                        EndWind endWind = new EndWind(this, wind, arrowController);
                        endWind.Execute();
                        AddActionToCurTurn(endWind);
                    }
                }

                if (curTurn.actions.Count > 0)
                    singleStepCommands.Add(curTurn);

                if (windTurnsCommand != null)
                    windTurnsCommand.commands.Add(curTurn);

                return;
            }
        }
        else if(state == GameState.DrawingRoute){ // drawing route for wind
            if (gameSpeed != 1)
                SetGameSpeed(1);
            //Debug.Log("here0");

            if (route.Count == 0){
                state = GameState.Paused;
                return;
            }

            if (isDrawingCompleted && Input.GetKeyUp(KeyCode.Space)){
                StartWindBlow();
            }
            else if(isDrawingMoveRoute && !isHoveringUI){
                // Drawing movement route for the looped winds
                //Vector3 cursorPos2 = cursor.pos;
                //Debug.Log("here1");
                if (Input.GetMouseButtonDown(0))
                    UpdateValidPositions(windMoveRoute[windMoveRoute.Count-1]);
                else if(Input.GetMouseButtonDown(1)){
                    if(windMoveRoute.Count >= 2)
                        UpdateValidPositions(windMoveRoute[windMoveRoute.Count-1], deleting : true);
                    else if (windMoveRoute.Count == 1){
                        //isDrawingMoveRoute = false;
                        UpdateValidPositions(route[route.Count-1], deleting : true);
                        //isDrawingMoveRoute = true;
                    }
                }
                else if (Input.GetMouseButtonUp(2)){
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
                    isDrawingCompleted = route.Count - 1 >= curWindSource.defWindSP ? true : false;
                    routeManager.DeletePos(route.Count - 1);
                    DeletePosition deletePosition = new DeletePosition(curWindSource, routeManager, route[route.Count - 1]);

                    curWindSource.oldCommands.Add(deletePosition);
                    ////curWindSource.RemovePosition(route.Count - 1);
                    curWindSource.UpdateWindSP(route.Count);

                    route.RemoveAt(route.Count - 1);
                    //if (route.Count >= 2)
                    UpdateValidPositions(route[route.Count - 1]);
                    //Debug.Log("here2");

                    return;
                }

                if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    UpdateValidPositions(cursorPos, none: true);

                //Debug.Log("here3");

                if (windMoveRoute.Count == 0 | 
                    cursorPos == windMoveRoute[windMoveRoute.Count - 1] | 
                    !validPos.Contains(cursorPos))    return;

                //Debug.Log("here4");

                if (Input.GetMouseButton(0)){
                    // Adds new position
                    arrowController.transform.gameObject.SetActive(true);
                    arrowController.AddPos(cursorPos);
                    windMoveRoute.Add(cursorPos);
                    UpdateValidPositions(windMoveRoute[windMoveRoute.Count-1]);
                }
                else if (Input.GetMouseButton(1)){
                    // Remove last position
                    arrowController.RemoveLastPos();
                    windMoveRoute.RemoveAt(windMoveRoute.Count -1);
                    if(windMoveRoute.Count>=2)
                        UpdateValidPositions(windMoveRoute[windMoveRoute.Count-2], deleting : true);
                    else if(windMoveRoute.Count == 1){
                        isDrawingMoveRoute = false;
                        windMoveRoute.Clear();
                        UpdateValidPositions(route[route.Count-1], deleting : true);
                        //isDrawingMoveRoute = true;
                    }
                    else if(windMoveRoute.Count == 0){
                        isDrawingMoveRoute = false;
                    }
                }

                isDrawingCompleted = windMoveRoute.Count == route.Count;

                return;
            }
            else if (!isDrawingMoveRoute && !isHoveringUI){ // Input.GetMouseButton(0)
                // Drawing wind route
                //Vector3 cursorPos = cursor.pos;

                if (Input.GetMouseButtonDown(0))
                    UpdateValidPositions(route[route.Count-1]);
                else if(Input.GetMouseButtonDown(1) && route.Count >= 2)
                    UpdateValidPositions(route[route.Count-1], deleting : true);
                else if (Input.GetMouseButtonUp(2)){
                    // Deletes all route tiles and cancels drawing route
                    CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
                    cancelDrawing.Execute();
                    Debug.LogWarning("should cancel drawing");
                    isDrawingCompleted = false;
                    turnCount = route.Count;
                    return;
                }

                if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    UpdateValidPositions(cursorPos, none: true);

                if(cursorPos == route[route.Count - 1]) return;



                if (Input.GetMouseButton(0)){ // validPos.Contains(cursorPos)

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

                    bool canLoop = route.Count == 4 && route[0] == newPos ? true : false;

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
                    else */if ((route.Count < curWindSource.defWindSP | canLoop) && validPos.Contains(newPos)) {

                        isLooping = canLoop;

                        // Adds new position to the route
                        AddRoutePosition addNewPos = new AddRoutePosition(curWindSource, routeManager, newPos, route.Count, isLooping);
                        addNewPos.Execute();
                        curWindSource.oldCommands.Add(addNewPos);

                        if (route.Count >= curWindSource.defWindSP) //Checks if the route is completed
                        {
                            // Set the drawing as completed but player still can draw if looping is possible
                            isDrawingCompleted = true;
                            turnCount = curWindSource.defWindSP;

                            // Starts drawing movement route for the looped wind
                            if (isLooping)
                            {
                                isDrawingCompleted = false;
                                StartDrawingMoveRoute startDrawingMoveRoute = new StartDrawingMoveRoute(arrowController, cursorPos);
                                startDrawingMoveRoute.Execute();
                                //oldCommands.Add(startDrawingMoveRoute);
                                startDrawingMoveRoute.turnID = turnID + 1;
                                //undoTimes.Add(turnID + 1);
                                /*isDrawingMoveRoute = true;
                                arrowController.transform.gameObject.SetActive(true);
                                arrowController.AddPos(cursorPos);
                                windMoveRoute.Add(cursorPos);*/
                            }

                            UpdateValidPositions(route[route.Count - 1]);
                            return;
                        }
                        else
                        {
                            isDrawingMoveRoute = false;
                            UpdateValidPositions(route[route.Count - 1]);
                            isDrawingCompleted = false;
                            arrowController.transform.gameObject.SetActive(false);
                        }
                    }
                }
                else if (Input.GetMouseButton(1)){ //route.Count >= 2 && cursorPos == route[route.Count - 2]
                    if (route.Count >= 2 && cursorPos == route[route.Count - 2])
                    {
                        // Deletes the last position of the route
                        if(route.Count == 2) {
                            new CancelRouteDrawing(curWindSource, routeManager, route).Execute();
                            
                        }
                        else {
                            isLooping = false;
                            isDrawingCompleted = route.Count - 1 >= curWindSource.defWindSP ? true : false;
                            routeManager.DeletePos(route.Count - 1);
                            DeletePosition deletePosition = new DeletePosition(curWindSource, routeManager, route[route.Count - 1]);
                            deletePosition.Execute();
                            curWindSource.oldCommands.Add(deletePosition);
                            ////curWindSource.RemovePosition(route.Count - 1);



                            //if (route.Count >= 2)
                            UpdateValidPositions(route[route.Count - 1], deleting: true); //route[route.Count - 2]
                        }
                    }
                }
            }
            if (isDrawingCompleted) return;

            turnCount = route.Count;
        }
        else if( state == GameState.Paused){
            if (gameSpeed != 1)
                SetGameSpeed(1);

            // Starts route drawing if player clicks on a wind source
            if (Input.GetMouseButtonDown(0)){
                // Raycast setup
                Vector3 origin = cursor.pos;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 1f, LayerMask.GetMask("WindSource"));

                if (hit){
                    StartDrawing(hit.transform.GetComponent<WindSourceController>());
                    //WindRoute newWindRoute = new WindRoute(new List<Vector3>(), hit.transform.GetComponent<WindSourceController>());
                    //windRoutes.Add(newWindRoute);
                }
            }
        }
    }

    private void OnTurnEndEvent(){
        Debug.Log("TURN");
        turnCount--;

        if (OnTurnEnd != null)
            OnTurnEnd();

        CheckForWindDeform(route);

        if ((isWaiting && route.Count == 0)) { // turnCount == 0
            if (!CheckForUnusedWindSources()) {
                CheckForLevelComplete();
            }
            else {
                isWaiting = true;
                turnCount = 1;
                defTurnCount = turnCount;
            }
        }
        else if (turnCount == 0 && (emptyDestinationMoves.Count > 0) && !CheckForUnusedWindSources()) { // || momentumTransferMoves.Count > 0
            Debug.Log("should END TURN  ");
            isWaiting = true;
            turnCount = 10;
            defTurnCount = turnCount;
        }
        else if (turnCount == 00 && !CheckForUnusedWindSources()) {
            Debug.Log("should END TURN  ");
            isWaiting = true;
            turnCount = 1;
            defTurnCount = turnCount;
        }


        if (turnCount <= 0){ // All turns end 
            state = GameState.Paused;

            t = 0;
        }



        if (pauseOnTurnEnd) {
            pauseOnTurnEnd = false;
            Pause();
        }
        else {

        }
    }
    public void CheckForWindDeform(List<Vector3> route) {
        if (state != GameState.Running) return;

        if (OnWindRouteGenerated != null && route.Count > 0){
            // Gets wind deforms requests
            OnWindRouteGenerated(route);
        }
        
        if(windCutRequests.Count > 0 | windRestoreRequests.Count > 0) {
            Debug.Log("should try deform, cut req: " + windCutRequests.Count + ", restore req: " + windRestoreRequests.Count);

            ChangeWindRoute changeWindRoute = new ChangeWindRoute(this, windCutRequests, windRestoreRequests);
            changeWindRoute.Execute();

            AddActionToCurTurn(changeWindRoute);

            wind.DrawWind();
        }
    }

    public void AddActionToCurTurn(Command action) {
        curTurn.actions.Add(action);
    }

    public void StartWindBlow(){
        //routeManager.UpdateValidPositions(cursorPos);
        if (previousRoute != null){
            previousRoute.nextWS = curWindSource;
        }
        //realTurnDur = defTurnDur / gameSpeed;
        UpdateValidPositions(Vector3.zero, none: true);
        curWindSource.isUsed = true;
        SetRoute setRoute = new SetRoute(instance, curWindSource, routeManager, route, isLooping);
        setRoute.executionTime = Time.time;
        setRoute.turnID = turnID +1;
        undoTimes.Add(turnID + 1);
        SetRoute(route);
        routeManager.WindTransition(route, isLooping);
        wind.StartWind(defTurnDur + 1.5f , isLooping, defTurnDur); // * (2/3)
        oldCommands.Add(setRoute);
        state = GameState.Running;

        previousRoute = setRoute;
        isDrawingCompleted = false;
        isDrawingMoveRoute = false;
        isWaiting = false;        
        pauseOnTurnEnd = false;
        t = -0.5f;

        windTurnsCommand = new MultipleCommand();
        singleStepCommands.Add(setRoute);
        multiStepCommands.Add(windTurnsCommand);
        windTurnsCommand.commands.Add(setRoute);

        if(OnPlay != null)
        {
            OnPlay();
        }
    }
    public void SetRoute(List<Vector3> route){
        turnCount = route.Count;
        defTurnCount = turnCount;
        
        //state = GameState.Running;
        isFirstTurn = true;
        routeManager.ClearValidPositions();
    }
    public void PauseWhenTurnEnd() {
        //if (state == GameState.Paused | state == GameState.DrawingRoute) return;

        //fastForward = true;
        //SetGameSpeed(10);
        pauseOnTurnEnd = true;
        //state = GameState.Paused;
    }
    public void Pause() {
        //if (state == GameState.Paused | state == GameState.DrawingRoute) return;

        if (isWaiting) {
            StopWaiting();
        }
        else {
            state = GameState.Paused;
        }
    }
    public void Play() {
        Debug.Log("should try starting play");


        if (state == GameState.Running) return;

        Debug.Log("should start playing");


        if (pauseOnTurnEnd)
            pauseOnTurnEnd = false;

  
        if(route.Count == 0) {
            StartWaiting();
        }
        else {
            state = GameState.Running;
        }
    
    }
    public void StartWaiting(){
        t = defTurnDur - Time.deltaTime * 2;

        // Cancels wind route drawing
        if (route.Count >= 1)
        {
            CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
            cancelDrawing.Execute();
        }
        state = GameState.Running;
        isWaiting = true;
        turnCount = 0;
        defTurnCount = turnCount;
    }

    public void StopWaiting(){
        turnCount = 0;
        state = GameState.Paused;
        isWaiting = false;
    }
    
    public List<MoveTo> GetMoveWithHighestPriority(List<MoveTo> moves, Vector3 relativeChainMoveDir) {
        if (moves.Count == 0) return null;

        int highest = 0;

        for (int i = 1; i < moves.Count ; i++){
            if (relativeChainMoveDir != Vector3.zero){
                if (moves[i].dir == relativeChainMoveDir){
                    highest = i;
                    break;
                }
                else if (moves[highest].dir == relativeChainMoveDir){
                    break;
                }
            }

            if ( moves[i].indexInWind > moves[highest].indexInWind)
                highest = i;
        }

        MoveTo temp = moves[0];
        moves[0] = moves[highest];
        moves[highest] = temp;

        return moves;
    }
    public void StartDrawing(WindSourceController windSource){
        route.Clear();
        this.curWindSource = windSource;
        routeManager.route.Clear();
        AddRoutePosition addNewPos = new AddRoutePosition(curWindSource, routeManager, cursor.pos, 0);
        addNewPos.Execute();
        curWindSource.oldCommands.Add(addNewPos);

        routeManager.StartDrawing(cursor.pos);
        UpdateValidPositions(cursor.pos);
        state = GameState.DrawingRoute;
        turnCount = route.Count;
    }

    public void UpdateValidPositions(Vector3 centerPos, bool setAllValid = false, bool deleting = false, bool none = false){
  
        validPos.Clear();
        foreach(GameObject sprite in validPositionSprites){
            sprite.SetActive(false);
        }
        foreach (GameObject sprite in validRemovePositionSprites)
        {
            sprite.SetActive(false);
        }

        bool mayLoop = (!deleting && isDrawingCompleted && !isLooping && route.Count == 4 && 
            (this.route[0] - centerPos).magnitude == 1 ) ? true : false;
        
        if (!deleting && ( //(!mayLoop && isDrawingCompleted && !isDrawingMoveRoute) ||
             windMoveRoute.Count >= curWindSource.defWindSP +1) ) return;


        /*if(!isDrawingMoveRoute && route.Count >= 2)
        {
            validPos.Add(route[route.Count - 2]);
        }*/

        if (none){
            validPos.Clear();
        }
        else if (mayLoop){
            validPos.Add(route[0]);
        }

        else if (deleting)
        {
            if (!isDrawingMoveRoute && route.Count >= 2) {
                validPos.Add(route[route.Count - 2]);
            }
            if (isDrawingMoveRoute && windMoveRoute.Count >= 2)
            {
                validPos.Remove(route[route.Count - 2]);
                validPos.Add(windMoveRoute[windMoveRoute.Count - 2]);
            }
        }
        else if (!mayLoop && isDrawingCompleted && !isDrawingMoveRoute) {

        }
        else if(setAllValid){
            validPos.Remove(route[route.Count - 2]);
            foreach (Vector3 neighborVector in neighborVectors){
                Vector3 origin = centerPos + neighborVector;
                validPos.Add(origin);
            }
        }
        else if (isLooping) {
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
                if (windMoveRoute.Count >= 2) {
                    offset = windMoveRoute[windMoveRoute.Count - 1] - windMoveRoute[0];
                }

                foreach (var item in route) {
                    //Debug.Log("offset: " + offset);
                    Vector3 origin = item + neighborVector + offset;
                    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, distance: 1f, layerMask: LayerMask.GetMask("Wall", "WindCutter"));

                    if (hit) {
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
            foreach (Vector3 neighborVector in neighborVectors){
                Vector3 origin = centerPos + neighborVector;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, distance: 1f, layerMask: LayerMask.GetMask("Wall", "WindCutter"));

                if (!hit && !route.Contains(origin)) //
                    validPos.Add(origin);
            }
        }

        for (int i = 0; i < validPos.Count; i++){
            if (deleting || (!isDrawingMoveRoute && route.Count >= 2 && validPos[i] == route[route.Count - 2]) )
            {
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

    public void UpdateGameSpeed(float value) {
        if (value <= 0 || value > 10) return;

        //Debug.Log("should update planned game speed: " + value);
        plannedGameSpeed = value;
        UpdateCurGameSpeedIndex();

        

        if (state != GameState.Running) return;

        SetGameSpeed(value);
    }

    public void SetGameSpeed(float value) {
        gameSpeed = value;

        Time.timeScale = value;

        /*if (OnSpeedChanged != null)
            OnSpeedChanged(gameSpeed);*/
    }

    public void SetNextGameSpeed(bool bypassStateCheck = false){

        int nextIndex = curGameSpeedIndex;

        nextIndex = curGameSpeedIndex == gameSpeeds.Count - 1 ? 0 : curGameSpeedIndex + 1;
        curGameSpeedIndex = nextIndex;

        UpdateGameSpeed(gameSpeeds[curGameSpeedIndex]);
    }

    public float GetPreviousGameSpeed(){
        int index = curGameSpeedIndex == 0 ? gameSpeeds.Count - 1 : curGameSpeedIndex - 1;
        return gameSpeeds[index];
    }

    public void UpdateCurGameSpeedIndex(){
        
        for (int i = gameSpeeds.Count -1; i >= 0; i--){
            if (plannedGameSpeed >= gameSpeeds[i]){
                curGameSpeedIndex = i;
                break;
            }
        }
        //plannedGameSpeed = gameSpeeds[curGameSpeedIndex];
    }

    public void CancelTurns(){
        turnCount = 0;
        state = GameState.DrawingRoute;
        isDrawingCompleted = false;
        emptyDestinationMoves.Clear();
        momentumTransferMoves.Clear();
        obstacleAtDestinationMoves.Clear();
    }

    private void ResetVariables(){
        windSources.Clear();
        destinations.Clear();
        isWaiting = false;
        state = GameState.Paused;
    }

    public void CheckForLevelComplete(){
        //Debug.LogWarning("Checking for level comp");
        if (destinations.Count == 0) return;
        foreach(ObjectDestination destination in destinations){
            if(destination.objMC == null)    return; 
        }
        //Debug.LogWarning("LEVEL COMPLETED");

        if (OnLevelComplete != null){
            oldCommands.Clear();
            curWindSource = null;
            OnLevelComplete();
        }
    }

    // Checks for unused wind sources. 
    // Returns false if all wind sources used, returns true if unused wind source exists
    private bool CheckForUnusedWindSources(){
        foreach(WindSourceController windSource in windSources){
            if (!windSource.isUsed)
                return true;
        }

        return false;
    }

    public void UndoMultiStep() {
        if (multiStepCommands.Count == 0) return;

        /*if (route.Count >= 1) {
            CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
            cancelDrawing.Execute();
        }

        CancelTurns();
        CancelInvoke();
        */

        //PauseWhenTurnEnd();

        int index = multiStepCommands.Count - 1;
        MultipleCommand command = multiStepCommands[index];
        command.Undo();

        foreach (var item in command.commands) {
            if (singleStepCommands.Contains(item)) {
                singleStepCommands.Remove(item);
            }
        }

        multiStepCommands.Remove(command);

    }

    public void UndoSingleStep() {

        if (singleStepCommands.Count == 0) return;

        /*if (route.Count >= 1) {
            CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
            cancelDrawing.Execute();
        }

        CancelTurns();
        CancelInvoke();
        */
        PauseWhenTurnEnd();

        int index = singleStepCommands.Count - 1;
        Command command = singleStepCommands[index];
        command.Undo();

        MultipleCommand lastMultiStepCommand = multiStepCommands[multiStepCommands.Count - 1];
        if (lastMultiStepCommand.commands.Contains(command)) {
            multiStepCommands[multiStepCommands.Count - 1].commands.Remove(command);

            if (multiStepCommands[multiStepCommands.Count - 1].commands.Count == 0) {
                multiStepCommands.Remove(lastMultiStepCommand);
            }
        }

        singleStepCommands.Remove(command);
    }
    
    public void Restart() {
        if(LevelManager.instance == null) {
            Debug.Log("level manager is null");
            return;
        }

        string curLevel = LevelManager.instance.curLevel.name;
        Level level = LevelManager.instance.curLevel;
        MainUIManager mainUIManager = MainUIManager.instance;
        MainUIManager.TransitionProperty tp = (level.seen | level.state == Level.State.completed)
            ? mainUIManager.transitionProperty2 : mainUIManager.transitionProperty1;

        StartCoroutine(SceneLoader.LoadAsyncSceneWithName(level.debugName, tp.durationFH,
            preLoadCallBack: () => mainUIManager.SceneTranstionFH(tp),
            onCompleteCallBack: () => MainUIManager.instance.SceneTranstionSH(tp)));
    }
    public void Undo(){
        if (oldCommands.Count == 0 || undoTimes.Count == 0) return;

        if (route.Count >= 1){
            CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
            cancelDrawing.Execute();
        }

        CancelTurns();
        CancelInvoke();
        //cutEffect.gameObject.SetActive(false);

        float undoTime = undoTimes[undoTimes.Count - 1];
        undoTimes.RemoveAt(undoTimes.Count - 1);
        //Debug.LogWarning("undo count: " + oldCommands.Count);
        while (true){
            if (oldCommands.Count == 0) break;

            //Debug.LogWarning("undo null check: " + oldCommands[oldCommands.Count - 1]);
            float executionTime = oldCommands[oldCommands.Count - 1].turnID; //.executionTime
            if (executionTime < undoTime){
                break;
            }

            oldCommands[oldCommands.Count - 1].Undo();
            oldCommands.Remove(oldCommands[oldCommands.Count - 1]);

        }

        if (OnUndo != null){
            OnUndo();
        }
    }

    public void UndoDrawing(){
        if (curWindSource == null || curWindSource.oldCommands.Count == 0) return;

        Command command = curWindSource.oldCommands[curWindSource.oldCommands.Count - 1];
        command.Undo();

        if (curWindSource == null) return;

        curWindSource.oldCommands.Remove(command);
    }

}
public enum GameState {
    Paused,
    DrawingRoute,
    Running
}