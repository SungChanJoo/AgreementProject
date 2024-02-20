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


    public void ShowText(int _reactionRate,int _answerCount,int _answers,int _playtime,int _totalScore)
    {
        reactionRate.text = _reactionRate.ToString();
        answerCount.text = _answerCount.ToString();
        answers.text = _answers.ToString();
        playTime.text = _playtime.ToString();
        totalScore.text = _totalScore.ToString();
    }

}
