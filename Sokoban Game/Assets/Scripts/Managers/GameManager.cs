using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;


public class GameManager : MonoBehaviour{
    #region Variables
    public Game game = new Game();
    public Game simGame = new Game();
    public Game curGame = null;

    //public WindRouteDeformInfo curWindDeformInfo = new WindRouteDeformInfo(null, -1, 0);

    public List<Command> oldCommands = new List<Command>();

    public List<ObjectDestination> destinations = new List<ObjectDestination>();// Stores all destinations in the level to check for level completion
    public List<WindSourceController> windSources = new List<WindSourceController>();

    public DeathFogController deathFog;
    [HideInInspector] public RouteManager routeManager;
    [HideInInspector] public Cursor cursor;
    public DrawingController drawingController;
    public Tilemap dustTileMap;
    public ArrowController arrowController;
    public GameObject[] validPositionSprites;
    public GameObject[] validRemovePositionSprites;
    public Color validPosAlternateColor;
    public SettingsDataHolder settingsHolder;
    public Wind wind;

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

    private List<float> gameSpeeds = new List<float> { 0.5f, 1f, 2f };

    Coroutine simCor;

    private Vector3 cursorPos;
    //public Vector3 windMoveDir;
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

    //public int turnID = 0;
    
    private int _turnCount;
    public int turnCount{
        get { return _turnCount; }
        set{
            _turnCount = value;

            if (OnTurnCountChange != null)
                OnTurnCountChange(_turnCount);
        }
    }


    [SerializeField] private bool _previewOutcome = true;
    public bool previewOutcome {
        get { return _previewOutcome; }
        set {
            _previewOutcome = value;

            OnPreviewOutcomeChanged?.Invoke(value);
        }
    }

    public bool isWaiting = false;
    public int defTurnCount = 0;
    public bool pauseOnTurnEnd = false;
    private bool fastForward = false;
    #endregion Variables

    #region Events

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

    public delegate void OnSpeedChangedDelegate(float gameSpeed);
    public event OnSpeedChangedDelegate OnSpeedChanged;

    public delegate void OnPlannedSpeedChangedDelegate(float gameSpeed);
    public event OnPlannedSpeedChangedDelegate OnPlannedSpeedChanged;

    public delegate void OnSimStartedDelegate();
    public event OnSimStartedDelegate OnSimStarted;

    public delegate void OnSimEndedDelegate();
    public event OnSimEndedDelegate OnSimEnded;

    public delegate void OnSimComletedDelegate(bool value);
    public event OnSimComletedDelegate OnSimComleted;

    public delegate void OnPreviewOutcomeChangedDelegate(bool value);
    public static event OnPreviewOutcomeChangedDelegate OnPreviewOutcomeChanged;

    #endregion Events

    public static GameManager instance = null;

