using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class GameManager : MonoBehaviour{
    /*public struct WindRoute{
        public List<Vector3> route;
        public WindSourceController windSource;
        public bool isCompleted;

        public WindRoute(List<Vector3> route, WindSourceController windSource, bool isCompleted = false)
        {
            this.route = route;
            this.windSource = windSource;
            this.isCompleted = isCompleted;
        }
    }

    public List<WindRoute> windRoutes = new List<WindRoute>();*/

    public List<Vector3> route = new List<Vector3>();
    public List<Vector3> windMoveRoute = new List<Vector3>();
    //public List<Door> _routeCuttingRequests = new List<Door>();
    //public IDictionary<int, Door> routeCuttingRequests = new Dictionary<int, Door>();
    public WindRouteDeformInfo windRouteDeformInfo = new WindRouteDeformInfo(null, -1, 0);
    //[HideInInspector] public int cutLenght;
    //[HideInInspector] public int windRouteCutIndex = -1;

    public List<MoveTo> emptyDestinationMoves = new List<MoveTo>();
    public List<MoveTo> momentumTransferMoves = new List<MoveTo>();             // object at the  not moving or moving opposite direction
    public List<MoveTo> obstacleAtDestinationMoves = new List<MoveTo>();        // wall or obstacle

    public List<Command> oldCommands = new List<Command>();
    private List<float> undoTimes = new List<float>();// Stores gameplay related commands to undo them 

    public List<ObjectDestination> destinations = new List<ObjectDestination>();// Stores all destinations in the level to check for level completion
    public List<WindSourceController> windSources = new List<WindSourceController>();

    [HideInInspector] public RouteManager routeManager;
    [HideInInspector] public Cursor cursor;

    public ArrowController arrowController;
    public GameObject[] validPositionSprites;
    public GameObject[] validRemovePositionSprites;
    public Color validPosAlternateColor;
    public WindSourceController curWindSource;
    //public ParticleSystem cutEffect;
    public SettingsDataHolder settingsHolder;
    public Wind wind;

    private SetRoute previousRoute;
    private GameState _state;
    [HideInInspector] public GameState state{
        get { return _state; }
        set{
            if (OnStateChange != null && value != _state){
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
            if (plannedGameSpeed != gameSpeed)
                SetGameSpeed(plannedGameSpeed);

            // Start new turn
            //t += Time.deltaTime* gameSpeed;
            t += Time.deltaTime;

            if (t >= defTurnDur){
                turnCount--;
                t = 0;
                turnID++;
                //realTurnDur = defTurnDur / gameSpeed;
                
                emptyDestinationMoves.Clear();
                momentumTransferMoves.Clear();
                obstacleAtDestinationMoves.Clear();

                isFirstTurn = defTurnCount - turnCount == 1 ? true : false;

                bool isWindRouteMoving = false;
                if(defTurnCount > 1 && turnCount > 0 && windMoveRoute.Count == route.Count && !isWaiting){
                    windMoveDir = windMoveRoute[defTurnCount-turnCount] - windMoveRoute[defTurnCount-turnCount-1];
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

                if (turnCount == 0){
                    routeManager.ClearTiles();
                    //cutEffect.gameObject.SetActive(false);
                    wind.EndWind(defTurnDur);
                }


                for (int i = 0; i < momentumTransferMoves.Count; i++){
                    // Hit
                    momentumTransferMoves[i].Hit(emptyDestinationMoves);
                    //momentumTransferMoves[i].ChainMomentumTransfer(emptyDestinationMoves);
                }
                if(OnHitsChecked != null){
                    OnHitsChecked(emptyDestinationMoves);
                }

                for (int i = 0; i < obstacleAtDestinationMoves.Count; i++)
                {
                    obstacleAtDestinationMoves[i].ChainFailedMove();
                }

                for (int i = 0; i < emptyDestinationMoves.Count; i++){
                    List<MoveTo> sameDestinationMoves = new List<MoveTo>();

                    for(int j = 0; j < emptyDestinationMoves.Count; j++){
                        if(emptyDestinationMoves[i].to == emptyDestinationMoves[j].to){
                            sameDestinationMoves.Add(emptyDestinationMoves[j]);
                        }
                    }

                    sameDestinationMoves = GetMoveWithHighestPriority(sameDestinationMoves, Vector3.zero);
                    if (emptyDestinationMoves[i] == sameDestinationMoves[0]){
                        emptyDestinationMoves[i].ChainMove();
                    }
                    else{
                        emptyDestinationMoves[i].ChainFailedMove();
                    }
                }

                if (isWaiting && (emptyDestinationMoves.Count > 0 || momentumTransferMoves.Count > 0 || obstacleAtDestinationMoves.Count > 0))
                {
                    undoTimes.Add(turnID);
                }

                if ((isWaiting && turnCount == 0)){
                    if (!CheckForUnusedWindSources()){
                        CheckForLevelComplete();
                    }
                    else{
                        isWaiting = true;
                        turnCount = 1;
                        defTurnCount = turnCount;
                    }
                }
                else if (turnCount == 0 && (emptyDestinationMoves.Count > 0 ) && !CheckForUnusedWindSources()){ // || momentumTransferMoves.Count > 0
                    route.Clear();
                    //routeManager.ClearTiles(); //GameState.Running, GameState.Paused
                    isWaiting = true;
                    turnCount = 10;
                    defTurnCount = turnCount;
                }
                else if (turnCount == 0 && !CheckForUnusedWindSources()){
                    route.Clear();
                    isWaiting = true;
                    turnCount = 1;
                    defTurnCount = turnCount;
                }
            
                Invoke("OnTurnEndEvent", defTurnDur - (defTurnDur / 15));
            
                if(isWindRouteMoving){
                    // Moves looped wind 

                    for (int i = 0; i < route.Count; i++){
                        route[i] += windMoveDir;
                    }
                    // TODO: kill this tween on undo
                    arrowController.origin.transform.DOMove(arrowController.origin.transform.position + windMoveDir, defTurnDur).SetEase(Ease.Linear);
                    //routeManager.transform.DOMove(routeManager.transform.position + windMoveDir, defTurnDur).SetEase(Ease.Linear);
                    wind.transform.DOMove(wind.transform.position + windMoveDir, defTurnDur).SetEase(Ease.Linear);
                }

                return;
            }
        }
        else if(state == GameState.DrawingRoute){ // drawing route for wind
            if (route.Count == 0)
            {
                state = GameState.Paused;
                return;
            }

            if (isDrawingCompleted && Input.GetKeyUp(KeyCode.Space)){
                StartWindBlow();
            }
            else if(isDrawingMoveRoute && !isHoveringUI){ 
                // Drawing movement route for the looped winds
                //Vector3 cursorPos2 = cursor.pos;

                if (Input.GetMouseButtonDown(0))
                    UpdateValidPositions(windMoveRoute[windMoveRoute.Count-1], setAllValid : true);
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
                    return;
                }

                if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    UpdateValidPositions(cursorPos, none: true);

                if(windMoveRoute.Count == 0 | 
                    cursorPos == windMoveRoute[windMoveRoute.Count - 1] | 
                    !validPos.Contains(cursorPos))    return;

                if(Input.GetMouseButton(0)){
                    // Adds new position
                    arrowController.transform.gameObject.SetActive(true);
                    arrowController.AddPos(cursorPos);
                    windMoveRoute.Add(cursorPos);
                    UpdateValidPositions(windMoveRoute[windMoveRoute.Count-1], setAllValid : true);
                    return;                
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

                if(cursorPos == route[route.Count - 1] || !validPos.Contains(cursorPos)) return;

                if (Input.GetMouseButton(0)){ // validPos.Contains(cursorPos)

                    if(route.Count >=2 && cursorPos == route[route.Count - 2])
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
                    else
                    {
                        isLooping = route.Count + 1 > 4 && route[0] == cursorPos ? true : false;

                        // Adds new position to the route
                        AddRoutePosition addNewPos = new AddRoutePosition(curWindSource, routeManager, cursorPos, route.Count, isLooping);
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

                            UpdateValidPositions(cursorPos, isLooping);
                            return;
                        }
                        else
                        {
                            isDrawingMoveRoute = false;
                            UpdateValidPositions(cursorPos);
                            isDrawingCompleted = false;
                            arrowController.transform.gameObject.SetActive(false);
                        }
                    }


                }
                else if (Input.GetMouseButton(1)){ //route.Count >= 2 && cursorPos == route[route.Count - 2]
                    if (route.Count >= 2 && cursorPos == route[route.Count - 2])
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
                        UpdateValidPositions(route[route.Count -1], deleting : true); //route[route.Count - 2]
                    }
                }
            }

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
        if(OnTurnEnd != null)
            OnTurnEnd();

        CheckForWindDeform();

        if (windRouteDeformInfo.cutIndex >= 0){
            
            CutWindRoute(windRouteDeformInfo.cutIndex);

            windRouteDeformInfo.door.isWindRouteInterrupted = true;
            windRouteDeformInfo.cutIndex = -1;
            windRouteDeformInfo.door = null;
        }
        else if( windRouteDeformInfo.cutLenght > 0 && windRouteDeformInfo.restore){
            RestoreWindRoute(windRouteDeformInfo.restoreDir);
            windRouteDeformInfo.restore = false;
        }

        if (turnCount <= 0){ // All turns end 
            state = GameState.Paused;
            route.Clear();
            windMoveRoute.Clear();
            arrowController.Clear();
            routeManager.transform.position = Vector3.zero;
            t = 0;
            return;
        }
    }
    public void CheckForWindDeform(){
        if (OnWindRouteGenerated != null && route.Count > 0){
            OnWindRouteGenerated(route);
        }
    }

    public void CutWindRoute(int index){
        Vector3 cutPos = route[index];
        CutWindRoute cutWindRoute = new CutWindRoute(routeManager, route, index);
        cutWindRoute.Execute();

        // Redraws the wind route
        List<Vector3> route2 = new List<Vector3>();
        route2.AddRange(route);
        route2.Add(cutPos);
        routeManager.DeleteTiles();

        if (route.Count == 0) return;

        wind.DrawWind();

        //routeManager.DrawWindRoute(route2, ignoreLastPos: true);
    }

    public void RestoreWindRoute(Vector3 restoreDir){
        //cutEffect.gameObject.SetActive(false);
        RestoreWindRoute restoreWindRoute = new RestoreWindRoute(routeManager, route, windRouteDeformInfo.cutIndex, windRouteDeformInfo.cutLenght);
        restoreWindRoute.Execute();
    }

    public void StartWindBlow(){
        //routeManager.UpdateValidPositions(cursorPos);
        if (previousRoute != null){
            previousRoute.nextWS = curWindSource;
        }
        //realTurnDur = defTurnDur / gameSpeed;
        curWindSource.isUsed = true;
        SetRoute setRoute = new SetRoute(instance, curWindSource, routeManager, route, isLooping);
        setRoute.executionTime = Time.time;
        setRoute.turnID = turnID +1;
        undoTimes.Add(turnID + 1);
        SetRoute(route);
        routeManager.WindTransition(route, isLooping);
        wind.StartWind(defTurnDur , defTurnDur * (2/3));
        oldCommands.Add(setRoute);
        state = GameState.Running;

        previousRoute = setRoute;
        isDrawingCompleted = false;
        isDrawingMoveRoute = false;

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
        turnCount = 1;
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

        bool mayLoop = (!deleting && isDrawingCompleted && !isLooping && route.Count>= 4 && 
            (this.route[0] - centerPos).magnitude == 1 ) ? true : false;
        
        if (!deleting && ( //(!mayLoop && isDrawingCompleted && !isDrawingMoveRoute) ||
             windMoveRoute.Count >= curWindSource.defWindSP + 1 ) ) return;


        if(!isDrawingMoveRoute && route.Count >= 2)
        {
            validPos.Add(route[route.Count - 2]);
        }

        if (none){
            validPos.Clear();
        }
        else if (mayLoop){
            validPos.Add(route[0]);
            //validPos.Add(route[route.Count - 2]);
        }
        else if(!mayLoop && isDrawingCompleted && !isDrawingMoveRoute)
        {

        }
        else if (deleting)
        {
            if(isDrawingMoveRoute && windMoveRoute.Count >= 2)
            {
                validPos.Remove(route[route.Count - 2]);
                validPos.Add(windMoveRoute[windMoveRoute.Count - 2]);
            }
        }
        else if(setAllValid){
            validPos.Remove(route[route.Count - 2]);
            foreach (Vector3 neighborVector in neighborVectors){
                Vector3 origin = centerPos + neighborVector;
                validPos.Add(origin);
            }
        }else{
            foreach (Vector3 neighborVector in neighborVectors){
                Vector3 origin = centerPos + neighborVector;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, distance: 1f, layerMask: LayerMask.GetMask("Wall"));

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

        plannedGameSpeed = value;

        if (state != GameState.Running) return;

        SetGameSpeed(value);
    }

    public void SetGameSpeed(float value) {
        gameSpeed = value;

        Time.timeScale = value;

        if (OnSpeedChanged != null)
            OnSpeedChanged(gameSpeed);
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

    public void UpdatePlannedGameSpeed(){
        for (int i = gameSpeeds.Count -1; i >= 0; i--){
            if (gameSpeed >= gameSpeeds[i]){
                curGameSpeedIndex = i;
                break;
            }
        }

        plannedGameSpeed = gameSpeeds[curGameSpeedIndex];
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