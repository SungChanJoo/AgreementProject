using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType
{
    Solo,
    Couple
}
public interface ITimeSlider
{
    void DecreaseTime()
    {
        //나중에 리팩토링시 인터페이스로 전체게임 시간 관리 가능하게 변경 *
    }
}

    public class TimeSlider : MonoBehaviour, ITimeSlider
{
    public static TimeSlider Instance = null;

    //1,2인모드일 경우를 나눠서 슬라이더 지정 
    [SerializeField] public Slider slider;
    [SerializeField] private Image sliderVar_Image;

    [SerializeField] public Slider slider_PlayerTwo;
    [SerializeField] private Image sliderVar_Image_PlayerTwo;

    [SerializeField] private float ChangeTime;
    [SerializeField] private float changeDuration;

    public GameType gameType;

    public float time=0;

    public float PlayTime = 0;

    public bool isStop = false;
    public bool isGameOver = false;

    [HideInInspector]public float startTime;
    [HideInInspector]public float duration;

    [HideInInspector] public float startTimeTwo;

    public float Decreasetime;

    public bool TimeStop = true;
    //1번 및 2번 플레이어 슬라이더 코루틴
    IEnumerator timeSlider_co;
    IEnumerator timeSlider_Second_co;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (gameType == GameType.Couple)
            {
                return;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator timeSlider() //추후 메인 UI에서 시간설정하면 값을 받아올 것
    {        
        float endTime = 0f;
        Color startColor = sliderVar_Image.color; // 시작 색상
        while (startTime > endTime)
        {
            //슬라이더 max value가 1이므로 제한시간에 맞게 감소할 수 있도록 값을 계산
            float normalizedTime = (startTime - endTime) / duration;
            slider.value = normalizedTime;
            startTime -= time;
            if (slider.value <= ChangeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor,sliderVar_Image));
            }
            yield return null;
        }
        slider.value = 0; // 종료 시간에 맞춰 슬라이더의 값을 설정 약간의 오차를 방지하기위해 값을 한번더 명시.
    }

    private IEnumerator timeSlider_Second()
    {
        startTimeTwo = startTime;
        float endTime = 0f;
        Color startColor = sliderVar_Image_PlayerTwo.color; // 시작 색상
        while (startTimeTwo > endTime)
        {
            // 슬라이더 max value가 1이므로 제한시간에 맞게 감소할 수 있도록 값을 계산
            float normalizedTime = (startTimeTwo - endTime) / duration;
            slider_PlayerTwo.value = normalizedTime; // 이 부분을 수정
            startTimeTwo -= time;
            if (slider_PlayerTwo.value <= ChangeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor, sliderVar_Image_PlayerTwo));
            }
            yield return null;
        }
        slider_PlayerTwo.value = 0; // 종료 시간에 맞춰 슬라이더의 값을 설정 약간의 오차를 방지하기위해 값을 한번더 명시.
    }


    private void Update()
    {
        TimeControl();
        PlayTimeChecker();
    }

    private IEnumerator ChangeColorOverTime(Color startColor, Image image)
    {
        float elapsedTime = 0f;

        while (elapsedTime < changeDuration)
        {
            // 현재 시간에 따른 보간 비율 계산
            float t = elapsedTime / changeDuration;

            // 이미지 색상 설정 변경
            // 종료시점 색상 지정
            Color endColor = Color.red;
            Color offset = new Color(0.0005f, -0.0005f, 0); // 직접 rgb값 더해줘서 슬라이더바 색상 변경
            if(image.color != endColor)
            {
                image.color += offset;
            }
            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;
            // 대기
            yield return null;
        }
        // 변경 완료 후 최종 색상 설정
        image.color = Color.red;
    }   

    public void DecreaseTime()
    {
        Instance.startTime -= Decreasetime;
    }

    public void DecreaseTime_Item(int Decreasetime)
    {
           startTime -= Decreasetime;
            if (startTime < 0)
            {
                startTime = 0;
                slider.value = 0; 
            }
    }

    public void DecreaseTime_CoupleMode(int Decreasetime, Slider slider_)
    {
        if(slider_ == slider)
        {
            startTime -= Decreasetime;
            if (startTime < 0)
            { 
                startTime = 0; 
                slider.value = 0;
            }
        }
        else
        {
            startTimeTwo -= Decreasetime;
            if(startTimeTwo < 0)
            {
                startTimeTwo = 0;
                slider_PlayerTwo.value = 0;
            }
        }

    }

    public void TimeControl()
    {
        if (!isStop)
        {
            time = Time.deltaTime;
        }
        else
        {
            time = Time.unscaledDeltaTime;
        }        
    }
    public void StartTime()
    {
        if(gameType == GameType.Solo)
        {
            timeSlider_co = timeSlider();
            StartCoroutine(timeSlider_co);
        }
        else
        {
            timeSlider_co = timeSlider();
            timeSlider_Second_co = timeSlider_Second();
            StartCoroutine(timeSlider_co);
            StartCoroutine(timeSlider_Second_co);
        }

    }
    public void TimeSliderControll()
    {        
        //시간 설정 변경을 위한 연산
        TimeStop = TimeStop.Equals(true) ? false : true;
        if(gameType == GameType.Solo)
        {
            if (!TimeStop && timeSlider_co == null)
            {
                //시간을 다시 흐르게
                timeSlider_co = timeSlider();
                StartCoroutine(timeSlider_co);
            }
            else
            {
                //설정창 및 결과표 출력시 시간 정지
                StopCoroutine(timeSlider_co);
                timeSlider_co = null;
            }
        }
        else
        {
            if (!TimeStop && timeSlider_co == null)
            {
                //시간을 다시 흐르게
                timeSlider_co = timeSlider();
                timeSlider_Second_co = timeSlider_Second();
                StartCoroutine(timeSlider_Second_co);
                StartCoroutine(timeSlider_co);
            }
            else
            {
                //설정창 및 결과표 출력시 시간 정지
                StopCoroutine(timeSlider_co);
                StopCoroutine(timeSlider_Second_co);
                timeSlider_co = null;
                timeSlider_Second_co = null;
            }
        }

    }
    public void GameOver()
    {
        isGameOver = true;
    }

    public void PlayTimeChecker()
    {
        if (startTime != 0 && !SettingManager.Instance.IsActive)
        {
            PlayTime += Time.deltaTime;
        }
    }

    public void StopAll()
    {
        StopAllCoroutines();
    }
}