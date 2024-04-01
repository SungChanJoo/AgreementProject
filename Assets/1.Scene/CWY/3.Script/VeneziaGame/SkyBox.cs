using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBox : MonoBehaviour
{
    public Material MoningBox;
    public Material NightBox;

    float timeHalf;

    private bool ischange = false;
    private void Start()
    {
        RenderSettings.skybox = MoningBox;
        timeHalf = TimeSlider.Instance.StartTime_ / 2;
    }


    private void Update()
    {
        if(TimeSlider.Instance.StartTime_ <= timeHalf)
        {
            if (!ischange)
            {
                ischange = true;
                RenderSettings.skybox = NightBox;
            }
        }        
    }


}
