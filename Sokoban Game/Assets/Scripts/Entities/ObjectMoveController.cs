using System.Transactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class ObjectMoveController : MonoBehaviour
{
    public enum State {
        none,
        standing,
        layingVertical,
        layingHorizantal
    }

    public class PushInfo{
        public MoveTo pushedBy;
        public Vector3 pushDir;
        public int indexInChainPush;
        public int destinationTile;
        public int pushOrigin; // 0 = wind, else = other obj's intance id
        public int isValidated; // 0 = false, 1 = true, 2 = unchecked
    }

    private bool _hasSpeed;
    public bool hasSpeed
    {
        get { return _hasSpeed; }
        set
        {
            _hasSpeed = value;
            if(OnSpeedChange != null)
            {
                OnSpeedChange(value);
            }
        }
    }

    //public List<PushInfo> pushInfos = new List<PushInfo>();
    public Dictionary<Vector3, PushInfo> pushedByInfos = new Dictionary<Vector3, PushInfo>();
    public PushInfo pushInfoThis;

    public Vector3 dir;
    public MoveTo movementReserve;
    public State startingState;
    public State curState;
    public Tween tween;

    protected GameManager gameManager;

    public delegate void OnMovResDelegate();
    public event OnMovResDelegate OnMovRes;

    public delegate void OnSpeedChangeDelegate(bool hasSpeed);
    public event OnSpeedChangeDelegate OnSpeedChange;

    public delegate void OnHitDelegate(MoveTo movRes);
    public event OnHitDelegate OnHit;

    protected virtual void Start()
    {
        //pushedByInfos.Add(Vector3.right, null);
        //pushedByInfos.Add(Vector3.left, null);
        //pushedByInfos.Add(Vector3.up, null);
        //pushedByInfos.Add(Vector3.down, null);
    }

    protected virtual void OnEnable(){
        gameManager = GameManager.instance;
        GameManager.instance.OnTurnStart1 += ReserveMovement;
        GameManager.instance.OnTurnStart2 += FindNeighbors;
        //GameManager.instance.OnSpeedChanged += UpdateAnimSpeed;
        gameManager.OnHitsChecked += ValidatePush;

    }

    protected virtual void OnDisable()
    {
        GameManager.instance.OnTurnStart1 -= ReserveMovement;
        GameManager.instance.OnTurnStart2 -= FindNeighbors;
        //GameManager.instance.OnSpeedChanged -= UpdateAnimSpeed;
        gameManager.OnHitsChecked -= ValidatePush;
    }

    public virtual void ReserveMovement(List<Vector3> route)
    {
        pushedByInfos.Clear();
        movementReserve = null;

        int index = -1; // index in wind route
        bool intentToMove = true;
        bool pushed = false;
        Vector3 pos = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);
        Vector3 previousDir = dir;
        // Check if the object is in the wind route
        if (route.Contains(pos)) {
            // Determines the movement direction
            index = route.FindIndex(i => i == pos); // finds index in wind

            // Calculates the direction depand on the index in the looped wind route
            if (GameManager.instance.isLooping && (pos == route[0])){
                dir = route[0] - route[route.Count - 2];
            }
            else{
                if (index == 0){
                    intentToMove = false;
                }
                else{
                    dir = route[index] - route[index - 1];
                }
            }
        }
        else if(!gameManager.isWaiting && gameManager.isLooping && gameManager.turnCount > 0){
            // Pushed by looped wind
            Vector3 windMoveDir = gameManager.windMoveDir;
            if(route.Contains(transform.position - windMoveDir)){
                dir = windMoveDir;
                index = -1;
                pushed = true;
                intentToMove = true;
                PushInfo pushInfo = new PushInfo();
                pushInfo.pushedBy = null;
                pushInfo.pushOrigin = 0;
                pushInfo.indexInChainPush = 0;
                pushInfo.pushDir = dir;
                pushInfo.isValidated = 2;
                pushedByInfos.Add(dir, pushInfo);
                pushInfoThis = pushInfo;
            }
            else{
                hasSpeed = false;
                intentToMove = false;
            }
        }
        else{
            hasSpeed = false;
            intentToMove = false;
        }

        //if (!reserveMov) return;

        Vector3 from = transform.position;
        Vector3 to = from + dir;

        // Reserves movement
        movementReserve = new MoveTo(this, from, to, previousDir, curState, index, tag);
        movementReserve.executionTime = Time.time;
        movementReserve.turnID = GameManager.instance.turnID;
        movementReserve.intentToMove = intentToMove;
        movementReserve.state = curState;
        movementReserve.hasSpeed = hasSpeed;
        movementReserve.pushed = pushed;
        if(OnMovRes != null){
            OnMovRes();
        }
        //if (GameManager.instance.isFirstTurn)
        //{
        GameManager.instance.oldCommands.Add(movementReserve);
        //}
    }

    // Validates movement reserve
    public virtual void FindNeighbors(List<Vector3> route) {
        if (movementReserve == null) return;

        GameManager gameManager = GameManager.instance;

        Vector3 origin = transform.position;
        int destinationTile = -1;

        List<Vector3> neighborVectors = new List<Vector3> { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
        foreach (Vector3 dir in neighborVectors) {
            RaycastHit2D hit = Physics2D.Raycast(origin + dir, Vector2.zero, distance: 1f, LayerMask.GetMask("Wall", "Obstacle", "Pushable"));
            MoveTo neighbor = null;
            if (hit) {
                GameObject obj = hit.transform.gameObject;

                if (movementReserve.intentToMove && this.dir == dir) {
                    //Debug.Log("destination tile layer: " + obj.layer);
                    if (obj.layer == 7) {
                        destinationTile = 1;
                        neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    }
                    else {
                        destinationTile = 2;
                    }
                }
                else {
                    if (obj.layer == 7) {
                        neighbor = obj.GetComponent<ObjectMoveController>().movementReserve;
                    }
                }
                movementReserve.neighbors.Add(dir, neighbor);
            }
        }
        movementReserve.destinationTile = destinationTile;

        MoveTo destinationObj;

        if (!movementReserve.intentToMove) return;

        // Determines destination tile type and adds movement reserve to the mov. res. list

        if (movementReserve.neighbors.TryGetValue(movementReserve.dir, out destinationObj)) {

            if (destinationObj == null) {
                // wall at destination
                gameManager.obstacleAtDestinationMoves.Add(movementReserve);
                //Debug.LogWarning("here");
                //pushedByInfos.Remove(movementReserve.dir);
            }
            else {
                // A moveable object at destination tile
                if (!destinationObj.intentToMove | (destinationObj.intentToMove && -destinationObj.dir == movementReserve.dir)) {
                    gameManager.momentumTransferMoves.Add(movementReserve);
                }
            }
        }
        else {
            // Destination tile is empty
            gameManager.emptyDestinationMoves.Add(movementReserve);
        }
    }

    public virtual void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false){
        
        Vector3 startPos = transform.position;
        tween = transform.DOMove(startPos + dir, GameManager.instance.defTurnDur).SetEase(Ease.InOutQuad); // Ease.Linear
        hasSpeed = true;

        gameManager.AddActionToCurTurn(movementReserve);
    }

    public virtual void FailedMove(){
        Debug.Log("failed move");
        tween = transform.DOPunchPosition(dir / 10, GameManager.instance.defTurnDur / 1.1f, vibrato: 0).SetEase(Ease.OutCubic);
        hasSpeed = false;
    }

    public virtual void Hit(List<MoveTo> emptyDestintionTileMoves){

        if (pushedByInfos.Count > 0)
        {
            TryToPush(pushedByInfos[movementReserve.dir]);
        }
        else
        {
            if(movementReserve != null) {
                //Debug.Log("should try chain momentum transfer: " + movementReserve.obj.transform.parent.name);

                movementReserve.ChainMomentumTransfer(emptyDestintionTileMoves);

            }
            else {
                Debug.Log("cant try chain momentum transfer, move res null: ");
            }
        }

        //ChainPush(movementReserve, emptyDestintionTileMoves);

        /*if(OnHit != null){
            OnHit(movementReserve);
        }*/
    }

    public virtual void TryToPush(PushInfo pushedByInfo) {
        MoveTo moveRes = movementReserve;
        MoveTo destinationObjA;
        //Vector3 dir = moveRes.dir;

        //PushInfo pushInfoThis = pushedByInfos[pushedByInfo.pushDir];

        // Checks if there is something in the way for current movement. if so add that object to the destinationObjA
        if (movementReserve.neighbors.TryGetValue(pushedByInfo.pushDir, out destinationObjA)) {

            if (destinationObjA == null) {
                // wall at destination
                pushedByInfo.destinationTile = 2;
                //Debug.LogWarning("wall at push dest.");
            }
            else {
                // A moveable object at destination tile
                //Debug.LogWarning("A moveable object at push dest. " + gameObject.name);
                pushedByInfo.destinationTile = 1;

                PushInfo pushInfoOther = new PushInfo();
                pushInfoOther.pushedBy = moveRes;
                pushInfoOther.pushDir = pushedByInfo.pushDir;
                pushInfoOther.indexInChainPush = pushedByInfo.indexInChainPush + 1;
                pushInfoOther.pushOrigin = pushedByInfo.pushOrigin;
                pushInfoOther.isValidated = 2;
                //MoveTo destinationObjB;
                //destinationObjA.neighbors.TryGetValue(dir, out destinationObjB)

                //pushInfo.destinationTile =
                //Debug.LogWarning(gameObject.name + " :" + pushInfoOther);
                destinationObjA.obj.pushedByInfos.Add(pushedByInfo.pushDir, pushInfoOther);
                destinationObjA.obj.TryToPush(pushInfoOther);
            }
        }
        else {
            // destination tile is empty
            pushedByInfo.destinationTile = 0;
        }
    }

    private void ValidatePush(List<MoveTo> emptyDestintionTileMoves)
    {
        //Debug.LogWarning("push  res count: " + pushedByInfos.Count + " :" + gameObject.name);
        if (pushedByInfos.Count == 0 | (pushedByInfos.Count == 1 && movementReserve.destinationTile == 2)) return;
        // Determines destination tile type and adds movement reserve to the mov. res. list
        Vector3 dirSum = Vector3.zero;
        for (int i = 0; i < pushedByInfos.Count; i++)
        {
            PushInfo info = pushedByInfos.ElementAt(i).Value;
            if (info == null)
            {
                pushedByInfos.Remove(pushedByInfos.ElementAt(i).Key);
                continue;
            }

            if (info.destinationTile == 2)
            {
                info.isValidated = 0;
                //pushedByInfos.Remove(pushedByInfos.ElementAt(i).Key);
                continue;
            }

            dirSum += pushedByInfos.ElementAt(i).Key;
        }
        //Debug.LogWarning("push  res count: " + pushedByInfos.Count + " :" + gameObject.name);
        //Debug.LogWarning("push  res dir sum: " + dirSum + " :" + gameObject.name);
        //Debug.LogWarning("push  res right dir : " + pushedByInfos[Vector3.right].pushDir + " :" + gameObject.name);
        //Debug.LogWarning("push  res left  dir : " + pushedByInfos[Vector3.left].pushDir + " :" + gameObject.name);
        if (dirSum == Vector3.zero)
        {
            // Failed move
            //Debug.LogWarning("push failed: " + gameObject.name);

            Vector3 failedMoveDir = Vector3.zero;
            int lowestIndex = 200;
            //int previoulowestIndex = lowestIndex;
            bool equalLowestIndexExists = false;
            foreach(var item in pushedByInfos)
            {
                if (item.Value.indexInChainPush < lowestIndex)
                {
                    //previoulowestIndex = lowestIndex;
                    lowestIndex = item.Value.indexInChainPush;
                    failedMoveDir = item.Key;
                    equalLowestIndexExists = false;
                }
                else if(item.Value.indexInChainPush == lowestIndex)
                {
                    equalLowestIndexExists = true;
                    //break;
                }
            }

            if(failedMoveDir != Vector3.zero && !equalLowestIndexExists)
            {
                movementReserve.dir = failedMoveDir;
                movementReserve.intentToMove = true;
                gameManager.obstacleAtDestinationMoves.Add(movementReserve);
            }
            
        }
        else
        {
            PushInfo pushInfo;
            if (pushedByInfos.ContainsKey(dirSum))
            {
                pushInfo = pushedByInfos[dirSum];
            }
            else
            {
                if(pushedByInfos.Count != 2)
                {
                    Debug.Log("Push info count is incorrect");
                }

                // Determine priority
                Vector3 moveDir = Vector3.zero;
                int lowestIndexInChain = 200;
                foreach (var item in pushedByInfos)
                {
                    if (item.Value.indexInChainPush < lowestIndexInChain)
                    {
                        lowestIndexInChain = item.Value.indexInChainPush;
                        moveDir = item.Key;
                    }
                    else if (item.Value.indexInChainPush == lowestIndexInChain)
                    {
                        moveDir = Vector3.zero;
                        break;
                    }
                }

                if(moveDir != Vector3.zero)
                {
                    pushInfo = pushedByInfos[moveDir];
                }
                else
                {
                    PushInfo pushWithWindOrigin = pushedByInfos.Values.ToList().Find(item => item.pushOrigin == 0);
                    if (pushWithWindOrigin != null)
                    {
                        pushInfo = pushWithWindOrigin;
                    }
                    else
                    {
                        //Debug.LogWarning("checking for instance id to determine priority");
                        PushInfo first = pushedByInfos.Values.First();
                        PushInfo last = pushedByInfos.Values.Last();
                        pushInfo = first.pushOrigin > last.pushOrigin ? first : last;
                    }
                }
            }
            

            movementReserve.destinationTile = pushInfo.destinationTile;
            movementReserve.intentToMove = true;
            movementReserve.pushed = true;
            movementReserve.dir = pushInfo.pushDir;
            movementReserve.to = movementReserve.from + pushInfo.pushDir;
            //Debug.LogWarning("push reserve: " + gameObject.name + " : " + movementReserve.from);
            if (pushInfo.destinationTile == 0)
            {
                emptyDestintionTileMoves.Add(movementReserve);
                //Debug.LogWarning("here2");
                //Debug.LogWarning("here2: " + gameObject.name);

                //movementReserve.isMomentumTransferred = false;
            }
            //movementReserve.isMomentumTransferred = true;

        }


    }

    public virtual void ChainPush(MoveTo moveRes, List<MoveTo> emptyDestintionTileMoves) {
        MoveTo destinationObjA;
        Vector3 dir = moveRes.dir;
        moveRes.pushed = true;
        if (moveRes.dir == -moveRes.pushedBy) {
            // chain failed move
            moveRes.ChainFailedMove();
            return;
        }
        Debug.LogWarning(gameObject.name + " dir: " + dir);
        if (movementReserve.neighbors.TryGetValue(dir, out destinationObjA)) { // Checks if there is something in the way for current movement. if so add that object to the destinationObjA
            if (destinationObjA.intentToMove) {
                if (dir == -destinationObjA.dir) // Checks if destination object wants to move towards current object's tile loc
                {
                    //  Extends chain failed move to the neighbor object
                    /*destinationObjA.destinationTile = 2;
                    destinationObjA.to = destinationObjA.from + dir;
                    destinationObjA.pushedBy = dir;
                    moveRes.isMomentumTransferred = true;
                    destinationObjA.Hit(emptyDestintionTileMoves);*/
                    moveRes.ChainFailedMove();
                    Debug.LogWarning("should chain failed move: " + gameObject.name);
                    destinationObjA.ChainFailedMove();
                    Debug.LogWarning("should chain failed move: " + destinationObjA.obj.name);
                    return;
                }
                else if (dir == destinationObjA.dir) {
                    return;
                }
            }


            destinationObjA.dir = dir;
            destinationObjA.to = destinationObjA.from + dir;
            destinationObjA.intentToMove = true;
            destinationObjA.pushedBy = dir;
            destinationObjA.pushed = true;
            moveRes.isMomentumTransferred = true;
            destinationObjA.Hit(emptyDestintionTileMoves);

        }
        else // destination tile is empty
        {
            // objA movement added to emptyDestinationTileMoves from GameManager which will starts to movement.
            moveRes.destinationTile = 0;
            emptyDestintionTileMoves.Add(moveRes);
            moveRes.isMomentumTransferred = false;
        }
    }

    public virtual void SetPos(Vector3 pos) {
        transform.position = pos;
    }

    public virtual void PlayMoveAnim()
    {

    }

    public virtual void PlayTurnAnim()
    {

    }

    public virtual void PlayFailedMoveAnim()
    {

    }

    /*public virtual void ChainPush(Vector3 dir)
    {
        MoveTo destinationObj;
        if (movementReserve.neighbors.TryGetValue(dir, out destinationObj))
        {
            if (destinationObj == null) return;
            if (destinationObj.intentToMove && destinationObj.dir != -dir) return;

            destinationObj.pushed = true;
            destinationObj.obj.hasSpeed = true;
            destinationObj.obj.ChainPush(dir);
        }
    }*/

    public virtual void SetState(State state)
    {
        this.curState = state;
    }
}
