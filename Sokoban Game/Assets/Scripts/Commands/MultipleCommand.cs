using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleCommand : Command{
    public List<Command> commands = new List<Command>();
    private GameManager gameManager;

    public MultipleCommand() {
        gameManager = GameManager.instance;
    }

    public override void Execute() {
        for (int i = 0; i < commands.Count; i++) {
            commands[i].Execute();
        }
    }

    public override void Undo() {
        for (int i = commands.Count - 1; i >= 0; i--) {
            commands[i].Undo();
        }
    }
}
