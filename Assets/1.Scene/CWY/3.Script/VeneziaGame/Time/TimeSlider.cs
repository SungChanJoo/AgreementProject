using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] public Slider slider;
    [SerializeField] private Image sliderVar_Image;
    [SerializeField] private float ChangeTime;
    [SerializeField] private float changeDuration;

    public float time=0;

    public bool isStop = false;
    public bool isGameOver = false;

    public float startTime = 6f; //Todo : 추후 재백이에게 데이터값을 받아와서 그값으로 변경
    public float duration = 6f;  //Todo : 위와동일 => 
    public float Decreasetime;

    public bool TimeStop = true;

    IEnumerator timeSlider_co;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator timeSlider() //추후 메인 UI에서 시간설정하면 값을 받아올 것
    {
        Debug.Log("코루틴실행중");
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
                StartCoroutine(ChangeColorOverTime(startColor));
            }
            yield return null;
        }
        slider.value = 0; // 종료 시간에 맞춰 슬라이더의 값을 설정 약간의 오차를 방지하기위해 값을 한번더 명시.
    }

    private void Start()
    {
        timeSlider_co = timeSlider();        
    }

    private void Update()
    {
        TimeControl();
    }

    private IEnumerator ChangeColorOverTime(Color startColor)
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
            if(sliderVar_Image.color != endColor)
            {
                sliderVar_Image.color += offset;
            }
            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;
            // 대기
            yield return null;
        }
        // 변경 완료 후 최종 색상 설정
        sliderVar_Image.color = Color.red;
    }   

    public void DecreaseTime()
    {
        Instance.startTime -= Decreasetime;
    }

    public void DecreaseTime_Item(int Decreasetime)
    {
        startTime -= Decreasetime;
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
        Debug.Log(TimeStop);        

    }
    public void StartTime()
    {        
        StartCoroutine(timeSlider_co);
    }
    public void TimeSliderControll()
    {
        //시간 설정 변경을 위한 연산
        TimeStop = TimeStop.Equals(true) ? false : true;
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
    public void GameOver()
    {
        isGameOver = true;
    }
}