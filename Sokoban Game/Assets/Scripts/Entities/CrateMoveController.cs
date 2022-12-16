using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CrateMoveController : ObjectMoveController
{
    public Animator animator;

    public override void Move(Vector3 dir, bool stopAftermoving = false)
    {
        base.Move(dir, stopAftermoving);

        animator.speed = 1 / GameManager.instance.turnDur;

        if (dir == Vector3.right)
            animator.Play("Crate_move_right");
        else if (dir == Vector3.left)
            animator.Play("Crate_move_left");
        else if (dir == Vector3.down)
            animator.Play("Crate_move_down");
        else if (dir == Vector3.up)
            animator.Play("Crate_move_up");
    }

    public override void FailedMove()
    {
        base.FailedMove();

        if (dir == Vector3.right)
            animator.Play("Crate_failed_move_right");
        else if (dir == Vector3.left)
            animator.Play("Crate_failed_move_left");
        else if (dir == Vector3.down)
            animator.Play("Crate_failed_move_down");
        else if (dir == Vector3.up)
            animator.Play("Crate_failed_move_up");
    }
}
