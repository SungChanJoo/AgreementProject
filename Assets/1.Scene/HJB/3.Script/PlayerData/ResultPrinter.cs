using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPrinter : MonoBehaviour
{    
    [SerializeField] private TextMeshProUGUI reactionRate;
    [SerializeField] private TextMeshProUGUI answerCount;
    [SerializeField] private TextMeshProUGUI answers;
    [SerializeField] private TextMeshProUGUI playTime;
    [SerializeField] private TextMeshProUGUI totalScore;

    [SerializeField] private Image[] start_img;
    [SerializeField] private Sprite get_img;
    [SerializeField] private Sprite non_img;

    public void ShowText(Player_Data data,Game_Type game_Type,int startcount)
    {

        reactionRate.text = data.ReactionRate.ToString("F2");        
        answers.text = $"{data.Answers}%";
        //��, �� ���
        int minute = (int)(data.PlayTime /60f);
        float second = data.PlayTime - (minute*60);
        if (minute == 0)
        {
            //60�ʸ� �ѱ��� �ʾҴٸ� �� ǥ�� ����
            playTime.text = $"{(int)second}��";
        }
        else
        {
            //60�ʸ� �Ѱ�ٸ� ǥ��
            playTime.text = $"{minute}�� {(int)second}��";
        }        
        //�� ���� ���
        totalScore.text = data.TotalScore.ToString();
        
        //���� ������ ���� Text���
        switch (game_Type)
        {
            case Game_Type.A:
                answerCount.text = $"����:{data.AnswersCount} / �� ���� ��:{data.TotalQuestions}";
                break;
            case Game_Type.B:
                answerCount.text = $"����:{data.AnswersCount} / Ŭ�� ��:{data.TotalQuestions}";
                break;
            case Game_Type.C:
            case Game_Type.D:
            case Game_Type.E:
                answerCount.text = $"����:{data.AnswersCount} / �� ���� ��:{data.TotalQuestions}";
                break;            
            default:
                break;
        }
        PrintStarCount(startcount);
    }
    
    private void PrintStarCount(int count)
    {
        //������ ���� �� ī��Ʈ
        for (int i = 0; i < 3; i++)
        {
            if (i<count)
            {
                start_img[i].sprite = get_img;
            }
            else
            {
                start_img[i].sprite = non_img;
            }
        }
    }

}