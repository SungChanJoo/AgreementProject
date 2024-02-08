using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{
    public static TimeSlider Instance = null;

    [SerializeField] public Slider slider;
    [SerializeField] private Image sliderVar_Image;
    [SerializeField] private float ChangeTime;
    [SerializeField] private float changeDuration;

    public float startTime = 6f; //Todo : 추후 재백이에게 데이터값을 받아와서 그값으로 변경
    public float duration = 6f;  //Todo : 위와동일 => 
    public float Decreasetime;

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
        float endTime = 0f;
        Color startColor = sliderVar_Image.color; // 시작 색상
        while (startTime > endTime)
        {
            //슬라이더 max value가 1이므로 제한시간에 맞게 감소할 수 있도록 값을 계산
            float normalizedTime = (startTime - endTime) / duration;
            slider.value = normalizedTime;
            startTime -= Time.deltaTime;
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
        StartCoroutine(timeSlider());
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
        TimeSlider.Instance.startTime -= Decreasetime;
    }
}