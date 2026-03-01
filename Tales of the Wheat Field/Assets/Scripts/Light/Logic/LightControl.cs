using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class LightControl : MonoBehaviour
{
    public LightPattenList_SO lightData;

    private Light2D currentLight;

    private LightDetails currentlightDetails;

    public void Awake()
    {
        currentLight=GetComponent<Light2D>();
    }

    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        currentlightDetails= lightData.GetLightDetails(season, lightShift);
        if(timeDifference< Settings.lightChangeDuration)
        {
            var colorOffst=(currentlightDetails.lightColor- currentLight.color)/Settings.lightChangeDuration*timeDifference;
            currentLight.color += colorOffst;
            //渐变颜色和亮度
            DOTween.To(()=>currentLight.color,c=>currentLight.color=c,currentlightDetails.lightColor,Settings.lightChangeDuration-timeDifference);
            DOTween.To(() => currentLight.intensity, c => currentLight.intensity = c, currentlightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }
        if(timeDifference>=Settings.lightChangeDuration)
        {
            currentLight.intensity = currentlightDetails.lightAmount;
            currentLight.color = currentlightDetails.lightColor;
        }
    }
}
