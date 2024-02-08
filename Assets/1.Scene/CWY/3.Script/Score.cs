using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    //������ �����ӿ��� ���������� ����ؾ� �� ��ұ� ������ ���ٿ��̼��� ���� �̱��� ������ �̿�

    public static Score Instance = null;
    [SerializeField] private Text score_text;

    private int Count;

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
            Destroy(gameObject);
        }
    }

    //�⺻�� �ʱ�ȭ
    private void Start()
    {
        score_text.text = "0";
    }

    //Todo : ������ ������ ��� ���� ���� > ���� ��ȹ������ �������� ������ �����ָ� �׿� �°� ���� ���� �� ��.
    public void Get_Score()
    {
        Count++;
        score_text.text = Count.ToString();
    }



}
