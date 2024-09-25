using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AddDustTiles : Command
{
    Tilemap tilemap;
    List<Vector3Int> tiles;
    TileBase tileBase;
    DeathFogController dfController;
    public AddDustTiles(DeathFogController dfController, Tilemap tilemap, List<Vector3Int> tiles, TileBase tileBase) {
        this.tilemap = tilemap;
        this.tiles = tiles;
        this.tileBase = tileBase;
        this.dfController = dfController;
    }

    public override void Execute() {
        foreach (var pos in tiles) {
            tilemap.SetTile(pos, tileBase);
            Vector3 loc = pos + tilemap.tileAnchor;
            if (!dfController.dustTiles.Contains(loc))
                dfController.dustTiles.Add(loc);
        }

        dfController.growCountDown = 5;

        //dfController.StartCoroutine(dfController.volumeController
        //    .LerpExposure(dfController.volumeController.colorAdjustments.postExposure.value - (1.3f / dfController.inDustAmount) * tiles.Count, GameManager.instance.defTurnDur));
        //dfController.volumeController.isVolumeDefault = true;
    }

    public override void Undo() {
        //dfController.StartCoroutine(dfController.volumeController
        //    .LerpExposure(dfController.volumeController.colorAdjustments.postExposure.value + (1.3f / dfController.inDustAmount)*tiles.Count, GameManager.instance.defTurnDur));
        //dfController.volumeController.isVolumeDefault = true;


        foreach (var pos in tiles) {
            tilemap.SetTile(pos, null);
            Vector3 loc = pos + tilemap.tileAnchor;
            if (dfController.dustTiles.Contains(loc))
                dfController.dustTiles.Remove(loc);
        }
        dfController.growCountDown = 0;




    }
}
