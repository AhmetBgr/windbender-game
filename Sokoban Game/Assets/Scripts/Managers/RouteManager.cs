using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using DG.Tweening;

[RequireComponent(typeof(Tilemap))]
public class RouteManager : MonoBehaviour
{
    public Tile headUp;
    public Tile headDown;
    public Tile headLeft;
    public Tile headRight;
    public Tile vertical;
    public Tile horizontal;
    public Tile upRightCorner;
    public Tile upLeftCorner;
    public Tile bottomRightCorner;
    public Tile bottomLeftCorner;
    public Tile validPosTile;

    public AnimatedTile windBottomToTop;
    public AnimatedTile windBottomToRight;
    public AnimatedTile windBottomToLeft;
    public AnimatedTile windTopToBottom;
    public AnimatedTile windTopToRight;
    public AnimatedTile windTopToLeft;
    public AnimatedTile windLeftToRight;
    public AnimatedTile windLeftToTop;
    public AnimatedTile windLeftToBottom;
    public AnimatedTile windRightToLeft;
    public AnimatedTile windRightToTop;
    public AnimatedTile windRightToBottom;
    public AnimatedTile windEndRight;
    public AnimatedTile windEndLeft;
    public AnimatedTile windEndTop;
    public AnimatedTile windEndBottom;


    //public List<Tile> tiles = new List<Tile>();
    public List<Vector3> route = new List<Vector3>();

    public Color windColor = Color.white;

    private Tilemap tilemap;
    private Cursor cursor;
    private WindSourceController curWindSource;
    private IEnumerator routine;
    private Color defColor;

    public List<Vector3> validPos = new List<Vector3>();
    private Vector3[] neighborVectors = new Vector3[4] {Vector3.up, Vector3.down, Vector3.right, Vector3.left};

    public int maxLenght = 0;

    private void OnEnable()
    {
        tilemap = GetComponent<Tilemap>();
        cursor = Cursor.instance;
        defColor = tilemap.color;
        //GameManager.instance.OnStateChange += ClearTiles;
        //GameManager.instance.OnTurnStart2 += ClearTiles;
        GameManager.instance.OnUndo += CancelCoroutine;
        //GameManager.instance.OnSpeedChanged += UpdateAnimSpeed;
    }

    private void OnDisable()
    {
        //GameManager.instance.OnStateChange += ClearTiles;
        //GameManager.instance.OnTurnStart2 -= ClearTiles;
        GameManager.instance.OnUndo -= CancelCoroutine;
        //GameManager.instance.OnSpeedChanged -= UpdateAnimSpeed;
    }
    public void StartDrawing(Vector3 pos)
    {
        //UpdateValidPositions(pos);
        SetLastTile(route);
    }

    public void AddPosition(Vector3 pos, bool isLooping = false)
    {
        //UpdateValidPositions(pos);
        this.route.Add(pos);
        SetLastTile(this.route);
    }

    public void DeletePos(int index)
    {
        DeleteLastTile(route);
        route.RemoveAt(index);
        SetLastTile(route);
        SetOriginTile(route);
        //UpdateValidPositions(route[route.Count - 1]);
    }

    private void SetOriginTile(List<Vector3> route)
    {
        if (route.Count == 0){ return; }

        if(route.Count == 1)
        {
            tilemap.SetTile(tilemap.WorldToCell(route[0]), vertical);
            return;
        }

        if (route[1].x == route[0].x)
        {
            tilemap.SetTile(tilemap.WorldToCell(route[0]), vertical);
        }
        else
        {
            tilemap.SetTile(tilemap.WorldToCell(route[0]), horizontal);
        }
    }


