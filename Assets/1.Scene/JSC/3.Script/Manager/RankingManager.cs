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
    //0. �÷��̾� ������(����, �̸�, total����, total�ð�)�� �޾ƿ´�
    //1. total���� ��, total�ð� ���ؼ� ���� ����
    //2. ���ĵ� ������� �迭�� �ε����� �°� ���� �����Ų��.

    public void SetRanking()
    {
        //PlayerTimeData[0].img.sprite = ;
        for(int i =0; i< timeRankData.Length; i++)
        {
            timeRankData[i].nameText.text = $"�ٺ�{i}";
            timeRankData[i].totalTimeText.text = $"99�ð�99��{i}";
        }

        for (int i = 0; i < scoreRankData.Length; i++)
        {
            scoreRankData[i].nameText.text = $"{i}�ٺ�";
            scoreRankData[i].totalScoreText.text = $"{i}9999";
        }
    }
    public void SetMyRanking()
    {
        Weekly.text = $"�Ⱓ : {"1998.3.31~ 3.31"}";
        Name.text = $"�̸� : {"�ٺ���"}";

        var timeRank = PersnalRank[0].transform.Find("TimeRank").GetChild(0).GetComponent<TMP_Text>();
        var totalTime = PersnalRank[0].transform.Find("TotalTime").GetChild(0).GetComponent<TMP_Text>();
        var num = 10;

        timeRank.text = $"���� : {num}\n" +
                        $"�ְ���� : {num}";
        totalTime.text = $"�����ð�\n {num}�ð� {num}��";


        var scoreRank = PersnalRank[1].transform.Find("ScoreRank").GetChild(0).GetComponent<TMP_Text>();
        var totalScore = PersnalRank[1].transform.Find("TotalScore").GetChild(0).GetComponent<TMP_Text>();
        var num1 = 20;

        scoreRank.text = $"���� : {num1}\n" +
                        $"�ְ���� : {num1}";
        totalScore.text = $"��������\n {num1}{num1}";
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
