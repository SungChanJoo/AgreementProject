using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalysisChart : MonoBehaviour
{
    [SerializeField] private GameObject reactionRate_Dot;
    [SerializeField] private GameObject answersRate_Dot;
    [SerializeField] private TextMeshProUGUI[] dayText;

    

    Result_DB result_Data = new Result_DB();
    
    private void DataSet()
    {
        for (int i = 0; i < 7; i++)
        {
            Data_value data = new Data_value(i, i, i, i, i);
            result_Data.Data.Add((Game_Type.A, 1, 1), data);
        }
    }

    private void DrawGraph()
    {
        Dictionary<int, int> dayData = new Dictionary<int, int>();
    }
}
