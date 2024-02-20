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


    public void ShowText(Result_Data data)
    {
        reactionRate.text = data.ReactionRate.ToString("F2");
        answerCount.text = $"{data.AnswersCount}/{data.TotalQuestions}";
        answers.text = $"{data.Answers}%";
        playTime.text = data.PlayTime.ToString("F2");
        totalScore.text = data.TotalScore.ToString();
    }

}
