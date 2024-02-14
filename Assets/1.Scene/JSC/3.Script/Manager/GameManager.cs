using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    [SerializeField] private Slider _shutdownSlider;
    [SerializeField] private GameObject _shutdownImg;

    private string _playTime = "PlayTime";
    private string _isShutdown = "IsShutdown";
    private string _day = "Day";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        //�˴ٿ� Ȱ��ȭ ����
        if (PlayerPrefs.GetInt(_isShutdown) == 0)
        {
            //�˴ٿ� �� ���� ���� ���̶�� �˴ٿ����
            //todo 0125 �д�� ���� �ٲٱ�
            Debug.Log(DateTime.Now.Minute + " | " + PlayerPrefs.GetInt(_day));
            if (DateTime.Now.Minute == PlayerPrefs.GetInt(_day))
            {
                PlayerPrefs.SetInt(_isShutdown, 0);
                _shutdownImg.SetActive(true);
            }
            //�ƴϸ� �˴ٿ� ���� ����
            else
            {
                StartShutdown();
                PlayerPrefs.SetInt(_isShutdown, 1);
                _shutdownImg.SetActive(false);
            }
        }
        else
        {
            _shutdownImg.SetActive(false);
        }
    }

    // �˴ٿ� ���
    //1. �θ� �ð��� ������ �� �־����
    //2. ���� �ð��� �Ǹ� ���� ����
    //3. ���� �ð��� �Ϸ簡 �Ǹ� �ʱ�ȭ
    //4. ���� �ð� PlayerPrefs�� ���� �ҷ�����

    public void SaveShutdownTime()
    {
        PlayerPrefs.SetInt(_playTime, (int)_shutdownSlider.value);
    }

    public void LoadShutdownTime()
    {
        Debug.Log(PlayerPrefs.GetInt(_playTime));
    }

    //�˴ٿ� ����
    public void StartShutdown()
    {
        //���� �ð� �ҷ�����
        //���� �ð��� 0�� �Ǹ� �� ����
        int limitTime = PlayerPrefs.GetInt(_playTime);
        PlayerPrefs.SetInt(_day, DateTime.Now.Minute);
        Debug.Log(PlayerPrefs.GetInt(_day));

        StartCoroutine(TimeLimit_co(limitTime));
    }
    IEnumerator TimeLimit_co(int limitTime)
    {
        WaitForSeconds seconds = new WaitForSeconds(1f);
        while (limitTime > 0)
        {
            limitTime--;
            Debug.Log(limitTime);
            yield return seconds;
        }
        Debug.Log("���� ����");
        PlayerPrefs.SetInt(_isShutdown, 0);
        _shutdownImg.SetActive(true);

        Application.Quit();
    }
}
