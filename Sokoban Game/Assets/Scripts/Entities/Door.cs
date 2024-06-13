using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;
    public Collider2D col;
    public GameObject invinsibleMask;
    public new TargetTag tag;
    public bool isOpen;
    public int intentedCutIndex;
    public bool isWindRouteInterrupted = false;

    void Start()
    {
        //Sets initial state
        if (isOpen)
            Open();
        else
            Close();

    }

    private void OnEnable()
    {
        ButtonEntity.OnButtonToggle += ToggleState;
        GameManager.instance.OnTurnStart1 += SaveState;
        GameManager.instance.OnWindRouteGenerated += CheckForDeformRequest;
    }

    private void OnDisable()
    {
        ButtonEntity.OnButtonToggle -= ToggleState;
        GameManager.instance.OnTurnStart1 -= SaveState;
        GameManager.instance.OnWindRouteGenerated -= CheckForDeformRequest;
    }

    // Toggles door state between open and close
    private void ToggleState(TargetTag tag)
    {
        if (tag != this.tag) return;


        if (isOpen)
            TryToClose();
        else
            Open();
    }

    private void CheckForDeformRequest(List<Vector3> route)
    {
        // Wind route cutting
        GameManager gameManager = GameManager.instance;
        if ( gameManager.turnCount > 0){
            if (isOpen && transform.position == route[route.Count -1] + gameManager.windRouteDeformInfo.restoreDir){

                gameManager.windRouteDeformInfo.restore = true;
            }
            else if(route.Contains(transform.position) && !isOpen ){

                intentedCutIndex = route.FindIndex(i => i == transform.position);
                WindRouteDeformInfo windRouteDeformInfo = gameManager.windRouteDeformInfo;
                if (intentedCutIndex >= 0 && (windRouteDeformInfo.cutIndex == -1 || intentedCutIndex < windRouteDeformInfo.cutIndex)){
                    gameManager.windRouteDeformInfo.cutIndex = intentedCutIndex;
                    gameManager.windRouteDeformInfo.door = this;
                    gameManager.windRouteDeformInfo.restoreDir = intentedCutIndex == 0 ? route[intentedCutIndex +1] - route[intentedCutIndex] : route[intentedCutIndex] - route[intentedCutIndex -1];
                }
            }
        }
    }
    public void Open(){
        isOpen = true;
        animator.SetBool("isOpen", isOpen);
        col.enabled = false;
        invinsibleMask.SetActive(false);
    }

    public void Close(){
        isOpen = false;
        animator.SetBool("isOpen", isOpen);
        col.enabled = true;
        invinsibleMask.SetActive(true);
    }

    // Checks if there is an object at the location of the door before closing
    private void TryToClose()
    {
        GameObject obj = Utility.CheckForObjectAt(transform.position, LayerMask.GetMask("Pushable")); 

        if (obj != null) return;

        Close();
    }
    // Saves state of door at the beginning of a turn for undo function
    private void SaveState(List<Vector3> route)
    {
        DoorState doorState = new DoorState(this, isOpen, Time.time);
        //if(GameManager.instance.isFirstTurn)
        GameManager.instance.oldCommands.Add(doorState);
    }


}
