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
    [SerializeField] public Slider Slider;
    [SerializeField] private Image sliderVar_Image;

    [SerializeField] public Slider Slider_PlayerTwo;
    [SerializeField] private Image sliderVar_Image_PlayerTwo;

    [SerializeField] private float changeTime;
    [SerializeField] private float changeDuration;

    public GameType GameType;

    public float Time=0;

    public float PlayTime = 0;

    public bool IsStop = false;
    public bool IsGameOver = false;

    [HideInInspector]public float StartTime_;
    [HideInInspector]public float Duration;

    [HideInInspector] public float StartTimeTwo;

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
            if (GameType == GameType.Couple)
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
        while (StartTime_ > endTime)
        {
            //슬라이더 max value가 1이므로 제한시간에 맞게 감소할 수 있도록 값을 계산
            float normalizedTime = (StartTime_ - endTime) / Duration;
            Slider.value = normalizedTime;
            StartTime_ -= Time;
            if (Slider.value <= changeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor,sliderVar_Image));
            }
            yield return null;
        }
        Slider.value = 0; // 종료 시간에 맞춰 슬라이더의 값을 설정 약간의 오차를 방지하기위해 값을 한번더 명시.
    }

    private IEnumerator timeSlider_Second()
    {
        StartTimeTwo = StartTime_;
        float endTime = 0f;
        Color startColor = sliderVar_Image_PlayerTwo.color; // 시작 색상
        while (StartTimeTwo > endTime)
        {
            // 슬라이더 max value가 1이므로 제한시간에 맞게 감소할 수 있도록 값을 계산
            float normalizedTime = (StartTimeTwo - endTime) / Duration;
            Slider_PlayerTwo.value = normalizedTime; // 이 부분을 수정
            StartTimeTwo -= Time;
            if (Slider_PlayerTwo.value <= changeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor, sliderVar_Image_PlayerTwo));
            }
            yield return null;
        }
        Slider_PlayerTwo.value = 0; // 종료 시간에 맞춰 슬라이더의 값을 설정 약간의 오차를 방지하기위해 값을 한번더 명시.
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
            elapsedTime += UnityEngine.Time.deltaTime;
            // 대기
            yield return null;
        }
        // 변경 완료 후 최종 색상 설정
        image.color = Color.red;
    }   

    public void DecreaseTime()
    {
        Instance.StartTime_ -= Decreasetime;
    }

    public void DecreaseTime_Item(int Decreasetime)
    {
           StartTime_ -= Decreasetime;
            if (StartTime_ < 0)
            {
                StartTime_ = 0;
                Slider.value = 0; 
            }
    }

    public void DecreaseTime_CoupleMode(int Decreasetime, Slider slider_)
    {
        if(slider_ == Slider)
        {
            StartTime_ -= Decreasetime;
            if (StartTime_ < 0)
            { 
                StartTime_ = 0; 
                Slider.value = 0;
            }
        }
        else
        {
            StartTimeTwo -= Decreasetime;
            if(StartTimeTwo < 0)
            {
                StartTimeTwo = 0;
                Slider_PlayerTwo.value = 0;
            }
        }

    }

    public void TimeControl()
    {
        if (!IsStop)
        {
            Time = UnityEngine.Time.deltaTime;
        }
        else
        {
            Time = UnityEngine.Time.unscaledDeltaTime;
        }        
    }
    public void StartTime()
    {
        if(GameType == GameType.Solo)
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
        if(GameType == GameType.Solo)
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
        IsGameOver = true;
    }
    //플레이타임 체크
    public void PlayTimeChecker()
    {
        if (StartTime_ != 0 && !SettingManager.Instance.IsActive)
        {
            PlayTime += UnityEngine.Time.deltaTime;
        }
    }

    public void StopAll()
    {
        StopAllCoroutines();
    }
}