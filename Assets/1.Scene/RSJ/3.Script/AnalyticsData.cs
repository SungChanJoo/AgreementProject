using System.Collections.Generic;

public class AnalyticsData
{
    // Dictionary 첫 번째 키는 int형식 1~7, 가장 최근 날짜부터(1) 가장 오래된 날짜까지(7)
    // 두 번째 키는 Game_Type형식 A~E, 게임 5가지 종류
    // 세 번째 키는 int형식 1~3, Level 1,2,3
    // 모든 키에 대한 값은 가지되, data_value가 없다면 키에 대한 값은 null이나 0으로 받거나 제공할 것임
    public Dictionary<(int, Game_Type, int), AnalyticsData_Value> Data = new Dictionary<(int, Game_Type, int), AnalyticsData_Value>();
}

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
