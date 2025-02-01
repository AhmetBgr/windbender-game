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
        Game.OnTurnEnd += CheckForLookingForObjectAtTheDestination2;
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
        Game.OnTurnEnd -= CheckForLookingForObjectAtTheDestination2;
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
        if (GameManager.instance.curGame.isSimulation) return;

        //GameObject obj = Utility.CheckForObjectAt(transform.position, LayerMask.GetMask("Pushable"));
        Vector2Int index = GridManager.Instance.PosToGridIndex(transform.position);
        GameObject obj = GridManager.Instance.GetCell(index).obj;
        if (obj != null)
        {
            if (objMC && objMC.gameObject == obj) return;

            if (obj.tag == "MetalCage")
            {
                objMC = null;
                return;
            }

            objMC = obj.GetComponent<ObjectMoveController>(); ;
        }
        else
        {
            objMC = null;
            //outlineSR.color = initialOutlineColor;

            return;
        }

        if (objMC != null && objMC.curState == lookingFor)
        {
            Debug.LogWarning("destination satisfied: " + obj.name);
            if (particleEffect)
            {

                Debug.LogWarning("particle played: " + obj.name);

                if (particleEffect.isPlaying)
                {
                    ParticleSystem particleEffect2 = Instantiate(particleEffect);
                    particleEffect2.transform.position = transform.position;
                    particleEffect2.Play();
                    return;
                }

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

        //GameObject obj = Utility.CheckForObjectAt(transform.position, LayerMask.GetMask("Pushable"));
        Vector2Int index = GridManager.Instance.PosToGridIndex(transform.position);
        GameObject obj = GridManager.Instance.GetCell(index).obj;
        if (obj != null)
        {
            objMC = obj.GetComponent<ObjectMoveController>();
        }

        if (objMC != null && objMC.curState != lookingFor) {
            objMC = null;
        }
    }


}
