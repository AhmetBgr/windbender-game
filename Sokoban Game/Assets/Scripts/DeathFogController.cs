using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
public class DeathFogController : ObjectMoveController
{
    public Tilemap fogTM1;
    public Tilemap fogTM2;
    public Tilemap fogTM3;
    public Tilemap fogTM4;


    public List<Vector3> dustTiles = new List<Vector3>();
    private Vector3 posDif = Vector3.zero;

    protected override void OnEnable() {
        GameManager.instance.OnTurnStart1 += TryToCleanFogTile;
        base.OnEnable();
    }

    protected override void OnDisable() {
        GameManager.instance.OnTurnStart1 -= TryToCleanFogTile;
        base.OnDisable();
    }


    // Start is called before the first frame update
    protected override void Start(){
        if (fogTM1 == null) return;

        dustTiles.Clear();
        foreach (var position in fogTM1.cellBounds.allPositionsWithin) {
            if (fogTM1.HasTile(position)) {
                dustTiles.Add(position + fogTM1.tileAnchor);
            }
        }
    }

    private void TryToCleanFogTile(List<Vector3> route) {
        dustTiles.Clear();
        foreach (var position in fogTM1.cellBounds.allPositionsWithin) {
            if (fogTM1.HasTile(position)) {
                dustTiles.Add(position + fogTM1.tileAnchor + Utility.RoundToNearestHalf(transform.position));
            }
        }
        bool canClear = false;
        for (int i = 0; i < route.Count; i++) {
            Vector3 pos = route[i]; // + Utility.RoundToNearestHalf(transform.position);
            //Debug.Log("dust tile check: " + (pos));
            if (dustTiles.Contains(pos)) { //
                if (!canClear) continue;

                Debug.Log("dust tile check positive: " + pos);
                pos -= Utility.RoundToNearestHalf(transform.position);
                Vector3Int posInt = new Vector3Int((int)(pos.x- 0.5f), (int)(pos.y - 0.5f), 0);

                ClearTiles clearTiles = new ClearTiles(this, fogTM1, new List<Vector3Int> { posInt }, fogTM1.GetTile(posInt));
                ClearTiles clearTiles2 = new ClearTiles(this, fogTM2, new List<Vector3Int> { posInt }, fogTM1.GetTile(posInt));
                ClearTiles clearTiles3 = new ClearTiles(this, fogTM3, new List<Vector3Int> { posInt }, fogTM1.GetTile(posInt));
                ClearTiles clearTiles4 = new ClearTiles(this, fogTM4, new List<Vector3Int> { posInt }, fogTM1.GetTile(posInt));

                clearTiles.Execute();
                clearTiles2.Execute();
                clearTiles3.Execute();
                clearTiles4.Execute();

                GameManager.instance.AddActionToCurTurn(clearTiles);
                GameManager.instance.AddActionToCurTurn(clearTiles2);
                GameManager.instance.AddActionToCurTurn(clearTiles3);
                GameManager.instance.AddActionToCurTurn(clearTiles4);

                canClear = false;

                //break;
            }
            else {
                canClear = true;
            }
        }

    }

    public override void ReserveMovement(List<Vector3> route) {
        return;

        pushedByInfos.Clear();
        movementReserve = null;

        int index = -1; // index in wind route
        bool intentToMove = true;
        bool pushed = false;
        Vector3 pos = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);
        Vector3 previousDir = dir;
        dir = Vector3.down;

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
    }
    public override void FindNeighbors(List<Vector3> route) {
        if (movementReserve == null) return;
        
        gameManager.emptyDestinationMoves.Add(movementReserve);
    }

    public override void Move(Vector3 dir, bool stopAftermoving = false, bool pushed = false) {
        /*for (int i = 0; i < dustTiles.Count; i++) {
            dustTiles[i] += dir;
        }*/

        Vector3 startPos = transform.position;
        //posDif += dir;
        tween = transform.DOMove(startPos + dir, GameManager.instance.defTurnDur).SetEase(Ease.Linear); // Ease.Linear
        hasSpeed = true;

        gameManager.AddActionToCurTurn(movementReserve);
    }
}
