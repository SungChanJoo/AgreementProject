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

    public float startTime = 6f; //Todo : ���� ����̿��� �����Ͱ��� �޾ƿͼ� �װ����� ����
    public float duration = 6f;  //Todo : ���͵��� => 
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

    private IEnumerator timeSlider() //���� ���� UI���� �ð������ϸ� ���� �޾ƿ� ��
    {
        float endTime = 0f;
        Color startColor = sliderVar_Image.color; // ���� ����
        while (startTime > endTime)
        {
            //�����̴� max value�� 1�̹Ƿ� ���ѽð��� �°� ������ �� �ֵ��� ���� ���
            float normalizedTime = (startTime - endTime) / duration;
            slider.value = normalizedTime;
            startTime -= Time.deltaTime;
            if (slider.value <= ChangeTime)
            {
                StartCoroutine(ChangeColorOverTime(startColor));
            }
            yield return null;
        }
        slider.value = 0; // ���� �ð��� ���� �����̴��� ���� ���� �ణ�� ������ �����ϱ����� ���� �ѹ��� ���.
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
        TimeSlider.Instance.startTime -= Decreasetime;
    }
}