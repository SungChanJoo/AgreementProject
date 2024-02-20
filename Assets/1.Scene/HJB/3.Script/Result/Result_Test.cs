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
        //------------------DB�� �Ҵ�Ǿ� ����(����)------------------      
        
        Result_DB DB_Result = new Result_DB(level, step, day);
        //------------------���� ������ ------------------
        Result_Data result_Data = new Result_Data();
        
        //�ʱ�ȭ �� ����

        Result_DB result_db = new Result_DB(level, step, day);
        
        Data_value _data = new Data_value(reactionRate, answersCount, answers, playTime, totalScore);

        result_db.Data.Add((Game_Type.A, level, step), _data);
        


        Data_value current_value = result_db.Data[(Game_Type.A, level, step)];
        //Ű�� ���ٸ� ����
        if (!DB_Result.Data.ContainsKey((Game_Type.A, level, step)))
        {
            result_db.Data.Add((Game_Type.A, level, step), _data);

        }
        else
        {
            //�� ������ ���� �ִ� ������ ���ų� �۴ٸ� �Ҵ� X
            if (current_value.TotalScore >= result_Data.TotalScore)
            {
                return;
            }
            //Ű�� �ִٸ� Data_value�� ���� �Ҵ�
            
        }       
    }
}
