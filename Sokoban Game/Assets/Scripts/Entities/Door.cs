using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;
    public Collider2D col;
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
    private void ToggleState(TargetTag tag)
    {
        if (tag != this.tag) return;


        if (isOpen)
            TryToClose();
        else
            Open();
    }

    public void Open()
    {
        isOpen = true;
        animator.SetBool("isOpen", isOpen);
        col.enabled = false;

    }
    public void Close()
    {
        isOpen = false;
        animator.SetBool("isOpen", isOpen);
        col.enabled = true;
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
        if(GameManager.instance.isFirstTurn)
            GameManager.instance.oldCommands.Add(doorState);
    }


}
