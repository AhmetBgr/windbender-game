using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindDeform : Command
{
    public struct CutEffectTransform {
        public Vector3 pos;
        public Vector3 rotation;
        public CutEffectTransform(Vector3 pos, Vector3 rotation){
            this.pos = pos;
            this.rotation = rotation;
        }
    }

    public CutEffectTransform wallRight = new CutEffectTransform(new Vector3(0.38f, 0.2f, 0f), new Vector3(0f, 0f, -90f));
    public CutEffectTransform wallLeft = new CutEffectTransform(new Vector3(-0.38f, 0.2f, 0f), new Vector3(0f, 0f, 90f));
    public CutEffectTransform wallUp = new CutEffectTransform(new Vector3(0f, 0.83f, 0f), new Vector3(0f, 0f, 0f));
    public CutEffectTransform wallDown = new CutEffectTransform(new Vector3(0f, 0.4f, 0f), new Vector3(0f, 0f, 180f));

    public RouteManager routeManager;
    public ParticleSystem cutEffect;
    public List<Vector3> route;
    public List<Vector3> routeBeforeDeforming = new List<Vector3>();

    public int cutLenght;

    public override void Execute(){
        base.Execute();
    }

    public override void Undo()
    {

    }

    protected void SetUpEndPlayCutEffect(Vector3 restoreDir, Vector3 cutPos){
        cutEffect.gameObject.SetActive(true);
        if (restoreDir == Vector3.left)
        {
            cutEffect.gameObject.transform.localPosition = wallRight.pos + cutPos;
            cutEffect.gameObject.transform.localRotation = Quaternion.Euler(wallRight.rotation);
        }
        else if (restoreDir == Vector3.right)
        {
            cutEffect.gameObject.transform.localPosition = wallLeft.pos + cutPos;
            cutEffect.gameObject.transform.localRotation = Quaternion.Euler(wallLeft.rotation);
        }
        else if (restoreDir == Vector3.up)
        {
            cutEffect.gameObject.transform.localPosition = wallDown.pos + cutPos;
            cutEffect.gameObject.transform.localRotation = Quaternion.Euler(wallDown.rotation);
        }
        else if (restoreDir == Vector3.down)
        {
            cutEffect.gameObject.transform.localPosition = wallUp.pos + cutPos;
            cutEffect.gameObject.transform.localRotation = Quaternion.Euler(wallUp.rotation);
        }
        cutEffect.Play();
    }
}
