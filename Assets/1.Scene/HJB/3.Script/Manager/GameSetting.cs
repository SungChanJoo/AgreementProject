using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Game_Type
{
    A,
    B,
    C
}
public abstract class GameSetting : MonoBehaviour
{
    public int level;
    public int step;
    public int timeSet;

    public float reactionRate;
    public int answersCount;
    public int answers;
    public int playTime;
    public int totalScore;
    public float remainingTime;

    public Result_Data result_data;

    public Result_Printer result_Printer;

    [SerializeField] GameObject resultCanvas_UI;

    private void Start()
    {
        startSet();
    }
   
    private void startSet()
    {
        //선택한 레벨, 스텝, 시간 값 초기 설정
        step = StepManager.Instance.CurrentStep;
        level = StepManager.Instance.CurrentLevel;
        timeSet = StepManager.Instance.CurrentTime;
        TimeSlider.Instance.startTime = timeSet;
        TimeSlider.Instance.duration = timeSet;

        //로직에 의한 시작
        SplitLevelAndStep();
    }
    private void ResultDataSet()
    {
        //레벨, 스텝, 시간 설정값 할당
        result_data.Level = level;
        result_data.Step = step;
        result_data.TimeSet = timeSet;
        //반응속도, 정답률, 정답수, 남은시간, 총합 점수 설정값 할당
        result_data.ReactionRate = reactionRate;
        result_data.Answers = answers;
        result_data.AnswersCount = answersCount;
        result_data.PlayTime = playTime;
        result_data.TotalScore = totalScore;
    }
    //현재 Level Step에 따라 나누기
    public virtual void SplitLevelAndStep()
    {
        switch (level)
        {
            case 1:
                Level_1(step);
                break;
            case 2: 
                Level_2(step);
                break;
            case 3:
                Level_3(step);
                break;
        }
    }
    protected abstract void Level_1(int step);
    protected abstract void Level_2(int step);
    protected abstract void Level_3(int step);

    public void EndGame()
    {
        //결과창 UI 활성화
        ResultCanvas_UI();
        //남은시간
        remainingTime = TimeSlider.Instance.startTime;
        //Result_Data에 게임결과 할당
        ScoreCalculation();
        //결과표 텍스트 출력
        ResultPrinter_UI();
    }

    public void ResultCanvas_UI()
    {
        resultCanvas_UI.SetActive(!resultCanvas_UI.activeSelf);
    }

    protected void ResultPrinter_UI()
    {
        //결과표 출력 Result_Data Class 형식으로 넘겨주기
        result_Printer.ShowText(result_data);
    }

    protected void ScoreCalculation()
    { 
        //여기서 점수 공식 계산할 것.
        

        ResultDataSet();
    }
}
