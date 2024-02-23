using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankData
{
    // 1~5 �� / 6�� �ڱ��ڽ��� ������
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



