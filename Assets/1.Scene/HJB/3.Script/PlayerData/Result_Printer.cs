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
        //분, 초 계산
        int minute = (int)(data.PlayTime /60f);
        float second = data.PlayTime - (minute*60);
        if (minute == 0)
        {
            //60초를 넘기지 않았다면 분 표시 생략
            playTime.text = $"{(int)second}초";
        }
        else
        {
            //60초를 넘겼다면 표시
            playTime.text = $"{minute}분 {(int)second}초";
        }

        //총 점수 계산
        totalScore.text = data.TotalScore.ToString();
        
        //게임 종류에 따른 Text출력
        switch (game_Type)
        {
            case Game_Type.A:
                answerCount.text = $"정답:{data.AnswersCount} / 총 문제 수:{data.TotalQuestions}";
                break;
            case Game_Type.B:
                answerCount.text = $"정답:{data.AnswersCount} / 클릭 수:{data.TotalQuestions}";
                break;
            case Game_Type.C:
                answerCount.text = $"정답:{data.AnswersCount} / 총 문제 수:{data.TotalQuestions}";
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
