using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using DG.Tweening;

public enum GameState {
    Waiting,
    DrawingRoute,
    Running,
}

public class GameManager : MonoBehaviour
{
    public struct WindRoute{
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
    public List<WindRoute> windRoutes = new List<WindRoute>();

    public RouteManager routeManager;
    public Cursor cursor;
    public WindSourceController curWindSource;

    public List<Vector3> route = new List<Vector3>();
    public int cutLenght;

    public List<MoveTo> emptyDestinationMoves = new List<MoveTo>();
    public List<MoveTo> momentumTransferMoves = new List<MoveTo>();             // object at the  not moving or moving opposite direction
    public List<MoveTo> obstacleAtDestinationMoves = new List<MoveTo>();        // wall or obstacle

    public List<Command> oldCommands = new List<Command>();                     // Stores gameplay related commands to undo them 
    public List<ObjectDestination> destinations = new List<ObjectDestination>();// Stores all destinations in the level to check for level completion

    public delegate void OnTurnStartDelegate(List<Vector3> route);
    public event OnTurnStartDelegate OnTurnStart1;
    public event OnTurnStartDelegate OnTurnStart2;

    public delegate void OnTurnEndDelegate();
    public event OnTurnEndDelegate OnTurnEnd;

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

    private SetRoute previousRoute;
    private GameState _state;
    [HideInInspector]
    public GameState state
    {
        get { return _state; }
        set
        {
            if (OnStateChange != null && value != _state) //
            {
                OnStateChange(_state, value);
            }
            _state = value; 
        }
    }

    public float turnDur;
    private float t = 0;
    
    private int _turnCount;
    public int turnCount{
        get { return _turnCount; }
        set{
            // This makes sure turn count is never larger than power of the wind source
            _turnCount = (curWindSource && value > curWindSource.defWindSP) ? curWindSource.defWindSP : value; 

            if (OnTurnCountChange != null)
                OnTurnCountChange(_turnCount);
        }
    }

    public bool isLooping = false;
    public bool isFirstTurn = true;
    private bool _isDrawingCompleted;
    public bool isDrawingCompleted { get { return _isDrawingCompleted; } 
        set {
            _isDrawingCompleted = value;
            if(OnDrawingCompleted != null)
            {
                OnDrawingCompleted(value);
            }
        }
    }
    private int defTurnCount = 0;

    public static GameManager instance = null;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        instance.routeManager = FindObjectOfType<RouteManager>();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        cursor = FindObjectOfType<Cursor>();
        turnCount = 0;
        state = GameState.Waiting;
    }

