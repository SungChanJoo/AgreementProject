using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeneziaManager : MonoBehaviour
{
    //전반적인 배네치아 게임을 관리하기 위한 스크립트
    //ex) 시간이 0 일때 게임을 멈추거나, UI관리를 해주거나 등의 활동

    private void Update()
    {
      //  GameStop();
    }


    private void GameStop()
    {
        if(TimeSlider.Instance.slider.value <= 0)
        {
            ObjectPooling.Instance.gameObject.SetActive(false);
        }
    }
}
