using UnityEngine;
using UnityEngine.UI;
using TMPro;

//�߰��ؾ��Ұ� ������ ���� ���� ���� �� ����� �����

enum ButtonType
{
    First,
    Second
}

public class GuGUDanFnc : GameSetting
{
    [SerializeField] private ButtonType buttonType;
    //������ ���� ��� ���� ��ũ��Ʈ
    //���� ���� 1��,2���� �������� ���ڰ� �߻��ؼ� �������� ���� 
    //����� ���ߴ� �����ߴ� �����ϰ� ������ O , Ʋ����� X ǥ��?
    //������ ��ư 1 ~ 9 �� ������ �Է� 

    //������� 
    //clear ��ư �߰� �� 0~9��ư�� ������ ������ ���������� clear ȿ�� �߻��ؾ��� => ���
    //xyz ���� z��(����)�� ������ �ƴ϶� , �����ɵ� �־ x,y�� �����ϰ� ������ ��찡 �־����
    //2~9���� �ƴ϶� 2~ 19�ܱ��� 
    //level & step �ܰ� �߰�
    //���� ó���� �ð�����

    [SerializeField] private TextMeshProUGUI first_num;  // ù��° ĭ�� �� ����
    [SerializeField] private TextMeshProUGUI second_num; // �ι�° ĭ�� �� ����.
    [SerializeField] private TextMeshProUGUI answer_num; // ������ �Է¹��� ĭ 

    [SerializeField] private Button[] buttons; // 0~9 ��ư 

    [SerializeField] private GameObject[] select;
    [SerializeField] private GameObject[] questMark;
    [SerializeField] private Animator anim;
    [SerializeField] Canvas canvas;

    [SerializeField] private AudioClip[] audioClip;
    #region ���� & ���� ����
    public int Level;
    public int Step;

    //���� ���� ���� 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isThird_Click = false;
    bool isGameOver = false;
    bool isInputValue = false;

    //���� ���� & �ʱ� ���� ���� ���� ���尪 & ���� �Է� �õ� �� Ƚ��
    int QuestCount;
    int StartQuestCount;
    int AnswerCount; //������ �Է��� �� Ƚ��
    //������ �ڸ��� Ȯ��
    int FirstNumDigit;
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

    //���� �����ӵ�
    float ReactionTime;

    string buttonText;



    //���Ǻ� ���̽� ������ ��ȣ Ȯ��
    int CaseNum;
    #endregion

    private void Awake()
    {
        first_num.text = "0";
        second_num.text = "";
        answer_num.text = "??";
        buttonText = "";

    }

    private void Update()
    {
        if (!isGameOver)
        {
            Reaction_speed();
        }

        GameOver();
 
        Select_onOff(CaseNum);
        QuestMark_onOff(CaseNum);
        Click();
        
    }
    protected override void startGame()
    {
        base.startGame();
    }

