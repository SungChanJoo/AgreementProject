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
    [SerializeField] private List<GameObject> TimeRankUI;
    [SerializeField] private List<GameObject> ScoreRankUI;


    [SerializeField] private GameObject[] TimeRankPlayerPanel;
    private PlayerTimeRankingData[] timeRankData;

    [SerializeField] private GameObject[] ScoreRankPlayerPanel;
    private PlayerScoreRankingData[] scoreRankData;

    [SerializeField] private TMP_Text Weekly;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private GameObject[] PersnalRank;
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
    //0. 플레이어 데이터(사진, 이름, total점수, total시간)를 받아온다
    //1. total점수 비교, total시간 비교해서 순위 정렬
    //2. 정렬된 순서대로 배열의 인덱스에 맞게 값을 변경시킨다.

    public void SetRanking()
    {
        //PlayerTimeData[0].img.sprite = ;
        for(int i =0; i< timeRankData.Length; i++)
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
    public void SetMyRanking()
    {
        Weekly.text = $"기간 : {"1998.3.31~ 3.31"}";
        Name.text = $"이름 : {"바보임"}";

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
}
