using UnityEngine;
using UnityEngine.UI;
using TMPro;

//추가해야할것 구구단 문제 출제 갯수 및 정답률 결과값

enum ButtonType
{
    First,
    Second
}

public class GuGUDan_Fnc : GameSetting
{
    [SerializeField] private ButtonType buttonType;
    //구구단 게임 기능 관련 스크립트
    //곱셈 숫자 1번,2번에 랜덤으로 숫자가 발생해서 구구단을 진행 
    //결과는 맞추던 못맞추던 공개하고 맞출경우 O , 틀릴경우 X 표시?
    //정답은 버튼 1 ~ 9 를 눌러서 입력 

    //변경사항 
    //clear 버튼 추가 및 0~9버튼을 제외한 영역을 눌렀을때도 clear 효과 발생해야함
    //xyz 에서 z값(정답)이 고정이 아니라 , 역산기능도 넣어서 x,y도 랜덤하게 정답일 경우가 있어야함
    //2~9단이 아니라 2~ 19단까지 
    //level & step 단계 추가
    //오답 처리시 시간감소

    [SerializeField] private TextMeshProUGUI First_num;  // 첫번째 칸에 들어갈 숫자
    [SerializeField] private TextMeshProUGUI Second_num; // 두번째 칸에 들어갈 숫자.
    [SerializeField] private TextMeshProUGUI Answer_num; // 정답을 입력받을 칸 

    [SerializeField] private Button[] buttons; // 0~9 버튼 

    [SerializeField] private GameObject[] select;
    [SerializeField] private GameObject[] QuestMark;
    [SerializeField] private Animator anim;
    [SerializeField] Canvas canvas;

    #region 변수 & 상태 관리
    public int Level;
    public int Step;

    //게임 진행 순서 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isThird_Click = false;
    bool isGameOver = false;
    bool isInputValue = false;

    //문제 개수 & 초기 문제 출제 개수 저장값 & 정답 입력 시도 총 횟수
    int QuestCount;
    int StartQuestCount;
    int AnswerCount; //정답을 입력한 총 횟수
    //정답의 자릿수 확인
    int FirstNumDigit;
    int SecondNumDigit;
    int AnswerNumDigit;
    //정답 자릿수에 맞게 입력했는지 확인하는 장치
    int Click_Count;

    //확인버튼을 눌렀는지 안눌렀는지 확인
    bool isAnswerCheck = false;
    //확인버튼을 누른것이 정답인지 아닌지 확인
    bool isAnswerCorrect = false;

    //정답 맞춘 갯수
    private int TrueAnswerCount = 0;
    //문제 출제 후 반응속도 시간 재기 정답만
    float trueReactionTime = 0f; //정답을 맞췄을때의 반응속도
    float totalReactionTime = 0f;

    //최종 반응속도
    float ReactionTime;

    string buttonText;



    //스탭별 케이스 선택지 번호 확인
    int CaseNum;
    #endregion

