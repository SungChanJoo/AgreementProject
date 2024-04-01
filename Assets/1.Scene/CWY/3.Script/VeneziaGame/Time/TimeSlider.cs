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

    //1,2�θ���� ��츦 ������ �����̴� ���� 
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
    //1�� �� 2�� �÷��̾� �����̴� �ڷ�ƾ
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

    private IEnumerator timeSlider() //���� ���� UI���� �ð������ϸ� ���� �޾ƿ� ��
    {        
        float endTime = 0f;
        Color startColor = sliderVar_Image.color; // ���� ����
        while (StartTime_ > endTime)
        {
            //�����̴� max value�� 1�̹Ƿ� ���ѽð��� �°� ������ �� �ֵ��� ���� ���
            float normalizedTime = (StartTime_ - endTime) / Duration;
            Slider.value = normalizedTime;
            StartTime_ -= Time;
            if (Slider.value <= changeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor,sliderVar_Image));
            }
            yield return null;
        }
        Slider.value = 0; // ���� �ð��� ���� �����̴��� ���� ���� �ణ�� ������ �����ϱ����� ���� �ѹ��� ���.
    }

    private IEnumerator timeSlider_Second()
    {
        StartTimeTwo = StartTime_;
        float endTime = 0f;
        Color startColor = sliderVar_Image_PlayerTwo.color; // ���� ����
        while (StartTimeTwo > endTime)
        {
            // �����̴� max value�� 1�̹Ƿ� ���ѽð��� �°� ������ �� �ֵ��� ���� ���
            float normalizedTime = (StartTimeTwo - endTime) / Duration;
            Slider_PlayerTwo.value = normalizedTime; // �� �κ��� ����
            StartTimeTwo -= Time;
            if (Slider_PlayerTwo.value <= changeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor, sliderVar_Image_PlayerTwo));
            }
            yield return null;
        }
        Slider_PlayerTwo.value = 0; // ���� �ð��� ���� �����̴��� ���� ���� �ణ�� ������ �����ϱ����� ���� �ѹ��� ���.
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
            elapsedTime += UnityEngine.Time.deltaTime;
            // ���
            yield return null;
        }
        // ���� �Ϸ� �� ���� ���� ����
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
        //�ð� ���� ������ ���� ����
        TimeStop = TimeStop.Equals(true) ? false : true;
        if(GameType == GameType.Solo)
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
        IsGameOver = true;
    }
    //�÷���Ÿ�� üũ
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