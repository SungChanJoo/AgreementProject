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

    [SerializeField] Canvas canvas;

    public int Level;
    public int Step;

    //���� ���� ���� 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isThird_Click = false;
    bool isGameOver = false;

    //������ �ڸ��� Ȯ��
    int FistNumDigit;
    int SecondNumDigit;
    int AnswerNumDigit;
    //���� �ڸ����� �°� �Է��ߴ��� Ȯ���ϴ� ��ġ
    int Click_Count;

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

    //���Ǻ� ���̽� ������ ��ȣ Ȯ��
    int CaseNum;


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

        GameOver();

        Click();
    }


    //�������� ���� �޼���
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //ǥ���� �ִ� ��ġ�� �������� ǥ�� ���� �޼���
    // ���� 1 ~ 3  / 4 ~ 6 ������ ���̵��� ������
    private void Random_ShowText(Text First, Text Second, Text Answer)
    {
        //���� ���� => x , y�� �������� ���� 
        //�� , UI�� ǥ���ϴ� ���� ���� * ���� �ý����� �־���� �ش� �޼���� ǥ�� ������ ����
        // 1. x �� ������� ��� ?? x Y = z  
        // 2. Y�� ������� ��� X x ?? = z
        // 3. z�� ������� ��� X x Y = ??


    }
    #region Level , Step�� ���� ���� �޼��� 
    private void Lv1_RandomNum()
    {
        if (Step < 4)
        {
            First_num.text = Random.Range(2, 7).ToString();
            Second_num.text = Random.Range(1, 10).ToString();
        }
        else
        {
            First_num.text = Random.Range(2, 10).ToString();
            Second_num.text = Random.Range(1, 10).ToString();
        }
    }
    //lv1���� ���� ����
    private void Lv1()
    {
        Lv1_RandomNum();
        //lv 1 > 2~9�ܱ���
        //����1~3

        //�ý�Ʈ�� ��ġ�� �������� �ٲ��� �� ������ �Ǵ�
        int First = int.Parse(First_num.text);
        int Second = int.Parse(Second_num.text);
        int result = int.Parse(First_num.text) * int.Parse(Second_num.text);
        switch (Random.Range(0, 2))
        {
            case 0: // x ���� ����
                First_num.text = "?";
                Second_num.text = Second.ToString();
                Answer_num.text = result.ToString();
                CaseNum = 0;
                break;
            case 1: // y���� ����
                First_num.text = First.ToString();
                Second_num.text = "?";
                Answer_num.text = result.ToString();
                CaseNum = 1;
                break;
            case 2: // �⺻ ����
                First_num.text = First.ToString();
                Second_num.text = Second.ToString();
                Answer_num.text = "??";
                CaseNum = 2;
                break;
            default:
                break;
        }

    }

    #endregion
    //

    //���� ���۽� ���� ���� �ο�
    //���� Level , Step�� ���� ������ ���� ����
    private void GameStart()
    {
        //���� ������ �˸�
        isStart = true;
        switch (Level)
        {
            case 1: //���� 1 ����
                Lv1();
                break;
            case 2: //���� 2 ����
                break;
            case 3: //���� 3 ����

                break;
            default:
                break;
        }
    }
    //������ ���������� ���� - ���� - ���Ḧ ���������� �޼��� �а�
    private void GameProgress()
    {
        //������ �����쿡�� ���� �ٽû���
        if (isAnswerCorrect)
        {
            switch (Level)
            {
                case 1:
                    Lv1();
                    break;
                case 2:
                    //Lv2();
                    break;
                case 3:
                    //Lv3()
                    break;
                default:
                    break;
            }
        }
        //Ŭ�� �ʱ�ȭ
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
        else
        {
            return;
        }  
    }

    //���� �Ǵ� �Լ�
    public void AnswerCheck()
    {
        if (TimeSlider.Instance.slider.value == 0) return; // Ÿ�ӿ����� ����
        //�����ϰ��
        if(CaseNum == 0) //x �� ����
        {
            int result = int.Parse(Answer_num.text) / int.Parse(Second_num.text);
            if (Second_num.text == $"{result}")
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
                // �����ϰ�� ���ѽð����� 
                //������ �ð� ������
                isAnswerCheck = true;
                isAnswerCorrect = false;
            }
        }
        else if(CaseNum == 1) // y���� ����
        {
            int result = int.Parse(Answer_num.text) / int.Parse(First_num.text);
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
                // �����ϰ�� ���ѽð����� 
                //������ �ð� ������
                isAnswerCheck = true;
                isAnswerCorrect = false;
            }
        }
        else if (CaseNum == 2)
        {
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
                // �����ϰ�� ���ѽð����� 
                //������ �ð� ������
                isAnswerCheck = true;
                isAnswerCorrect = false;
            }
        }


        if (!isGameOver) GameProgress();
    }

    //num�Ķ���͸� �޾Ƽ� �Է��� ���� �����ϴ� �޼���
    public void Clicked_NumberBtn(int num)
    {
        //Todo : ���ҵ� ��ġ�� �������� ������ ���� �� �� �ֵ��� ������ �����ؾ���
        if (isGameOver) return; //���� ����� �Է� ����
        Check_AnswerNumDigit(); // ó�� Ŭ�� ���� �� 

        switch (CaseNum)
        {
            case 0:
                if (isFirst_Click)
                {
                    First_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    First_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    First_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                break;

            default:
                break;
        }
        //������ �ڸ����� Ŭ���� Ƚ���� ��ġ�ϸ� ����äũ �� Ŭ�� ī��Ʈ �ʱ�ȭ
        if (AnswerNumDigit == Click_Count)
        {
            AnswerCheck();
            Click_Count = 0;
        }

    }

    private void Check_AnswerNumDigit()
    {
        //������ ���� �Ǿ����� ������ �ڸ����� �޾ƿ���
        //������ 1�ڸ���� 1�� Ŭ�� , 2�ڸ���� 2��Ŭ�� 3�ڸ���� 3��Ŭ���� ���� �̿��Ұ�.
        int AnswerNum = int.Parse(First_num.text) * int.Parse(Second_num.text);
        if(AnswerNum > 99)
        {
            AnswerNumDigit = 3;
        }
        else if(AnswerNum < 100 && AnswerNum > 9)
        {
            AnswerNumDigit = 2;
        }
        else if (AnswerNum < 10)
        {
            AnswerNumDigit = 1;
        }

        FistNumDigit = (int.Parse(First_num.text) > 9) ? 2 : 1;
        SecondNumDigit = (int.Parse(Second_num.text) > 9) ? 2 : 1;
    }

    public void Clear_btn()
    {
        Answer_num.text = "??";
        isFirst_Click = true;
        isSecond_Click = false;
        Click_Count = 0;
    }



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

    //��ư�� �ƴ� �ٸ� ������ Ŭ���ϸ� �ʱ�ȭ �����ֱ�
    private void Click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //���� ����ȭ�鿡 ���콺(�հ�����ġ)�� �Է½� �� ��ǥ�� ���
            Vector2 mousePosition = Input.mousePosition;
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>(); // YourCanvas���� ĵ���� GameObject�� �Ҵ�
            Vector2 canvasSize = canvasRectTransform.sizeDelta;
            Vector2 canvasPosition = canvasRectTransform.position;
            Vector2 canvasMousePosition = new Vector2(mousePosition.x / Screen.width * canvasSize.x, mousePosition.y / Screen.height * canvasSize.y) - (canvasSize / 2f);
            print(mousePosition.y / Screen.height * canvasSize.y);
            //�� ���� ���� Ŭ���� Clear
            if (mousePosition.y / Screen.height * canvasSize.y > 255f) Clear_btn();
        }
    }

}