    //�������� ���� �޼���
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //ǥ���� �ִ� ��ġ�� �������� ǥ�� ���� �޼���
    // ���� 1 ~ 3  / 4 ~ 6 ������ ���̵��� ������
    private void Random_ShowText()
    {
        //���� ���� => x , y�� �������� ���� 
        //�� , UI�� ǥ���ϴ� ���� ���� * ���� �ý����� �־���� �ش� �޼���� ǥ�� ������ ����
        // 1. x �� ������� ��� ?? x Y = z  
        // 2. Y�� ������� ��� X x ?? = z
        // 3. z�� ������� ��� X x Y = ??
        int First = int.Parse(first_num.text);
        int Second = int.Parse(second_num.text);
        int result = int.Parse(first_num.text) * int.Parse(second_num.text);

        //������ �� ���� ��ġ �������� ����.
        switch (Random.Range(0, 3))
        {
            case 0: // x ���� ����
                first_num.text = "?";
                second_num.text = Second.ToString();
                answer_num.text = result.ToString();
                CaseNum = 0;
                break;
            case 1: // y���� ����
                first_num.text = First.ToString();
                second_num.text = "?";
                answer_num.text = result.ToString();
                CaseNum = 1;
                break;
            case 2: // �⺻ ����
                first_num.text = First.ToString();
                second_num.text = Second.ToString();
                answer_num.text = "??";
                CaseNum = 2;
                break;
            default:
                break;
        }
        //Select_onOff(CaseNum);
    }
    #region Level , Step�� ���� ���� �޼��� 
    private void Lv1_RandomNum(int step)
    {
        if (step < 4)
        {
            first_num.text = Random.Range(2, 7).ToString();
            second_num.text = Random.Range(1, 10).ToString();
        }
        else
        {
            first_num.text = Random.Range(2, 10).ToString();
            second_num.text = Random.Range(1, 10).ToString();
        }
    }
    private void Lv2_RandomNum(int step)
    {
        if (step < 4)
        {
            first_num.text = Random.Range(2, 10).ToString();
            second_num.text = Random.Range(1, 15).ToString();
        }
        else
        {
            first_num.text = Random.Range(2, 10).ToString();
            second_num.text = Random.Range(1, 20).ToString();
        }

    }
    private void Lv3_RandomNum(int step)
    {
        if (step < 4)
        {
            first_num.text = Random.Range(2, 15).ToString();
            second_num.text = Random.Range(1, 20).ToString();
        }
        else
        {
            first_num.text = Random.Range(2, 20).ToString();
            second_num.text = Random.Range(1, 20).ToString();
        }
    }
    #region lv�� ����
    //lv�� ����
    protected override void Level_1(int step)
    {
        GameStart();
        Lv1_RandomNum(step);
        Random_ShowText();
    }
    protected override void Level_2(int step)
    {
        GameStart();
        Lv2_RandomNum(step);
        Random_ShowText();
    }
    protected override void Level_3(int step)
    {
        GameStart();
        Lv3_RandomNum(step);
        Random_ShowText();
    }
    #endregion
    #endregion
    //

    //���� ���۽� ���� ���� �ο�
    //���� Level , Step�� ���� ������ ���� ����
    private void GameStart()
    {
        //���� ������ �˸�
        if (!isStart)
        {
            isStop = false;
            Start_Btn();
            Set_QuestCount();
            QuestCount--;
            isStart = true;
            AnswerCount = 0;
            TimeSlider.Instance.StartTime();
            TimeSlider.Instance.TimeStop = false;
        }


    }
    //������ ���������� ���� - ���� - ���Ḧ ���������� �޼��� �а�
    private void GameProgress()
    {
        //������ �����쿡�� ���� �ٽû���
        isInputValue = false;
        if (isAnswerCorrect)
        {
            QuestCount--;
            SplitLevelAndStep();
        }
        //Ŭ�� �ʱ�ȭ
        isFirst_Click = true;

        //����Ȯ�� �ʱ�ȭ
        isAnswerCheck = false;
    }

    //��������
    private void GameOver()
    {
        //Ÿ�ӽ����̴��� Value ���� 0 �ϰ�� ���� ��.
        if (TimeSlider.Instance.Slider.value == 0)
        {
            ReactionTime = trueReactionTime / TrueAnswerCount;
            totalReactionTime = 0;
            if (!isGameOver)
            {
                isGameOver = true;
                answersCount = TrueAnswerCount;
                totalQuestions = StartQuestCount;
                reactionRate = ReactionTime;
                AnswerRate();
                EndGame();
            }
        }
        else
        {
            if (QuestCount < 0 && !isGameOver)
            {
                ReactionTime = trueReactionTime / TrueAnswerCount;
                isGameOver = true;
                answersCount = TrueAnswerCount;
                totalQuestions = StartQuestCount;
                reactionRate = ReactionTime;
                AnswerRate();
                EndGame();
            }
        }
    }

