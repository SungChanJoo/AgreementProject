using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player_Data
{
    public Game_Type Game_type;

    public int Level;
    public int Step;
    public int TimeSet;

    public float ReactionRate;
    public int AnswersCount;
    public int Answers;
    public float PlayTime;
    public int RemainingTime;        
    public int TotalQuestions;
    public int TotalScore;
}
