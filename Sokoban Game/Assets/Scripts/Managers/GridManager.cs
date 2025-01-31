using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Cell
{
    public GameObject obj;  // Change obj to a List<GameObject>
    public Vector3 pos;
    //public string layer;
    public bool isBump;

    // Initialize the list in the constructor
    public Cell(Vector3 position, Layer layer = Layer.Other, bool isBump = false)
    {
        obj = null;
        pos = position;
        //this.layer = layer;
        this.isBump = isBump;
    }
}

public enum Layer
{
    Other, Wall, Obstacle, Moveable
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public static Cell[,] grid; // A 2D array to hold cells
    private const float cellSize = 1.0f; // Assuming each cell is 1x1 unit in size
    [SerializeField] private int _gridWidth = 25; // Number of cells along the X-axis. Todo: makesure given value is an odd number.
    [SerializeField] private int _gridHeight = 15; // Number of cells along the Y-axis. Todo: makesure given value is an odd number.

    public int GridWidth
    {
        get => _gridWidth;
        set{
            if (value % 2 == 0){
                Debug.LogWarning("GridWidth must be an odd number. Incrementing value by 1 to make it odd.");
                _gridWidth = value + 1; // Ensure the value is odd
            }
            else
                _gridWidth = value;
        }
    }

    public int GridHeight{
        get => _gridHeight;
        set{
            if (value % 2 == 0){
                Debug.LogWarning("GridHeight must be an odd number. Incrementing value by 1 to make it odd.");
                _gridHeight = value + 1; // Ensure the value is odd
            }
            else
                _gridHeight = value;
        }
    }


    public static event Action GridChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }



        Instance = this;

        GridWidth = _gridWidth;
        GridHeight = _gridHeight;

        grid = new Cell[GridWidth, GridHeight];
        PopulateGrid();
        //AssignObjectsToCells();
    }

    private void OnEnable()
    {
        //Game.OnTurnEnd += InvokeGridChanged;
    }

    private void OnDisable()
    {
        //Game.OnTurnEnd -= InvokeGridChanged;

    }

    public void InvokeGridChanged()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if(grid[x,y].obj != null && grid[x, y].obj.layer == 8)
                {
                    continue;
                }
                grid[x, y].obj = null;
            }
        }

        GridChanged?.Invoke();
    }

    private void PopulateGrid()
    {
        Vector3 gridCenterOffset = new Vector3((GridWidth - 1) * cellSize / 2.0f, (GridHeight - 1) * cellSize / 2.0f, 0);

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, y * cellSize, 0) - gridCenterOffset + new Vector3(cellSize / 2.0f, cellSize / 2.0f, 0);
                grid[x, y] = new Cell(cellPosition); // Initialize the Cell with an empty list
            }
        }
    }

    /*private void AssignObjectsToCells()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 cellPosition = grid[x, y].pos;
                GameObject obj = Utility.CheckForObjectAt(cellPosition, LayerMask.GetMask("Wall"));

                if (obj != null)
                {
                    grid[x, y].objs.Add(obj);  // Add object to the list of objects in the cell
                }
            }
        }
    }*/

    public Vector2Int PosToGridIndex(Vector3 pos)
    {
        Vector3 gridCenterOffset = new Vector3((GridWidth - 1) * cellSize / 2.0f, (GridHeight - 1) * cellSize / 2.0f, 0);

        int x = Mathf.RoundToInt((pos.x + gridCenterOffset.x - cellSize / 2.0f) / cellSize);
        int y = Mathf.RoundToInt((pos.y + gridCenterOffset.y - cellSize / 2.0f) / cellSize);

        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            return new Vector2Int(x, y);
        }

        Debug.LogWarning("Position is out of grid bounds.");
        return new Vector2Int(-1, -1); // Return an invalid value if out of bounds
    }


    public void SetCell(int x, int y, GameObject obj, Vector3 pos)
    {
        grid[x, y].obj = obj;  // Set the entire list of objects
        grid[x, y].pos = pos;
    }
    public void SetCell(GameObject obj, Vector3 pos)
    {
        Vector2Int index = PosToGridIndex(pos);

        grid[index.x, index.y].obj = obj;
        grid[index.x, index.y].pos = pos;

        Debug.LogWarning("No cell found near the provided position.");
    }

    public Cell GetCell(Vector3 pos)
    {
        Vector2Int index = PosToGridIndex(pos);

        if (IsOutSideOfGrid(index)) return new Cell();

        return grid[index.x, index.y];
    }

    public void AddObjectToCell(int x, int y, GameObject obj)
    {
        /*if (grid[x, y].obj == null)
            grid[x, y].obj = null;
        */

        if (IsOutSideOfGrid(new Vector2Int(x,y))) return;

        grid[x, y].obj = obj;  // Add a new object to the cell's list
    }
    /*public void AddObjectToBottomCell(int x, int y, GameObject obj)
    {
        if (grid[x, y].objs == null)
            grid[x, y].objs = new List<GameObject>();

        grid[x, y].objs.Insert(0, obj);  // Add a new object to the cell's list

    }*/

    public void RemoveObjectFromCell(int x, int y, GameObject obj)
    {
        if (grid[x, y].obj != null)
            grid[x, y].obj = null;  // Remove a specific object from the cell
    }

    public void SetCellBumped(int x, int y, bool isBumped)
    {
        grid[x, y].isBump = isBumped;
    }

    public bool IsOutSideOfGrid(Vector2Int index)
    {
        // Check if the index is outside the bounds of the grid
        if (index.x < 0 || index.x >= GridWidth || index.y < 0 || index.y >= GridHeight)
        {
            return true; // The index is outside the grid
        }

        return false; // The index is within the grid
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        // Draw grid cells and highlight those with objects
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Gizmos.color = grid[x, y].obj != null ? Color.green : (Color.gray * new Color(1, 1, 1, 0.5f));

                Gizmos.DrawWireCube(grid[x, y].pos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));

                if (grid[x, y].obj != null) continue;

                Gizmos.DrawWireCube(grid[x, y].pos, new Vector3(cellSize * 0.92f, cellSize * 0.92f, 0.1f));
                Gizmos.DrawWireCube(grid[x, y].pos, new Vector3(cellSize * 0.94f, cellSize * 0.94f, 0.1f));

            }
        }
    }
}

