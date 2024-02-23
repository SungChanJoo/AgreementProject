using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankData
{
    // 1~5 등 / 6은 자기자신의 데이터
    public Rank_Score[] rank_Score;
    public Rank_Time[] rank_Time;

}

public class Rank_Score
{
    public int place;
    public int userlicensenumber;
    public int usercharactor;
    public byte[] userProfile;
    public string userName;
    public int totalScore;
}

public class Rank_Time
{
    public int place;
    public int userlicensenumber;
    public int usercharactor;
    public byte[] userProfile;
    public string userName;
    public float totalTime;
}



