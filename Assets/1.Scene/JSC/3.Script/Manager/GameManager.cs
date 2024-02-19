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

        //셧다운 활성화 상태
        if (PlayerPrefs.GetInt(_isShutdown) == 0)
        {
            //셧다운 된 날과 같은 날이라면 셧다운상태
            //todo 0125 분대신 날로 바꾸기
            Debug.Log(DateTime.Now.Minute + " | " + PlayerPrefs.GetInt(_day));
            if (DateTime.Now.Minute == PlayerPrefs.GetInt(_day))
            {
                PlayerPrefs.SetInt(_isShutdown, 0);
                _shutdownImg.SetActive(true);
            }
            //아니면 셧다운 상태 해제
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

    // 셧다운 기능
    //1. 부모가 시간을 설정할 수 있어야함
    //2. 제한 시간이 되면 앱을 종료
    //3. 제한 시간은 하루가 되면 초기화
    //4. 제한 시간 PlayerPrefs로 저장 불러오기

    public void SaveShutdownTime()
    {
        PlayerPrefs.SetInt(_playTime, (int)_shutdownSlider.value);
    }

    public void LoadShutdownTime()
    {
        Debug.Log(PlayerPrefs.GetInt(_playTime));
    }

    //셧다운 시작
    public void StartShutdown()
    {
        //제한 시간 불러오기
        //남은 시간이 0이 되면 앱 종료
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
        Debug.Log("어플 종료");
        PlayerPrefs.SetInt(_isShutdown, 0);
        _shutdownImg.SetActive(true);

        Application.Quit();
    }
}
