using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class FogofWar : MonoBehaviour
{
    public GameObject area01;
    public GameObject area02;
    public GameObject area03;

    private AreaData area02Data;

    private LevelManager levelManager;
    //public Tilemap area01;

    // Start is called before the first frame update
    void Start()
    {
        //mesh = GetComponent<MeshFilter>().mesh;
        //UpdateColor();
        levelManager = LevelManager.instance;

        ParticleSystem areaP01 = area01.GetComponent<ParticleSystem>();
        ParticleSystem areaP02 = area02.GetComponent<ParticleSystem>();

        areaP01.gameObject.SetActive(false);
        //var main = area01.main;
        //var forceOverLifetime = area01.forceOverLifetime;

        //forceOverLifetime.x = -2f;
        //main.loop = false;
        area02Data = levelManager.are02Data;
        if (area02Data.unlocker.state == Level.State.completed) {
            if (area02Data.isUnlocked) {
                area02.SetActive(false);
            }
            else {
                UnlockArea(areaP02);
            }
        }
    }

    private void UnlockArea(ParticleSystem areaP, float delay = 0f) {
        StartCoroutine(_UnlockArea(areaP, delay));
    }

    private IEnumerator _UnlockArea(ParticleSystem areaP, float delay = 0f) {
        yield return new WaitForSeconds(delay);

        var main2 = areaP.main;
        var forceOverLifetime2 = areaP.forceOverLifetime;
        var renderer = area02.GetComponent<ParticleSystemRenderer>();

        main2.loop = false;
        forceOverLifetime2.enabled = true;
        renderer.sortingOrder = 0;
        area02Data.SetUnlocked();
    }

}
