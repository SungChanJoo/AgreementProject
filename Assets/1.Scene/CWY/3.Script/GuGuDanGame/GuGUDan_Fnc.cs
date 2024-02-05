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
    }

    private void GameOver()
    {
        //타임슬라이더의 Value 값이 0 일경우 게임 끝.
        if(TimeSlider.Instance.slider.value == 0)
        {
            //Todo : 게임 종료 로직 구현해주세요 > 게임화면 종료하고 결과창? 띄워줄듯
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
        }
        else //오답일경우
        {
            //Todo: 기획팀에서 어떻게 할 계획인지 말해주면 그냥 리턴하던지
            //점수를 깍던지 시간을 깍던지... 
        }
        GameProgress();
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

}
