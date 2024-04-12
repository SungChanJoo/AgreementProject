using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartButtonEvent : MonoBehaviour
{
    [Header("���κм�ǥ ��ư")]
    [SerializeField] private GameObject[] chartlevel_btn;
    [SerializeField] private GameObject[] chartgame_btn;
    [SerializeField] private Image[] venezia_btn;

    [SerializeField] private Sprite select_img;
    [SerializeField] private Sprite non_img;

    [SerializeField] private GameObject venezia_panel;

    private Image[] chartlevel_img;
    private Image[] chartGame_img;          
   
    private void Awake()
    {
        ChartButtonReference();
        
    }
    private void Start()
    {
        //�⺻ �� ����
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
        //level ��ư Ŭ�� sprite ����
        for (int i = 0; i < chartlevel_btn.Length; i++)
        {
            if (i == (level - 1))
            {
                chartlevel_img[i].sprite = select_img;
            }
            else
            {
                chartlevel_img[i].sprite = non_img;
            }
        }              
    }
    public void ChangeButtonChart_Gmae(int game)
    {
        //Game ��ư Ŭ�� sprite ����
        if (game ==3)
        {
            //���뺰 ���� ��ư Ȱ��ȭ
            venezia_panel.SetActive(true);
            ChangeButtonChart_Level(1);
            for (int i = 0; i < chartlevel_btn.Length; i++)
            {
                //���뺰 ������ ������ ���� ������ ��Ȱ��ȭ
                chartlevel_img[i].gameObject.SetActive(false);
            }
        }
        else
        {
            //���뺰 ���� ��ư ��Ȱ��ȭ
            venezia_panel.SetActive(false);
            for (int i = 0; i < chartlevel_btn.Length; i++)
            {
                chartlevel_img[i].gameObject.SetActive(true);
            }
        }
        for (int i = 0; i < chartgame_btn.Length; i++)
        {
            if (i == (game- 1))
            {
                chartGame_img[i].sprite = select_img;
            }
            else
            {
                chartGame_img[i].sprite = non_img;
            }
        }               
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
                venezia_btn[i].sprite = non_img;
            }
        }        
    }
    


}
