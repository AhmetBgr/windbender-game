using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ClearTiles : Command
{
    Tilemap tilemap;
    List<Vector3Int> tiles;
    TileBase tileBase;
    DeathFogController dfController;
    public ClearTiles(DeathFogController dfController, Tilemap tilemap, List<Vector3Int> tiles, TileBase tileBase) {
        this.tilemap = tilemap;
        this.tiles = tiles;
        this.tileBase = tileBase;
        this.dfController = dfController;
    }

    public override void Execute() {

        foreach (var pos in tiles) {
            tilemap.SetTile(pos, null);
            Vector3 loc = pos + tilemap.tileAnchor;
            if (dfController.dustTiles.Contains(loc))
                dfController.dustTiles.Remove(loc);
        }

        if (dfController.dustTiles.Count == 0) { // dfController.dustTiles.Count == 0 && !dfController.volumeController.isVolumeDefault
            dfController.StartCoroutine(dfController.volumeController
                .LerpExposure(0.5f, GameManager.instance.defTurnDur));
            dfController.volumeController.isVolumeDefault = true;
        }

        /*if (true) { // dfController.dustTiles.Count == 0 && !dfController.volumeController.isVolumeDefault
            dfController.StartCoroutine(dfController.volumeController
                .LerpExposure(dfController.volumeController.colorAdjustments.postExposure.value + (1.3f/dfController.inDustAmount), GameManager.instance.defTurnDur));
            dfController.volumeController.isVolumeDefault = true;
        }*/
    }

    public override void Undo() {

        foreach (var pos in tiles) {
            tilemap.SetTile(pos, tileBase);
            Vector3 loc = pos + tilemap.tileAnchor;
            if (!dfController.dustTiles.Contains(loc))
                dfController.dustTiles.Add(loc);
        }

        dfController.StartCoroutine(dfController.volumeController
            .LerpExposure(-0.8f, GameManager.instance.defTurnDur));
        dfController.volumeController.isVolumeDefault = true;

        /*if (dfController.dustTiles.Count > 0 && dfController.volumeController.isVolumeDefault) {
            dfController.StartCoroutine(dfController.volumeController.LerpExposure(-0.8f, 0f));
            dfController.volumeController.isVolumeDefault = false;
        }*/

    }
}
