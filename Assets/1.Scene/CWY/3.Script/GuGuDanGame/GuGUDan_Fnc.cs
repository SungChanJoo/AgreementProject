using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuGUDan_Fnc : MonoBehaviour
{
    //������ ���� ��� ���� ��ũ��Ʈ
    //���� ���� 1��,2���� �������� ���ڰ� �߻��ؼ� �������� ���� 
    //����� ���ߴ� �����ߴ� �����ϰ� ������ O , Ʋ����� X ǥ��?
    //������ ��ư 1 ~ 9 �� ������ �Է� 

    [SerializeField] private Text First_num;  // ù��° ĭ�� �� ����
    [SerializeField] private Text Second_num; // �ι�° ĭ�� �� ����.
    [SerializeField] private Text Answer_num; // ������ �Է¹��� ĭ 

    [SerializeField] private Button[] buttons; // 0~9 ��ư 


    bool isStart = false;

    bool isFirst_Click = true;
    bool isSecond_Click = false;

    string buttonText;
    private void Awake()
    {
        First_num.text = "0";
        Second_num.text = "";
        Answer_num.text = "??";
        buttonText = "";
        GameStart();
    }


    //�������� ���� �޼���
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //���� ���۽� ���� ���� �ο�
    private void GameStart()
    {
        //���� ������ �˸�
        isStart = true;
        //ù��° ���� �������� �޾ƿ���
        First_num.text = Random_Num().ToString();
        //�ι�° ���� �������� �޾ƿ���
        Second_num.text = Random_Num().ToString();
    }
    //������ ���������� ���� - ���� - ���Ḧ ���������� �޼��� �а�
    private void GameProgress()
    {
        //ù��° ���� �������� �޾ƿ���
        First_num.text = Random_Num().ToString();
        //�ι�° ���� �������� �޾ƿ���
        Second_num.text = Random_Num().ToString();
        //��� ���� �ʱ�ȭ
        Answer_num.text = "??";
        //ù��° Ŭ�� �ʱ�ȭ
        isFirst_Click = true;
    }

    private void GameOver()
    {
        //Ÿ�ӽ����̴��� Value ���� 0 �ϰ�� ���� ��.
        if(TimeSlider.Instance.slider.value == 0)
        {
            //Todo : ���� ���� ���� �������ּ��� > ����ȭ�� �����ϰ� ���â? ����ٵ�
        }  
    }

    //���� �Ǵ� �Լ�
    public void AnswerCheck()
    {
        //�����ϰ��
        int result = int.Parse(First_num.text) * int.Parse(Second_num.text);
        if (Answer_num.text == $"{result}")
        {
            Score.Instance.Get_Score();
        }
        else //�����ϰ��
        {
            //Todo: ��ȹ������ ��� �� ��ȹ���� �����ָ� �׳� �����ϴ���
            //������ ����� �ð��� �����... 
        }
        GameProgress();
    }

    //num�Ķ���͸� �޾Ƽ� �Է��� ���� �����ϴ� �޼���
    public void Clicked_NumberBtn(int num)
    {
        if (isFirst_Click)
        {
            Answer_num.text = num.ToString();
            isFirst_Click = false;
            isSecond_Click = true;
        }
        else if (isSecond_Click)
        {
            Answer_num.text += num.ToString();
            isSecond_Click = false;
        }        
    }

}
