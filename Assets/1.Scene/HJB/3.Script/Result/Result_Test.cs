using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result_Test : MonoBehaviour
{
    private string playerName;
    private int level;
    private int step;
    private string day;
    private int totalAnswer;
    private int totalTime;

    int reactionRate;
    int answersCount;
    int answers;
    int playTime;
    int totalScore;
    
    private void Start()
    {
        PrintResultData();
    }
    private void Update()
    {

    }
    private void PrintResultData()
    {
        //------------------DB값 할당되어 있음(가정)------------------
        Dictionary<(Game_Type, int, int), Data_value> data_
           = new Dictionary<(Game_Type, int, int), Data_value>();
        
        Result_DB DB_Result = new Result_DB(level, step, day, data_);
        //--------------------------------------------------------

        //초기화 및 생성
        Dictionary<(Game_Type, int, int), Data_value> data
        = new Dictionary<(Game_Type, int, int), Data_value>();

        Result_DB result_Data = new Result_DB(level, step, day, data);
        
        Data_value _data = new Data_value(reactionRate, answersCount, answers, playTime, totalScore);

        

        Data_value current_value = result_Data.Data[(Game_Type.A, level, step)];
        //키가 없다면 생성
        if (!DB_Result.Data.ContainsKey((Game_Type.A, level, step)))
        {
            result_Data.Data.Add((Game_Type.A, level, step), _data);

        }
        else
        {
            //총 점수가 원래 있던 값보다 같거나 작다면 할당 X
            if (current_value.TotalScore >= totalScore)
            {
                return;
            }
            //키가 있다면 Data_value에 값을 할당
            current_value = _data;
        }

        //result_Data.Data.Add((Game_Type.A, 1, 1), data_Value);

        //Data_value _data_Value = result_Data.Data[(Game_Type.A, 1, 1)];
        //Debug.Log(_data_Value.Answers);
        //Debug.Log("asdf"+result_Data.Level);
        //_data_Value.Answers = 2;
        //Debug.Log(result_Data.Data[(Game_Type.A, 1, 1)].Answers);
    }
}
