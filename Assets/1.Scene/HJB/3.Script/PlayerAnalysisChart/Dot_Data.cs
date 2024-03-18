using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dot_Data : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dot_data;

    public void Print_DotData(float data,bool first)
    {
        
        if (first)
        {
            data /= 100;
            dot_data.text = data.ToString("F0");
        }
        else
        {
            dot_data.text = data.ToString("F1");
        }
    }
}
