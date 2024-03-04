using System.Collections.Generic;

public class GameAnalytics
{
    // Dictionary ù ��° Ű�� int���� 1~7, ���� ������ ��¥����(1) ���� �ֱ��� ��¥����(7)
    // �� ��° Ű�� Game_Type���� A~E, ���� 5���� ����
    // �� ��° Ű�� int���� 1~3, Level 1,2,3
    // ��� Ű�� ���� ���� ������, data_value�� ���ٸ� Ű�� ���� ���� null�� �ްų� ������ ����
    public Dictionary<(int, Game_Type, int), AnalyticsData_Value> Data = new Dictionary<(int, Game_Type, int), AnalyticsData_Value>();
}

public class AnalyticsData_Value
{
    public string day;
    public float reactionRate;
    public int answerRate;
    public AnalyticsData_Value(float _reactionRate, int _answerRate)
    {
        reactionRate = _reactionRate;
        answerRate = _answerRate;
    }
}