    //���� �Ǵ� �Լ�
    public void AnswerCheck()
    {
        if (TimeSlider.Instance.Slider.value == 0) return; // Ÿ�ӿ����� ����
        //�����ϰ��
        int resultx = int.Parse(answer_num.text) / int.Parse(second_num.text);
        int resulty = int.Parse(answer_num.text) / int.Parse(first_num.text);
        int resultz = int.Parse(first_num.text) * int.Parse(second_num.text);
        if (CaseNum == 0) //x �� ����
        {
            if (first_num.text == $"{resultx}")
            {
                //Get_Score();
                TrueAnswerCount++; //���� ���� ���� -> ����� �ݿ��� ����Ұ�
                isAnswerCheck = true;
                isAnswerCorrect = true;
                AnswerCount++;
                //�����ð� ������ ����.
                source.PlayOneShot(audioClip[0]);
            }
            else //�����ϰ��
            {
                //Todo: ��ȹ������ ��� �� ��ȹ���� �����ָ� �׳� �����ϴ���
                // �����ϰ�� ���ѽð����� 
                //������ �ð� ������
                AnswerCount++;
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
                source.PlayOneShot(audioClip[1]);
            }
        }
        else if (CaseNum == 1) // y���� ����
        {
            if (second_num.text == $"{resulty}")
            {
                //Get_Score();
                TrueAnswerCount++; //���� ���� ���� -> ����� �ݿ��� ����Ұ�
                isAnswerCheck = true;
                isAnswerCorrect = true;
                AnswerCount++;
                source.PlayOneShot(audioClip[0]);
                //�����ð� ������ ����.
            }
            else //�����ϰ��
            {
                //Todo: ��ȹ������ ��� �� ��ȹ���� �����ָ� �׳� �����ϴ���
                // �����ϰ�� ���ѽð����� 
                //������ �ð� ������
                isAnswerCheck = true;
                isAnswerCorrect = false;
                AnswerCount++;
                TimeSlider.Instance.DecreaseTime_Item(5);
                source.PlayOneShot(audioClip[1]);
            }
        }
        else if (CaseNum == 2)
        {
            if (answer_num.text == $"{resultz}")
            {
                //Get_Score();
                TrueAnswerCount++; //���� ���� ���� -> ����� �ݿ��� ����Ұ�
                AnswerCount++;
                isAnswerCheck = true;
                isAnswerCorrect = true;
                source.PlayOneShot(audioClip[0]);
                //�����ð� ������ ����.
            }
            else //�����ϰ��
            {
                //Todo: ��ȹ������ ��� �� ��ȹ���� �����ָ� �׳� �����ϴ���
                // �����ϰ�� ���ѽð����� 
                //������ �ð� ������
                AnswerCount++;
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
                source.PlayOneShot(audioClip[1]);
            }
        }

        //�ʱ�ȭ �ʿ�
        Clear_btn();
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
            case 0: //x ĭ�� ?? �� ��µǴ� ��� Ŭ���� x�ʿ� ���� �Էµǰ�, �ڸ����� �°� ���� �Է� �ؾ� ��.
                    //19�� ������ �����ϱ� ������ x ,y �� �ִ� 2�ڸ�  z�� 3�ڸ����� üũ. ����, �Էµ� �ִ� 3��(19x19�� �ִ� 3�ڸ�)
                if (isFirst_Click)
                {
                    isInputValue = true;
                    first_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    first_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                break;
            case 1:
                if (isFirst_Click)
                {
                    isInputValue = true;
                    second_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    second_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                break;
            case 2:
                if (isFirst_Click)
                {
                    isInputValue = true;
                    answer_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    answer_num.text += num.ToString();
                    isSecond_Click = false;
                    isThird_Click = true;
                    Click_Count++;
                }
                else if (isThird_Click)
                {
                    answer_num.text += num.ToString();
                    isThird_Click = false;
                    Click_Count++;
                }
                break;
            default:
                break;
        }


        //������ �ڸ����� Ŭ���� Ƚ���� ��ġ�ϸ� ����äũ �� Ŭ�� ī��Ʈ �ʱ�ȭ
        switch (CaseNum)
        {
            case 0:
                if (FirstNumDigit == Click_Count) AnswerCheck();
                break;
            case 1:
                if (SecondNumDigit == Click_Count) AnswerCheck();
                break;
            case 2:
                if (AnswerNumDigit == Click_Count) AnswerCheck();
                break;
            default:
                break;
        }

    }

    //���� �Է½� �ڸ��� üũ
    private void Check_AnswerNumDigit()
    {
        //������ ���� �Ǿ����� ������ �ڸ����� �޾ƿ���
        //������ 1�ڸ���� 1�� Ŭ�� , 2�ڸ���� 2��Ŭ�� 3�ڸ���� 3��Ŭ���� ���� �̿��Ұ�.
        //������ �ڸ��� x y z �� ��츦 �����ؾ���
        switch (CaseNum)
        {
            case 0: //x�� ���� �ִ� �ڸ���
                int firstnum = int.Parse(answer_num.text) / int.Parse(second_num.text);
                FirstNumDigit = firstnum.ToString().Length;
                break;
            case 1: //y�� ���� �ִ� �ڸ���
                int secondnum = int.Parse(answer_num.text) / int.Parse(first_num.text);
                SecondNumDigit = secondnum.ToString().Length;
                break;
            case 2: //z�� ���� �ִ� �ڸ���
                int AnswerNum = int.Parse(first_num.text) * int.Parse(second_num.text);
                AnswerNumDigit = AnswerNum.ToString().Length;
                break;

            default:
                break;
        }
    }

    //�ʱ�ȭ��ư
    public void Clear_btn()
    {
        switch (CaseNum)
        {
            case 0:
                first_num.text = "?";
                break;
            case 1:
                second_num.text = "?";
                break;
            case 2:
                answer_num.text = "??";
                break;
            default:
                break;
        }
        isFirst_Click = true;
        isSecond_Click = false;
        isThird_Click = false;
        Click_Count = 0;
        isInputValue = false;
        
    }



    //�����ӵ�
    public void Reaction_speed()
    {
        // ���ӽð��� ���� �� ���� ������ ��� ������� �����ӵ� ���.
        totalReactionTime += Time.deltaTime; // �ڷ�ƾXXXX
        if (isAnswerCorrect)
        {
            trueReactionTime += totalReactionTime;
        }
        totalReactionTime = 0;
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
            //print(mousePosition.y / Screen.height * canvasSize.y);
            //�� ���� ���� Ŭ���� Clear
            //if (mousePosition.y / Screen.height * canvasSize.y > 255f) Clear_btn();
        }
    }
    private void Set_QuestCount()
    {
        switch (timeSet)
        {
            case 60:
                QuestCount = 15;
                break;
            case 180:
                QuestCount = 45; // ����
                break;
            case 300:
                QuestCount = 75; // ���� ��
                break;
            default:
                break;
        }
        StartQuestCount = QuestCount;
    }

    //�����
    private void AnswerRate()
    {
        answers = TrueAnswerCount * 100 / StartQuestCount;
    }

    //����
    private void Select_onOff(int caseNum)
    {
        for (int i = 0; i < select.Length; i++)
        {
            if(i == caseNum)
            {
                select[i].SetActive(true);
            }
            else
            {
                select[i].SetActive(false);
            }
        }
    }

    // UI ? on/off
    private void QuestMark_onOff(int caseNum)
    {
        for (int i = 0; i < questMark.Length; i++)
        {
            if (i == caseNum && !isInputValue)
            {
                questMark[i].SetActive(true);
            }
            else
            {
                questMark[i].SetActive(false);
            }
        }
    }
    public void ClearBtn_SFX_Solo()
    {
        source.PlayOneShot(audioClip[2]);
    }
    public void ClearBtn_SFX_Two()
    {
        source.PlayOneShot(audioClip[2]);
    }

}