    /*public void UpdateValidPositions(Vector3 cursorPos, bool setAllValid = false)
    {
        ClearValidPositions();
        if(setAllValid){
            foreach (Vector3 neighborVector in neighborVectors){
                Vector3 origin = cursorPos + neighborVector;
                validPos.Add(origin);
                tilemap.SetTile(tilemap.WorldToCell(origin), validPosTile);
            }
            return;
        }

        if(route.Count == maxLenght) return;
        
        bool mayLoop = ( GameManager.instance.isDrawingCompleted &&
                         !GameManager.instance.isLooping && route.Count>= 4 && 
                         (this.route[0] - cursorPos).magnitude == 1 ) ? true : false;
        
        if (!mayLoop && GameManager.instance.isDrawingCompleted) return;

        if (mayLoop)
        {
            validPos.Add(route[0]);
        }
        else
        {
            foreach (Vector3 neighborVector in neighborVectors)
            {
                Vector3 origin = cursorPos + neighborVector;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, distance: 1f, layerMask: LayerMask.GetMask("Wall"));

                if ( !hit && !route.Contains(origin) )
                {
                    validPos.Add(origin);
                }
            }
        }

        foreach (Vector3 pos in validPos)
        {
            tilemap.SetTile(tilemap.WorldToCell(pos), validPosTile);
        }
    }*/

    public void ClearValidPositions()
    {
        foreach (Vector3 pos in validPos)
        {
            if (pos == route[0]) continue;
            tilemap.SetTile(tilemap.WorldToCell(pos), null);
        }

        validPos.Clear();
    }


