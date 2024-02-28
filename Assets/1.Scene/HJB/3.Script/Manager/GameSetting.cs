using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Game_Type
{
    A,
    B,
    C,
    D,
    E,
}
public abstract class GameSetting : MonoBehaviour
{
    [HideInInspector] public Game_Type game_Type;

    [HideInInspector] public int level;
    [HideInInspector] public int step;
    [HideInInspector] public int timeSet;

    [HideInInspector] public float reactionRate;
    [HideInInspector] public int answersCount;
    [HideInInspector] public int answers;
    [HideInInspector] public float playTime;
    [HideInInspector] public int totalQuestions;
    [HideInInspector] public int totalScore;
    [HideInInspector] public float remainingTime;
    [HideInInspector] public int starcount;


    [SerializeField] private GameObject nextStep_Btn;

    public Player_Data result_data;

    public Result_Printer result_Printer;

    [SerializeField] GameObject resultCanvas_UI;

    IEnumerator UpdateDatabaseFromData_co;

   
    private void Start()
    {
        UpdateDatabaseFromData_co = UpdateDatabaseFromData();
        startSet();        
    }

    private void startSet()
    {
        //선택한 레벨, 스텝, 시간 값 초기 설정
        game_Type = (Game_Type)SceneManager.GetActiveScene().buildIndex - 2;
        step = StepManager.Instance.CurrentStep;
        level = StepManager.Instance.CurrentLevel;
        print(step);
        timeSet = StepManager.Instance.CurrentTime;
        print("1");
        TimeSlider.Instance.startTime = timeSet;
        print("2");
        TimeSlider.Instance.duration = timeSet;
        //로직에 의한 시작
        SplitLevelAndStep();
    }
    private void ResultDataSet()
    {
        result_data.Game_type = game_Type;
        //레벨, 스텝, 시간 설정값 할당
        result_data.Level = level;
        result_data.Step = step;
        result_data.TimeSet = timeSet;
        //반응속도, 정답률, 정답수, 남은시간, 총합 점수 설정값 할당
        result_data.ReactionRate = reactionRate;
        result_data.Answers = answers;
        result_data.AnswersCount = answersCount;
        result_data.PlayTime = playTime;
        result_data.RemainingTime = (int)remainingTime;
        result_data.TotalQuestions = totalQuestions;
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
        Player_DB sdf = new Player_DB();
        for (int i = 0; i < level; i++)
        {
            Data_value asdf = new(reactionRate, answersCount, answers, playTime, totalScore , starcount);
            sdf.Data.Add((game_Type, level, step), asdf);
        }
    }
    protected abstract void Level_1(int step);
    protected abstract void Level_2(int step);
    protected abstract void Level_3(int step);

    public void EndGame()
    {
        //시간 정지
        TimeSlider.Instance.TimeSliderControll();
        //결과창 UI 활성화
        ResultCanvas_UI();
        //현재 마지막 Step이면 버튼 비활성화
        if (step == 6)
        {
            nextStep_Btn.SetActive(false);
        }
        //남은시간
        remainingTime = TimeSlider.Instance.startTime;
        playTime = TimeSlider.Instance.duration - remainingTime;

        //Result_Data에 게임결과 할당
        ScoreCalculation();
        StartCoroutine(UpdateDatabaseFromData_co);
        //결과표 텍스트 출력
        ResultPrinter_UI();
    }
    public virtual void TimeOut()
    {

    }
    public void ResultCanvas_UI()
    {
        resultCanvas_UI.SetActive(!resultCanvas_UI.activeSelf);
    }

    protected void ResultPrinter_UI()
    {
        //결과표 출력 Result_Data Class 형식으로 넘겨주기
        result_Printer.ShowText(result_data,game_Type);
    }

    protected void ScoreCalculation()
    {
        //여기서 점수 공식 계산할 것.
        TotalScoreCalculation();

        ResultDataSet();
    }
    private void TotalScoreCalculation()
    {
        //기본점수
        int x = answersCount * 10;
        //반응속도계수
        int y = (20 - (int)reactionRate);
        //남은시간
        int z = 180 - (int)reactionRate * totalQuestions;
        //남은시간 계수
        int t = z * 3 + 1;
        
        //난이도 계수
        float n = 1f + level * step / 10f;
        totalScore =
            (int)((answersCount * t + 1) + (n * answers / 100f) * (1 + y * x));

        //총 점수 십의자리수 날리기
        totalScore = (int)(totalScore / 100f) * 100;

    }

    private IEnumerator UpdateDatabaseFromData()
    {
        //string day = System.DateTime.Now.ToString("dd-MM-yy");
        Player_DB result_DB = new Player_DB();
        Data_value data_Value = new Data_value(reactionRate, answersCount, answers, playTime, totalScore,starcount);
        //Reult_DB가 Null일 때 처리
        if (!result_DB.Data.ContainsKey((game_Type, level, step)))
        {
            result_DB.Data.Add((game_Type, level, step), data_Value);            
        }
        else
        {
            //만약 totalScore가 DB에 있는 점수보다 크다면 다시 할당
            if (result_DB.Data[(game_Type, level, step)].TotalScore < totalScore)
            {
                result_DB.Data[(game_Type, level, step)] = data_Value;
            }
        }
        Client.instance.AppGame_SaveResultDataToDB(result_DB,game_Type,level,step);
        yield return null;
    }
    public void Setting_UI()
    {
        SettingManager.Instance.EnableSettingBtn();
        SettingManager.Instance.Setting_Btn();
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextStep_Btn()
    {
        StepManager.Instance.NextStep();
    }
    public void ExitGame()
    {
        SceneManager.LoadScene("HJB_MainMenu");
    }

}
