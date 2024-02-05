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

    private void Start()
    {
        score_text.text = "0";
    }

    public void Get_Score()
    {
        Count++;
        score_text.text = Count.ToString();
    }



}
