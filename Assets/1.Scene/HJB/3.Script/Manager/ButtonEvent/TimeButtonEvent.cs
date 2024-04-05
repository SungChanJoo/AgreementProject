using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeButtonEvent : MonoBehaviour
{

    [SerializeField] private Image[] timer_img;
    [SerializeField] private Sprite timerOn;
    [SerializeField] private Sprite timerOff;
    public void ChangeColor_Time(int time)
    {
        switch (time)
        {
            case 60:
                time = 0;
                break;
            case 180:
                time = 1;
                break;
            case 300:
                time = 2;
                break;
        }
        for (int i = 0; i < timer_img.Length; i++)
        {

            if (time == i)
            {
                timer_img[i].sprite = timerOn;
            }
            else
            {
                timer_img[i].sprite = timerOff;
            }            
        }
    }
}
