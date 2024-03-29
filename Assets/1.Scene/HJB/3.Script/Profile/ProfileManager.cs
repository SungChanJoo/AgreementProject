using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerChange_panel;
    [SerializeField] private GameObject PlayerRegistration_panel;
    [SerializeField] private GameObject PlayerNameChange_panel;
    [SerializeField] private GameObject PlayerBirthDay_panel;
    [SerializeField] private GameObject RecordDelete_panel;
    [SerializeField] private GameObject PlayerImage_panel;
    [SerializeField] private LoadImage profileLoad ;

    [SerializeField] private TMP_InputField characterName;
    private void OnEnable()
    {
        profileLoad.ProfileImage_Set();
    }
    public void PlayerChange_UI()
    {
        PlayerChange_panel.SetActive(!PlayerChange_panel.activeSelf);
        if(PlayerChange_panel.activeSelf)
        {
            DataBase.Instance.LoadUserList();
            GetComponent<ProfileText>().LoadCharacterProfile();
        }
    }

    public void PlayerRegistration_UI()
    {
        PlayerRegistration_panel.SetActive(!PlayerRegistration_panel.activeSelf);
        if (PlayerChange_panel.activeSelf)
            GetComponent<ProfileText>().Registration_InputField();
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
        DataBase.Instance.ChangeCharactor(num);  
    }
    public void AddCharacter()
    {
        //이름 등록
        var characters = GetComponent<ProfileText>().Characters.
            transform.GetChild(DataBase.Instance.UserList.createdCharactorCount).gameObject;
        characters.SetActive(true);
        Debug.Log(characters.name);
        characters.transform.GetChild(1).transform.Find("Name_Text").gameObject.GetComponent<TMP_Text>().text = characterName.text;
        DataBase.Instance.CharactorAdd(characterName.text);
        DataBase.Instance.LoadUserList();
    }

}
