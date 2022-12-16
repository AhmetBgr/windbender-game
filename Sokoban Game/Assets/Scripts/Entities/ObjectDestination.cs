using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectDestination : MonoBehaviour
{
    public ObjectMoveController.State lookingFor;

    public ObjectMoveController objMC;

    private void OnEnable()
    {
        GameManager.instance.OnStateChange += CheckForLookingForObjectAtTheDestination;
    }

    private void OnDisable()
    {
        GameManager.instance.OnStateChange -= CheckForLookingForObjectAtTheDestination;
    }

    void Start()
    {
        GameManager.instance.destinations.Add(this);
    }
    
    public void CheckForLookingForObjectAtTheDestination(GameState from, GameState to)
    {
        if (from != GameState.Running && to != GameState.Waiting) return;

        GameObject obj = Utility.CheckForObjectAt(transform.position, LayerMask.GetMask("Pushable"));
        if (obj != null)
        {
            objMC = obj.GetComponent<ObjectMoveController>();
        }

        if (objMC != null && objMC.curState != lookingFor) {
            objMC = null;
        }
    }


}
