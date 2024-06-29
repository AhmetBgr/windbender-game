using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : Command{

    public List<Command> actions = new List<Command>();
    public int turnCount;

    private GameManager gameManager;

    public Turn(int turnCount, int turnID) {
        this.turnCount = turnCount;
        this.turnID = turnID;
        gameManager = GameManager.instance;
    }

    public override void Execute() {
        gameManager.turnID++;
        gameManager.turnCount--;
        

        for (int i = 0; i < actions.Count; i++) {
            actions[i].Execute();
        }
    }

    public override void Undo() {
        gameManager.turnID = turnID;
        gameManager.turnCount = turnCount;
        //gameManager.state = GameState.Paused;

        for (int i = actions.Count -1; i >= 0; i--) {
            actions[i].Undo();
        }
    }

}


