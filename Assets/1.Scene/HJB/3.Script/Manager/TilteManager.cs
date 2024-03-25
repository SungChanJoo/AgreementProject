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
        if(Application.platform == RuntimePlatform.Android)
            licenseFolderPath = Application.persistentDataPath + "/License";
        else
            licenseFolderPath = Application.dataPath + "/License";
        
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
        //Ÿ��Ʋ���� ���θ޴��� Scene �̵� �� ������ �ε��ؾ��� -> DB �񵿱�
        PlayerLoadData_Btn();

    }
    public void PlayerLoadData_Btn()
    {
        DataBase.Instance.LoadUserList();
        DataBase.Instance.PlayerDataLoad();
    }
    public void Registration_UI()
    {
        if (firstCheck)
        {
            RegistrationCanvas.SetActive(!RegistrationCanvas.activeSelf);
            Debug.Log("������ ��� ���â�� ���ٰԿ�");
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
            Client.instance.RegisterCharactorName_SaveDataToDB(name_text.text);
            SceneManager.LoadScene("HJB_MainMenu");
        }
        else
        {
            inputError_text.text = "�̸��� �Է����ּ���.";
        }
    }
}
