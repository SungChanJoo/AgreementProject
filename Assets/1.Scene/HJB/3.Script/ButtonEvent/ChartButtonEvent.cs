using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartButtonEvent : MonoBehaviour
{
    [Header("개인분석표 버튼")]
    [SerializeField] private GameObject[] chartlevel_btn;
    [SerializeField] private GameObject[] chartgame_btn;
    [SerializeField] private Image[] venezia_btn;

    [SerializeField] private Sprite select_img;
    [SerializeField] private Sprite Non_img;

    [SerializeField] private GameObject venezia_panel;

    private Image[] chartlevel_img;
    private Image[] chartGame_img;

   

    private byte current_day;
    private byte current_gameType;
    private byte current_level;
    private void Awake()
    {
        ChartButtonReference();
        
    }
    private void Start()
    {
        //기본 값 세팅
        ChangeButtonChart_Gmae(1);
        ChangeButtonChart_Level(1);
    }
    private void ChartButtonReference()
    {
        chartlevel_img = new Image[chartlevel_btn.Length];
        chartGame_img = new Image[chartgame_btn.Length];
        for (int i = 0; i < chartlevel_btn.Length; i++)
        {
            chartGame_img[i] = chartgame_btn[i].GetComponent<Image>();
            chartlevel_img[i] = chartlevel_btn[i].GetComponent<Image>();
        }
    }
    public void ChangeButtonChart_Level(int level)
    {
        for (int i = 0; i < chartlevel_btn.Length; i++)
        {
            if (i == (level - 1))
            {
                chartlevel_img[i].sprite = select_img;
            }
            else
            {
                chartlevel_img[i].sprite = Non_img;
            }
        }
        current_level = (byte)level;
    }
    public void ChangeButtonChart_Gmae(int game)
    {
        if (game ==3)
        {
            venezia_panel.SetActive(true);
        }
        else
        {
            venezia_panel.SetActive(false);
        }
        for (int i = 0; i < chartgame_btn.Length; i++)
        {
            if (i == (game- 1))
            {
                chartGame_img[i].sprite = select_img;
            }
            else
            {
                chartGame_img[i].sprite = Non_img;
            }
        }
        current_gameType = (byte)game;
    }
    public void Venezia_Btn(int game)
    {
        for (int i = 0; i < venezia_btn.Length; i++)
        {
            if (i == (game - 2))
            {
                venezia_btn[i].sprite = select_img;
            }
            else
            {
                venezia_btn[i].sprite = Non_img;
            }
        }
        current_gameType = (byte)game;
    }
    


}
