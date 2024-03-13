using System.Collections.Generic;
using System;

// 클라이언트에서 사용
public class AnalyticsData
{
    // Dictionary 첫 번째 키는 int형식 1~7, 가장 최근 날짜부터(1) 가장 오래된 날짜까지(7)
    // 두 번째 키는 Game_Type형식 A~E, 게임 5가지 종류
    // 세 번째 키는 int형식 1~3, Level 1,2,3
    // 모든 키에 대한 값은 가지되, data_value가 없다면 키에 대한 값은 null이나 0으로 받거나 제공할 것임
    public Dictionary<(int, Game_Type, int), AnalyticsData_Value> Data = new Dictionary<(int, Game_Type, int), AnalyticsData_Value>();
}

// 클라이언트에서 사용
public class AnalyticsData_Value
{
    public string day; // 요일은 YY.MM.DD 
    public float reactionRate;
    public int answerRate;
    public AnalyticsData_Value(string _day, float _reactionRate, int _answerRate)
    {
        day = _day;
        reactionRate = _reactionRate;
        answerRate = _answerRate;
    }
}

// DBManager에서 사용, DateDB 만들때
public class AnalyticsColumnValue
{
    public int licenseNumber;
    public int charactor;
    public float reactionRate;
    public int answerRate;
    public AnalyticsColumnValue(int _licenseNumber, int _charactor, float _reactionRate, int _answerRate)
    {
        licenseNumber = _licenseNumber;
        charactor = _charactor;
        reactionRate = _reactionRate;
        answerRate = _answerRate;
    }
}

// 클라이언트에서 사용 - 개인 Profile
public class AnalyticsProfileData
{
    // 게임명, ReactionRate, AnswerRate
    public Tuple<string, float, int>[] Data = new Tuple<string, float, int>[3];
    void dsdf()
    {
        Data[0] = new Tuple<string, float, int>("베네치아_한글", 1, 1);
        // Data[0].Item1;
    }

}