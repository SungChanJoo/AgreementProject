using System.Collections.Generic;


public class Result_DB
{
    public string playerName { get; private set; }

    public Game_Type game_type;

    public byte[] image;

    public int Level { get; private set; }
    public int Step { get; private set; }
    public string Day { get; private set; }
    public int TotalAnswers { get; private set; }

    public Dictionary<(Game_Type, int, int), Data_value> Data = 
        new Dictionary<(Game_Type, int, int), Data_value>();

    public Result_DB(int level, int step, string day)
    {        
        Day = day;        
    }
}
public class Data_value
{
    public float ReactionRate;
    public int AnswersCount;
    public int Answers;
    public float PlayTime;
    public int TotalScore;
    public Data_value(float reactionRate, int answersCount, int answers, float playTime, int totalScore)
    {
        ReactionRate = reactionRate;
        AnswersCount = answersCount;
        Answers = answers;
        PlayTime = playTime;
        TotalScore = totalScore;
    }
}




