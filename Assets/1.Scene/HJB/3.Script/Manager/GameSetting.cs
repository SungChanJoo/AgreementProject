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
    public float playTime;
    public int totalQuestions;
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
        //������ ����, ����, �ð� �� �ʱ� ����
        step = StepManager.Instance.CurrentStep;
        level = StepManager.Instance.CurrentLevel;
        timeSet = StepManager.Instance.CurrentTime;
        TimeSlider.Instance.startTime = timeSet;
        TimeSlider.Instance.duration = timeSet;

        //������ ���� ����
        SplitLevelAndStep();
    }
    private void ResultDataSet()
    {
        //����, ����, �ð� ������ �Ҵ�
        result_data.Level = level;
        result_data.Step = step;
        result_data.TimeSet = timeSet;
        //�����ӵ�, �����, �����, �����ð�, ���� ���� ������ �Ҵ�
        result_data.ReactionRate = reactionRate;
        result_data.Answers = answers;
        result_data.AnswersCount = answersCount;
        result_data.PlayTime = playTime;
        result_data.RemainingTime = (int)remainingTime;
        result_data.TotalQuestions = totalQuestions;
        result_data.TotalScore = totalScore;
    }
    //���� Level Step�� ���� ������
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
        //���â UI Ȱ��ȭ
        ResultCanvas_UI();
        //�����ð�
        remainingTime = TimeSlider.Instance.startTime;
        playTime = TimeSlider.Instance.duration - remainingTime;
        
        //Result_Data�� ���Ӱ�� �Ҵ�
        ScoreCalculation();
        //���ǥ �ؽ�Ʈ ���
        ResultPrinter_UI();
    }

    public void ResultCanvas_UI()
    {
        resultCanvas_UI.SetActive(!resultCanvas_UI.activeSelf);
    }

    protected void ResultPrinter_UI()
    {
        //���ǥ ��� Result_Data Class �������� �Ѱ��ֱ�
        result_Printer.ShowText(result_data);
    }

    protected void ScoreCalculation()
    {
        //���⼭ ���� ���� ����� ��.
        TotalScoreCalculation();

        ResultDataSet();
    }
    private void TotalScoreCalculation()
    {
        //�⺻����
        int x = answersCount * 10;
        //�����ӵ����
        int y = 3 * (30 - (int)reactionRate);
        //�����ð�
        int z = 180 - (int)reactionRate * totalQuestions;
        //�����ð� ���
        int t = 0;
        switch (timeSet)
        {
            case (int)TimeSet._1m:
                t = z * 3;
                break;
            case (int)TimeSet._3m:
                t = (z * 1) * 3 + 1;
                break;
            case (int)TimeSet._5m:
                t = (z * 3 / 5) * 3 + 1;
                break;
        }
        //���̵� ���
        float n = 1f + level * step / 10f;
        totalScore =
            (answersCount * t + 1) + ((int)n / 100) * ((1 + y) * x);
        
        //�� ���� �����ڸ��� ������
        totalScore = (int)(totalScore / 100f) * 100;

    }

}
