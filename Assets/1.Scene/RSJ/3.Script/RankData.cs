using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankData
{
    // 0~4 -> 1~5�� / 5 -> ���� ���� �� ����
    public RankData_value[] rankdata_score;
    public RankData_value[] rankdata_time;
}

public class RankData_value
{
    public byte[] userProfile;
    public string userName;
    public int totalScore;
    public float totalTime;
    // ����(index 5)�� ����
    public int scorePlace;
    public int timePlace;
    public int highestScorePlace;
    public int highestTimePlace;
}


