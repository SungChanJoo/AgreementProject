using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEvent : MonoBehaviour
{
    [SerializeField] private GameObject[] level_btn;
    [SerializeField] private GameObject[] time_btn;


    private int level;
    private int timeSet;   
    
    private Image[] level_img;    
    private Image[] time_img;

    [SerializeField] private Sprite select_img;
    [SerializeField] private Sprite Non_img;

    private void Awake()
    {
        level_img = new Image[level_btn.Length];
        time_img = new Image[time_btn.Length];

        ButtonReference();
        ChangeColor_Level();
        ChangeColor_Time();
    }
    private void Start()
    {        
        
    }
    private void ButtonReference()
    {
        for (int i = 0; i < level_btn.Length; i++)
        {
            level_img[i] = level_btn[i].GetComponent<Image>();
            time_img[i] = time_btn[i].GetComponent<Image>();                        
        }
    }
    
    private void ButtonDataSet()
    {
        level = StepManager.Instance.CurrentLevel;
        timeSet = StepManager.Instance.CurrentTime;
        timeSet = timeSet / 60;       
    }
    public void ChangeColor_Level()
    {
        ButtonDataSet();

        for (int i = 0; i < level_btn.Length; i++)
        {
            if (i == (level - 1))
            {
                level_img[i].sprite= select_img;
            }
            else
            {                
                level_img[i].sprite = Non_img;
            }
        }        
    }
    public void ChangeColor_Time()
    {
        ButtonDataSet();
        int count = 1;
        for (int i = 0; i < time_btn.Length; i++)
        {        
            
            if (i+count == timeSet)
            {
                time_img[i].sprite= select_img;
            }
            else
            {
                time_img[i].sprite = Non_img;
            }
            count++;
        }        
    }
}
