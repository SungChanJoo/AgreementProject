using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public enum Game_Type
{
    A,
    B,
    C,
    D,
    E,
}
public abstract class GameSetting : MonoBehaviour,ITouchEffect
{
    [HideInInspector] public Game_Type game_Type;
    [HideInInspector] public PlayMode play_mode;

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
    [HideInInspector] public int starCoin;

    [HideInInspector] public bool isStop = true;

    [SerializeField] private TextMeshProUGUI startTimeSet;

    [SerializeField] private GameObject nextStep_Btn;

    [SerializeField] private float timeset;

    [SerializeField] GameObject resultCanvas_UI;

    public Player_Data result_data;

    public Result_Printer result_Printer;

    private IEnumerator UpdateDatabaseFromData_co;

    public AudioSource source;


    private void Awake()
    {
        //source = GetComponent<AudioSource>();        
    }

    private void Start()
    {
        UpdateDatabaseFromData_co = UpdateDatabaseFromData();
        startSet();
        AudioManager.Instance.BGMAudio.Stop();
    }

    private void startSet()
    {
        //선택한 레벨, 스텝, 시간 값 초기 설정
        game_Type = StepManager.Instance.game_Type;
        step = StepManager.Instance.CurrentStep;
        level = StepManager.Instance.CurrentLevel;
        
        if(TimeSlider.Instance.gameType == GameType.Solo)
        {
            timeSet = StepManager.Instance.CurrentTime;
        }
        else
        {
            timeSet = 60;
        }
        TimeSlider.Instance.startTime = timeSet;
        TimeSlider.Instance.duration = timeSet;
        Debug.Log($"game_Type : {game_Type}, level : {level}, step: {step} ");        
        
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
    }
    public void GameStart_Btn()
    {        
        startGame();
    }
    protected abstract void Level_1(int step);
    protected abstract void Level_2(int step);
    protected abstract void Level_3(int step);    
    protected virtual void startGame()
    {
        SplitLevelAndStep();
    }    

    public void EndGame()
    {
        //시간 정지
        TimeSlider.Instance.TimeSliderControll();       

        //현재 마지막 Step이면 버튼 비활성화
        if (step == 6)
        {
            nextStep_Btn.SetActive(false);
        }
        //남은시간
        remainingTime = TimeSlider.Instance.startTime;
        playTime = TimeSlider.Instance.PlayTime;

        //Result_Data에 게임결과 할당
        ScoreCalculation();
        //업적 활성화
        starcount = GameScoreToStarCount(totalScore);

        //결과창 UI 활성화
        ResultCanvas_UI();
        //결과표 텍스트 출력
        ResultPrinter_UI();
        //업적 코인 계산
        CalculateStarCoin();

        try
        {
            StartCoroutine(UpdateDatabaseFromData_co);
        }
        catch (Exception)
        {

            Debug.Log("DB 연결 부탁.");
        }
        finally
        {
            
        }
    }

    public void ResultCanvas_UI()
    {
        resultCanvas_UI.SetActive(!resultCanvas_UI.activeSelf);
    }

    protected void ResultPrinter_UI()
    {
        //결과표 출력 Result_Data Class 형식으로 넘겨주기
        result_Printer.ShowText(result_data,game_Type,starcount);        
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
    private void CalculateStarCoin()
    {
        Player_DB db = DataBase.Instance.PlayerCharacter[0];
        int db_starCount = db.Data[(game_Type, level, step)].StarCount;
        
        starCoin = starcount * 15;
        Debug.Log($"현재 별의 갯수 : {starcount} 코인 : {starCoin}");
        if (db_starCount < starcount)
        {
           int count = starcount - db_starCount;
            starCoin += count * 30;
        }
        Debug.Log($"totalCoin : {starCoin}");
        DataBase.Instance.PlayerCharacter[0].StarCoin += starCoin;
    }
    private int GameScoreToStarCount(int score)
    {
        score *= 2;
        if (20000 <= score)
        {
            return 3;
        }
        else if (10000 <= score && score < 20000)
        {
            return 2;
        }
        else if (7000 <= score && score < 10000)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    private IEnumerator UpdateDatabaseFromData()
    {
        //string day = System.DateTime.Now.ToString("dd-MM-yy");
        Player_DB db = DataBase.Instance.PlayerCharacter[0];
        Data_value data_Value = new Data_value(reactionRate, answersCount, answers, playTime, totalScore,starcount);        
        
        //만약 totalScore가 DB에 있는 점수보다 크다면 다시 할당
        if (db.Data[(game_Type, level, step)].TotalScore < totalScore)
        {
            db.Data[(game_Type, level, step)] = data_Value;            
            Client.instance.AppGame_SaveResultDataToDB(db, game_Type,level,step);
            Debug.Log("정상적으로 DB에 저장");
        }
        else
        {
            Debug.Log("최종점수가 DB에 있는 점수보다 낮아서 저장안함 ");
        }
        yield return null;
    }
    public void Setting_UI()
    {        
        SettingManager.Instance.EnableSettingBtn();
        
        if (isStop)
        {   //게임 시작 전이라면 시간에 영향받지 않는 
            SettingManager.Instance.NonTemporalSetting_Btn();
        }
        else
        {
            //게임 시작 후라면 시간 정지에 영향 주는 
            SettingManager.Instance.Setting_Btn();
        }       
    }
    public void Start_Btn()
    {
        SettingManager.Instance.Stop = false;
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

    public virtual void TouchSoundCheck(bool answerCheck)
    {

    }
}
