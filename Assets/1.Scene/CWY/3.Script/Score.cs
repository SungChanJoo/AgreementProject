using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Score : MonoBehaviour
{
    //������ �����ӿ��� ���������� ����ؾ� �� ��ұ� ������ ���ٿ��̼��� ���� �̱��� ������ �̿�
    [SerializeField]private PlayMode playMode;

    public static Score Instance = null;
    //Todo : ���Ŀ� ���Ӹ�庰�� �����ϵ��� ���� ���� �ʿ� => ����� ���θ�忩�� 3���� �����ؾ��� (��ɱ����� ������ ������ ���ʿ��� ���� �߻�)
    [SerializeField] private TextMeshProUGUI score_text;
    [SerializeField] private TextMeshProUGUI firstscore_text;
    [SerializeField] private TextMeshProUGUI secondscore_text;
    

    private int count;
    private int firstCount;
    private int secondCount;

    private bool isOk; // �̰����ؼ� �����Ұ� (isOk true�� �׿�����Ʈ���� �ƴϸ� ����.) <- ���߿� ���� 

    //�䱸���� �ǹ�Ÿ�� 
    //�ǹ�Ÿ�� �� �� ������ �ö����� ��������� ������������ (������ ���丸 �������� �Ҳ���)

    // 100 * 95 * (lv 1 > 1.1 lv2 1.2 ...) * step(1~6) * 


    //�̱������� ����
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (playMode == PlayMode.Couple) return;
            Destroy(gameObject);
        }
    }

    //�⺻�� �ʱ�ȭ
    private void Start()
    {
        if (playMode == PlayMode.Solo)
        {
            firstscore_text.text = "0";
        }
        else
        {
            firstscore_text.text = "0";
            secondscore_text.text = "0";
        }
    }
    //---------------------------------------------------------
    public void Get_FirstScore()
    {
        if (true)//playMode == PlayMode.Solo)
        {
            firstCount++;
            firstscore_text.text = firstCount.ToString();
        }
    }

    public void Get_SecondScore()
    {
        if (true)//playMode == PlayMode.Ŀ��)
        {
            secondCount++;
            secondscore_text.text = secondCount.ToString();
        }
    }


}
