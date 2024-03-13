using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;


public class TilteManager : MonoBehaviour
{
    
    [SerializeField] private GameObject RegistrationCanvas;    
    [SerializeField] private TextMeshProUGUI inputError_text;
    [SerializeField] private TMP_InputField name_text;
    private string licenseFolderPath = string.Empty;
    private bool firstCheck = false;
    private void Awake()
    {
        //licenseFolderPath = Application.dataPath + "/License";
        licenseFolderPath = Application.persistentDataPath + "/License";
        string licenseFilePath = licenseFolderPath + "/clientlicense.json";
        if (!File.Exists(licenseFilePath))
        {
            firstCheck = true;
        }
        else
        {
            firstCheck = false;
        }        
    }
    private void Start()
    {
        //타이틀에서 메인메뉴로 Scene 이동 시 데이터 로드해야함 -> DB 비동기
        PlayerLoadData_Btn();
    }
    public void PlayerLoadData_Btn()
    {
        DataBase.Instance.PlayerDataLoad();
    }
    public void Registration_UI()
    {
        if (firstCheck)
        {
            RegistrationCanvas.SetActive(!RegistrationCanvas.activeSelf);
            Debug.Log("파일이 없어서 등록창을 띄줄게요");
        }
        else
        {
            SceneManager.LoadScene("HJB_MainMenu");
        }
    }

    public void Registration_Btn()
    {        
        if (!name_text.text.Equals(string.Empty))
        {
            SceneManager.LoadScene("HJB_MainMenu");
        }
        else
        {
            inputError_text.text = "이름을 입력해주세요.";
        }
    }
}
