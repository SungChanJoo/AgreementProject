using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum Operator
{
    Add,
    Sub,
    Mul,
    Div,
}

public class AOP_Manager : MonoBehaviour
{
    public static AOP_Manager Instance = null;
    public int operator_Ran;
    public string operator_ran;

    public int first_num;
    public int second_num;    
    private int result;
    
    public TextMeshPro result_text;    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public float Calculator_Random()
    {
        operator_Ran = Random.Range(0, 3);
        switch (operator_Ran)
        {
            case (int)Operator.Add:
                operator_ran = "+";
                break;
            case (int)Operator.Sub:
                operator_ran = "-";
                break;
            case (int)Operator.Mul:
                operator_ran = "X";
                break;
            case (int)Operator.Div:
                operator_ran = "/";
                break;
        }
        first_num = Random.Range(0, 10);
        second_num = Random.Range(0, 10);
        Answer_Calculator(first_num,operator_Ran, second_num);

        return result;
    }
    public void Show_Result(float result)
    {        
        result_text.text = result.ToString();
    }
    

    public int Answer_Calculator(int _first_num,int _operator, int _second_num)
    {        
        switch (_operator)
        {
            case (int)Operator.Add:
                result = _first_num + _second_num;
                break;
            case (int)Operator.Sub:
                result = _first_num - _second_num;
                break;
            case (int)Operator.Mul:
                result = _first_num * _second_num;
                break;
            case (int)Operator.Div:
                result = _first_num / _second_num;
                break;
        }
        return result;
    }
}
