using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
public class VolumeController : MonoBehaviour
{

    public Volume volume;
    public ColorAdjustments colorAdjustments;
    public bool isVolumeDefault = false;

    // Start is called before the first frame update
    void Start()
    {
        if (volume.profile.TryGet(out colorAdjustments)) {
            colorAdjustments.postExposure.overrideState = true;
        }
    }

    public IEnumerator LerpExposure(float endValue, float duration) {


        float t = 0;
        float startValue = (float)colorAdjustments.postExposure;

        while (t <= duration) {
            t += Time.deltaTime;

            float percent = Mathf.Clamp01(t / duration);
            colorAdjustments.postExposure.Interp(startValue, endValue, percent);
            Debug.Log("should change exposure to: " + percent);
            yield return null;
        }

    }
}
