using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightControl : MonoBehaviour
{
    public LightPattenList_SO lightData;

    private Light2D currentLight;
    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }

    //TODO: Shift the current light
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        currentLightDetails = lightData.GetLightDetails(season, lightShift);

        if(timeDifference < Settings.lightChangeDuration)
        {
            var colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference; // Time Difference is the number of time to get into morning/night model
            currentLight.color += colorOffset;
            DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            // To(What to change?, What it is after computing?, End value of this change., How long it should cost to change?)
            DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }

        if(timeDifference >= Settings.lightChangeDuration)
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightAmount;
        }
    }
}
