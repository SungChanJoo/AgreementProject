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
        timeHalf = TimeSlider.Instance.startTime / 2;
    }


    private void Update()
    {
        if(TimeSlider.Instance.startTime <= timeHalf)
        {
            if (!ischange)
            {
                ischange = true;
                RenderSettings.skybox = NightBox;
            }
        }
        print(RenderSettings.skybox);
        
    }


}
