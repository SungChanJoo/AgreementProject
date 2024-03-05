using System.Collections.Generic;

public class AnalyticsData
{
    // Dictionary ù ��° Ű�� int���� 1~7, ���� �ֱ� ��¥����(1) ���� ������ ��¥����(7)
    // �� ��° Ű�� Game_Type���� A~E, ���� 5���� ����
    // �� ��° Ű�� int���� 1~3, Level 1,2,3
    // ��� Ű�� ���� ���� ������, data_value�� ���ٸ� Ű�� ���� ���� null�̳� 0���� �ްų� ������ ����
    public Dictionary<(int, Game_Type, int), AnalyticsData_Value> Data = new Dictionary<(int, Game_Type, int), AnalyticsData_Value>();
}

public class AnalyticsData_Value
{
    public string day; // ������ YY.MM.DD 
    public float reactionRate;
    public int answerRate;
    public AnalyticsData_Value(string _day, float _reactionRate, int _answerRate)
    {
        day = _day;
        reactionRate = _reactionRate;
        answerRate = _answerRate;
    }
}