    public void FixPreviousTile(List<Vector3> positions)
    {
        int lastPosIndex = positions.Count - 1;
        int previousPosIndex = positions.Count - 2;
        
        if (positions.Count < 2) {
            return;
        }
        else if (positions.Count == 2){
 
            if (positions[lastPosIndex].x == positions[previousPosIndex].x)
            {
                tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), vertical);
            }
            else
            {
                tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), horizontal);
            }
            return;
        }

        // Find neighbors
        Vector3 firstNeighbor = positions[lastPosIndex] - positions[previousPosIndex];
        Vector3 secondNeighbor = positions[previousPosIndex - 1] - positions[previousPosIndex];

        Vector3 sum = firstNeighbor + secondNeighbor;

        if (sum == Vector3.up + Vector3.right)
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), bottomLeftCorner);
        }
        else if (sum == Vector3.up + Vector3.left)
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), bottomRightCorner);
        }
        else if (sum == Vector3.down + Vector3.right)
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), upLeftCorner);
        }
        else if(sum == Vector3.down + Vector3.left)
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), upRightCorner);
        }
        else if( (firstNeighbor == Vector3.up | firstNeighbor == Vector3.down) && (sum == Vector3.zero) )
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), vertical);
        }
        else if ((firstNeighbor == Vector3.right | firstNeighbor == Vector3.left) && (sum == Vector3.zero) )
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[previousPosIndex]), horizontal);
        }

    }

    public void SetLastTile(List<Vector3> positions)
    {
        //Tile tile = null;
        if (route.Count == 0) return;
        int lastPosIndex = positions.Count - 1;
        int previousPosIndex = positions.Count - 2;
        if (positions.Count == 1)
        {
            tilemap.SetTile(tilemap.WorldToCell(positions[lastPosIndex]), vertical);
            FixPreviousTile(positions);
            return;
        }


        if (positions[lastPosIndex].x == positions[previousPosIndex].x )
        {
            if(positions[lastPosIndex].y > positions[previousPosIndex].y)
                tilemap.SetTile(tilemap.WorldToCell(positions[lastPosIndex]), headUp);
            else
                tilemap.SetTile(tilemap.WorldToCell(positions[lastPosIndex]), headDown);

            FixPreviousTile(positions);
            return;
       
        }

        if(positions[lastPosIndex].y == positions[previousPosIndex].y)
        {
            if(positions[lastPosIndex].x > positions[previousPosIndex].x)
                tilemap.SetTile(tilemap.WorldToCell(positions[lastPosIndex]), headRight);
            else{
                tilemap.SetTile(tilemap.WorldToCell(positions[lastPosIndex]), headLeft);
            }
            FixPreviousTile(positions);
            return;
        }
        
    }
    public void DrawWindRoute(List<Vector3> route, bool isLooping = false, bool ignoreLastPos = false)
    {
        //UpdateAnimSpeed(GameManager.instance.gameSpeed);
        //int routCount = isLooping ? route.Count : route.Count - 1;
        for (int i = 0; i < route.Count; i++)
        {
            int lastPosIndex = i;
            int previousPosIndex = i - 1;
            //Debug.Log("last pos : " + lastPosIndex);
            if (lastPosIndex == 0) // draws wind in first pos
            {
                if (route[lastPosIndex + 1].x == route[lastPosIndex].x)
                {
                    if (route[lastPosIndex + 1].y > route[lastPosIndex].y)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windTopToBottom);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windBottomToTop);
                }

                if (route[lastPosIndex + 1].y == route[lastPosIndex].y)
                {
                    if (route[lastPosIndex + 1].x > route[lastPosIndex].x)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windLeftToRight);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windRightToLeft);
                }
            }
            else if (lastPosIndex == route.Count - 1 && !isLooping) {

                if (ignoreLastPos) continue;

                // draws wind in last pos
                if (route[lastPosIndex].x == route[lastPosIndex -1].x)
                {
                    if (route[lastPosIndex].y > route[lastPosIndex -1].y)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windTopToBottom);  //windEndTop 
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windBottomToTop); //windEndBottom
                }

                if (route[lastPosIndex].y == route[lastPosIndex -1].y)
                {
                    if (route[lastPosIndex].x > route[lastPosIndex -1].x)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windLeftToRight); //windEndRight
                    else
                    {
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windRightToLeft); //windEndLeft
                    }
                }
            }
            else
            {
                // draws winds  between first and last positions

                // Find neighbors
                int nextposIndex = lastPosIndex + 1;
                Vector3 firstNeighbor;
                Vector3 secondNeighbor;

                if (isLooping  && i == route.Count - 1)
                {
                    firstNeighbor = route[1] - route[lastPosIndex];
                    secondNeighbor = route[route.Count - 2] - route[lastPosIndex];
                }
                else
                {
                    firstNeighbor = route[nextposIndex] - route[lastPosIndex];
                    secondNeighbor = route[previousPosIndex] - route[lastPosIndex];
                }


                Vector3 sum = firstNeighbor + secondNeighbor;

                if (sum == Vector3.up + Vector3.right)
                {
                    if(firstNeighbor == Vector3.up)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windRightToTop);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windTopToRight);

                }
                else if (sum == Vector3.up + Vector3.left)
                {
                    if (firstNeighbor == Vector3.up)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windLeftToTop);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windTopToLeft);
                }
                else if (sum == Vector3.down + Vector3.right)
                {
                    if (firstNeighbor == Vector3.down)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windRightToBottom);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windBottomToRight);
                }
                else if (sum == Vector3.down + Vector3.left)
                {
                    if (firstNeighbor == Vector3.down)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windLeftToBottom);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windBottomToLeft);
                }
                else if ((firstNeighbor == Vector3.up | firstNeighbor == Vector3.down) && (sum == Vector3.zero))
                {
                    if (firstNeighbor == Vector3.up)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windTopToBottom); 
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windBottomToTop);
                }
                else if ((firstNeighbor == Vector3.right | firstNeighbor == Vector3.left) && (sum == Vector3.zero))
                {
                    if (firstNeighbor == Vector3.right)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windLeftToRight);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), windRightToLeft);
                }
            }
        }
    }

    public void WindTransition(List<Vector3> route, bool isLooping = false)
    {
        float disappearDur = GameManager.instance.defTurnDur / 3;
        float appearDur = GameManager.instance.defTurnDur - disappearDur;
        Debug.LogWarning("dissappear duration: " + disappearDur);
        Debug.LogWarning("Real Turn duration: " + GameManager.instance.defTurnDur);
        Debug.LogWarning("def turn duration: " + GameManager.instance.defTurnDur);
        Debug.LogWarning("game Speed: " + GameManager.instance.gameSpeed);
        
        Color clearColor = new Color(1f, 1f, 1f, 0f);
        Color startColor = new Color(1f, 1f, 1f, 0.75f); //tilemap.color;
        routine = Utility.ChangeTilemapColor(tilemap, clearColor, disappearDur, onCompleteCallBack: () => {
            tilemap.transform.position = new Vector3(0f, 0.1f, 0f);
            DeleteTiles();
            //DrawWindRoute(route, isLooping);
        });
        StartCoroutine(routine);

        //routine = Utility.ChangeTilemapColor(tilemap, windColor, appearDur, delay: disappearDur);
        //StartCoroutine(routine);

    }

    public void DrawRoute(List<Vector3> route)
    {
        for (int i = 0; i < route.Count; i++)
        {
            int lastPosIndex = i;
            int previousPosIndex = i - 1;
            if (lastPosIndex == 0)
            {
                if (route[lastPosIndex + 1].x == route[lastPosIndex].x)
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), vertical);
                }
                else
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), horizontal);
                }
            }
            else if (lastPosIndex == route.Count - 1)
            {
                if (route[lastPosIndex].x == route[previousPosIndex].x)
                {
                    if (route[lastPosIndex].y > route[previousPosIndex].y)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), headUp);
                    else
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), headDown);

                }

                if (route[lastPosIndex].y == route[previousPosIndex].y)
                {
                    if (route[lastPosIndex].x > route[previousPosIndex].x)
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), headRight);
                    else
                    {
                        tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), headLeft);
                    }
                }
            }
            else
            {
                // Find neighbors
                int nextposIndex = lastPosIndex + 1;
                Vector3 firstNeighbor = route[nextposIndex] - route[lastPosIndex];
                Vector3 secondNeighbor = route[previousPosIndex] - route[lastPosIndex];

                Vector3 sum = firstNeighbor + secondNeighbor;

                if (sum == Vector3.up + Vector3.right)
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), bottomLeftCorner);
                }
                else if (sum == Vector3.up + Vector3.left)
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), bottomRightCorner);
                }
                else if (sum == Vector3.down + Vector3.right)
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), upLeftCorner);
                }
                else if (sum == Vector3.down + Vector3.left)
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), upRightCorner);
                }
                else if ((firstNeighbor == Vector3.up | firstNeighbor == Vector3.down) && (sum == Vector3.zero))
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), vertical);
                }
                else if ((firstNeighbor == Vector3.right | firstNeighbor == Vector3.left) && (sum == Vector3.zero))
                {
                    tilemap.SetTile(tilemap.WorldToCell(route[lastPosIndex]), horizontal);
                }
            }
        }
    }

    public void DeleteLastTile(List<Vector3> positions)
    {
        tilemap.SetTile(tilemap.WorldToCell(positions[positions.Count -1]), null);
        FixPreviousTile(positions);
    }

    public void ClearTiles() //GameState from, GameState to
    {
        //if (GameManager.instance.turnCount != 0) return;

        //if (from != GameState.Running && to != GameState.Paused) return;

        //if (from != GameState.Running || to != GameState.Waiting && to != GameState.Paused) return;

        if (tilemap == null)
        {
            Debug.LogWarning("tilemap is null");
            return;
        }
        

        Color clearColor = new Color(1f, 1f, 1f, 0f);
        
        routine = Utility.ChangeTilemapColor(tilemap, clearColor, GameManager.instance.defTurnDur / 2, onCompleteCallBack: () =>
        {
            tilemap.transform.position = Vector3.zero;
            tilemap.ClearAllTiles();
            tilemap.color = defColor;
        });
        
        StartCoroutine(routine);
    }

    private void CancelCoroutine()
    {
        if(routine != null)
        {
            tilemap.color = defColor;
            StopCoroutine(routine);
        }
    }

    private void UpdateAnimSpeed(float gameSpeed)
    {
        tilemap.animationFrameRate = gameSpeed;
    }

    public void DeleteTiles()
    {
        tilemap.ClearAllTiles();
    }
}
