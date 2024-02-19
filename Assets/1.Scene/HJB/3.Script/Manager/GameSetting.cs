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
        

        ResultDataSet();
    }
}
