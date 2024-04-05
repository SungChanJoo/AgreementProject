using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerTimeRankingData
{
    public Image Img;
    public TMP_Text NameText;
    public TMP_Text TotalTimeText;
    public PlayerTimeRankingData(Image img, TMP_Text nameText, TMP_Text totalTimeText)
    {
        this.Img = img;
        this.NameText = nameText;
        this.TotalTimeText = totalTimeText;
    }
}
public class PlayerScoreRankingData
{
    public Image Img;
    public TMP_Text NameText;
    public TMP_Text TotalScoreText;
    public PlayerScoreRankingData(Image img, TMP_Text nameText, TMP_Text totalScoreText)
    {
        this.Img = img;
        this.NameText = nameText;
        this.TotalScoreText = totalScoreText;
    }
}
public class RankingManager : MonoBehaviour
{
    //View UI
    [SerializeField] private List<GameObject> timeRankUI;
    [SerializeField] private List<GameObject> scoreRankUI;

    [SerializeField] private GameObject[] timeRankPlayerPanel;
    private PlayerTimeRankingData[] timeRankData;// ��ũ �Է� �ʵ�

    [SerializeField] private GameObject[] scoreRankPlayerPanel;
    private PlayerScoreRankingData[] scoreRankData; // ��ũ �Է� �ʵ�
    [Header("PersnalData")]
    //���� ������ ����
    [SerializeField] private TMP_Text weekly;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private GameObject[] persnalRank;
    [SerializeField] private Image myImg;
    private void Awake()
    {
        timeRankData = new PlayerTimeRankingData[5];
        for (int i = 0; i < timeRankPlayerPanel.Length; i++)
        {
            var img = timeRankPlayerPanel[i].transform.GetChild(1).GetComponent<Image>();
            var name = timeRankPlayerPanel[i].transform.Find("Name").GetChild(0).GetComponent<TMP_Text>();
            var totalTime = timeRankPlayerPanel[i].transform.Find("Total_Time").GetChild(0).GetComponent<TMP_Text>();

            var playerData = new PlayerTimeRankingData(img, name, totalTime);
            timeRankData[i] = playerData;
        }

        scoreRankData = new PlayerScoreRankingData[5];
        for (int i = 0; i < scoreRankPlayerPanel.Length; i++)
        {
            var img = scoreRankPlayerPanel[i].transform.GetChild(1).GetComponent<Image>();
            var name = scoreRankPlayerPanel[i].transform.Find("Name").GetChild(0).GetComponent<TMP_Text>();
            var totalScore = scoreRankPlayerPanel[i].transform.Find("Total_Score").GetChild(0).GetComponent<TMP_Text>();

            var playerData = new PlayerScoreRankingData(img, name, totalScore);
            scoreRankData[i] = playerData;
        }

        SetRanking();
        SetMyRanking();
    }
    //0. DB���� �÷��̾� ������(����, �̸�, total����, total�ð�)�� �޾ƿ´�
    //1. �޾ƿ� ���� �Է� �ʵ忡 �Ҵ��Ѵ�.
    public void SetRanking()
    {
        //PlayerTimeData[0].img.sprite = ;
        if (Client.Instance != null)
        {
            var rankInfo = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].RankingInfo;
            for (int i = 0; i < timeRankData.Length; i++)
            {
                timeRankData[i].Img.sprite = ProfileImage_Set(rankInfo.rankdata_time[^(i + 2)].userProfile);
                timeRankData[i].NameText.text = rankInfo.rankdata_time[^(i + 2)].userName;
                timeRankData[i].TotalTimeText.text = $"{rankInfo.rankdata_time[^(i + 2)].totalTime}";
            }

            for (int i = 0; i < scoreRankData.Length; i++)
            {
                scoreRankData[i].Img.sprite = ProfileImage_Set(rankInfo.rankdata_time[^(i + 2)].userProfile);
                scoreRankData[i].NameText.text = rankInfo.rankdata_score[^(i + 2)].userName;
                scoreRankData[i].TotalScoreText.text = $"{rankInfo.rankdata_score[^(i + 2)].totalScore}";
            }
        }
        else
        {
            for (int i = 0; i < timeRankData.Length; i++)
            {
                timeRankData[i].NameText.text = $"{i}";
                timeRankData[i].TotalTimeText.text = $"99�ð�99��{i}";
            }

            for (int i = 0; i < scoreRankData.Length; i++)
            {
                scoreRankData[i].NameText.text = $"{i}";
                scoreRankData[i].TotalScoreText.text = $"{i}9999";
            }
        }

    }
    public void SetMyRanking()
    {
        if (Client.Instance != null)
        {
            var rankInfo = DataBase.Instance.playerInfo.RankingInfo;
            var myData = rankInfo.rankdata_score[5];
            weekly.text = $"�Ⱓ : {rankInfo.periodDate}";
            myImg.sprite = ProfileImage_Set(DataBase.Instance.playerInfo.image);
            playerName.text = $"�̸� : {DataBase.Instance.playerInfo.playerName}";
            var timeRank = persnalRank[0].transform.Find("TimeRank").GetChild(0).GetComponent<TMP_Text>();
            var totalTime = persnalRank[0].transform.Find("TotalTime").GetChild(0).GetComponent<TMP_Text>();
            timeRank.text = $"���� : {myData.scorePlace}\n" +
                            $"�ְ���� : {myData.highestTimePlace}";

            var hour = myData.totalTime / 60;
            var minute = myData.totalTime % 60;
            totalTime.text = $"�����ð�\n {hour}�ð� {minute}��";

            var scoreRank = persnalRank[1].transform.Find("ScoreRank").GetChild(0).GetComponent<TMP_Text>();
            var totalScore = persnalRank[1].transform.Find("TotalScore").GetChild(0).GetComponent<TMP_Text>();
            scoreRank.text = $"���� : {myData.timePlace}\n" +
                            $"�ְ���� : {myData.highestScorePlace}";
            totalScore.text = $"��������\n {myData.totalScore}";
        }
        else
        {
            weekly.text = $"�Ⱓ : {"1998.3.31~ 3.31"}";
            playerName.text = $"�̸� : {"null"}";

            var timeRank = persnalRank[0].transform.Find("TimeRank").GetChild(0).GetComponent<TMP_Text>();
            var totalTime = persnalRank[0].transform.Find("TotalTime").GetChild(0).GetComponent<TMP_Text>();
            var num = 10;

            timeRank.text = $"���� : {num}\n" +
                            $"�ְ���� : {num}";
            totalTime.text = $"�����ð�\n {num}�ð� {num}��";


            var scoreRank = persnalRank[1].transform.Find("ScoreRank").GetChild(0).GetComponent<TMP_Text>();
            var totalScore = persnalRank[1].transform.Find("TotalScore").GetChild(0).GetComponent<TMP_Text>();
            var num1 = 20;

            scoreRank.text = $"���� : {num1}\n" +
                            $"�ְ���� : {num1}";
            totalScore.text = $"��������\n {num1}{num1}";
        }

    }

    public void ToggleRanking(bool enable)
    {
        for (int i = 0; i < timeRankUI.Count; i++)
        {
            timeRankUI[i].SetActive(enable);
        }
        for (int i = 0; i < scoreRankUI.Count; i++)
        {
            scoreRankUI[i].SetActive(!enable);
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
