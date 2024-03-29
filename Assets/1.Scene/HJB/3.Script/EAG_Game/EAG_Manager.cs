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
public class EAG_Manager : GameSetting
{
    
    public TextMeshPro resultText;    
    public Operator selectedOperator;

    private char[] operators = new char[] { '+', '-', 'x', '÷' };
    public int firstNum;
    public int secondNum;
    public int result;
    public char _Operator;    

    public float RotationPerSecond = 1;

    private int step_opreator1 = 0;
    private int step_opreator2 = 0;

    [SerializeField] private AudioClip[] audioClip;

    //결과 출력
    public void Show_Result(float result)
    {
        resultText.text = result.ToString();
    }
    protected override void startGame()
    {
        StartGamePooling();
    }
    private void StartGamePooling()
    {
        EAG_ObjectPooling.Instance.ObjectPooling();
    }

    public int AnswerCalculator(Operator selectedOperator)
    {
        GenerateRandomNumbers();

        switch (selectedOperator)
        {
            case Operator.Add:
                result = firstNum + secondNum;
                break;
            case Operator.Sub:
                result = firstNum - secondNum;
                break;
            case Operator.Mul:
                result = firstNum * secondNum;
                break;
            case Operator.Div:
                while (firstNum % secondNum != 0 || firstNum < secondNum)
                {
                    GenerateRandomNumbers();
                }
                result = firstNum / secondNum;
                break;
        }
        return result;
    }

    private void GenerateRandomNumbers()
    {
        firstNum = Random.Range(1, 99);
        secondNum = Random.Range(1, 99);
    }

    public override void TouchSoundCheck(bool answerCheck)
    {
        if (answerCheck)
        {
            source.PlayOneShot(audioClip[0]);
        }
        else
        {
            source.PlayOneShot(audioClip[1]);
        }
    }
    public override void SplitLevelAndStep()
    {
        base.SplitLevelAndStep();
    }

    protected override void Level_1(int step)
    {        
        int resultMax = 20;
        switch (step)
        {
            case 1:
            case 2:
                step_opreator1 = 0;         //'+' 
                step_opreator2 = 0;
                break;
            case 3:
            case 4:
                step_opreator1 = 1;         //'-' 
                step_opreator2 = 1;
                break;
            case 5:
            case 6:
                step_opreator1 = 0;         //'+', '-' 및 60까지 제한
                step_opreator2 = 1;
                resultMax = 60;
                break;

        }
        selectedOperator = GetOperatorForStep(step_opreator1, step_opreator2);
        result = AnswerCalculator(selectedOperator);
        EnsureResultInRange(0, resultMax);
    }

    protected override void Level_2(int step)
    {       
        int randomSelectOper = Random.Range(0, 5);

        switch (step)
        {
            case 1:
            case 2:               
                step_opreator1 = 2;         //'x'
                step_opreator2 = 2;                
                break;
            case 3:
            case 4:                
                step_opreator1 = 3;         //'÷'
                step_opreator2 = 3;
                break;
            case 5:
            case 6:
                step_opreator1 = 2;         //'x', '÷'
                step_opreator2 = 3;
                break;
        }
        //25퍼 확률로 덧셈 뺄셈 출력
        if (randomSelectOper == 1)
        {
            step_opreator1 = 0;
            step_opreator2 = 1;
        }

        selectedOperator = GetOperatorForStep(step_opreator1, step_opreator2);        
        EnsureResultInRangeCoroutine(0, 100, 400);
    }

    protected override void Level_3(int step)
    {
        selectedOperator = GetOperatorForStep(0, 3);
        EnsureResultInRangeCoroutine(0, 100, 400);
    }

    private Operator GetOperatorForStep(int minOperatorIndex, int maxOperatorIndex)
    {
        //연산자 선택
        int randomOperator = Random.Range(minOperatorIndex, maxOperatorIndex + 1);
        _Operator = operators[randomOperator];
        return (Operator)randomOperator;
    }

    private void EnsureResultInRange(int min, int max)
    {
        //조건에 맞는 문제 선택
        do
        {
            result = AnswerCalculator(selectedOperator);
        } 
        while (result < min || result > max);          
    }



    private void EnsureResultInRangeCoroutine(int min1, int max1, int max2)
    {
        int randomValue = Random.Range(0, 5);

        int min = randomValue != 1 ? min1 : max1;
        int max = randomValue != 1 ? max1 : max2;
        //'-' 연산자라면 무한루프 방지를 위해
        if (_Operator.Equals('-')||_Operator.Equals('÷'))
        {
            min = 0;
            max = 100;
        }
        
        //조건에 맞는 문제 선택
        do
        {
            result = AnswerCalculator(selectedOperator);
        } 
        while (result < min || result >= max);
    }    

    
}
