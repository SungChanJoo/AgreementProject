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
    private void OnEnable()
    {
        
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
            Debug.Log(answersRate);
        }
    }
    private void ChangePlayerGameData(int level)
    {
        level -= 1;
        try
        {
            //moreGame.text = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].analyticsProfileData.Data[level].Item1;
            //reactionRate.text =$"{ DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].analyticsProfileData.Data[level].Item2}";
            //answersRate.text = $"{ DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].analyticsProfileData.Data[level].Item3}";
        }
        catch (System.Exception)
        {

            Debug.Log("프로필에 할당된 데이터가 없거나 Null입니다.");
        }
    }
}
