using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



public class ObjectDestination : MonoBehaviour
{
    public ObjectMoveController.State lookingFor;

    public ObjectMoveController objMC;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer outlineSR;
    public ParticleSystem particleEffect;

    private Color initialOutlineColor;

    private void OnEnable()
    {
        //GameManager.instance.OnStateChange += CheckForLookingForObjectAtTheDestination;
        GameManager.instance.OnTurnEnd += CheckForLookingForObjectAtTheDestination2;
        GameManager.instance.OnUndo += ResetObject;

        Color initialCol = spriteRenderer.color;
        initialOutlineColor = outlineSR.color;

        Sequence sequence = DOTween.Sequence();
        sequence.Append( spriteRenderer.DOFade(0f, 2f) );
        sequence.Append( spriteRenderer.DOFade(0.5f, 2f) );
        sequence.SetLoops(-1);

        Sequence sequence2 = DOTween.Sequence();
        sequence2.Append(outlineSR.DOFade(1f, 2f));
        sequence2.Append(outlineSR.DOFade(0f, 2f));
        sequence2.SetLoops(-1);
    }

    private void OnDisable()
    {
        //GameManager.instance.OnStateChange -= CheckForLookingForObjectAtTheDestination;
        GameManager.instance.OnTurnEnd -= CheckForLookingForObjectAtTheDestination2;
        GameManager.instance.OnUndo -= ResetObject;

        DOTween.KillAll();
    }

    void Start()
    {
        GameManager.instance.destinations.Add(this);

        if (particleEffect)
            particleEffect.transform.localPosition = Vector3.zero;
    }

    public void CheckForLookingForObjectAtTheDestination2()
    {
        GameObject obj = Utility.CheckForObjectAt(transform.position, LayerMask.GetMask("Pushable"));
        if (obj != null)
        {
            

            if (objMC)  return;

            objMC = obj.GetComponent<ObjectMoveController>();
        }
        else
        {
            objMC = null;
            //outlineSR.color = initialOutlineColor;
        }

        if (objMC != null && objMC.curState == lookingFor)
        {
            Debug.LogWarning("destination satisfied");
            if (particleEffect)
            {
                particleEffect.Play();
            }
            //outlineSR.color = Color.white;
        }
        else
        {
            objMC = null;
            //outlineSR.color = initialOutlineColor;
        }
    }

    private void ResetObject()
    {
        objMC = null;        
    }

    public void CheckForLookingForObjectAtTheDestination(GameState from, GameState to)
    {
        if (from != GameState.Running && to != GameState.Paused) return;

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
