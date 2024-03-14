using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerChange_panel;
    [SerializeField] private GameObject PlayerRegistration_panel;
    [SerializeField] private GameObject PlayerNameChange_panel;
    [SerializeField] private GameObject PlayerBirthDay_panel;
    [SerializeField] private GameObject RecordDelete_panel;
    [SerializeField] private GameObject PlayerImage_panel;

    
    public void PlayerChange_UI()
    {
        PlayerChange_panel.SetActive(!PlayerChange_panel.activeSelf);        
    }

    public void PlayerRegistration_UI()
    {
        PlayerRegistration_panel.SetActive(!PlayerRegistration_panel.activeSelf);
    }
    public void PlayerNameChange_UI()
    {
        PlayerNameChange_panel.SetActive(!PlayerNameChange_panel.activeSelf);
    }
    public void PlayerBirthDay_UI()
    {
        PlayerBirthDay_panel.SetActive(!PlayerBirthDay_panel.activeSelf);
    }
    public void RecordDelete_Panel()
    {
        RecordDelete_panel.SetActive(!RecordDelete_panel.activeSelf);
    }
    public void PlayerImage_UI()
    {
        PlayerImage_panel.SetActive(!PlayerImage_panel.activeSelf);
    }
    public void PlayerChange_btn(int num)
    {
        DataBase.Instance.CharacterIndex = num;  
    }
}