    private void Awake()
    {
        First_num.text = "0";
        Second_num.text = "";
        Answer_num.text = "??";
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

    //랜덤숫자 생성 메서드
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //표기해 주는 위치를 랜덤으로 표시 해줄 메서드
    // 스탭 1 ~ 3  / 4 ~ 6 에서의 난이도는 동일함
    private void Random_ShowText()
    {
        //문제 생성 => x , y를 생성한후 곱셈 
        //단 , UI에 표기하는 것은 랜덤 * 역산 시스템이 있어야함 해당 메서드는 표기 순서만 변경
        // 1. x 가 비어있을 경우 ?? x Y = z  
        // 2. Y가 비어있을 경우 X x ?? = z
        // 3. z가 비어있을 경우 X x Y = ??
        int First = int.Parse(First_num.text);
        int Second = int.Parse(Second_num.text);
        int result = int.Parse(First_num.text) * int.Parse(Second_num.text);

        //정답이 될 숫자 위치 랜덤으로 선택.
        switch (Random.Range(0, 3))
        {
            case 0: // x 쪽을 역산
                First_num.text = "?";
                Second_num.text = Second.ToString();
                Answer_num.text = result.ToString();
                CaseNum = 0;
                break;
            case 1: // y쪽을 역산
                First_num.text = First.ToString();
                Second_num.text = "?";
                Answer_num.text = result.ToString();
                CaseNum = 1;
                break;
            case 2: // 기본 연산
                First_num.text = First.ToString();
                Second_num.text = Second.ToString();
                Answer_num.text = "??";
                CaseNum = 2;
                break;
            default:
                break;
        }
        //Select_onOff(CaseNum);
    }
    #region Level , Step별 문제 제작 메서드 
    private void Lv1_RandomNum(int step)
    {
        if (step < 4)
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
    private void Lv2_RandomNum(int step)
    {
        if (step < 4)
        {
            First_num.text = Random.Range(2, 10).ToString();
            Second_num.text = Random.Range(1, 15).ToString();
        }
        else
        {
            First_num.text = Random.Range(2, 10).ToString();
            Second_num.text = Random.Range(1, 20).ToString();
        }

    }
    private void Lv3_RandomNum(int step)
    {
        if (step < 4)
        {
            First_num.text = Random.Range(2, 15).ToString();
            Second_num.text = Random.Range(1, 20).ToString();
        }
        else
        {
            First_num.text = Random.Range(2, 20).ToString();
            Second_num.text = Random.Range(1, 20).ToString();
        }
    }
    #region lv별 게임
    //lv별 로직
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

    //게임 시작시 랜덤 숫자 부여
    //선택 Level , Step에 따라 숫자의 범위 제한
    private void GameStart()
    {
        //게임 시작을 알림
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
    //로직은 동일하지만 시작 - 진행 - 종료를 나누기위해 메서드 분개
    private void GameProgress()
    {
        //정답을 맞춘경우에는 문제 다시생성
        isInputValue = false;
        if (isAnswerCorrect)
        {
            QuestCount--;
            SplitLevelAndStep();
        }
        //클릭 초기화
        isFirst_Click = true;

        //정답확인 초기화
        isAnswerCheck = false;
    }

    private void GameOver()
    {
        //타임슬라이더의 Value 값이 0 일경우 게임 끝.
        if (TimeSlider.Instance.slider.value == 0)
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

    //정답 판단 함수
    public void AnswerCheck()
    {
        if (TimeSlider.Instance.slider.value == 0) return; // 타임오버시 리턴
        //정답일경우
        int resultx = int.Parse(Answer_num.text) / int.Parse(Second_num.text);
        int resulty = int.Parse(Answer_num.text) / int.Parse(First_num.text);
        int resultz = int.Parse(First_num.text) * int.Parse(Second_num.text);
        if (CaseNum == 0) //x 쪽 역산
        {
            if (First_num.text == $"{resultx}")
            {
                Get_Score();
                TrueAnswerCount++; //정답 갯수 증가 -> 정답률 반영에 사용할거
                isAnswerCheck = true;
                isAnswerCorrect = true;
                AnswerCount++;
                //누적시간 변수에 저장.
            }
            else //오답일경우
            {
                //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
                // 오답일경우 제한시간감소 
                //누적된 시간 날리기
                AnswerCount++;
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
            }
        }
        else if (CaseNum == 1) // y쪽을 역산
        {
            if (Second_num.text == $"{resulty}")
            {
                Get_Score();
                TrueAnswerCount++; //정답 갯수 증가 -> 정답률 반영에 사용할거
                isAnswerCheck = true;
                isAnswerCorrect = true;
                AnswerCount++;
                //누적시간 변수에 저장.
            }
            else //오답일경우
            {
                //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
                // 오답일경우 제한시간감소 
                //누적된 시간 날리기
                isAnswerCheck = true;
                isAnswerCorrect = false;
                AnswerCount++;
                TimeSlider.Instance.DecreaseTime_Item(5);
            }
        }
        else if (CaseNum == 2)
        {
            if (Answer_num.text == $"{resultz}")
            {
                Get_Score();
                TrueAnswerCount++; //정답 갯수 증가 -> 정답률 반영에 사용할거
                AnswerCount++;
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //누적시간 변수에 저장.
            }
            else //오답일경우
            {
                //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
                // 오답일경우 제한시간감소 
                //누적된 시간 날리기
                AnswerCount++;
                isAnswerCheck = true;
                isAnswerCorrect = false;
                TimeSlider.Instance.DecreaseTime_Item(5);
            }
        }

        //초기화 필요
        Clear_btn();
        if (!isGameOver) GameProgress();
    }

    //num파라미터를 받아서 입력한 숫자 판정하는 메서드
    public void Clicked_NumberBtn(int num)
    {
        //Todo : 스왑된 위치를 기준으로 점수를 판정 할 수 있도록 로직을 변경해야함
        if (isGameOver) return; //게임 종료시 입력 방지
        Check_AnswerNumDigit(); // 처음 클릭 했을 때 

        switch (CaseNum)
        {
            case 0: //x 칸이 ?? 로 출력되는 경우 클릭시 x쪽에 답이 입력되고, 자릿수에 맞게 답을 입력 해야 함.
                    //19단 까지만 고려하기 때문에 x ,y 는 최대 2자리  z는 3자리까지 체크. 따라서, 입력도 최대 3번(19x19가 최대 3자리)
                if (isFirst_Click)
                {
                    isInputValue = true;
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
                break;
            case 1:
                if (isFirst_Click)
                {
                    isInputValue = true;
                    Second_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    Second_num.text += num.ToString();
                    isSecond_Click = false;
                    Click_Count++;
                }
                break;
            case 2:
                if (isFirst_Click)
                {
                    isInputValue = true;
                    Answer_num.text = num.ToString();
                    isFirst_Click = false;
                    isSecond_Click = true;
                    Click_Count++;
                }
                else if (isSecond_Click)
                {
                    Answer_num.text += num.ToString();
                    isSecond_Click = false;
                    isThird_Click = true;
                    Click_Count++;
                }
                else if (isThird_Click)
                {
                    Answer_num.text += num.ToString();
                    isThird_Click = false;
                    Click_Count++;
                }
                break;
            default:
                break;
        }


        //정답의 자릿수와 클릭의 횟수가 일치하면 정답채크 후 클릭 카운트 초기화
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

    private void Check_AnswerNumDigit()
    {
        //문제가 출제 되었을때 정답의 자릿수를 받아오고
        //정답이 1자리라면 1번 클릭 , 2자리라면 2번클릭 3자리라면 3번클릭한 값을 이용할것.
        //문제의 자리가 x y z 일 경우를 고려해야함
        switch (CaseNum)
        {
            case 0: //x에 들어가는 최대 자릿수
                int firstnum = int.Parse(Answer_num.text) / int.Parse(Second_num.text);
                FirstNumDigit = firstnum.ToString().Length;
                break;
            case 1: //y에 들어가는 최대 자릿수
                int secondnum = int.Parse(Answer_num.text) / int.Parse(First_num.text);
                SecondNumDigit = secondnum.ToString().Length;
                break;
            case 2: //z에 들어가는 최대 자릿수
                int AnswerNum = int.Parse(First_num.text) * int.Parse(Second_num.text);
                AnswerNumDigit = AnswerNum.ToString().Length;
                break;

            default:
                break;
        }
    }

    public void Clear_btn()
    {
        //Todo : 2인모드시 클리어버튼 초기화 위치 지정 필요.
        switch (CaseNum)
        {
            case 0:
                First_num.text = "?";
                break;
            case 1:
                Second_num.text = "?";
                break;
            case 2:
                Answer_num.text = "??";
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




    public void Reaction_speed()
    {
        // 게임시간이 끝날 때 까지 정답을 몇개나 맞췄는지 반응속도 잴것.
        totalReactionTime += Time.deltaTime; // 코루틴XXXX
        if (isAnswerCorrect)
        {
            trueReactionTime += totalReactionTime;
        }
        totalReactionTime = 0;
    }

    //버튼이 아닌 다른 영역을 클릭하면 초기화 시켜주기
    private void Click()
    {

        if (Input.GetMouseButtonDown(0))
        {
            //현재 게임화면에 마우스(손가락터치)를 입력시 그 좌표를 계산
            Vector2 mousePosition = Input.mousePosition;
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>(); // YourCanvas에는 캔버스 GameObject를 할당
            Vector2 canvasSize = canvasRectTransform.sizeDelta;
            Vector2 canvasPosition = canvasRectTransform.position;
            Vector2 canvasMousePosition = new Vector2(mousePosition.x / Screen.width * canvasSize.x, mousePosition.y / Screen.height * canvasSize.y) - (canvasSize / 2f);
            //print(mousePosition.y / Screen.height * canvasSize.y);
            //이 영역 위로 클릭시 Clear
            //if (mousePosition.y / Screen.height * canvasSize.y > 255f) Clear_btn();
        }
    }

    public void Get_Score()
    {
        //if (buttonType == ButtonType.First)
        //{
        //    Score.Instance.Get_FirstScore();
        //}
        //else
        //{
        //    Score.Instance.Get_SecondScore();
        //}
    }

    private void Set_QuestCount()
    {
        switch (timeSet)
        {
            case 60:
                QuestCount = 15;
                break;
            case 180:
                QuestCount = 45; // 예시
                break;
            case 300:
                QuestCount = 75; // 예시 값
                break;
            default:
                break;
        }
        StartQuestCount = QuestCount;
    }


    private void AnswerRate()
    {
        answers = TrueAnswerCount * 100 / StartQuestCount;
    }


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

    private void QuestMark_onOff(int caseNum)
    {
        for (int i = 0; i < QuestMark.Length; i++)
        {
            if (i == caseNum && !isInputValue)
            {
                QuestMark[i].SetActive(true);
            }
            else
            {
                QuestMark[i].SetActive(false);
            }
        }
    }


}


