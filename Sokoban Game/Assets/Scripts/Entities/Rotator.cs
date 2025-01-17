using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour{

    public Animator animator;
    public new TargetTag tag;

    private GameManager gameManager;

    public List<Vector3> cells = new List<Vector3>();
    public bool isRotating;

    public int rotationCount = 0;
    public int prevTurn = -1;


    public delegate void OnRotatesDelegate(float dir, TargetTag tag);
    public static event OnRotatesDelegate OnRotates;


    void Awake()
    {
        gameManager = GameManager.instance;

        Vector3 pos;

        transform.position = new Vector3(Utility.RoundToNearestHalf(transform.position.x), Utility.RoundToNearestHalf(transform.position.y), 0);

        // Adds all cells which this entitiy occupies.
        pos = transform.position + new Vector3(0.5f, 0.5f, 0);
        cells.Add(pos);
        pos = transform.position + new Vector3(-0.5f, 0.5f, 0);
        cells.Add(pos);
        pos = transform.position + new Vector3(-0.5f,-0.5f, 0);
        cells.Add(pos);
        pos = transform.position + new Vector3(0.5f, -0.5f, 0);
        cells.Add(pos);


        UpdateAnimSpeed(GameManager.instance.state, GameManager.instance.state);
    }

    private void OnEnable() {
        //gameManager.OnTurnEnd += CheckRotation;
        Game.OnTurnStart1 += CheckRotation;
        gameManager.OnStateChange += UpdateAnimSpeed;
    }
    private void OnDisable() {
        //gameManager.OnTurnEnd -= CheckRotation;
        Game.OnTurnStart1 -= CheckRotation;
        gameManager.OnStateChange -= UpdateAnimSpeed;

    }

    private void CheckRotation() {
        if (gameManager.turnCount <= 1) {
            animator.SetBool("rotateCounterClockwise", false);
            animator.SetBool("rotateClockwise", false);
            isRotating = false;
            return;
        }

        CheckRotation(gameManager.curGame.route);
    }

    private void CheckRotation(List<Vector3> route) {
        if (gameManager.turnCount <= 1) {
            animator.SetBool("rotateCounterClockwise", false);
            animator.SetBool("rotateClockwise", false);
            isRotating = false;
            return;
        }

        List<int> rotateRequests = new List<int>(); // 1 = clockwise, -1 = counter clockwise
        foreach (var cell in cells) {
            if (route.Contains(cell)) {

                Vector3 dir;
                int index = route.IndexOf(cell);

                if (index == 0 && !gameManager.curGame.isLooping)
                    continue;

                int prevIndex = gameManager.curGame.isLooping && index == 0 ? route.Count - 1 : index - 1;

                if (cells.Contains(route[prevIndex])) {

                    dir = (gameManager.curGame.isLooping && index == 0)? (route[index] - route[route.Count - 1]).normalized : (route[index] - route[index - 1]).normalized;

                    int rotationDir = 0;

                    if (dir == Vector3.up && cell.x - transform.position.x > 0) {
                        rotationDir = -1;
                    }
                    else if (dir == Vector3.up && cell.x - transform.position.x < 0) {
                        rotationDir = 1;
                    }
                    else if (dir == Vector3.down && cell.x - transform.position.x < 0) {
                        rotationDir = -1;
                    }
                    else if (dir == Vector3.down && cell.x - transform.position.x > 0) {
                        rotationDir = 1;
                    }
                    else if (dir == Vector3.right && cell.y - transform.position.y > 0) {
                        rotationDir = 1;
                    }
                    else if (dir == Vector3.right && cell.y - transform.position.y < 0) {
                        rotationDir = -1;
                    }
                    else if (dir == Vector3.left && cell.y - transform.position.y > 0) {
                        rotationDir = -1;
                    }
                    else if (dir == Vector3.left && cell.y - transform.position.y < 0) {
                        rotationDir = 1;
                    }

                    rotateRequests.Add(rotationDir);
                }
            }
        }

        float sum = 0;
        foreach (var item in rotateRequests) {
            sum += item;
        }

        //Debug.Log("  rot sum : " + sum);
        //Debug.Log("  rot requ count : " + rotateRequests.Count);

        if (sum > 0) {
            // rotating clockwise
            InvokeOnRotate(sum);
            PlayRotateClockwiseAnim();
            RotatorCommand rotate = new RotatorCommand(this);
            rotate.Execute();
            gameManager.AddActionToCurTurn(rotate);
        }
        else if(sum < 0) {
            // rotating coutner clockwise
            InvokeOnRotate(sum);
            PlayRotateCounterClockwiseAnim();
            RotatorCommand rotate = new RotatorCommand(this);
            rotate.Execute();
            gameManager.AddActionToCurTurn(rotate);
        }
        else {
            // not rotating
            PlayIdleAnim();
            isRotating = false;
        }

    }

    public void Rotate() {
        isRotating = true;
        if (prevTurn != gameManager.turnCount)
            rotationCount++;

        prevTurn = gameManager.turnCount;
    }

    private void InvokeOnRotate(float dir) {
        if ((rotationCount + 1) % 2 == 0)
            OnRotates?.Invoke(dir, tag);
    }

    public void UpdateAnimSpeed(GameState from, GameState to) {
        if(to == GameState.Running && !gameManager.curGame.isSimulation) { 

            animator.speed = 1;
        }
        else {
            animator.speed = 0;
        }
    }



    private void PlayRotateClockwiseAnim() {
        animator.Play("Rotator_Anim");

        //animator.SetBool("rotateCounterClockwise", false);
        //animator.SetBool("rotateClockwise", true);
    }
    private void PlayRotateCounterClockwiseAnim() {
        animator.Play("Rotator_CounterClockwise_Anim");


        //animator.SetBool("rotateClockwise", false);
        //animator.SetBool("rotateCounterClockwise", true);
    }

    public void PlayIdleAnim() {
        animator.Play("Idle");


        //animator.SetBool("rotateCounterClockwise", false);
        //animator.SetBool("rotateClockwise", false);
    }
}
