using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWindRoute : Command
{
    public Wind wind;
    GameManager gameManager;
    public Vector3 dir;
    public Vector3 prevPos;
    public float dur;

    public MoveWindRoute(Wind wind, Vector3 dir, float dur) {
        this.wind = wind;
        this.dir = dir;
        this.dur = dur;
        this.gameManager = GameManager.instance;
    }

    public override void Execute() {
        // Moves looped wind 
        for (int i = 0; i < gameManager.route.Count; i++) {
            gameManager.route[i] += dir;
        }

        prevPos = wind.transform.position;
        wind.Move(dir, dur);
    }

    public override void Undo() {
        Debug.Log("wind prev pos:" + prevPos);
        for (int i = 0; i < gameManager.route.Count; i++) {
            gameManager.route[i] -= dir;
        }
        wind.transform.position = prevPos;
    }


}
