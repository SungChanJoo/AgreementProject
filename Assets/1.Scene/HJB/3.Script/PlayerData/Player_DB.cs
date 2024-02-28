using System.Collections.Generic;


public class Player_DB
{
    public string playerName;
    public byte[] image;
    public string Day;
    public int TotalAnswers;
    public float TotalTime;
    
    public Dictionary<(Game_Type, int, int), Data_value> Data = 
        new Dictionary<(Game_Type, int, int), Data_value>();
    
}
public class Data_value
{
    public float ReactionRate;
    public int AnswersCount;
    public int Answers;
    public float PlayTime;
    public int TotalScore;
    public int StarCount;
    //���߰� 
    public Data_value(float reactionRate, int answersCount, int answers, float playTime, int totalScore,int starcount)
    {
        ReactionRate = reactionRate;
        AnswersCount = answersCount;
        Answers = answers;
        PlayTime = playTime;
        TotalScore = totalScore;
        StarCount = starcount;
    }
}



