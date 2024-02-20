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
        
        Result_DB DB_Result = new Result_DB(level, step, day);
        //------------------현재 데이터 ------------------
        Result_Data result_Data = new Result_Data();
        
        //초기화 및 생성

        Result_DB result_db = new Result_DB(level, step, day);
        
        Data_value _data = new Data_value(reactionRate, answersCount, answers, playTime, totalScore);

        result_db.Data.Add((Game_Type.A, level, step), _data);
        


        Data_value current_value = result_db.Data[(Game_Type.A, level, step)];
        //키가 없다면 생성
        if (!DB_Result.Data.ContainsKey((Game_Type.A, level, step)))
        {
            result_db.Data.Add((Game_Type.A, level, step), _data);

        }
        else
        {
            //총 점수가 원래 있던 값보다 같거나 작다면 할당 X
            if (current_value.TotalScore >= result_Data.TotalScore)
            {
                return;
            }
            //키가 있다면 Data_value에 값을 할당
            
        }       
    }
}
