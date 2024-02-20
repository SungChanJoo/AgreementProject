using System.Collections.Generic;


public class Result_Data
{
    public string playerName { get; private set; }

    public Game_Type game_type;

    public int Level { get; private set; }
    public int Step { get; private set; }
    public string Day { get; private set; }
    public int TotalAnswers { get; private set; }

    public Dictionary<(Game_Type, int, int), Data_value> Data;

    public Result_Data(int level, int step, string day,
        Dictionary<(Game_Type, int, int), Data_value> data)
    {
        //playerName = name;
        Level = level;
        Step = step;
        Day = day;
        Data = data;
    }
}
public class Data_value
{
    public int ReactionRate;
    public int AnswersCount;
    public int Answers;
    public int PlayTime;
    public int TotalScore;
    public Data_value(int reactionRate, int answersCount, int answers, int playTime, int totalScore)
    {
        ReactionRate = reactionRate;
        AnswersCount = answersCount;
        Answers = answers;
        PlayTime = playTime;
        TotalScore = totalScore;
    }
}




