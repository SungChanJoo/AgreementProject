using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientTest : MonoBehaviour
{
    private UserData testUserData;
    private Player_DB testPlayer_DB;
    private AnalyticsData testAnalyticsData;
    private RankData testRankData;
    private ExpenditionCrew testExpenditionCrew;
    private LastPlayData testLastPlayData;
    private AnalyticsProfileData testAnalyticsProfileData;

    public void OnClickClientRequestLoadDataToDB()
    {
        TestLoadUserData();
    }

    #region TestLoad
    private void TestLoadUserData()
    {
        testUserData = Client.instance.AppStart_LoadUserDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadUserDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadPlayerDB();
    }

    private void TestLoadPlayerDB()
    {
        testPlayer_DB = Client.instance.AppStart_LoadAllDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAllDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadAnalyticsData();
    }

    private void TestLoadAnalyticsData()
    {
        testAnalyticsData = Client.instance.AppStart_LoadAnalyticsDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAnalyticsDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadRankData();
    }

    private void TestLoadRankData()
    {
        testRankData = Client.instance.AppStart_LoadRankDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadRankDataFromDB(), Current time : {System.DateTime.Now}");
        TestLoadExpenditionCrew();
    }

    private void TestLoadExpenditionCrew()
    {
        testExpenditionCrew = Client.instance.AppStart_LoadExpenditionCrewFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadExpenditionCrewFromDB(), Current time : {System.DateTime.Now}");
        TestLoadLastPlayData();
    }

    private void TestLoadLastPlayData()
    {
        testLastPlayData = Client.instance.AppStart_LoadLastPlayFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadLastPlayFromDB(), Current time : {System.DateTime.Now}");
        TestLoadAnalyticsProfileData();
    }

    private void TestLoadAnalyticsProfileData()
    {
        testAnalyticsProfileData = Client.instance.AppStart_LoadAnalyticsProfileDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAnalyticsProfileDataFromDB(), Current time : {System.DateTime.Now}");

        Debug.Log("[ClientTest] Complete Load All Datas From DB!!");
    }
    #endregion

    public void OnClickCheckDatas()
    {
        // UserData
        Debug.Log($"[UserData] Charactor Count : {testUserData.createdCharactorCount}");
        for (int i = 0; i < testUserData.user.Count; i++)
        {
            Debug.Log($"[UserData] {i}'th CharactorNumber : {testUserData.user[i].CharactorNumber}");
            Debug.Log($"[UserData] {i}'th CharactorName : {testUserData.user[i].Name}");
            Debug.Log($"[UserData] {i}'th CharactorProfile : {testUserData.user[i].Profile}");
        }

        // Player_DB
        Debug.Log($"[PlayerDB] playerName : {testPlayer_DB.playerName}");
        Debug.Log($"[PlayerDB] image : {testPlayer_DB.image}");
        Debug.Log($"[PlayerDB] day : {testPlayer_DB.Day}");
        Debug.Log($"[PlayerDB] BirthDay : {testPlayer_DB.BirthDay}");
        Debug.Log($"[PlayerDB] TotalAnswers : {testPlayer_DB.TotalAnswers}");
        Debug.Log($"[PlayerDB] TotalTime : {testPlayer_DB.TotalTime}");

        int count = 0;
        ICollection<Data_value> allvalues = testPlayer_DB.Data.Values;
        foreach (Data_value datavalue in allvalues)
        {
            count++;
            Debug.Log($"[PlayerDB] count : {count}, ReactionRate : {datavalue.ReactionRate}");
            Debug.Log($"[PlayerDB] count : {count}, AnswersCount : {datavalue.AnswersCount}");
            Debug.Log($"[PlayerDB] count : {count}, Answers : {datavalue.Answers}");
            Debug.Log($"[PlayerDB] count : {count}, PlayTime : {datavalue.PlayTime}");
            Debug.Log($"[PlayerDB] count : {count}, TotlaScore : {datavalue.TotalScore}");
            Debug.Log($"[PlayerDB] count : {count}, StarCount : {datavalue.StarCount}");
        }

        // AnalyticsData
        // 가장 최근날짜, Venezia_kor, Level1
        Debug.Log($"[AnalyticsData] testAnalyticsData.Data[(1, Game_Type.A, 1)].day : {testAnalyticsData.Data[(1, Game_Type.A, 1)].day}");
        Debug.Log($"[AnalyticsData] testAnalyticsData.Data[(1, Game_Type.A, 1)].reactionRate : {testAnalyticsData.Data[(1, Game_Type.A, 1)].reactionRate}");
        Debug.Log($"[AnalyticsData] testAnalyticsData.Data[(1, Game_Type.A, 1)].answerRate : {testAnalyticsData.Data[(1, Game_Type.A, 1)].answerRate}");

        // RankData
        try
        {
            Debug.Log($"[RankData] testRankData.rankdata_score[0].userProfile : {testRankData.rankdata_score[0].userProfile}");
            Debug.Log($"[RankData] testRankData.rankdata_score[0].userName : {testRankData.rankdata_score[0].userName}");
            Debug.Log($"[RankData] testRankData.rankdata_score[0].totalScore : {testRankData.rankdata_score[0].totalScore}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].userProfile : {testRankData.rankdata_score[5].userProfile}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].userName : {testRankData.rankdata_score[5].userName}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].totalScore : {testRankData.rankdata_score[5].totalScore}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].scorePlace : {testRankData.rankdata_score[5].scorePlace}");
            Debug.Log($"[RankData] Client testRankData.rankdata_score[5].highestScorePlace : {testRankData.rankdata_score[5].highestScorePlace}");

        }
        catch
        {

        }
        

        // ExpenditionCrew
        Debug.Log($"[ExpendtionCrew] testExpenditionCrew.SelectedCrew : {testExpenditionCrew.SelectedCrew}");
        for (int i = 0; i < testExpenditionCrew.OwnedCrew.Count; i++)
        {
            Debug.Log($"[ExpendtionCrew] testExpenditionCrew.OwnedCrew[{i}] : {testExpenditionCrew.OwnedCrew[i]}");
        }

        // LastPlayData
        // Venezia_Kor, Level1
        Debug.Log($"[LastPlayData] testLastPlayData.Step[(Game_Type.A,1)] : {testLastPlayData.Step[(Game_Type.A, 1)]}");

        // AnalyticsProfileData
        // [0] -> level1, [1] -> level2, [2] -> level3
        // Item1 = 게임명, Item2 = ReactionRate, Item3 = AnswerRate
        Debug.Log($"[AnalyticsProfileData] testAnalyticsProfileData.Data[0].Item1 : {testAnalyticsProfileData.Data[0].Item1}");
        Debug.Log($"[AnalyticsProfileData] testAnalyticsProfileData.Data[0].Item2 : {testAnalyticsProfileData.Data[0].Item2}");
        Debug.Log($"[AnalyticsProfileData] testAnalyticsProfileData.Data[0].Item3 : {testAnalyticsProfileData.Data[0].Item3}");
        

    }


}