    void Update()
    {
        if (state == GameState.Running)
        {
            if (turnCount <= 0){ // All turns end
                state = GameState.Waiting;
                CheckForLevelComplete();
                return;
            }

            // Star new turn
            t += Time.deltaTime;
            if (t >= turnDur){
                turnCount--;
                t = 0;

                if (defTurnCount - turnCount == 1){
                    isFirstTurn = true;
                }
                else{
                    isFirstTurn = false;

                    // Gets all movement reservs
                    if (OnTurnStart1 != null)
                    {
                        OnTurnStart1(route);
                    }

                    // Object which reserved movement will find their negihboring objects
                    if (OnTurnStart2 != null)
                    {
                        OnTurnStart2(route);
                    }
                }

                for (int i = 0; i < obstacleAtDestinationMoves.Count; i++)
                {
                    obstacleAtDestinationMoves[i].ChainFailedMove();
                }

                for (int i = 0; i < momentumTransferMoves.Count; i++)
                {
                    
                    momentumTransferMoves[i].ChainMomentumTransfer(emptyDestinationMoves);
                }

                for (int i = 0; i < emptyDestinationMoves.Count; i++)
                {
                    List<MoveTo> sameDestinationMoves = new List<MoveTo>();
                    for(int j = 0; j < emptyDestinationMoves.Count; j++)
                    {
                        if(emptyDestinationMoves[i].to == emptyDestinationMoves[j].to)
                        {
                            sameDestinationMoves.Add(emptyDestinationMoves[j]);
                        }
                    }

                    sameDestinationMoves = GetMoveWithHighestPriority(sameDestinationMoves, Vector3.zero);
                    if (emptyDestinationMoves[i] == sameDestinationMoves[0])
                    {
                        emptyDestinationMoves[i].ChainMove();
                    }
                    else
                    {
                        emptyDestinationMoves[i].ChainFailedMove();
                    }
                }

                emptyDestinationMoves.Clear();
                momentumTransferMoves.Clear();
                obstacleAtDestinationMoves.Clear();

                Invoke("OnTurnEndEvent", turnDur - (turnDur/20f));
            }
        }
        else if(state == GameState.DrawingRoute) // drawing route for wind
        {
            if (isDrawingCompleted && Input.GetKeyUp(KeyCode.Space))
            {
                StartWindBlow();
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 cursorPos = cursor.cursorPos;

                if(cursorPos == route[route.Count - 1]) return;

                if (routeManager.validPos.Contains(cursorPos))
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
                        routeManager.UpdateValidPositions(cursorPos);
                        return;
                    }
                    else
                    {
                        isDrawingCompleted = false;
                    }

                }
                else if (route.Count >= 2 && cursorPos == route[route.Count - 2])
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
                }
                
            }
            else if (Input.GetMouseButton(1))
            {
                // Deletes all route tiles and cancels drawing route
                CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
                cancelDrawing.Execute();
                
                isDrawingCompleted = false;
            }

            turnCount = route.Count;
        }
        else if( state == GameState.Waiting)
        {
            // Starts route drawing if player clicks on a wind source
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast setup
                Vector3 origin = cursor.cursorPos;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 1f, LayerMask.GetMask("WindSource"));

                if (hit)
                {
                    StartDrawing(hit.transform.GetComponent<WindSourceController>());

                    //WindRoute newWindRoute = new WindRoute(new List<Vector3>(), hit.transform.GetComponent<WindSourceController>());
                    //windRoutes.Add(newWindRoute);
                }
            }
        }
    }

    private void OnTurnEndEvent()
    {
        if(OnTurnEnd != null)
        {
            OnTurnEnd();
        }
    }

    public void StartWindBlow()
    {
        //routeManager.UpdateValidPositions(cursorPos);
        if (previousRoute != null)
        {
            previousRoute.nextWS = curWindSource;
        }

        curWindSource.isUsed = true;
        SetRoute setRoute = new SetRoute(curWindSource, routeManager, route);
        setRoute.executionTime = Time.time;
        SetRoute(route);
        routeManager.WindTransition(route, isLooping);
        oldCommands.Add(setRoute);
        state = GameState.Running;

        previousRoute = setRoute;
        isDrawingCompleted = false;
    }

    public void WaitATurn()
    {
        turnCount = 1;
        state = GameState.Running;
    }

    // Cuts wind route from given index. This is happen when a wall appears on the wind route. ie: when door closses
    public void CutWindRoute(int index)
    {
        int count = route.Count;
        cutLenght = count - index;
        route.RemoveRange(index, cutLenght);

        routeManager.DeleteTiles();
        routeManager.DrawWindRoute(route);
    }

    // Restores wind route with straight line after cut position. (does not restore to the route before cut)
    public void RestoreWindRoute(Vector3 cutPos)
    {
        if (cutLenght == 0) return;


        for (int i = 0; i < cutLenght; i++)
        {
            Vector3 lastPos = route[route.Count - 1];

            route.Add(cutPos);
            routeManager.DeleteTiles();
            routeManager.DrawWindRoute(route); // TODO: this is very unoptimized. find an optimization
            cutPos += cutPos - lastPos;
        }
    }
    
    public List<MoveTo> GetMoveWithHighestPriority(List<MoveTo> moves, Vector3 relativeChainMoveDir) 
    {
        if (moves.Count == 0) return null;

        int highest = 0;

        for (int i = 1; i < moves.Count ; i++)
        {
            if (relativeChainMoveDir != Vector3.zero)
            {
                if (moves[i].dir == relativeChainMoveDir)
                {
                    highest = i;
                    break;
                }
                else if (moves[highest].dir == relativeChainMoveDir)
                {
                    break;
                }
            }

            if ( moves[i].indexInWind > moves[highest].indexInWind)
            {
                highest = i;
            }
        }

        MoveTo temp = moves[0];
        moves[0] = moves[highest];
        moves[highest] = temp;

        return moves;
    }
    public void StartDrawing(WindSourceController windSource)
    {
        route.Clear();
        this.curWindSource = windSource;
        routeManager.route.Clear();
        AddRoutePosition addNewPos = new AddRoutePosition(curWindSource, routeManager, cursor.cursorPos, 0);
        addNewPos.Execute();
        //addNewPos.executionTime = Time.time;
        curWindSource.oldCommands.Add(addNewPos);
        //curWindSource.AddPosition(cursor.cursorPos);
        //oldCommands.Add(addNewPos);

        //route.Add(cursor.cursorPos);
        routeManager.StartDrawing(cursor.cursorPos);
        state = GameState.DrawingRoute;
        turnCount = route.Count;
    }

    public void SetRoute(List<Vector3> route)
    {
        //this.route = route;
        turnCount = route.Count;
        defTurnCount = turnCount;
        //tilesCleared = false;
        state  = GameState.Running;
        isFirstTurn = true;
        routeManager.ClearValidPositions();
        // Get move reserv
        if (OnTurnStart1 != null)
        {
            OnTurnStart1(route);
        }

        // find neighbors
        if (OnTurnStart2 != null)
        {
            OnTurnStart2(route);
        }
    }

    /*public void GetWindRoute(Vector3 pos)
    {

    }*/

    public void CancelTurns()
    {
        turnCount = 0;
        state = GameState.DrawingRoute;
        isDrawingCompleted = false;
        emptyDestinationMoves.Clear();
        momentumTransferMoves.Clear();
        obstacleAtDestinationMoves.Clear();
    }

    public void CheckForLevelComplete()
    {
        if (destinations.Count == 0) return;
        foreach(ObjectDestination destination in destinations)
        {
            if(destination.objMC == null)
            {
                Debug.LogWarning("destination is not satisfied: " + destination.gameObject.name);
                return; 
            }
        }
        Debug.LogWarning("LEVEL COMPLETED");
        
        destinations.Clear();

        if(OnLevelComplete != null)
        {
            OnLevelComplete();
        }
    }

    public void Undo()
    {
        if (oldCommands.Count == 0) return;

        CancelTurns();
        CancelInvoke();
        float executionTime = oldCommands[oldCommands.Count -1].executionTime; ;
        for(int i = oldCommands.Count -1; i >= 0; i--)
        {
            if (executionTime == oldCommands[i].executionTime)
            {
                oldCommands[i].Undo();
                oldCommands.Remove(oldCommands[i]);
            }

        }

        if (OnUndo != null)
        {
            OnUndo();
        }
    }

    public void UndoDrawing()
    {
        if (curWindSource == null) return;
        
        if (curWindSource.oldCommands.Count == 0) return;

        Command command = curWindSource.oldCommands[curWindSource.oldCommands.Count - 1];

        command.Undo();

        if (curWindSource == null) return;
        curWindSource.oldCommands.Remove(command);

    }



}
