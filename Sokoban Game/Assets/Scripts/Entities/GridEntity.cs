using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridEntityType
{
    Obj, FloorObj, SpaceLess
}

public class GridEntity : MonoBehaviour
{
    public GridEntityType type;

    public bool dontAddAtTheStart = false;

    protected Vector2Int gridIndex;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (dontAddAtTheStart) return;

        AddToGridCell();
    }

    protected virtual void OnEnable()
    {
        GridManager.GridChanged += AddToGridCell;
    }

    protected virtual void OnDisable()
    {
        GridManager.GridChanged -= AddToGridCell;
    }

    public virtual void AddToGridCell()
    {
        gridIndex = GridManager.Instance.PosToGridIndex(transform.position);
        
        if(type == GridEntityType.Obj)
        {
            GridManager.Instance.AddObjectToCell(gridIndex.x, gridIndex.y, gameObject);
        }
        else if(type == GridEntityType.FloorObj)
        {
            GridManager.Instance.AddFloorObjectToCell(gridIndex.x, gridIndex.y, gameObject);
        }
        else
        {
            GridManager.Instance.AddSpacelessObjectToCell(gridIndex.x, gridIndex.y, gameObject);
        }

    }
    
    public void RemoveFromGridCell()
    {
        if (type == GridEntityType.Obj)
        {
            GridManager.Instance.RemoveObjectFromCell(gridIndex.x, gridIndex.y, this.gameObject);

        }
        else if (type == GridEntityType.FloorObj)
        {
            GridManager.Instance.RemoveFloorObjectFromCell(gridIndex.x, gridIndex.y, this.gameObject);
        }
        else
        {
            GridManager.Instance.RemoveSpacelessObjectFromCell(gridIndex.x, gridIndex.y, this.gameObject);
        }

    }

    public Vector2Int GetGridIndex()
    {
        return GridManager.Instance.PosToGridIndex(transform.position);
    }

}

