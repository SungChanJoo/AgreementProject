using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
	public static SettingManager Instance = null;

    [SerializeField] private GameObject setting_Btn;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    //환경설정 UI Off
    public void Setting_Btn()
    {
        setting_Btn.SetActive(!setting_Btn.activeSelf);
    }
    public void MetaWorldSceneLoad_Btn()
    {
        SceneManager.LoadScene("JSC_Test_MetaWorld");
    }
    
    //Application 종료 버튼
    public void ApplicationExit_Btn()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
