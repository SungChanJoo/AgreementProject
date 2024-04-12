using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;


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

    [HideInInspector] public static bool settingStop = false;

    [SerializeField] private TextMeshProUGUI startTimeSet;

    [SerializeField] private GameObject nextStep_Btn;

    [SerializeField] private float timeset;

    [SerializeField] GameObject resultCanvas_UI;

    public Player_Data result_data;

    public ResultPrinter result_Printer;

    private IEnumerator UpdateDatabaseFromData_co;

    public AudioSource source;

    private int IndexDB;

    private void Start()
    {
        UpdateDatabaseFromData_co = UpdateDatabaseFromData();
        startSet();
        AudioManager.Instance.BGMAudio.Stop();
    }

    private void startSet()
    {
        //������ ����, ����, �ð� �� �ʱ� ����
        play_mode = StepManager.Instance.playMode;
        game_Type = StepManager.Instance.game_Type;
        step = StepManager.Instance.CurrentStep;
        level = StepManager.Instance.CurrentLevel;
        if (game_Type >= Game_Type.C)
        {
            level = 1;
        }
        
        if(TimeSlider.Instance.GameType == GameType.Solo)
        {
            timeSet = StepManager.Instance.CurrentTime;
        }
        else
        {
            timeSet = 60;
        }
        TimeSlider.Instance.StartTime_ = timeSet;
        TimeSlider.Instance.Duration = timeSet;
        Debug.Log($"game_Type : {game_Type}, level : {level}, step: {step} ");
        IndexDB = DataBase.Instance.CharacterIndex;
        
    }
    private void ResultDataSet()
    {
        result_data.Game_type = game_Type;
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
        //�ð� ����
        TimeSlider.Instance.TimeSliderControll();       

        //���� ������ Step�̸� ��ư ��Ȱ��ȭ
        //�����ð�
        remainingTime = TimeSlider.Instance.StartTime_;
        playTime = TimeSlider.Instance.PlayTime;

        //Result_Data�� ���Ӱ�� �Ҵ�
        ScoreCalculation();
        //���� Ȱ��ȭ
        starcount = GameScoreToStarCount(totalScore);

        if (step == 6 || starcount<1)
        {
            nextStep_Btn.SetActive(false);
        }
        //���â UI Ȱ��ȭ
        ResultCanvas_UI();
        //���ǥ �ؽ�Ʈ ���
        ResultPrinter_UI();
        //���� ���� ���
        CalculateStarCoin();

        try
        {
            StartCoroutine(UpdateDatabaseFromData_co);
        }
        catch (Exception)
        {

            Debug.Log("DB ���� ��Ź.");
        }       
    }

    public void ResultCanvas_UI()
    {
        resultCanvas_UI.SetActive(!resultCanvas_UI.activeSelf);
    }

    protected void ResultPrinter_UI()
    {
        //���ǥ ��� Result_Data Class �������� �Ѱ��ֱ�
        result_Printer.ShowText(result_data,game_Type,starcount);        
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
        int y = (20 - (int)reactionRate);
        //�����ð�
        int z = 180 - (int)reactionRate * totalQuestions;
        //�����ð� ���
        int t = z * 3 + 1;
        
        //���̵� ���
        float n = 1f + level * step / 10f;
        totalScore =
            (int)((answersCount * t + 1) + (n * answers / 100f) * (1 + y * x));

        //�� ���� �����ڸ��� ������
        totalScore = (int)(totalScore / 100f) * 100;        

    }
    private void CalculateStarCoin()
    {
        
        Player_DB db = DataBase.Instance.PlayerCharacter[IndexDB];
        int db_starCount = db.Data[(game_Type, level, step)].StarCount;
        //�⺻ StarCoin ����
        starCoin = starcount * 15;
        
        //���� Star�� ȹ�� �� 30�� ����
        if (db_starCount < starcount)
        {
           int count = starcount - db_starCount;
            starCoin += count * 30;
        }        
        DataBase.Instance.PlayerCharacter[IndexDB].StarCoin += starCoin;
    }
    private int GameScoreToStarCount(int score)
    {
        //Score 2�� �� Score�� ���� Star ��ȯ
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
        Player_DB db = DataBase.Instance.PlayerCharacter[IndexDB];
        db.StarCoin = DataBase.Instance.PlayerCharacter[IndexDB].StarCoin;
        Data_value data_Value = new Data_value(reactionRate, answersCount, answers, playTime, totalScore,starcount);        
        
        //���� totalScore�� DB�� �ִ� �������� ũ�ٸ� �ٽ� �Ҵ�
        if (db.Data[(game_Type, level, step)].TotalScore < totalScore)
        {
            db.Data[(game_Type, level, step)] = data_Value;
            Client.Instance.AppGame_SaveResultDataToDB(db, game_Type,level,step);
            Debug.Log("���������� DB�� ����");
        }
        else
        {
            Debug.Log("���������� DB�� �ִ� �������� ���Ƽ� ������� ");
            Client.Instance.AppGame_SaveCoinToDB(db.StarCoin);
        }
        yield return null;
    }
    public void Setting_UI()
    {        
        SettingManager.Instance.EnableSettingBtn();

        
        if (isStop)
        {   //���� ���� ���̶�� �ð��� ������� �ʴ�
            SettingManager.Instance.NonTemporalSetting_Btn();
        }
        else
        {
            //���� ���� �Ķ�� �ð� ������ ���� �ִ� 
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
        //�� ���ӸŴ��� override 
    }
}
