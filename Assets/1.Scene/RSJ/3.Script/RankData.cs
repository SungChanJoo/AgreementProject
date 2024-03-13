using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankData
{
    // 0~4 -> 1~5등 / 5 -> 개인 순위 및 점수
    public RankData_value[] rankdata_score;
    public RankData_value[] rankdata_time;
}

public class RankData_value
{
    public byte[] userProfile;
    public string userName;
    public int totalScore;
    public float totalTime;
    // 개인(index 5)용 순위
    public int scorePlace;
    public int timePlace;
    public int highestScorePlace;
    public int highestTimePlace;
}


