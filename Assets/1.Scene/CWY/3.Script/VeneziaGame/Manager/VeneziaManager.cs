using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeneziaManager : MonoBehaviour
{
    //�������� ���ġ�� ������ �����ϱ� ���� ��ũ��Ʈ
    //ex) �ð��� 0 �϶� ������ ���߰ų�, UI������ ���ְų� ���� Ȱ��

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
