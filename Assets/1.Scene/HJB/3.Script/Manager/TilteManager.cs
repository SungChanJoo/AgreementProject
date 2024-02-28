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
    public void PlayerLoadData_Btn()
    {
        DataBase.Instance.PlayerDataLoad();
    }
    public void Registration_UI()
    {
        if (firstCheck)
        {
            RegistrationCanvas.SetActive(!RegistrationCanvas.activeSelf);
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
            inputError_text.text = "Please enter your name";
        }
    }
}
