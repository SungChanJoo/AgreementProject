using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileButtonEvent : MonoBehaviour
{
    [SerializeField]private Image[] profile_btn;

    [SerializeField] private Sprite select_img;
    [SerializeField] private Sprite Non_img;

    [SerializeField] private TextMeshProUGUI moreGame;
    [SerializeField] private TextMeshProUGUI reactionRate;
    [SerializeField] private TextMeshProUGUI answersRate;

    private void Start()
    {
        ChangeImage_Btn(1);
    }
    public void ChangeImage_Btn(int level)
    {
        for (int i = 0; i < profile_btn.Length; i++)
        {
            if (level-1 == i)
            {
                profile_btn[i].sprite = select_img;
                ChangePlayerGameData(level);
            }
            else
            {
                profile_btn[i].sprite = Non_img;
            }
        }
    }
    private void ChangePlayerGameData(int level)
    {
        moreGame.text = $"Lv_{level} 데이터 받기";
        reactionRate.text =$"Lv_{level}데이터 받기";
        answersRate.text = $"Lv_{level}데이터 받기";
    }
}
