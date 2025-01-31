using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    [SerializeField] private bool isBump = false;

    protected Vector2Int gridIndex;

    // Start is called before the first frame update
    protected virtual void Start()
    {
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

        //GridManager.Instance.RemoveObjectFromCell(gridIndex.x, gridIndex.y, gameObject);

        if (isBump)
        {
            GridManager.Instance.SetCellBumped(gridIndex.x, gridIndex.y, true);
            return;
        }

        GridManager.Instance.AddObjectToCell(gridIndex.x, gridIndex.y, gameObject);
    }
    
    public void RemoveFromGridCell()
    {

        if (isBump)
        {
            GridManager.Instance.SetCellBumped(gridIndex.x, gridIndex.y, false);
            return;
        }

        GridManager.Instance.RemoveObjectFromCell(gridIndex.x, gridIndex.y, this.gameObject);
    }

    public Vector2Int GetGridIndex()
    {
        return GridManager.Instance.PosToGridIndex(transform.position);
    }

}

