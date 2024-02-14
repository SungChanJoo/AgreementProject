using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class GameSetting : MonoBehaviour
{
    public int step;
    public int level;
    public int timeSet;
    
    private void Start()
    {
        step = StepManager.Instance.CurrentStep;
        level = StepManager.Instance.CurrentLevel;
        timeSet = StepManager.Instance.CurrentTime;
        SplitLevelAndStep();
    }    
    
    //현재 Level Step에 따라 나누기
    public virtual void SplitLevelAndStep()
    {
        switch (level)
        {
            case 1:
                Level_1(step);
                break;
            case 2:
                Level_2(step);
                break;
            case 3:
                Level_3(step);
                break;            
        }
    }  
    public abstract void Level_1(int step);
    public abstract void Level_2(int step);
    public abstract void Level_3(int step);

}
