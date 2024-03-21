using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonEvent : MonoBehaviour
{
    [SerializeField] private GameObject[] level_btn;
    [SerializeField] private GameObject[] time_btn;


    [SerializeField] private Sprite select_img;
    [SerializeField] private Sprite Non_img;

    [SerializeField]private TextMeshProUGUI[] btn_text;

    private int level;
    private int timeSet;   
    
    private Image[] level_img;
    private Image[] time_img;   
    private void Awake()
    {
        level_img = new Image[level_btn.Length];
        time_img = new Image[time_btn.Length];
        
        ButtonReference();
        ChangeColor_Level();
        ChangeColor_Time();
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
        string[] venezia_text = { "한글", "영어", "한자" };
        string[] level_text = { "Lv_1", "Lv_2", "Lv_3" };
        if (StepManager.Instance.game_Type >= Game_Type.C)
        {
            GameTypeSelectBtn_Text(venezia_text);
        }
        else
        {
            GameTypeSelectBtn_Text(level_text);
        }
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

    private void GameTypeSelectBtn_Text(string[] text)
    {
        
        for (int i = 0; i < level_btn.Length; i++)
        {            
            btn_text[i].text = text[i];
        }
    }


    
}
