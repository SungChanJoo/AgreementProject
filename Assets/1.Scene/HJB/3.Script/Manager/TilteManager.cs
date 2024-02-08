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

    /*private void Awake()
    {
        AppSetPermission();
    }

    private void AppSetPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (WebCamTexture.devices.Length == 0)
        {
            return;
        }
#endif
    }
    {
        yield return StartCoroutine(AppSetPermission());
}*/
    
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
