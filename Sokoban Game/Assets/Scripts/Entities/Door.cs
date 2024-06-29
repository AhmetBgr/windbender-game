using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;
    public Collider2D col;
    public GameObject invinsibleMask;
    public WindCutter windCutter;
    public new TargetTag tag;
    public bool isOpen;

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
    }

    private void OnDisable()
    {
        ButtonEntity.OnButtonToggle -= ToggleState;
        GameManager.instance.OnTurnStart1 -= SaveState;
    }

    // Toggles door state between open and close
    public void ToggleState(TargetTag tag)
    {
        if (tag != this.tag) return;

        DoorState doorState = new DoorState(this, isOpen, Time.time);
        GameManager.instance.curTurn.actions.Add(doorState);

        if (isOpen) {
            TryToClose();

        }
        else {
            Open();
            windCutter.isMoved = true;
        }
    }

    public void Open(){
        isOpen = true;
        animator.SetBool("isOpen", isOpen);
        col.enabled = false;
        invinsibleMask.SetActive(false);
        windCutter.canCut = false;

        windCutter.OnMoved(Vector3.right * 100000);
    }

    public void Close(){

        isOpen = false;
        animator.SetBool("isOpen", isOpen);
        col.enabled = true;
        invinsibleMask.SetActive(true);
        windCutter.canCut = true;
        windCutter.OnMoved(transform.position);

    }

    // Checks if there is an object at the location of the door before closing
    private void TryToClose()
    {
        GameObject obj = Utility.CheckForObjectAt(transform.position, LayerMask.GetMask("Pushable")); 

        if (obj != null) return;

        Close();
        windCutter.isMoved = false;
    }
    // Saves state of door at the beginning of a turn for undo function
    private void SaveState(List<Vector3> route)
    {
        DoorState doorState = new DoorState(this, isOpen, Time.time);
        //if(GameManager.instance.isFirstTurn)
        GameManager.instance.oldCommands.Add(doorState);
    }


}
