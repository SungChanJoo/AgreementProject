using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class TilteManager : MonoBehaviour
{
    
    [SerializeField] private GameObject RegistrationCanvas;    
    [SerializeField] private TextMeshProUGUI inputError_text;
    [SerializeField] private TMP_InputField name_text;

    int df=6502;
    private void Start()
    {
        int a;
        a = (int)(df/ 100f) * 100;
        Debug.Log(a);
    }
    public void Registration_UI()
    {
        RegistrationCanvas.SetActive(!RegistrationCanvas.activeSelf);
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