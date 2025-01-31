using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Game {
    public List<Vector3> route = new List<Vector3>();
    public List<Vector3> windMoveRoute = new List<Vector3>();
    public List<Vector3> windPath = new List<Vector3>(); // used when route drawn with alternative windsource

    public List<WindCutRequest> windCutRequests = new List<WindCutRequest>();
    public WindCutRequest curWindCutRequest = null;
    public List<WindRestoreRequest> windRestoreRequests = new List<WindRestoreRequest>();

    public WindRestoreRequest curWindRestoreRequest = null;

    public List<MoveTo> emptyDestinationMoves = new List<MoveTo>();
    public List<MoveTo> momentumTransferMoves = new List<MoveTo>();             // object at the  not moving or moving opposite direction
    public List<MoveTo> obstacleAtDestinationMoves = new List<MoveTo>();        // wall or obstacle

    public List<Command> singleStepCommands = new List<Command>();
    public List<MultipleCommand> multiStepCommands = new List<MultipleCommand>();
    //public List<MultipleCommand> simMultiStepCommands = new List<MultipleCommand>();
    public WindRouteDeformInfo curWindDeformInfo = new WindRouteDeformInfo(null, -1, 0);


    [HideInInspector] public Turn curTurn;
    [HideInInspector] public MultipleCommand windTurnsCommand;

    public Wind wind;
    public WindSourceController curWindSource;

    public Vector3 windMoveDir;

    public bool isLooping = false;
    public bool isWindRouteMoving = false;
    public bool isFirstTurn = true;

    public bool isSimulation = false;

    public delegate void OnTurnStartDelegate(List<Vector3> route);
    public static event OnTurnStartDelegate OnTurnStart1;
    public static event OnTurnStartDelegate OnTurnStart2;

    public delegate void OnHitsCheckedDelegate(List<MoveTo> emptyDestintionMoves);
    public static event OnHitsCheckedDelegate OnHitsChecked;

    public delegate void OnTurnEndDelegate();
    public static event OnTurnEndDelegate OnTurnEnd;

    public delegate void OnWindRouteGeneratedDelegate(List<Vector3> route);
    public static event OnWindRouteGeneratedDelegate OnWindRouteGenerated;

    public delegate void OnRouteChangedDelegate(List<Vector3> route);
    public static event OnRouteChangedDelegate OnRouteChanged;

    public static event Action MovementCompleted;

    public void ResetData() {
        route.Clear();
        windMoveRoute.Clear();
        windPath.Clear();
        windCutRequests.Clear();
        curWindCutRequest = null;
        windRestoreRequests.Clear();
        curWindRestoreRequest = null;
        emptyDestinationMoves.Clear();
        momentumTransferMoves.Clear();
        obstacleAtDestinationMoves.Clear();
        singleStepCommands.Clear();
        multiStepCommands.Clear();

        curTurn = null;
        windTurnsCommand = null;
        curWindSource = null;
        wind = null;
        windMoveDir = Vector3.zero;
    }

    public void PlayTurn(int curturnCount, int defTurnCount, float turnDur, Action onComplete = null) {
        curTurn = new Turn(curturnCount); //, turnID

        emptyDestinationMoves.Clear();
        momentumTransferMoves.Clear();
        obstacleAtDestinationMoves.Clear();
        windCutRequests.Clear();
        windRestoreRequests.Clear();

        isFirstTurn = defTurnCount - curturnCount == 0;

        isWindRouteMoving = false;
        /**if (curturnCount > 1 && windMoveRoute.Count > 0) {

            int index = defTurnCount - curturnCount;
            windMoveDir = windMoveRoute[index + 1] - windMoveRoute[index];

            isWindRouteMoving = true;
        }*/


        if (curturnCount > 1 && windMoveRoute.Count > 1) {

            int index = defTurnCount - curturnCount;
            if (index >= windMoveRoute.Count - 1)
                windMoveDir = Vector3.zero;//  windMoveRoute[windMoveRoute.Count - 2] - windMoveRoute[windMoveRoute.Count - 1]; //
            else
                windMoveDir = windMoveRoute[index + 1] - windMoveRoute[index];

            isWindRouteMoving = true;
        }

        GridManager.Instance.InvokeGridChanged();

        // Gets all movement reservs
        if (OnTurnStart1 != null) {
            OnTurnStart1(route);
        }
        // Object which reserved movement will find their negihboring objects
        if (OnTurnStart2 != null) {
            OnTurnStart2(route);
        }

        for (int i = 0; i < momentumTransferMoves.Count; i++) {
            // Hit
            momentumTransferMoves[i].Hit(emptyDestinationMoves);
        }
        if (OnHitsChecked != null) {
            OnHitsChecked(emptyDestinationMoves);
        }

        for (int i = 0; i < obstacleAtDestinationMoves.Count; i++) {
            obstacleAtDestinationMoves[i].ChainFailedMove();
        }

        for (int i = 0; i < emptyDestinationMoves.Count; i++) {
            List<MoveTo> sameDestinationMoves = new List<MoveTo>();
            for (int j = 0; j < emptyDestinationMoves.Count; j++) {
                if (emptyDestinationMoves[i].to == emptyDestinationMoves[j].to) {
                    sameDestinationMoves.Add(emptyDestinationMoves[j]);
                }
            }

            sameDestinationMoves = GetMoveWithHighestPriority(sameDestinationMoves, Vector3.zero);
            if (emptyDestinationMoves[i] == sameDestinationMoves[0]) {
                emptyDestinationMoves[i].ChainMove();
            }
            else {
                emptyDestinationMoves[i].ChainFailedMove();
            }
        }

        if (isWindRouteMoving) {
            // TODO: kill this tween on undo
            MoveWindRoute moveWindRoute = new MoveWindRoute(wind, windMoveDir, turnDur);
            moveWindRoute.Execute();
            AddActionToCurTurn(moveWindRoute);
        }
        else if (curWindSource.isAlternative) {
            route.Clear();
            wind.EndWind(0f, false);
            int index = defTurnCount - curturnCount;
            int count = windPath.Count - index > 4 ? 4 : windPath.Count - index;
            route.AddRange(windPath.GetRange(index, count));
            wind.StartWind(0f, false);
        }

        if (curturnCount == 1 && !isSimulation) {
            GameManager.instance.routeManager.ClearTiles();
            if (route.Count > 0) {
                Debug.Log("should END TURN  ");
                EndWind endWind = new EndWind(GameManager.instance, wind, GameManager.instance.arrowController);
                endWind.Execute();
                AddActionToCurTurn(endWind);
            }
        }

        if (curTurn.actions.Count > 0 && !isSimulation)
            singleStepCommands.Add(curTurn);

        if (windTurnsCommand != null)
            windTurnsCommand.commands.Add(curTurn);

        //yield return new WaitForSeconds(turnDur);


    }

    public void HandleTurnEnd(float delay, Action onComplete = null) {
        //yield return new WaitForSeconds(delay);

        // End of the turn

        GridManager.Instance.InvokeGridChanged();


        if (OnTurnEnd != null)
            OnTurnEnd();

        CheckForWindDeform(route);

        onComplete?.Invoke();
    }

    public void CheckForWindDeform(List<Vector3> route) {
        //if (GameManager.instance.state != GameState.Running) return;

        if (OnWindRouteGenerated != null && route.Count > 0) {
            // Gets wind deforms requests
            OnWindRouteGenerated(route);
        }

        if (windCutRequests.Count > 0 | windRestoreRequests.Count > 0) {
            Debug.Log("should try deform, cut req: " + windCutRequests.Count + ", restore req: " + windRestoreRequests.Count);

            ChangeWindRoute changeWindRoute = new ChangeWindRoute(GameManager.instance, windCutRequests, windRestoreRequests);
            changeWindRoute.Execute();

            AddActionToCurTurn(changeWindRoute);

            OnRouteChanged?.Invoke(route);

            wind.DrawWind();
        }
    }

    public void UndoMultiStep() {
        //Debug.Log("try multi step undo");

        if (multiStepCommands.Count == 0) return;

        /*if (route.Count >= 1) {
            CancelRouteDrawing cancelDrawing = new CancelRouteDrawing(curWindSource, routeManager, route);
            cancelDrawing.Execute();
        }

        CancelTurns();
        CancelInvoke();
        */

        //PauseWhenTurnEnd();
        //Debug.Log("multi step undo");
        int index = multiStepCommands.Count - 1;
        MultipleCommand command = multiStepCommands[index];
        command.Undo();

        foreach (var item in command.commands) {
            if (singleStepCommands.Contains(item)) {
                singleStepCommands.Remove(item);
            }
        }

        multiStepCommands.Remove(command);

        //CancelDrawing();
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

        GameManager.instance.pauseOnTurnEnd = true;
        ///PauseWhenTurnEnd();

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

    public void AddActionToCurTurn(Command action) {
        curTurn.actions.Add(action);
    }

    public List<MoveTo> GetMoveWithHighestPriority(List<MoveTo> moves, Vector3 relativeChainMoveDir) {
        if (moves.Count == 0) return null;

        int highest = 0;

        for (int i = 1; i < moves.Count; i++) {
            if (relativeChainMoveDir != Vector3.zero) {
                if (moves[i].dir == relativeChainMoveDir) {
                    highest = i;
                    break;
                }
                else if (moves[highest].dir == relativeChainMoveDir) {
                    break;
                }
            }

            if (moves[i].indexInWind > moves[highest].indexInWind)
                highest = i;
        }

        MoveTo temp = moves[0];
        moves[0] = moves[highest];
        moves[highest] = temp;

        return moves;
    }
}
