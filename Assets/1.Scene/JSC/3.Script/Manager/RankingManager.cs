using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerTimeRankingData
{
    public Image img;
    public TMP_Text nameText;
    public TMP_Text totalTimeText;
    public PlayerTimeRankingData(Image img, TMP_Text nameText, TMP_Text totalTimeText)
    {
        this.img = img;
        this.nameText = nameText;
        this.totalTimeText = totalTimeText;
    }
}
public class PlayerScoreRankingData
{
    public Image img;
    public TMP_Text nameText;
    public TMP_Text totalScoreText;
    public PlayerScoreRankingData(Image img, TMP_Text nameText, TMP_Text totalScoreText)
    {
        this.img = img;
        this.nameText = nameText;
        this.totalScoreText = totalScoreText;
    }
}
public class RankingManager : MonoBehaviour
{
    //View UI
    [SerializeField] private List<GameObject> TimeRankUI;
    [SerializeField] private List<GameObject> ScoreRankUI;

    [SerializeField] private GameObject[] TimeRankPlayerPanel;
    private PlayerTimeRankingData[] timeRankData;// 랭크 입력 필드

    [SerializeField] private GameObject[] ScoreRankPlayerPanel;
    private PlayerScoreRankingData[] scoreRankData; // 랭크 입력 필드

    //개인 데이터 저장
    [SerializeField] private TMP_Text Weekly;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private GameObject[] PersnalRank;
    [SerializeField] private Image MyImg;
    private void Awake()
    {
        timeRankData = new PlayerTimeRankingData[5];
        for (int i = 0; i < TimeRankPlayerPanel.Length; i++)
        {
            var img = TimeRankPlayerPanel[i].transform.GetChild(1).GetComponent<Image>();
            var name = TimeRankPlayerPanel[i].transform.Find("Name").GetChild(0).GetComponent<TMP_Text>();
            var totalTime = TimeRankPlayerPanel[i].transform.Find("Total_Time").GetChild(0).GetComponent<TMP_Text>();

            var playerData = new PlayerTimeRankingData(img, name, totalTime);
            timeRankData[i] = playerData;
        }

        scoreRankData = new PlayerScoreRankingData[5];
        for (int i = 0; i < ScoreRankPlayerPanel.Length; i++)
        {
            var img = ScoreRankPlayerPanel[i].transform.GetChild(1).GetComponent<Image>();
            var name = ScoreRankPlayerPanel[i].transform.Find("Name").GetChild(0).GetComponent<TMP_Text>();
            var totalScore = ScoreRankPlayerPanel[i].transform.Find("Total_Score").GetChild(0).GetComponent<TMP_Text>();

            var playerData = new PlayerScoreRankingData(img, name, totalScore);
            scoreRankData[i] = playerData;
        }

        SetRanking();
        SetMyRanking();
    }
    //0. DB에서 플레이어 데이터(사진, 이름, total점수, total시간)를 받아온다
    //1. 받아온 값을 입력 필드에 할당한다.
    public void SetRanking()
    {
        //PlayerTimeData[0].img.sprite = ;
        if (Client.instance != null)
        {
            var rankInfo = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].RankingInfo;
            for (int i = 0; i < timeRankData.Length; i++)
            {
                timeRankData[i].img.sprite = ProfileImage_Set(rankInfo.rankdata_time[^(i + 2)].userProfile);
                timeRankData[i].nameText.text = rankInfo.rankdata_time[^(i + 2)].userName;
                timeRankData[i].totalTimeText.text = $"{rankInfo.rankdata_time[^(i + 2)].totalTime}";
            }

            for (int i = 0; i < scoreRankData.Length; i++)
            {
                scoreRankData[i].img.sprite = ProfileImage_Set(rankInfo.rankdata_time[^(i + 2)].userProfile);
                scoreRankData[i].nameText.text =          rankInfo.rankdata_score[^(i + 2)].userName;
                scoreRankData[i].totalScoreText.text = $"{rankInfo.rankdata_score[^(i + 2)].totalScore}";
            }
        }
        else
        {
            for (int i = 0; i < timeRankData.Length; i++)
            {
                timeRankData[i].nameText.text = $"바보{i}";
                timeRankData[i].totalTimeText.text = $"99시간99분{i}";
            }

            for (int i = 0; i < scoreRankData.Length; i++)
            {
                scoreRankData[i].nameText.text = $"{i}바보";
                scoreRankData[i].totalScoreText.text = $"{i}9999";
            }
        }

    }
    public void SetMyRanking()
    {
        if (Client.instance != null)
        {
            var rankInfo = DataBase.Instance.playerInfo.RankingInfo;
            var myData = rankInfo.rankdata_score[5];
            Weekly.text = $"기간 : {rankInfo.periodDate}";
            MyImg.sprite = ProfileImage_Set(DataBase.Instance.playerInfo.image);
            Name.text = $"이름 : {DataBase.Instance.playerInfo.playerName}";
            var timeRank = PersnalRank[0].transform.Find("TimeRank").GetChild(0).GetComponent<TMP_Text>();
            var totalTime = PersnalRank[0].transform.Find("TotalTime").GetChild(0).GetComponent<TMP_Text>();
            timeRank.text = $"순위 : {myData.scorePlace}\n" +
                            $"최고순위 : {myData.highestTimePlace}";

            var hour = myData.totalTime / 60;
            var minute = myData.totalTime % 60;
            totalTime.text = $"누적시간\n {hour}시간 {minute}분";

            var scoreRank = PersnalRank[1].transform.Find("ScoreRank").GetChild(0).GetComponent<TMP_Text>();
            var totalScore = PersnalRank[1].transform.Find("TotalScore").GetChild(0).GetComponent<TMP_Text>();
            scoreRank.text = $"순위 : {myData.timePlace}\n" +
                            $"최고순위 : {myData.highestScorePlace}";
            totalScore.text = $"누적점수\n {myData.totalScore}";
        }
        else
        {
            Weekly.text = $"기간 : {"1998.3.31~ 3.31"}";
            Name.text = $"이름 : {"바보에"}";

            var timeRank = PersnalRank[0].transform.Find("TimeRank").GetChild(0).GetComponent<TMP_Text>();
            var totalTime = PersnalRank[0].transform.Find("TotalTime").GetChild(0).GetComponent<TMP_Text>();
            var num = 10;

            timeRank.text = $"순위 : {num}\n" +
                            $"최고순위 : {num}";
            totalTime.text = $"누적시간\n {num}시간 {num}분";


            var scoreRank = PersnalRank[1].transform.Find("ScoreRank").GetChild(0).GetComponent<TMP_Text>();
            var totalScore = PersnalRank[1].transform.Find("TotalScore").GetChild(0).GetComponent<TMP_Text>();
            var num1 = 20;

            scoreRank.text = $"순위 : {num1}\n" +
                            $"최고순위 : {num1}";
            totalScore.text = $"누적점수\n {num1}{num1}";
        }

    }

    public void ToggleRanking(bool enable)
    {
        for (int i = 0; i < TimeRankUI.Count; i++)
        {
            TimeRankUI[i].SetActive(enable);
        }
        for (int i = 0; i < ScoreRankUI.Count; i++)
        {
            ScoreRankUI[i].SetActive(!enable);
        }
    }
    public Sprite ProfileImage_Set(byte[] img)
    {
        byte[] fileData = img;
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Texture2D texture2Dload = texture;

        return Sprite.Create(texture2Dload, rect, new Vector2(0.5f, 0.5f));
    }
}
