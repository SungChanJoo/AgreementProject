using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum GameType
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
    [SerializeField]private GameType gameType;
    public static TimeSlider Instance = null;

    [SerializeField] public Slider slider;
    [SerializeField] private Image sliderVar_Image;
    [SerializeField] private float ChangeTime;
    [SerializeField] private float changeDuration;

    public float time=0;

    public float PlayTime = 0;

    public bool isStop = false;
    public bool isGameOver = false;

    [HideInInspector]public float startTime; //Todo : ���� ����̿��� �����Ͱ��� �޾ƿͼ� �װ����� ����
    [HideInInspector]public float duration;  //Todo : ���͵��� => 
    public float Decreasetime;

    public bool TimeStop = true;

    IEnumerator timeSlider_co;
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
                StartCoroutine(ChangeColorOverTime(startColor));
            }
            yield return null;
        }
        slider.value = 0; // ���� �ð��� ���� �����̴��� ���� ���� �ణ�� ������ �����ϱ����� ���� �ѹ��� ���.
    }


    private void Update()
    {
        TimeControl();
        PlayTimeChecker();
    }

    private IEnumerator ChangeColorOverTime(Color startColor)
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
            if(sliderVar_Image.color != endColor)
            {
                sliderVar_Image.color += offset;
            }
            // ��� �ð� ������Ʈ
            elapsedTime += Time.deltaTime;
            // ���
            yield return null;
        }
        // ���� �Ϸ� �� ���� ���� ����
        sliderVar_Image.color = Color.red;
    }   

    public void DecreaseTime()
    {
        Instance.startTime -= Decreasetime;
    }

    public void DecreaseTime_Item(int Decreasetime)
    {
        startTime -= Decreasetime;
        if(startTime < 0)
        {
            startTime = 0;
            slider.value = 0;
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
        if (startTime>0)
        {
            
        }
    }
    public void StartTime()
    {
        timeSlider_co = timeSlider();
        StartCoroutine(timeSlider_co);
    }
    public void TimeSliderControll()
    {        
        //�ð� ���� ������ ���� ����
        TimeStop = TimeStop.Equals(true) ? false : true;
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
    public void GameOver()
    {
        isGameOver = true;
    }

    public void PlayTimeChecker()
    {
        if (startTime != 0)
        {
            PlayTime += Time.deltaTime;
        }
    }
}