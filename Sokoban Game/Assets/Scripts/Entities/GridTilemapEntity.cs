using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTilemapEntity : GridEntity
{
    [SerializeField] private Tilemap tilemap;

    protected override void OnEnable()
    {
        //base.OnEnable();
    }

    protected override void OnDisable()
    {
        //base.OnDisable();
    }


    public override void AddToGridCell()
    {
        // Ensure the tilemap is valid
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned.");
            return;
        }

        // Get the bounds of the tilemap
        BoundsInt tilemapBounds = tilemap.cellBounds;

        // Iterate over all the positions in the tilemap's bounds
        for (int x = tilemapBounds.xMin; x < tilemapBounds.xMax; x++)
        {
            for (int y = tilemapBounds.yMin; y < tilemapBounds.yMax; y++)
            {
                // Get the tile at the current position
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                // If there's a valid tile at this position, update the grid
                if (tile != null)
                {
                    // Convert the tile position to world position
                    Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(x, y, 0)) + tilemap.tileAnchor;

                    // Calculate grid indices based on the world position
                    Vector2Int gridIndex = GridManager.Instance.PosToGridIndex(worldPos);
                    int gridWidth = GridManager.Instance.GridWidth;
                    int gridHeight = GridManager.Instance.GridHeight;

                    if (gridIndex.x >= 0 && gridIndex.x < gridWidth && gridIndex.y >= 0 && gridIndex.y < gridHeight)
                    {
                        // Add or update the tile object in the grid
                        // Assuming you have a prefab or GameObject associated with the tile

                        // Assuming each cell holds multiple objects as in your original structure
                        GridManager.Instance.AddObjectToCell(gridIndex.x, gridIndex.y, this.gameObject);
                    }
                }
            }
        }
    }
}
