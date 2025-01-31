using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetTag {
    Blue, Yellow
}

public class ButtonEntity : MonoBehaviour
{
    public new TargetTag tag;
    public bool isDown = false;

    public delegate void OnButtonToggleDelegate(TargetTag tag);
    public static event OnButtonToggleDelegate OnButtonToggle;

    public void OnEnable()
    {
        Game.OnTurnEnd += CheckForObject;
        Game.OnTurnStart1 += SaveState;
    }

    public void OnDisable()
    {
        Game.OnTurnEnd -= CheckForObject;
        Game.OnTurnStart1 -= SaveState;
    }

    public void CheckForObject()
    {
        Vector3 pos = transform.position;
        //RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, distance: 1f, LayerMask.GetMask("Pushable"));
        Vector2Int index = GridManager.Instance.PosToGridIndex(pos);
        ObjectMoveController moveCont = null;
        GameObject obj = GridManager.grid[index.x, index.y].obj; 

        if(obj != null)
            obj.TryGetComponent(out moveCont);

        //ObjectMoveController obj = null;
        /*if (hit)
        {
            obj = hit.transform.gameObject.GetComponent<ObjectMoveController>();
        }*/

        ChangeButtonState(moveCont);
        
    }

    private void ChangeButtonState(ObjectMoveController obj){
        if (!isDown && obj != null && OnButtonToggle != null){
            ChangeButtonState buttonState = new ChangeButtonState(this, isDown, Time.time);
            GameManager.instance.curGame.curTurn.actions.Add(buttonState);

            OnButtonToggle(tag);
            isDown = true;
            return;
        }
        else if ( isDown && obj == null && OnButtonToggle != null){
            ChangeButtonState buttonState = new ChangeButtonState(this, isDown, Time.time);
            GameManager.instance.curGame.curTurn.actions.Add(buttonState);

            OnButtonToggle(tag);
            isDown = false;
        }
        /*if(obj == null)
        {
            isDown = false;
        }*/
    }
    private void SaveState(List<Vector3> route)
    {
        ChangeButtonState buttonState = new ChangeButtonState(this, isDown, Time.time);
        //if (GameManager.instance.isFirstTurn)
        GameManager.instance.oldCommands.Add(buttonState);
    }


}
