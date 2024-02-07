using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuGUDan_Fnc : MonoBehaviour
{
    //구구단 게임 기능 관련 스크립트
    //곱셈 숫자 1번,2번에 랜덤으로 숫자가 발생해서 구구단을 진행 
    //결과는 맞추던 못맞추던 공개하고 맞출경우 O , 틀릴경우 X 표시?
    //정답은 버튼 1 ~ 9 를 눌러서 입력 

    [SerializeField] private Text First_num;  // 첫번째 칸에 들어갈 숫자
    [SerializeField] private Text Second_num; // 두번째 칸에 들어갈 숫자.
    [SerializeField] private Text Answer_num; // 정답을 입력받을 칸 

    [SerializeField] private Button[] buttons; // 0~9 버튼 

    [SerializeField] Canvas canvas;

    public int Level;
    public int Step;

    //게임 진행 순서 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isThird_Click = false;
    bool isGameOver = false;

    //정답의 자릿수 확인
    int FistNumDigit;
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
    string buttonText;

    //스탭별 케이스 선택지 번호 확인
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


    //랜덤숫자 생성 메서드
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //표기해 주는 위치를 랜덤으로 표시 해줄 메서드
    // 스탭 1 ~ 3  / 4 ~ 6 에서의 난이도는 동일함
    private void Random_ShowText(Text First, Text Second, Text Answer)
    {
        //문제 생성 => x , y를 생성한후 곱셈 
        //단 , UI에 표기하는 것은 랜덤 * 역산 시스템이 있어야함 해당 메서드는 표기 순서만 변경
        // 1. x 가 비어있을 경우 ?? x Y = z  
        // 2. Y가 비어있을 경우 X x ?? = z
        // 3. z가 비어있을 경우 X x Y = ??


    }
    #region Level , Step별 문제 제작 메서드 
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
    //lv1게임 로직 구현
    private void Lv1()
    {
        Lv1_RandomNum();
        //lv 1 > 2~9단까지
        //스탭1~3

        //택스트의 위치를 랜덤으로 바꿔준 후 정답을 판단
        int First = int.Parse(First_num.text);
        int Second = int.Parse(Second_num.text);
        int result = int.Parse(First_num.text) * int.Parse(Second_num.text);
        switch (Random.Range(0, 2))
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

    }

    #endregion
    //

    //게임 시작시 랜덤 숫자 부여
    //선택 Level , Step에 따라 숫자의 범위 제한
    private void GameStart()
    {
        //게임 시작을 알림
        isStart = true;
        switch (Level)
        {
            case 1: //레벨 1 선택
                Lv1();
                break;
            case 2: //레벨 2 선택
                break;
            case 3: //레벨 3 선택

                break;
            default:
                break;
        }
    }
    //로직은 동일하지만 시작 - 진행 - 종료를 나누기위해 메서드 분개
    private void GameProgress()
    {
        //정답을 맞춘경우에는 문제 다시생성
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
        //클릭 초기화
        isFirst_Click = true;

        //정답확인 초기화
        isAnswerCheck = false;
    }

    private void GameOver()
    {
        //타임슬라이더의 Value 값이 0 일경우 게임 끝.
        if(TimeSlider.Instance.slider.value == 0)
        {
            //Todo : 게임 종료 로직 구현해주세요 > 게임화면 종료하고 결과창? 띄워줄듯
            //반응속도 쳌
            print(trueReactionTime / TrueAnswerCount);
            isGameOver = true;
            totalReactionTime = 0;
        }
        else
        {
            return;
        }  
    }

    //정답 판단 함수
    public void AnswerCheck()
    {
        if (TimeSlider.Instance.slider.value == 0) return; // 타임오버시 리턴
        //정답일경우
        if(CaseNum == 0) //x 쪽 역산
        {
            int result = int.Parse(Answer_num.text) / int.Parse(Second_num.text);
            if (Second_num.text == $"{result}")
            {
                Score.Instance.Get_Score();
                TrueAnswerCount++; //정답 갯수 증가 -> 정답률 반영에 사용할거
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //누적시간 변수에 저장.
            }
            else //오답일경우
            {
                //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
                // 오답일경우 제한시간감소 
                //누적된 시간 날리기
                isAnswerCheck = true;
                isAnswerCorrect = false;
            }
        }
        else if(CaseNum == 1) // y쪽을 역산
        {
            int result = int.Parse(Answer_num.text) / int.Parse(First_num.text);
            if (Answer_num.text == $"{result}")
            {
                Score.Instance.Get_Score();
                TrueAnswerCount++; //정답 갯수 증가 -> 정답률 반영에 사용할거
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //누적시간 변수에 저장.
            }
            else //오답일경우
            {
                //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
                // 오답일경우 제한시간감소 
                //누적된 시간 날리기
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
                TrueAnswerCount++; //정답 갯수 증가 -> 정답률 반영에 사용할거
                isAnswerCheck = true;
                isAnswerCorrect = true;
                //누적시간 변수에 저장.
            }
            else //오답일경우
            {
                //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
                // 오답일경우 제한시간감소 
                //누적된 시간 날리기
                isAnswerCheck = true;
                isAnswerCorrect = false;
            }
        }


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
        //정답의 자릿수와 클릭의 횟수가 일치하면 정답채크 후 클릭 카운트 초기화
        if (AnswerNumDigit == Click_Count)
        {
            AnswerCheck();
            Click_Count = 0;
        }

    }

    private void Check_AnswerNumDigit()
    {
        //문제가 출제 되었을때 정답의 자릿수를 받아오고
        //정답이 1자리라면 1번 클릭 , 2자리라면 2번클릭 3자리라면 3번클릭한 값을 이용할것.
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
        // 게임시간이 끝날 때 까지 정답을 몇개나 맞췄는지 반응속도 잴것.
        totalReactionTime += Time.deltaTime; // 코루틴XXXX
     //   trueReactionTime = (isAnswerCorrect) ? (trueReactionTime += totalReactionTime) : (trueReactionTime += 0); // 정답을 맞춘 경우에는 시간을 더해줄것 아닌경우에는 0을더함(반응속도에 영향x)
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
            print(mousePosition.y / Screen.height * canvasSize.y);
            //이 영역 위로 클릭시 Clear
            if (mousePosition.y / Screen.height * canvasSize.y > 255f) Clear_btn();
        }
    }

}
