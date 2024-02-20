using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Level
{
    Low_Level =1,
    Middle_Level=2,
    Hight_Level = 3,
}
public enum Step
{
    _1 = 1,
    _2,
    _3,
    _4,
    _5,
    _6 = 6,}

public enum TimeSet
{
    _1m = 60,
    _3m = 180,
    _5m = 300,
}

public class StepManager : MonoBehaviour
{   

    public static StepManager Instance = null;

    public Step _Setp;
    public Level _Level;
    public int CurrentStep { get; private set; }
    public int CurrentLevel { get; private set; }
    public int CurrentTime { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        //기본 셋팅
        CurrentStep = (int)Step._1;
        CurrentLevel = (int)Level.Low_Level;
        CurrentTime = (int)TimeSet._1m;
    }

    //Level 변경
    public void SelectLevel(int level)
    {
        CurrentLevel = level;
    }

    //Step 변경
    public void SelectStep(int Step)
    {
        CurrentStep = Step;
    }

    //TimeSet 변경
    public void SelectTimeSet(int time)
    {
        CurrentTime = time;
    }



}

