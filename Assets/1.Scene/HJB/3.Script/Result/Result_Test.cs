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
        Dictionary<(Game_Type, int, int), Data_value> data_
           = new Dictionary<(Game_Type, int, int), Data_value>();
        
        Result_DB DB_Result = new Result_DB(level, step, day, data_);
        //--------------------------------------------------------

        //�ʱ�ȭ �� ����
        Dictionary<(Game_Type, int, int), Data_value> data
        = new Dictionary<(Game_Type, int, int), Data_value>();

        Result_DB result_Data = new Result_DB(level, step, day, data);
        
        Data_value _data = new Data_value(reactionRate, answersCount, answers, playTime, totalScore);

        

        Data_value current_value = result_Data.Data[(Game_Type.A, level, step)];
        //Ű�� ���ٸ� ����
        if (!DB_Result.Data.ContainsKey((Game_Type.A, level, step)))
        {
            result_Data.Data.Add((Game_Type.A, level, step), _data);

        }
        else
        {
            //�� ������ ���� �ִ� ������ ���ų� �۴ٸ� �Ҵ� X
            if (current_value.TotalScore >= totalScore)
            {
                return;
            }
            //Ű�� �ִٸ� Data_value�� ���� �Ҵ�
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
