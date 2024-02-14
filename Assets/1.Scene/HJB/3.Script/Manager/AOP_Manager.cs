using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public enum Operator
{
    Add,
    Sub,
    Mul,
    Div,
}

public class AOP_Manager : GameSetting
{    
    public int operator_num;
    public string operator_ran;

    public int first_num;
    public int second_num;
    public int result;
    
    public TextMeshPro result_text;

    private char[] _operator = new char[] { '+', '-', 'x', '��' };
    public char _Operator;
    
    public void Show_Result(float result)
    {
        result_text.text = result.ToString();
    }

    //�����ڿ� �� ���
    public float Answer_Calculator(int _operator)
    {
        Random_Num();
        //����� ���ǰ� ��ġ�� �� ����
        switch (_operator)
        {
            case (int)Operator.Add:
                result = first_num + second_num;
                break;
            case (int)Operator.Sub:
                result = first_num - second_num;
                break;
            case (int)Operator.Mul:
                result = first_num * second_num;
                break;
            case (int)Operator.Div:
                while (!(first_num%second_num).Equals(0)&&
                    first_num<second_num)
                {
                    Random_Num();
                }
                result = first_num / second_num;
                break;
        }
        return result;
    }
    private void Random_Num()
    {
        first_num = Random.Range(1, 99);
        second_num = Random.Range(1, 99);
    }


    //���� ���� �� ���������� ������Ʈ�� ������ ���� �Ҵ��ϱ� ����
    public override void SplitLevelAndStep()
    {
        base.SplitLevelAndStep();
    }

    public override void Level_1(int step)
    {        
        switch(step)
        {
                //'+'�� ���
            case (int)Step._1:
            case (int)Step._2:
                operator_num = (int)Operator.Add;
                _Operator = _operator[(int)Operator.Add];
                break;
                //'-'�� ���
            case (int)Step._3:
            case (int)Step._4:
                operator_num = (int)Operator.Sub;
                _Operator = _operator[(int)Operator.Sub];
                break;
                // '+','-'�� ���
            case (int)Step._5:
            case (int)Step._6:
                operator_num = Random.Range(0, 2);
                _Operator = _operator[operator_num];
                break;
        }
        //�����ڸ� �Ķ���ͷ� �Ѱ��־� ��Ģ�� �´� �� �̱�
        Answer_Calculator(operator_num);
        
        while (0 > result ||  99 < result)
        {
            Answer_Calculator(operator_num);
        }
    }
    public override void Level_2(int step)
    {
        int random_oper = Random.Range(0, 5);
        switch (step)
        {
            case (int)Step._1:
            case (int)Step._2:
                // + , - , x ���( x Ȯ�� ����)
                if (random_oper == 0)
                {
                    operator_num = Random.Range(0, 2);
                    _Operator = _operator[operator_num];
                }
                else
                {
                    operator_num = (int)Operator.Mul;
                    _Operator = _operator[(int)Operator.Mul];
                }
                break;
            case (int)Step._3:
            case (int)Step._4:
                // + , - , / ���( / Ȯ�� ����)
                if (operator_num < 1)
                {
                    operator_num = Random.Range(0, 2);
                    _Operator = _operator[operator_num];
                }
                else
                {
                    operator_num = (int)Operator.Div;
                    _Operator = _operator[(int)Operator.Div];
                }
                break;
            case (int)Step._5:
            case (int)Step._6:
                //  x , / ���                               
                operator_num = Random.Range(2, 4);
                _Operator = _operator[operator_num];
                break;
        }
        //�����ڸ� �Ķ���ͷ� �Ѱ��־� ��Ģ�� �´� �� �̱�
        Answer_Calculator(operator_num);
        int ranResult = Random.Range(0, 5);
        
        if (ranResult != 1)
        {
            //result�� ���� ���� ������� 0 <= result <= 99
            while (0 > result || result >= 100)
            {
                Answer_Calculator(operator_num);
            }
        }
        else
        {
            //result�� ���� ���� ������� 100 <= result <= 399
            while (99 > result || result >= 400)
            {
                Answer_Calculator(operator_num);
            }
        }
    }
    public override void Level_3(int step)
    {
        // + , - , x , / ���
        operator_num = Random.Range(0, 4);
        _Operator = _operator[operator_num];
        Answer_Calculator(operator_num);
        while (0 >= result || 1000 <= result)
        {
            Answer_Calculator(operator_num);
        }
    }  
    
    
    public void ExitGame()
    {
        SceneManager.LoadScene("HJB_MainMenu");
    }
}
