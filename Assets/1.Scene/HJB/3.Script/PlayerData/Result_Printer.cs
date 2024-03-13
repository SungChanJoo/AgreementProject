using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Result_Printer : MonoBehaviour
{    
    [SerializeField] private TextMeshProUGUI reactionRate;
    [SerializeField] private TextMeshProUGUI answerCount;
    [SerializeField] private TextMeshProUGUI answers;
    [SerializeField] private TextMeshProUGUI playTime;
    [SerializeField] private TextMeshProUGUI totalScore;


    public void ShowText(Player_Data data,Game_Type game_Type)
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
                answerCount.text = $"����:{data.AnswersCount} / �� ���� ��:{data.TotalQuestions}";
                break;
            case Game_Type.D:
                break;
            case Game_Type.E:
                break;
            default:
                break;
        }
    }

}
