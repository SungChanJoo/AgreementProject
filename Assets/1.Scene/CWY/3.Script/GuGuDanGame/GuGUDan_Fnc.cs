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



    //���� ���� ���� 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isGameOver = false;

    //Ȯ�ι�ư�� �������� �ȴ������� Ȯ��
    bool isAnswerCheck = false;
    //Ȯ�ι�ư�� �������� �������� �ƴ��� Ȯ��
    bool isAnswerCorrect = false;

    //���� ���� ����
    private int TrueAnswerCount = 0;
    //���� ���� �� �����ӵ� �ð� ��� ���丸
    float trueReactionTime = 0f; //������ ���������� �����ӵ�
    float totalReactionTime = 0f;
    string buttonText;
    private void Awake()
    {
        First_num.text = "0";
        Second_num.text = "";
        Answer_num.text = "??";
        buttonText = "";
        
    }

    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            Reaction_speed();
        }
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

        //����Ȯ�� �ʱ�ȭ
        isAnswerCheck = false;
    }

    private void GameOver()
    {
        //Ÿ�ӽ����̴��� Value ���� 0 �ϰ�� ���� ��.
        if(TimeSlider.Instance.slider.value == 0)
        {
            //Todo : ���� ���� ���� �������ּ��� > ����ȭ�� �����ϰ� ���â? ����ٵ�
            //�����ӵ� �n
            print(trueReactionTime / TrueAnswerCount);
            isGameOver = true;
            totalReactionTime = 0;
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
            TrueAnswerCount++; //���� ���� ���� -> ����� �ݿ��� ����Ұ�
            isAnswerCheck = true;
            isAnswerCorrect = true;
            //�����ð� ������ ����.
        }
        else //�����ϰ��
        {
            //Todo: ��ȹ������ ��� �� ��ȹ���� �����ָ� �׳� �����ϴ���
            //������ ����� �ð��� �����... 
            //������ �ð� ������
            isAnswerCheck = true;
            isAnswerCorrect = false;
        }
        if(!isGameOver) GameProgress();
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



    //�����̰� ��Ų �����ӵ� ���...
    //�켱.. ���丸 �������� ���....
    //���� ���� -> �ð� �ȵ��ȵ� -> ������ ���߸� �� �ð��� ����
    // ex) 6���� ���߸� ������ ������� �ӵ��� �� �� �����ٰ�.. 

    public void Reaction_speed()
    {
        // ���ӽð��� ���� �� ���� ������ ��� ������� �����ӵ� ���.
        totalReactionTime += Time.deltaTime; // �ڷ�ƾXXXX
     //   trueReactionTime = (isAnswerCorrect) ? (trueReactionTime += totalReactionTime) : (trueReactionTime += 0); // ������ ���� ��쿡�� �ð��� �����ٰ� �ƴѰ�쿡�� 0������(�����ӵ��� ����x)
        if(isAnswerCorrect)
        {
            trueReactionTime += totalReactionTime;
            totalReactionTime = 0;
        }
        else if (!isAnswerCorrect)
        {
            totalReactionTime = 0;
        }
    }
}
