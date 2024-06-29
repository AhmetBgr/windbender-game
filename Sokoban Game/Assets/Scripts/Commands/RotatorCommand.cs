using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorCommand : Command{
    Rotator rotator;
    int rotationCount;
    int prevTurn = -1;
    public RotatorCommand(Rotator rotator) {
        this.rotator = rotator;
        this.rotationCount = rotator.rotationCount;
        this.prevTurn = rotator.prevTurn;
    }

    public override void Execute() {
        rotator.Rotate();
    }

    public override void Undo() {
        rotator.rotationCount = rotationCount;
        rotator.prevTurn = prevTurn;
    }
}
