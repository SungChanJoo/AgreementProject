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

    public AnalyticsProfileData analyticsProfile;
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
        level -= 1;
        try
        {
            PlayerProfileDataCheck(level);
        }
        catch (System.Exception)
        {

            Debug.Log("프로필에 할당된 데이터가 없거나 Null입니다.");
        }
    }

    private void PlayerProfileDataCheck(int level)
    {        
        string a =  DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].analyticsProfileData.Data[level].Item1;
        float b = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].analyticsProfileData.Data[level].Item2;
        int c = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].analyticsProfileData.Data[level].Item3;
        if (a=="0")
        {
            moreGame.text = "플레이한 내역이 없습니다.";
            reactionRate.text = "";
            answersRate.text = "";
        }
        else
        {
            moreGame.text = a;
            reactionRate.text = $"{b.ToString("F2")}초";
            answersRate.text = $"{c}%";
        }
        
    }
}
