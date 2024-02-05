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



    //게임 진행 순서 
    bool isStart = false;
    bool isFirst_Click = true;
    bool isSecond_Click = false;
    bool isGameOver = false;

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


    //랜덤숫자 생성 메서드
    public int Random_Num()
    {
        int num = Random.Range(1, 10);
        return num;
    }

    //게임 시작시 랜덤 숫자 부여
    private void GameStart()
    {
        //게임 시작을 알림
        isStart = true;
        //첫번째 숫자 랜덤으로 받아오기
        First_num.text = Random_Num().ToString();
        //두번째 숫자 랜덤으로 받아오기
        Second_num.text = Random_Num().ToString();
    }
    //로직은 동일하지만 시작 - 진행 - 종료를 나누기위해 메서드 분개
    private void GameProgress()
    {
        //첫번째 숫자 랜덤으로 받아오기
        First_num.text = Random_Num().ToString();
        //두번째 숫자 랜덤으로 받아오기
        Second_num.text = Random_Num().ToString();
        //결과 숫자 초기화
        Answer_num.text = "??";
        //첫번째 클릭 초기화
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
    }

    //정답 판단 함수
    public void AnswerCheck()
    {
        //정답일경우
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
            //점수를 깍던지 시간을 깍던지... 
            //누적된 시간 날리기
            isAnswerCheck = true;
            isAnswerCorrect = false;
        }
        if(!isGameOver) GameProgress();
    }

    //num파라미터를 받아서 입력한 숫자 판정하는 메서드
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



    //성찬이가 시킨 반응속도 계산...
    //우선.. 정답만 기준으로 계산....
    //문제 출제 -> 시간 똑딱똑딱 -> 정답을 맞추면 잰 시간을 저장
    // ex) 6개를 맞추면 각각의 정답맞춘 속도를 잰 후 나눠줄것.. 

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
}