    void Awake(){
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;

        instance.routeManager = FindObjectOfType<RouteManager>();
        //DontDestroyOnLoad(this.gameObject);


        game.wind = wind;
        curGame = game;

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
        cursor = Cursor.instance;
        turnCount = 0;
        state = GameState.Paused;
        previewOutcome = _previewOutcome; // makes sure value given in inspecter is triggered as well

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


            if (t >= defTurnDur) {
                t = 0;

                curGame.PlayTurn(turnCount, defTurnCount, defTurnDur - Time.deltaTime);

                Invoke("HandleEndTurn", defTurnDur - Time.deltaTime);

                turnCount--;
                return;
            }
            
        }
        else if(state == GameState.DrawingRoute){ // drawing route for wind
            if (gameSpeed != 1)
                SetGameSpeed(1);
            //Debug.Log("here0");

            if (game.route.Count == 0){
                state = GameState.Paused;
                return;
            }

            if (drawingController.isDrawingCompleted && Input.GetKeyUp(KeyCode.Space)) {
                StartWindBlow();
            }
            turnCount = game.route.Count;
        }
        else if( state == GameState.Paused){
            if (gameSpeed != 1)
                SetGameSpeed(1);

            // Starts route drawing if player clicks on a wind source
            if (Input.GetMouseButtonDown(0)){
                // Raycast setup
                Vector3 origin = cursor.pos;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 1f, LayerMask.GetMask("WindSource"));
                //GameObject obj = GridManager.Instance.GetCell(origin).obj;
                if (hit){
                    StartDrawing(hit.transform.GetComponent<WindSourceController>());
                    //WindRoute newWindRoute = new WindRoute(new List<Vector3>(), hit.transform.GetComponent<WindSourceController>());
                    //windRoutes.Add(newWindRoute);
                }
            }
        }

    }
    private void HandleEndTurn() {
        curGame.HandleTurnEnd(0f);

        if ((curGame.route.Count == 0)) {
            Debug.Log("should check for level complete");

            if (!CheckForUnusedWindSources() && !curGame.isSimulation) {
                CheckForLevelComplete();
            }
        }

        if (turnCount <= 0 && !isWaiting) { // All turns end 
            
            //simGame.UndoMultiStep();
            state = GameState.Paused;
            t = 0;
        }

        if (pauseOnTurnEnd) {
            pauseOnTurnEnd = false;
            Pause();
        }

    }
    public void InvokeOnSimStartedEvent() {
        OnSimStarted?.Invoke();
    }

    public void InvokeOnSimEndedEvent() {
        OnSimEnded?.Invoke();
    }

    public void StartSim(bool skipOnSimStarted = false, Action onTurnComplete = null, Action onComplete = null) {
        if (!previewOutcome) return;

        StopSimCor();

        simGame.UndoMultiStep();

        // Simulate turns
        simCor = StartCoroutine(Simulate(curGame.route.Count, skipOnSimStarted, onComplete));

        InvokeOnSimStartedEvent();
    } 

    private void StopSimCor() {

        if (simCor != null) {
            StopCoroutine(simCor);
            InvokeOnSimEndedEvent();
            //gameManager.simGame.multiStepCommands.Clear(); // clear simulation commands
        }
    }

    public IEnumerator Simulate(int turnCount, bool skipOnSimStarted = false, Action onTurnComplete = null, Action onComplete = null) {
        //simGame.UndoMultiStep();

        //simGame.ResetData();

        state = GameState.DrawingRoute;
        //state = GameState.Simulating;


        simGame = new Game();

        simGame.route.AddRange(game.route);
        simGame.windMoveRoute.AddRange(game.windMoveRoute);
        simGame.windPath.AddRange(game.windPath);
        simGame.curWindSource = game.curWindSource;
        simGame.wind = wind;
        simGame.isLooping = game.isLooping;
        simGame.isSimulation = true;
        curGame = simGame;




        curGame.windTurnsCommand = new MultipleCommand();
        curGame.multiStepCommands.Add(curGame.windTurnsCommand);

        /*if (!skipOnSimStarted)
            InvokeOnSimStartedEvent();
        */
        int initTurnCount = turnCount;
        
        while (turnCount > 0) {

            yield return new WaitForFixedUpdate(); // new WaitForFixedUpdate();//

            //Debug.Log("play turn: " + turnCount);

            curGame.PlayTurn(turnCount, initTurnCount, Time.fixedDeltaTime);
            turnCount--;

            yield return new WaitForFixedUpdate();

            //Debug.Log("handle end turn");

            HandleEndTurn();


            yield return new WaitForFixedUpdate();

            onTurnComplete?.Invoke();
        }



        yield return new WaitForFixedUpdate(); //  WaitForFixedUpdate();

        //OnSimComleted?.Invoke(drawingController.isDrawingCompleted);
        OnSimEnded?.Invoke();

        curGame = game;

        onComplete?.Invoke();

        //state = GameState.DrawingRoute;
    }


    public void AddActionToCurTurn(Command action) {
        /*if (curGame.isSimulation)
            simGame.curTurn.actions.Add(action);
        
        else*/
            curGame.curTurn.actions.Add(action);
    }

    public void StartWindBlow(){
        OnSimComleted?.Invoke(drawingController.isDrawingCompleted);
        simGame.multiStepCommands.Clear(); // clear simulation commands

        StopSimCor();

        curGame = game;

        //routeManager.UpdateValidPositions(cursorPos);
        if (previousRoute != null){
            previousRoute.nextWS = curGame.curWindSource;
        }
        //realTurnDur = defTurnDur / gameSpeed;
        drawingController.UpdateValidPositions(Vector3.zero, none: true);
        curGame.curWindSource.isUsed = true;
        SetRoute setRoute = new SetRoute(instance, curGame.curWindSource, routeManager, curGame.route, curGame.isLooping);
        setRoute.executionTime = Time.time;
        //setRoute.turnID = turnID +1;
        SetRoute(game.route);
        routeManager.WindTransition(curGame.route, curGame.isLooping);
        state = GameState.Running;
        wind.StartWind(defTurnDur + 1.5f, curGame.isLooping, defTurnDur);

        previousRoute = setRoute;
        drawingController.isDrawingCompleted = false;
        ///isDrawingMoveRoute = false;
        isWaiting = false;        
        pauseOnTurnEnd = false;
        t = -0.5f;

        curGame.windTurnsCommand = new MultipleCommand();
        curGame.singleStepCommands.Add(setRoute);
        curGame.multiStepCommands.Add(curGame.windTurnsCommand);
        curGame.windTurnsCommand.commands.Add(setRoute);

        if(OnPlay != null)
        {
            OnPlay();
        }
    }
    public void SetRoute(List<Vector3> route){
        if (curGame.curWindSource.isAlternative) {
            turnCount = game.windPath.Count;

        }
        else {
            turnCount = route.Count;

        }
        defTurnCount = turnCount;

        //state = GameState.Running;
        curGame.isFirstTurn = true;
        routeManager.ClearValidPositions();
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

  
        if(curGame.route.Count == 0) {
            StartWaiting();
        }
        else {
            state = GameState.Running;
        }
    
    }
    public void StartWaiting(){
        t = defTurnDur - Time.deltaTime * 2;

        // Cancels wind route drawing
        if (curGame.route.Count >= 1)
        {
            CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curGame.curWindSource, routeManager, drawingController, curGame.route);
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
    
    public void StartDrawing(WindSourceController windSource){
        curGame.route.Clear();
        this.curGame.curWindSource = windSource;
        routeManager.route.Clear();
        AddRoutePosition addNewPos = new AddRoutePosition(curGame.curWindSource, routeManager, drawingController, cursor.pos, 0);
        addNewPos.Execute();
        curGame.curWindSource.oldCommands.Add(addNewPos);

        routeManager.StartDrawing(cursor.pos);

        if (curGame.curWindSource.isAlternative) {
            arrowController.transform.gameObject.SetActive(true);
            arrowController.AddPos(cursorPos);
            curGame.windPath.Add(cursorPos);
        }

        drawingController.UpdateValidPositions(cursor.pos);
        state = GameState.DrawingRoute;
        drawingController.isDrawingMoveRoute = false;
        turnCount = game.route.Count;

        //InvokeOnSimStarted();
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
        drawingController.isDrawingCompleted = false;
        curGame.emptyDestinationMoves.Clear();
        curGame.momentumTransferMoves.Clear();
        curGame.obstacleAtDestinationMoves.Clear();
    }

    private void ResetVariables(){
        windSources.Clear();
        destinations.Clear();
        isWaiting = false;
        state = GameState.Paused;
        drawingController.isDrawingMoveRoute = false;

        PreviewManager.instance.previewControllers.Clear();
    }

    public void CheckForLevelComplete(){
        Debug.LogWarning("Checking for level comp");
        if(deathFog != null && deathFog.dustTiles.Count == 0) {
            //InvokeOnLevelComple();
            Invoke("InvokeOnLevelComple", 0.2f);
            return;
        }


        if (destinations.Count == 0) return;
        foreach(ObjectDestination destination in destinations){
            if(destination.objMC == null)    return; 
        }
        Debug.LogWarning("LEVEL COMPLETED");

        InvokeOnLevelComple();
    }

    public void InvokeOnLevelComple() {
        if (OnLevelComplete != null) {
            oldCommands.Clear();
            curGame.curWindSource = null;
            OnLevelComplete();
        }
    }

    // Checks for unused wind sources. 
    // Returns false if all wind sources used, returns true if unused wind source exists
    private bool CheckForUnusedWindSources(){
        Debug.Log("should check for unused wind sources");

        foreach (WindSourceController windSource in windSources){
            if (!windSource.isUsed)
                return true;
        }

        return false;
    }

    /*public void UndoMultiStep() {
        curGame.UndoMultiStep();
    }

    public void UndoSingleStep() {
        curGame.UndoSingleStep();
    }*/
    
    public void Restart() {
        if(LevelManager.instance == null) {
            Debug.Log("level manager is null");
            return;
        }

        int multiStepCommandsCount = curGame.multiStepCommands.Count;

        for (int i = 0; i < multiStepCommandsCount; i++) {
            game.UndoMultiStep();

        }
        drawingController.CancelDrawing();

    }

    public void TogglePreviewOutcome() {
        previewOutcome = !previewOutcome;
    }
}
public enum GameState {
    Paused,
    DrawingRoute,
    Running,
    Simulating
}