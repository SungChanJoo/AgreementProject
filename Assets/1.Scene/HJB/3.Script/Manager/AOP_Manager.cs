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

    private char[] _operator = new char[] { '+', '-', 'x', '÷' };
    public char _Operator;
    
    public void Show_Result(float result)
    {
        result_text.text = result.ToString();
    }

    //연산자와 값 계산
    public float Answer_Calculator(int _operator)
    {
        Random_Num();
        //결과가 조건과 일치할 때 까지
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


    //게임 진행 중 지속적으로 오브젝트에 랜덤한 값을 할당하기 위함
    public override void SplitLevelAndStep()
    {
        base.SplitLevelAndStep();
    }

    public override void Level_1(int step)
    {        
        switch(step)
        {
                //'+'만 출력
            case (int)Step._1:
            case (int)Step._2:
                operator_num = (int)Operator.Add;
                _Operator = _operator[(int)Operator.Add];
                break;
                //'-'만 출력
            case (int)Step._3:
            case (int)Step._4:
                operator_num = (int)Operator.Sub;
                _Operator = _operator[(int)Operator.Sub];
                break;
                // '+','-'만 출력
            case (int)Step._5:
            case (int)Step._6:
                operator_num = Random.Range(0, 2);
                _Operator = _operator[operator_num];
                break;
        }
        //연산자를 파라미터로 넘겨주어 규칙에 맞는 수 뽑기
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
                // + , - , x 출력( x 확률 높음)
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
                // + , - , / 출력( / 확률 높음)
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
                //  x , / 출력                               
                operator_num = Random.Range(2, 4);
                _Operator = _operator[operator_num];
                break;
        }
        //연산자를 파라미터로 넘겨주어 규칙에 맞는 수 뽑기
        Answer_Calculator(operator_num);
        int ranResult = Random.Range(0, 5);
        
        if (ranResult != 1)
        {
            //result값 범위 설정 해줘야함 0 <= result <= 99
            while (0 > result || result >= 100)
            {
                Answer_Calculator(operator_num);
            }
        }
        else
        {
            //result값 범위 설정 해줘야함 100 <= result <= 399
            while (99 > result || result >= 400)
            {
                Answer_Calculator(operator_num);
            }
        }
    }
    public override void Level_3(int step)
    {
        // + , - , x , / 출력
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
