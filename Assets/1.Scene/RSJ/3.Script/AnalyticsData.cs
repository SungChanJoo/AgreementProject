using System.Collections.Generic;
using System;

// Ŭ���̾�Ʈ���� ���
public class AnalyticsData
{
    // Dictionary ù ��° Ű�� int���� 1~7, ���� �ֱ� ��¥����(1) ���� ������ ��¥����(7)
    // �� ��° Ű�� Game_Type���� A~E, ���� 5���� ����
    // �� ��° Ű�� int���� 1~3, Level 1,2,3
    // ��� Ű�� ���� ���� ������, data_value�� ���ٸ� Ű�� ���� ���� null�̳� 0���� �ްų� ������ ����
    public Dictionary<(int, Game_Type, int), AnalyticsData_Value> Data = new Dictionary<(int, Game_Type, int), AnalyticsData_Value>();
}

// Ŭ���̾�Ʈ���� ���
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

// DBManager���� ���, DateDB ���鶧
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

// Ŭ���̾�Ʈ���� ��� - ���� Profile
public class AnalyticsProfileData
{
    // ���Ӹ�, ReactionRate, AnswerRate
    public Tuple<string, float, int>[] Data = new Tuple<string, float, int>[3];
    void dsdf()
    {
        Data[0] = new Tuple<string, float, int>("����ġ��_�ѱ�", 1, 1);
        // Data[0].Item1;
    }

}