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
        //���߿� �����丵�� �������̽��� ��ü���� �ð� ���� �����ϰ� ���� *
    }
}

    public class TimeSlider : MonoBehaviour, ITimeSlider
{
    public static TimeSlider Instance = null;

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

    private IEnumerator timeSlider() //���� ���� UI���� �ð������ϸ� ���� �޾ƿ� ��
    {        
        float endTime = 0f;
        Color startColor = sliderVar_Image.color; // ���� ����
        while (startTime > endTime)
        {
            //�����̴� max value�� 1�̹Ƿ� ���ѽð��� �°� ������ �� �ֵ��� ���� ���
            float normalizedTime = (startTime - endTime) / duration;
            slider.value = normalizedTime;
            startTime -= time;
            if (slider.value <= ChangeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor,sliderVar_Image));
            }
            yield return null;
        }
        slider.value = 0; // ���� �ð��� ���� �����̴��� ���� ���� �ణ�� ������ �����ϱ����� ���� �ѹ��� ���.
    }

    private IEnumerator timeSlider_Second()
    {
        startTimeTwo = startTime;
        float endTime = 0f;
        Color startColor = sliderVar_Image_PlayerTwo.color; // ���� ����
        while (startTimeTwo > endTime)
        {
            // �����̴� max value�� 1�̹Ƿ� ���ѽð��� �°� ������ �� �ֵ��� ���� ���
            float normalizedTime = (startTimeTwo - endTime) / duration;
            slider_PlayerTwo.value = normalizedTime; // �� �κ��� ����
            startTimeTwo -= time;
            if (slider_PlayerTwo.value <= ChangeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor, sliderVar_Image_PlayerTwo));
            }
            yield return null;
        }
        slider_PlayerTwo.value = 0; // ���� �ð��� ���� �����̴��� ���� ���� �ణ�� ������ �����ϱ����� ���� �ѹ��� ���.
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
            // ���� �ð��� ���� ���� ���� ���
            float t = elapsedTime / changeDuration;

            // �̹��� ���� ���� ����
            // ������� ���� ����
            Color endColor = Color.red;
            Color offset = new Color(0.0005f, -0.0005f, 0); // ���� rgb�� �����༭ �����̴��� ���� ����
            if(image.color != endColor)
            {
                image.color += offset;
            }
            // ��� �ð� ������Ʈ
            elapsedTime += Time.deltaTime;
            // ���
            yield return null;
        }
        // ���� �Ϸ� �� ���� ���� ����
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
        //�ð� ���� ������ ���� ����
        TimeStop = TimeStop.Equals(true) ? false : true;
        if(gameType == GameType.Solo)
        {
            if (!TimeStop && timeSlider_co == null)
            {
                //�ð��� �ٽ� �帣��
                timeSlider_co = timeSlider();
                StartCoroutine(timeSlider_co);
            }
            else
            {
                //����â �� ���ǥ ��½� �ð� ����
                StopCoroutine(timeSlider_co);
                timeSlider_co = null;
            }
        }
        else
        {
            if (!TimeStop && timeSlider_co == null)
            {
                //�ð��� �ٽ� �帣��
                timeSlider_co = timeSlider();
                timeSlider_Second_co = timeSlider_Second();
                StartCoroutine(timeSlider_Second_co);
                StartCoroutine(timeSlider_co);
            }
            else
            {
                //����â �� ���ǥ ��½� �ð� ����
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