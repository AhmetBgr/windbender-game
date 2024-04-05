using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Push : MonoBehaviour
{   
    [System.Serializable]
    public struct NeighborDir{
        public Vector3 dir;
        public List<ObjectMoveController> neighbors;

        /*public NeighborDir(int safd){
            dir = Vector3.zero;
            neighbors = new List<MoveTo>();
        }*/
    }

    /*public ObjectMoveController objectMoveController;
    public NeighborDir[] neighborDirs;

    public IDictionary<Vector3, MoveTo> neighbors = new Dictionary<Vector3, MoveTo>();


    private GameManager gameManager;

    protected virtual void OnEnable(){
        gameManager = GameManager.instance;
        //MoveTo.OnMomentumTransfer += ChainPush;
        objectMoveController.OnMovRes += AddEvent;
    }

    protected virtual void OnDisable(){
        objectMoveController.OnMovRes -= AddEvent;
        
        objectMoveController.OnHit -= ChainPush;
    }

    private void AddEvent(){
        objectMoveController.OnHit += ChainPush;
    }*/

    /*public void ChainPush(MoveTo moveRes){
        MoveTo destinationObjA;
        Vector3 dir = moveRes.dir;

        if(moveRes.pushed && moveRes.dir == -moveRes.pushedBy){
            // chain failed move
            moveRes.ChainFailedMove();
            return;
        }

        if (neighbors.TryGetValue(dir, out destinationObjA)) { // Checks if there is something in the way for current movement. if so add that object to the destinationObjA
            if (destinationObjA.intentToMove){
                if(dir == -destinationObjA.dir) // Checks if destination object wants to move towards current object's tile loc
                {
                    //  Extends chain failed move to the neighbor object
                    destinationObjA.destinationTile = 2;
                    destinationObjA.Hit();
                    destinationObjA.pushedBy = dir;
                    moveRes.isMomentumTransferred = true;
                    return;
                }
                else if(dir == destinationObjA.dir){
                    return;
                }
            }
            

            destinationObjA.dir = dir;
            destinationObjA.to = destinationObjA.from + dir;
            destinationObjA.intentToMove = true;
            destinationObjA.Hit();
            moveRes.isMomentumTransferred = true;
        }

    }*/
}
